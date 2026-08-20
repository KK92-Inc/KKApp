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
public enum FileType
{
    [JsonPropertyName(nameof(Text))]
    Text = 0,

    [JsonPropertyName(nameof(Binary))]
    Binary = 1,
}
