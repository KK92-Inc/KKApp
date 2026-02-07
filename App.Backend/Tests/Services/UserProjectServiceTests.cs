// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Services.Implementation;
using App.Backend.Domain.Enums;
using App.Backend.Tests.Fixtures;
using App.Backend.Tests.Fixtures.Factory;

// ============================================================================

namespace App.Backend.Tests.Services;

public class UserProjectServiceTests : ServiceTestBase
{
    private readonly UserProjectService _sut;

    public UserProjectServiceTests() => _sut = new UserProjectService(Context);

    // [Fact]
    // public async Task RecordAsync_ShouldCreateTransaction()
    // {
    //     // Arrange
    //     var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
    //     var userProject = await UserFactory.CreateUserProject(project.Id).WithContext(Context).GenerateAsync();
    //     var user = await UserFactory.Create().WithContext(Context).GenerateAsync();

    //     // Act
    //     var result = await _sut.RecordAsync(userProject.Id, user.Id, UserProjectTransactionVariant.Started);

    //     // Assert
    //     Assert.NotNull(result);
    //     Assert.Equal(userProject.Id, result.UserProjectId);
    //     Assert.Equal(user.Id, result.UserId);
    //     Assert.Equal(UserProjectTransactionVariant.Started, result.Type);

    //     var dbTransaction = await Context.UserProjectTransactions.FindAsync(result.Id);
    //     Assert.NotNull(dbTransaction);
    // }

    [Fact]
    public async Task CreateAsync_ShouldCreateUserProject()
    {
        // Arrange
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var userProject = UserFactory.CreateUserProject(project.Id).Generate();

        // Act
        var result = await _sut.CreateAsync(userProject);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Id, result.ProjectId);

        var dbUserProject = await Context.UserProjects.FindAsync(result.Id);
        Assert.NotNull(dbUserProject);
    }
}
