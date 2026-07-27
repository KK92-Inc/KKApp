// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Requests.Application;

public class PatchApplicationRequestDTO
{
    [StringLength(255, MinimumLength = 1)]
    [Description("The name of the application.")]
    public string? Name { get; set; }

    [Required]
    [Description("Whether the application is enabled.")]
    public bool? Enabled { get; set; }

    [StringLength(2048, MinimumLength = 1)]
    [Description("A description of the application.")]
    public string? Description { get; set; }

    [Description("List of scopes this app has.")]
    public ICollection<string>? Scopes { get; set; }

    [Description("List of allowed redirect URIs after authentication.")]
    public ICollection<string>? RedirectUris { get; set; }
}