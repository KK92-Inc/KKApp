// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

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
    public SpotlightNotificationDO(Spotlight spotlight) : base(spotlight)
    {
        EndsAt = spotlight.EndsAt;
        StartsAt = spotlight.StartsAt;
        Title = spotlight.Title;
        Description = spotlight.Description;
        ActionText = spotlight.ActionText;
        Href = spotlight.Href;
        BackgroundUrl = spotlight.BackgroundUrl ?? ""; // TODO: Fallback image?
    }

    /// <summary>
    /// The title of the spotlight.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The description or body text of the spotlight.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The text to display on the action button.
    /// </summary>
    public string ActionText { get; set; }

    /// <summary>
    /// The URL to navigate to when the action is clicked.
    /// </summary>
    public string Href { get; set; }

    /// <summary>
    /// The background image URL for the spotlight card.
    /// </summary>
    public string BackgroundUrl { get; set; }

    /// <summary>
    /// When this spotlight becomes visible.
    /// </summary>
    public DateTimeOffset StartsAt { get; set; }

    /// <summary>
    /// When this spotlight expires (null = no expiry).
    /// </summary>
    public DateTimeOffset? EndsAt { get; set; }
}
