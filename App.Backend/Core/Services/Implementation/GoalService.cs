// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Relations;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using App.Backend.Core.Query;
using App.Backend.Domain.Entities.Users;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class GoalService(DatabaseContext ctx) : BaseService<Goal>(ctx), IGoalService
{
    private readonly DatabaseContext context = ctx;

    public Task<Goal?> FindBySlugAsync(string slug)
    {
        return _dbSet.FirstOrDefaultAsync(g => g.Slug == slug);
    }

    public Task<Goal?> FindBySlugAsync(string slug, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Project>> GetGoalProjectsAsync(Guid goalId)
    {
        return await context.GoalProject
            .Where(gp => gp.GoalId == goalId)
            .Include(gp => gp.Project)
            .Select(gp => gp.Project)
            .ToListAsync();
    }

    public Task<IEnumerable<Project>> GetProjectsAsync(Guid goalId, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<PaginatedList<UserGoal>> GetUsersAsync(Guid goalId, ISorting sorting, IPagination pagination, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}