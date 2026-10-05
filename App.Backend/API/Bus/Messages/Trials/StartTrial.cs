// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Messages.Trials;

/// <summary>
/// A trial is due to begin. Sent by the trial job, safe to receive more than once.
/// </summary>
public record StartTrial(Guid TrialId);
