// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Relations;

// ============================================================================

namespace App.Backend.Core.Services.Persistence.Interface;

public readonly record struct Node(Guid GoalId, Guid? ParentGoalId, Guid? ChoiceGroup);

public readonly record struct Difference(
    IReadOnlyList<Guid> ToRemove,
    IReadOnlyList<Node> ToAdd,
    IReadOnlyList<Node> ToRepoint)
{
    public bool IsEmpty => ToRemove.Count == 0 && ToAdd.Count == 0 && ToRepoint.Count == 0;
}

// ============================================================================

/// <summary>
/// Meshes a master cursus track into a single user's frozen snapshot.
///
/// Rule: once any goal in a user's snapshot is locked-in (Active/Completed), that
/// goal's entire subtree, started or not, is frozen exactly as recorded. The
/// master track can never reach into a branch the user has already committed to.
/// A master node whose entire purpose is feeding an already-frozen descendant
/// (i.e. it has no unclaimed content anywhere below it) is pruned rather than
/// inserted, so a "replacement root" that only exists to host an already-frozen
/// child never shows up as a dangling extra root.
///
/// Pure and side-effect free by design: takes plain snapshots of state, returns
/// an instruction set, touches no <c>DbContext</c>. This is what lets the same
/// algorithm serve two very different callers - <see cref="CursusService"/>,
/// meshing one master-track edit across an entire cohort in a batch, and
/// <see cref="CursusSnapshotTracker"/>, meshing the current master track into
/// one user the moment their locked-in set shrinks (e.g. on unsubscribe).
/// </summary>
public interface IPersistenceGraphMesher
{
    /// <summary>
    /// Computes the mesh diff for one user.
    /// </summary>
    /// <param name="oldSnapshot">The user's current frozen track, empty if they have none yet.</param>
    /// <param name="masterTrack">The live master track to mesh in.</param>
    /// <param name="lockedInGoalIds">
    /// Goal IDs this user currently has Active or Completed - i.e. genuinely committed to,
    /// not merely present in an old snapshot.
    /// </param>
    public Difference Diff(IReadOnlyList<UserCursusGoal> oldSnapshot, IReadOnlyList<CursusGoal> masterTrack, IReadOnlySet<Guid> lockedInGoalIds);
}
