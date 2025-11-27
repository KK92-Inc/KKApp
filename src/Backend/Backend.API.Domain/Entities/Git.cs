// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Backend.API.Domain.Entities.Users;
using Backend.API.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace Backend.API.Domain.Entities;

[Table("tbl_git")]
public class Git : BaseEntity
{
    public Git()
    {
        Name = string.Empty;
        Namespace = string.Empty;
        Url = string.Empty;
    }

    [Column("name")]
    public string Name { get; set; }

    [Column("namespace")]
    public string Namespace { get; set; }

    [Column("url")]
    public string Url { get; set; }

    [Column("ownership")]
    public EntityOwnership Ownership { get; set; }

    public virtual ICollection<Project> Projects { get; set; }

    // public virtual ICollection<Rubric> Rubrics { get; set; }

    public virtual ICollection<UserProject> UserProjects { get; set; }
}
