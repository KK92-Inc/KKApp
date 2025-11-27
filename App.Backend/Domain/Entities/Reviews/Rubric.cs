// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using App.Backend.Domain;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Entities.Reviews;

[Table("tbl_rubric")]
[Index(nameof(Name))]
public class Rubric : BaseEntity
{
    public Rubric()
    {
        Name = string.Empty;
        Markdown = string.Empty;
        Public = false;
        Enabled = false;

        ProjectId = Guid.Empty;
        Project = null!;

        CreatorId = Guid.Empty;
        Creator = null!;

        GitInfoId = Guid.Empty;
        GitInfo = null!;
    }

    [Column("name")]
    public string Name { get; set; }

    [Column("markdown")]
    public string Markdown { get; set; }

    [Column("public")]
    public bool Public { get; set; }

    [Column("enabled")]
    public bool Enabled { get; set; }

    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [Column("creator_id")]
    public Guid CreatorId { get; set; }

    [Column("git_info_id")]
    public Guid GitInfoId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project Project { get; set; }

    [ForeignKey(nameof(CreatorId))]
    public virtual User Creator { get; set; }

    [ForeignKey(nameof(GitInfoId))]
    public virtual Git GitInfo { get; set; }

    public virtual ICollection<Review> Reviews { get; set; }

    public virtual ICollection<UserProject> UserProjects { get; set; }
}
