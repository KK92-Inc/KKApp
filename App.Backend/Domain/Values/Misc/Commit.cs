// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Values.Misc;

public class Commit
{
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("author")]
    public required CommitAuthor Author { get; init; }

    [JsonPropertyName("files")]
    public required IEnumerable<CommitFile> Files { get; init; }
}
