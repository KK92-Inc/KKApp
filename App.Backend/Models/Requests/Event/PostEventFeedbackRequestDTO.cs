// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Event;

// ============================================================================

/// <summary>
/// Request DTO for submitting event feedback.
/// </summary>
public record PostEventFeedbackRequestDTO
{
    /// <summary>
    /// Rating score given by the user.
    /// </summary>
    [Required, Range(0, 10, ErrorMessage = "Rating must be between 0 and 10.")]
    [Description("A rating from 0 to 10 on how the event was.")]
    public int Rating { get; init; }

    /// <summary>
    /// Comment body left by the user.
    /// </summary>
    [Required, StringLength(2048, MinimumLength = 1)]
    [Description("The comment left by the user.")]
    public required string Comment { get; init; }
}