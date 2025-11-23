// ============================================================================
// W2Inc, Amsterdam 2023-2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using Backend.API.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace Backend.API.Core.Query;

/// <summary>
/// Paginated list of items.
/// /// </summary>
/// <typeparam name="T">The object type</typeparam>
public class PaginatedList<T>(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize) where T : BaseEntity
{
    /// <summary>
    /// Create a paginated list from a queryable source.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="index"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int index, int size)
    {
        int count = await source.CountAsync();
        var items = await source
            .Skip((index - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PaginatedList<T>(items, count, index, size);
    }

    /// <summary>
    /// Append pagination headers to the response headers.
    /// </summary>
    /// <param name="headers"></param>
    public void AppendHeaders(IDictionary<string, StringValues> headers)
    {
        headers.Add("X-Page", Page.ToString());
        headers.Add("X-Pages", TotalPages.ToString());
    }

    public IReadOnlyCollection<T> Items { get; } = items;

    public int Page { get; } = pageNumber;

    public int TotalPages { get; } = (int)Math.Ceiling(count / (double)pageSize);
}
