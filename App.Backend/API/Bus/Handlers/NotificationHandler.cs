// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json;
using Wolverine.Attributes;
using App.Backend.API.Notifications.Channels;
using App.Backend.API.Notifications;
using App.Backend.Core.Services.Interface;
using App.Backend.API.Notifications.Registers.Interface;
using App.Backend.API.Notifications.Variants;

// ============================================================================

namespace App.Backend.API.Bus.Handlers;

[WolverineHandler]
public class NotificationHandler(INotificationService service, IBroadcastRegistry registry)
{
    public async Task Handle(WelcomeUserNotification p, CancellationToken t) => await Internal(p, t);
    public async Task Handle(ProjectInviteNotification p, CancellationToken t) => await Internal(p, t);
    public async Task Handle(ProjectCompletedNotification p, CancellationToken t) => await Internal(p, t);
    public async Task Handle(GoalCompletedNotification p, CancellationToken t) => await Internal(p, t);
    public async Task Handle(CursusCompletedNotification p, CancellationToken t) => await Internal(p, t);

    private async Task Internal(INotificationMessage notification, CancellationToken token)
    {
        if (notification is IBroadcastChannel broadcast)
        {
            var notifiableId = notification.NotifiableId;
            await registry.PublishAsync(notifiableId, broadcast.ToBroadcast(), token);
        }
        if (notification is IDatabaseChannel message)
        {
            await service.CreateAsync(new()
            {
                Descriptor = notification.Meta,
                ResourceId = notification.ResourceId,
                NotifiableId = notification.NotifiableId,
                Data = JsonSerializer.Serialize(message.ToDatabase())
            }, token);
        }
    }
}
