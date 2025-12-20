// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Services.Implementation;
using App.Backend.Tests.Fixtures;
using App.Backend.Tests.Fixtures.Factory;

// ============================================================================

namespace App.Backend.Tests.Services;

public class ProjectServiceTests : ServiceTestBase
{
    private readonly ProjectService _sut;

    public ProjectServiceTests() => _sut = new ProjectService(Context);

    [Fact]
    public async Task FindBySlugAsync_WhenExists_ShouldReturnProject()
    {
        // Arrange
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await _sut.FindBySlugAsync(project.Slug);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Id, result.Id);
        Assert.Equal(project.Slug, result.Slug);
    }

    [Fact]
    public async Task FindBySlugAsync_WhenNotExists_ShouldReturnNull()
    {
        // Act
        var result = await _sut.FindBySlugAsync("non-existent-slug");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        // Arrange
        var project = await ProjectFactory.Create()
            .WithContext(Context)
            .With(p => p.Deprecated, false)
            .GenerateAsync();

        // Act
        await _sut.DeleteAsync(project);

        // Assert
        var dbProject = await Context.Projects.FindAsync(project.Id);
        Assert.NotNull(dbProject);
        Assert.True(dbProject.Deprecated);
    }
}
