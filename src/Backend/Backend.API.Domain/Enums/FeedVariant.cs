// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace Backend.API.Domain.Enums;

/// <summary>
/// The different kinds of feeds that exist.
/// </summary>
[Flags]
// [JsonConverter(typeof(JsonStringEnumConverter)),]
public enum FeedVariant
{
	/// <summary>
	/// Feed requires user to accept or decline.
	/// </summary>
	[JsonPropertyName(nameof(AcceptOrDecline))]
	AcceptOrDecline = 1 << 0,

	/// <summary>
	/// Broadcasted to all users, such as a new feature or update.
	/// If this is set, the feed *wont* need to set the NotifiableId, instead it will be ignored and the feed will be broadcasted to all users.
	/// </summary>
	[JsonPropertyName(nameof(Global))]
	Global = 1 << 1,

	/// <summary>
	/// Only visible to the user who receives the feed.
	/// If this is set, the feed need to set the NotifiableId to the user who should receive it.
	/// </summary>
	[JsonPropertyName(nameof(Private))]
	Private = 1 << 2,

	/// <summary>
	/// A normal feed that just notifies the user of something that has happened.
	/// </summary>
	[JsonPropertyName(nameof(Default))]
	Default = 1 << 3,

	/// <summary>
	/// Feed is viewable only (no accept/decline) directs the user to a page where they can view more information about the feed item.
	/// </summary>
	[JsonPropertyName(nameof(Viewable))]
	Viewable = 1 << 4,

	/// <summary>
	/// Feed related to a project.
	/// </summary>
	[JsonPropertyName(nameof(Project))]
	Project = 1 << 5,

	/// <summary>
	/// Feed related to a goal.
	/// </summary>
	[JsonPropertyName(nameof(Goal))]
	Goal = 1 << 6,

	/// <summary>
	/// Feed related to a cursus.
	/// </summary>
	[JsonPropertyName(nameof(Cursus))]
	Cursus = 1 << 7,

	/// <summary>
	/// Feed related to a review.
	/// </summary>
	[JsonPropertyName(nameof(Review))]
	Review = 1 << 8,

	/// <summary>
	/// Feed related to being welcomed.
	/// </summary>
	[JsonPropertyName(nameof(Welcome))]
	Welcome = 1 << 9,
}
