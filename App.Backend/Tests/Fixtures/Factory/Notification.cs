// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;
using Bogus;

// ============================================================================

namespace App.Backend.Tests.Fixtures.Factory;

/// <summary>
/// Factory for creating fake Notification entities.
/// </summary>
public static class NotificationFactory
{
    /// <summary>
    /// Creates a Faker for Notification entities.
    /// </summary>
    public static Faker<Notification> Create(Guid? notifiableId = null) => new Faker<Notification>()
        .RuleFor(n => n.Id, f => f.Random.Guid())
        .RuleFor(n => n.NotifiableId, f => notifiableId ?? f.Random.Guid())
        .RuleFor(n => n.Type, f => nameof(Notification))
        .RuleFor(n => n.Descriptor, f => f.PickRandom<NotificationVariant>())
        .RuleFor(n => n.Data, f => "{}")
        .RuleFor(n => n.ReadAt, f => null)
        .RuleFor(n => n.ResourceId, f => f.Random.Bool() ? f.Random.Guid() : null)
        .RuleFor(n => n.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(n => n.UpdatedAt, (f, n) => n.CreatedAt);
}
