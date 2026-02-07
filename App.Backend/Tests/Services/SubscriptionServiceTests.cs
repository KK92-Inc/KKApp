// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core;
using App.Backend.Core.Services.Implementation;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Enums;
using App.Backend.Tests.Fixtures;
using App.Backend.Tests.Fixtures.Factory;
using Moq;

// ============================================================================

namespace App.Backend.Tests.Services;

public class SubscriptionServiceTests : ServiceTestBase
{
    private readonly SubscriptionService _sut;

    public SubscriptionServiceTests()
    {
        _sut = new SubscriptionService(Context);
    }

    #region Cursus Subscription Tests

    [Fact]
    public async Task Subscribe_To_Cursus_When_Not_Subscribed_Should_Create_New_Subscription()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await _sut.SubscribeToCursusAsync(user.Id, cursus.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(cursus.Id, result.CursusId);
        Assert.Equal(EntityObjectState.Active, result.State);
    }

    [Fact]
    public async Task Subscribe_To_Cursus_When_Already_Active_Should_Throw_Conflict_Exception()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        await _sut.SubscribeToCursusAsync(user.Id, cursus.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(
            () => _sut.SubscribeToCursusAsync(user.Id, cursus.Id));
        Assert.Equal(409, exception.StatusCode);
    }

    [Fact]
    public async Task Subscribe_To_Cursus_When_Already_Completed_Should_Throw_Unprocessable_Exception()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var subscription = await _sut.SubscribeToCursusAsync(user.Id, cursus.Id);
        subscription.State = EntityObjectState.Completed;
        Context.UserCursi.Update(subscription);
        await Context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(
            () => _sut.SubscribeToCursusAsync(user.Id, cursus.Id));
        Assert.Equal(422, exception.StatusCode);
    }

    [Fact]
    public async Task Subscribe_To_Cursus_When_Previously_Unsubscribed_Should_Reactivate_Subscription()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var existingSubscription = await _sut.SubscribeToCursusAsync(user.Id, cursus.Id);
        existingSubscription.State = EntityObjectState.Inactive;
        Context.UserCursi.Update(existingSubscription);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.SubscribeToCursusAsync(user.Id, cursus.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(cursus.Id, result.CursusId);
        Assert.Equal(EntityObjectState.Active, result.State);
        Assert.Equal(existingSubscription.Id, result.Id);
    }

    [Fact]
    public async Task Unsubscribe_From_Cursus_When_Subscribed_Should_Set_State_To_Inactive()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var subscription = await _sut.SubscribeToCursusAsync(user.Id, cursus.Id);

        // Act
        await _sut.UnsubscribeFromCursusAsync(user.Id, cursus.Id);

        // Assert
        var updatedSubscription = await Context.UserCursi.FindAsync(subscription.Id);
        Assert.NotNull(updatedSubscription);
        Assert.Equal(EntityObjectState.Inactive, updatedSubscription!.State);
    }

    [Fact]
    public async Task Unsubscribe_From_Cursus_When_Not_Subscribed_Should_Throw_Not_Found_Exception()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(
            () => _sut.UnsubscribeFromCursusAsync(user.Id, cursus.Id));
        Assert.Equal(404, exception.StatusCode);
    }

    [Fact]
    public async Task Unsubscribe_From_Cursus_When_Already_Inactive_Should_Throw_Conflict_Exception()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        var subscription = await _sut.SubscribeToCursusAsync(user.Id, cursus.Id);
        subscription.State = EntityObjectState.Inactive;
        Context.UserCursi.Update(subscription);
        await Context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(
            () => _sut.UnsubscribeFromCursusAsync(user.Id, cursus.Id));
        Assert.Equal(409, exception.StatusCode);
    }

    #endregion

    #region Goal Subscription Tests

    [Fact]
    public async Task Subscribe_To_Goal_When_Not_Subscribed_Should_Create_New_Subscription()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await _sut.SubscribeToGoalAsync(user.Id, goal.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(goal.Id, result.GoalId);
        Assert.Equal(EntityObjectState.Active, result.State);
    }

    [Fact]
    public async Task Subscribe_To_Goal_When_Already_Active_Should_Throw_Conflict_Exception()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        await _sut.SubscribeToGoalAsync(user.Id, goal.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(
            () => _sut.SubscribeToGoalAsync(user.Id, goal.Id));
        Assert.Equal(409, exception.StatusCode);
    }

    [Fact]
    public async Task Subscribe_To_Goal_When_Previously_Unsubscribed_Should_Reactivate_Subscription()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var existingSubscription = await _sut.SubscribeToGoalAsync(user.Id, goal.Id);
        existingSubscription.State = EntityObjectState.Inactive;
        Context.UserGoals.Update(existingSubscription);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.SubscribeToGoalAsync(user.Id, goal.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(goal.Id, result.GoalId);
        Assert.Equal(EntityObjectState.Active, result.State);
        Assert.Equal(existingSubscription.Id, result.Id);
    }

    [Fact]
    public async Task Unsubscribe_From_Goal_When_Subscribed_Should_Set_State_To_Inactive()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var subscription = await _sut.SubscribeToGoalAsync(user.Id, goal.Id);

        // Act
        await _sut.UnsubscribeFromGoalAsync(user.Id, goal.Id);

        // Assert
        var updatedSubscription = await Context.UserGoals.FindAsync(subscription.Id);
        Assert.NotNull(updatedSubscription);
        Assert.Equal(EntityObjectState.Inactive, updatedSubscription!.State);
    }

    [Fact]
    public async Task Unsubscribe_From_Goal_When_Not_Subscribed_Should_Throw_Not_Found_Exception()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(
            () => _sut.UnsubscribeFromGoalAsync(user.Id, goal.Id));
        Assert.Equal(404, exception.StatusCode);
    }

    [Fact]
    public async Task Unsubscribe_From_Goal_When_Already_Inactive_Should_Throw_Conflict_Exception()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var subscription = await _sut.SubscribeToGoalAsync(user.Id, goal.Id);
        subscription.State = EntityObjectState.Inactive;
        Context.UserGoals.Update(subscription);
        await Context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(
            () => _sut.UnsubscribeFromGoalAsync(user.Id, goal.Id));
        Assert.Equal(409, exception.StatusCode);
    }

    #endregion

    #region Project Subscription Tests

    [Fact]
    public async Task Subscribe_To_Project_When_Not_Subscribed_Should_Create_New_Subscription()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await _sut.SubscribeToProjectAsync(user.Id, project.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Id, result.ProjectId);
        Assert.Equal(EntityObjectState.Active, result.State);
        Assert.Contains(result.Members, m => m.UserId == user.Id);
    }

    [Fact]
    public async Task Subscribe_To_Project_When_Already_Active_Should_Throw_Conflict_Exception()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        await _sut.SubscribeToProjectAsync(user.Id, project.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(
            () => _sut.SubscribeToProjectAsync(user.Id, project.Id));
        Assert.Equal(409, exception.StatusCode);
    }

    [Fact]
    public async Task Subscribe_To_Project_When_Previously_Unsubscribed_Should_Reactivate_Subscription()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var subscription = await _sut.SubscribeToProjectAsync(user.Id, project.Id);
        subscription.State = EntityObjectState.Inactive;
        Context.UserProjects.Update(subscription);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.SubscribeToProjectAsync(user.Id, project.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Id, result.ProjectId);
        Assert.Equal(EntityObjectState.Active, result.State);
        Assert.Equal(subscription.Id, result.Id);
    }

    [Fact]
    public async Task Subscribe_To_Project_Should_Set_User_As_Leader()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await _sut.SubscribeToProjectAsync(user.Id, project.Id);

        // Assert
        Assert.NotNull(result);
        var member = result.Members.First(m => m.UserId == user.Id);
        Assert.Equal(UserProjectRole.Leader, member.Role);
    }

    [Fact]
    public async Task Unsubscribe_From_Project_When_Leader_Should_Set_State_To_Inactive()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        await _sut.SubscribeToProjectAsync(user.Id, project.Id);

        // Act
        await _sut.UnsubscribeFromProjectAsync(user.Id, project.Id);

        // Assert
        var subscription = await Context.UserProjects.FindAsync(
            Context.UserProjects.First(up => up.ProjectId == project.Id).Id);
        Assert.NotNull(subscription);
        Assert.Equal(EntityObjectState.Inactive, subscription!.State);
    }

    [Fact]
    public async Task Unsubscribe_From_Project_When_Not_Subscribed_Should_Throw_Not_Found_Exception()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(
            () => _sut.UnsubscribeFromProjectAsync(user.Id, project.Id));
        Assert.Equal(404, exception.StatusCode);
    }

    #endregion
}
