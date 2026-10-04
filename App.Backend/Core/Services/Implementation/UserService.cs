// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using Keycloak.AuthServices.Sdk.Kiota.Admin.Models;
using Microsoft.EntityFrameworkCore;
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
    [FromKeyedServices("student")] IKeycloakService student,
    [FromKeyedServices("admin")] IKeycloakService admin
) : BaseService<User>(ctx), IUserService
{
    private readonly DatabaseContext context = ctx;

    #region Keycloak helpers

    /// <summary>
    /// Maps a domain role to the Keycloak service (realm) to create the user in and the realm
    /// role to assign. Applicants and students live in the student realm, staff in the admin realm.
    /// Only used for creation; anonymize discovers the realm via <see cref="FindKeycloakUserAsync"/>.
    /// </summary>
    private (IKeycloakService Keycloak, string? RealmRole) Resolve(UserRole role) => role switch
    {
        UserRole.Applicant => (student, "applicant"),
        UserRole.Student => (student, "student"),
        UserRole.Staff => (admin, null),
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown user role."),
    };

    /// <summary>
    /// Finds which realm a user lives in by asking Keycloak itself, so there is no
    /// realm/role column to keep in sync. IDs are server-generated UUIDs, so a hit in
    /// one realm can't be a different person in the other.
    /// Student realm goes first since most accounts live there.
    /// </summary>
    private async Task<(IKeycloakService Keycloak, UserRepresentation User)?> FindKeycloakUserAsync(
        Guid id,
        CancellationToken token
    )
    {
        foreach (var keycloak in new[] { student, admin })
            if (await keycloak.FindUserAsync(id, token) is { } user)
                return (keycloak, user);
        return null;
    }

    #endregion

    #region Queries

    public override async Task<User?> FindByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _dbSet.Include(u => u.Details).FirstOrDefaultAsync(u => u.Id == id, token);
    }

    public async Task<User?> FindByLoginAsync(string login, CancellationToken token = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Login == login, cancellationToken: token);
    }

    public async Task<User?> FindByNameAsync(string displayName, CancellationToken token = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Display == displayName, cancellationToken: token);
    }

    #endregion

    #region Create / Update

    public override async Task UpdateAsync(User entity, CancellationToken token = default)
    {
        if (entity.Details is not null && context.Entry(entity.Details).State is EntityState.Detached)
            context.Add(entity.Details);

        await base.UpdateAsync(entity, token);
    }

    // The base overload would skip Keycloak entirely, so make it unusable.
    public override Task<User> CreateAsync(User entity, CancellationToken token = default)
        => throw new NotSupportedException(
            "Use CreateAsync(User, UserRole, CancellationToken) so the account is provisioned in Keycloak.");

    /// <summary>
    /// Provision the user in the Keycloak realm matching <paramref name="role"/>, then persist
    /// the database user and their personal workspace. No password is generated: the user
    /// sets one through the "Forgot password" flow.
    /// </summary>
    public async Task<User> CreateAsync(User user, UserRole role, CancellationToken token = default)
    {
        var (keycloak, realmRole) = Resolve(role);

        var id = await keycloak.CreateUserAsync(new UserRepresentation
        {
            Username = user.Login,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            // Applicants need to go through a trial, if they pass they must select a kickoff.
            Enabled = role is not UserRole.Applicant,
            EmailVerified = true, // required for "Forgot password" to send mail
            // TODO: Force 2FA from the get go ?
        }, token);

        // Bind the server-generated ID to the domain entity
        user.Id = id;
        user.Details?.UserId = id;

        try
        {
            if (realmRole is not null)
                await keycloak.AddRoleAsync(id, realmRole, token);

            var strategy = context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async (ct) =>
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
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to finish creating user {Login}. Rolling back Keycloak account.", user.Login);
            try
            {
                // CancellationToken.None: the cleanup must still run if the request was cancelled.
                await keycloak.DeleteUserAsync(id, CancellationToken.None);
            }
            catch (Exception kcEx)
            {
                log.LogError(kcEx, "Failed to cleanup Keycloak user {UserId} during rollback.", id);
            }

            throw new ServiceException(500, "Failed to create user account.");
        }
    }

    #endregion

    #region SSH keys

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

    #endregion

    #region Anonymize

    public async Task AnonymizeAsync(Guid id, CancellationToken token = default)
    {
        var handle = $"n0bdy-{time.GetUtcNow().ToUnixTimeSeconds()}";
        var user = await FindByIdAsync(id, token);
        ServiceException.ThrowIf(user is null, 404, "User not found.");
        ServiceException.ThrowIf(user.Login.StartsWith("n0bdy"), 422, "User is already anonymized.");

        var kc = await FindKeycloakUserAsync(id, token);
        if (kc is null)
        {
            // Erasure shouldn't be blocked by an account someone already removed from Keycloak.
            // Swap this for a ThrowIf(404) if you'd rather fail loudly.
            log.LogWarning("User {UserId} was not found in any Keycloak realm; anonymizing DB records only.", id);
        }

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

            await context.SaveChangesAsync(ct);

            // Keycloak last: if this throws, the DB transaction rolls back with it.
            if (kc is not null)
            {
                var (keycloak, kcUser) = kc.Value;
                kcUser.Enabled = false;
                kcUser.FirstName = "";
                kcUser.LastName = "";
                kcUser.Email = $"{handle}@unknown.com";
                kcUser.EmailVerified = false;
                await keycloak.SetUserAsync(id, kcUser, ct);
            }

            await transaction.CommitAsync(ct);
        }, token);
    }

    #endregion

    #region Freezes

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

    #endregion
}