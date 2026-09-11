// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using App.Backend.Models.Responses.Entities.Projects;

// ============================================================================

namespace App.Backend.API.Controllers.Interfaces;

public interface IInviteController
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="inviteeId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<ActionResult<MemberDO>> InviteAsync(Guid entityId, Guid inviteeId, CancellationToken token);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="inviteeId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<ActionResult<MemberDO>> UninviteAsync(Guid entityId, Guid inviteeId, CancellationToken token);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<ActionResult<MemberDO>> AcceptAsync(Guid entityId, CancellationToken token);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<ActionResult<MemberDO>> DeclineAsync(Guid entityId, CancellationToken token);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="newLeaderId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<ActionResult> TransferLeadershipAsync(Guid entityId, Guid newLeaderId, CancellationToken token);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<ActionResult> LeaveAsync(Guid entityId, CancellationToken token);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="memberId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<ActionResult> KickAsync(Guid entityId, Guid memberId, CancellationToken token);
}
