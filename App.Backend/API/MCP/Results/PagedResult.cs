// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.MCP.Results;

// ============================================================================

/// <summary>
/// A page of tool results.
/// </summary>
public sealed record PagedResult<T>(int Total, int Page, int Pages, IReadOnlyCollection<T> Items);
