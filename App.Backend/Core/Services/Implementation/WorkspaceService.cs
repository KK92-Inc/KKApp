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
using App.Backend.Domain.Relations;
using App.Git.Models.Requests;

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

    public async Task<Rubric> AddRubricAsync(Guid workspaceId, Rubric rubric, PostCommitWithAuthorDTO commit, CancellationToken token = default)
    {
        var workspace = await FindByIdAsync(workspaceId, token) ?? throw new ServiceException(404, "Workspace not found");
        if (rubric.ProjectId is null)
        {
            ServiceException.ThrowIf(workspace.OwnerId is not null, "Wildcard rubrics can only be created in the root workspace.");
            var taken = await ctx.Rubrics.AnyAsync(r => r.ProjectId == null && r.WorkspaceId == workspaceId, token);
            ServiceException.ThrowIf(taken, "Wildcard Rubric already exists, there can only ever be one wildcard rubric.");
        }
        else
        {
            var project = await ctx.Projects.FirstOrDefaultAsync(p => p.Id == rubric.ProjectId, token) ?? throw new ServiceException(404, "Project not found");
            if (project.WorkspaceId != workspaceId)
                throw new ServiceException(422, "Rubric project must belong to the target workspace");

            var taken = await ctx.Rubrics.AnyAsync(r => r.ProjectId == rubric.ProjectId, token);
            ServiceException.ThrowIf(taken, "A rubric for this project already exists");
        }

        var strategy = ctx.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            var owner = "rubric";
            var name = rubric.Id.ToString();

            try
            {
                if (!await git.CreateAsync(owner, name, ct))
                    throw new ServiceException(409, "Repository for such rubric already exists");

                var repo = await ctx.GitInfo.AddAsync(new()
                {
                    Owner = owner,
                    Name = name,
                    Ownership = workspace.Owner is null ? EntityOwnership.Organization : EntityOwnership.User
                }, ct);
                await ctx.SaveChangesAsync(ct);

                rubric.WorkspaceId = workspace.Id;
                rubric.GitInfoId = repo.Entity.Id;
                var output = await ctx.Rubrics.AddAsync(rubric, ct);
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
                await git.DeleteAsync(owner, name, ct);
                throw new ServiceException(500, $"Something went wrong: {e.Message}");
            }
        }, token);
    }

    public async Task<Project> AddProjectAsync(Guid workspaceId, Project project, PostCommitWithAuthorDTO commit, CancellationToken token = default)
    {
        var strategy = ctx.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);
            var workspace = await FindByIdAsync(workspaceId, ct) ?? throw new ServiceException(404, "Workspace not found");

            var owner = "project";
            var name = project.Id.ToString();

            try
            {
                if (!await git.CreateAsync(owner, name, ct))
                    throw new ServiceException(409, "Repository for such project already exists");

                var repo = await ctx.GitInfo.AddAsync(new()
                {
                    Owner = owner,
                    Name = name,
                    Ownership = workspace.Owner is null ? EntityOwnership.Organization : EntityOwnership.User
                }, ct);
                await ctx.SaveChangesAsync(ct);

                project.WorkspaceId = workspace.Id;
                project.GitId = repo.Entity.Id;
                var output = await ctx.Projects.AddAsync(project, ct);
                await ctx.SaveChangesAsync(ct);

                if (!await git.Commit(owner, name, options.Value.DefaultBranch, commit, ct))
                    throw new ServiceException(409, "Repository for such project already exists");

                await transaction.CommitAsync(ct);
                return output.Entity;
            }
            catch (Exception e)
            {
                if (e is ServiceException se)
                    throw se;

                await transaction.RollbackAsync(ct);
                await git.DeleteAsync(owner, name, ct);
                throw new ServiceException(500, $"Something went wrong: {e.Message}");
            }
        }, token);
    }
}
