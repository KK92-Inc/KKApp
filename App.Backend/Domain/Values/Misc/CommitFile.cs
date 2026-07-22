// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;
using App.Backend.Domain.Enums;

namespace App.Backend.Domain.Values.Misc;

public record CommitFile(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("content")] string Content,
    [property: JsonPropertyName("encoding")] FileEncoding Encoding
);
