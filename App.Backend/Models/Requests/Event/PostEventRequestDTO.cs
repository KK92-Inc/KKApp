// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Event;

// ============================================================================

/// <summary>
/// Request DTO for creating a new campus event.
/// </summary>
public record PostEventRequestDTO : IValidatableObject
{
    /// <summary>
    /// Name of the event.
    /// </summary>
    [Required, StringLength(255, MinimumLength = 1)]
    [Description("Name of the event.")]
    public required string Name { get; init; }

    /// <summary>
    /// A short readable description of the event.
    /// </summary>
    [Required, StringLength(255, MinimumLength = 1)]
    [Description("Short description of the event.")]
    public required string Description { get; init; }

    /// <summary>
    /// Optional event thumbnail URL.
    /// </summary>
    [Url, StringLength(255)]
    [Description("Optional thumbnail URL for the event.")]
    public string? Thumbnail { get; init; }

    /// <summary>
    /// Detailed markdown content describing the event.
    /// </summary>
    [Required, StringLength(2048, MinimumLength = 1)]
    [Description("Markdown presentation of what the event is about.")]
    public required string Markdown { get; init; }

    /// <summary>
    /// Maximum capacity of attendees.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")]
    [Description("The maximum number of participants allowed.")]
    public int Capacity { get; init; }

    /// <summary>
    /// Minimum required occupancy threshold to transition state.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Threshold must be at least 1.")]
    [Description("Minimum required occupancy for the event to switch state. Only staff can set it to null.")]
    public int? Threshold { get; init; }

    /// <summary>
    /// Event start time.
    /// </summary>
    [Required]
    [Description("When the event will start.")]
    public DateTimeOffset StartsAt { get; init; }

    /// <summary>
    /// Event end time.
    /// </summary>
    [Required]
    [Description("When the event will conclude.")]
    public DateTimeOffset EndsAt { get; init; }

    /// <summary>
    /// Registration closing time.
    /// </summary>
    [Description("When the event stops accepting new attendees or allowing leaves.")]
    public DateTimeOffset? ClosesAt { get; init; }

    /// <summary>
    /// Cross-property logical validations executed automatically during model binding.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (EndsAt <= StartsAt)
        {
            yield return new ValidationResult(
                "Event end time must be after the start time.",
                [nameof(EndsAt)]
            );
        }

        if (ClosesAt.HasValue && ClosesAt > StartsAt)
        {
            yield return new ValidationResult(
                "Registration close time cannot be set after the event start time.",
                [nameof(ClosesAt)]
            );
        }

        if (Threshold.HasValue && Threshold > Capacity)
        {
            yield return new ValidationResult(
                "Threshold cannot exceed maximum capacity.",
                [nameof(Threshold)]
            );
        }
    }
}