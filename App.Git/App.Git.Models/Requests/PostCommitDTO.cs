// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Git.Models.Requests;


public class PostCommitDTO()
{
    [Required]
    public required string Author { get; set; }

    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Message { get; set; }

    /// <summary>
    /// Key: File Path
    /// Value: File Content Base64 Encoded.
    /// </summary>
    [Required, MinLength(1)]
    public required Dictionary<string, string> Files { get; set; }
}

