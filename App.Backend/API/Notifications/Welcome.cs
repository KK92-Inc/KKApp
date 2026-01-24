// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities.Notifications;

// ============================================================================

public sealed record WelcomeNotification(Guid UserId) : BaseNotification
{
    public override Guid NotifiableId => UserId;

    public override NotificationData? Data => null;

    public override NotificationVariant Descriptor => NotificationVariant.Welcome | NotificationVariant.Default;
}
