// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.MCP.Results;

// ============================================================================

/// <summary>Slim user. Enums are rendered as names rather than numbers.</summary>
public sealed record UserSummary(
    Guid Id,
    string Login,
    string? DisplayName,
    string FirstName,
    string LastName,
    string Email,
    string Role);

public sealed record FreezeSummary(
    Guid UserId,
    string Login,
    string Reason,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);

/// <summary>Who the server thinks the caller is, plus the current tool mode.</summary>
public sealed record WhoAmIResult(
    string? Sub,
    string? AdminUserId,
    string? Username,
    string? Client,
    string[] Roles,
    string? Scope,
    string Mode);
