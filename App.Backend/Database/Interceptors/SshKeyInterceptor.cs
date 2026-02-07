// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Security.Cryptography;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

// ============================================================================

namespace App.Backend.Database.Interceptors;

/// <summary>
/// Interceptor to (re-)compute the fingerprint in case it changes or is added.
/// </summary>
public sealed class SshKeyInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateSshKeyFingerprints(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateSshKeyFingerprints(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateSshKeyFingerprints(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries<SshKey>()
            .Where(e => e.State is EntityState.Added || e.State is EntityState.Modified);

        foreach (var entry in entries)
        {
            // Only recompute if the KeyBlob has changed or it's a new entity
            if (entry.State is EntityState.Added || entry.Property(x => x.KeyBlob).IsModified)
                entry.Entity.Fingerprint = ComputeFingerprint(entry.Entity.KeyBlob);
        }
    }

    /// <summary>
    /// Computes the SHA256 fingerprint for a given base64 key blob.
    /// </summary>
    public static string ComputeFingerprint(string blob)
    {
        if (string.IsNullOrWhiteSpace(blob))
            return string.Empty;

        try
        {
            var hash = SHA256.HashData(Convert.FromBase64String(blob));
            return $"SHA256:{Convert.ToBase64String(hash).TrimEnd('=')}";
        }
        catch (FormatException)
        {
            // Handle invalid base64 if necessary, or return empty
            return string.Empty;
        }
    }
}
