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

    /// <summary>
    /// Eager-load Variants so EF Core tracks child entity state changes accurately.
    /// </summary>
    public override async Task<Rubric?> FindByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _dbSet
            .Include(r => r.Variants)
            .FirstOrDefaultAsync(x => x.Id == id, token);
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

    public async Task<Rubric> UpdateRubricAsync(Rubric entity, IEnumerable<(ReviewKinds Kind, int Count)>? variants = null, CancellationToken token = default)
    {
        if (variants is not null)
        {
            var desiredVariants = variants.ToList();

            var existingVariants = await _context.Set<RubricVariant>()
                .Where(v => v.RubricId == entity.Id)
                .ToListAsync(token);

            // 1. Remove variants no longer present in incoming request
            var toDelete = existingVariants
                .Where(e => !desiredVariants.Any(d => d.Kind == e.Kind))
                .ToList();

            if (toDelete.Count > 0)
            {
                _context.Set<RubricVariant>().RemoveRange(toDelete);
            }

            // 2. Add or Update existing variants
            foreach (var desired in desiredVariants)
            {
                var existing = existingVariants.FirstOrDefault(e => e.Kind == desired.Kind);
                if (existing is not null)
                {
                    existing.Count = desired.Count;
                }
                else
                {
                    var newVariant = new RubricVariant
                    {
                        RubricId = entity.Id,
                        Kind = desired.Kind,
                        Count = desired.Count
                    };
                    // Explicit Add forces EntityState.Added regardless of pre-generated Id
                    _context.Set<RubricVariant>().Add(newVariant);
                }
            }
        }

        await _context.SaveChangesAsync(token);
        return entity;
    }
}