// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Users;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// A detailed data object representing user details.
/// </summary>
/// <param name="details"></param>
public class UserDetailsDO : BaseEntityDO<Details>
{
    public UserDetailsDO() { }

    public UserDetailsDO(Details details) : base(details)
    {
        Email = details.Email;
        Markdown = details.Markdown;
        FirstName = details.FirstName;
        LastName = details.LastName;
        GithubUrl = details.GithubUrl;
        LinkedinUrl = details.LinkedinUrl;
        RedditUrl = details.RedditUrl;
        WebsiteUrl = details.WebsiteUrl;
    }

    [Required]
    public string? Email { get; set; }
    [Required]
    public string? Markdown { get; set; }
    [Required]
    public string? FirstName { get; set; }
    [Required]
    public string? LastName { get; set; }
    [Required]
    public string? GithubUrl { get; set; }
    [Required]
    public string? LinkedinUrl { get; set; }
    [Required]
    public string? RedditUrl { get; set; }
    [Required]
    public string? WebsiteUrl { get; set; }

    public static implicit operator UserDetailsDO?(Details? data) => data is null ? null : new(data);
}
