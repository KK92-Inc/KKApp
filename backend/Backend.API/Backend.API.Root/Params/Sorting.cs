using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Backend.API.Root.Params;

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

/// <summary>
/// Represents parameters used for sorting query results.
/// </summary>
/// <remarks>
/// This class provides properties to specify both the field to sort by and the sorting direction.
/// </remarks>
public class SortingParams
{
    /// <summary>
    /// Gets or sets the name of the property or field to order results by.
    /// </summary>
    /// <value>The name of the property to use for sorting.</value>
    [FromQuery(Name = "sort[by]")]
    public string? OrderBy { get; set; }

    /// <summary>
    /// Gets or sets the sort direction (ascending or descending).
    /// </summary>
    /// <value>The sort direction. Defaults to <see cref="Order.Ascending"/>.</value>
    [FromQuery( Name = "sort[order]")]
    public Order Order { get; set; } = Order.Ascending;
}
