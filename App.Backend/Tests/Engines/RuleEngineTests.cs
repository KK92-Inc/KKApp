// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Engines.Evaluations;
using App.Backend.Core.Engines.Evaluations.Enums;
using App.Backend.Core.Engines.Evaluations.Rules;
using App.Backend.Domain;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Rules.Evaluations;
using App.Backend.Domain.Rules.Evaluations.Composites;
using App.Backend.Tests.Fixtures;
using App.Backend.Tests.Fixtures.Factory;

// ============================================================================

namespace App.Backend.Tests.Engines;

public class RuleEngineTests : ServiceTestBase
{
    private readonly RuleEngine _engine;

    public RuleEngineTests()
    {
        _engine = new RuleEngine([
            new HasCursusEvaluator(Context),
            new HasProjectEvaluator(Context),
            new IsMemberEvaluator(Context),
            new MinDaysRegisteredEvaluator(),
            new MinProjectsCompletedEvaluator(Context),
            new MinReviewsCompletedEvaluator(Context),
            new SameTimezoneEvaluator(),
        ]);
    }

    #region MinDaysRegistered Tests

    [Fact]
    public async Task MinDaysRegistered_WhenUserRegisteredLongEnough_ShouldPass()
    {
        // Arrange
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-30))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new MinDaysRegisteredRule { Days = 7 };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MinDaysRegistered_WhenUserTooNew_ShouldFail()
    {
        // Arrange
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-3))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new MinDaysRegisteredRule { Days = 7 };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("at least 7", result.Reasons[0]);
    }

    [Fact]
    public async Task MinDaysRegistered_WithCustomDescription_ShouldUseCustomMessage()
    {
        // Arrange
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-1))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new MinDaysRegisteredRule { Days = 30, Description = "Custom failure message" };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Custom failure message", result.Reasons[0]);
    }

    #endregion

    #region HasCursus Tests

    [Fact]
    public async Task HasCursus_WhenUserEnrolled_ShouldPass()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create()
            .WithContext(Context)
            .With(c => c.Slug, "test-cursus")
            .GenerateAsync();

        var userCursus = UserFactory.CreateUserCursus(user.Id, cursus.Id).Generate();
        Context.UserCursi.Add(userCursus);
        await Context.SaveChangesAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new HasCursusRule { CursusId = cursus.Id };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HasCursus_WhenUserNotEnrolled_ShouldFail()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new HasCursusRule { CursusId = Guid.NewGuid() };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("nonexistent-cursus", result.Reasons[0]);
    }

    #endregion

    #region MinReviewsCompleted Tests

    [Fact]
    public async Task MinReviewsCompleted_WhenUserHasEnoughReviews_ShouldPass()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var userProject = await UserFactory.CreateUserProject(project.Id).WithContext(Context).GenerateAsync();
        var rubric = await ReviewFactory.CreateRubric(creatorId: user.Id).WithContext(Context).GenerateAsync();

        for (int i = 0; i < 5; i++)
        {
            var review = ReviewFactory.Create(userProject.Id, rubric.Id, user.Id).Generate();
            review.State = ReviewState.Finished;
            Context.Reviews.Add(review);
        }
        await Context.SaveChangesAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new MinReviewsCompletedRule { Count = 3 };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MinReviewsCompleted_WhenUserHasNotEnoughReviews_ShouldFail()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new MinReviewsCompletedRule { Count = 5 };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("at least 5", result.Reasons[0]);
    }

    #endregion

    #region HasProject Tests

    [Fact]
    public async Task HasProject_WhenUserCompletedProject_ShouldPass()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create()
            .WithContext(Context)
            .With(p => p.Slug, "required-project")
            .GenerateAsync();
        var userProject = await UserFactory.CreateUserProject(project.Id)
            .WithContext(Context)
            .With(up => up.State, EntityObjectState.Completed)
            .GenerateAsync();

        var member = UserProjectFactory.CreateMember(userProject.Id, user.Id).Generate();
        Context.UserProjectMembers.Add(member);
        await Context.SaveChangesAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new HasProjectRule { ProjectId = project.Id };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HasProject_WhenUserNotCompletedProject_ShouldFail()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create()
            .WithContext(Context)
            .With(p => p.Slug, "required-project")
            .GenerateAsync();
        var userProject = await UserFactory.CreateUserProject(project.Id)
            .WithContext(Context)
            .With(up => up.State, EntityObjectState.Active)
            .GenerateAsync();

        var member = UserProjectFactory.CreateMember(userProject.Id, user.Id).Generate();
        Context.UserProjectMembers.Add(member);
        await Context.SaveChangesAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new HasProjectRule { ProjectId = project.Id };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.False(result.IsSuccess);
    }

    #endregion

    #region IsMember Tests

    [Fact]
    public async Task IsMember_WhenUserIsMemberOfProject_ShouldPass()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var userProject = await UserFactory.CreateUserProject(project.Id).WithContext(Context).GenerateAsync();

        var member = UserProjectFactory.CreateMember(userProject.Id, user.Id).Generate();
        Context.UserProjectMembers.Add(member);
        await Context.SaveChangesAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer, SubjectProject = userProject };
        var rule = new IsMemberRule();

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task IsMember_WhenNoSubjectProject_ShouldSkip()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer, SubjectProject = null };
        var rule = new IsMemberRule();

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.IsSkipped);
    }

    #endregion

    #region MinProjectsCompleted Tests

    [Fact]
    public async Task MinProjectsCompleted_WhenUserHasEnoughProjects_ShouldPass()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();

        for (int i = 0; i < 5; i++)
        {
            var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
            var userProject = await UserFactory.CreateUserProject(project.Id)
                .WithContext(Context)
                .With(up => up.State, EntityObjectState.Completed)
                .GenerateAsync();

            var member = UserProjectFactory.CreateMember(userProject.Id, user.Id).Generate();
            Context.UserProjectMembers.Add(member);
        }
        await Context.SaveChangesAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new MinProjectsCompletedRule { Count = 3 };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MinProjectsCompleted_WhenUserHasNotEnoughProjects_ShouldFail()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new MinProjectsCompletedRule { Count = 3 };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("at least 3", result.Reasons[0]);
    }

    #endregion

    #region SameTimezone Tests

    [Fact]
    public async Task SameTimezone_ShouldSkip_BecauseNotImplemented()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new SameTimezoneRule();

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.IsSkipped);
        Assert.Contains("not yet implemented", result.Warnings[0]);
    }

    #endregion

    #region Composite Rule Tests

    [Fact]
    public async Task AllOf_WhenAllRulesPass_ShouldPass()
    {
        // Arrange
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-30))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new AllOfRule
        {
            Rules = [
                new MinDaysRegisteredRule { Days = 7 },
                new MinDaysRegisteredRule { Days = 14 }
            ]
        };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AllOf_WhenOneRuleFails_ShouldFail()
    {
        // Arrange
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-10))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new AllOfRule
        {
            Rules = [
                new MinDaysRegisteredRule { Days = 7 },
                new MinDaysRegisteredRule { Days = 14 }
            ]
        };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Reasons);
    }

    [Fact]
    public async Task AllOf_WhenMultipleRulesFail_ShouldCollectAllFailures()
    {
        // Arrange
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-3))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new AllOfRule
        {
            Rules = [
                new MinDaysRegisteredRule { Days = 7 },
                new MinDaysRegisteredRule { Days = 14 }
            ]
        };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Reasons.Count);
    }

    [Fact]
    public async Task AnyOf_WhenOneRulePasses_ShouldPass()
    {
        // Arrange
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-10))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new AnyOfRule
        {
            Rules = [
                new MinDaysRegisteredRule { Days = 30 },
                new MinDaysRegisteredRule { Days = 7 }
            ]
        };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AnyOf_WhenAllRulesFail_ShouldFail()
    {
        // Arrange
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-3))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new AnyOfRule
        {
            Rules = [
                new MinDaysRegisteredRule { Days = 7 },
                new MinDaysRegisteredRule { Days = 14 }
            ]
        };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Not_WhenInnerRulePasses_ShouldFail()
    {
        // Arrange
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-30))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new NotRule { Rule = new MinDaysRegisteredRule { Days = 7 } };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Not_WhenInnerRuleFails_ShouldPass()
    {
        // Arrange
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-3))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new NotRule { Rule = new MinDaysRegisteredRule { Days = 7 } };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Not_WhenInnerRuleIsSkipped_ShouldStaySkipped()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new NotRule { Rule = new SameTimezoneRule() };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.IsSkipped);
    }

    #endregion

    #region Complex Nested Rule Tests

    [Fact]
    public async Task NestedRules_ComplexScenario_ShouldEvaluateCorrectly()
    {
        // Arrange: User must be registered for 7 days AND (have 3 projects OR have 5 reviews)
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-30))
            .Generate();

        for (int i = 0; i < 4; i++)
        {
            var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
            var userProject = await UserFactory.CreateUserProject(project.Id)
                .WithContext(Context)
                .With(up => up.State, EntityObjectState.Completed)
                .GenerateAsync();

            var member = UserProjectFactory.CreateMember(userProject.Id, user.Id).Generate();
            Context.UserProjectMembers.Add(member);
        }
        await Context.SaveChangesAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new AllOfRule
        {
            Rules = [
                new MinDaysRegisteredRule { Days = 7 },
                new AnyOfRule
                {
                    Rules = [
                        new MinProjectsCompletedRule { Count = 3 },
                        new MinReviewsCompletedRule { Count = 5 }
                    ]
                }
            ]
        };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task EvaluateAll_ShouldEvaluateAllRulesAndCombineResults()
    {
        // Arrange
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-3))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rules = new List<Rule>
        {
            new MinDaysRegisteredRule { Days = 7 },
            new MinProjectsCompletedRule { Count = 1 }
        };

        // Act
        var result = await _engine.EvaluateAllAsync(rules, ctx);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Reasons.Count);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task EmptyRuleList_InAllOf_ShouldPass()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new AllOfRule { Rules = [] };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task EmptyRuleList_InAnyOf_ShouldPass()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();

        var ctx = new Context { User = user, Role = Role.Reviewer };
        var rule = new AnyOfRule { Rules = [] };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Branch Rule Tests

    [Fact]
    public async Task Branch_WhenConditionPasses_ShouldEvaluateThen()
    {
        // Arrange: User registered 30 days ago
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-30))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };

        // If registered >= 7 days, require 14 days; else require 3 days
        var rule = new BranchRule
        {
            Condition = new MinDaysRegisteredRule { Days = 7 },
            Then = new MinDaysRegisteredRule { Days = 14 },
            Else = new MinDaysRegisteredRule { Days = 3 }
        };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert: Condition passes (30 >= 7), Then evaluated (30 >= 14), should pass
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Branch_WhenConditionFails_ShouldEvaluateElse()
    {
        // Arrange: User registered 5 days ago
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-5))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };

        // If registered >= 7 days, require 14 days; else require 3 days
        var rule = new BranchRule
        {
            Condition = new MinDaysRegisteredRule { Days = 7 },
            Then = new MinDaysRegisteredRule { Days = 14 },
            Else = new MinDaysRegisteredRule { Days = 3 }
        };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert: Condition fails (5 < 7), Else evaluated (5 >= 3), should pass
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Branch_WhenConditionPassesButThenFails_ShouldFail()
    {
        // Arrange: User registered 10 days ago
        var user = UserFactory.Create()
            .WithContext(Context)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow.AddDays(-10))
            .Generate();

        var ctx = new Context { User = user, Role = Role.Reviewer };

        // If registered >= 7 days, require 14 days; else require 3 days
        var rule = new BranchRule
        {
            Condition = new MinDaysRegisteredRule { Days = 7 },
            Then = new MinDaysRegisteredRule { Days = 14 },
            Else = new MinDaysRegisteredRule { Days = 3 }
        };

        // Act
        var result = await _engine.EvaluateAsync(rule, ctx);

        // Assert: Condition passes (10 >= 7), Then evaluated (10 < 14), should fail
        Assert.False(result.IsSuccess);
    }

    #endregion
}
