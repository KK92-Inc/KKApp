// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;
using App.Backend.Models.Responses.Entities.Goals;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursus;

/// <summary>
/// A single node in the user's cursus track tree.
/// Extends the base track node with the user's computed state for this goal.
/// </summary>
public class UserCursusTrackNodeDO
{
    /// <summary>
    /// The goal this node represents.
    /// </summary>
    [Required]
    public required GoalLightDO Goal { get; set; }

    /// <summary>
    /// The user's computed state for this goal.
    /// Inactive = not started, Active = in progress, Completed = done, etc.
    /// </summary>
    [Required]
    public EntityObjectState State { get; set; } = EntityObjectState.Inactive;

    /// <summary>
    /// If non-null, this goal is part of a choice group — siblings sharing
    /// the same value are alternatives the user picks from.
    /// </summary>
    public Guid? ChoiceGroup { get; set; }

    /// <summary>
    /// Child nodes in the hierarchy.
    /// </summary>
    public IList<UserCursusTrackNodeDO> Children { get; set; } = [];

    /// <summary>
    /// Build a tree of user track nodes from a flat list of CursusGoal relations
    /// paired with user goal states.
    /// </summary>
    /// <param name="nodes">The flat CursusGoal relations (with Goal loaded).</param>
    /// <param name="userGoalStates">Lookup of GoalId → EntityObjectState from the user's UserGoal records.</param>
    public static IList<UserCursusTrackNodeDO> BuildTree(
        IEnumerable<CursusGoal> nodes,
        IReadOnlyDictionary<Guid, EntityObjectState> userGoalStates)
    {
        var lookup = nodes
            .Select(n => new
            {
                Node = new UserCursusTrackNodeDO
                {
                    Goal = new GoalLightDO(n.Goal),
                    ChoiceGroup = n.ChoiceGroup,
                    State = userGoalStates.TryGetValue(n.GoalId, out var state)
                        ? state
                        : EntityObjectState.Inactive
                },
                n.GoalId,
                n.ParentGoalId
            })
            .ToList();

        var byGoalId = lookup.ToDictionary(x => x.GoalId, x => x.Node);
        var roots = new List<UserCursusTrackNodeDO>();

        foreach (var item in lookup)
        {
            if (item.ParentGoalId is not null && byGoalId.TryGetValue(item.ParentGoalId.Value, out var parent))
                parent.Children.Add(item.Node);
            else
                roots.Add(item.Node);
        }

        return roots;
    }
}
