// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Services.Implementation;
using App.Backend.Tests.Fixtures;
using App.Backend.Tests.Fixtures.Factory;

// ============================================================================

namespace App.Backend.Tests.Services;

public class CursusServiceTests : ServiceTestBase
{
    private readonly CursusService _sut;

    public CursusServiceTests() => _sut = new CursusService(Context);

    [Fact]
    public async Task CreateAsync_ShouldCreateCursus()
    {
        // Arrange
        var cursus = CursusFactory.Create().Generate();

        // Act
        var result = await _sut.CreateAsync(cursus);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cursus.Name, result.Name);
        Assert.Equal(cursus.Slug, result.Slug);

        var dbCursus = await Context.Cursi.FindAsync(result.Id);
        Assert.NotNull(dbCursus);
    }

    [Fact]
    public async Task FindByIdAsync_WhenExists_ShouldReturnCursus()
    {
        // Arrange
        var cursus = await CursusFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await _sut.FindByIdAsync(cursus.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cursus.Id, result.Id);
        Assert.Equal(cursus.Name, result.Name);
    }

    [Fact]
    public async Task FindByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        // Act
        var result = await _sut.FindByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }
}
