// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using Wolverine;

namespace App.Backend.API.Notifications;

// ============================================================================

/// <summary>
/// Interface for defining notification messages.
/// </summary>
public interface INotificationMessage
{
    public Guid NotifiableId { get; }
    public Guid? ResourceId { get; }
    public NotificationMeta Meta { get; }
}
