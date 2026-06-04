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
/// Notification sent to a user when they are invited to a workspace.
/// Delivered via SSE broadcast so the invitee sees it in real-time.
/// </summary>
public sealed record WorkspaceInviteNotification(
    Guid InviteeId,
    Guid InviterUserId,
    Guid WorkspaceId
) : INotificationMessage, IBroadcastChannel, IDatabaseChannel
{
    public Guid NotifiableId => InviteeId;
    public Guid? ResourceId => WorkspaceId;
    public NotificationMeta Meta => NotificationMeta.AcceptOrDecline | NotificationMeta.Project | NotificationMeta.User | NotificationMeta.Feed;

    public BroadcastMessage ToBroadcast() => new("WorkspaceInvite", new
    {
        WorkspaceId,
        InviterUserId,
    });

    public NotificationData ToDatabase() => new WorkspaceInviteData(WorkspaceId, InviterUserId);
}
