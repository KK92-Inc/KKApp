using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Enums;

namespace App.Backend.Domain.Entities.Users;

[Table("tbl_user_details")]
public class Details : BaseEntity
{
    public Details()
    {
        Email = null;
        Markdown = null;
        FirstName = null;
        LastName = null;
        GithubUrl = null;
        LinkedinUrl = null;
        RedditUrl = null;
        WebsiteUrl = null;
        User = null!;
        EnabledNotifications = NotificationVariant.All;
    }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("bio"), MaxLength(16384)]
    public string? Markdown { get; set; }

    [Column("first_name")]
    public string? FirstName { get; set; }

    [Column("last_name")]
    public string? LastName { get; set; }

    /// <summary>
    /// Flags / Combination of notifications the user does wish to
    /// receive in the future.
    /// </summary>
    [Column("enabled_notifications")]
    public NotificationVariant EnabledNotifications { get; set; }

    [Column("github_url")]
    public string? GithubUrl { get; set; }

    [Column("linkedin_url")]
    public string? LinkedinUrl { get; set; }

    [Column("reddit_url")]
    public string? RedditUrl { get; set; }

    [Column("website_url")]
    public string? WebsiteUrl { get; set; }
}
