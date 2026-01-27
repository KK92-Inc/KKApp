// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

/// <summary>
/// Different notification variants used to classify and control notification behaviour.
/// </summary>
[Flags]
public enum NotificationVariant
{
	/// <summary>
	/// Requires the recipient to accept or decline (e.g., invitations or requests).
	/// </summary>
	[JsonPropertyName(nameof(AcceptOrDecline))]
	AcceptOrDecline = 1 << 0,

	/// <summary>
	/// Notification that references a project (should include the associated project identifier).
	/// </summary>
	[JsonPropertyName(nameof(Project))]
	Project = 1 << 5,

	/// <summary>
	/// Notification related to a goal or milestone (includes goal identifier when applicable).
	/// </summary>
	[JsonPropertyName(nameof(Goal))]
	Goal = 1 << 6,

	/// <summary>
	/// Notification tied to a cursus (course or learning path); may include cursus identifier.
	/// </summary>
	[JsonPropertyName(nameof(Cursus))]
	Cursus = 1 << 7,

	/// <summary>
	/// Notification about a review or feedback item (references a specific review).
	/// </summary>
	[JsonPropertyName(nameof(Review))]
	Review = 1 << 8,

    /// <summary>
    /// The notification is intented for a user.
    /// </summary>
	[JsonPropertyName(nameof(User))]
	User = 1 << 9,

    /// <summary>
    /// The notification is intented for a organizaton.
    /// </summary>
    [JsonPropertyName(nameof(Organization))]
	Organization = 1 << 10,

    /// <summary>
	/// Show the notification as a feed notification
    /// I.e: Show up on the homepage feed
	/// </summary>
	[JsonPropertyName(nameof(Feed))]
	Feed = 1 << 10,
}
