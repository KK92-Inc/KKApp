// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using App.Backend.Core.Query;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class ProjectService(DatabaseContext ctx) : BaseService<Project>(ctx), IProjectService
{
    private readonly DatabaseContext context = ctx;

    public override Task DeleteAsync(Project entity, CancellationToken token = default)
    {
        entity.Deprecated = true; // Deletion should be a soft delete.
        return UpdateAsync(entity, token);
    }

    public async Task<Project?> FindBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<PaginatedList<UserProject>> GetUserProjectsAsync(Guid userId, ISorting sorting, IPagination pagination, CancellationToken token = default)
    {
        return await _dbSet.AsQueryable<Project>()
            .Where(p => p.Active)
            .Join(
                context.UserProjects,
                project => project.Id,
                userProject => userProject.ProjectId,
                (project, userProject) => new { project, userProject }
            )
            .Where(joined => joined.userProject.Members.Any(m => m.UserId == userId))
            .Select(joined => joined.userProject)
            .Sort(sorting)
            .PaginateAsync(pagination, token);
        // return await _dbSet.AsQueryable()
        //     .Sort(sorting)
        //     .PaginateAsync(pagination, token);
    }
}
