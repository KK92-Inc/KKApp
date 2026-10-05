// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Wolverine.Attributes;
using App.Backend.API.Bus.Messages.Trials;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.API.Bus.Handlers.Trials;

/// <summary>
/// Handles an ending trial.
///
/// Marks the trial as ended, which also wraps up its participants, and fans out one
/// deactivation per participant that made it to the end. The ones that quit earlier had their
/// account switched off back then. Safe to run more than once, deactivating is idempotent.
/// </summary>
[WolverineHandler]
public class EndTrialHandler(ITrialService trials, ILogger<EndTrialHandler> log)
{
    public async Task<IEnumerable<object>> Handle(EndTrial message, CancellationToken ct)
    {
        // Missing, never started, or moved to a later date after the job picked it up.
        if (!await trials.StopTrialAsync(message.TrialId, ct))
            return [];

        var completed = await trials.GetUserIdsAsync(message.TrialId, UserTrialState.Completed, ct);
        log.LogInformation("Trial {TrialId} ended, deactivating {Count} participant(s)", message.TrialId, completed.Count);

        return completed.Select(id => new DeactivateTrialParticipant(id, message.TrialId));
    }
}
