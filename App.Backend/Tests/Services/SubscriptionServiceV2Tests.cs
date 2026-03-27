// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Services.Implementation;
using App.Backend.Core.Services.Options;
using App.Backend.Domain.Enums;
using App.Backend.Tests.Fixtures;
using App.Backend.Tests.Fixtures.Factory;
using Microsoft.Extensions.Options;

// ============================================================================

namespace App.Backend.Tests.Services;

public class SubscriptionServiceV2Tests : ServiceTestBase
{
    private SubscriptionServiceV2 CreateService(ProgressionMode mode = ProgressionMode.Restricted)
    {
        var options = Options.Create(new SubscriptionOptions { Mode = mode });
        return new SubscriptionServiceV2(Context, options);
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
}
