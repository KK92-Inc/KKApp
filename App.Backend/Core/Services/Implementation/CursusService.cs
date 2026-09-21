// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities.Cursi;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class CursusService(
    DatabaseContext ctx,
    IGoalService goalService
) : BaseService<Cursus>(ctx), ICursusService, ISlugQueryable<Cursus>
{
    public async Task<Cursus?> FindBySlugAsync(string slug, CancellationToken token = default)
    {
        return await ctx.Cursi.FirstOrDefaultAsync(g => g.Slug == slug, token);
    }

    public async Task ValidateTrackAsync(IReadOnlyList<(Guid GoalId, Guid? ParentId)> nodes, CancellationToken token = default)
    {
        // Structural checks (duplicates, dangling parents, cycles, depth, fan-out) already
        // ran in PutCursusTrackRequestDTO.Validate before this is ever called.
        var goalIds = nodes.Select(n => n.GoalId).Distinct().ToList();
        ServiceException.ThrowIf(!await goalService.ExistsAsync(goalIds, token), "One or more goal IDs are invalid");
    }

    public async Task<IReadOnlyList<CursusGoal>> SetTrackAsync(Guid cursusId, IEnumerable<CursusGoal> nodes, CancellationToken token = default)
    {
        var cursus = await FindByIdAsync(cursusId, token) ?? throw new ServiceException(404, "Cursus not found");
        ServiceException.ThrowIf(cursus.Variant is not CursusVariant.Static, "Track can only be replaced on static cursus types");

        var existing = await ctx.CursusGoal.Where(cg => cg.CursusId == cursusId).ToListAsync(token);
        if (existing.Count > 0)
            ctx.CursusGoal.RemoveRange(existing);

        var list = nodes.Select(n => { n.CursusId = cursusId; return n; }).ToList();
        await ctx.CursusGoal.AddRangeAsync(list, token);
        await ctx.SaveChangesAsync(token);

        return await ctx.CursusGoal
            .Where(cg => cg.CursusId == cursusId)
            .Include(cg => cg.Goal)
            .ToListAsync(token);
    }

    public async Task<IReadOnlyList<CursusGoal>> GetTrackAsync(Guid cursusId, CancellationToken token = default)
    {
        return await ctx.CursusGoal
            .Where(cg => cg.CursusId == cursusId)
            .Include(cg => cg.Goal)
            .ToListAsync(token);
    }

    public CursusTrackDO AssembleTrack(Cursus cursus, IReadOnlyList<CursusGoal> nodes) => new()
    {
        CursusId = cursus.Id,
        Name = cursus.Name,
        CompletionMode = cursus.Mode,
        Nodes = [.. nodes.Select(n => new CursusTrackNodeDO
        {
            GoalId = n.GoalId,
            Name = n.Goal.Name,
            Slug = n.Goal.Slug,
            ParentGoalId = n.ParentGoalId,
        })]
    };
}
