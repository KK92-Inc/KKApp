using System.Net;
using System.Net.Http.Json;
using App.Backend.Domain.Enums;
using App.Backend.Models.Requests.Cursus;
using App.Backend.Models.Responses.Entities.Cursus;

namespace App.Backend.Tests.Integration.Entities;

public class CursusCompletionTests3
{
    private static async Task PutTrackAsync(HttpClient client, Guid cursusId, List<Models.Requests.Cursus.PutCursusTrackNodeDO> nodes)
    {
        var response = await client.PostAsJsonAsync($"/cursus/{cursusId}/track",
            new PutCursusTrackRequestDTO { Nodes = nodes }, JsonOptions.Default);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

        private static async Task<UserCursusDO> SubscribeToCursusAsync(HttpClient client, Guid userId, Guid cursusId)
    {
        var response = await client.PostAsync($"/subscribe/{userId}/cursus/{cursusId}", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UserCursusDO>(JsonOptions.Default);
        Assert.NotNull(result);
        return result;
    }

    [Fact]
    public async Task LinearTrack_PartialCompletion_NeverCompletesCursus()
    {
        var (factory, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var cursus = await client.CreateCursusAsync(workspace.Id);

        var a = await client.CreateGoalAsync(workspace.Id);
        var b = await client.CreateGoalAsync(workspace.Id);
        var c = await client.CreateGoalAsync(workspace.Id);

        await PutTrackAsync(client, cursus.Id, [
            new() { GoalId = a.Id, ParentId = null },
            new() { GoalId = b.Id, ParentId = a.Id },
            new() { GoalId = c.Id, ParentId = b.Id },
        ]);

        var userCursus = await SubscribeToCursusAsync(client, user.Id, cursus.Id);

        await factory.CompleteGoalAsync(db, user.Id, a.Id);
        await factory.CompleteGoalAsync(db, user.Id, b.Id);
        // Deliberately never complete C.

        // No event will ever flip this cursus to Completed - assert the state holds
        // steady rather than waiting for a condition that should never become true.
        await Task.Delay(TimeSpan.FromMilliseconds(250));
        Assert.Equal(EntityObjectState.Active, await GetCursusStateAsync(factory, userCursus.Id));
    }
}