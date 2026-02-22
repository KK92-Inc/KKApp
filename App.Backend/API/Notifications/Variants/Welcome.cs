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

namespace App.Backend.API.Notifications.Variants;

// ============================================================================

// Change 'class' to 'record'
// Capitalize 'User' to follow property naming conventions
public sealed record WelcomeUserNotification(UserDO User) : INotificationMessage, IBroadcastChannel, IDatabaseChannel
{
    public Guid? ResourceId => null;

    public Guid NotifiableId => User.Id;
    public NotificationMeta Meta => NotificationMeta.User | NotificationMeta.Feed | NotificationMeta.Welcome;

    public BroadcastMessage ToBroadcast() => new("DemoEvent", User);

    public NotificationData ToDatabase() => new MessageDO(
        $"# Welcome, {User.Login}!\nYour account is ready."
    );
}
