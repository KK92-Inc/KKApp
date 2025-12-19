// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

// ============================================================================

namespace App.Backend.Domain.Entities;

/// <summary>
/// Project entity.
/// </summary>
[Table("cursus")]
[Index(nameof(Name)), Index(nameof(Slug))]
public class Cursus : BaseEntity
{
    [Column("name")]
    public string Name { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("slug")]
    public string Slug { get; set; }

    /// <summary>
    /// The track / path of the Cursus stored in the .graph format.
    /// </summary>
    [Column("track", TypeName = "jsonb")]
    public JsonDocument? Track { get; set; }
}
