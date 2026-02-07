// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Projects;
using App.Backend.Domain.Enums;
using Bogus;

// ============================================================================

namespace App.Backend.Tests.Fixtures.Factory;

/// <summary>
/// Factory for creating fake UserProjectMember and UserProjectTransaction entities.
/// </summary>
public static class UserProjectFactory
{
    /// <summary>
    /// Creates a Faker for UserProjectMember entities.
    /// </summary>
    public static Faker<UserProjectMember> CreateMember(Guid? userProjectId = null, Guid? userId = null) => new Faker<UserProjectMember>()
        .RuleFor(m => m.Id, f => f.Random.Guid())
        .RuleFor(m => m.UserProjectId, f => userProjectId ?? f.Random.Guid())
        .RuleFor(m => m.UserId, f => userId ?? f.Random.Guid())
        .RuleFor(m => m.Role, f => f.PickRandom<UserProjectRole>())
        .RuleFor(m => m.LeftAt, f => null)
        .RuleFor(m => m.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(m => m.UpdatedAt, (f, m) => m.CreatedAt);

    /// <summary>
    /// Creates a Faker for UserProjectTransaction entities.
    /// </summary>
    public static Faker<UserProjectTransaction> CreateTransaction(Guid? userProjectId = null, Guid? userId = null) => new Faker<UserProjectTransaction>()
        .RuleFor(t => t.Id, f => f.Random.Guid())
        .RuleFor(t => t.UserProjectId, f => userProjectId ?? f.Random.Guid())
        .RuleFor(t => t.UserId, f => userId)
        .RuleFor(t => t.Type, f => f.PickRandom<UserProjectTransactionVariant>())
        .RuleFor(t => t.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(t => t.UpdatedAt, (f, t) => t.CreatedAt);
}
