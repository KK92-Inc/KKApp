// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities;


/// <summary>
/// Project entity.
/// </summary>
[Table("projects")]
[Index(nameof(Name)), Index(nameof(Slug))]
public class Project : BaseEntity
{
    [Column("name")]
    public string Name { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("slug")]
    public string Slug { get; set; }

    [Column("active")]
    public bool Active { get; set; }

    [Column("public")]
    public bool Public { get; set; }

    [Column("deprecated")]
    public bool Deprecated { get; set; }
}
