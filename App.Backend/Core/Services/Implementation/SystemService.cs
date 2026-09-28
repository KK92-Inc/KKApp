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
using Keycloak.AuthServices.Sdk.Kiota.Admin.Models;
using Microsoft.Extensions.Logging;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

/// <inheritdoc />
public class SystemService(
    DatabaseContext context,
    ILogger<SystemService> log,
    [FromKeyedServices("admin")] IKeycloakService keycloak
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
    public async Task<User> InitializeAsync(string Login, string First, string Last, string Email, CancellationToken token = default)
    {
        if (await context.System.AsNoTracking().AnyAsync(token))
            throw new ServiceException(403, "System is already initialized.");

        // NOTE(W2): Keycloak ignores any client-supplied "Id" on user creation and
        // always server-generates its own UUID, so we don't send one here.
        // See: https://github.com/keycloak/keycloak/issues/12454
        var id = await keycloak.CreateUserAsync(new UserRepresentation
        {
            Username = Login,
            Email = Email,
            Enabled = true,
            EmailVerified = true,
            Credentials =
            [
                new CredentialRepresentation
                {
                    Type = "password",
                    Value = Login,
                    Temporary = true,
                }
            ],
        }, token);

        try
        {
            var strategy = context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async (ct) =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(ct);
                var account = await context.Users.AddAsync(new()
                {
                    Id = id,
                    Login = Login,
                    Display = Login,
                    Email = Email,
                    FirstName = First,
                    LastName = Last,
                }, ct);

                // NOTE(W2): Adds membership automatically
                await context.Workspaces.AddAsync(new()
                {
                    OwnerId = id,
                    Ownership = EntityOwnership.User,
                }, ct);

                // NOTE(W2): Unlike here because root / orgs shouldn't be
                // owned by a single user.
                // TODO: In the future we might want to add EntityOwnership.Root
                // to separate it from organizations.
                var space = await context.Workspaces.AddAsync(new()
                {
                    Ownership = EntityOwnership.Organization,
                }, ct);

                await context.Members.AddAsync(
                    new()
                    {
                        EntityId = space.Entity.Id,
                        EntityType = MemberEntityType.Workspace,
                        Role = MemberRole.Member,
                        UserId = id,
                    }
                , ct);

                await context.System.AddAsync(new(), ct);
                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return account.Entity;
            }, token);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to bootstrap the system. Rolling back Keycloak user {UserId}.", id);
            try
            {
                // CancellationToken.None: the cleanup must still run if the request was cancelled.
                await keycloak.DeleteUserAsync(id, CancellationToken.None);
            }
            catch (Exception kcEx)
            {
                log.LogError(kcEx, "Failed to delete initial User, please report this bug...");
            }

            throw new ServiceException(500, "Failed to bootstrap, please report this.");
        }
    }
}