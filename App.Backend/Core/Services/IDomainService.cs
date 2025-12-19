// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Linq.Expressions;
using App.Backend.Core.Query;
using App.Backend.Domain;

// ============================================================================

namespace App.Backend.Core.Services;

public interface IDomainService<T> where T : BaseEntity
{
    /// <summary>
    /// Returns a queryable for the entity.
    /// </summary>
    /// <param name="tracking">Whether to track changes or not.</param>
    /// <returns>A queryable for the entity.</returns>
    public IQueryable<T> Query(bool tracking = true);

    /// <summary>
    /// Find the entity by its ID.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <returns>The entity found by that ID or null if not found.</returns>
    public Task<T?> FindByIdAsync(Guid id, CancellationToken token = default);

    /// <summary>
    /// Update the entity.
    /// </summary>
    /// <param name="entity">The updated entity.</param>
    /// <returns>The updated entity.</returns>
    public Task UpdateAsync(T entity, CancellationToken token = default);

    /// <summary>
    /// Validates if all provided IDs exist in the database.
    /// </summary>
    /// <param name="ids">Collection of IDs to validate.</param>
    /// <returns>True if all IDs are valid, false otherwise.</returns>
    public bool Exists(IEnumerable<Guid> ids);

    /// <summary>
    /// Delete the entity.
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <returns>The deleted entity.</returns>
    public Task DeleteAsync(T entity, CancellationToken token = default);

    /// <summary>
    /// Create a new entity.
    /// </summary>
    /// <param name="entity">The newly created entity.</param>
    /// <returns>The newly created entity.</returns>
    public Task<T> CreateAsync(T entity, CancellationToken token = default);

    /// <summary>
    /// Get all entities with pagination, sorting and filtering.
    /// </summary>
    /// <param name="pagination">The pagination options.</param>
    /// <param name="sorting">The sorting options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="filters">The filters to apply.</param>
    /// <returns>A paginated list of entities.</returns>
    public Task<PaginatedList<T>> GetAllAsync(
        ISorting sorting,
        IPagination pagination,
        CancellationToken token = default,
        params Expression<Func<T, bool>>?[] filters
    );
}
