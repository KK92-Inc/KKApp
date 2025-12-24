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
/// User API endpoints.
/// </summary>
public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Get the current authenticated user");

        group.MapGet("/{username}", GetUserByUsername)
            .WithName("GetUserByUsername")
            .WithSummary("Get a user by username")
            .AllowAnonymous();

        group.MapGet("/{username}/repos", ListUserRepos)
            .WithName("ListUserRepos")
            .WithSummary("List repositories owned by a user")
            .AllowAnonymous();

        return group;
    }

    private static async Task<Results<Ok<UserResponse>, UnauthorizedHttpResult>>
        GetCurrentUser(
            UserService userService,
            ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return TypedResults.Unauthorized();
        }

        var dbUser = await userService.GetByIdAsync(userId.Value);
        if (dbUser == null)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(MapToResponse(dbUser));
    }

    private static async Task<Results<Ok<UserResponse>, NotFound>>
        GetUserByUsername(
            string username,
            UserService userService)
    {
        var dbUser = await userService.GetByUsernameAsync(username);
        if (dbUser == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(MapToResponse(dbUser));
    }

    private static async Task<Results<Ok<PagedResponse<RepositoryResponse>>, NotFound>>
        ListUserRepos(
            string username,
            UserService userService,
            RepositoryService repoService,
            ClaimsPrincipal user,
            int skip = 0,
            int take = 30)
    {
        var dbUser = await userService.GetByUsernameAsync(username);
        if (dbUser == null)
        {
            return TypedResults.NotFound();
        }

        var repos = await repoService.ListByOwnerAsync(dbUser.Id, skip, take);

        // Filter based on visibility for non-owners
        var currentUserId = GetUserId(user);
        var visibleRepos = repos.Where(r =>
            r.OwnerId == currentUserId ||
            r.Visibility == RepositoryVisibility.Public ||
            (currentUserId.HasValue && r.Visibility == RepositoryVisibility.Internal)
        );

        var response = visibleRepos.Select(r => new RepositoryResponse(
            r.Id,
            r.Name,
            r.Description,
            r.Owner?.Username ?? "unknown",
            r.OwnerId,
            r.DefaultBranch,
            r.Visibility.ToString().ToLowerInvariant(),
            r.IsArchived,
            r.CreatedAt,
            r.UpdatedAt
        ));

        return TypedResults.Ok(new PagedResponse<RepositoryResponse>(response, skip, take));
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private static UserResponse MapToResponse(User user) => new(
        user.Id,
        user.Username,
        user.Email,
        user.IsAdmin,
        user.CreatedAt
    );
}
