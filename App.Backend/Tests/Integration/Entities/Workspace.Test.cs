/*
delete /cursus/{id}
delete /goals/{id}
delete /projects/{id}
delete /rubrics/{id}
get /workspace/current
post /workspace/{workspace}/cursus
post /workspace/{workspace}/goal
post /workspace/{workspace}/project
post /workspace/{id}/rubric
post /workspace/{id}/application
patch /workspace/{id}/application/{appId}
delete /workspace/{id}/application/{appId}
post /workspace/{id}/application/{appId}/secret/rotate
post /workspace/{from}/transfer/cursus/{to}
post /workspace/{from}/transfer/goal/{to}
post /workspace/{from}/transfer/project/{to}
post /workspace/{id}/invite/{inviteeId}
delete /workspace/{id}/invite/{inviteeId}
post /workspace/{id}/invite/accept
post /workspace/{id}/invite/decline
post /workspace/{id}/member/leave
post /workspace/{id}/member/kick/{memberId}
*/

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using App.Backend.Core;
using App.Backend.Database;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Responses.Entities.Projects;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace App.Backend.Tests.Integration.Entities;

public class WorkspaceIntegrationTests2
{
    private static readonly Faker Faker = new();

    private static readonly JsonSerializerOptions options = DOJsonOptions.Default;

    [Fact]
    public async Task Workspace_Current()
    {
        // Arrange
        using var factory = new ApiFactory();
        using var db = factory.CreateDbContext();

        var (user, workspace) = await SetupWorkspace(db);
        var client = factory.CreateClient(user.Id);

        // Act
        var response = await client.GetAsync("/workspace/current");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        var content = await response.Content.ReadFromJsonAsync<WorkspaceDO>(options);
        Assert.NotNull(content);
        Assert.Equal(workspace.Id, content.Id);
        Assert.Equal(workspace.OwnerId, content.Owner?.Id);
        Assert.Equal(workspace.Ownership, content.Ownership);
    }

    [Fact]
    public async Task Workspace_Delete_Rubric()
    {
        // Arrange
        using var factory = new ApiFactory();
        var client = factory.CreateClient(Guid.NewGuid());

        // Act
        var response = await client.GetAsync("/workspace/current");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]

    /// <summary>
    /// Creates a new workspace for the specified owner in the provided database context.
    /// This method is intended for use in integration tests to set up test data.
    /// </summary>
    /// <param name="db"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    private static async Task<(User, Workspace)> SetupWorkspace(DatabaseContext db)
    {
        var user = db.Users.Add(new ()
        {
            Login = Faker.Internet.UserName(),
            Display = Faker.Internet.UserName(),
        });

        var workspace = db.Workspaces.Add(new ()
        {
            OwnerId = user.Entity.Id,
            Ownership = EntityOwnership.User
        });

        await db.SaveChangesAsync();
        return (user.Entity, workspace.Entity);
    }
}