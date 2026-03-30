// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;
using App.Backend.Core.Engines.Evaluations.Enums;

// ============================================================================

namespace App.Backend.Core.Engines.Evaluations;

/// <summary>
/// All state an evaluator might need. Build one per evaluation call — never reuse.
/// </summary>
public record Context
{
    /// <summary>
    /// The user being evaluated.
    /// </summary>
    public required User User { get; init; }

    /// <summary>Whether this user is acting as a reviewer or a reviewee.</summary>
    public required Role Role { get; init; }

    /// <summary>
    /// The project under review (owned by the reviewee).
    /// Present in both reviewer and reviewee checks so rules
    /// can inspect the project regardless of role.
    /// </summary>
    public UserProject? SubjectProject { get; init; }

    /// <summary>
    /// Injectable for deterministic testing of time-based rules.
    /// </summary>
    public DateTimeOffset Now { get; init; } = DateTimeOffset.UtcNow;
}