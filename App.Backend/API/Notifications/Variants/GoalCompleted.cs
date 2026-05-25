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

public sealed record GoalCompleted(UserDO User, UserGoalDO UserGoal) : INotificationMessage, IDatabaseChannel
{
    public Guid? ResourceId => UserGoal.Id;

    public Guid NotifiableId => User.Id;
    public NotificationMeta Meta => NotificationMeta.User | NotificationMeta.Feed | NotificationMeta.Goal;

    public NotificationData ToDatabase() => new MessageDO(
        $"# Congratulations, {User.Login}!\nYour goal '{UserGoal.Goal.Name}' has been completed."
    );
}
