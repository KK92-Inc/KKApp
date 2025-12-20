// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;
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
    public static Faker<Review> Create(Guid? userProjectId = null, Guid? reviewerId = null) => new Faker<Review>()
        .RuleFor(r => r.Id, f => f.Random.Guid())
        .RuleFor(r => r.UserProjectId, f => userProjectId ?? f.Random.Guid())
        .RuleFor(r => r.ReviewerId, f => reviewerId)
        .RuleFor(r => r.Kind, f => f.PickRandom<ReviewVariant>())
        .RuleFor(r => r.State, f => f.PickRandom<ReviewState>())
        .RuleFor(r => r.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(r => r.UpdatedAt, (f, r) => r.CreatedAt);

    /// <summary>
    /// Creates a Faker for Rubric entities.
    /// </summary>
    public static Faker<Rubric> CreateRubric(Guid? projectId = null, Guid? creatorId = null, Guid? gitInfoId = null) => new Faker<Rubric>()
        .RuleFor(r => r.Id, f => f.Random.Guid())
        .RuleFor(r => r.Name, f => f.Commerce.ProductName())
        .RuleFor(r => r.Markdown, f => f.Lorem.Paragraphs(3))
        .RuleFor(r => r.Public, f => f.Random.Bool())
        .RuleFor(r => r.Enabled, f => true)
        .RuleFor(r => r.ProjectId, f => projectId ?? f.Random.Guid())
        .RuleFor(r => r.CreatorId, f => creatorId ?? f.Random.Guid())
        .RuleFor(r => r.GitInfoId, f => gitInfoId ?? f.Random.Guid())
        .RuleFor(r => r.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(r => r.UpdatedAt, (f, r) => r.CreatedAt);
}
