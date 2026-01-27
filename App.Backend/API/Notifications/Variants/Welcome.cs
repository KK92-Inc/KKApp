// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using App.Backend.API.Views.Models;
using App.Backend.API.Notifications.Channels;
using Wolverine;
using System.Net.Mail;
using App.Backend.Domain.Entities.Users;

namespace App.Backend.API.Notifications.Variants;

// ============================================================================

public sealed record WelcomeUserNotification(User User) : INotificationMessage, IDatabaseChannel, IEmailChannel
{
    public Guid NotifiableId => User.Id;

    public NotificationMeta Meta => throw new NotImplementedException();

    public Guid? ResourceId => null;

    public NotificationRequest Transform() => new(this);
}

/// <summary>
/// Notification for when a user is newly registered.
/// </summary>
/// <param name="UserId">The user ID</param>
// public sealed record WelcomeUserNotification(
//     Guid UserId,
//     string FirstName,
//     string LastName
// ) : INotificationMessage,
//     IDatabaseChannel,
//     IEmailChannel
// {
//     public string View => "~/Views/Welcome.cshtml";
//     public string Subject => $"Welcome {FirstName} {LastName}";
//     public WelcomeViewModel Model => new(FirstName, LastName);

//     public Guid NotifiableId => UserId;

//     public Guid? ResourceId => null;

//     public NotificationMeta Meta => NotificationMeta.Feed | NotificationMeta.User;

//     public MailMessage ToMail() => new()
//     {
//         To =
//     };
//     public NotificationRequest Transform() => new(this);
// }
