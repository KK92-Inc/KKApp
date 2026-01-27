// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities.Notifications;

namespace App.Backend.API.Notifications;

// ============================================================================

/// <summary>
///
/// </summary>
public abstract record BaseNotification
{
    public virtual Guid NotifiableId { get; init; }

    public virtual NotificationVariant Descriptor { get; }

    // public virtual NotificationData? Data { get; }

    public virtual Guid? ResourceId => null;
}
