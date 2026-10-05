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
/// Switches the account of a user off again once their trial is over,
/// so that between the trial and their kickoff
/// (or for good, if they weren't selected) they can't log in.
///
/// One message per user so a failure for one account is retried on its own.
/// Safe to run more than once, disabling an account that is disabled
/// already does nothing.
/// </summary>
[WolverineHandler]
public class DeactivateTrialParticipantHandler(
    DatabaseContext context,
    [FromKeyedServices("student")] IKeycloakService student,
    ILogger<DeactivateTrialParticipantHandler> log
)
{
    public async Task Handle(DeactivateTrialParticipant message, CancellationToken ct)
    {
        var participation = await context.UserTrials
            .AsNoTracking()
            .FirstOrDefaultAsync(
                ut => ut.TrialId == message.TrialId &&
                ut.UserId == message.UserId,
            ct);

        // Gone, or still taking part (e.g: this message is stale): the trial isn't over for them.
        if (participation is null || participation.State is not (UserTrialState.Completed or UserTrialState.Quit))
            return;

        var role = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == message.UserId)
            .Select(u => (UserRole?)u.Role)
            .FirstOrDefaultAsync(ct);

        // Gone, or promoted to a student by a kickoff in the meantime, whose account has to stay on.
        if (role is not UserRole.Applicant)
            return;

        // Another trial they are taking part in right now needs the account.
        // Protects against messages arriving out of order
        if (await context.UserTrials.AnyAsync(ut => ut.UserId == message.UserId && ut.State == UserTrialState.Active, ct))
            return;

        await student.DisableUserAsync(message.UserId, ct);
        log.LogInformation("Deactivated participant {UserId} of trial {TrialId}", message.UserId, message.TrialId);
    }
}
