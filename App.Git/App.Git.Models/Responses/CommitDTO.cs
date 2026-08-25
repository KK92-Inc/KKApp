// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Git.Models.Responses;


public class CommitDTO(string Sha, string Message, string Author, DateTimeOffset UpdatedAt)
{
    [Required]
    public string Sha { get; init; } = Sha;

    [Required]
    public string Message { get; set; } = Message;

    [Required]
    public string Author { get; set; } = Author;

    [Required]
    public DateTimeOffset UpdatedAt { get; set; } = UpdatedAt;
}
