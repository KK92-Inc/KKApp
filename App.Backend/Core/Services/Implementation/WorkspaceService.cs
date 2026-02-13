// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Users;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Entities.Projects;
using App.Backend.Domain.Entities;
using System.Net;
using System.Data;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class WorkspaceService(DatabaseContext ctx, IGitService git) : BaseService<Workspace>(ctx), IWorkspaceService
{
    private readonly DatabaseContext _context = ctx;

    public async Task<Workspace?> FindByUserId(Guid id, CancellationToken token = default)
    {
        return await _context.Workspaces
            .FirstOrDefaultAsync(w => w.OwnerId == id && w.Ownership == EntityOwnership.User, token);
    }

    public async Task<Workspace> GetRootWorkspace(CancellationToken token = default)
    {
        var root = await _context.Workspaces.FirstOrDefaultAsync(w => w.Ownership == EntityOwnership.Organization);
        return root ?? throw new ServiceException(501, "Environment is missing a root workspace");
    }

    public async Task<Project> AddProjectAsync(Guid workspaceId, Project project, CancellationToken token = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            var workspace = await FindByIdAsync(workspaceId, ct) ?? throw new ServiceException(404, "Workspace not found");
            var owner = workspace.Owner?.Login ?? "root";

            try
            {
                var name = project.Name.ToSlug();
                if (!await git.CreateAsync(owner, name, ct))
                    throw new ServiceException(409, "Repository for such project already exists");

                var repo = await _context.GitInfo.AddAsync(new()
                {
                    Owner = owner,
                    Name = name,
                    Ownership = workspace.Owner is null ? EntityOwnership.Organization : EntityOwnership.User
                }, ct);

                await _context.SaveChangesAsync(ct);

                project.GitId = repo.Entity.Id;
                project.WorkspaceId = workspace.Id;
                var output = await _context.Projects.AddAsync(project, ct);

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return output.Entity;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(ct);
                await git.DeleteAsync(owner, project.Name);
                throw new ServiceException(500, $"Something went wrong: {e.Message}");
            }
        }, token);
    }

    public async Task<Goal> AddGoalAsync(Guid workspaceId, Goal goal, CancellationToken token = default)
    {
        var workspace = await FindByIdAsync(workspaceId, token) ?? throw new ServiceException(404, "Workspace not found");

        goal.WorkspaceId = workspace.Id;
        var output = await _context.Goals.AddAsync(goal);
        await _context.SaveChangesAsync(token);
        return output.Entity;
    }

    public async Task<Cursus> AddCursusAsync(Guid workspaceId, Cursus cursus, CancellationToken token = default)
    {
        var workspace = await FindByIdAsync(workspaceId, token) ?? throw new ServiceException(404, "Workspace not found");

        cursus.WorkspaceId = workspace.Id;
        var output = await _context.Cursi.AddAsync(cursus);
        await _context.SaveChangesAsync(token);
        return output.Entity;
    }
}
