// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Security.Claims;
using App.Git.Models;
using App.Git.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace App.Git.Endpoints;

/// <summary>
/// Repository API endpoints.
/// </summary>
public static class RepositoryEndpoints
{
    public static RouteGroupBuilder MapRepositoryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListRepositories)
            .WithName("ListRepositories")
            .WithSummary("List all repositories accessible to the current user");

        group.MapPost("/", CreateRepository)
            .WithName("CreateRepository")
            .WithSummary("Create a new repository")
            .RequireAuthorization();

        group.MapGet("/{owner}/{name}", GetRepository)
            .WithName("GetRepository")
            .WithSummary("Get a repository by owner and name");

        group.MapPatch("/{owner}/{name}", UpdateRepository)
            .WithName("UpdateRepository")
            .WithSummary("Update repository settings")
            .RequireAuthorization();

        group.MapDelete("/{owner}/{name}", DeleteRepository)
            .WithName("DeleteRepository")
            .WithSummary("Delete a repository")
            .RequireAuthorization();

        // Git data endpoints
        group.MapGet("/{owner}/{name}/commits", GetCommits)
            .WithName("GetCommits")
            .WithSummary("Get commits for a repository");

        group.MapGet("/{owner}/{name}/commits/{sha}", GetCommit)
            .WithName("GetCommit")
            .WithSummary("Get a specific commit");

        group.MapGet("/{owner}/{name}/branches", GetBranches)
            .WithName("GetBranches")
            .WithSummary("Get branches for a repository");

        group.MapGet("/{owner}/{name}/tree/{**path}", GetTree)
            .WithName("GetTree")
            .WithSummary("List files in a directory");

        group.MapGet("/{owner}/{name}/blob/{**path}", GetBlob)
            .WithName("GetBlob")
            .WithSummary("Get file content (base64 encoded)");

        return group;
    }

    private static async Task<Results<Ok<PagedResponse<RepositoryResponse>>, UnauthorizedHttpResult>>
        ListRepositories(
            RepositoryService repoService,
            ClaimsPrincipal user,
            int skip = 0,
            int take = 30)
    {
        var userId = GetUserId(user);
        var repos = await repoService.ListAsync(userId, skip, take);

        var response = repos.Select(r => MapToResponse(r));
        return TypedResults.Ok(new PagedResponse<RepositoryResponse>(response, skip, take));
    }

    private static async Task<Results<Created<RepositoryResponse>, BadRequest<ErrorResponse>, UnauthorizedHttpResult>>
        CreateRepository(
            CreateRepositoryRequest request,
            RepositoryService repoService,
            ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return TypedResults.Unauthorized();
        }

        try
        {
            var visibility = Enum.Parse<RepositoryVisibility>(request.Visibility, ignoreCase: true);
            var repo = await repoService.CreateAsync(userId.Value, request.Name, request.Description, visibility);
            return TypedResults.Created($"/repos/{repo.Owner!.Username}/{repo.Name}", MapToResponse(repo));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<Results<Ok<RepositoryResponse>, NotFound, ForbidHttpResult>>
        GetRepository(
            string owner,
            string name,
            RepositoryService repoService,
            ClaimsPrincipal user)
    {
        var repo = await repoService.GetAsync(owner, name);
        if (repo == null)
        {
            return TypedResults.NotFound();
        }

        var userId = GetUserId(user);
        if (!await repoService.HasAccessAsync(repo.Id, userId))
        {
            return TypedResults.Forbid();
        }

        return TypedResults.Ok(MapToResponse(repo));
    }

    private static async Task<Results<Ok<RepositoryResponse>, NotFound, ForbidHttpResult, BadRequest<ErrorResponse>>>
        UpdateRepository(
            string owner,
            string name,
            UpdateRepositoryRequest request,
            RepositoryService repoService,
            ClaimsPrincipal user)
    {
        var repo = await repoService.GetAsync(owner, name);
        if (repo == null)
        {
            return TypedResults.NotFound();
        }

        var userId = GetUserId(user);
        if (!await repoService.HasAccessAsync(repo.Id, userId, CollaboratorPermission.Admin))
        {
            return TypedResults.Forbid();
        }

        try
        {
            RepositoryVisibility? visibility = null;
            if (request.Visibility != null)
            {
                visibility = Enum.Parse<RepositoryVisibility>(request.Visibility, ignoreCase: true);
            }

            var updated = await repoService.UpdateAsync(repo.Id, request.Description, visibility, request.IsArchived);
            return TypedResults.Ok(MapToResponse(updated!));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<Results<NoContent, NotFound, ForbidHttpResult>>
        DeleteRepository(
            string owner,
            string name,
            RepositoryService repoService,
            ClaimsPrincipal user)
    {
        var repo = await repoService.GetAsync(owner, name);
        if (repo == null)
        {
            return TypedResults.NotFound();
        }

        var userId = GetUserId(user);
        // Only owner can delete
        if (repo.OwnerId != userId)
        {
            return TypedResults.Forbid();
        }

        await repoService.DeleteAsync(repo.Id);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<PagedResponse<CommitInfo>>, NotFound, ForbidHttpResult>>
        GetCommits(
            string owner,
            string name,
            RepositoryService repoService,
            GitService gitService,
            ClaimsPrincipal user,
            string? branch = null,
            int skip = 0,
            int take = 30)
    {
        var repo = await repoService.GetAsync(owner, name);
        if (repo == null)
        {
            return TypedResults.NotFound();
        }

        var userId = GetUserId(user);
        if (!await repoService.HasAccessAsync(repo.Id, userId))
        {
            return TypedResults.Forbid();
        }

        var commits = gitService.GetCommits(repo.Path, branch, skip, take);
        return TypedResults.Ok(new PagedResponse<CommitInfo>(commits, skip, take));
    }

    private static async Task<Results<Ok<CommitInfo>, NotFound, ForbidHttpResult>>
        GetCommit(
            string owner,
            string name,
            string sha,
            RepositoryService repoService,
            GitService gitService,
            ClaimsPrincipal user)
    {
        var repo = await repoService.GetAsync(owner, name);
        if (repo == null)
        {
            return TypedResults.NotFound();
        }

        var userId = GetUserId(user);
        if (!await repoService.HasAccessAsync(repo.Id, userId))
        {
            return TypedResults.Forbid();
        }

        var commit = gitService.GetCommit(repo.Path, sha);
        if (commit == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(commit);
    }

    private static async Task<Results<Ok<IEnumerable<BranchInfo>>, NotFound, ForbidHttpResult>>
        GetBranches(
            string owner,
            string name,
            RepositoryService repoService,
            GitService gitService,
            ClaimsPrincipal user)
    {
        var repo = await repoService.GetAsync(owner, name);
        if (repo == null)
        {
            return TypedResults.NotFound();
        }

        var userId = GetUserId(user);
        if (!await repoService.HasAccessAsync(repo.Id, userId))
        {
            return TypedResults.Forbid();
        }

        var branches = gitService.GetBranches(repo.Path);
        return TypedResults.Ok(branches);
    }

    private static async Task<Results<Ok<IEnumerable<TreeEntry>>, NotFound, ForbidHttpResult, BadRequest<ErrorResponse>>>
        GetTree(
            string owner,
            string name,
            string? path,
            RepositoryService repoService,
            GitService gitService,
            ClaimsPrincipal user,
            string? branch = null)
    {
        var repo = await repoService.GetAsync(owner, name);
        if (repo == null)
        {
            return TypedResults.NotFound();
        }

        var userId = GetUserId(user);
        if (!await repoService.HasAccessAsync(repo.Id, userId))
        {
            return TypedResults.Forbid();
        }

        try
        {
            var entries = gitService.ListFiles(repo.Path, path, branch);
            return TypedResults.Ok(entries);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<Results<Ok<FileContent>, NotFound, ForbidHttpResult>>
        GetBlob(
            string owner,
            string name,
            string path,
            RepositoryService repoService,
            GitService gitService,
            ClaimsPrincipal user,
            string? branch = null)
    {
        var repo = await repoService.GetAsync(owner, name);
        if (repo == null)
        {
            return TypedResults.NotFound();
        }

        var userId = GetUserId(user);
        if (!await repoService.HasAccessAsync(repo.Id, userId))
        {
            return TypedResults.Forbid();
        }

        var content = gitService.GetFileContent(repo.Path, path, branch);
        if (content == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(content);
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private static RepositoryResponse MapToResponse(Repository repo) => new(
        repo.Id,
        repo.Name,
        repo.Description,
        repo.Owner?.Username ?? "unknown",
        repo.OwnerId,
        repo.DefaultBranch,
        repo.Visibility.ToString().ToLowerInvariant(),
        repo.IsArchived,
        repo.CreatedAt,
        repo.UpdatedAt
    );
}
