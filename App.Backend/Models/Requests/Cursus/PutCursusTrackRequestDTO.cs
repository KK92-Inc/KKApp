// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Requests.Cursus;

/// <summary>
/// 
/// </summary>
public class PutCursusTrackRequestDTO : IValidatableObject
{
    public required IEnumerable<(Guid ParentGoalId, Guid GoalId)> Nodes;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Nodes == null || !Nodes.Any())
        {
            yield return new ValidationResult("A track must contain at least one goal.", [nameof(Nodes)]);
            yield break;
        }

        var duplicates = Nodes
            .GroupBy(n => n.GoalId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .Any();

        if (duplicates)
        {
            yield return new ValidationResult("Track contains duplicate goals.", [nameof(Nodes)]);
            yield break;
        }

        // Parent Existence Check
        var map = Nodes.ToDictionary(n => n.GoalId, n => n.ParentGoalId);
        foreach (var (ParentGoalId, GoalId) in Nodes)
        {
            if (!map.ContainsKey(ParentGoalId))
            {
                yield return new ValidationResult(
                    $"A Goal references missing parent goal.",
                    [nameof(Nodes)]);
            }
        }

        // Maximum 4 Children Per Node Check
        var exceeding = Nodes
            .GroupBy(n => n.ParentGoalId)
            .Where(g => g.Count() > 4)
            .Select(g => new {
                ParentGoalId = g.Key,
                ChildCount = g.Count()
            });

        foreach (var excess in exceeding)
        {
            yield return new ValidationResult(
                $"Goal {excess.ParentGoalId} exceeds the maximum fan-out limit with {excess.ChildCount} children (max allowed is 4).",
                [nameof(Nodes)]);
        }

        foreach (var (ParentGoalId, GoalId) in Nodes)
        {
            int depth = 0;
            var visited = new HashSet<Guid>();
            Guid? current = GoalId;

            while (current.HasValue)
            {
                if (!visited.Add(current.Value))
                {
                    yield return new ValidationResult($"Infinite Loop/cycle detected.", [nameof(Nodes)]);
                    break;
                }

                depth++;
                if (depth > 6)
                {
                    yield return new ValidationResult(
                        $"Track depth exceeds the maximum allowed limit of 5 goals.",
                        [nameof(Nodes)]
                    );
                    break;
                }

                if (map.TryGetValue(current.Value, out var parentId))
                    current = parentId;
                else
                    break;
            }
        }
    }
}
