// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.Linq.Expressions;
using System.Security.Claims;
using App.Backend.API.Bus.Messages.Freeze;
using App.Backend.API.Params;
using App.Backend.Core;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using Keycloak.AuthServices.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Wolverine;

// ============================================================================

namespace App.Backend.API.Mcp;

/// <summary>
/// Staff-only user administration exposed over MCP.
///
/// Design rules:
///  - The /mcp endpoint already requires the "staff" policy (see Program.cs), and the caller is the
///    real staff member behind the bearer token, so every action is attributable.
///  - Tools go through the same services as <see cref="Controllers.UserController"/>, never DbContext,
///    so Keycloak sync, Wolverine messages and interceptors all still happen.
///  - Everything beyond reads is gated by <see cref="McpToolOptions.Mode"/>.
///
/// TODO: Freeze/Unfreeze mirror the controller logic. Extract it into a shared service (or Wolverine
/// handler) so the controller and these tools can't drift apart.
/// </summary>
[McpServerToolType]
public sealed class UserTools(
    IUserService users,
    IMessageBus bus,
    TimeProvider time,
    IAuthorizationService auth,
    IHttpContextAccessor http,
    IOptions<McpToolOptions> options,
    ILogger<UserTools> log)
{
    // Response shapes. Deliberately small, the model doesn't need the full DTOs and enums
    // are rendered as names rather than numbers.
    public sealed record UserSummary(Guid Id, string Login, string? DisplayName, string FirstName, string LastName, string Email, string Role);
    public sealed record FreezeSummary(Guid UserId, string Login, string Reason, DateTimeOffset StartsAt, DateTimeOffset EndsAt);
    public sealed record WhoAmIResult(string? Sub, string? AdminUserId, string? Username, string? Client, string[] Roles, string? Scope, string Mode);
    public sealed record PagedResult<T>(int Total, int Page, int Pages, IReadOnlyCollection<T> Items);

    private static UserSummary Map(User u) =>
        new(u.Id, u.Login, u.Display, u.FirstName, u.LastName, u.Email, u.Role.ToString());

    private static FreezeSummary Map(Freeze f) =>
        new(f.UserId, f.User.Login, f.Reason, f.StartsAt, f.EndsAt);

    // Helpers
    // ============================================================================

    private ClaimsPrincipal Caller =>
        http.HttpContext?.User ?? throw new McpException("No authenticated caller.");

    private void Require(McpMode needed)
    {
        if (options.Value.Mode < needed)
            throw new McpException(
                $"The MCP server is running in '{options.Value.Mode}' mode, this tool needs '{needed}'. " +
                "A human has to raise Mcp:Mode in the backend configuration.");
    }

    /// <summary>Same UMA check the controller's [ProtectedResource] attribute performs.</summary>
    private async Task RequirePermission(string scope)
    {
        var result = await auth.AuthorizeAsync(Caller, null, new DecisionRequirement("users", scope));
        if (!result.Succeeded)
            throw new McpException($"Forbidden: you lack the '{scope}' permission.");
    }

    private void Audit(string tool, object args) =>
        log.LogWarning("MCP audit: caller={Caller} tool={Tool} args={@Args}",
            Caller.FindFirstValue("preferred_username") ?? Caller.FindFirstValue(ClaimTypes.NameIdentifier), tool, args);

    /// <summary>
    /// McpException messages reach the model, other exceptions are masked by the SDK.
    /// Translate ServiceException so the model sees "User not found" instead of "an error occurred".
    /// </summary>
    private static async Task<T> Safe<T>(Func<Task<T>> action)
    {
        try { return await action(); }
        catch (ServiceException e) { throw new McpException(e.Message, e); }
    }

    // Read
    // ============================================================================

    [McpServerTool(Name = "whoami", ReadOnly = true)]
    [Description("Shows who the MCP server thinks you are (token subject, roles, scopes) and the current server mode. Use first when debugging.")]
    public WhoAmIResult WhoAmI() => new(
        Caller.FindFirstValue(ClaimTypes.NameIdentifier),
        Caller.FindFirstValue("admin_user_id"),
        Caller.FindFirstValue("preferred_username"),
        Caller.FindFirstValue("azp"),
        Caller.FindAll(ClaimTypes.Role).Select(c => c.Value).Order().ToArray(),
        Caller.FindFirstValue("scope"),
        options.Value.Mode.ToString());

    [McpServerTool(Name = "list_users", ReadOnly = true)]
    [Description("Search users. login and display are case-insensitive substring filters. Returns at most 50 per page. Always use this to resolve a person to an id, never guess ids.")]
    public Task<PagedResult<UserSummary>> ListUsers(
        [Description("Substring of the login")] string? login = null,
        [Description("Substring of the display name")] string? display = null,
        [Description("0-based page index")] int page = 0,
        [Description("Items per page, 1-50")] int pageSize = 20,
        CancellationToken ct = default) => Safe(async () =>
    {
        await RequirePermission("users:read");

        Expression<Func<User, bool>>? byLogin = string.IsNullOrWhiteSpace(login)
            ? null : u => EF.Functions.ILike(u.Login, $"%{login}%");
        Expression<Func<User, bool>>? byDisplay = string.IsNullOrWhiteSpace(display)
            ? null : u => u.Display != null && EF.Functions.ILike(u.Display, $"%{display}%");

        var result = await users.GetAllAsync(new Sorting(), new Pagination { Page = page, Size = pageSize }, ct, byLogin, byDisplay);
        return new PagedResult<UserSummary>(result.TotalCount, result.Page, result.TotalPages, result.Items.Select(i => Map(i)).ToList());
    });

    [McpServerTool(Name = "get_user", ReadOnly = true)]
    [Description("Get one user by id.")]
    public Task<UserSummary> GetUser([Description("The user's UUID")] Guid userId, CancellationToken ct = default) => Safe(async () =>
    {
        await RequirePermission("users:read");
        var user = await users.FindByIdAsync(userId, ct) ?? throw new McpException("User not found.");
        return Map(user);
    });

    [McpServerTool(Name = "list_frozen_users", ReadOnly = true)]
    [Description("List freeze records (users who are frozen or have a freeze scheduled).")]
    public Task<PagedResult<FreezeSummary>> ListFrozenUsers(
        [Description("0-based page index")] int page = 0,
        [Description("Items per page, 1-50")] int pageSize = 20,
        CancellationToken ct = default) => Safe(async () =>
    {
        var result = await users.GetFrozenUsersAsync(new Sorting(), new Pagination { Page = page, Size = pageSize }, ct);
        return new PagedResult<FreezeSummary>(result.TotalCount, result.Page, result.TotalPages, result.Items.Select(i => Map(i)).ToList());
    });

    [McpServerTool(Name = "get_freeze", ReadOnly = true)]
    [Description("Get the freeze record of one user.")]
    public Task<FreezeSummary> GetFreeze([Description("The user's UUID")] Guid userId, CancellationToken ct = default) => Safe(async () =>
    {
        var freeze = await users.GetFreezeAsync(userId, ct) ?? throw new McpException("This user has no freeze.");
        return Map(freeze);
    });

    // Write (reversible)
    // ============================================================================

    [McpServerTool(Name = "freeze_user", Destructive = false, Idempotent = false)]
    [Description("Freeze a user account for a period. Start and end must be in the future and start must be before end. Resolve the user with list_users first and confirm with the human if the request is ambiguous.")]
    public Task<FreezeSummary> FreezeUser(
        [Description("The user's UUID")] Guid userId,
        [Description("Why the user is frozen (4-2048 chars)")] string reason,
        [Description("ISO 8601 start, e.g. 2026-11-01T00:00:00+01:00")] DateTimeOffset startsAt,
        [Description("ISO 8601 end")] DateTimeOffset endsAt,
        CancellationToken ct = default) => Safe(async () =>
    {
        Require(McpMode.Write);
        Audit("freeze_user", new { userId, reason, startsAt, endsAt });

        if (userId == Caller.GetSID()) throw new McpException("Can't freeze yourself.");
        if (reason.Length is < 4 or > 2048) throw new McpException("Reason must be 4-2048 characters.");

        var now = time.GetUtcNow();
        var start = startsAt.ToUniversalTime();
        var end = endsAt.ToUniversalTime();
        if (start >= end) throw new McpException("The start must be earlier than the end.");
        if (start < now || end < now) throw new McpException("Freeze start and end times cannot be in the past.");

        var user = await users.FindByIdAsync(userId, ct) ?? throw new McpException("User not found.");
        if (await users.GetFreezeAsync(userId, ct) is not null) throw new McpException("User already has a freeze. Unfreeze first.");

        var freeze = await users.FreezeAsync(userId, new()
        {
            Reason = reason,
            StartsAt = start,
            EndsAt = end,
            UserId = user.Id,
        }, ct);

        await bus.ScheduleAsync(new FreezeUserMessage(user.Id), freeze.StartsAt);
        return Map(freeze);
    });

    [McpServerTool(Name = "unfreeze_user", Destructive = false, Idempotent = true)]
    [Description("Remove a user's freeze and re-enable their account.")]
    public Task<string> UnfreezeUser([Description("The user's UUID")] Guid userId, CancellationToken ct = default) => Safe(async () =>
    {
        Require(McpMode.Write);
        Audit("unfreeze_user", new { userId });

        if (userId == Caller.GetSID()) throw new McpException("Can't unfreeze yourself.");
        var user = await users.FindByIdAsync(userId, ct) ?? throw new McpException("User not found.");

        await users.UnFreezeAsync(user.Id, ct);
        await bus.PublishAsync(new UnFreezeUserMessage(user.Id));
        return $"Unfroze {user.Login}.";
    });

    // Destructive (irreversible)
    // ============================================================================

    [McpServerTool(Name = "anonymize_user", Destructive = true, Idempotent = false)]
    [Description("IRREVERSIBLE. Anonymizes a user (GDPR style erasure), deletes their SSH keys and disables their Keycloak account. Look the user up first, show the human who it is, and only call this after they explicitly confirm. confirmLogin must equal the user's current login.")]
    public Task<string> AnonymizeUser(
        [Description("The user's UUID")] Guid userId,
        [Description("The user's exact current login, as a safety check")] string confirmLogin,
        CancellationToken ct = default) => Safe(async () =>
    {
        Require(McpMode.Destructive);
        Audit("anonymize_user", new { userId, confirmLogin });

        if (userId == Caller.GetSID()) throw new McpException("Can't anonymize yourself.");
        var user = await users.FindByIdAsync(userId, ct) ?? throw new McpException("User not found.");
        if (!string.Equals(user.Login, confirmLogin, StringComparison.Ordinal))
            throw new McpException($"confirmLogin '{confirmLogin}' does not match the user's login '{user.Login}'. Aborted, nothing changed.");

        await users.AnonymizeAsync(user.Id, ct);
        return $"Anonymized {confirmLogin} ({userId}).";
    });
}
