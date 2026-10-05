// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Requests.Trials;

// ============================================================================

/// <summary>
/// Request DTO for recording the same verdict on several participants at once.
/// </summary>
public record TrialOutcomesRequestDTO
{
    [Required, MinLength(1), MaxLength(256)]
    [Description("The ids of the users. Duplicates are ignored.")]
    public required Guid[] UserIds { get; init; }

    [Required, EnumDataType(typeof(TrialOutcome))]
    [Description("The verdict: Selected, Rejected, or Pending to clear an earlier one.")]
    public required TrialOutcome Outcome { get; init; }
}
