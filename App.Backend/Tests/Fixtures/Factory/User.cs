// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using Bogus;

// ============================================================================

namespace App.Backend.Tests.Fixtures.Factory;

/// <summary>
/// Factory for creating fake User entities and related user data.
/// </summary>
public static class UserFactory
{
    /// <summary>
    /// Creates a Faker for User entities.
    /// </summary>
    public static Faker<User> Create() => new Faker<User>()
        .RuleFor(u => u.Id, f => f.Random.Guid())
        .RuleFor(u => u.Login, f => f.Internet.UserName().ToLowerInvariant())
        .RuleFor(u => u.Display, f => f.Name.FullName())
        .RuleFor(u => u.AvatarUrl, f => f.Internet.Avatar())
        .RuleFor(u => u.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(u => u.UpdatedAt, (f, u) => u.CreatedAt);

    /// <summary>
    /// Creates a Faker for Details entities.
    /// </summary>
    public static Faker<Details> CreateDetails(Guid? userId = null) => new Faker<Details>()
        .RuleFor(d => d.Id, f => f.Random.Guid())
        .RuleFor(d => d.UserId, f => userId ?? f.Random.Guid())
        .RuleFor(d => d.Email, f => f.Internet.Email())
        .RuleFor(d => d.Markdown, f => f.Lorem.Paragraphs(2))
        .RuleFor(d => d.FirstName, f => f.Name.FirstName())
        .RuleFor(d => d.LastName, f => f.Name.LastName())
        .RuleFor(d => d.GithubUrl, f => $"https://github.com/{f.Internet.UserName()}")
        .RuleFor(d => d.LinkedinUrl, f => $"https://linkedin.com/in/{f.Internet.UserName()}")
        .RuleFor(d => d.RedditUrl, f => $"https://reddit.com/u/{f.Internet.UserName()}")
        .RuleFor(d => d.WebsiteUrl, f => f.Internet.Url())
        .RuleFor(d => d.EnabledNotifications, f => NotifiableVariant.All)
        .RuleFor(d => d.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(d => d.UpdatedAt, (f, d) => d.CreatedAt);

    /// <summary>
    /// Creates a Faker for UserCursus entities.
    /// </summary>
    public static Faker<UserCursus> CreateUserCursus(Guid? userId = null, Guid? cursusId = null) => new Faker<UserCursus>()
        .RuleFor(uc => uc.Id, f => f.Random.Guid())
        .RuleFor(uc => uc.UserId, f => userId ?? f.Random.Guid())
        .RuleFor(uc => uc.CursusId, f => cursusId ?? f.Random.Guid())
        .RuleFor(uc => uc.State, f => f.PickRandom<EntityObjectState>())
        .RuleFor(uc => uc.Track, f => null)
        .RuleFor(uc => uc.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(uc => uc.UpdatedAt, (f, uc) => uc.CreatedAt);

    /// <summary>
    /// Creates a Faker for UserGoal entities.
    /// </summary>
    public static Faker<UserGoal> CreateUserGoal(Guid? userId = null, Guid? goalId = null) => new Faker<UserGoal>()
        .RuleFor(ug => ug.Id, f => f.Random.Guid())
        .RuleFor(ug => ug.UserId, f => userId ?? f.Random.Guid())
        .RuleFor(ug => ug.GoalId, f => goalId ?? f.Random.Guid())
        .RuleFor(ug => ug.State, f => f.PickRandom<EntityObjectState>())
        .RuleFor(ug => ug.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(ug => ug.UpdatedAt, (f, ug) => ug.CreatedAt);

    /// <summary>
    /// Creates a Faker for UserProject entities.
    /// </summary>
    public static Faker<UserProject> CreateUserProject(Guid? projectId = null) => new Faker<UserProject>()
        .RuleFor(up => up.Id, f => f.Random.Guid())
        .RuleFor(up => up.ProjectId, f => projectId ?? f.Random.Guid())
        .RuleFor(up => up.State, f => f.PickRandom<EntityObjectState>())
        .RuleFor(up => up.GitInfoId, f => null)
        .RuleFor(up => up.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(up => up.UpdatedAt, (f, up) => up.CreatedAt);
}
