// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using Bogus;

// ============================================================================

namespace App.Backend.Tests.Fixtures.Factory;

/// <summary>
/// Factory for creating fake Goal entities.
/// </summary>
public static class GoalFactory
{
    /// <summary>
    /// Creates a Faker for Goal entities.
    /// </summary>
    public static Faker<Goal> Create() => new Faker<Goal>()
        .RuleFor(g => g.Id, f => f.Random.Guid())
        .RuleFor(g => g.Name, f => f.Hacker.Phrase())
        .RuleFor(g => g.Description, f => f.Lorem.Paragraph())
        .RuleFor(g => g.Slug, (f, g) => f.Random.Guid().ToString()[..8])
        .RuleFor(g => g.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(g => g.UpdatedAt, (f, g) => g.CreatedAt);
}
