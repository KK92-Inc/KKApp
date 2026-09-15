// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Security.Cryptography;
using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using Keycloak.AuthServices.Sdk.Kiota.Admin;
using Keycloak.AuthServices.Sdk.Kiota.Admin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using App.Backend.Domain.Entities;
using App.Backend.Core.Query;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class UserService(
    DatabaseContext ctx,
    ILogger<UserService> log,
    TimeProvider time,
    IConfiguration configuration,
    [FromKeyedServices("student")] KeycloakAdminApiClient keycloak
) : BaseService<User>(ctx), IUserService
{
    private readonly DatabaseContext context = ctx;

    private readonly string realm = configuration["KeycloakStudent:realm"] ?? "student";

    public override async Task<User?> FindByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _dbSet.Include(u => u.Details).FirstOrDefaultAsync(u => u.Id == id, token);
    }

    public override async Task UpdateAsync(User entity, CancellationToken token = default)
    {
        if (entity.Details is not null && context.Entry(entity.Details).State is EntityState.Detached)
            context.Add(entity.Details);

        await base.UpdateAsync(entity, token);
    }

    public async Task<User?> FindByLoginAsync(string login, CancellationToken token = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Login == login, cancellationToken: token);
    }

    public async Task<User?> FindByNameAsync(string displayName, CancellationToken token = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Display == displayName, cancellationToken: token);
    }

    [Obsolete("Use CreateUserAsync instead as it propegates to keycloak and returns a password")]
    public override Task<User> CreateAsync(User entity, CancellationToken token = default)
    {
        return base.CreateAsync(entity, token);
    }

    /// <summary>
    /// Provision a new user in Keycloak and persist their database account & personal workspace.
    /// Returns the created <see cref="User"/> along with their generated temporary password.
    /// </summary>
    public async Task<(User User, string TempPassword)> CreateUserAsync(User user, CancellationToken token = default)
    {
        var temp = Guid.CreateVersion7().ToString();
        var realm = configuration["KeycloakStudent:realm"] ?? "student";

        // Post to Keycloak (Keycloak server-generates its own UUID ID)
        await keycloak.Admin.Realms[realm].Users.PostAsync(new()
        {
            Username = user.Login,
            Email = user.Details?.Email,
            FirstName = user.Details?.FirstName,
            LastName = user.Details?.LastName,
            Enabled = true,
            EmailVerified = true,
            RealmRoles = ["student"],
            Credentials =
            [
                new CredentialRepresentation
                {
                    Type = "password",
                    Value = temp,
                    Temporary = true,
                }
            ],
            // TODO: Force them to use 2FA from the get go ?
            RequiredActions = ["UPDATE_PASSWORD"],
        }, null, token);

        // Query Keycloak to resolve the generated UUID
        var lookup = await keycloak.Admin.Realms[realm].Users.GetAsync(cfg =>
        {
            cfg.QueryParameters.Username = user.Login;
            cfg.QueryParameters.Exact = true;
        }, token);

        var created = lookup?.FirstOrDefault()
            ?? throw new ServiceException(500, "Failed to create user: could not resolve created user in Keycloak.");

        var id = Guid.Parse(created.Id!);

        // Bind the server-generated ID to the domain entity
        user.Id = id;
        user.Details?.UserId = id;

        // Persist User and Personal Workspace atomically
        try
        {
            var strategy = context.Database.CreateExecutionStrategy();
            var account = await strategy.ExecuteAsync(async (ct) =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(ct);

                var newUser = await context.Users.AddAsync(user, ct);
                await context.Workspaces.AddAsync(new()
                {
                    OwnerId = id,
                    Ownership = EntityOwnership.User,
                }, ct);

                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return newUser.Entity;
            }, token);

            return (account, temp);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to create user DB records for {Login}. Rolling back Keycloak account.", user.Login);
            try
            {
                await keycloak.Admin.Realms[realm].Users[id.ToString()].DeleteAsync(null, token);
            }
            catch (Exception kcEx)
            {
                log.LogError(kcEx, "Failed to cleanup Keycloak user {UserId} during rollback.", id);
            }

            throw new ServiceException(500, "Failed to create user account.");
        }
    }

    public async Task AddSshKeyAsync(Guid userId, SshKey sshKey, CancellationToken token = default)
    {
        var exists = await context.SshKeys.FirstOrDefaultAsync(
            k => k.KeyType == sshKey.KeyType && k.KeyBlob == sshKey.KeyBlob,
            token);

        ServiceException.ThrowIf(exists is not null, "This SSH Key already exists.");

        sshKey.UserId = userId;
        await context.SshKeys.AddAsync(sshKey, token);
        await context.SaveChangesAsync(token);
    }

    public async Task<bool> RemoveSshKeyAsync(string fingerprint, CancellationToken token = default)
    {
        var key = await context.SshKeys.FirstOrDefaultAsync(k => k.Fingerprint == fingerprint, token);
        if (key is null)
            return false;

        context.SshKeys.Remove(key);
        await context.SaveChangesAsync(token);
        return true;
    }

    public async Task<IEnumerable<SshKey>> GetSshKeysAsync(Guid userId, CancellationToken token = default)
    {
        return await context.Set<SshKey>()
            .Where(k => k.UserId == userId)
            .ToListAsync(token);
    }

    public async Task AnonymizeAsync(Guid id, CancellationToken token = default)
    {
        var handle = $"n0bdy-{time.GetUtcNow().ToUnixTimeSeconds()}";
        var user = await FindByIdAsync(id, token);
        ServiceException.ThrowIf(user is null, 404, "User not found.");
        ServiceException.ThrowIf(user.Login.StartsWith("n0bdy"), 422, "User is already anonymized.");

        var studentRealm = keycloak.Admin.Realms[realm].Users[id.ToString()];
        var kcUser = await studentRealm.GetAsync(null, token)!;

        await context.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            if (user.Details is not null)
                context.Remove(user.Details);

            user.Login = handle;
            user.Display = null;
            user.AvatarUrl = null;

            // Don't forget to also delete all the keys
            context.SshKeys.RemoveRange(await context.SshKeys
                .Where(k => k.UserId == id)
                .ToListAsync(ct));

            kcUser.Enabled = false;
            kcUser.FirstName = "";
            kcUser.LastName = "";
            kcUser.Email = $"{handle}@unknown.com";
            kcUser.EmailVerified = false;
            await studentRealm.PutAsync(kcUser, null, token);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }, token);
    }

    public async Task<Freeze?> GetFreezeAsync(Guid id, CancellationToken token = default)
    {
        var now = time.GetUtcNow();
        return await context.Freezes
            .Where(f => f.UserId == id && f.StartsAt <= now && f.EndsAt > now)
            .FirstOrDefaultAsync(token);
    }

    public async Task<Freeze> FreezeAsync(Guid id, Freeze entity, CancellationToken token = default)
    {
        var freeze = await GetFreezeAsync(id, token);
        ServiceException.ThrowIf(freeze is not null, "User is already frozen.");

        var result = await context.Freezes.AddAsync(entity, token);
        await context.SaveChangesAsync(token);
        return result.Entity;
    }

    public async Task UnFreezeAsync(Guid id, CancellationToken token = default)
    {
        var freeze = await GetFreezeAsync(id, token);
        ServiceException.ThrowIf(freeze is null, "User has no active freeze.");

        freeze.InvalidatedAt = time.GetUtcNow();
        await context.SaveChangesAsync(token);
    }

    public async Task<PaginatedList<Freeze>> GetFrozenUsersAsync(ISorting sorting, IPagination pagination, CancellationToken token = default)
    {
        return await context.Freezes.AsNoTracking().Sort(sorting).PaginateAsync(pagination, token);
    }
}