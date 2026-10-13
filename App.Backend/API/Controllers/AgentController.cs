// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.API.MCP.Agent;
using App.Backend.Models.Requests.Agent;
using App.Backend.Models.Responses.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// ============================================================================

namespace App.Backend.API.Controllers;

[Route("agent"), ApiExplorerSettings(IgnoreApi = true)]
[ApiController, Authorize(Policy = "staff")]
public class AgentController(AgentService agent) : Controller
{
    [HttpPost("chat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Chat with the administrative agent")]
    [EndpointDescription(@"
Runs one turn of the staff agent. Stateless: send the full history, get back the new messages.
Changes require approval: when 'pending' is set, repeat the call with 'decision'.
    ")]
    public async Task<ActionResult<AgentReplyDO>> Chat([FromBody] PostAgentChatRequestDTO body, CancellationToken token)
    {
        return Ok(await agent.ChatAsync(body, token));
    }
}
