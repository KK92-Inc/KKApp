// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;
using App.Backend.Models.Responses;

namespace App.Backend.Models.Responses.Entities.Notifications;

// ============================================================================

/// <summary>
/// Spotlight data object - projects a Spotlight entity into the notification response format.
/// This allows spotlights to be returned alongside regular notifications with a consistent API.
/// </summary>
public class SpotlightNotificationDO : BaseEntityDO<Spotlight>
{
    public SpotlightNotificationDO() { }

    public SpotlightNotificationDO(Spotlight spotlight) : base(spotlight)
    {
        Title = spotlight.Title;
        Description = spotlight.Description;
        ActionText = spotlight.ActionText;
        Href = spotlight.Href;
        BackgroundUrl = spotlight.BackgroundUrl ?? "";
        StartsAt = spotlight.StartsAt;
        EndsAt = spotlight.EndsAt;
    }

    /// <summary>
    /// The title of the spotlight.
    /// </summary>
    [Required, Description("")]
    public string Title { get; set; } = null!;

    /// <summary>
    /// The description or body text of the spotlight.
    /// </summary>
    [Required, Description("")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// The text to display on the action button.
    /// </summary>
    [Required, Description("")]
    public string ActionText { get; set; } = null!;

    /// <summary>
    /// The URL to navigate to when the action is clicked.
    /// </summary>
    [Required, Description("")]
    public string Href { get; set; } = null!;

    /// <summary>
    /// The background image URL for the spotlight card.
    /// </summary>
    [Required, Description("")]
    public string BackgroundUrl { get; set; } = null!;

    /// <summary>
    /// When this spotlight becomes visible.
    /// </summary>
    [Required, Description("")]
    public DateTimeOffset StartsAt { get; set; }

    /// <summary>
    /// When this spotlight expires (null = no expiry).
    /// </summary>
    [Required, Description("")]
    public DateTimeOffset? EndsAt { get; set; }
}
