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
using Microsoft.Extensions.Logging;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class GoalService(DatabaseContext ctx, ILogger<GoalService> log) : BaseService<Goal>(ctx), IGoalService
{
    private readonly DatabaseContext _context = ctx;

    public async Task<Goal?> FindBySlugAsync(string slug, CancellationToken token = default)
    {
        return await _context.Goals.FirstOrDefaultAsync(g => g.Slug == slug);
    }

    public Task<PaginatedList<UserGoal>> GetUsersAsync(Guid goalId, ISorting sorting, IPagination pagination, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<Goal> SetProjectsAsync(Guid goalId, IEnumerable<Guid> projects, CancellationToken token = default)
    {
        var goal = await FindByIdAsync(goalId, token) ?? throw new ServiceException(404, "Goal not found");
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            var existing = await _context.GoalProject
                .Where(gp => gp.GoalId == goalId)
                .ToListAsync(ct);

            _context.GoalProject.RemoveRange(existing);

            var updated = projects.Select(pid => new GoalProject
            {
                GoalId = goalId,
                ProjectId = pid
            });

            await _context.GoalProject.AddRangeAsync(updated, ct);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return goal;
        }, token);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Project>> GetProjectsAsync(Guid goalId, CancellationToken token = default)
    {
        return await _context.GoalProject
            .AsNoTracking()
            .Include(gp => gp.Project)
            .Include(gp => gp.Project.Workspace)
            .Include(gp => gp.Project.Workspace.Owner)
            .Where(gp => gp.GoalId == goalId)
            .Select(r => r.Project)
            .ToListAsync(token);
    }
}
