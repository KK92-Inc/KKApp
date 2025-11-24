// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace Backend.API.Domain.Enums;

/// <summary>
/// The different kinds of notifications that exist.
/// </summary>
[Flags]
public enum NotificationKind
{
    /// <summary>
    ///     No specific notification kind, acts as a wildcard/ignore
    /// </summary>
    [JsonPropertyName(nameof(None))] None = 0,

    /// <summary>
    /// Notification requires user to accept or decline.
    /// </summary>
    [JsonPropertyName(nameof(AcceptOrDecline))]
    AcceptOrDecline = 1 << 0,

    /// <summary>
    /// Broadcasted to all users, such as a new feature or update.
    /// If this is set, the notification *wont* need to set the NotifiableId, instead it will be ignored and the notification will be broadcasted to all users.
    /// </summary>
    [JsonPropertyName(nameof(Global))]
    Global = 1 << 1,

    /// <summary>
    /// Only visible to the user who receives the notification.
    /// If this is set, the notification need to set the NotifiableId to the user who should receive it.
    /// </summary>
    [JsonPropertyName(nameof(Private))]
    Private = 1 << 2,

    /// <summary>
    /// A normal notification that just notifies the user of something that has happened.
    /// </summary>
    [JsonPropertyName(nameof(Default))]
    Default = 1 << 3,

    /// <summary>
    /// Notification is viewable, ALLOWS to direct the user to a page where they can view more information about the notification item.
    /// </summary>
    [JsonPropertyName(nameof(Viewable))]
    Viewable = 1 << 4,

    /// <summary>
    /// Notification related to a project.
    /// </summary>
    [JsonPropertyName(nameof(Project))]
    Project = 1 << 5,

    /// <summary>
    /// Notification related to a goal.
    /// </summary>
    [JsonPropertyName(nameof(Goal))]
    Goal = 1 << 6,

    /// <summary>
    /// Notification related to a cursus.
    /// </summary>
    [JsonPropertyName(nameof(Cursus))]
    Cursus = 1 << 7,

    /// <summary>
    /// Notification related to a review.
    /// </summary>
    [JsonPropertyName(nameof(Review))]
    Review = 1 << 8,

    /// <summary>
    /// Notification related to being welcomed.
    /// </summary>
    [JsonPropertyName(nameof(Welcome))]
    Welcome = 1 << 9,

    /// <summary>
    /// The notification should also be presented as a feed
    /// </summary>
    [JsonPropertyName(nameof(Feed))]
    Feed = 1 << 10,

    /// <summary>
    /// The notification should *ONLY* be presented as a feed.
    /// Meaning a notification won't show up as a notification in a traditional sense.
    /// </summary>
    [JsonPropertyName(nameof(FeedOnly))]
    FeedOnly = 1 << 11,
}
