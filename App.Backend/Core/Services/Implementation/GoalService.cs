// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
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

public class GoalService(DatabaseContext ctx, IProjectService projectService) : BaseService<Goal>(ctx), IGoalService
{
    private readonly DatabaseContext _context = ctx;

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
        return await _context.GoalProject
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

    /// <inheritdoc />
    public async Task<Goal> SetProjectsAsync(Guid goalId, IEnumerable<Guid> projects, CancellationToken token = default)
    {
        var goal = await FindByIdAsync(goalId, token) ?? throw new ServiceException(404, "Goal not found");

        var valid = await projectService.ExistsAsync(projects, token);
        if (!valid) throw new ServiceException(404, "One or more projects not found");

        // NOTE(W2): Goals may have up to 5 Projects
        var existing = await _context.GoalProject.Where(gp => gp.GoalId == goalId).ToListAsync(token);
        _context.GoalProject.RemoveRange(existing);

        var relations = projects.Select(pid => new GoalProject { GoalId = goalId, ProjectId = pid });
        await _context.GoalProject.AddRangeAsync(relations, token);
        await _context.SaveChangesAsync(token);
        return goal;
    }
}
