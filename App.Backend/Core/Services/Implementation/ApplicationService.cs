// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Net.Http.Json;
using System.Text.Json;
using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Keycloak.AuthServices.Sdk.Kiota.Admin;
using Microsoft.Kiota.Abstractions;
using App.Backend.Core.Query;
using Keycloak.AuthServices.Sdk.Kiota.Admin.Models;

namespace App.Backend.Core.Services.Implementation;

// ============================================================================

public class ApplicationService(DatabaseContext ctx, ILogger<ApplicationService> log, KeycloakAdminApiClient client) : BaseService<Application>(ctx), IApplicationService
{
    private const string Realm = "student";
    private const string StaffRoleName = "staff";
    private readonly DatabaseContext context = ctx;

    public async Task<List<Application>> GetConsentedApplicationsAsync(Guid userId, CancellationToken token = default)
    {
        // 1. Fetch user consents from Keycloak
        var consents = await client.Admin.Realms[Realm]
            .Users[userId.ToString()]
            .Consents
            .GetAsync(null, token);

        if (consents is null || consents.Count == 0)
            return [];

        var consented = consents
            .Select(c =>
            {
                if (c.AdditionalData.TryGetValue("clientId", out var val))
                    return val?.ToString();
                return null;
            })
            .Where(id => !string.IsNullOrEmpty(id))
            .Cast<string>()
            .Distinct()
            .ToList();

        if (consented.Count == 0)
            return [];
        return await _dbSet.AsNoTracking()
            .Where(a => consented.Contains(a.ClientId))
            .ToListAsync(token);
    }

    public override async Task<Application> CreateAsync(Application entity, CancellationToken token = default)
    {
        var workspace = await context.Workspaces.FirstOrDefaultAsync(w => w.Id == entity.WorkspaceId, token)
            ?? throw new ServiceException(400, "Associated workspace not found");

        try
        {
            var realm = client.Admin.Realms[Realm];
            await realm.Clients.PostAsync(new()
            {
                Name = entity.Name,
                Id = entity.Id.ToString(),
                ClientId = entity.ClientId,
                Description = entity.Description,
                Enabled = entity.Enabled,
                Protocol = "openid-connect",
                PublicClient = false,
                StandardFlowEnabled = true,
                ConsentRequired = true,
                ServiceAccountsEnabled = true,
                FullScopeAllowed = false,
                RedirectUris = [.. entity.RedirectUris],
                Attributes = new() { AdditionalData = { { "pkce.code.challenge.method", "S256" } } }
            }, null, token);

            if (workspace.Owner is null)
            {
                // First we get the role and service account
                var role = await realm.Roles[StaffRoleName].GetAsync(null, token);
                var account = await realm.Clients[entity.Id.ToString()].ServiceAccountUser.GetAsync(null, token);

#pragma warning disable CS0618 // Type or member is obsolete
                /**
                    The desired role must be set in both Scope and Service account roles tabs,
                    or allow full scope and then just set the role in Service account roles.

                    However Full scope is dangerous for third party clients as the roles of
                    a privilged users i.e: Staff can by hijacked and then used to make privileged requests.

                    Thus we have to explicitely add the role to both Role and Scope Mappings.
                */
                await realm.Clients[entity.Id.ToString()]
                    .ScopeMappings
                    .Realm.PostAsync([role!], null, token);

                await realm.Users[account!.Id]
                    .RoleMappings
                    .Realm.PostAsync([role!], null, token);
#pragma warning restore CS0618 // There is no alternative.

            }

            await SyncClientScopesAsync(entity.Id, entity.Scopes, token);
        }
        catch (Exception e)
        {
            log.LogError("Failed to create Keycloak client for application: {Error}", e.Message);
            throw new ServiceException(500, "Failed to create associated client in Keycloak");
        }

        var app = await base.CreateAsync(entity, token);
        log.LogInformation("Created application {AppId} in workspace {WorkspaceId}", app.Id, workspace.Id);
        return app;
    }

    public override async Task UpdateAsync(Application entity, CancellationToken token = default)
    {
        var app = await FindByIdAsync(entity.Id, token)
            ?? throw new ServiceException(404, "Application not found");

        var id = entity.Id.ToString();
        var clients = client.Admin.Realms[Realm].Clients[id];
        var kcClient = await clients.GetAsync(cancellationToken: token)
            ?? throw new ServiceException(404, "Keycloak client not found");

        kcClient.Name = entity.Name;
        kcClient.Enabled = entity.Enabled;
        kcClient.Description = entity.Description;
        kcClient.RedirectUris = entity.RedirectUris?.ToList() ?? [];
        await clients.PutAsync(kcClient, cancellationToken: token);
        await SyncClientScopesAsync(entity.Id, entity.Scopes, token);
        await base.UpdateAsync(entity, token);
    }

    public override async Task DeleteAsync(Application entity, CancellationToken token = default)
    {
        var app = await context.Applications.FirstOrDefaultAsync(a => a.Id == entity.Id, token)
            ?? throw new ServiceException(404, "Application not found");

        await client.Admin.Realms[Realm].Clients[entity.Id.ToString()].DeleteAsync(null, token);
        await base.DeleteAsync(entity, token);
    }

    public async Task<string> RotateClientSecretAsync(Guid id, CancellationToken token = default)
    {
        var app = await context.Applications.FirstOrDefaultAsync(a => a.Id == id, token)
            ?? throw new ServiceException(404, "Application not found");

        var rotated = await client.Admin.Realms[Realm].Clients[id.ToString()].ClientSecret.PostAsync(null, token);
        return rotated?.Value ?? throw new ServiceException(500, "No credentials found");
    }

    public async Task RevokeAccess(Guid id, Guid userId, CancellationToken token = default)
    {
       var app = await context.Applications.FirstOrDefaultAsync(a => a.Id == id, token)
            ?? throw new ServiceException(404, "Application not found");

        var realm = client.Admin.Realms[Realm];
        await realm.Users[userId.ToString()].Consents[app.ClientId].DeleteAsync(null, token);
    }

    /// <summary>
    /// Syncs the requested scopes for the client with Keycloak.
    /// Effectively removes those that are not specified anymore.
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="scopes"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task SyncClientScopesAsync(Guid appId, ICollection<string>? scopes, CancellationToken token)
    {
        var id = appId.ToString();
        var realm = client.Admin.Realms[Realm];

        var requested = scopes?.ToHashSet() ?? [];

        // Fetch available realm scopes to validate requests and resolve Names to IDs
        var realmScopes = await realm.ClientScopes.GetAsync(null, token);
        var realmScopeMap = realmScopes?.ToDictionary(s => s.Name!, s => s.Id!) ?? [];

        // Fetch currently assigned optional scopes for this specific client
        var assignedScopes = await realm.Clients[id].OptionalClientScopes.GetAsync(null, token);
        var assignedScopeMap = assignedScopes?.ToDictionary(s => s.Name!, s => s.Id!) ?? [];

        var assigned = assignedScopeMap.Keys.ToHashSet();

        // Scopes that are requested but not currently assigned
        var add = requested.Except(assigned);
        foreach (var scope in add)
        {
            if (!realmScopeMap.TryGetValue(scope, out var scopeId))
            {
                log.LogWarning("Trying to add non-existent scope '{ScopeName}' to client '{ClientId}', Skipping.", scope, id);
                continue;
            }

            await realm.Clients[id].OptionalClientScopes[scopeId].PutAsync(null, token);
        }

        // Scopes that are currently assigned but no longer requested
        var remove = assigned.Except(requested);
        foreach (var scope in remove)
        {
            if (assignedScopeMap.TryGetValue(scope, out var scopeId))
                await realm.Clients[id].OptionalClientScopes[scopeId].DeleteAsync(null, token);
        }
    }
}