// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Requests.Cursus;

/// <summary>
/// A single node in the flat representation of a cursus track.
/// </summary>
public class PutCursusTrackNodeDO
{
    /// <summary>
    /// The goal ID this node represents.
    /// </summary>
    [Required]
    [Description("The goal ID this node represents.")]
    public required Guid GoalId { get; init; }

    /// <summary>
    /// The parent goal ID within this cursus track.
    /// Null for root-level goals.
    /// </summary>
    [Required]
    [Description("The parent goal ID within this cursus track. Null for root-level goals.")]
    public Guid? ParentId { get; init; }
}


/// <summary>
/// The full proposed track for a cursus. Always a complete replacement, never a delta -
/// see <see cref="Core.Services.Interface.ICursusService.SetTrackAsync"/>.
/// </summary>
public class PutCursusTrackRequestDTO : IValidatableObject
{
    public required IEnumerable<PutCursusTrackNodeDO> Nodes { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var nodes = Nodes?.ToList() ?? [];
        if (nodes.Count == 0)
        {
            yield return new ValidationResult("A track must contain at least one goal.", [nameof(Nodes)]);
            yield break;
        }

        var duplicates = nodes
            .GroupBy(n => n.GoalId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            yield return new ValidationResult(
                $"Track contains duplicate goal(s): {string.Join(", ", duplicates)}.", [nameof(Nodes)]);
            yield break;
        }

        var byId = nodes.ToDictionary(n => n.GoalId, n => n.ParentId);

        // A parent reference is only valid if it points at another node IN THIS TRACK.
        // Null (root) is always fine and is not checked here.
        foreach (var node in nodes)
        {
            if (node.ParentId is Guid parentId && !byId.ContainsKey(parentId))
            {
                yield return new ValidationResult(
                    $"Goal {node.GoalId} references parent {parentId}, which is not part of this track.",
                    [nameof(Nodes)]);
            }
        }

        const int maxChildren = 4;
        var crowded = nodes
            .Where(n => n.ParentId is not null)
            .GroupBy(n => n.ParentId!.Value)
            .Where(g => g.Count() > maxChildren);

        foreach (var group in crowded)
        {
            yield return new ValidationResult(
                $"Goal {group.Key} has {group.Count()} direct children, exceeding the maximum fan-out of {maxChildren}.",
                [nameof(Nodes)]);
        }

        const int maxDepth = 10;
        var marks = new Dictionary<Guid, byte>(); // 0/unset = white, 1 = gray (on this walk), 2 = black (resolved)
        var depths = new Dictionary<Guid, int>();

        foreach (var start in byId.Keys)
        {
            if (marks.GetValueOrDefault(start) == 2)
                continue;

            var path = new List<Guid>();
            var current = start;
            var cycle = false;

            while (true)
            {
                if (marks.GetValueOrDefault(current) == 1)
                {
                    cycle = true;
                    break;
                }

                if (marks.GetValueOrDefault(current) == 2)
                    break;

                marks[current] = 1;
                path.Add(current);

                var parent = byId[current];
                if (parent is null)
                    break;

                current = parent.Value;
            }

            if (cycle)
            {
                yield return new ValidationResult($"Track contains a cycle involving goal {current}.", [nameof(Nodes)]);
                yield break;
            }

            // Walk the collected path back to front, assigning depth from the root down.
            var baseDepth = marks.GetValueOrDefault(current) == 2 ? depths[current] : 0;
            for (var i = path.Count - 1; i >= 0; i--)
            {
                baseDepth++;
                depths[path[i]] = baseDepth;
                marks[path[i]] = 2;
            }
        }

        foreach (var (goalId, depth) in depths)
        {
            if (depth > maxDepth)
            {
                yield return new ValidationResult(
                    $"Goal {goalId} sits at depth {depth}, exceeding the maximum track depth of {maxDepth}.",
                    [nameof(Nodes)]);
            }
        }
    }
}
