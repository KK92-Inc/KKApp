// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Relations;
using Bogus;

// ============================================================================

namespace App.Backend.Tests.Fixtures.Factory;

/// <summary>
/// Factory for creating fake relation entities.
/// </summary>
public static class RelationFactory
{
    /// <summary>
    /// Creates a Faker for GoalProject relations.
    /// </summary>
    public static Faker<GoalProject> CreateGoalProject(Guid? goalId = null, Guid? projectId = null) => new Faker<GoalProject>()
        .RuleFor(gp => gp.GoalId, f => goalId ?? f.Random.Guid())
        .RuleFor(gp => gp.ProjectId, f => projectId ?? f.Random.Guid())
        .RuleFor(gp => gp.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(gp => gp.UpdatedAt, (f, gp) => gp.CreatedAt);

    /// <summary>
    /// Creates a Faker for CursusGoal relations.
    /// </summary>
    public static Faker<CursusGoal> CreateCursusGoal(
        Guid? cursusId = null,
        Guid? goalId = null,
        Guid? parentGoalId = null,
        Guid? choiceGroup = null) => new Faker<CursusGoal>()
        .RuleFor(cg => cg.CursusId, f => cursusId ?? f.Random.Guid())
        .RuleFor(cg => cg.GoalId, f => goalId ?? f.Random.Guid())
        .RuleFor(cg => cg.ParentGoalId, f => parentGoalId)
        .RuleFor(cg => cg.ChoiceGroup, f => choiceGroup)
        .RuleFor(cg => cg.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(cg => cg.UpdatedAt, (f, cg) => cg.CreatedAt);
}
