// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Core.Services.Interface;
using App.Backend.Models.Responses.Entities.Projects;
using App.Backend.API.Notifications.Variants;
using Wolverine;

// ============================================================================

namespace App.Backend.API.Controllers;

[Authorize]
[ApiController]
[Route("invite"), Tags("Invitations")]
public class InviteController(
    ILogger<InviteController> log,
    IInviteService service,
    IMessageBus bus
) : Controller
{
    [HttpPost("{inviteeId:guid}/project/{userProjectId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Invite a user to a project session")]
    [EndpointDescription("The calling user (leader) invites another user to their active project session.")]
    public async Task<ActionResult<UserProjectMemberDO>> InviteToProject(
        Guid inviteeId, Guid userProjectId, CancellationToken token)
    {
        var inviterId = User.GetSID();
        var member = await service.InviteToProjectAsync(inviterId, inviteeId, userProjectId, token);

        await bus.PublishAsync(new ProjectInviteNotification(
            InviteeId: inviteeId,
            InviterUserId: inviterId,
            UserProjectId: userProjectId
        ));

        return Ok(new UserProjectMemberDO(member));
    }

    [HttpPost("{userProjectId:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Accept a project invite")]
    [EndpointDescription("Accept a pending invitation to join a project session.")]
    public async Task<ActionResult<UserProjectMemberDO>> AcceptInvite(
        Guid userProjectId, CancellationToken token)
    {
        var userId = User.GetSID();
        var member = await service.AcceptInviteAsync(userId, userProjectId, token);
        return Ok(new UserProjectMemberDO(member));
    }

    [HttpPost("{userProjectId:guid}/decline")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Decline a project invite")]
    [EndpointDescription("Decline a pending invitation to a project session. Removes the invite.")]
    public async Task<ActionResult> DeclineInvite(
        Guid userProjectId, CancellationToken token)
    {
        var userId = User.GetSID();
        await service.DeclineInviteAsync(userId, userProjectId, token);
        return NoContent();
    }
}
