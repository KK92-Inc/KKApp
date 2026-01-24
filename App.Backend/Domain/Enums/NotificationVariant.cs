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
	/// Standard informational notification that informs the recipient about an event; usually requires no action.
	/// </summary>
	[JsonPropertyName(nameof(Default))]
	Default = 1 << 3,

	/// <summary>
	/// Read-only notification that directs the user to view further details (no accept/decline actions).
	/// </summary>
	[JsonPropertyName(nameof(Viewable))]
	Viewable = 1 << 4,

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
	/// Welcome notification, typically sent when a user registers or is onboarded.
	/// </summary>
	[JsonPropertyName(nameof(Welcome))]
	Welcome = 1 << 9,

    /// <summary>
	/// Notification shall not be shown as a feed.
	/// </summary>
	[JsonPropertyName(nameof(NonFeed))]
	NonFeed = 1 << 10,

	/// <summary>
	/// Represents all flags combined (bitwise all ones).
	/// </summary>
	[JsonPropertyName(nameof(All))]
	All = ~0,
}
