// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Messages.Freeze;

/// <summary>
/// Re-enable a user's account because a freeze is over.
/// </summary>
/// <param name="UserId">The user.</param>
/// <param name="Cancelled">
/// True when a staff member cancelled a freeze that was in effect, false for the one
/// scheduled at the end of a freeze.
/// </param>
public record UnFreezeUserMessage(Guid UserId, bool Cancelled = false);
