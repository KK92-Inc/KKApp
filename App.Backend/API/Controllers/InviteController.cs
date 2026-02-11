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

    [HttpDelete("{inviteeId:guid}/project/{userProjectId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Remove a pending invite from a project session")]
    [EndpointDescription("Reject a pending invite")]
    public async Task<ActionResult<UserProjectMemberDO>> UninviteToProject(
        Guid inviteeId, Guid userProjectId, CancellationToken token)
    {
        var inviterId = User.GetSID();
        var member = await service.UninviteFromProjectAsync(inviterId, inviteeId, userProjectId, token);

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

    [HttpPost("{userProjectId:guid}/transfer/{newLeaderId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Transfer session leadership")]
    [EndpointDescription("Transfer leadership of a project session to another active member. The caller must be the current leader.")]
    public async Task<ActionResult> TransferLeadership(
        Guid userProjectId, Guid newLeaderId, CancellationToken token)
    {
        var currentLeaderId = User.GetSID();
        await service.TransferLeadershipAsync(currentLeaderId, newLeaderId, userProjectId, token);
        return NoContent();
    }

    [HttpPost("{userProjectId:guid}/leave")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Leave a project session")]
    [EndpointDescription("Voluntarily leave a project session as an accepted member. Leaders must transfer leadership first.")]
    public async Task<ActionResult> LeaveProject(
        Guid userProjectId, CancellationToken token)
    {
        var userId = User.GetSID();
        await service.LeaveProjectAsync(userId, userProjectId, token);
        return NoContent();
    }

    [HttpDelete("{memberId:guid}/project/{userProjectId:guid}/kick")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Kick a member from a project session")]
    [EndpointDescription("The session leader kicks a member or cancels a pending invite.")]
    public async Task<ActionResult> KickMember(
        Guid memberId, Guid userProjectId, CancellationToken token)
    {
        var leaderId = User.GetSID();
        await service.KickMemberAsync(leaderId, memberId, userProjectId, token);
        return NoContent();
    }
}
