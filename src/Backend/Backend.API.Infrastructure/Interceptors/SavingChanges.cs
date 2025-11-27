// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Backend.API.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

// ============================================================================

namespace Backend.API.Infrastructure.Interceptors;

/// <inheritdoc />
/// <param name="dateTime"></param>
public class SavingChangesInterceptor(TimeProvider dateTime) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetDateTimeValues(eventData.Context!);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SetDateTimeValues(eventData.Context!);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SetDateTimeValues(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                var utcNow = dateTime.GetUtcNow();
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }
}
