// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Enums;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class ApplicationService(DatabaseContext ctx, IHttpClientFactory factory, ILogger<ApplicationService> log) : BaseService<Application>(ctx), IApplicationService
{
    private readonly DatabaseContext context = ctx;

    private readonly HttpClient keycloak = factory.CreateClient("kc_admin");

    public override async Task<Application> CreateAsync(Application entity, CancellationToken token = default)
    {
        // Ensure the application is associated with a workspace
        var workspace = await context.Workspaces.FirstOrDefaultAsync(w => w.Id == entity.WorkspaceId, token)
            ?? throw new ServiceException(400, "Associated workspace not found");

        // Add the application to the database
        var app = await base.CreateAsync(entity, token);
        var response = await keycloak.PostAsJsonAsync("clients", new
        {
            name = app.Name,
            clientId = app.ClientId,
            description = app.Description,
            enabled = true,
            protocol = "openid-connect",
            publicClient = false,
            standardFlowEnabled = true,
            consentRequired = true,
            serviceAccountsEnabled = true,
            redirectUris = app.RedirectUris ?? [],
            attributes = new Dictionary<string, string> {
                { "pkce.code.challenge.method", "S256" }
            }
        }, token);

        if (!response.IsSuccessStatusCode)
        {
            // Rollback the application creation if Keycloak client creation fails
            await DeleteAsync(app, token);
            var errorContent = await response.Content.ReadAsStringAsync(token);
            log.LogError("Failed to create Keycloak client for application {AppId}: {Error}", app.Id, errorContent);
            throw new ServiceException(500, "Failed to create associated client in Keycloak");
        }

        log.LogInformation("Created application {AppId} in workspace {WorkspaceId}", app.Id, workspace.Id);
        return app;
    }

    public override async Task UpdateAsync(Application entity, CancellationToken token = default)
    {
        // Ensure the application exists
        var existingApp = await context.Applications.FirstOrDefaultAsync(a => a.Id == entity.Id, token)
            ?? throw new ServiceException(404, "Application not found");

        // Update the application in the database
        await base.UpdateAsync(entity, token);

        // Update the corresponding Keycloak client
        var response = await keycloak.PutAsJsonAsync($"clients/{existingApp.ClientId}", new
        {
            name = entity.Name,
            description = entity.Description,
            enabled = true,
            protocol = "openid-connect",
            publicClient = false,
            standardFlowEnabled = true,
            consentRequired = true,
            serviceAccountsEnabled = true,
            redirectUris = entity.RedirectUris ?? [],
            attributes = new Dictionary<string, string> {
                { "pkce.code.challenge.method", "S256" }
            }
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
        // Ensure the application exists
        var existingApp = await context.Applications.FirstOrDefaultAsync(a => a.Id == entity.Id, token)
            ?? throw new ServiceException(404, "Application not found");

        // Delete the corresponding Keycloak client
        var response = await keycloak.DeleteAsync($"clients/{existingApp.ClientId}", token);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(token);
            log.LogError("Failed to delete Keycloak client for application {AppId}: {Error}", entity.Id, errorContent);
            throw new ServiceException(500, "Failed to delete associated client in Keycloak");
        }

        // Delete the application from the database
        await base.DeleteAsync(entity, token);
        log.LogInformation("Deleted application {AppId} from workspace {WorkspaceId}", entity.Id, entity.WorkspaceId);
    }
}
