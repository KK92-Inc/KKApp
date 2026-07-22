// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

/// <summary>
/// Represents the encoding of a file, e.g: A File to be commited.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileEncoding
{
    /// <summary>
    /// This file is UTF-8 Encoded,
    /// </summary>
    [JsonPropertyName(nameof(UTF8))]
    UTF8 = 0,

    /// <summary>
    /// This file is Base64 Encoded,
    /// </summary>
    [JsonPropertyName(nameof(Base64))]
    Base64 = 1,
}
