// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Models.Responses.Entities.Notifications;

/// <summary>
/// Notification data for a project session invite.
/// </summary>
/// <param name="UserProjectId">The project session the invitee is being invited to.</param>
/// <param name="InviterUserId">The user who sent the invite.</param>
public record ProjectInviteData(Guid UserProjectId, Guid InviterUserId) : NotificationData;
