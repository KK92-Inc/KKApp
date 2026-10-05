// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Messages.Trials;

/// <summary>
/// The trial is over for this user (it ended or they quit): disable their account again,
/// unless they have become a student in the meantime.
/// </summary>
public record DeactivateTrialParticipant(Guid UserId, Guid TrialId);
