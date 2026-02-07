// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using Bogus;

// ============================================================================

namespace App.Backend.Tests.Fixtures.Factory;

/// <summary>
/// Factory for creating fake Comment entities.
/// </summary>
public static class CommentFactory
{
    /// <summary>
    /// Creates a Faker for Comment entities.
    /// </summary>
    public static Faker<Comment> Create(Guid? userId = null, Guid? entityId = null) => new Faker<Comment>()
        .RuleFor(c => c.Id, f => f.Random.Guid())
        .RuleFor(c => c.UserId, f => userId ?? f.Random.Guid())
        .RuleFor(c => c.EntityId, f => entityId ?? f.Random.Guid())
        .RuleFor(c => c.EntityType, f => f.PickRandom("Review", "Project", "Goal"))
        .RuleFor(c => c.Body, f => f.Lorem.Paragraph())
        .RuleFor(c => c.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(c => c.UpdatedAt, (f, c) => c.CreatedAt);
}
