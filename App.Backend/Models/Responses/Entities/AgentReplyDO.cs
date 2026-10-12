// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json;
using App.Backend.Models.Requests.Agent;

namespace App.Backend.Models.Responses.Entities;

// ============================================================================

public class AgentReplyDO
{
    /// <summary>
    /// New messages produced by this request. Append them to the conversation.
    /// </summary>
    public required IReadOnlyList<AgentMessageDTO> Messages { get; init; }

    /// <summary>
    /// Set when the agent wants to change something and needs a human to approve it first.
    /// </summary>
    public AgentPendingDO? Pending { get; init; }
}

public class AgentPendingDO
{
    public required IReadOnlyList<AgentPendingCallDO> Calls { get; init; }
}

public class AgentPendingCallDO
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public JsonElement Arguments { get; init; }

    /// <summary>
    /// False for read-only calls that are only held back because they came with a write.
    /// </summary>
    public bool RequiresApproval { get; init; }
}
