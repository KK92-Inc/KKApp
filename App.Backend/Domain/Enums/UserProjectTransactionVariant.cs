// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

/// <summary>
/// Represents the various types of activities or events that can occur within a project lifecycle.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserProjectTransactionVariant
{
    /// <summary>
    /// Indicates that the project has started.
    /// </summary>
    [JsonPropertyName(nameof(Started))]
    Started,

    /// <summary>
    /// Indicates that a new member has joined the project.
    /// </summary>
    [JsonPropertyName(nameof(MemberJoined))]
    MemberJoined,

    /// <summary>
    /// Indicates that a member has left the project.
    /// </summary>
    [JsonPropertyName(nameof(MemberLeft))]
    MemberLeft,

    /// <summary>
    /// Indicates a Git commit was pushed or associated with the project.
    /// TODO: Future use - can be expanded to include more Git-related activities.
    /// </summary>
    [JsonPropertyName(nameof(GitCommit))]
    GitCommit,

    /// <summary>
    /// Indicates the project state has changed to InActive.
    /// </summary>
    [JsonPropertyName(nameof(StateChangedToInActive))]
    StateChangedToInActive,

    /// <summary>
    /// Indicates the project state has changed to Active.
    /// </summary>
    [JsonPropertyName(nameof(StateChangedToActive))]
    StateChangedToActive,

    /// <summary>
    /// Indicates the project state has changed to Completed.
    /// </summary>
    [JsonPropertyName(nameof(StateChangedToCompleted))]
    StateChangedToCompleted,

    /// <summary>
    /// Indicates the project state has changed to Awaiting.
    /// </summary>
    [JsonPropertyName(nameof(StateChangedToAwaiting))]
    StateChangedToAwaiting,

    [JsonPropertyName(nameof(MemberInvited))]
    MemberInvited,

    [JsonPropertyName(nameof(MemberAccepted))]
    MemberAccepted,

    [JsonPropertyName(nameof(MemberDeclined))]
    MemberDeclined,
}
