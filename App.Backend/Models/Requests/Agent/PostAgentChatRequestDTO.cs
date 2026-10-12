// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace App.Backend.Models.Requests.Agent;

// ============================================================================

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AgentDecision
{
    Approve,
    Deny,
}

public class PostAgentChatRequestDTO
{
    [Required, MinLength(1), MaxLength(100)]
    [Description("The conversation so far. Must end with a user message, or with an assistant message that is waiting for a decision.")]
    public required List<AgentMessageDTO> Messages { get; init; }

    [Description("Answer to a pending approval. Only valid when the last message is an assistant message with tool calls.")]
    public AgentDecision? Decision { get; init; }
}
