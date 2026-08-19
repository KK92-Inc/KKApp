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

    public override async Task<Rubric> UpdateAsync(Rubric entity, CancellationToken token = default)
    {
        // If variants were updated on the entity, handle existing variants cleanup
        var existingVariants = await _context.RubricsVariants
            .Where(rv => rv.RubricId == entity.Id)
            .ToListAsync(token);

        _context.RubricsVariants.RemoveRange(existingVariants);
        if (entity.Variants.Count > 0)
            _context.RubricsVariants.AddRange(entity.Variants);

        _context.Rubrics.Update(entity);
        await _context.SaveChangesAsync(token);
        return entity;
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
            .AsNoTracking()
            .Where(rv => rv.RubricId == rubricId)
            .ToListAsync(token);
    }

    public async Task<Rubric> SetVariantsAsync(IEnumerable<RubricVariant> variants, CancellationToken token = default)
    {
        var variantList = variants.Where(v => v.Count > 0).ToList();
        var rubricId = variantList.FirstOrDefault()?.RubricId
            ?? throw new ArgumentException("Variants must contain a valid RubricId", nameof(variants));

        var rubric = await _context.Rubrics
            .Include(r => r.Variants)
            .FirstOrDefaultAsync(r => r.Id == rubricId, token)
            ?? throw new ServiceException(404, "Rubric not found");

        _context.RubricsVariants.RemoveRange(rubric.Variants);
        rubric.Variants = variantList;
        await _context.SaveChangesAsync(token);
        return rubric;
    }
}
