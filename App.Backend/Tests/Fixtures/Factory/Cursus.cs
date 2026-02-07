// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using Bogus;

// ============================================================================

namespace App.Backend.Tests.Fixtures.Factory;

/// <summary>
/// Factory for creating fake Cursus entities.
/// </summary>
public static class CursusFactory
{
    /// <summary>
    /// Creates a Faker for Cursus entities.
    /// </summary>
    public static Faker<Cursus> Create() => new Faker<Cursus>()
        .RuleFor(c => c.Id, f => f.Random.Guid())
        .RuleFor(c => c.Name, f => f.Company.CatchPhrase())
        .RuleFor(c => c.Description, f => f.Lorem.Paragraph())
        .RuleFor(c => c.Slug, (f, c) => f.Random.Guid().ToString()[..8])
        .RuleFor(c => c.Track, f => null)
        .RuleFor(c => c.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(c => c.UpdatedAt, (f, c) => c.CreatedAt);
}
