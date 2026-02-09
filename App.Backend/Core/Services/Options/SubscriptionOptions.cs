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
    /// Defaults to <see cref="SubscriptionMode.FreeForAll"/>.
    /// </summary>
    public SubscriptionMode Mode { get; set; } = SubscriptionMode.FreeForAll;
}
