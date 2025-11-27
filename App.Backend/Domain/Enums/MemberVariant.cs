// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

/// <summary>
/// Represents the role and status of a member within a project or group context.
/// This enumeration is treated as a bit flag, allowing for combinations of roles and states.
/// </summary>
[Flags]
public enum MemberVariant
{
	/// <summary>
	/// Indicates the user has project privileges over other members.
	/// This role typically grants permissions to invite or remove others from projects.
	/// </summary>
	[JsonPropertyName(nameof(Master))]
	Master = 1 << 0,

	/// <summary>
	/// Indicates the user is a standard member without project privileges.
	/// This role is subject to the management of a <see cref="Master"/>.
	/// </summary>
	[JsonPropertyName(nameof(Pawn))]
	Pawn = 1 << 1,

	/// <summary>
	/// Indicates the member's invitation or application is currently awaiting approval.
	/// </summary>
	[JsonPropertyName(nameof(Pending))]
	Pending = 1 << 2,

	/// <summary>
	/// Indicates the member has officially joined and their status is confirmed.
	/// </summary>
	[JsonPropertyName(nameof(Accepted))]
	Accepted = 1 << 3,
}
