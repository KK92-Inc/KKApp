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

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class RubricService(DatabaseContext ctx, IGitService git) : BaseService<Rubric>(ctx), IRubricService
{
    private readonly DatabaseContext _context = ctx;

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

    public async Task<Rubric?> SetVariantsAsync(
        Guid rubricId,
        IEnumerable<(ReviewKinds Kind, int Required)> variants,
        CancellationToken token = default)
    {
        var rubric = await _context.Rubrics
            .Include(r => r.Variants)
            .FirstOrDefaultAsync(r => r.Id == rubricId, token);

        if (rubric is null)
            return null;

        _context.RubricsVariants.RemoveRange(rubric.Variants);
        rubric.Variants = [.. variants.Where(v => v.Required > 0)
            .Select(v => new RubricVariant
            {
                RubricId = rubricId,
                Kind = v.Kind,
                Count = v.Required,
            })
        ];

        await _context.SaveChangesAsync(token);
        return rubric;
    }
}
