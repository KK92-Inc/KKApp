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
/// Factory for creating fake Git entities.
/// </summary>
public static class GitFactory
{
    /// <summary>
    /// Creates a Faker for Git entities.
    /// </summary>
    public static Faker<Git> Create() => new Faker<Git>()
        .RuleFor(g => g.Id, f => f.Random.Guid())
        .RuleFor(g => g.Name, f => f.Hacker.Noun().ToLowerInvariant().Replace(" ", "-"))
        .RuleFor(g => g.Owner, f => f.Internet.UserName().ToLowerInvariant())
        .RuleFor(g => g.Name, f => f.Internet.UserName().ToLowerInvariant())
        .RuleFor(g => g.Ownership, f => f.PickRandom<EntityOwnership>())
        .RuleFor(g => g.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(g => g.UpdatedAt, (f, g) => g.CreatedAt);
}
