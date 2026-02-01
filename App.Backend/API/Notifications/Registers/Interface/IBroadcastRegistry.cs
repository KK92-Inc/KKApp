// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.API.Bus.Messages;

namespace App.Backend.API.Notifications.Registers.Interface;

// ============================================================================

/// <summary>
/// A Pub/Sub registry useful for SSE
/// </summary>
public interface IBroadcastRegistry
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="notifiableId"></param>
    /// <param name="message"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task PublishAsync(Guid notifiableId, BroadcastMessage message, CancellationToken token = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="notifiableId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    IAsyncEnumerable<BroadcastMessage> SubscribeAsync(Guid notifiableId, CancellationToken token = default);
}
