// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Enums;

/// <summary>
/// Defines the permission levels for entities within the application.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EntityPermission
{
    /// <summary>
    /// Read-only permission.
    /// </summary>
    [JsonPropertyName(nameof(Read))]
    Read,

    /// <summary>
    /// Write permission, also includes read access.
    /// </summary>
    [JsonPropertyName(nameof(Write))]
    Write,
}
