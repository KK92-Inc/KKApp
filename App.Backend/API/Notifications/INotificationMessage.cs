// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using Wolverine;

namespace App.Backend.API.Notifications;

// ============================================================================

/// <summary>
/// Wrapper for INotificationMessage which allows *any* variant of it to be
/// discovered by WolverineFX.
/// </summary>
/// <param name="Content">The Notification to deliver.</param>
public record NotificationRequest(INotificationMessage Content);

// ============================================================================

/// <summary>
/// Interface for defining notification messages.
/// </summary>
public interface INotificationMessage : IForwardsTo<NotificationRequest>
{
    Guid NotifiableId { get; }
    NotificationMeta Meta { get; }
    Guid? ResourceId { get; }
}
