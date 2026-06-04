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
using App.Backend.Models.Responses.Entities.Projects;

namespace App.Backend.API.Notifications.Variants;

// ============================================================================

/// <summary>
/// Represents a notification for when a project is completed.
/// </summary>
/// <param name="UserId"></param>
/// <param name="UserLogin"></param>
/// <param name="UserProjectId"></param>
/// <param name="ProjectName"></param>
public sealed record ProjectCompletedNotification(
    Guid UserId,
    string UserLogin,
    Guid UserProjectId,
    string ProjectName
) : INotificationMessage, IDatabaseChannel
{
    public Guid? ResourceId => UserProjectId;

    public Guid NotifiableId => UserId;
    public NotificationMeta Meta => NotificationMeta.User | NotificationMeta.Feed | NotificationMeta.Project;

    public NotificationData ToDatabase() => new MessageDO(
        $"# Congratulations, {UserLogin}!\nYour project '{ProjectName}' has been completed."
    );
}
