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
/// Collaborator API endpoints.
/// </summary>
public static class CollaboratorEndpoints
{
    public static RouteGroupBuilder MapCollaboratorEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/{owner}/{name}/collaborators", ListCollaborators)
            .WithName("ListCollaborators")
            .WithSummary("List all collaborators for a repository");

        group.MapPut("/{owner}/{name}/collaborators", AddCollaborator)
            .WithName("AddCollaborator")
            .WithSummary("Add or update a collaborator");

        group.MapDelete("/{owner}/{name}/collaborators/{username}", RemoveCollaborator)
            .WithName("RemoveCollaborator")
            .WithSummary("Remove a collaborator");

        return group;
    }

    private static async Task<Results<Ok<IEnumerable<CollaboratorResponse>>, NotFound, ForbidHttpResult>>
        ListCollaborators(
            string owner,
            string name,
            RepositoryService repoService,
            CollaboratorService collabService,
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

        var collabs = await collabService.ListAsync(repo.Id);
        var response = collabs.Select(c => MapToResponse(c));
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<CollaboratorResponse>, NotFound, ForbidHttpResult, BadRequest<ErrorResponse>>>
        AddCollaborator(
            string owner,
            string name,
            AddCollaboratorRequest request,
            RepositoryService repoService,
            CollaboratorService collabService,
            UserService userService,
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

        var targetUser = await userService.GetByUsernameAsync(request.Username);
        if (targetUser == null)
        {
            return TypedResults.BadRequest(new ErrorResponse($"User '{request.Username}' not found"));
        }

        try
        {
            var permission = Enum.Parse<CollaboratorPermission>(request.Permission, ignoreCase: true);
            var collab = await collabService.AddAsync(repo.Id, targetUser.Id, permission);

            // Reload to get user info
            var collabs = await collabService.ListAsync(repo.Id);
            var result = collabs.First(c => c.Id == collab.Id);
            return TypedResults.Ok(MapToResponse(result));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<Results<NoContent, NotFound, ForbidHttpResult, BadRequest<ErrorResponse>>>
        RemoveCollaborator(
            string owner,
            string name,
            string username,
            RepositoryService repoService,
            CollaboratorService collabService,
            UserService userService,
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

        var targetUser = await userService.GetByUsernameAsync(username);
        if (targetUser == null)
        {
            return TypedResults.BadRequest(new ErrorResponse($"User '{username}' not found"));
        }

        var removed = await collabService.RemoveAsync(repo.Id, targetUser.Id);
        return removed ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private static CollaboratorResponse MapToResponse(Collaborator collab) => new(
        collab.Id,
        collab.UserId,
        collab.User?.Username ?? "unknown",
        collab.Permission.ToString().ToLowerInvariant(),
        collab.CreatedAt
    );
}
