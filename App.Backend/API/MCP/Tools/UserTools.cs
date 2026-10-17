// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.Linq.Expressions;
using System.Security.Claims;
using App.Backend.API.Bus.Messages.Freeze;
using App.Backend.API.MCP.Results;
using App.Backend.API.Params;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Wolverine;

// ============================================================================

namespace App.Backend.API.MCP.Tools;

/// <summary>
/// Staff-only user administration.
///
/// Design rules:
///  - Exposed twice: over MCP (/mcp, staff policy) and to the built-in agent (/agent/chat).
///    Both run as the real staff member behind the bearer token, so every action is attributable.
///  - Tools go through the same services as <see cref="Controllers.UserController"/>, never DbContext,
///    so Keycloak sync, Wolverine messages and interceptors all still happen.
///  - Everything beyond reads is gated by <see cref="McpToolOptions.Mode"/>.
///  - Mark every tool that doesn't change data with ReadOnly = true. The built-in agent uses that flag
///    to decide which calls need a human's approval first.
///
/// TODO: Freeze/Unfreeze mirror the controller logic. Extract it into a shared service (or Wolverine
/// handler) so the controller and these tools can't drift apart.
/// </summary>
[McpServerToolType]
public sealed class UserTools(
    IUserService users,
    IMessageBus bus,
    TimeProvider time,
    IHttpContextAccessor http,
    IOptions<McpToolOptions> options,
    IAuthorizationService auth,
    ILogger<UserTools> log) : MCPTool(http, options, auth, log)
{
    private static UserSummary Map(User u) =>
        new(u.Id, u.Login, u.Display, u.FirstName, u.LastName, u.Email, u.Role.ToString());

    private static FreezeSummary Map(Freeze f) =>
        new(f.UserId, f.User.Login, f.Reason, f.StartsAt, f.EndsAt);

    // Read
    // ============================================================================

    [McpServerTool(Name = "whoami", ReadOnly = true)]
    [Description("Shows who the caller is (token subject, username, roles, scopes) and the current server mode.")]
    public WhoAmIResult WhoAmI() => new(
        Caller.FindFirstValue(ClaimTypes.NameIdentifier),
        Caller.FindFirstValue("admin_user_id"),
        Caller.FindFirstValue("preferred_username"),
        Caller.FindFirstValue("azp"),
        Caller.FindAll(ClaimTypes.Role).Select(c => c.Value).Order().ToArray(),
        Caller.FindFirstValue("scope"),
        Options.Mode.ToString());

    [McpServerTool(Name = "list_users", ReadOnly = true)]
    [Description("Search users. login and display are case-insensitive substring filters. Returns at most 50 per page. Always use this to resolve a person to an id, never guess ids.")]
    public Task<PagedResult<UserSummary>> ListUsers(
        [Description("Substring of the login")] string? login = null,
        [Description("Substring of the display name")] string? display = null,
        [Description("0-based page index")] int page = 0,
        [Description("Items per page, 1-50")] int pageSize = 20,
        CancellationToken ct = default) => Safe(async () =>
    {
        await RequirePermission("users", "users:read");

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
        await RequirePermission("users", "users:read");
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
    [Description("Freeze a user account for a period. Start and end must be in the future and start must be before end. Resolve the user with list_users first.")]
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
    [Description("Cancel a user's freeze, in effect or upcoming, and re-enable their account if it was locked.")]
    public Task<string> UnfreezeUser([Description("The user's UUID")] Guid userId, CancellationToken ct = default) => Safe(async () =>
    {
        Require(McpMode.Write);
        Audit("unfreeze_user", new { userId });

        if (userId == Caller.GetSID()) throw new McpException("Can't unfreeze yourself.");
        var user = await users.FindByIdAsync(userId, ct) ?? throw new McpException("User not found.");

        var freeze = await users.UnFreezeAsync(user.Id, ct);
        if (freeze.StartsAt > time.GetUtcNow()) return $"Cancelled the upcoming freeze of {user.Login}.";

        await bus.PublishAsync(new UnFreezeUserMessage(user.Id, Cancelled: true));
        return $"Unfroze {user.Login}.";
    });

    // Destructive (irreversible)
    // ============================================================================

    [McpServerTool(Name = "anonymize_user", Destructive = true, Idempotent = false)]
    [Description("IRREVERSIBLE. Anonymizes a user (GDPR style erasure), deletes their SSH keys and disables their Keycloak account. confirmLogin must equal the user's current login.")]
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
