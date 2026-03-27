// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core;
using App.Backend.Core.Services.Implementation;
using App.Backend.Core.Services.Options;
using App.Backend.Domain.Enums;
using App.Backend.Tests.Fixtures;
using App.Backend.Tests.Fixtures.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// ============================================================================

namespace App.Backend.Tests.Services;

public class SubscriptionServiceV2Tests : ServiceTestBase
{
    private SubscriptionService CreateService(ProgressionMode mode = ProgressionMode.Restricted)
    {
        var options = Options.Create(new SubscriptionOptions { Mode = mode });
        return new SubscriptionService(Context, options);
    }

    #region Free Mode Tests

    [Fact]
    public async Task CanSubscribeToGoal_In_Free_Mode_Should_Always_Return_True()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Free);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToGoalAsync(user.Id, goal.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Orphan Goal Tests

    [Fact]
    public async Task CanSubscribeToGoal_Orphan_Goal_Should_Return_True()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        // Goal is not linked to any cursus (orphan)

        // Act
        var result = await sut.CanSubscribeToGoalAsync(user.Id, goal.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Already Subscribed Tests

    [Fact]
    public async Task CanSubscribeToGoal_Already_Active_Should_Return_False()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        // Create existing subscription
        await UserFactory.CreateUserGoal(user.Id, goal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToGoalAsync(user.Id, goal.Id, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanSubscribeToGoal_Already_Completed_Should_Return_False()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        await UserFactory.CreateUserGoal(user.Id, goal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Completed)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToGoalAsync(user.Id, goal.Id, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Cursus With No Goals

    [Fact]
    public async Task CanSubscribeToGoal_Cursus_With_No_Goals_User_Not_Enrolled_Should_Return_False()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        // Link goal to cursus (root goal, no parent)
        await RelationFactory.CreateCursusGoal(cursus.Id, goal.Id)
            .WithContext(Context)
            .GenerateAsync();

        // User is NOT enrolled in cursus

        // Act
        var result = await sut.CanSubscribeToGoalAsync(user.Id, goal.Id, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Cursus With 1 Goal No Project

    [Fact]
    public async Task CanSubscribeToGoal_Cursus_With_1_Goal_No_Project_User_Enrolled_Should_Return_True()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        // Link goal to cursus as root
        await RelationFactory.CreateCursusGoal(cursus.Id, goal.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Enroll user in cursus
        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToGoalAsync(user.Id, goal.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanSubscribeToGoal_Cursus_With_1_Goal_No_Project_User_Not_Enrolled_Should_Return_False()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        // Link goal to cursus as root
        await RelationFactory.CreateCursusGoal(cursus.Id, goal.Id)
            .WithContext(Context)
            .GenerateAsync();

        // User is NOT enrolled in cursus

        // Act
        var result = await sut.CanSubscribeToGoalAsync(user.Id, goal.Id, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Cursus With 1 Goal and 1 Project

    [Fact]
    public async Task CanSubscribeToGoal_Cursus_With_1_Goal_1_Project_User_Enrolled_Should_Return_True()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Link goal to cursus
        await RelationFactory.CreateCursusGoal(cursus.Id, goal.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Link project to goal
        await RelationFactory.CreateGoalProject(goal.Id, project.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Enroll user in cursus
        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToGoalAsync(user.Id, goal.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Cursus With 2 Goals and 2 Projects (Hierarchical)

    [Fact]
    public async Task CanSubscribeToGoal_Child_Goal_When_Parent_Not_Completed_Should_Return_False()
    {
        // Arrange
        // Structure: Cursus -> Goal1 (root) -> Goal2 (child of Goal1)
        //                        |                |
        //                     Project1         Project2
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var goal1 = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var goal2 = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var project1 = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var project2 = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Link goals to cursus (goal1 is root, goal2 has goal1 as parent)
        await RelationFactory.CreateCursusGoal(cursus.Id, goal1.Id)
            .WithContext(Context)
            .GenerateAsync();
        await RelationFactory.CreateCursusGoal(cursus.Id, goal2.Id, parentGoalId: goal1.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Link projects to goals
        await RelationFactory.CreateGoalProject(goal1.Id, project1.Id)
            .WithContext(Context)
            .GenerateAsync();
        await RelationFactory.CreateGoalProject(goal2.Id, project2.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Enroll user in cursus
        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // User has NOT completed goal1

        // Act - Try to subscribe to child goal (goal2)
        var result = await sut.CanSubscribeToGoalAsync(user.Id, goal2.Id, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanSubscribeToGoal_Child_Goal_When_Parent_Completed_Should_Return_True()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var goal1 = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var goal2 = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var project1 = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var project2 = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Link goals to cursus
        await RelationFactory.CreateCursusGoal(cursus.Id, goal1.Id)
            .WithContext(Context)
            .GenerateAsync();
        await RelationFactory.CreateCursusGoal(cursus.Id, goal2.Id, parentGoalId: goal1.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Link projects to goals
        await RelationFactory.CreateGoalProject(goal1.Id, project1.Id)
            .WithContext(Context)
            .GenerateAsync();
        await RelationFactory.CreateGoalProject(goal2.Id, project2.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Enroll user in cursus
        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // User HAS completed goal1
        await UserFactory.CreateUserGoal(user.Id, goal1.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Completed)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToGoalAsync(user.Id, goal2.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanSubscribeToGoal_Root_Goal_Should_Return_True_When_Enrolled()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var goal1 = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var goal2 = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        // Link goals to cursus
        await RelationFactory.CreateCursusGoal(cursus.Id, goal1.Id)
            .WithContext(Context)
            .GenerateAsync();
        await RelationFactory.CreateCursusGoal(cursus.Id, goal2.Id, parentGoalId: goal1.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Enroll user in cursus
        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act - Subscribe to root goal should succeed
        var result = await sut.CanSubscribeToGoalAsync(user.Id, goal1.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Cursus With Choice Groups (3 Goals: 1 Root + 2 Choices)

    [Fact]
    public async Task CanSubscribeToGoal_Choice_Group_First_Choice_Should_Return_True()
    {
        // Arrange
        // Structure: Cursus -> Goal1 (root) -> [Goal2, Goal3] (choice group - pick one)
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var rootGoal = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var choiceGoalA = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var choiceGoalB = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        var choiceGroupId = Guid.NewGuid();

        // Link root goal to cursus
        await RelationFactory.CreateCursusGoal(cursus.Id, rootGoal.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Link choice goals as children of root with same choice_group
        await RelationFactory.CreateCursusGoal(cursus.Id, choiceGoalA.Id, parentGoalId: rootGoal.Id, choiceGroup: choiceGroupId)
            .WithContext(Context)
            .GenerateAsync();
        await RelationFactory.CreateCursusGoal(cursus.Id, choiceGoalB.Id, parentGoalId: rootGoal.Id, choiceGroup: choiceGroupId)
            .WithContext(Context)
            .GenerateAsync();

        // Enroll user in cursus
        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // User has completed root goal
        await UserFactory.CreateUserGoal(user.Id, rootGoal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Completed)
            .WithContext(Context)
            .GenerateAsync();

        // Act - Try to subscribe to first choice goal
        var result = await sut.CanSubscribeToGoalAsync(user.Id, choiceGoalA.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanSubscribeToGoal_Choice_Group_Second_Choice_After_First_Chosen_Should_Return_False()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var rootGoal = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var choiceGoalA = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var choiceGoalB = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        var choiceGroupId = Guid.NewGuid();

        // Link root goal to cursus
        await RelationFactory.CreateCursusGoal(cursus.Id, rootGoal.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Link choice goals as children of root with same choice_group
        await RelationFactory.CreateCursusGoal(cursus.Id, choiceGoalA.Id, parentGoalId: rootGoal.Id, choiceGroup: choiceGroupId)
            .WithContext(Context)
            .GenerateAsync();
        await RelationFactory.CreateCursusGoal(cursus.Id, choiceGoalB.Id, parentGoalId: rootGoal.Id, choiceGroup: choiceGroupId)
            .WithContext(Context)
            .GenerateAsync();

        // Enroll user in cursus
        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // User has completed root goal
        await UserFactory.CreateUserGoal(user.Id, rootGoal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Completed)
            .WithContext(Context)
            .GenerateAsync();

        // User has already chosen choiceGoalA (Active)
        await UserFactory.CreateUserGoal(user.Id, choiceGoalA.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act - Try to subscribe to second choice goal (should be blocked)
        var result = await sut.CanSubscribeToGoalAsync(user.Id, choiceGoalB.Id, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanSubscribeToGoal_Choice_Group_Second_Choice_After_First_Completed_Should_Return_False()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var rootGoal = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var choiceGoalA = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var choiceGoalB = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        var choiceGroupId = Guid.NewGuid();

        // Link root goal to cursus
        await RelationFactory.CreateCursusGoal(cursus.Id, rootGoal.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Link choice goals
        await RelationFactory.CreateCursusGoal(cursus.Id, choiceGoalA.Id, parentGoalId: rootGoal.Id, choiceGroup: choiceGroupId)
            .WithContext(Context)
            .GenerateAsync();
        await RelationFactory.CreateCursusGoal(cursus.Id, choiceGoalB.Id, parentGoalId: rootGoal.Id, choiceGroup: choiceGroupId)
            .WithContext(Context)
            .GenerateAsync();

        // Enroll user
        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // User has completed root goal
        await UserFactory.CreateUserGoal(user.Id, rootGoal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Completed)
            .WithContext(Context)
            .GenerateAsync();

        // User has COMPLETED choiceGoalA
        await UserFactory.CreateUserGoal(user.Id, choiceGoalA.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Completed)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToGoalAsync(user.Id, choiceGoalB.Id, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanSubscribeToGoal_Choice_Group_Without_Parent_Completed_Should_Return_False()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var rootGoal = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var choiceGoalA = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var choiceGoalB = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        var choiceGroupId = Guid.NewGuid();

        // Link all goals
        await RelationFactory.CreateCursusGoal(cursus.Id, rootGoal.Id)
            .WithContext(Context)
            .GenerateAsync();
        await RelationFactory.CreateCursusGoal(cursus.Id, choiceGoalA.Id, parentGoalId: rootGoal.Id, choiceGroup: choiceGroupId)
            .WithContext(Context)
            .GenerateAsync();
        await RelationFactory.CreateCursusGoal(cursus.Id, choiceGoalB.Id, parentGoalId: rootGoal.Id, choiceGroup: choiceGroupId)
            .WithContext(Context)
            .GenerateAsync();

        // Enroll user
        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // User has NOT completed root goal

        // Act
        var result = await sut.CanSubscribeToGoalAsync(user.Id, choiceGoalA.Id, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Multiple Cursus Enrollment Tests

    [Fact]
    public async Task CanSubscribeToGoal_Goal_In_Multiple_Cursi_Should_Allow_If_Any_Matches()
    {
        // Arrange - Goal exists in two cursi, user enrolled only in one
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus1 = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var cursus2 = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        // Link goal to both cursi as root
        await RelationFactory.CreateCursusGoal(cursus1.Id, goal.Id)
            .WithContext(Context)
            .GenerateAsync();
        await RelationFactory.CreateCursusGoal(cursus2.Id, goal.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Enroll user only in cursus1
        await UserFactory.CreateUserCursus(user.Id, cursus1.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToGoalAsync(user.Id, goal.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Project Subscription Tests - Free Mode

    [Fact]
    public async Task CanSubscribeToProject_In_Free_Mode_Should_Always_Return_True()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Free);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Project Subscription Tests - Orphan Projects

    [Fact]
    public async Task CanSubscribeToProject_Orphan_Project_Should_Return_True()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        // Project is not linked to any goal (orphan)

        // Act
        var result = await sut.CanSubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Project Subscription Tests - Goal Linked

    [Fact]
    public async Task CanSubscribeToProject_User_Subscribed_To_Active_Goal_Should_Return_True()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create()
            .RuleFor(g => g.Active, true)
            .RuleFor(g => g.Deprecated, false)
            .WithContext(Context)
            .GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Link project to goal
        await RelationFactory.CreateGoalProject(goal.Id, project.Id)
            .WithContext(Context)
            .GenerateAsync();

        // User is subscribed to the goal (Active)
        await UserFactory.CreateUserGoal(user.Id, goal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanSubscribeToProject_User_Not_Subscribed_To_Goal_Should_Return_False()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create()
            .RuleFor(g => g.Active, true)
            .RuleFor(g => g.Deprecated, false)
            .WithContext(Context)
            .GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Link project to goal
        await RelationFactory.CreateGoalProject(goal.Id, project.Id)
            .WithContext(Context)
            .GenerateAsync();

        // User is NOT subscribed to the goal

        // Act
        var result = await sut.CanSubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanSubscribeToProject_User_Goal_Subscription_Completed_Should_Return_False()
    {
        // Arrange - Completed goal subscription shouldn't allow new project subscriptions
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create()
            .RuleFor(g => g.Active, true)
            .RuleFor(g => g.Deprecated, false)
            .WithContext(Context)
            .GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        await RelationFactory.CreateGoalProject(goal.Id, project.Id)
            .WithContext(Context)
            .GenerateAsync();

        // User's goal subscription is Completed (not Active)
        await UserFactory.CreateUserGoal(user.Id, goal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Completed)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Project Subscription Tests - Goal State

    [Fact]
    public async Task CanSubscribeToProject_Goal_Not_Active_Should_Return_False()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create()
            .RuleFor(g => g.Active, false) // Goal is not active
            .RuleFor(g => g.Deprecated, false)
            .WithContext(Context)
            .GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        await RelationFactory.CreateGoalProject(goal.Id, project.Id)
            .WithContext(Context)
            .GenerateAsync();

        // User is subscribed to goal
        await UserFactory.CreateUserGoal(user.Id, goal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanSubscribeToProject_Goal_Deprecated_Should_Return_False()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create()
            .RuleFor(g => g.Active, true)
            .RuleFor(g => g.Deprecated, true) // Goal is deprecated
            .WithContext(Context)
            .GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        await RelationFactory.CreateGoalProject(goal.Id, project.Id)
            .WithContext(Context)
            .GenerateAsync();

        await UserFactory.CreateUserGoal(user.Id, goal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Project Subscription Tests - User Goal Awaiting

    [Fact]
    public async Task CanSubscribeToProject_User_Goal_Awaiting_Should_Return_False()
    {
        // Arrange - Awaiting goal subscription shouldn't allow new project subscriptions (only Active does)
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create()
            .RuleFor(g => g.Active, true)
            .RuleFor(g => g.Deprecated, false)
            .WithContext(Context)
            .GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        await RelationFactory.CreateGoalProject(goal.Id, project.Id)
            .WithContext(Context)
            .GenerateAsync();

        // User's goal subscription is Awaiting (not Active)
        await UserFactory.CreateUserGoal(user.Id, goal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Awaiting)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Project Subscription Tests - Multiple Goals

    [Fact]
    public async Task CanSubscribeToProject_Multiple_Goals_One_Active_Should_Return_True()
    {
        // Arrange - Project linked to 2 goals, user subscribed to only one
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal1 = await GoalFactory.Create()
            .RuleFor(g => g.Active, true)
            .RuleFor(g => g.Deprecated, false)
            .WithContext(Context)
            .GenerateAsync();
        var goal2 = await GoalFactory.Create()
            .RuleFor(g => g.Active, true)
            .RuleFor(g => g.Deprecated, false)
            .WithContext(Context)
            .GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Link project to both goals
        await RelationFactory.CreateGoalProject(goal1.Id, project.Id)
            .WithContext(Context)
            .GenerateAsync();
        await RelationFactory.CreateGoalProject(goal2.Id, project.Id)
            .WithContext(Context)
            .GenerateAsync();

        // User subscribed to only goal1
        await UserFactory.CreateUserGoal(user.Id, goal1.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanSubscribeToProject_Multiple_Goals_One_Deprecated_One_Active_Should_Return_True()
    {
        // Arrange - Project linked to deprecated goal and active goal
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var deprecatedGoal = await GoalFactory.Create()
            .RuleFor(g => g.Active, true)
            .RuleFor(g => g.Deprecated, true)
            .WithContext(Context)
            .GenerateAsync();
        var activeGoal = await GoalFactory.Create()
            .RuleFor(g => g.Active, true)
            .RuleFor(g => g.Deprecated, false)
            .WithContext(Context)
            .GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        await RelationFactory.CreateGoalProject(deprecatedGoal.Id, project.Id)
            .WithContext(Context)
            .GenerateAsync();
        await RelationFactory.CreateGoalProject(activeGoal.Id, project.Id)
            .WithContext(Context)
            .GenerateAsync();

        // User subscribed to the active goal
        await UserFactory.CreateUserGoal(user.Id, activeGoal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    #endregion

    // ==========================================================================
    // Subscribe Tests
    // ==========================================================================

    #region SubscribeToCursus Tests

    [Fact]
    public async Task SubscribeToCursus_New_Subscription_Should_Create_UserCursus()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await sut.SubscribeToCursusAsync(user.Id, cursus.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(cursus.Id, result.CursusId);
        Assert.Equal(EntityObjectState.Active, result.State);
    }

    [Fact]
    public async Task SubscribeToCursus_Already_Active_Should_Throw()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(
            () => sut.SubscribeToCursusAsync(user.Id, cursus.Id, CancellationToken.None));
        Assert.Contains("Already subscribed", ex.Message);
    }

    [Fact]
    public async Task SubscribeToCursus_Completed_Should_Throw()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Completed)
            .WithContext(Context)
            .GenerateAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(
            () => sut.SubscribeToCursusAsync(user.Id, cursus.Id, CancellationToken.None));
        Assert.Contains("Cannot resubscribe to a completed cursus", ex.Message);
    }

    [Fact]
    public async Task SubscribeToCursus_Awaiting_Should_Throw()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Awaiting)
            .WithContext(Context)
            .GenerateAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(
            () => sut.SubscribeToCursusAsync(user.Id, cursus.Id, CancellationToken.None));
        Assert.Contains("awaiting approval", ex.Message);
    }

    [Fact]
    public async Task SubscribeToCursus_Inactive_Should_Reactivate()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Inactive)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.SubscribeToCursusAsync(user.Id, cursus.Id, CancellationToken.None);

        // Assert
        Assert.Equal(EntityObjectState.Active, result.State);
    }

    #endregion

    #region SubscribeToGoal Tests

    [Fact]
    public async Task SubscribeToGoal_New_Subscription_Should_Create_UserGoal()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await sut.SubscribeToGoalAsync(user.Id, goal.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(goal.Id, result.GoalId);
        Assert.Equal(EntityObjectState.Active, result.State);
    }

    [Fact]
    public async Task SubscribeToGoal_Already_Active_Should_Throw()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        await UserFactory.CreateUserGoal(user.Id, goal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(
            () => sut.SubscribeToGoalAsync(user.Id, goal.Id, CancellationToken.None));
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task SubscribeToGoal_Inactive_Should_Reactivate()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        await UserFactory.CreateUserGoal(user.Id, goal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Inactive)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.SubscribeToGoalAsync(user.Id, goal.Id, CancellationToken.None);

        // Assert
        Assert.Equal(EntityObjectState.Active, result.State);
    }

    [Fact]
    public async Task SubscribeToGoal_With_All_Projects_Completed_Should_Set_Completed_State()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Link project to goal
        await RelationFactory.CreateGoalProject(goal.Id, project.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Create completed user project
        var userProject = await UserFactory.CreateUserProject(project.Id)
            .RuleFor(up => up.State, EntityObjectState.Completed)
            .WithContext(Context)
            .GenerateAsync();

        // Add user as member
        await UserProjectFactory.CreateMember(userProject.Id, user.Id)
            .RuleFor(m => m.Role, UserProjectRole.Leader)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.SubscribeToGoalAsync(user.Id, goal.Id, CancellationToken.None);

        // Assert
        Assert.Equal(EntityObjectState.Completed, result.State);
    }

    #endregion

    #region SubscribeToProject Tests

    [Fact]
    public async Task SubscribeToProject_New_Subscription_Should_Create_UserProject_With_Leader()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await sut.SubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Id, result.ProjectId);
        Assert.Equal(EntityObjectState.Active, result.State);
        Assert.Single(result.Members);
        Assert.Equal(user.Id, result.Members.First().UserId);
        Assert.Equal(UserProjectRole.Leader, result.Members.First().Role);

        // Verify transaction was created
        var transaction = await Context.UserProjectTransactions.FirstOrDefaultAsync(t => t.UserProjectId == result.Id);
        Assert.NotNull(transaction);
        Assert.Equal(UserProjectTransactionVariant.Started, transaction.Type);
    }

    [Fact]
    public async Task SubscribeToProject_Already_Active_Should_Throw()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        var userProject = await UserFactory.CreateUserProject(project.Id)
            .RuleFor(up => up.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        await UserProjectFactory.CreateMember(userProject.Id, user.Id)
            .RuleFor(m => m.Role, UserProjectRole.Leader)
            .WithContext(Context)
            .GenerateAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(
            () => sut.SubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None));
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task SubscribeToProject_Inactive_Session_Should_Reactivate()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        var userProject = await UserFactory.CreateUserProject(project.Id)
            .RuleFor(up => up.State, EntityObjectState.Inactive)
            .WithContext(Context)
            .GenerateAsync();

        await UserProjectFactory.CreateMember(userProject.Id, user.Id)
            .RuleFor(m => m.Role, UserProjectRole.Leader)
            .RuleFor(m => m.LeftAt, DateTimeOffset.UtcNow.AddDays(-1))
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.SubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        Assert.Equal(EntityObjectState.Active, result.State);
        var member = result.Members.First(m => m.UserId == user.Id);
        Assert.Null(member.LeftAt); // LeftAt should be reset
    }

    [Fact]
    public async Task SubscribeToProject_Should_Cancel_Pending_Invites()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var otherUser = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Create existing session with a pending invite for the user
        var existingSession = await UserFactory.CreateUserProject(project.Id)
            .RuleFor(up => up.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        await UserProjectFactory.CreateMember(existingSession.Id, otherUser.Id)
            .RuleFor(m => m.Role, UserProjectRole.Leader)
            .WithContext(Context)
            .GenerateAsync();

        await UserProjectFactory.CreateMember(existingSession.Id, user.Id)
            .RuleFor(m => m.Role, UserProjectRole.Pending)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await sut.SubscribeToProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        Assert.NotEqual(existingSession.Id, result.Id); // Should be a new session
        var pendingInvite = await Context.UserProjectMembers
            .FirstOrDefaultAsync(m => m.UserProjectId == existingSession.Id && m.UserId == user.Id);
        Assert.Null(pendingInvite); // Pending invite should be removed
    }

    #endregion

    // ==========================================================================
    // Unsubscribe Tests
    // ==========================================================================

    #region UnsubscribeFromCursus Tests

    [Fact]
    public async Task UnsubscribeFromCursus_Active_Should_Set_Inactive()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        await sut.UnsubscribeFromCursusAsync(user.Id, cursus.Id, CancellationToken.None);

        // Assert
        var userCursus = await Context.UserCursi
            .FirstOrDefaultAsync(uc => uc.UserId == user.Id && uc.CursusId == cursus.Id);
        Assert.NotNull(userCursus);
        Assert.Equal(EntityObjectState.Inactive, userCursus.State);
    }

    [Fact]
    public async Task UnsubscribeFromCursus_Not_Subscribed_Should_Throw()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(
            () => sut.UnsubscribeFromCursusAsync(user.Id, cursus.Id, CancellationToken.None));
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task UnsubscribeFromCursus_Already_Inactive_Should_Throw()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        await UserFactory.CreateUserCursus(user.Id, cursus.Id)
            .RuleFor(uc => uc.State, EntityObjectState.Inactive)
            .WithContext(Context)
            .GenerateAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(
            () => sut.UnsubscribeFromCursusAsync(user.Id, cursus.Id, CancellationToken.None));
        Assert.Equal(409, ex.StatusCode);
    }

    #endregion

    #region UnsubscribeFromGoal Tests

    [Fact]
    public async Task UnsubscribeFromGoal_Active_Should_Set_Inactive()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        await UserFactory.CreateUserGoal(user.Id, goal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        await sut.UnsubscribeFromGoalAsync(user.Id, goal.Id, CancellationToken.None);

        // Assert
        var userGoal = await Context.UserGoals
            .FirstOrDefaultAsync(ug => ug.UserId == user.Id && ug.GoalId == goal.Id);
        Assert.NotNull(userGoal);
        Assert.Equal(EntityObjectState.Inactive, userGoal.State);
    }

    [Fact]
    public async Task UnsubscribeFromGoal_Not_Subscribed_Should_Throw()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(
            () => sut.UnsubscribeFromGoalAsync(user.Id, goal.Id, CancellationToken.None));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UnsubscribeFromGoal_Already_Inactive_Should_Throw()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        await UserFactory.CreateUserGoal(user.Id, goal.Id)
            .RuleFor(ug => ug.State, EntityObjectState.Inactive)
            .WithContext(Context)
            .GenerateAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(
            () => sut.UnsubscribeFromGoalAsync(user.Id, goal.Id, CancellationToken.None));
        Assert.Equal(409, ex.StatusCode);
    }

    #endregion

    #region UnsubscribeFromProject Tests

    [Fact]
    public async Task UnsubscribeFromProject_Leader_Only_Member_Should_Deactivate_Session()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        var userProject = await UserFactory.CreateUserProject(project.Id)
            .RuleFor(up => up.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        await UserProjectFactory.CreateMember(userProject.Id, user.Id)
            .RuleFor(m => m.Role, UserProjectRole.Leader)
            .RuleFor(m => m.LeftAt, (DateTimeOffset?)null)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        await sut.UnsubscribeFromProjectAsync(user.Id, project.Id, CancellationToken.None);

        // Assert
        var updatedProject = await Context.UserProjects.FindAsync(userProject.Id);
        Assert.NotNull(updatedProject);
        Assert.Equal(EntityObjectState.Inactive, updatedProject.State);

        var member = await Context.UserProjectMembers
            .FirstOrDefaultAsync(m => m.UserProjectId == userProject.Id && m.UserId == user.Id);
        Assert.NotNull(member);
        Assert.NotNull(member.LeftAt);
    }

    [Fact]
    public async Task UnsubscribeFromProject_Leader_With_Other_Members_Should_Throw()
    {
        // Arrange
        var sut = CreateService();
        var leader = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var member = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        var userProject = await UserFactory.CreateUserProject(project.Id)
            .RuleFor(up => up.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        await UserProjectFactory.CreateMember(userProject.Id, leader.Id)
            .RuleFor(m => m.Role, UserProjectRole.Leader)
            .RuleFor(m => m.LeftAt, (DateTimeOffset?)null)
            .WithContext(Context)
            .GenerateAsync();

        await UserProjectFactory.CreateMember(userProject.Id, member.Id)
            .RuleFor(m => m.Role, UserProjectRole.Member)
            .RuleFor(m => m.LeftAt, (DateTimeOffset?)null)
            .WithContext(Context)
            .GenerateAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(
            () => sut.UnsubscribeFromProjectAsync(leader.Id, project.Id, CancellationToken.None));
        Assert.Equal(422, ex.StatusCode);
        Assert.Contains("Transfer leadership", ex.Message);
    }

    [Fact]
    public async Task UnsubscribeFromProject_Member_Should_Set_LeftAt()
    {
        // Arrange
        var sut = CreateService();
        var leader = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var memberUser = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        var userProject = await UserFactory.CreateUserProject(project.Id)
            .RuleFor(up => up.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        await UserProjectFactory.CreateMember(userProject.Id, leader.Id)
            .RuleFor(m => m.Role, UserProjectRole.Leader)
            .RuleFor(m => m.LeftAt, (DateTimeOffset?)null)
            .WithContext(Context)
            .GenerateAsync();

        await UserProjectFactory.CreateMember(userProject.Id, memberUser.Id)
            .RuleFor(m => m.Role, UserProjectRole.Member)
            .RuleFor(m => m.LeftAt, (DateTimeOffset?)null)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        await sut.UnsubscribeFromProjectAsync(memberUser.Id, project.Id, CancellationToken.None);

        // Assert
        var member = await Context.UserProjectMembers
            .FirstOrDefaultAsync(m => m.UserProjectId == userProject.Id && m.UserId == memberUser.Id);
        Assert.NotNull(member);
        Assert.NotNull(member.LeftAt);

        // Session should still be active
        var updatedProject = await Context.UserProjects.FindAsync(userProject.Id);
        Assert.Equal(EntityObjectState.Active, updatedProject!.State);
    }

    [Fact]
    public async Task UnsubscribeFromProject_Not_Subscribed_Should_Throw()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(
            () => sut.UnsubscribeFromProjectAsync(user.Id, project.Id, CancellationToken.None));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UnsubscribeFromProject_Already_Left_Should_Throw()
    {
        // Arrange
        var sut = CreateService();
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        var userProject = await UserFactory.CreateUserProject(project.Id)
            .RuleFor(up => up.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        await UserProjectFactory.CreateMember(userProject.Id, user.Id)
            .RuleFor(m => m.Role, UserProjectRole.Member)
            .RuleFor(m => m.LeftAt, DateTimeOffset.UtcNow.AddDays(-1)) // Already left
            .WithContext(Context)
            .GenerateAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(
            () => sut.UnsubscribeFromProjectAsync(user.Id, project.Id, CancellationToken.None));
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task UnsubscribeFromProject_Leader_Should_Cancel_Pending_Invites()
    {
        // Arrange
        var sut = CreateService();
        var leader = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var pendingUser = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        var userProject = await UserFactory.CreateUserProject(project.Id)
            .RuleFor(up => up.State, EntityObjectState.Active)
            .WithContext(Context)
            .GenerateAsync();

        await UserProjectFactory.CreateMember(userProject.Id, leader.Id)
            .RuleFor(m => m.Role, UserProjectRole.Leader)
            .RuleFor(m => m.LeftAt, (DateTimeOffset?)null)
            .WithContext(Context)
            .GenerateAsync();

        await UserProjectFactory.CreateMember(userProject.Id, pendingUser.Id)
            .RuleFor(m => m.Role, UserProjectRole.Pending)
            .RuleFor(m => m.LeftAt, (DateTimeOffset?)null)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        await sut.UnsubscribeFromProjectAsync(leader.Id, project.Id, CancellationToken.None);

        // Assert - Pending invite should be removed
        var pendingMember = await Context.UserProjectMembers
            .FirstOrDefaultAsync(m => m.UserProjectId == userProject.Id && m.UserId == pendingUser.Id);
        Assert.Null(pendingMember);
    }

    #endregion

    #region CanSubscribeToCursus Tests

    [Fact]
    public async Task CanSubscribeToCursus_In_Free_Mode_Should_Return_True()
    {
        // Arrange
        var sut = CreateService(ProgressionMode.Free);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToCursusAsync(user.Id, cursus.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanSubscribeToCursus_In_Restricted_Mode_Should_Return_True()
    {
        // Arrange - Currently implementation always returns true in Restricted mode too
        var sut = CreateService(ProgressionMode.Restricted);
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await sut.CanSubscribeToCursusAsync(user.Id, cursus.Id, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    #endregion
}
