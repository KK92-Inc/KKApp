// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Requests.Apply;

/// <summary>
/// Request DTO for updating a check-in or an open day. Omitted (null) fields are left untouched.
/// </summary>
/// <remarks>
/// The start and the deadline are checked against each other once merged with what is stored, not here.
/// </remarks>
public class PatchApplyEventRequestDTO
{
    [StringLength(255, MinimumLength = 1)]
    [Description("A new name for the session.")]
    public string? Name { get; init; }

    [StringLength(2000)]
    [Description("A new description. An empty string clears it.")]
    public string? Description { get; init; }

    [StringLength(500, MinimumLength = 1)]
    [Description("A new location.")]
    public string? Location { get; init; }

    [Range(1, int.MaxValue)]
    [Description("The new max amount of people that can sign up. Can't be lower than the amount already signed up.")]
    public int? Capacity { get; init; }

    [Description("Set to true to remove the capacity, so anyone can sign up. Takes precedence over capacity.")]
    public bool? Unlimited { get; init; }

    [Description("The new start date.")]
    public DateTimeOffset? StartsAt { get; init; }

    [Description("The new sign-up deadline.")]
    public DateTimeOffset? DeadlineAt { get; init; }
}
