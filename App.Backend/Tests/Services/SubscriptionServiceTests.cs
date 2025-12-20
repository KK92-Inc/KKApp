using App.Backend.Core;
using App.Backend.Core.Services.Implementation;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Enums;
using App.Backend.Tests.Fixtures;
using App.Backend.Tests.Fixtures.Factory;
using Moq;
using Xunit;

namespace App.Backend.Tests.Services;

public class SubscriptionServiceTests : ServiceTestBase
{
    private readonly Mock<IUserProjectService> _userProjectServiceMock = new();
    private readonly SubscriptionService _sut;

    public SubscriptionServiceTests()
    {
        _sut = new SubscriptionService(Context, _userProjectServiceMock.Object);
    }

    [Fact]
    public async Task SubscribeToCursusAsync_WhenNotSubscribed_ShouldCreateNewSubscription()
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
    public async Task SubscribeToCursusAsync_WhenAlreadySubscribedAndActive_ShouldThrowServiceException()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        
        // Pre-existing active subscription
        await _sut.SubscribeToCursusAsync(user.Id, cursus.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(() => _sut.SubscribeToCursusAsync(user.Id, cursus.Id));
        Assert.Equal(409, exception.StatusCode);
        Assert.Equal("Already subscribed to this cursus", exception.Message);
    }

    [Fact]
    public async Task SubscribeToCursusAsync_WhenAlreadySubscribedAndCompleted_ShouldThrowServiceException()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();
        
        // Create a completed subscription manually
        var existingSubscription = await _sut.SubscribeToCursusAsync(user.Id, cursus.Id);
        existingSubscription.State = EntityObjectState.Completed;
        Context.UserCursi.Update(existingSubscription);
        await Context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(() => _sut.SubscribeToCursusAsync(user.Id, cursus.Id));
        Assert.Equal(422, exception.StatusCode);
        Assert.Equal("Cursus is already completed", exception.Message);
    }

    [Fact]
    public async Task SubscribeToCursusAsync_WhenPreviouslyUnsubscribed_ShouldReactivateSubscription()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        // Create an inactive subscription manually
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
        Assert.Equal(existingSubscription.Id, result.Id); // Should be the same entity
    }
}