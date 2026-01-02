// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Domain.Rules;

/// <summary>
/// User must be a member of the same team/project group.
/// Required for self-evaluation reviews.
/// </summary>
public sealed class IsMemberRule : Rule
{
    // No additional properties needed - the context provides the team.
}
