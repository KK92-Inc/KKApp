// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Values.Misc;

// ============================================================================

public record CommitAuthor(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email
);
