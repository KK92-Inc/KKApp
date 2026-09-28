// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Services.Interface;
using Keycloak.AuthServices.Sdk.Kiota.Admin;
using Keycloak.AuthServices.Sdk.Kiota.Admin.Admin.Realms.Item;
using Keycloak.AuthServices.Sdk.Kiota.Admin.Models;
using Microsoft.Kiota.Abstractions;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class KeycloakService(KeycloakAdminApiClient client, string realm) : IKeycloakService
{
    public string Realm => realm;

    private WithRealmItemRequestBuilder Builder => client.Admin.Realms[realm];

    #region Users

    public async Task<UserRepresentation?> FindUserAsync(Guid id, CancellationToken token = default)
    {
        try
        {
            return await Builder.Users[id.ToString()].GetAsync(null, token);
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == 404)
        {
            return null;
        }
    }

    public async Task<UserRepresentation?> FindUserAsync(string username, CancellationToken token = default)
    {
        var users = await Builder.Users.GetAsync(cfg =>
        {
            cfg.QueryParameters.Username = username;
            cfg.QueryParameters.Exact = true;
        }, token);

        return users?.FirstOrDefault();
    }

    public async Task<Guid> CreateUserAsync(UserRepresentation user, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(user.Username);

        try
        {
            await Builder.Users.PostAsync(user, null, token);
        }
        catch (ApiException ex) when (ex.ResponseStatusCode is 409)
        {
            throw new ServiceException(409, "A user with that username or email already exists.");
        }

        // The create endpoint returns no body, so resolve the generated ID by username.
        // NOTE(W2): The headers technically contain the user url + id ... but getting it is
        // a bit annoying via kiota.
        var created = await FindUserAsync(user.Username, token);
        if (created?.Id is null)
            throw new ServiceException(500, "Failed to create user: could not resolve created user in Keycloak.");

        return Guid.Parse(created.Id);
    }

    public async Task SetUserAsync(Guid id, UserRepresentation user, CancellationToken token = default)
    {
        await Builder.Users[id.ToString()].PutAsync(user, null, token);
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken token = default)
    {
        await Builder.Users[id.ToString()].DeleteAsync(null, token);
    }

    public Task EnableUserAsync(Guid id, CancellationToken token = default) => SetEnabledAsync(id, true, token);

    public Task DisableUserAsync(Guid id, CancellationToken token = default) => SetEnabledAsync(id, false, token);

    private async Task SetEnabledAsync(Guid id, bool enabled, CancellationToken token)
    {
        // Fetch-Then-Put: A partial representation can wipe fields you didn't send.
        var user = await FindUserAsync(id, token);
        ServiceException.ThrowIf(user is null, 404, $"User not found in realm '{realm}'.");

        user.Enabled = enabled;
        await SetUserAsync(id, user, token);
    }

    #endregion

    #region Roles

    public async Task AddRoleAsync(Guid id, string role, CancellationToken token = default)
    {
        var representation = await GetRoleAsync(role, token);
        await Builder.Users[id.ToString()].RoleMappings.Realm.PostAsync([representation], null, token);
    }

    public async Task RemoveRoleAsync(Guid id, string role, CancellationToken token = default)
    {
        var representation = await GetRoleAsync(role, token);
        await Builder.Users[id.ToString()].RoleMappings.Realm.DeleteAsync([representation], null, token);
    }

    private async Task<RoleRepresentation> GetRoleAsync(string role, CancellationToken token)
    {
        try
        {
            // Role mappings need the full role representation (id + name), not just the name.
            return await Builder.Roles[role].GetAsync(null, token)
                ?? throw new ServiceException(404, $"Role '{role}' does not exist in realm '{realm}'.");
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == 404)
        {
            throw new ServiceException(404, $"Role '{role}' does not exist in realm '{realm}'.");
        }
    }

    #endregion
}