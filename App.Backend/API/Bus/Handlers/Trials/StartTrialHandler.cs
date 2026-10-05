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
/// Handles a starting trial.
///
/// All it does is mark the trial as started and fan out one activation per registered
/// participant, the actual work happens per user in <see cref="ActivateTrialParticipantHandler"/>.
/// Safe to run more than once: users that are active already are no longer registered, so
/// repeating this only queues the ones that still have to be activated.
/// </summary>
[WolverineHandler]
public class StartTrialHandler(ITrialService trials, ILogger<StartTrialHandler> log)
{
    public async Task<IEnumerable<object>> Handle(StartTrial message, CancellationToken ct)
    {
        // Missing, or moved to a later date after the job picked it up.
        if (!await trials.StartTrialAsync(message.TrialId, ct))
            return [];

        var registered = await trials.GetUserIdsAsync(message.TrialId, UserTrialState.Registered, ct);
        log.LogInformation("Trial {TrialId} started, activating {Count} participant(s)", message.TrialId, registered.Count);

        return registered.Select(id => new ActivateTrialParticipant(id, message.TrialId));
    }
}
