// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Core.Services.Options;

/// <summary>
/// Configuration options for the Repository HTTP service.
/// </summary>
public class GitServiceOptions
{
    public const string SectionName = "Git";

    /// <summary>
    /// The base URL of the Bun repository REST API (e.g. "http://localhost:3000").
    /// </summary>
    public required string BaseUrl { get; set; }
}
