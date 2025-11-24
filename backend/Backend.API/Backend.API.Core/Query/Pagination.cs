// ============================================================================
// W2Inc, Amsterdam 2023-2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using Backend.API.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

// ============================================================================

namespace Backend.API.Core.Query;

/// <summary>
/// Defines the contract for pagination parameters.
/// </summary>
public interface IPagination
{
    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int Size { get; set; }
}

/// <summary>
/// Paginated list of items.
/// </summary>
/// <typeparam name="T">The object type</typeparam>
public class PaginatedList<T>(IReadOnlyCollection<T> items, int count, int index, int size) where T : BaseEntity
{
    /// <summary>
    /// Append pagination headers to the response headers.
    /// </summary>
    /// <param name="headers"></param>
    public PaginatedList<T> AppendHeaders(IDictionary<string, StringValues> headers)
    {
        headers.Add("X-Page", Page.ToString());
        headers.Add("X-Pages", TotalPages.ToString());
        return this;
    }

    /// <summary>
    /// The items on the current page.
    /// </summary>
    public IReadOnlyCollection<T> Items { get; } = items;

    /// <summary>
    /// The current page index.
    /// </summary>
    public int Page { get; } = index;

    /// <summary>
    /// The total number of pages available.
    /// </summary>
    public int TotalPages { get; } = (int)Math.Ceiling(count / (double)size);
}

/// <summary>
/// Extension methods for pagination.
/// </summary>
public static class PaginationExtension
{
    /// <summary>
    /// Asynchronously creates a paginated list from an IQueryable source.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="pagination">The pagination parameters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paginated list.</returns>
    public static async Task<PaginatedList<T>> PaginateAsync<T>(this IQueryable<T> source, IPagination pagination) where T : BaseEntity
    {
        int count = await source.CountAsync();
        var items = await source
            .Skip((pagination.Page - 1) * pagination.Size)
            .Take(pagination.Size)
            .ToListAsync();

        return new PaginatedList<T>(items, count, pagination.Page, pagination.Size);
    }
}
