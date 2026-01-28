// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Notifications.Channels;

/// <summary>
/// Interface that mandates that the notification is to be sent as plain text.
/// </summary>
/// <remarks>These notifications can be viewed on the notification dashboard.</remarks>
public interface IDatabaseChannel
{
    //TODO: Use JSON polymorphic
    public object ToDatabase();
}
