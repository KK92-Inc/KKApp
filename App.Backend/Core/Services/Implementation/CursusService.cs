// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Enums;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class CursusService(DatabaseContext ctx, ILogger<CursusService> log) : BaseService<Cursus>(ctx), ICursusService
{
    private readonly DatabaseContext context = ctx;

    public async Task<Cursus?> FindBySlugAsync(string slug)
    {
        return await context.Cursi.FirstOrDefaultAsync(g => g.Slug == slug);
    }

    public Task<IEnumerable<Project>> GetCursusGoals(Guid goalId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Project>> GetCursusProjects(Guid cursusId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> GetCursusUsers(Guid cursusId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CursusGoal>> SetTrackAsync(
        Guid cursusId,
        IEnumerable<CursusGoal> nodes,
        CancellationToken token = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            var cursus = await FindByIdAsync(cursusId, ct)
                ?? throw new ServiceException(404, "Cursus not found");

            if (cursus.Variant != CursusVariant.Fixed)
                throw new ServiceException(400, "Track can only be set on Fixed cursus types");

            // Remove all existing track nodes
            var existing = await context.CursusGoal
                .Where(cg => cg.CursusId == cursusId)
                .ToListAsync(ct);

            if (existing.Count > 0)
                context.CursusGoal.RemoveRange(existing);

            // Add the new track nodes
            var nodeList = nodes.ToList();
            foreach (var node in nodeList)
                node.CursusId = cursusId;

            await context.CursusGoal.AddRangeAsync(nodeList, ct);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            log.LogInformation("Set track for cursus {CursusId} with {Count} nodes", cursusId, nodeList.Count);
            return (IEnumerable<CursusGoal>)nodeList;
        }, token);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CursusGoal>> GetTrackAsync(Guid cursusId, CancellationToken token = default)
    {
        return await context.CursusGoal
            .Where(cg => cg.CursusId == cursusId)
            .Include(cg => cg.Goal)
            .OrderBy(cg => cg.Position)
            .ToListAsync(token);
    }
}
