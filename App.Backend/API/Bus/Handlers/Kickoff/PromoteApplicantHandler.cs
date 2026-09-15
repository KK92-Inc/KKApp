// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Handlers;

using Wolverine.Attributes;
using App.Backend.Database;
using Keycloak.AuthServices.Sdk.Kiota.Admin;
using App.Backend.API.Bus.Messages.Kickoff;
using Microsoft.EntityFrameworkCore;

// ============================================================================

[WolverineHandler]
public class PromoteApplicantHandler(
    DatabaseContext context,
    TimeProvider time,
    IConfiguration configuration,
    [FromKeyedServices("student")] KeycloakAdminApiClient client
)
{
    private const string RoleTo = "student";
    private const string RoleFrom = "applicant";

    public async Task Handle(PromoteApplicant message, CancellationToken ct)
    {
        var realmName = configuration["KeycloakStudent:realm"] ?? "student";
        var link = await context.UserKickoff
            .Include(uk => uk.Kickoff)
            .FirstOrDefaultAsync(
                uk => uk.UserId == message.UserId &&
                uk.KickoffId == message.KickoffId,
            ct);

        // This user has no kickoff or is already processed.
        if (link is null || link.ProcessedAt is not null)
            return;

        var realm = client.Admin.Realms[realmName];
        var userId = link.UserId.ToString();

        var kcUser = await realm.Users[userId].GetAsync(cancellationToken: ct)
            ?? throw new InvalidOperationException($"Keycloak user {link.UserId} not found");

        if (kcUser.Enabled is not true)
        {
            kcUser.Enabled = true;
            await realm.Users[userId].PutAsync(kcUser, cancellationToken: ct);
        }

        // Assign to student role
        var targetRole = await realm.Roles[RoleTo].GetAsync(cancellationToken: ct)
            ?? throw new InvalidOperationException($"Role '{RoleTo}' not found");
        await realm.Users[userId].RoleMappings.Realm.PostAsync([targetRole], cancellationToken: ct);

        // Remove the applicant role.
        var role = await realm.Roles[RoleFrom].GetAsync(cancellationToken: ct);
        if (role is not null)
            await realm.Users[userId].RoleMappings.Realm.DeleteAsync([role], cancellationToken: ct);

        link.ProcessedAt = time.GetUtcNow();
        await context.SaveChangesAsync(ct);
    }
}