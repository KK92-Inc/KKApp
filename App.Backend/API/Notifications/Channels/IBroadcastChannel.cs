// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.API.Bus.Messages;

namespace App.Backend.API.Notifications.Channels;

/// <summary>
/// Represents a channel capable of broadcasting messages.
/// </summary>
public interface IBroadcastChannel
{
    /// <summary>
    /// Converts the current notification into a <see cref="BroadcastMessage"/> for broadcasting.
    /// </summary>
    /// <returns>A <see cref="BroadcastMessage"/> representing the broadcastable message.</returns>
    public BroadcastMessage ToBroadcast();
}
