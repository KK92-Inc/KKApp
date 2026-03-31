// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Reviews;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class RubricService(DatabaseContext ctx, IGitService git) : BaseService<Rubric>(ctx), IRubricService
{
    private readonly DatabaseContext _context = ctx;

    public async Task<Rubric?> FindBySlugAsync(string slug, CancellationToken token = default)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Slug == slug, token);
    }

    public async Task<bool> HasRubricMarkdownAsync(Guid rubricId, CancellationToken token = default)
    {
        var rubric = await _dbSet
            .Include(r => r.GitInfo)
            .FirstOrDefaultAsync(r => r.Id == rubricId, token);

        if (rubric?.GitInfo is null)
            return false;

        var blob = await git.GetBlobAsync(
            rubric.GitInfo.Owner,
            rubric.GitInfo.Name,
            "main",
            "RUBRIC.md",
            token
        );

        return blob is not null;
    }
}
