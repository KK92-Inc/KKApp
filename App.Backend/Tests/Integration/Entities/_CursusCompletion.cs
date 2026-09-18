// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Net;
using System.Net.Http.Json;
using App.Backend.Database;
using App.Backend.Domain.Enums;
using App.Backend.Models.Requests.Cursus;
using App.Backend.Models.Responses.Entities.Cursus;
using App.Backend.Tests.Integration;
using Microsoft.EntityFrameworkCore;
using Xunit;

// ============================================================================

namespace App.Backend.Tests.Integration.Entities;

/// <summary>
/// Covers cursus *completion*: whether/when a UserCursus flips to Completed, how
/// choice groups factor in, isolation between users, idempotency, and - most
/// importantly - how completion interacts with the PGM repair mechanism already
/// covered in PGM.Test.cs (does completion survive a later track edit; does
/// swapping an unfinished goal still let the user complete the cursus afterwards).
///
/// These drive goals to completion through the real GoalCompletionMessage
/// pipeline (see Extension.CompleteGoalAsync) rather than flipping UserCursus.State
/// by hand, so they actually exercise GoalCompletionHandler.CheckCursusProgressionAsync
/// and CursusCompletionHandler - the two pieces that had zero coverage before this file.
/// </summary>
public class CursusCompletionTests
{
    private static async Task PutTrackAsync(HttpClient client, Guid cursusId, List<Models.Requests.Cursus.CursusTrackNodeDO> nodes)
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

    /// <summary>Always reads through a fresh context - see PGM.Test.cs's GetSnapshotAsync for why.</summary>
    private static async Task<EntityObjectState> GetCursusStateAsync(WebAppTestFactory factory, Guid userCursusId)
    {
        await using var verify = factory.CreateDbContext();
        var uc = await verify.UserCursi.AsNoTracking().SingleAsync(x => x.Id == userCursusId);
        return uc.State;
    }

    private static Task AssertCursusCompletesAsync(WebAppTestFactory factory, Guid userCursusId) =>
        TestUtils.WaitForAsync(async () => await GetCursusStateAsync(factory, userCursusId) == EntityObjectState.Completed);

    private static async Task<int> CountCompletionNotificationsAsync(WebAppTestFactory factory, Guid userCursusId)
    {
        await using var verify = factory.CreateDbContext();
        return await verify.Notifications.AsNoTracking()
            .CountAsync(n => n.ResourceId == userCursusId && n.Descriptor.HasFlag(NotificationMeta.Completed));
    }

    // ==============================================================================
    // Baseline: does it complete at all, and only when it should?
    // ==============================================================================

    [Fact]
    public async Task LinearTrack_CompletingAllRequiredGoals_CompletesCursus()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var cursus = await client.CreateCursusAsync(workspace.Id);

        var a = await client.CreateGoalAsync(workspace.Id);
        var b = await client.CreateGoalAsync(workspace.Id);
        var c = await client.CreateGoalAsync(workspace.Id);

        await PutTrackAsync(client, cursus.Id, [
            new() { GoalId = a.Id, ParentId = null },
            new() { GoalId = b.Id, ParentId = a.Id },
            new() { GoalId = c.Id, ParentId = b.Id },
        ]);

        var userCursus = await SubscribeToCursusAsync(client, user.Id, cursus.Id);

        await factory.CompleteGoalAsync(db, user.Id, a.Id);
        Assert.Equal(EntityObjectState.Active, await GetCursusStateAsync(factory, userCursus.Id));

        await factory.CompleteGoalAsync(db, user.Id, b.Id);
        Assert.Equal(EntityObjectState.Active, await GetCursusStateAsync(factory, userCursus.Id));

        // Last required goal - this should cascade into an actual cursus completion.
        await factory.CompleteGoalAsync(db, user.Id, c.Id);
        await AssertCursusCompletesAsync(factory, userCursus.Id);
        Assert.Equal(1, await CountCompletionNotificationsAsync(factory, userCursus.Id));
    }

    [Fact]
    public async Task LinearTrack_PartialCompletion_NeverCompletesCursus()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var cursus = await client.CreateCursusAsync(workspace.Id);

        var a = await client.CreateGoalAsync(workspace.Id);
        var b = await client.CreateGoalAsync(workspace.Id);
        var c = await client.CreateGoalAsync(workspace.Id);

        await PutTrackAsync(client, cursus.Id, [
            new() { GoalId = a.Id, ParentId = null },
            new() { GoalId = b.Id, ParentId = a.Id },
            new() { GoalId = c.Id, ParentId = b.Id },
        ]);

        var userCursus = await SubscribeToCursusAsync(client, user.Id, cursus.Id);

        await factory.CompleteGoalAsync(db, user.Id, a.Id);
        await factory.CompleteGoalAsync(db, user.Id, b.Id);
        // Deliberately never complete C.

        // No event will ever flip this cursus to Completed - assert the state holds
        // steady rather than waiting for a condition that should never become true.
        await Task.Delay(TimeSpan.FromMilliseconds(250));
        Assert.Equal(EntityObjectState.Active, await GetCursusStateAsync(factory, userCursus.Id));
    }

    // ==============================================================================
    // Choice groups
    // ==============================================================================

    [Fact]
    public async Task ChoiceGroup_CompletingOneOption_SatisfiesTheGroup()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var cursus = await client.CreateCursusAsync(workspace.Id);

        var root = await client.CreateGoalAsync(workspace.Id);
        var optionX = await client.CreateGoalAsync(workspace.Id);
        var optionY = await client.CreateGoalAsync(workspace.Id);
        var group = Guid.NewGuid();

        await PutTrackAsync(client, cursus.Id, [
            new() { GoalId = root.Id, ParentId = null },
            new() { GoalId = optionX.Id, ParentId = root.Id, Group = group },
            new() { GoalId = optionY.Id, ParentId = root.Id, Group = group },
        ]);

        var userCursus = await SubscribeToCursusAsync(client, user.Id, cursus.Id);

        await factory.CompleteGoalAsync(db, user.Id, root.Id);
        await factory.CompleteGoalAsync(db, user.Id, optionX.Id);
        // optionY is deliberately left untouched.

        await AssertCursusCompletesAsync(factory, userCursus.Id);
    }

    [Fact]
    public async Task ChoiceGroup_CompletingNeitherOption_NeverCompletesCursus()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var cursus = await client.CreateCursusAsync(workspace.Id);

        var root = await client.CreateGoalAsync(workspace.Id);
        var optionX = await client.CreateGoalAsync(workspace.Id);
        var optionY = await client.CreateGoalAsync(workspace.Id);
        var group = Guid.NewGuid();

        await PutTrackAsync(client, cursus.Id, [
            new() { GoalId = root.Id, ParentId = null },
            new() { GoalId = optionX.Id, ParentId = root.Id, Group = group },
            new() { GoalId = optionY.Id, ParentId = root.Id, Group = group },
        ]);

        var userCursus = await SubscribeToCursusAsync(client, user.Id, cursus.Id);

        await factory.CompleteGoalAsync(db, user.Id, root.Id);
        // Neither X nor Y ever completed.

        await Task.Delay(TimeSpan.FromMilliseconds(250));
        Assert.Equal(EntityObjectState.Active, await GetCursusStateAsync(factory, userCursus.Id));
    }

    // ==============================================================================
    // Idempotency
    // ==============================================================================

    [Fact]
    public async Task ReprocessingAnAlreadyCompletedGoal_DoesNotDuplicateCompletionOrNotification()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var cursus = await client.CreateCursusAsync(workspace.Id);
        var a = await client.CreateGoalAsync(workspace.Id);

        await PutTrackAsync(client, cursus.Id, [new() { GoalId = a.Id, ParentId = null }]);
        var userCursus = await SubscribeToCursusAsync(client, user.Id, cursus.Id);

        await factory.CompleteGoalAsync(db, user.Id, a.Id);
        await AssertCursusCompletesAsync(factory, userCursus.Id);

        // Fire the exact same completion event again - GoalCompletionHandler's guard
        // (`if (userGoal.State is Completed) return;`) should make this a no-op.
        await factory.CompleteGoalAsync(db, user.Id, a.Id);
        await Task.Delay(TimeSpan.FromMilliseconds(250));

        Assert.Equal(EntityObjectState.Completed, await GetCursusStateAsync(factory, userCursus.Id));
        Assert.Equal(1, await CountCompletionNotificationsAsync(factory, userCursus.Id));
    }

    // ==============================================================================
    // Isolation between users
    // ==============================================================================

    [Fact]
    public async Task TwoUsersOnSameCursus_CompletionIsPerUser()
    {
        var (factory, db, user1, client1) = await TestUtils.SetupAsync();
        var (user2, client2) = await factory.SetupAdditionalUserAsync(db);

        var workspace = await client1.GetWorkspaceAsync(db, user1);
        var cursus = await client1.CreateCursusAsync(workspace.Id);
        var a = await client1.CreateGoalAsync(workspace.Id);

        await PutTrackAsync(client1, cursus.Id, [new() { GoalId = a.Id, ParentId = null }]);

        var userCursus1 = await SubscribeToCursusAsync(client1, user1.Id, cursus.Id);
        var userCursus2 = await SubscribeToCursusAsync(client2, user2.Id, cursus.Id);

        await factory.CompleteGoalAsync(db, user1.Id, a.Id);
        await AssertCursusCompletesAsync(factory, userCursus1.Id);

        // User 2 never touched the goal - their enrollment must stay untouched.
        await Task.Delay(TimeSpan.FromMilliseconds(250));
        Assert.Equal(EntityObjectState.Active, await GetCursusStateAsync(factory, userCursus2.Id));
    }

    // ==============================================================================
    // Completion x repair interplay - the scenarios from the original question.
    // ==============================================================================

    [Fact]
    public async Task CompletedCursus_SurvivesALaterTrackEdit_NoTreadmill()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var cursus = await client.CreateCursusAsync(workspace.Id);

        var a = await client.CreateGoalAsync(workspace.Id);
        var b = await client.CreateGoalAsync(workspace.Id);
        var c = await client.CreateGoalAsync(workspace.Id);

        await PutTrackAsync(client, cursus.Id, [
            new() { GoalId = a.Id, ParentId = null },
            new() { GoalId = b.Id, ParentId = a.Id },
            new() { GoalId = c.Id, ParentId = b.Id },
        ]);

        var userCursus = await SubscribeToCursusAsync(client, user.Id, cursus.Id);

        await factory.CompleteGoalAsync(db, user.Id, a.Id);
        await factory.CompleteGoalAsync(db, user.Id, b.Id);
        await factory.CompleteGoalAsync(db, user.Id, c.Id);
        await AssertCursusCompletesAsync(factory, userCursus.Id);

        // Staff now replaces Goal A entirely with a brand new goal.
        var replacementA = await client.CreateGoalAsync(workspace.Id);
        await PutTrackAsync(client, cursus.Id, [
            new() { GoalId = replacementA.Id, ParentId = null },
            new() { GoalId = b.Id, ParentId = replacementA.Id },
            new() { GoalId = c.Id, ParentId = b.Id },
        ]);

        // The user is not sent back to Active, and A/B/C stay recorded as done -
        // there is no re-triggering of completion logic here at all, so this is
        // really just confirming the state write from before wasn't reverted by
        // the track-edit propagation path (Propegate only touches UserCursusGoal
        // snapshot rows, never UserCursus.State).
        await using var verify = factory.CreateDbContext();
        Assert.Equal(EntityObjectState.Completed, await GetCursusStateAsync(factory, userCursus.Id));

        var snapshot = await verify.UserCursusGoal.AsNoTracking()
            .Where(n => n.UserCursusId == userCursus.Id)
            .ToListAsync();

        // Frozen (locked-in) goals A, B, C remain exactly as completed - the new
        // replacement root goal never even shows up, since HasNewContent for it
        // is false (its only child, B, is already frozen). See PGM.Test.cs for
        // dedicated coverage of this pruning rule.
        Assert.Contains(snapshot, n => n.GoalId == a.Id);
        Assert.Contains(snapshot, n => n.GoalId == b.Id);
        Assert.Contains(snapshot, n => n.GoalId == c.Id);
        Assert.DoesNotContain(snapshot, n => n.GoalId == replacementA.Id);
    }

    [Fact]
    public async Task TrackEditBeforeCompletion_SwapsUnlockedGoal_ThenCompletingTheReplacementCompletesCursus()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var cursus = await client.CreateCursusAsync(workspace.Id);

        var a = await client.CreateGoalAsync(workspace.Id);
        var b = await client.CreateGoalAsync(workspace.Id);
        var c = await client.CreateGoalAsync(workspace.Id);

        await PutTrackAsync(client, cursus.Id, [
            new() { GoalId = a.Id, ParentId = null },
            new() { GoalId = b.Id, ParentId = a.Id },
            new() { GoalId = c.Id, ParentId = b.Id },
        ]);

        var userCursus = await SubscribeToCursusAsync(client, user.Id, cursus.Id);

        // The user has locked in A and B, but never even started C.
        await factory.CompleteGoalAsync(db, user.Id, a.Id);
        await factory.CompleteGoalAsync(db, user.Id, b.Id);

        // Staff swaps C for a new goal F.
        var f = await client.CreateGoalAsync(workspace.Id);
        await PutTrackAsync(client, cursus.Id, [
            new() { GoalId = a.Id, ParentId = null },
            new() { GoalId = b.Id, ParentId = a.Id },
            new() { GoalId = f.Id, ParentId = b.Id },
        ]);

        await using (var verify = factory.CreateDbContext())
        {
            var snapshot = await verify.UserCursusGoal.AsNoTracking()
                .Where(n => n.UserCursusId == userCursus.Id)
                .ToListAsync();

            Assert.DoesNotContain(snapshot, n => n.GoalId == c.Id);
            Assert.Contains(snapshot, n => n.GoalId == f.Id);
        }

        // Completing the *replacement* goal should now satisfy the cursus - this
        // is the exact end-to-end path that matters: repair swaps the requirement,
        // and the completion check reads off the post-repair snapshot correctly.
        await factory.CompleteGoalAsync(db, user.Id, f.Id);
        await AssertCursusCompletesAsync(factory, userCursus.Id);
    }
}