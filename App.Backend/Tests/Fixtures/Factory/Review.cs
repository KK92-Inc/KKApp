// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Rules;
using Bogus;

// ============================================================================

namespace App.Backend.Tests.Fixtures.Factory;

/// <summary>
/// Factory for creating fake Review and Rubric entities.
/// </summary>
public static class ReviewFactory
{
    /// <summary>
    /// Creates a Faker for Review entities.
    /// </summary>
    public static Faker<Review> Create(
        Guid? userProjectId = null,
        Guid? rubricId = null,
        Guid? reviewerId = null) => new Faker<Review>()
        .RuleFor(r => r.Id, f => f.Random.Guid())
        .RuleFor(r => r.UserProjectId, f => userProjectId ?? f.Random.Guid())
        .RuleFor(r => r.RubricId, f => rubricId ?? f.Random.Guid())
        .RuleFor(r => r.ReviewerId, f => reviewerId)
        .RuleFor(r => r.Kind, f => f.PickRandom<ReviewVariant>())
        .RuleFor(r => r.State, f => f.PickRandom<ReviewState>())
        .RuleFor(r => r.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(r => r.UpdatedAt, (f, r) => r.CreatedAt);

    /// <summary>
    /// Creates a Faker for Rubric entities.
    /// </summary>
    public static Faker<Rubric> CreateRubric(
        Guid? creatorId = null,
        Guid? gitInfoId = null,
        ReviewKinds? supportedKinds = null) => new Faker<Rubric>()
        .RuleFor(r => r.Id, f => f.Random.Guid())
        .RuleFor(r => r.Name, f => f.Commerce.ProductName())
        .RuleFor(r => r.Slug, (f, r) => f.Internet.UserName().ToLowerInvariant().Replace(" ", "-"))
        .RuleFor(r => r.Markdown, f => f.Lorem.Paragraphs(3))
        .RuleFor(r => r.Public, f => f.Random.Bool())
        .RuleFor(r => r.Enabled, f => true)
        .RuleFor(r => r.SupportedReviewKinds, f => supportedKinds ?? ReviewKinds.All)
        .RuleFor(r => r.CreatorId, f => creatorId ?? f.Random.Guid())
        .RuleFor(r => r.GitInfoId, f => gitInfoId)
        .RuleFor(r => r.ReviewerEligibilityRules, f => new List<Rule>())
        .RuleFor(r => r.RevieweeEligibilityRules, f => new List<Rule>())
        .RuleFor(r => r.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(r => r.UpdatedAt, (f, r) => r.CreatedAt);
}
