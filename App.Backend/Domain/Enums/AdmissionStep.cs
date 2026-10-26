// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

/// <summary>
/// The steps an applicant goes through on their way to becoming a student.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdmissionStep
{
    /// <summary>
    /// The applicant still has to be verified at a check-in.
    /// </summary>
    [JsonPropertyName(nameof(CheckIn))]
    CheckIn = 0,

    /// <summary>
    /// The applicant still has to be verified at an open day.
    /// </summary>
    [JsonPropertyName(nameof(OpenDay))]
    OpenDay = 1,

    /// <summary>
    /// The applicant still has to take part in, and be selected after, a trial.
    /// </summary>
    [JsonPropertyName(nameof(Trial))]
    Trial = 2,

    /// <summary>
    /// The applicant was selected and may pick a kickoff.
    /// </summary>
    [JsonPropertyName(nameof(Kickoff))]
    Kickoff = 3,

    /// <summary>
    /// Nothing left to do: the person is in a kickoff or already is a student.
    /// </summary>
    [JsonPropertyName(nameof(Done))]
    Done = 4,
}
