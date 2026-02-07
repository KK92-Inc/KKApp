// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain;

// ============================================================================

namespace App.Backend.Core.Services;

public interface ISlugQueryable<T> where T : BaseEntity
{
    /// <summary>
    /// Find the entity by its ID.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <returns>The entity found by that ID or null if not found.</returns>
    public Task<T?> FindBySlugAsync(Guid id, CancellationToken token = default);
}
