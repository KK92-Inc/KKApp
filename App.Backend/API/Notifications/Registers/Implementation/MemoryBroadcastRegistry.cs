// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using App.Backend.API.Bus.Messages;
using App.Backend.API.Notifications.Registers.Interface;

namespace App.Backend.API.Notifications.Registers.Implementation;

// ============================================================================

/// <summary>
/// A in memory implementation of a broadcast registry.
/// </summary>
public sealed class MemoryBroadcastRegistry : IBroadcastRegistry
{
    /// <inheritdoc />
    public async Task PublishAsync(Guid notifiableId, BroadcastMessage message, CancellationToken ct = default)
    {
        var channel = _channels.GetOrAdd(notifiableId, _ => Channel.CreateBounded<BroadcastMessage>(_options));
        await channel.Writer.WriteAsync(message, ct);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<BroadcastMessage> SubscribeAsync(Guid notifiableId, [EnumeratorCancellation] CancellationToken ct)
    {
        var channel = _channels.GetOrAdd(notifiableId, _ => Channel.CreateBounded<BroadcastMessage>(_options));
        await foreach (var message in channel.Reader.ReadAllAsync(ct))
            yield return message;
    }

    /// <summary>
    /// Dict for all the user channels
    /// </summary>
    private readonly ConcurrentDictionary<Guid, Channel<BroadcastMessage>> _channels = new();

    /// <summary>
    /// 6 Channels per entity should be more than enough.
    /// </summary>
    private readonly BoundedChannelOptions _options = new(6)
    {
        FullMode = BoundedChannelFullMode.DropOldest
    };
}
