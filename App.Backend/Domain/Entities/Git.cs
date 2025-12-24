// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities;


[Table("tbl_git")]
public class Git : BaseEntity
{
    public Git()
    {
        Name = string.Empty;
        Owner = string.Empty;
    }

    [Column("name")]
    public string Name { get; set; }

    [Column("owner")]
    public string Owner { get; set; }

    [Column("ownership")]
    public EntityOwnership Ownership { get; set; }

    public virtual ICollection<Project> Projects { get; set; }

    // public virtual ICollection<Rubric> Rubrics { get; set; }

    public virtual ICollection<UserProject> UserProjects { get; set; }
}
