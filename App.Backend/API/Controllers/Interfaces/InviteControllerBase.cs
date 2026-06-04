// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities.Projects;

// ============================================================================

namespace App.Backend.API.Controllers.Interfaces;

public interface IInviteController
{
    Task<ActionResult<MemberDO>> InviteAsync(Guid entityId, Guid inviteeId, CancellationToken token);
    Task<ActionResult<MemberDO>> UninviteAsync(Guid entityId, Guid inviteeId, CancellationToken token);
    Task<ActionResult<MemberDO>> AcceptAsync(Guid entityId, CancellationToken token);
    Task<ActionResult<MemberDO>> DeclineAsync(Guid entityId, CancellationToken token);
    Task<ActionResult> TransferLeadershipAsync(Guid entityId, Guid newLeaderId, CancellationToken token);
    Task<ActionResult> LeaveAsync(Guid entityId, CancellationToken token);
    Task<ActionResult> KickAsync(Guid entityId, Guid memberId, CancellationToken token);
}

// {
//     protected abstract MemberEntityType EntityType { get; }

//     /// <summary>
//     /// Override to supply the entity's git repo ID and optional member cap.
//     /// Default returns (null, null) — correct for Goal, Cursus, Workspace.
//     /// </summary>
//     protected virtual Task<(Guid? gitId, int? maxMembers)> GetEntityMetaAsync(
//         Guid entityId, CancellationToken token)
//         => Task.FromResult<(Guid?, int?)>((null, null));

//     // ── Helpers ───────────────────────────────────────────────────────────────

//     [HttpPost("/entityId:guid/invite/{inviteeId:guid}")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [ProducesErrorResponseType(typeof(ProblemDetails))]
//     [EndpointSummary("Invite a user to a project session")]
//     [EndpointDescription("The calling user (leader) invites another user to their active project session.")]
//     protected async Task<ActionResult<MemberDO>> InviteAsync(
//         Guid entityId, Guid inviteeId, CancellationToken token)
//     {
//         var (gitId, maxMembers) = await GetEntityMetaAsync(entityId, token);
//         var member = await service.InviteAsync(
//             EntityType, entityId, User.GetSID(), inviteeId, gitId, maxMembers, token);
//         return Ok(new MemberDO(member));
//     }

//     protected async Task<ActionResult<MemberDO>> UninviteAsync(
//         Guid entityId, Guid inviteeId, CancellationToken token)
//     {
//         var member = await service.UninviteAsync(
//             EntityType, entityId, User.GetSID(), inviteeId, token);
//         return Ok(new MemberDO(member));
//     }

//     protected async Task<ActionResult<MemberDO>> AcceptAsync(
//         Guid entityId, CancellationToken token)
//     {
//         var member = await service.AcceptAsync(EntityType, entityId, User.GetSID(), token);
//         return Ok(new MemberDO(member));
//     }

//     protected async Task<ActionResult<MemberDO>> DeclineAsync(
//         Guid entityId, CancellationToken token)
//     {
//         var member = await service.DeclineAsync(EntityType, entityId, User.GetSID(), token);
//         return Ok(new MemberDO(member));
//     }

//     protected async Task<ActionResult> TransferLeadershipAsync(
//         Guid entityId, Guid newLeaderId, CancellationToken token)
//     {
//         await service.TransferLeadershipAsync(EntityType, entityId, User.GetSID(), newLeaderId, token);
//         return NoContent();
//     }

//     protected async Task<ActionResult> LeaveAsync(Guid entityId, CancellationToken token)
//     {
//         await service.LeaveAsync(EntityType, entityId, User.GetSID(), token);
//         return NoContent();
//     }

//     protected async Task<ActionResult> KickAsync(
//         Guid entityId, Guid memberId, CancellationToken token)
//     {
//         await service.KickAsync(EntityType, entityId, User.GetSID(), memberId, token);
//         return NoContent();
//     }
// }