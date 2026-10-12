// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace App.Backend.Models.Requests.Agent;

// ============================================================================

/// <summary>
/// One message of the conversation. Used in both directions: the client stores what the server
/// returns and sends the whole history back on the next request (the server keeps no state).
/// </summary>
public class AgentMessageDTO
{
    [Required, RegularExpression("^(user|assistant|tool)$")]
    [Description("Who wrote the message: user, assistant or tool (a tool result).")]
    public required string Role { get; init; }

    [MaxLength(16_000)]
    [Description("The text of the message (Markdown for the assistant).")]
    public string Content { get; init; } = "";

    [Description("Tool calls the assistant made in this message, if any.")]
    public List<AgentToolCallDTO>? ToolCalls { get; init; }

    [Description("For role=tool: the name of the tool that produced this result.")]
    public string? ToolName { get; init; }
}

public class AgentToolCallDTO
{
    [Required]
    [Description("Name of the tool.")]
    public required string Name { get; init; }

    [Description("The arguments as a JSON object.")]
    public JsonElement Arguments { get; init; }
}
