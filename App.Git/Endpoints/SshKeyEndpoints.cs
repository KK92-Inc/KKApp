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
/// SSH Key API endpoints.
/// </summary>
public static class SshKeyEndpoints
{
    public static RouteGroupBuilder MapSshKeyEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListKeys)
            .WithName("ListSshKeys")
            .WithSummary("List all SSH keys for the current user");

        group.MapPost("/", AddKey)
            .WithName("AddSshKey")
            .WithSummary("Add a new SSH key");

        group.MapDelete("/{id:guid}", DeleteKey)
            .WithName("DeleteSshKey")
            .WithSummary("Delete an SSH key");

        return group;
    }

    private static async Task<Results<Ok<IEnumerable<SshKeyResponse>>, UnauthorizedHttpResult>>
        ListKeys(
            SshKeyService sshKeyService,
            ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return TypedResults.Unauthorized();
        }

        var keys = await sshKeyService.GetKeysAsync(userId.Value);
        var response = keys.Select(k => MapToResponse(k));
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Created<SshKeyResponse>, BadRequest<ErrorResponse>, UnauthorizedHttpResult>>
        AddKey(
            AddSshKeyRequest request,
            SshKeyService sshKeyService,
            ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return TypedResults.Unauthorized();
        }

        try
        {
            var key = await sshKeyService.AddKeyAsync(userId.Value, request.Title, request.PublicKey);
            return TypedResults.Created($"/user/keys/{key.Id}", MapToResponse(key));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<Results<NoContent, NotFound, UnauthorizedHttpResult>>
        DeleteKey(
            Guid id,
            SshKeyService sshKeyService,
            ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return TypedResults.Unauthorized();
        }

        var deleted = await sshKeyService.RemoveKeyAsync(userId.Value, id);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private static SshKeyResponse MapToResponse(SshKey key) => new(
        key.Id,
        key.Title,
        key.Fingerprint,
        key.KeyType,
        key.LastUsedAt,
        key.CreatedAt
    );
}
