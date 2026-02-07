// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using Bogus;

// ============================================================================

namespace App.Backend.Tests.Fixtures.Factory;

/// <summary>
/// Factory for creating fake Spotlight and SpotlightDismissal entities.
/// </summary>
public static class SpotlightFactory
{
    /// <summary>
    /// Creates a Faker for Spotlight entities.
    /// </summary>
    public static Faker<Spotlight> Create() => new Faker<Spotlight>()
        .RuleFor(s => s.Id, f => f.Random.Guid())
        .RuleFor(s => s.Title, f => f.Commerce.ProductName())
        .RuleFor(s => s.Description, f => f.Lorem.Sentence(10))
        .RuleFor(s => s.ActionText, f => f.PickRandom("Learn More", "Register Now", "View Details", "Get Started"))
        .RuleFor(s => s.Href, f => f.Internet.Url())
        .RuleFor(s => s.BackgroundUrl, f => f.Image.PicsumUrl())
        .RuleFor(s => s.StartsAt, f => f.Date.PastOffset(7))
        .RuleFor(s => s.EndsAt, f => f.Date.FutureOffset(30))
        .RuleFor(s => s.IsActive, f => true)
        .RuleFor(s => s.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(s => s.UpdatedAt, (f, s) => s.CreatedAt);

    /// <summary>
    /// Creates a Faker for SpotlightDismissal entities.
    /// </summary>
    public static Faker<SpotlightDismissal> CreateDismissal(Guid? userId = null, Guid? spotlightId = null) => new Faker<SpotlightDismissal>()
        .RuleFor(sd => sd.UserId, f => userId ?? f.Random.Guid())
        .RuleFor(sd => sd.SpotlightId, f => spotlightId ?? f.Random.Guid())
        .RuleFor(sd => sd.DismissedAt, f => f.Date.RecentOffset(7));
}
