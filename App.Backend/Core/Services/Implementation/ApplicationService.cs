// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace App.Backend.Core.Services.Implementation;

public class ApplicationService(DatabaseContext ctx, IHttpClientFactory factory, ILogger<ApplicationService> log) : BaseService<Application>(ctx), IApplicationService
{
    private readonly DatabaseContext context = ctx;
    private readonly HttpClient keycloak = factory.CreateClient("kc_admin");

    public override async Task<Application> CreateAsync(Application entity, CancellationToken token = default)
    {
        var workspace = await context.Workspaces.FirstOrDefaultAsync(w => w.Id == entity.WorkspaceId, token)
            ?? throw new ServiceException(400, "Associated workspace not found");

        var app = await base.CreateAsync(entity, token);
        var response = await keycloak.PostAsJsonAsync("clients", new
        {
            name = app.Name,
            clientId = app.ClientId,
            description = app.Description,
            enabled = app.Enabled,
            protocol = "openid-connect",
            publicClient = false,
            standardFlowEnabled = true,
            consentRequired = true,
            serviceAccountsEnabled = true,
            redirectUris = app.RedirectUris ?? [],
            attributes = new Dictionary<string, string> {
                { "pkce.code.challenge.method", "S256" }
            },
        }, token);

        if (!response.IsSuccessStatusCode)
        {
            await _dbSet.Where(a => a.Id == app.Id).ExecuteDeleteAsync(token);
            var errorContent = await response.Content.ReadAsStringAsync(token);
            log.LogError("Failed to create Keycloak client for application {AppId}: {Error}", app.Id, errorContent);
            throw new ServiceException(500, "Failed to create associated client in Keycloak");
        }

        // Keycloak returns the internal client-uuid inside the Location header!
        var locationHeader = response.Headers.Location?.ToString();
        if (!string.IsNullOrEmpty(locationHeader))
        {
            var internalId = locationHeader.Split('/').Last();
            app.KeycloakId = Guid.Parse(internalId);
            await context.SaveChangesAsync(token); // Persist internal Keycloak ID
        }

        log.LogInformation("Created application {AppId} in workspace {WorkspaceId}", app.Id, workspace.Id);
        return app;
    }

    public override async Task UpdateAsync(Application entity, CancellationToken token = default)
    {
        var existingApp = await context.Applications.FirstOrDefaultAsync(a => a.Id == entity.Id, token)
            ?? throw new ServiceException(404, "Application not found");

        await base.UpdateAsync(entity, token);

        // Fixed: Route now points to app.KeycloakId (internal UUID) instead of app.ClientId
        var response = await keycloak.PutAsJsonAsync($"clients/{existingApp.KeycloakId}", new
        {
            name = entity.Name,
            enabled = entity.Enabled,
            description = entity.Description,
            redirectUris = entity.RedirectUris ?? [],
        }, token);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(token);
            log.LogError("Failed to update Keycloak client for application {AppId}: {Error}", entity.Id, errorContent);
            throw new ServiceException(500, "Failed to update associated client in Keycloak");
        }

        log.LogInformation("Updated application {AppId} in workspace {WorkspaceId}", entity.Id, entity.WorkspaceId);
    }

    public override async Task DeleteAsync(Application entity, CancellationToken token = default)
    {
        var app = await context.Applications.FirstOrDefaultAsync(a => a.Id == entity.Id, token)
            ?? throw new ServiceException(404, "Application not found");

        var response = await keycloak.DeleteAsync($"clients/{app.KeycloakId}", token);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(token);
            log.LogError("Failed to delete Keycloak client for application {AppId}: {Error}", entity.Id, errorContent);
            throw new ServiceException(500, "Failed to delete associated client in Keycloak");
        }

        await base.DeleteAsync(entity, token);
        log.LogInformation("Deleted application {AppId} from workspace {WorkspaceId}", entity.Id, entity.WorkspaceId);
    }

    /// <summary>
    /// Step 1: Demotes current secret to 'rotated' slot and generates/returns a brand new primary secret.
    /// </summary>
    public async Task<string?> RotateClientSecretAsync(Guid id, CancellationToken token = default)
    {
        var app = await context.Applications.FirstOrDefaultAsync(a => a.Id == id, token)
            ?? throw new ServiceException(404, "Application not found");

        // Triggers Keycloak to push current secret to fallback and regenerate primary
        var response = await keycloak.PostAsync($"clients/{app.KeycloakId}/client-secret", null, token);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(token);
            log.LogError("Failed to rotate secret for application {AppId}: {Error}", id, errorContent);
            throw new ServiceException(500, "Failed to rotate client secret in Keycloak");
        }

        // Map Keycloak's CredentialRepresentation back to read the plaintext new secret value
        var credential = await response.Content.ReadFromJsonAsync<CredentialRepresentation>(cancellationToken: token);
        log.LogInformation("Successfully initiated secret rotation for application {AppId}", id);
        return credential?.Value;
    }
}

/// <summary>
/// Helper DTO matching Keycloak's internal CredentialRepresentation scheme
/// </summary>
public class CredentialRepresentation
{
    public string? Type { get; set; }
    public string? Value { get; set; }
}