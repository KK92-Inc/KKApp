// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using App.Backend.Domain.Entities.Users;

// ============================================================================

namespace App.Backend.Models.Entities;

public class UserDetailsDO(Details details) : BaseEntityDO<Details>(details)
{
    [Required]
    public string? Email { get; set; } = details.Email;

    [Required]
    public string? Markdown { get; set; } = details.Markdown;

    [Required]
    public string? FirstName { get; set; } = details.FirstName;

    [Required]
    public string? LastName { get; set; } = details.LastName;

    [Required]
    public string? GithubUrl { get; set; } = details.GithubUrl;

    [Required]
    public string? LinkedinUrl { get; set; } = details.LinkedinUrl;

    [Required]
    public string? RedditUrl { get; set; } = details.RedditUrl;

    [Required]
    public string? WebsiteUrl { get; set; } = details.WebsiteUrl;

    public static implicit operator UserDetailsDO?(Details? data) => data is null ? null : new(data);
}
