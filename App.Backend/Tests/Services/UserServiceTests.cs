// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Services.Implementation;
using App.Backend.Tests.Fixtures;
using App.Backend.Tests.Fixtures.Factory;

// ============================================================================

namespace App.Backend.Tests.Services;

public class UserServiceTests : ServiceTestBase
{
    private readonly UserService _sut;

    public UserServiceTests() => _sut = new UserService(Context);

    [Fact]
    public async Task FindByLoginAsync_WhenExists_ShouldReturnUser()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await _sut.FindByLoginAsync(user.Login);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Login, result.Login);
    }

    [Fact]
    public async Task FindByNameAsync_WhenExists_ShouldReturnUser()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await _sut.FindByLoginAsync(user.Login);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Login, result.Login);
    }

    [Fact]
    public async Task FindByLoginAsync_WhenNotExists_ShouldReturnNull()
    {
        Assert.Null(await _sut.FindByLoginAsync("HalfLife3"));
    }

    [Fact]
    public async Task FindByNameAsync_WhenNotExists_ShouldReturnNull()
    {
        Assert.Null(await _sut.FindByNameAsync("HalfLife3"));
    }
}
