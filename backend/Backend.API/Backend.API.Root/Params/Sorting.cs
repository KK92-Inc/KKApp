// ============================================================================
// W2Inc, Amsterdam 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.ComponentModel;
using Backend.API.Core.Query;
using Microsoft.AspNetCore.Mvc;

// ============================================================================

namespace Backend.API.Root.Params;

/// <summary>
/// Represents parameters used for sorting query results.
/// </summary>
/// <remarks>
/// This class provides properties to specify both the field to sort by and the sorting direction.
/// </remarks>
public class Sorting : ISorting
{
    /// <summary>
    /// Gets or sets the name of the property or field to order results by.
    /// </summary>
    /// <value>The name of the property to use for sorting.</value>
    [FromQuery(Name = "sort[by]")]
    [Description("The name of the property to use for sorting.")]
    public string? OrderBy { get; set; }

    /// <summary>
    /// Gets or sets the sort direction (ascending or descending).
    /// </summary>
    /// <value>The sort direction. Defaults to <see cref="Order.Ascending"/>.</value>
    [FromQuery( Name = "sort[order]")]
    [Description("The sort direction.")]
    public Order? Order { get; set; }
}
