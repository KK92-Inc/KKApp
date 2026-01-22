// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using App.Backend.Domain;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Entities.Users;

/// <summary>
/// A feature is a experimental feature that is being developed.
/// </summary>
[Table("tbl_user")]
[Index(nameof(Login), IsUnique = true)]
[Index(nameof(Login), nameof(Display))]
public class User : BaseEntity
{
    public User()
    {
        Login = string.Empty;
        Display = null;
        AvatarUrl = null;
        DetailsId = null;
        Details = null;
    }

    [Column("login")]
    public string Login { get; set; }

    [Column("display")]
    public string? Display { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("details_id")]
    public Guid? DetailsId { get; set; }

    [ForeignKey(nameof(DetailsId))]
    public virtual Details? Details { get; set; }

    //= Relations =//

    /// <summary>
    /// SSH keys associated with this user for git operations.
    /// </summary>
    public virtual ICollection<SshKey> SshKeys { get; set; } = [];

    // public virtual ICollection<Review> Rubricer { get; set; }
    // public virtual ICollection<Project> CreatedProjects { get; set; }
    // public virtual ICollection<Rubric> CreatedRubrics { get; set; }
    // public virtual ICollection<LearningGoal> CreatedGoals { get; set; }
    // public virtual ICollection<Cursus> CreatedCursus { get; set; }
    // public virtual ICollection<UserGoal> UserGoals { get; set; }
    // public virtual ICollection<UserCursus> UserCursi { get; set; }
    // public virtual ICollection<Comment> Comments { get; set; }
    // public virtual ICollection<Member> ProjectMember { get; set; }
    // public virtual ICollection<CursusCollaborator> CollaboratedCursi { get; set; }
    // public virtual ICollection<GoalCollaborator> CollaboratedGoals { get; set; }

    // NOTE(W2): Project collaboration is tracked via the external git source control.
    // That way we can sync the state more conveniently in case user gets added as a collaborator
    // via the frontend or directly from Gitea.
    //
    // Therefor this Many-To-Many relationship is not necessary.
    // public virtual ICollection<Project> CollaboratesOnProjects { get; set; }
}
