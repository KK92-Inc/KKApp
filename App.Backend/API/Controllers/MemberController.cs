// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using App.Backend.Core.Services.Interface;
using App.Backend.Database;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities.Projects;
using App.Backend.API.Notifications.Variants;
using Wolverine;
using App.Backend.Domain.Entities.Users;
using App.Backend.Core;

// ============================================================================

namespace App.Backend.API.Controllers;

[Authorize]
[ApiController]
[Route("member"), Tags("Invitations")]
public class MemberController(
    ILogger<MemberController> log,
    IMemberService service,
    ISubscriptionService subscription,
    DatabaseContext ctx,
    IMessageBus bus
) : Controller
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Loads the UserProject and asserts it is in Active state.
    /// Centralises the guard that every UserProject-membership endpoint needs.
    /// </summary>
    private async Task<UserProject> RequireActiveSessionAsync(Guid userProjectId, CancellationToken token)
    {
        var up = await ctx.UserProjects.FirstOrDefaultAsync(up => up.Id == userProjectId, token)
            ?? throw new ServiceException(404, "Project session not found");
        if (up.State is not EntityObjectState.Active)
            throw new ServiceException(422, "Project session is not active");
        return up;
    }

    // ── Invite ────────────────────────────────────────────────────────────────

    [HttpPost("{inviteeId:guid}/project/{userProjectId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Invite a user to a project session")]
    [EndpointDescription("The calling user (leader) invites another user to their active project session.")]
    public async Task<ActionResult<MemberDO>> InviteToProject(
        Guid inviteeId, Guid userProjectId, CancellationToken token)
    {
        var up = await RequireActiveSessionAsync(userProjectId, token);
        var inviterId = User.GetSID();

        var member = await service.InviteAsync(
            MemberEntityType.UserProject,
            userProjectId,
            inviterId,
            inviteeId,
            gitId: up.GitInfoId,
            maxMembers: up.Project.MaxMembers,
            token: token);

        await ctx.UserProjectTransactions.AddAsync(new()
        {
            UserId = inviterId,
            UserProjectId = userProjectId,
            Type = UserProjectTransactionVariant.MemberInvited,
        }, token);
        await ctx.SaveChangesAsync(token);

        await bus.PublishAsync(new ProjectInviteNotification(
            InviteeId: inviteeId,
            InviterUserId: inviterId,
            UserProjectId: userProjectId));

        return Ok(new MemberDO(member));
    }

    // ── Uninvite ──────────────────────────────────────────────────────────────

    [HttpDelete("{inviteeId:guid}/project/{userProjectId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Cancel a pending invite")]
    [EndpointDescription("The session leader cancels a pending invitation before it is accepted.")]
    public async Task<ActionResult<MemberDO>> UninviteFromProject(
        Guid inviteeId, Guid userProjectId, CancellationToken token)
    {
        await RequireActiveSessionAsync(userProjectId, token);
        var inviterId = User.GetSID();

        var member = await service.UninviteAsync(
            MemberEntityType.UserProject,
            userProjectId,
            inviterId,
            inviteeId,
            token);

        await bus.PublishAsync(new ProjectInviteNotification(
            InviteeId: inviteeId,
            InviterUserId: inviterId,
            UserProjectId: userProjectId));

        return Ok(new MemberDO(member));
    }

    // ── Accept / Decline ──────────────────────────────────────────────────────

    [HttpPost("{userProjectId:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Accept a project invite")]
    [EndpointDescription("Accept a pending invitation to join a project session.")]
    public async Task<ActionResult<MemberDO>> AcceptInvite(
        Guid userProjectId, CancellationToken token)
    {
        await RequireActiveSessionAsync(userProjectId, token);
        var userId = User.GetSID();

        var member = await service.AcceptAsync(MemberEntityType.UserProject, userProjectId, userId, token);

        await ctx.UserProjectTransactions.AddAsync(new()
        {
            UserId = userId,
            UserProjectId = userProjectId,
            Type = UserProjectTransactionVariant.MemberAccepted,
        }, token);
        await ctx.SaveChangesAsync(token);

        return Ok(new MemberDO(member));
    }

    [HttpPost("{userProjectId:guid}/decline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Decline a project invite")]
    [EndpointDescription("Decline a pending invitation to a project session. Removes the invite.")]
    public async Task<ActionResult<MemberDO>> DeclineInvite(
        Guid userProjectId, CancellationToken token)
    {
        var userId = User.GetSID();
        var member = await service.DeclineAsync(MemberEntityType.UserProject, userProjectId, userId, token);

        await ctx.UserProjectTransactions.AddAsync(new()
        {
            UserId = userId,
            UserProjectId = userProjectId,
            Type = UserProjectTransactionVariant.MemberDeclined,
        }, token);
        await ctx.SaveChangesAsync(token);

        return Ok(new MemberDO(member));
    }

    // ── Leadership ────────────────────────────────────────────────────────────

    [HttpPost("{userProjectId:guid}/transfer/{newLeaderId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Transfer session leadership")]
    [EndpointDescription("Transfer leadership of a project session to another active member. The caller must be the current leader.")]
    public async Task<ActionResult> TransferLeadership(
        Guid userProjectId, Guid newLeaderId, CancellationToken token)
    {
        await RequireActiveSessionAsync(userProjectId, token);

        await service.TransferLeadershipAsync(
            MemberEntityType.UserProject,
            userProjectId,
            currentLeaderId: User.GetSID(),
            newLeaderId,
            token);

        return NoContent();
    }

    // ── Leave / Kick ──────────────────────────────────────────────────────────

    [HttpPost("{userProjectId:guid}/leave")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Leave a project session")]
    [EndpointDescription("Voluntarily leave a project session as an accepted member. Leaders must transfer leadership first.")]
    public async Task<ActionResult> LeaveProject(
        Guid userProjectId, CancellationToken token)
    {
        await RequireActiveSessionAsync(userProjectId, token);
        var userId = User.GetSID();

        await service.LeaveAsync(MemberEntityType.UserProject, userProjectId, userId, token);

        await ctx.UserProjectTransactions.AddAsync(new()
        {
            UserId = userId,
            UserProjectId = userProjectId,
            Type = UserProjectTransactionVariant.MemberLeft,
        }, token);
        await ctx.SaveChangesAsync(token);

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
        await RequireActiveSessionAsync(userProjectId, token);
        var leaderId = User.GetSID();

        await service.KickAsync(MemberEntityType.UserProject, userProjectId, leaderId, memberId, token);

        await ctx.UserProjectTransactions.AddAsync(new()
        {
            UserId = leaderId,
            UserProjectId = userProjectId,
            Type = UserProjectTransactionVariant.MemberKicked,
        }, token);
        await ctx.SaveChangesAsync(token);

        return NoContent();
    }
}