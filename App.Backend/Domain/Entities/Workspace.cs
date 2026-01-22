// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities;

/// <summary>
/// Project entity.
/// </summary>
[Table("tbl_workspace")]
public class Workspace : BaseEntity
{
    /// <summary>
    /// If owner is null, it indicates that this is a staff/system
    /// workspace meant for "official" projects, goals, cursi, ...
    /// </summary>
    [Column("owner_id")]
    public Guid? OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public required virtual User? Owner { get; set; }

    [Column("ownership")]
    public EntityOwnership Ownership { get; set; }

    public ICollection<Cursus> Cursi { get; set; }
    public ICollection<Goal> Goals { get; set; }
    public ICollection<Project> Projects { get; set; }
}
