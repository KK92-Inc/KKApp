// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Enums;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using App.Backend.Core.Query;
using System.Linq.Expressions;
using App.Backend.Models.Responses.Entities.Cursus;
using App.Backend.Models.Responses.Entities.Goals;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class CursusService(DatabaseContext ctx, IGoalService goalService, ILogger<CursusService> log) : BaseService<Cursus>(ctx), ICursusService, ISlugQueryable<Cursus>
{
    public async Task<string?> ValidateTrackAsync(
        IReadOnlyList<(Guid GoalId, Guid? ParentId, Guid? Group)> nodes,
        CancellationToken token = default)
    {
        var allIds = nodes.Select(n => n.GoalId).ToList();
        var distinctIds = allIds.Distinct().ToList();

        if (distinctIds.Count != allIds.Count)
            return "Duplicate goals are not allowed in a track";

        if (!await goalService.ExistsAsync(distinctIds, token))
            return "One or more goal IDs are invalid";

        var parentLookup = nodes.ToDictionary(n => n.GoalId, n => n.ParentId);
        var validated = new HashSet<Guid>();

        foreach (var (GoalId, ParentId, Group) in nodes)
        {
            var current = GoalId;
            var path = new HashSet<Guid>();
            while (parentLookup.TryGetValue(current, out var parentId) && parentId.HasValue)
            {
                if (!parentLookup.ContainsKey(parentId.Value))
                    return $"Parent goal {parentId.Value} is not part of this track";

                if (validated.Contains(current))
                    break;

                if (!path.Add(current))
                    return $"Cyclic dependency detected involving goal {current}";

                current = parentId.Value;
            }

            validated.UnionWith(path);
        }

        var invalidGroup = nodes
            .Where(n => n.Group.HasValue)
            .GroupBy(n => n.Group!.Value)
            .FirstOrDefault(g => g.Select(n => n.ParentId).Distinct().Count() > 1);

        if (invalidGroup is not null)
            return $"All goals in choice group {invalidGroup.Key} must share the same parent";

        return null;
    }

    public CursusTrackDO AssembleTrack(Cursus cursus, IReadOnlyList<CursusGoal> goals)
    {
        var entries = goals.Select(g => (
            Node: new CursusTrackNodeDO { Goal = new GoalLightDO(g.Goal), ChoiceGroup = g.ChoiceGroup },
            g.GoalId,
            g.ParentGoalId
        )).ToList();

        var byId = entries.ToDictionary(e => e.GoalId, e => e.Node);
        var roots = new List<CursusTrackNodeDO>();

        foreach (var (node, _, parentId) in entries)
        {
            if (parentId is not null && byId.TryGetValue(parentId.Value, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        return new CursusTrackDO
        {
            CursusId = cursus.Id,
            Variant = cursus.Variant,
            CompletionMode = cursus.CompletionMode,
            Nodes = roots
        };
    }

    public async Task<IReadOnlyList<CursusGoal>> ReplaceTrackAsync(
        Guid cursusId,
        IEnumerable<CursusGoal> nodes,
        CancellationToken token = default)
    {
        var strategy = ctx.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            var cursus = await FindByIdAsync(cursusId, ct)
                ?? throw new ServiceException(404, "Cursus not found");

            if (cursus.Variant != CursusVariant.Static)
                throw new ServiceException(400, "Track can only be replaced on static cursus types");

            var existing = await ctx.CursusGoal
                .Where(cg => cg.CursusId == cursusId)
                .ToListAsync(ct);

            if (existing.Count > 0)
                ctx.CursusGoal.RemoveRange(existing);

            var nodeList = nodes.Select(n => { n.CursusId = cursusId; return n; }).ToList();
            await ctx.CursusGoal.AddRangeAsync(nodeList, ct);
            await ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            log.LogInformation("Replaced track for cursus {CursusId} with {Count} nodes", cursusId, nodeList.Count);

            // Reload with navigation properties so the caller doesn't need a second round-trip
            return await ctx.CursusGoal
                .Where(cg => cg.CursusId == cursusId)
                .Include(cg => cg.Goal)
                .ToListAsync(ct);
        }, token);
    }

    public async Task<IReadOnlyList<CursusGoal>> GetTrackAsync(Guid cursusId, CancellationToken token = default)
    {
        return await ctx.CursusGoal
            .Where(cg => cg.CursusId == cursusId)
            .Include(cg => cg.Goal)
            .ToListAsync(token);
    }

    public async Task<Cursus?> FindBySlugAsync(string slug, CancellationToken token = default)
    {
        return await ctx.Cursi.FirstOrDefaultAsync(g => g.Slug == slug, token);
    }
}
