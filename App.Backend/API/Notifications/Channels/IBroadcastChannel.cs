// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Notifications.Channels;

/// <summary>
/// Describes a SignalR broadcast event.
/// </summary>
public interface IBroadcastMessage
{
    /// <summary>
    /// The SignalR method name (event identifier).
    /// </summary>
    string Event { get; }

    /// <summary>
    /// Payload sent to connected clients.
    /// </summary>
    object Payload { get; }
}

/// <summary>
/// Default broadcast message implementation.
/// </summary>
/// <typeparam name="T">Payload type.</typeparam>
public sealed record BroadcastMessage(string Event, object Payload) : IBroadcastMessage;

/// <summary>
/// Marks a notification as eligible for real-time broadcasting.
/// </summary>
public interface IBroadcastChannel
{
    /// <summary>
    /// Creates the broadcast message for this notification.
    /// </summary>
    IBroadcastMessage ToBroadcast();
}
