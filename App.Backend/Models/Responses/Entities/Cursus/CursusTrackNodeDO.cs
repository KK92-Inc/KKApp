// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Relations;
using App.Backend.Models.Responses.Entities.Goals;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursus;

/// <summary>
/// A single node in the cursus track tree.
/// Contains the goal information and its children.
/// </summary>
public class CursusTrackNodeDO
{
    /// <summary>
    /// The goal this node represents.
    /// </summary>
    [Required]
    public required GoalLightDO Goal { get; set; }

    /// <summary>
    /// If non-null, this goal is part of a choice group — siblings sharing
    /// the same value are alternatives the user picks from.
    /// </summary>
    public Guid? ChoiceGroup { get; set; }

    /// <summary>
    /// Child nodes in the hierarchy.
    /// </summary>
    public IList<CursusTrackNodeDO> Children { get; set; } = [];

    /// <summary>
    /// Build a tree of track nodes from a flat list of CursusGoal relations.
    /// </summary>
    public static IList<CursusTrackNodeDO> BuildTree(IEnumerable<CursusGoal> nodes)
    {
        var lookup = nodes
            .Select(n => new
            {
                Node = new CursusTrackNodeDO
                {
                    Goal = new GoalLightDO(n.Goal),
                    ChoiceGroup = n.ChoiceGroup
                },
                n.GoalId,
                n.ParentGoalId
            })
            .ToList();

        var byGoalId = lookup.ToDictionary(x => x.GoalId, x => x.Node);
        var roots = new List<CursusTrackNodeDO>();

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
