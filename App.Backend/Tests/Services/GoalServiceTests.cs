// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Services.Implementation;
using App.Backend.Tests.Fixtures;
using App.Backend.Tests.Fixtures.Factory;

// ============================================================================

namespace App.Backend.Tests.Services;

public class GoalServiceTests : ServiceTestBase
{
    private readonly GoalService _sut;

    public GoalServiceTests() => _sut = new GoalService(Context);

    [Fact]
    public async Task FindBySlugAsync_WhenExists_ShouldReturnGoal()
    {
        // Arrange
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();

        // Act
        var result = await _sut.FindBySlugAsync(goal.Slug);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(goal.Id, result.Id);
        Assert.Equal(goal.Slug, result.Slug);
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
    public async Task GetGoalProjectsAsync_ShouldReturnAssociatedProjects()
    {
        // Arrange
        var goal = await GoalFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        await RelationFactory.CreateGoalProject(goal.Id, project.Id).WithContext(Context).GenerateAsync();

        // Act
        var result = await _sut.GetGoalProjectsAsync(goal.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal(project.Id, result.First().Id);
    }
}
