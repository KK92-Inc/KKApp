// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using App.Backend.API.Views.Models;
using App.Backend.API.Notifications.Channels;
using Wolverine;
using System.Net.Mail;
using App.Backend.Domain.Entities.Users;
using Microsoft.AspNetCore.SignalR;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Responses.Entities.Notifications;
using App.Backend.API.Bus.Messages;
using App.Backend.Models.Responses.Entities.Cursus;

namespace App.Backend.API.Notifications.Variants;

// ============================================================================

/// <summary>
/// Notification sent when a user completes a cursus.
/// </summary>
/// <param name="UserId"></param>
/// <param name="UserLogin"></param>
/// <param name="UserCursusId"></param>
/// <param name="CursusName"></param>
public sealed record CursusCompletedNotification(
    Guid UserId,
    string UserLogin,
    Guid UserCursusId,
    string CursusName
) : INotificationMessage, IDatabaseChannel
{
    public Guid? ResourceId => UserCursusId;

    public Guid NotifiableId => UserId;
    public NotificationMeta Meta => NotificationMeta.User | NotificationMeta.Feed | NotificationMeta.Cursus | NotificationMeta.Completed;

    public NotificationData ToDatabase() => new MessageDO(
        $"# Congratulations, {UserLogin}!\nYour cursus '{CursusName}' has been completed."
    );
}
