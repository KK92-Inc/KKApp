// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;
using App.Backend.API.Bus.Messages.Kickoff;
using App.Backend.Core.Services.Interface;
using App.Backend.Database;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.API.Bus.Handlers.Kickoff;

/// <summary>
/// Promotes a single applicant of a started kickoff to a student: enables their account, swaps the
/// applicant realm role for the student one, and updates the role of the database user.
///
/// One message per user so a failure for one account (e.g: Keycloak hiccup) is retried
/// on its own without holding up the rest of the cohort.
///
/// Safe to run more than once. Keycloak goes first and each of its calls can be repeated, so if saving
/// fails the retry just runs through them again. The other way around could leave a student who can't log in.
/// </summary>
[WolverineHandler]
public class PromoteApplicantHandler(
    DatabaseContext context,
    [FromKeyedServices("student")] IKeycloakService student,
    ILogger<PromoteApplicantHandler> log)
{
    public async Task Handle(PromoteApplicant message, CancellationToken ct)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == message.UserId, ct);

        // Gone, removed from the kickoff after this was queued, or already promoted: nothing left to do.
        if (user is null || user.KickoffId != message.KickoffId || user.Role is not UserRole.Applicant)
            return;

        await student.EnableUserAsync(user.Id, ct);
        await student.AddRoleAsync(user.Id, "student", ct);
        await student.RemoveRoleAsync(user.Id, "applicant", ct);

        user.Role = UserRole.Student;
        await context.SaveChangesAsync(ct);

        log.LogInformation("Promoted applicant {UserId} to student (kickoff {KickoffId})", user.Id, message.KickoffId);
    }
}
