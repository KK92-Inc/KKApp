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
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Entities;
using System.Net;
using System.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Options;
using App.Backend.Core.Services.Options;
using App.Backend.Domain.Values.Misc;
using App.Backend.Domain.Relations;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class WorkspaceService(DatabaseContext ctx, IGitService git, ICursusService cursusService, IOptions<GitServiceOptions> options) : BaseService<Workspace>(ctx), IWorkspaceService
{
    private readonly DatabaseContext ctx = ctx;

    public async override Task<Workspace> CreateAsync(Workspace entity, CancellationToken token = default)
    {
        var result = await base.CreateAsync(entity, token);
        if (result.Ownership is not EntityOwnership.Organization)
        {
            await ctx.Members.AddAsync(new()
            {
                EntityId = result.Id,
                UserId = result.OwnerId ?? throw new ServiceException(500, "User-owned workspace must have an owner ID"),
                EntityType = MemberEntityType.Workspace,
                Role = MemberRole.Leader
            }, token);
        }

        await ctx.SaveChangesAsync(token);
        return result;
    }

    public async Task<Workspace?> FindByUserId(Guid id, CancellationToken token = default)
    {
        return await ctx.Workspaces
            .FirstOrDefaultAsync(w => w.OwnerId == id && w.Ownership == EntityOwnership.User, token);
    }

    public async Task<Workspace> GetRootWorkspace(CancellationToken token = default)
    {
        var root = await ctx.Workspaces.FirstOrDefaultAsync(w => w.OwnerId == null, token);
        return root ?? throw new ServiceException(501, "Environment is missing a root workspace");
    }

    public async Task<Project> AddProjectAsync(Guid workspaceId, Project project, Commit commit, CancellationToken token = default)
    {
        var strategy = ctx.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);
            var workspace = await FindByIdAsync(workspaceId, ct) ?? throw new ServiceException(404, "Workspace not found");

            try
            {
                project.WorkspaceId = workspace.Id;
                var output = await ctx.Projects.AddAsync(project, ct);
                await ctx.SaveChangesAsync(ct);

                var owner = "project";
                var name = project.Id.ToString();
                if (!await git.CreateAsync(owner, name, ct))
                    throw new ServiceException(409, "Repository for such project already exists");

                var repo = await ctx.GitInfo.AddAsync(new()
                {
                    Owner = owner,
                    Name = name,
                    Ownership = workspace.Owner is null ? EntityOwnership.Organization : EntityOwnership.User
                }, ct);

                project.GitId = repo.Entity.Id;
                await ctx.SaveChangesAsync(ct);

                // Commit will assemble the branch on the other end.
                if (!await git.Commit(owner, name, options.Value.DefaultBranch, commit, ct))
                    throw new ServiceException(409, "Repository for such rubric");

                await transaction.CommitAsync(ct);
                return output.Entity;
            }
            catch (Exception e)
            {
                if (e is ServiceException se)
                    throw se;

                await transaction.RollbackAsync(ct);
                await git.DeleteAsync("project", project.Id.ToString(), ct);
                throw new ServiceException(500, $"Something went wrong: {e.Message}");
            }
        }, token);
    }

    public async Task<Goal> AddGoalAsync(Guid workspaceId, Goal goal, CancellationToken token = default)
    {
        var workspace = await FindByIdAsync(workspaceId, token) ?? throw new ServiceException(404, "Workspace not found");

        goal.WorkspaceId = workspace.Id;
        var output = await ctx.Goals.AddAsync(goal);
        await ctx.SaveChangesAsync(token);
        return output.Entity;
    }

    public async Task<Cursus> AddCursusAsync(Guid workspaceId, Cursus cursus, IEnumerable<CursusGoal> nodes, CancellationToken token = default)
    {
        var workspace = await FindByIdAsync(workspaceId, token) ?? throw new ServiceException(404, "Workspace not found");
        var strategy = ctx.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);
            cursus.WorkspaceId = workspace.Id;

            var output = await ctx.Cursi.AddAsync(cursus, ct);
            await ctx.SaveChangesAsync(ct);

            await cursusService.SetTrackAsync(cursus.Id, nodes, ct);
            await transaction.CommitAsync(ct);
            return output.Entity;
        }, token);
    }

    public async Task<Rubric> AddRubricAsync(Guid workspaceId, Rubric rubric, Commit commit, CancellationToken token = default)
    {
        if (await ctx.Rubrics.FirstOrDefaultAsync(r => r.ProjectId == null, token) is not null && rubric.ProjectId is null)
            throw new ServiceException("Wildcard Rubric already exists, there can only ever be one wildcard rubric.");

        var strategy = ctx.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);
            var workspace = await FindByIdAsync(workspaceId, ct) ?? throw new ServiceException(404, "Workspace not found");

            try
            {
                rubric.WorkspaceId = workspace.Id;
                var output = await ctx.Rubrics.AddAsync(rubric, ct);
                await ctx.SaveChangesAsync(ct);

                var owner = "rubric";
                var name = rubric.Id.ToString();
                if (!await git.CreateAsync(owner, name, ct))
                    throw new ServiceException(409, "Repository for such rubric already exists");

                var repo = await ctx.GitInfo.AddAsync(new()
                {
                    Owner = owner,
                    Name = name,
                    Ownership = workspace.Owner is null ? EntityOwnership.Organization : EntityOwnership.User
                }, ct);

                rubric.GitInfoId = repo.Entity.Id;
                await ctx.SaveChangesAsync(ct);

                // Commit will assemble the branch on the other end.
                if (!await git.Commit(owner, name, options.Value.DefaultBranch, commit, ct))
                    throw new ServiceException(409, "Repository for such rubric already exists");

                await transaction.CommitAsync(ct);
                return output.Entity;
            }
            catch (Exception e)
            {
                if (e is ServiceException se)
                    throw se;

                await transaction.RollbackAsync(ct);
                await git.DeleteAsync("project", rubric.Id.ToString(), ct);
                throw new ServiceException(500, $"Something went wrong: {e.Message}");
            }
        }, token);
    }
}
