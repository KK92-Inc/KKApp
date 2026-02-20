// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using App.Backend.API.Notifications.Channels;
using App.Backend.API.Bus.Messages;
using App.Backend.Models.Responses.Entities.Notifications;

// ============================================================================

namespace App.Backend.API.Notifications.Variants;

/// <summary>
/// Notification sent to a user when they are invited to a project session.
/// Delivered via SSE broadcast so the invitee sees it in real-time.
/// </summary>
public sealed record ProjectInviteNotification(
    Guid InviteeId,
    Guid InviterUserId,
    Guid UserProjectId
) : INotificationMessage, IBroadcastChannel, IDatabaseChannel
{
    public Guid NotifiableId => InviteeId;
    public Guid? ResourceId => UserProjectId;
    public NotificationMeta Meta => NotificationMeta.AcceptOrDecline | NotificationMeta.Project | NotificationMeta.User;

    public BroadcastMessage ToBroadcast() => new("ProjectInvite", new
    {
        UserProjectId,
        InviterUserId,
    });

    public NotificationData ToDatabase() => new ProjectInviteData(UserProjectId, InviterUserId);
}
