// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Net;
using System.Net.Http.Json;
using App.Backend.Core;
using App.Backend.Database;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Models.Requests.Cursus;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Responses.Entities.Cursus;
using App.Backend.Models.Responses.Entities.Goals;
using App.Backend.Tests.Integration;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace App.Backend.Tests.Integration.Entities;

public class CursusTests
{
    private static readonly Faker Faker = new();

    private static async Task<(
        WebAppTestFactory factory,
        DatabaseContext db,
        User user,
        HttpClient client
    )> Setup()
    {
        var factory = new WebAppTestFactory();
        var db = factory.CreateDbContext();
        var user = db.Users.Add(new()
        {
            Login = Faker.Internet.UserName(),
            Display = Faker.Internet.UserName(),
        });

        await db.SaveChangesAsync();
        var client = factory.CreateClient(user.Entity.Id);
        return (factory, db, user.Entity, client);
    }

    private static async Task<WorkspaceDO> GetWorkspaceAsync(DatabaseContext db, HttpClient client, User user)
    {
        var response = await client.GetAsync("/workspace/current");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var workspace = await response.Content.ReadFromJsonAsync<WorkspaceDO>(JsonOptions.Default);
        Assert.NotNull(workspace);

        var member = await db.Members.FirstOrDefaultAsync(m =>
            m.EntityId == workspace.Id &&
            m.EntityType == MemberEntityType.Workspace &&
            m.LeftAt == null &&
            m.UserId == user.Id);

        Assert.NotNull(member);
        return workspace;
    }

    [Fact]
    public async Task CursusTrack_ShouldManageGoalHierarchyAndTreeCorrectly()
    {
        var (_, db, user, client) = await Setup();

        // 1. Setup Workspace and Cursus
        var workspace = await GetWorkspaceAsync(db, client, user);
        var cursus = await CreateCursusAsync(client, workspace.Id);

        // 2. Create two distinct goals
        var rootGoal = await CreateGoalAsync(client, workspace.Id);
        var childGoal = await CreateGoalAsync(client, workspace.Id);

        // 3. Add Root Goal and Child Goal to the Cursus Track
        var addTrackResponse = await client.PostAsJsonAsync($"/cursus/{cursus.Id}/track", new PostCursusTrackRequestDTO
        {
            Nodes = [
                new() { GoalId = rootGoal.Id, ParentId = null },
                new() { GoalId = childGoal.Id, ParentId = rootGoal.Id }
            ]
        }, JsonOptions.Default);

        Assert.Equal(HttpStatusCode.OK, addTrackResponse.StatusCode);

        // 4. Fetch and verify the Cursus Track tree structure
        var trackResponse = await client.GetAsync($"/cursus/{cursus.Id}/track");
        Assert.Equal(HttpStatusCode.OK, trackResponse.StatusCode);

        var track = await trackResponse.Content.ReadFromJsonAsync<CursusTrackDO>(JsonOptions.Default);
        Assert.NotNull(track);
        Assert.Single(track.Nodes); // Only 1 top-level root node should exist
    }

    [Fact]
    public async Task CursusTrack_ShouldSupportChoiceGroups()
    {
        var (_, db, user, client) = await Setup();
        var workspace = await GetWorkspaceAsync(db, client, user);
        var cursus = await CreateCursusAsync(client, workspace.Id);

        var rootGoal = await CreateGoalAsync(client, workspace.Id);
        var optionAGoal = await CreateGoalAsync(client, workspace.Id);
        var optionBGoal = await CreateGoalAsync(client, workspace.Id);
        var choiceGroupId = Guid.NewGuid();

        // Add nodes where option A and option B share a choice group under the root node
        var addTrackResponse = await client.PostAsJsonAsync($"/cursus/{cursus.Id}/track", new PostCursusTrackRequestDTO
        {
            Nodes = [
                new() { GoalId = rootGoal.Id, ParentId = null },
                new() { GoalId = optionAGoal.Id, ParentId = rootGoal.Id, Group = choiceGroupId },
                new() { GoalId = optionBGoal.Id, ParentId = rootGoal.Id, Group = choiceGroupId }
            ]
        }, JsonOptions.Default);

        Assert.Equal(HttpStatusCode.OK, addTrackResponse.StatusCode);

        // Verify choice groups match correctly in the tree structure
        var trackResponse = await client.GetAsync($"/cursus/{cursus.Id}/track");
        var track = await trackResponse.Content.ReadFromJsonAsync<CursusTrackDO>(JsonOptions.Default);

        Assert.NotNull(track);
        var children = track.Nodes.First().Children;
        Assert.Equal(2, children.Count);
        Assert.All(children, node => Assert.Equal(choiceGroupId, node.ChoiceGroup));
    }

    [Fact]
    public async Task CursusTrack_SelfReferencingGoal_ShouldFail()
    {
        var (_, db, user, client) = await Setup();
        var workspace = await GetWorkspaceAsync(db, client, user);
        var cursus = await CreateCursusAsync(client, workspace.Id);
        var targetGoal = await CreateGoalAsync(client, workspace.Id);

        // Attempting to create an invalid combination where a node is its own parent
        var badRequestPayload = new PostCursusTrackRequestDTO
        {
            Nodes = [
                new() { GoalId = targetGoal.Id, ParentId = targetGoal.Id }
            ]
        };

        var response = await client.PostAsJsonAsync($"/cursus/{cursus.Id}/track", badRequestPayload, JsonOptions.Default);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    private static async Task<CursusDO> CreateCursusAsync(HttpClient client, Guid workspaceId)
    {
        var response = await client.PostAsJsonAsync($"/workspace/{workspaceId}/cursus", new
        {
            Name = Faker.Internet.DomainName(),
            Description = Faker.Lorem.Paragraph(1),
            Active = true,
            Public = true,
        }, JsonOptions.Default);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<CursusDO>(JsonOptions.Default);
    }

    private static async Task<GoalDO> CreateGoalAsync(HttpClient client, Guid workspaceId)
    {
        var response = await client.PostAsJsonAsync($"/workspace/{workspaceId}/goal", new
        {
            Name = Faker.Internet.DomainName(),
            Description = Faker.Lorem.Paragraph(1),
            Public = true,
        }, JsonOptions.Default);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<GoalDO>(JsonOptions.Default);
    }
}