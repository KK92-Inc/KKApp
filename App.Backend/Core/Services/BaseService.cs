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
    /// <inheritdoc />
    public IQueryable<T> Query(bool tracking = true) => tracking ? _dbSet.AsQueryable<T>() : _dbSet.AsNoTracking();

    /// <inheritdoc />
    public async Task<T?> FindByIdAsync(Guid id) => await _dbSet.FirstOrDefaultAsync(x => x.Id == id);

    /// <inheritdoc />
    public bool Exists(IEnumerable<Guid> ids) => ids.Any(id => !_dbSet.Any(f => f.Id == id));

    /// <inheritdoc />
    public async Task<T> CreateAsync(T entity)
    {
        var createdEntity = await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();
        return createdEntity.Entity;
    }

    /// <inheritdoc />
    public async Task<T> DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    /// <inheritdoc />
    public async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    /// <inheritdoc />
    public async Task<PaginatedList<T>> GetAllAsync(IPagination pagination, ISorting sorting, params Expression<Func<T, bool>>?[] filters)
    {
        var query = _dbSet.AsQueryable();
        return await filters
            .Where(f => f is not null)
            .Aggregate(query, (c, filter) => c.Where(filter!))
            .Sort(sorting)
            .PaginateAsync(pagination);
    }

    protected readonly DbSet<T> _dbSet = context.Set<T>();
}
