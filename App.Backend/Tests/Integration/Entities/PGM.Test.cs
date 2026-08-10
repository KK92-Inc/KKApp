// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Net;
using System.Net.Http.Json;
using App.Backend.Database;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;
using App.Backend.Models.Requests.Cursus;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Responses.Entities.Cursus;
using App.Backend.Tests.Integration;
using Microsoft.EntityFrameworkCore;
using Xunit;

// ============================================================================

namespace App.Backend.Tests.Integration.Entities;

/// <summary>
/// Covers the PGM (PersistenceGraphMesher) snapshot meshing behavior: what
/// survives a track replace, what gets frozen, what gets released, and when a
/// stale snapshot catches back up. Every test builds the same tree used to
/// design the algorithm:
///
///   A
///   ├─ A.B
///   ├─ A.C
///   ├─ A.D
///   │  └─ D.A
///   └─ A.E
/// </summary>
public class PersistenceGraphMeshingTests
{
    private sealed record Tree(CursusDO Cursus, GoalDO A, GoalDO AB, GoalDO AC, GoalDO AD, GoalDO DA, GoalDO AE);

    private static async Task<Tree> BuildTreeAsync(HttpClient client, Guid workspaceId)
    {
        var cursus = await client.CreateCursusAsync(workspaceId);
        var a = await client.CreateGoalAsync(workspaceId);
        var ab = await client.CreateGoalAsync(workspaceId);
        var ac = await client.CreateGoalAsync(workspaceId);
        var ad = await client.CreateGoalAsync(workspaceId);
        var da = await client.CreateGoalAsync(workspaceId);
        var ae = await client.CreateGoalAsync(workspaceId);

        await PutTrackAsync(client, cursus.Id, [
            new() { GoalId = a.Id, ParentId = null },
            new() { GoalId = ab.Id, ParentId = a.Id },
            new() { GoalId = ac.Id, ParentId = a.Id },
            new() { GoalId = ad.Id, ParentId = a.Id },
            new() { GoalId = da.Id, ParentId = ad.Id },
            new() { GoalId = ae.Id, ParentId = a.Id },
        ]);

        return new Tree(cursus, a, ab, ac, ad, da, ae);
    }

    private static async Task PutTrackAsync(HttpClient client, Guid cursusId, List<CursusTrackNodeDTO> nodes)
    {
        var response = await client.PostAsJsonAsync($"/cursus/{cursusId}/track",
            new PostCursusTrackRequestDTO { Nodes = nodes }, JsonOptions.Default);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<UserCursusDO> SubscribeToCursusAsync(HttpClient client, Guid userId, Guid cursusId)
    {
        var response = await client.PostAsync($"/subscribe/{userId}/cursus/{cursusId}", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UserCursusDO>(JsonOptions.Default);
        Assert.NotNull(result);
        return result;
    }

    /// <summary>
    /// Directly seeds a locked-in UserGoal row, bypassing the subscribe/eligibility
    /// pipeline on purpose - PGM only cares about UserGoal.State, not how a user got
    /// there, and driving Completed through the real project/review grading flow
    /// would entangle this suite with an entirely different subsystem.
    /// </summary>
    private static async Task LockInAsync(DatabaseContext db, Guid userId, Guid goalId, EntityObjectState state)
    {
        db.UserGoals.Add(new UserGoal { UserId = userId, GoalId = goalId, State = state });
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Always reads through a fresh DbContext - the `db` handle used for seeding may
    /// hold stale tracked instances of rows the API has since mutated through its own,
    /// separate DbContext scope, which would silently mask a broken propagation.
    /// </summary>
    private static async Task<List<UserCursusGoal>> GetSnapshotAsync(WebAppTestFactory factory, Guid userCursusId)
    {
        await using var verify = factory.CreateDbContext();
        return await verify.UserCursusGoal
            .AsNoTracking()
            .Where(n => n.UserCursusId == userCursusId)
            .ToListAsync();
    }

    [Fact]
    public async Task Subscribe_MirrorsMasterTrackExactly()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var tree = await BuildTreeAsync(client, workspace.Id);

        var userCursus = await SubscribeToCursusAsync(client, user.Id, tree.Cursus.Id);
        var snapshot = await GetSnapshotAsync(factory, userCursus.Id);

        Assert.Equal(6, snapshot.Count);
        Assert.Contains(snapshot, n => n.GoalId == tree.A.Id && n.ParentGoalId == null);
        Assert.Contains(snapshot, n => n.GoalId == tree.AB.Id && n.ParentGoalId == tree.A.Id);
        Assert.Contains(snapshot, n => n.GoalId == tree.AD.Id && n.ParentGoalId == tree.A.Id);
        Assert.Contains(snapshot, n => n.GoalId == tree.DA.Id && n.ParentGoalId == tree.AD.Id);
        Assert.Contains(snapshot, n => n.GoalId == tree.AE.Id && n.ParentGoalId == tree.A.Id);
    }

    [Fact]
    public async Task Renaming_CompletedGoal_PreservesHistory_AndNewGoalJoinsAlongsideIt()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var tree = await BuildTreeAsync(client, workspace.Id);
        var userCursus = await SubscribeToCursusAsync(client, user.Id, tree.Cursus.Id);

        await LockInAsync(db, user.Id, tree.AB.Id, EntityObjectState.Completed);

        // Staff renames A.B -> A.F: a brand new goal at the same position.
        var af = await client.CreateGoalAsync(workspace.Id);
        await PutTrackAsync(client, tree.Cursus.Id, [
            new() { GoalId = tree.A.Id, ParentId = null },
            new() { GoalId = af.Id, ParentId = tree.A.Id },
            new() { GoalId = tree.AC.Id, ParentId = tree.A.Id },
            new() { GoalId = tree.AD.Id, ParentId = tree.A.Id },
            new() { GoalId = tree.DA.Id, ParentId = tree.AD.Id },
            new() { GoalId = tree.AE.Id, ParentId = tree.A.Id },
        ]);

        var snapshot = await GetSnapshotAsync(factory, userCursus.Id);

        // Product decision: history is preserved, but never exempts the user from
        // whatever the master track currently requires. The completed A.B stays
        // recorded exactly as it happened, and the new A.F simply joins alongside
        // it as an additional requirement - nothing suppresses it because a sibling
        // happens to be frozen. There is no data linking A.F to A.B as "the same
        // edit"; a track replace is just a flat list, so this is indistinguishable
        // from staff deleting A.B and separately adding a new required A.F.
        Assert.Contains(snapshot, n => n.GoalId == tree.AB.Id && n.ParentGoalId == tree.A.Id);
        Assert.Contains(snapshot, n => n.GoalId == af.Id && n.ParentGoalId == tree.A.Id);
        Assert.Equal(7, snapshot.Count); // A, A.B, A.C, A.D, D.A, A.E, A.F
    }

    [Fact]
    public async Task Renaming_ActiveGoal_NewSiblingJoinsImmediately_OldOneClearsOnlyOnUnsubscribe()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var tree = await BuildTreeAsync(client, workspace.Id);
        var userCursus = await SubscribeToCursusAsync(client, user.Id, tree.Cursus.Id);

        await LockInAsync(db, user.Id, tree.AE.Id, EntityObjectState.Active);

        // Staff renames A.E -> A.G while the user is still actively working on it.
        var ag = await client.CreateGoalAsync(workspace.Id);
        await PutTrackAsync(client, tree.Cursus.Id, [
            new() { GoalId = tree.A.Id, ParentId = null },
            new() { GoalId = tree.AB.Id, ParentId = tree.A.Id },
            new() { GoalId = tree.AC.Id, ParentId = tree.A.Id },
            new() { GoalId = tree.AD.Id, ParentId = tree.A.Id },
            new() { GoalId = tree.DA.Id, ParentId = tree.AD.Id },
            new() { GoalId = ag.Id, ParentId = tree.A.Id },
        ]);

        // Option A: A.G joins right away - it doesn't wait for A.E to be released.
        // Nothing about a locked-in sibling stops a genuinely new node from being
        // added under the same parent; freezing only protects a locked-in goal's
        // own row, it isn't a block on that goal's siblings.
        var beforeUnsub = await GetSnapshotAsync(factory, userCursus.Id);
        Assert.Contains(beforeUnsub, n => n.GoalId == tree.AE.Id);
        Assert.Contains(beforeUnsub, n => n.GoalId == ag.Id && n.ParentGoalId == tree.A.Id);
        Assert.Equal(7, beforeUnsub.Count); // A, A.B, A.C, A.D, D.A, A.E, A.G

        // The user unsubscribes from A.E - this doesn't add A.G (already there);
        // it drops the now-orphaned old A.E entry, since nothing keeps a record
        // around once the user is no longer actively committed to it and it no
        // longer exists anywhere in the master track.
        var unsubResponse = await client.DeleteAsync($"/subscribe/{user.Id}/goals/{tree.AE.Id}");
        Assert.Equal(HttpStatusCode.OK, unsubResponse.StatusCode);

        var afterUnsub = await GetSnapshotAsync(factory, userCursus.Id);
        Assert.DoesNotContain(afterUnsub, n => n.GoalId == tree.AE.Id);
        Assert.Contains(afterUnsub, n => n.GoalId == ag.Id && n.ParentGoalId == tree.A.Id);
        Assert.Equal(6, afterUnsub.Count); // A, A.B, A.C, A.D, D.A, A.G
    }

    [Fact]
    public async Task Renaming_UntouchedGoal_UpdatesSnapshotIncludingChildren()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var tree = await BuildTreeAsync(client, workspace.Id);
        var userCursus = await SubscribeToCursusAsync(client, user.Id, tree.Cursus.Id);

        // Staff renames A.D -> A.H. Nobody touched A.D, so the whole subtree -
        // including D.A - should follow the rename.
        var ah = await client.CreateGoalAsync(workspace.Id);
        var ha = await client.CreateGoalAsync(workspace.Id);
        await PutTrackAsync(client, tree.Cursus.Id, [
            new() { GoalId = tree.A.Id, ParentId = null },
            new() { GoalId = tree.AB.Id, ParentId = tree.A.Id },
            new() { GoalId = tree.AC.Id, ParentId = tree.A.Id },
            new() { GoalId = ah.Id, ParentId = tree.A.Id },
            new() { GoalId = ha.Id, ParentId = ah.Id },
            new() { GoalId = tree.AE.Id, ParentId = tree.A.Id },
        ]);

        var snapshot = await GetSnapshotAsync(factory, userCursus.Id);

        Assert.DoesNotContain(snapshot, n => n.GoalId == tree.AD.Id);
        Assert.DoesNotContain(snapshot, n => n.GoalId == tree.DA.Id);
        Assert.Contains(snapshot, n => n.GoalId == ah.Id && n.ParentGoalId == tree.A.Id);
        Assert.Contains(snapshot, n => n.GoalId == ha.Id && n.ParentGoalId == ah.Id);
        Assert.Equal(6, snapshot.Count);
    }

    [Fact]
    public async Task Renaming_UntouchedGoal_UnderACompletedRoot_StillUpdates()
    {
        // The root goal itself is completed, but that alone must not freeze an
        // untouched sibling branch underneath it (A.D / D.A) just because they
        // share an ancestor - completing A doesn't mean the user ever committed
        // to A.D specifically.
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var tree = await BuildTreeAsync(client, workspace.Id);
        var userCursus = await SubscribeToCursusAsync(client, user.Id, tree.Cursus.Id);

        await LockInAsync(db, user.Id, tree.A.Id, EntityObjectState.Completed);
        await LockInAsync(db, user.Id, tree.AB.Id, EntityObjectState.Completed);
        await LockInAsync(db, user.Id, tree.AC.Id, EntityObjectState.Completed);
        await LockInAsync(db, user.Id, tree.AE.Id, EntityObjectState.Active);
        // A.D and D.A: deliberately no UserGoal row at all - never started.

        var ah = await client.CreateGoalAsync(workspace.Id);
        await PutTrackAsync(client, tree.Cursus.Id, [
            new() { GoalId = tree.A.Id, ParentId = null },
            new() { GoalId = tree.AB.Id, ParentId = tree.A.Id },
            new() { GoalId = tree.AC.Id, ParentId = tree.A.Id },
            new() { GoalId = ah.Id, ParentId = tree.A.Id },
            new() { GoalId = tree.AE.Id, ParentId = tree.A.Id },
        ]);

        var snapshot = await GetSnapshotAsync(factory, userCursus.Id);

        // Completed/active branches: untouched by the rename.
        Assert.Contains(snapshot, n => n.GoalId == tree.A.Id);
        Assert.Contains(snapshot, n => n.GoalId == tree.AB.Id);
        Assert.Contains(snapshot, n => n.GoalId == tree.AC.Id);
        Assert.Contains(snapshot, n => n.GoalId == tree.AE.Id);

        // The untouched branch follows the master track, even though its
        // grandparent is completed.
        Assert.DoesNotContain(snapshot, n => n.GoalId == tree.AD.Id);
        Assert.DoesNotContain(snapshot, n => n.GoalId == tree.DA.Id);
        Assert.Contains(snapshot, n => n.GoalId == ah.Id && n.ParentGoalId == tree.A.Id);
        Assert.Equal(5, snapshot.Count); // A, A.B, A.C, A.E, A.H
    }

    [Fact]
    public async Task CompletedBranch_ThenRenamed_KeepsHistoryAndAddsReplacementSideBySide()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var tree = await BuildTreeAsync(client, workspace.Id);
        var userCursus = await SubscribeToCursusAsync(client, user.Id, tree.Cursus.Id);

        // This time the user actually completed A.D and its child D.A.
        await LockInAsync(db, user.Id, tree.AD.Id, EntityObjectState.Completed);
        await LockInAsync(db, user.Id, tree.DA.Id, EntityObjectState.Completed);

        var ah = await client.CreateGoalAsync(workspace.Id);
        await PutTrackAsync(client, tree.Cursus.Id, [
            new() { GoalId = tree.A.Id, ParentId = null },
            new() { GoalId = tree.AB.Id, ParentId = tree.A.Id },
            new() { GoalId = tree.AC.Id, ParentId = tree.A.Id },
            new() { GoalId = ah.Id, ParentId = tree.A.Id },
            new() { GoalId = tree.AE.Id, ParentId = tree.A.Id },
        ]);

        var snapshot = await GetSnapshotAsync(factory, userCursus.Id);

        // The completed branch survives exactly as recorded...
        Assert.Contains(snapshot, n => n.GoalId == tree.AD.Id && n.ParentGoalId == tree.A.Id);
        Assert.Contains(snapshot, n => n.GoalId == tree.DA.Id && n.ParentGoalId == tree.AD.Id);
        // ...and the new goal shows up alongside it as a sibling, not a replacement.
        Assert.Contains(snapshot, n => n.GoalId == ah.Id && n.ParentGoalId == tree.A.Id);
        Assert.Equal(7, snapshot.Count); // A, A.B, A.C, A.D, D.A, A.E, A.H
    }

    [Fact]
    public async Task Reactivation_CatchesUpSnapshotToTrackChangesMissedWhileInactive()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var tree = await BuildTreeAsync(client, workspace.Id);
        var userCursus = await SubscribeToCursusAsync(client, user.Id, tree.Cursus.Id);

        // Simulate having gone inactive a while ago (no cooldown, so reactivating
        // below doesn't need to fight the real unsubscribe cooldown clock).
        var userCursusRow = await db.UserCursi.SingleAsync(uc => uc.Id == userCursus.Id);
        userCursusRow.State = EntityObjectState.Inactive;
        userCursusRow.UnlocksAt = null;
        await db.SaveChangesAsync();

        // Track changes while inactive - the batch propagation intentionally skips
        // inactive subscriptions, so this snapshot should now be stale relative to
        // the master track.
        var ah = await client.CreateGoalAsync(workspace.Id);
        await PutTrackAsync(client, tree.Cursus.Id, [
            new() { GoalId = tree.A.Id, ParentId = null },
            new() { GoalId = tree.AB.Id, ParentId = tree.A.Id },
            new() { GoalId = tree.AC.Id, ParentId = tree.A.Id },
            new() { GoalId = ah.Id, ParentId = tree.A.Id },
            new() { GoalId = tree.AE.Id, ParentId = tree.A.Id },
        ]);

        var staleSnapshot = await GetSnapshotAsync(factory, userCursus.Id);
        // Confirms it was actually skipped rather than magically staying in sync.
        Assert.Contains(staleSnapshot, n => n.GoalId == tree.AD.Id);
        Assert.DoesNotContain(staleSnapshot, n => n.GoalId == ah.Id);

        // Resubscribing should catch it up to the master track in one shot.
        var resubResponse = await client.PostAsync($"/subscribe/{user.Id}/cursus/{tree.Cursus.Id}", null);
        Assert.Equal(HttpStatusCode.OK, resubResponse.StatusCode);

        var caughtUpSnapshot = await GetSnapshotAsync(factory, userCursus.Id);
        Assert.DoesNotContain(caughtUpSnapshot, n => n.GoalId == tree.AD.Id);
        Assert.DoesNotContain(caughtUpSnapshot, n => n.GoalId == tree.DA.Id);
        Assert.Contains(caughtUpSnapshot, n => n.GoalId == ah.Id && n.ParentGoalId == tree.A.Id);
        Assert.Equal(5, caughtUpSnapshot.Count); // A, A.B, A.C, A.E, A.H
    }
}