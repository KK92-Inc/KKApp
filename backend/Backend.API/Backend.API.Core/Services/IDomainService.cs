// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Linq.Expressions;
using NXTBackend.API.Core.Utils.Query;
using NXTBackend.API.Domain.Common;
using NXTBackend.API.Models;

// ============================================================================

namespace NXTBackend.API.Core;

/// <summary>
/// Interface for Domain models.
///
/// Ensure that basic CRUD functions are defined.
/// </summary>
/// <remarks></remarks>
/// <typeparam name="T">The model type.</typeparam>
public interface IDomainService<T> where T : BaseEntity
{
	public IQueryable<T> Query(bool tracking = true);

	/// <summary>
	/// Find the entity by its ID.
	/// </summary>
	/// <param name="id">The ID.</param>
	/// <returns>The entity found by that ID or null if not found.</returns>
	Task<T?> FindByIdAsync(Guid id);

	/// <summary>
	/// Update the entity.
	/// </summary>
	/// <param name="entity">The updated entity.</param>
	/// <returns>The updated entity.</returns>
	Task<T> UpdateAsync(T entity);

	/// <summary>
	/// Validates if all provided IDs exist in the database.
	/// </summary>
	/// <param name="ids">Collection of IDs to validate.</param>
	/// <returns>True if all IDs are valid, false otherwise.</returns>
	Task<bool> AreValid(IEnumerable<Guid> ids);

	/// <summary>
	/// Delete the entity.
	/// </summary>
	/// <param name="entity">The entity to delete</param>
	/// <returns>The deleted entity.</returns>
	Task<T> DeleteAsync(T entity);

	/// <summary>
	/// Create a new entity.
	/// </summary>
	/// <param name="entity">The newly created entity.</param>
	/// <returns>The newly created entity.</returns>
	Task<T> CreateAsync(T entity);

	/// <summary>
	/// Get all entities.
	/// </summary>
	/// <param name="pagination">Specific pagination parameters (as to avoid large queries)</param>
	/// <param name="sorting">Parameters for sorting the results</param>
	/// <param name="filter">Optional filters to apply to the query</param>
	/// <returns>A paginated list of entities.</returns>
	Task<PaginatedList<T>> GetAllAsync(PaginationParams pagination, SortingParams sorting, FilterDictionary? filter = null);
}
