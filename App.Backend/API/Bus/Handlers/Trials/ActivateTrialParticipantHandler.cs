// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;
using App.Backend.API.Bus.Messages.Trials;
using App.Backend.Core.Services.Interface;
using App.Backend.Database;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.API.Bus.Handlers.Trials;

/// <summary>
/// Switches a single registered user over to active in a running trial: enables their account
/// (applicants are created disabled), subscribes them to the trial's cursus, and marks them active.
///
/// One message per user so a failure for one account (e.g: Keycloak hiccup) is retried
/// on its own without holding up the rest of the cohort.
///
/// Safe to run more than once. Keycloak and the subscription go first and can be repeated, and the
/// state is only flipped at the very end, so if anything fails the retry just runs through them again.
/// </summary>
[WolverineHandler]
public class ActivateTrialParticipantHandler(
    DatabaseContext context,
    ISubscriptionService subscriptions,
    TimeProvider time,
    [FromKeyedServices("student")] IKeycloakService student,
    ILogger<ActivateTrialParticipantHandler> log
)
{
    public async Task Handle(ActivateTrialParticipant message, CancellationToken ct)
    {
        var participation = await context.UserTrials
            .Include(ut => ut.Trial)
            .Include(ut => ut.User)
            .FirstOrDefaultAsync(
                ut => ut.TrialId == message.TrialId &&
                ut.UserId == message.UserId,
            ct);

        // Gone, left, already active, or the trial is over: nothing left to do.
        if (participation is null || participation.State is not UserTrialState.Registered)
            return;
        if (participation.Trial.EndedAt is not null)
            return;

        // Only applicants get their account switched on and off by a trial. Anyone else
        // already has one that works and isn't ours to touch.
        if (participation.User.Role is not UserRole.Applicant)
            return;

        await student.EnableUserAsync(participation.UserId, ct);

        var cursusId = participation.Trial.CursusId;
        var subscription = await context.UserCursi.FirstOrDefaultAsync(
            uc => uc.UserId == participation.UserId &&
            uc.CursusId == cursusId,
        ct);

        // Subscribing refuses someone that is subscribed already, which a retry would be.
        if (subscription is null || subscription.State is EntityObjectState.Inactive)
            await subscriptions.SubscribeToCursusAsync(participation.UserId, cursusId, ct);

        var now = time.GetUtcNow();
        var affected = await context.UserTrials
            .Where(ut => ut.Id == participation.Id && ut.State == UserTrialState.Registered)
            .ExecuteUpdateAsync(s => s
                .SetProperty(ut => ut.State, UserTrialState.Active)
                .SetProperty(ut => ut.StartedAt, (DateTimeOffset?)now)
                .SetProperty(ut => ut.UpdatedAt, now), ct);

        // They left (or the trial ended) while we were busy, don't resurrect them.
        if (affected == 0)
        {
            log.LogWarning("Participant {UserId} of trial {TrialId} changed while being activated", message.UserId, message.TrialId);
            return;
        }

        log.LogInformation("Activated participant {UserId} of trial {TrialId}", message.UserId, message.TrialId);
    }
}
