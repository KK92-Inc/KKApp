// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Linq.Expressions;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Projects;
using App.Backend.Domain.Entities.Users;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IMemberService
{
    /// <summary>
    /// Invite a user to an existing project session. Adds a Pending member row.
    /// </summary>
    /// <param name="inviterId"></param>
    /// <param name="inviteeId"></param>
    /// <param name="userProjectId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Member> InviteToProjectAsync(Guid inviterId, Guid inviteeId, Guid userProjectId, CancellationToken token);

    // Add to IMemberService
    Task<List<Member>> GetProjectMembersAsync(Guid userProjectId, CancellationToken token = default);

    // EF-translatable expression used in GetAllAsync predicates — NOT async,
    // returns an IQueryable-compatible bool expression
    Expression<Func<UserProject, bool>> HasActiveMember(Guid userProjectId, Guid userId);

    /// <summary>
    /// Remove a user from an existing project session. Removes the pending member row.
    /// </summary>
    /// <param name="inviterId"></param>
    /// <param name="inviteeId"></param>
    /// <param name="userProjectId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Member> UninviteFromProjectAsync(Guid inviterId, Guid inviteeId, Guid userProjectId, CancellationToken token);

    /// <summary>
    /// Accept a pending invite — flips Pending → Member.
    /// </summary>
    Task<Member> AcceptInviteAsync(Guid userId, Guid userProjectId, CancellationToken token);

    /// <summary>
    /// Decline a pending invite — removes the pending member row.
    /// </summary>
    Task DeclineInviteAsync(Guid userId, Guid userProjectId, CancellationToken token);

    /// <summary>
    /// Transfer leadership of a project session to another active member.
    /// The current leader becomes a regular member.
    /// </summary>
    Task TransferLeadershipAsync(Guid currentLeaderId, Guid newLeaderId, Guid userProjectId, CancellationToken token);

    /// <summary>
    /// An accepted (non-leader) member voluntarily leaves a project session.
    /// Sets LeftAt on their membership row rather than deleting it (preserves history).
    /// </summary>
    Task LeaveProjectAsync(Guid userId, Guid userProjectId, CancellationToken token);

    /// <summary>
    /// Leader kicks an accepted member from the project session.
    /// Sets LeftAt on the member and records a MemberKicked transaction.
    /// </summary>
    Task KickMemberAsync(Guid leaderId, Guid memberId, Guid userProjectId, CancellationToken token);
}
