// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Messages.Trials;

/// <summary>
/// A running trial is due to end. Sent by the trial job, safe to receive more than once.
/// </summary>
public record EndTrial(Guid TrialId);
