// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;

namespace App.Backend.Core.Services.Options;

/// <summary>
/// Configuration options for subscription chain enforcement.
/// </summary>
public class SubscriptionOptions
{
    public const string SectionName = "Subscription";

    /// <summary>
    /// The subscription enforcement mode.
    /// Defaults to <see cref="ProgressionMode.Free"/>.
    /// </summary>
    public ProgressionMode Mode { get; init; } = ProgressionMode.Free;

    /// <summary>
    /// Cooldown period for subscribing/unsubscribing to the same entity.
    /// </summary>
    public TimeSpan Cooldown { get; init; } = TimeSpan.FromHours(6);
}
