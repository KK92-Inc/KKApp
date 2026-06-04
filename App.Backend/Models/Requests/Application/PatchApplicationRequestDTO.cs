// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Requests.Applications;

public class PatchApplicationRequestDTO
{
    [StringLength(255)]
    public string? Name { get; set; }

    [StringLength(2048)]
    public string? Description { get; set; }

    public ICollection<string>? RedirectUris { get; set; }
}