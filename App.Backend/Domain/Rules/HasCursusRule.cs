// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Rules;

/// <summary>
/// User must be enrolled in a specific cursus.
/// </summary>
public sealed class HasCursusRule : Rule
{
    /// <summary>
    /// The slug of the cursus the user must be enrolled in.
    /// </summary>
    [JsonPropertyName("cursusSlug")]
    public required string CursusSlug { get; set; }
}
