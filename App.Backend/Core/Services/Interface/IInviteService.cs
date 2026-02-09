// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Projects;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IInviteService
{
    /// <summary>
    /// Invite a user to an existing project session. Adds a Pending member row.
    /// </summary>
    Task<UserProjectMember> InviteToProjectAsync(Guid inviterId, Guid inviteeId, Guid userProjectId, CancellationToken token);

    /// <summary>
    /// Accept a pending invite — flips Pending → Member.
    /// </summary>
    Task<UserProjectMember> AcceptInviteAsync(Guid userId, Guid userProjectId, CancellationToken token);

    /// <summary>
    /// Decline a pending invite — removes the pending member row.
    /// </summary>
    Task DeclineInviteAsync(Guid userId, Guid userProjectId, CancellationToken token);
}
