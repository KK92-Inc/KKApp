// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using App.Backend.Core.Query;
using App.Backend.Domain;
using App.Backend.Database;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Core.Services;

/// <summary>
/// Base service for all services.
///
/// This allows you do to basic CRUD operations on a specific model.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseService<T>(DatabaseContext context) : IDomainService<T> where T : BaseEntity
{
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    /// <summary>
    /// Query the entities.
    /// </summary>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public IQueryable<T> Query(bool tracking = true) => tracking ? _dbSet.AsQueryable<T>() : _dbSet.AsNoTracking();

    /// <summary>
    /// Create a new entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public virtual async Task<T> CreateAsync(T entity, CancellationToken token = default)
    {
        var result = await _dbSet.AddAsync(entity, token);
        await context.SaveChangesAsync(token);
        return result.Entity;
    }

    /// <summary>
    /// Update the entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(T entity, CancellationToken token = default)
    {
        _dbSet.Update(entity);
        await context.SaveChangesAsync(token);
    }

    /// <summary>
    /// Delete the entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public virtual async Task DeleteAsync(T entity, CancellationToken token = default)
    {
        _dbSet.Remove(entity);
        await context.SaveChangesAsync(token);
    }

    /// <summary>
    /// Validates if any provided IDs exist in the database.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public virtual async Task<bool> ExistsAsync(IEnumerable<Guid> ids, CancellationToken token = default)
    {
        var valid = await _dbSet.Select(x => x.Id).ToListAsync();
        return ids.All(valid.Contains);
    }


    /// <summary>
    /// Find the entity by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public virtual async Task<T?> FindByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Id == id, token);
    }

    /// <summary>
    /// Get all entities with pagination, sorting and filtering.
    /// </summary>
    /// <param name="sorting"></param>
    /// <param name="pagination"></param>
    /// <param name="token"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    public virtual async Task<PaginatedList<T>> GetAllAsync(
        ISorting sorting,
        IPagination pagination,
        CancellationToken token = default,
        params Expression<Func<T, bool>>?[] filters
    )
    {
        return await filters
            .Where(f => f is not null)
            .Aggregate(_dbSet.AsQueryable(), (c, filter) => c.Where(filter!))
            .Sort(sorting)
            .PaginateAsync(pagination, token);
    }
}
