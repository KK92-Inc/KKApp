// ============================================================================
// W2Inc, Amsterdam 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using Backend.API.Core.Query;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace Backend.API.Root.Params;

/// <summary>
/// Query parameters for pagination.
/// </summary>
public class Pagination : IPagination
{
	private const int c_MaxPageSize = 50;

	private int _pageSize = 30;
	private int _pageNumber = 1;

	/// <summary>
	/// The page number.
	/// </summary>
	[Range(1, int.MaxValue)]
	[FromQuery(Name = "page[index]")]
    [Description("The page number/index")]
	public int Page
	{
		get => _pageNumber;
		set => _pageNumber = Math.Max(1, value);
	}

	/// <summary>
	/// How many items per page.
	/// </summary>
	[Range(1, c_MaxPageSize)]
	[FromQuery(Name = "page[size]")]
    [Description("The amount of results per page")]
	public int Size
	{
		get => _pageSize;
		set => _pageSize = Math.Min(c_MaxPageSize, Math.Max(1, value));
	}
}
