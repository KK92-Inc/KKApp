// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Git.Models.Requests;

public class PostCommitWithAuthorDTO : PostCommitDTO
{
    [Required, StringLength(255, MinimumLength = 1)]
    [Description("Name of the commit author.")]
    public required string Author { get; set; }

    [Required, EmailAddress, StringLength(255, MinimumLength = 1)]
    [Description("Email of the commit author.")]
    public required string Email { get; set; }
}

