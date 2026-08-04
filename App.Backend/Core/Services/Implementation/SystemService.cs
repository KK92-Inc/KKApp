// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Entities.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Keycloak.AuthServices.Sdk.Kiota.Admin;
using Keycloak.AuthServices.Sdk.Kiota.Admin.Models;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

/// <inheritdoc />
public class SystemService(
    DatabaseContext context,
    ILogger<SystemService> log,
    IConfiguration configuration,
    [FromKeyedServices("admin")] KeycloakAdminApiClient keycloak
) : ISystemService
{
    /// <inheritdoc />
    public async Task<Domain.Entities.System?> CheckAsync(CancellationToken token = default)
    {
        return await context.System
            .AsNoTracking()
            .FirstOrDefaultAsync(token);
    }

    /// <inheritdoc />
    public async Task<User> InitializeAsync(string login, string email, CancellationToken token = default)
    {
        if (await context.System.AsNoTracking().AnyAsync(token))
            throw new ServiceException(403, "System is already initialized.");

        var realm = configuration["KeycloakAdmin:realm"] ?? "admin";

        // NOTE(W2): Keycloak ignores any client-supplied "Id" on user creation and
        // always server-generates its own UUID, so we don't send one here.
        // See: https://github.com/keycloak/keycloak/issues/12454
        await keycloak.Admin.Realms[realm].Users.PostAsync(new()
        {
            Username = login,
            Email = email,
            Enabled = true,
            EmailVerified = true,
            Credentials =
            [
                new CredentialRepresentation
                {
                    Type = "password",
                    Value = "admin",
                    Temporary = true,
                }
            ],
            RequiredActions = ["UPDATE_PASSWORD"],
        }, null, token);

        var lookup = await keycloak.Admin.Realms[realm].Users.GetAsync(cfg =>
        {
            cfg.QueryParameters.Username = login;
            cfg.QueryParameters.Exact = true;
        }, token);

        var created = lookup?.SingleOrDefault()
            ?? throw new ServiceException(500, "Failed to bootstrap: could not resolve created user in Keycloak.");

        var id = Guid.Parse(created.Id!);
        try
        {
            var strategy = context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async (ct) =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(ct);
                var account = await context.Users.AddAsync(new()
                {
                    Id = id,
                    Login = login,
                    Display = login,
                    Details = new()
                    {
                        UserId = id,
                        Email = email,
                    },
                }, ct);

                var mine = await context.Workspaces.AddAsync(new()
                {
                    OwnerId = id,
                    Ownership = EntityOwnership.User,
                }, ct);

                var space = await context.Workspaces.AddAsync(new()
                {
                    Ownership = EntityOwnership.Organization,
                }, ct);

                await context.Members.AddRangeAsync([
                    new()
                    {
                        EntityId = space.Entity.Id,
                        EntityType = MemberEntityType.Workspace,
                        Role = MemberRole.Member,
                        UserId = id,
                    },
                    new()
                    {
                        EntityId = mine.Entity.Id,
                        EntityType = MemberEntityType.Workspace,
                        Role = MemberRole.Leader,
                        UserId = id,
                    },
                ], ct);

                await context.System.AddAsync(new(), ct);
                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return account.Entity;
            }, token);
        }
        catch
        {
            try
            {
                await keycloak.Admin.Realms[realm].Users[id.ToString()].DeleteAsync(null, token);
            }
            catch
            {
                log.LogError("Failed to delete initial User, please report this bug...");
            }
            throw new ServiceException(500, "Failed to bootstrap, please report this.");
        }
    }
}