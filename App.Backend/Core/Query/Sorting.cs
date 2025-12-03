// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Text.Json.Serialization;
using App.Backend.Domain;

// ============================================================================

namespace App.Backend.Core.Query;

/// <summary>
/// The different kinds of order that exist.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Order
{
    /// <summary>
    /// Order the requested resource in ascending order.
    /// </summary>
    [JsonPropertyName(nameof(Ascending))]
    Ascending,

    /// <summary>
    /// Order the requested resource in descending order.
    /// </summary>
    [JsonPropertyName(nameof(Descending))]
    Descending,
}

public interface ISorting
{
    string? OrderBy { get; set; }
    Order Order { get; set; }
}

public static class SortingExtensions
{
    /// <summary>
    /// Applies sorting to a queryable collection based on the provided sorting parameters.
    /// </summary>
    /// <param name="source">The source queryable to sort.</param>
    /// <param name="sorting">The sorting parameters to apply.</param>
    /// <returns>A sorted queryable.</returns>
    public static IQueryable<T> Sort<T>(this IQueryable<T> source, ISorting sorting) where T : BaseEntity
    {
        try
        {
            // Use default sorting if no valid sort parameters are provided
            if (string.IsNullOrWhiteSpace(sorting.OrderBy))
                return source.ApplyDefaultSorting(sorting.Order);

            // Process each sort parameter
            var orderParams = sorting.OrderBy.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (orderParams.Length == 0)
                return source.ApplyDefaultSorting(sorting.Order);

            var defaultDirection = sorting.Order == Order.Descending ? "desc" : "asc";
            var sortExpressions = new List<string>(orderParams.Length);

            foreach (var param in orderParams)
            {
                var parts = param.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                var field = parts[0];
                var direction = parts.Length > 1 ? parts[1].ToLowerInvariant() : defaultDirection;
                sortExpressions.Add($"{field} {direction}");
            }

            // Apply sorting or default if no valid expressions
            return sortExpressions.Count > 0
                ? source.OrderBy(string.Join(", ", sortExpressions))
                : source.ApplyDefaultSorting(sorting.Order);
        }
        catch (ParseException e)
        {
            throw new ServiceException(400, e.Message);
        }
    }

    /// <summary>
    /// Applies default sorting by Id to a queryable.
    /// </summary>
    private static IQueryable<T> ApplyDefaultSorting<T>(this IQueryable<T> source, Order order) where T : BaseEntity
    {
        return source.OrderBy(order == Order.Descending
            ? $"{nameof(BaseEntity.Id)} descending"
            : nameof(BaseEntity.Id));
    }
}
