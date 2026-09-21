// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Entities.Events;
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
    [Column("login")]
    public string Login { get; set; }

    [Column("display")]
    public string? Display { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; }

    [Column("last_name")]
    public string LastName { get; set; }

    /// <summary>
    /// The 
    /// </summary>
    [Column("email")]
    public string Email { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("kickoff_id")]
    public Guid? KickoffId { get; set; }

    public virtual Kickoff? Kickoff { get; set; }

    public virtual Details? Details { get; set; }

    //= Relations =//

    public virtual ICollection<SshKey> SshKeys { get; set; }

    public virtual ICollection<Event> Events { get; set; }
}
