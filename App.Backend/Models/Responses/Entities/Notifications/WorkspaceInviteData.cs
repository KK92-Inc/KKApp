// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Responses.Entities.Notifications;

/// <summary>
/// Notification data for a workspace invite.
/// </summary>
/// <param name="WorkspaceId">The workspace the invitee is being invited to.</param>
/// <param name="InviterUserId">The user who sent the invite.</param>
public record WorkspaceInviteData([Required] Guid WorkspaceId,[Required] Guid InviterUserId) : NotificationData;
