// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

/// <summary>
/// Where a user currently is within a trial period.
/// </summary>
/// <remarks>
/// <see cref="Registered"/> and <see cref="Active"/> are the "open" states: a user can only be in one
/// open trial at a time. The database enforces this with a filtered index on the numeric values
/// of these two, so keep <c>DatabaseContext.OnModelCreating</c> in sync if you ever renumber them.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserTrialState
{
    /// <summary>
    /// Signed up for a trial that hasn't started yet.
    /// </summary>
    [JsonPropertyName(nameof(Registered))]
    Registered = 0,

    /// <summary>
    /// The trial is running and the user is taking part in it.
    /// </summary>
    [JsonPropertyName(nameof(Active))]
    Active = 1,

    /// <summary>
    /// The user was still taking part when the trial ended.
    /// </summary>
    [JsonPropertyName(nameof(Completed))]
    Completed = 2,

    /// <summary>
    /// The user left while the trial was running, or never showed up for it.
    /// </summary>
    [JsonPropertyName(nameof(Quit))]
    Quit = 3,
}
