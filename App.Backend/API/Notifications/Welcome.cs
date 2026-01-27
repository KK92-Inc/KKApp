// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using App.Backend.API.Views.Models;
using App.Backend.API.Notifications.Channels;

namespace App.Backend.API.Notifications;

// ============================================================================

/// <summary>
/// Notification for when a user is newly registered.
/// </summary>
/// <param name="UserId">The user ID</param>
public sealed record WelcomeUserNotification(Guid UserId, string FirstName, string LastName) :
    BaseNotification,
    IDatabaseChannel,
    IEmailChannel<WelcomeViewModel>
{
    public string View => "~/Views/Welcome.cshtml";

    public string Subject => $"Welcome {FirstName} {LastName}";


    public WelcomeViewModel Model => new (FirstName, LastName);

    public override Guid NotifiableId => UserId;

    public override NotificationVariant Descriptor => NotificationVariant.Feed | NotificationVariant.User;

}
