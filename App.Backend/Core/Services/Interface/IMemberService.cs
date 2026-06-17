// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Linq.Expressions;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IMemberService
{
    /// <summary>
    /// Find a member ship of a specific user on a specific entity.
    /// If null then user is not a member of this entity.
    /// </summary>
    /// <param name="EntityId"></param>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Member?> FindByEntityAndUserId(Guid entityId, Guid userId, CancellationToken token = default);

    /// <summary>
    /// Invite a user to a certain entity to become a member.
    /// </summary>
    /// <param name="entityId">The entity in question</param>
    /// <param name="userId">The user to invite</param>
    /// <param name="gitId">When provided, grants access to the following git repository</param>
    /// <param name="max">When provided, reject the invite attempt if member count exceeds max.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Member> InviteAsync(Guid entityId, Guid userId, Guid? gitId, int? max, CancellationToken token = default);

    /// <summary>
    /// Revoke any invite the specified user may have had on this entity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Member> UnInviteAsync(Guid entityId, Guid userId, CancellationToken token = default);

    /// <summary>
    /// Sets the role of a member, if set to leader it will transfer the leadership.
    /// </summary>
    /// <param name="memberId">The member to </param>
    /// <param name="role"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Member> SetRoleAsync(Guid memberId, MemberRole role, CancellationToken token = default);

    /// <summary>
    /// Accept the pending invite for a member.
    /// </summary>
    /// <param name="memberId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Member> AcceptAsync(Guid memberId, CancellationToken token = default);

    /// <summary>
    /// Decline a pending invite for a member.
    /// </summary>
    /// <param name="memberId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Member> DeclineAsync(Guid memberId, CancellationToken token = default);

    /// <summary>
    /// Leave the membership.
    /// </summary>
    /// <param name="memberId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Member> LeaveAsync(Guid memberId, CancellationToken token = default);

    /// <summary>
    /// Kick a user out of the membership.
    /// </summary>
    /// <param name="memberId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Member> KickAsync(Guid memberId, CancellationToken token = default);
}