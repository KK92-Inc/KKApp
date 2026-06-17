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
using App.Backend.Models.Requests.Goals;
using App.Backend.Models.Requests.Projects;
using App.Backend.Models.Requests.Rubrics;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Responses.Entities.Cursus;
using App.Backend.Models.Responses.Entities.Projects;
using App.Backend.Models.Responses.Entities.Reviews;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace App.Backend.Tests.Integration;

public static class TestUtils
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Spins up a test factory, resolves the DB context, inserts a fake user, and sets up a client.
    /// </summary>
    public static async Task<(WebAppTestFactory Factory, DatabaseContext Db, User User, HttpClient Client)> SetupAsync()
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

    /// <summary>
    /// Adds a secondary user to the existing database context and returns an authenticated client.
    /// </summary>
    public static async Task<(User User, HttpClient Client)> SetupAdditionalUserAsync(this WebAppTestFactory factory, DatabaseContext db)
    {
        var user = db.Users.Add(new()
        {
            Login = Faker.Internet.UserName(),
            Display = Faker.Internet.UserName(),
        });

        await db.SaveChangesAsync();
        var client = factory.CreateClient(user.Entity.Id);
        return (user.Entity, client);
    }

    /// <summary>
    /// Fetches the current workspace via API and asserts matching database records.
    /// </summary>
    public static async Task<WorkspaceDO> GetWorkspaceAsync(this HttpClient client, DatabaseContext db, User user)
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

    /// <summary>
    /// Executes an invite & accept handshake sequence, returning the primary Member ID.
    /// </summary>
    public static async Task<Guid> InviteAndAcceptAsync(this HttpClient ownerClient, HttpClient inviteeClient, Guid workspaceId, Guid inviteeId)
    {
        var inviteResponse = await ownerClient.PostAsync($"/workspace/{workspaceId}/invite/{inviteeId}", null);
        Assert.Equal(HttpStatusCode.OK, inviteResponse.StatusCode);

        var acceptResponse = await inviteeClient.PostAsync($"/workspace/{workspaceId}/invite/accept", null);
        Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);

        var member = await acceptResponse.Content.ReadFromJsonAsync<MemberDO>(JsonOptions.Default);
        Assert.NotNull(member);
        return member.Id;
    }

    // ---- Entity Creation Sinks ----------------------------------------

    public static async Task<ProjectDO> CreateProjectAsync(this HttpClient client, Guid workspaceId)
    {
        var response = await client.PostAsJsonAsync($"/workspace/{workspaceId}/project", new PostProjectRequestDTO
        {
            Name = Faker.Internet.DomainName(),
            Description = Faker.Lorem.Paragraph(1),
            Active = true,
            MaxMembers = 1,
            Public = true,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ProjectDO>(JsonOptions.Default);
        Assert.NotNull(result);
        return result;
    }

    public static async Task<GoalDO> CreateGoalAsync(this HttpClient client, Guid workspaceId)
    {
        var response = await client.PostAsJsonAsync($"/workspace/{workspaceId}/goal", new PostGoalRequestDTO
        {
            Name = Faker.Internet.DomainName(),
            Description = Faker.Lorem.Paragraph(1),
            Active = true,
            Public = true,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GoalDO>(JsonOptions.Default);
        Assert.NotNull(result);
        return result;
    }

    public static async Task<CursusDO> CreateCursusAsync(this HttpClient client, Guid workspaceId)
    {
        var response = await client.PostAsJsonAsync($"/workspace/{workspaceId}/cursus", new PostCursusRequestDTO
        {
            Name = Faker.Internet.DomainName(),
            Description = Faker.Lorem.Paragraph(1),
            Active = true,
            Public = true,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CursusDO>(JsonOptions.Default);
        Assert.NotNull(result);
        return result;
    }

    public static async Task<RubricDO> CreateRubricAsync(this HttpClient client, Guid workspaceId)
    {
        var response = await client.PostAsJsonAsync($"/workspace/{workspaceId}/rubric", new PostRubricEntityRequestDTO
        {
            Name = Faker.Internet.DomainName(),
            Public = true,
            Variants = [new() { Kind = ReviewKinds.Peer, Required = 1 }],
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RubricDO>(JsonOptions.Default);
        Assert.NotNull(result);
        return result;
    }
}