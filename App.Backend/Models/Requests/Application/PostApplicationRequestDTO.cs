// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Requests.Applications;

public class PostApplicationRequestDTO
{
    [Required]
    [StringLength(255)]
    public required string Name { get; set; }

    [Required]
    [StringLength(2048)]
    public required string Description { get; set; }

    public ICollection<string> RedirectUris { get; set; } = [];
}