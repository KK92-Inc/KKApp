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
    /// Returns all membership rows for a given entity.
    /// </summary>
    Task<List<Member>> GetAsync(MemberEntityType type, Guid entityId, CancellationToken ct = default);

    /// <summary>
    /// EF-translatable predicate for filtering queries by active membership.
    /// Use inside Where() clauses — not async.
    /// </summary>
    Expression<Func<Member, bool>> IsActiveMember(MemberEntityType type, Guid entityId, Guid userId);

    /// <summary>
    /// Invite a user to an entity. Adds a Pending member row.
    /// Pass <paramref name="gitId"/> for entities that have a repository.
    /// Pass <paramref name="maxMembers"/> to enforce a capacity cap.
    /// </summary>
    Task<Member> InviteAsync(
        MemberEntityType type,
        Guid entityId,
        Guid inviterId,
        Guid inviteeId,
        Guid? gitId       = null,
        int? maxMembers   = null,
        CancellationToken token = default);

    /// <summary>
    /// Cancel a pending invite. Only the leader may uninvite; the invitee
    /// must still be in Pending state.
    /// </summary>
    Task<Member> UninviteAsync(
        MemberEntityType type,
        Guid entityId,
        Guid inviterId,
        Guid inviteeId,
        CancellationToken token = default);

    /// <summary>
    /// Accept a pending invite — flips Pending → Member.
    /// </summary>
    Task<Member> AcceptAsync(MemberEntityType type, Guid entityId, Guid userId, CancellationToken token = default);

    /// <summary>
    /// Decline a pending invite — removes the Pending row.
    /// </summary>
    Task<Member> DeclineAsync(MemberEntityType type, Guid entityId, Guid userId, CancellationToken token = default);

    /// <summary>
    /// Transfer leadership to another active, non-pending member.
    /// The current leader is demoted to Member.
    /// </summary>
    Task TransferLeadershipAsync(
        MemberEntityType type,
        Guid entityId,
        Guid currentLeaderId,
        Guid newLeaderId,
        CancellationToken token = default);

    /// <summary>
    /// An accepted non-leader member voluntarily leaves an entity.
    /// Sets LeftAt rather than deleting the row so history is preserved.
    /// </summary>
    Task LeaveAsync(MemberEntityType type, Guid entityId, Guid userId, CancellationToken token = default);

    /// <summary>
    /// Leader removes another member from an entity.
    /// Pending members are deleted outright; active members get a LeftAt timestamp.
    /// </summary>
    Task KickAsync(
        MemberEntityType type,
        Guid entityId,
        Guid leaderId,
        Guid memberId,
        CancellationToken token = default);
}