// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Messages.Trials;

/// <summary>
/// Switch a registered user over to active in a running trial: enable their account and
/// subscribe them to the trial's cursus.
/// </summary>
public record ActivateTrialParticipant(Guid UserId, Guid TrialId);
