// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Reviews;
using Microsoft.EntityFrameworkCore;
using App.Backend.Models.Requests.Rubrics;
using App.Backend.Domain.Enums;
using App.Backend.Core.Query;
using System.Linq.Expressions;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class RubricService(DatabaseContext ctx, IGitService git) : BaseService<Rubric>(ctx), IRubricService
{
    private readonly DatabaseContext _context = ctx;

    public override Task<PaginatedList<Rubric>> GetAllAsync(ISorting sorting, IPagination pagination, CancellationToken token = default, params Expression<Func<Rubric, bool>>?[] filters)
    {
        return base.GetAllAsync(sorting, pagination, token, [.. filters, r => r.Public]);
    }

    public override Task DeleteAsync(Rubric entity, CancellationToken token = default)
    {
        entity.Deprecated = true;
        return UpdateAsync(entity, token);
    }

    public async Task<Rubric?> FindByProjectId(Guid projectId, CancellationToken token = default)
    {
        return await _context.Rubrics
            .Include(r => r.Variants)
            .Where(r => r.Enabled && (r.ProjectId == projectId || r.ProjectId == null))
            .OrderByDescending(r => r.ProjectId != null)
            .FirstOrDefaultAsync(token);
    }

    public async Task<Rubric?> FindBySlugAsync(string slug, CancellationToken token = default)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Slug == slug, token);
    }

    public async Task<IEnumerable<RubricVariant>?> GetVariantsAsync(Guid rubricId, CancellationToken token = default)
    {
        return await _context.RubricsVariants
            .AsNoTracking() // <-- Bypasses the change tracker for read-only speed
            .Where(rv => rv.RubricId == rubricId)
            .ToListAsync(token);
    }

    public async Task<Rubric> SetVariantsAsync(
        Guid rubricId,
        IEnumerable<RubricVariant> variants,
        CancellationToken token = default)
    {
        var rubric = await _context.Rubrics.FirstOrDefaultAsync(r => r.Id == rubricId, token)
            ?? throw new ServiceException(404, "Rubric not found");

        await using var transaction = await _context.Database.BeginTransactionAsync(token);
        var existing = await _context.RubricsVariants
            .Where(rv => rv.RubricId == rubricId)
            .ToListAsync(token);

        _context.RubricsVariants.RemoveRange(existing);
        var newVariants = variants
            .Where(v => v.Count > 0)
            .Select(v => new RubricVariant
            {
                RubricId = rubricId,
                Kind = v.Kind,
                Count = v.Count,
            })
            .ToList();

        if (newVariants.Count > 0)
            _context.RubricsVariants.AddRange(newVariants);

        await _context.SaveChangesAsync(token);
        await transaction.CommitAsync(token);

        rubric.Variants = newVariants;
        return rubric;
    }
}
