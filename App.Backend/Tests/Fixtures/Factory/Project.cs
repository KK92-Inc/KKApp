// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using Bogus;

// ============================================================================

namespace App.Backend.Tests.Fixtures.Factory;

/// <summary>
/// Factory for creating fake Project entities.
/// </summary>
public static class ProjectFactory
{
    /// <summary>
    /// Creates a Faker for Project entities.
    /// </summary>
    public static Faker<Project> Create() => new Faker<Project>()
        .RuleFor(p => p.Id, f => f.Random.Guid())
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Description, f => f.Lorem.Paragraph())
        .RuleFor(p => p.Slug, (f, p) => f.Random.Guid().ToString()[..8])
        .RuleFor(p => p.Active, f => true)
        .RuleFor(p => p.Public, f => true)
        .RuleFor(p => p.Deprecated, f => false)
        .RuleFor(p => p.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(p => p.UpdatedAt, (f, p) => p.CreatedAt);
}
