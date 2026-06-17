// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Net;
using System.Net.Http.Json;
using App.Backend.Core;
using App.Backend.Database;
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

// ============================================================================

namespace App.Backend.Tests.Integration.Entities;

public class WorkspaceIntegrationTests2
{
    private static readonly Faker Faker = new();

    #region Create tests

    [Fact]
    public async Task Workspace_Current_Should_Return_Workspace()
    {
        var (_, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        Assert.Equal(user.Id, workspace.Owner?.Id);
    }

    [Fact]
    public async Task Workspace_Create_Project()
    {
        var (_, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);

        var name = Faker.Internet.DomainName();
        var response = await client.PostAsJsonAsync($"/workspace/{workspace.Id}/project", new PostProjectRequestDTO
        {
            Name = name,
            Description = Faker.Lorem.Paragraph(1),
            Active = true,
            MaxMembers = 1,
            Public = true,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var project = await response.Content.ReadFromJsonAsync<ProjectDO>(JsonOptions.Default);
        Assert.NotNull(project);
        Assert.Equal(name, project.Name);
        Assert.NotNull(project.GitInfo);
    }

    [Fact]
    public async Task Workspace_Create_Goal()
    {
        var (_, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);

        var name = Faker.Internet.DomainName();
        var response = await client.PostAsJsonAsync($"/workspace/{workspace.Id}/goal", new PostGoalRequestDTO
        {
            Name = name,
            Description = Faker.Lorem.Paragraph(1),
            Active = true,
            Public = true,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var goal = await response.Content.ReadFromJsonAsync<GoalDO>(JsonOptions.Default);
        Assert.NotNull(goal);
        Assert.Equal(name, goal.Name);
    }

    [Fact]
    public async Task Workspace_Create_Cursus()
    {
        var (_, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);

        var name = Faker.Internet.DomainName();
        var response = await client.PostAsJsonAsync($"/workspace/{workspace.Id}/cursus", new PostCursusRequestDTO
        {
            Name = name,
            Description = Faker.Lorem.Paragraph(1),
            Active = true,
            Public = true,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cursus = await response.Content.ReadFromJsonAsync<CursusDO>(JsonOptions.Default);
        Assert.NotNull( cursus);
        Assert.Equal(name, cursus.Name);
    }

    [Fact]
    public async Task Workspace_Create_Rubric()
    {
        var (_, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);

        var name = Faker.Internet.DomainName();
        var response = await client.PostAsJsonAsync($"/workspace/{workspace.Id}/rubric", new PostRubricEntityRequestDTO
        {
            Name = name,
            Public = true,
            Variants = [new() { Kind = ReviewKinds.Peer, Required = 2 }],
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rubric = await response.Content.ReadFromJsonAsync<RubricDO>(JsonOptions.Default);
        Assert.NotNull(rubric);
        Assert.Equal(name, rubric.Name);
        Assert.NotNull(rubric.GitInfo);
        Assert.Single(rubric.Variants);
        Assert.Equal(2, rubric.Variants.First().Requires);
        Assert.Equal(ReviewKinds.Peer, rubric.Variants.First().Kind);
    }

    #endregion

    #region Delete tests

    [Fact]
    public async Task Workspace_Delete_Project()
    {
        var (_, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var project = await client.CreateProjectAsync(workspace.Id);

        var response = await client.DeleteAsync($"/projects/{project.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var found = await db.Projects.FindAsync(project.Id);
        Assert.True(found is null || found.Deprecated);
    }

    [Fact]
    public async Task Workspace_Delete_Goal()
    {
        var (_, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var goal = await client.CreateGoalAsync(workspace.Id);

        var response = await client.DeleteAsync($"/goals/{goal.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var found = await db.Goals.FindAsync(goal.Id);
        Assert.True(found is null || found.Deprecated);
    }

    [Fact]
    public async Task Workspace_Delete_Cursus()
    {
        var (_, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var cursus = await client.CreateCursusAsync(workspace.Id);

        var response = await client.DeleteAsync($"/cursus/{cursus.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var found = await db.Cursi.FindAsync(cursus.Id);
        Assert.True(found is null || found.Deprecated);
    }

    [Fact]
    public async Task Workspace_Delete_Rubric()
    {
        var (_, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var rubric = await client.CreateRubricAsync(workspace.Id);

        var response = await client.DeleteAsync($"/rubrics/{rubric.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var found = await db.Rubrics.FindAsync(rubric.Id);
        Assert.True(found is null || found.Deprecated);
    }

    #endregion

    #region Transfer tests

    [Fact]
    public async Task Workspace_Transfer_Project()
    {
        var (factory, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var fromWorkspace = await ownerClient.GetWorkspaceAsync(db, owner);
        var (toUser, toClient) = await factory.SetupAdditionalUserAsync(db);
        var toWorkspace = await toClient.GetWorkspaceAsync(db, toUser);

        var project = await ownerClient.CreateProjectAsync(fromWorkspace.Id);
        var response = await ownerClient.PostAsJsonAsync(
            $"/workspace/{fromWorkspace.Id}/transfer/project/{toWorkspace.Id}",
            new[] { project.Id });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var transferred = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == project.Id);
        Assert.Equal(toWorkspace.Id, transferred?.WorkspaceId);
    }

    [Fact]
    public async Task Workspace_Transfer_Goal()
    {
        var (factory, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var fromWorkspace = await ownerClient.GetWorkspaceAsync(db, owner);
        var (toUser, toClient) = await factory.SetupAdditionalUserAsync(db);
        var toWorkspace = await toClient.GetWorkspaceAsync(db, toUser);

        var goal = await ownerClient.CreateGoalAsync(fromWorkspace.Id);
        var response = await ownerClient.PostAsJsonAsync(
            $"/workspace/{fromWorkspace.Id}/transfer/goal/{toWorkspace.Id}",
            new[] { goal.Id });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var transferred = await db.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
        Assert.Equal(toWorkspace.Id, transferred?.WorkspaceId);
    }

    [Fact]
    public async Task Workspace_Transfer_Cursus()
    {
        var (factory, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var fromWorkspace = await ownerClient.GetWorkspaceAsync(db, owner);
        var (toUser, toClient) = await factory.SetupAdditionalUserAsync(db);
        var toWorkspace = await toClient.GetWorkspaceAsync(db, toUser);

        var cursus = await ownerClient.CreateCursusAsync(fromWorkspace.Id);
        var response = await ownerClient.PostAsJsonAsync(
            $"/workspace/{fromWorkspace.Id}/transfer/cursus/{toWorkspace.Id}",
            new[] { cursus.Id });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var transferred = await db.Cursi.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cursus.Id);
        Assert.Equal(toWorkspace.Id, transferred?.WorkspaceId);
    }

    #endregion

    #region Invite tests

    [Fact]
    public async Task Workspace_Invite_User()
    {
        var (factory, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var workspace = await ownerClient.GetWorkspaceAsync(db, owner);
        var (invitee, _) = await factory.SetupAdditionalUserAsync(db);

        var response = await ownerClient.PostAsync($"/workspace/{workspace.Id}/invite/{invitee.Id}", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var member = await response.Content.ReadFromJsonAsync<MemberDO>(JsonOptions.Default);
        Assert.NotNull(member);
        Assert.Equal(invitee.Id, member.UserId);
        Assert.Equal(workspace.Id, member.EntityId);
        Assert.Equal(MemberRole.Pending, member.Role);
    }

    [Fact]
    public async Task Workspace_Cancel_Invite()
    {
        var (factory, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var workspace = await ownerClient.GetWorkspaceAsync(db, owner);
        var (invitee, _) = await factory.SetupAdditionalUserAsync(db);

        var inviteResponse = await ownerClient.PostAsync($"/workspace/{workspace.Id}/invite/{invitee.Id}", null);
        Assert.Equal(HttpStatusCode.OK, inviteResponse.StatusCode);

        var cancelResponse = await ownerClient.DeleteAsync($"/workspace/{workspace.Id}/invite/{invitee.Id}");
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        var member = await db.Members.FirstOrDefaultAsync(m =>
            m.EntityId == workspace.Id &&
            m.EntityType == MemberEntityType.Workspace &&
            m.UserId == invitee.Id &&
            m.Role == MemberRole.Pending);

        Assert.Null(member);
    }

    [Fact]
    public async Task Workspace_Accept_Invite()
    {
        var (factory, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var workspace = await ownerClient.GetWorkspaceAsync(db, owner);
        var (invitee, inviteeClient) = await factory.SetupAdditionalUserAsync(db);

        var inviteResponse = await ownerClient.PostAsync($"/workspace/{workspace.Id}/invite/{invitee.Id}", null);
        Assert.Equal(HttpStatusCode.OK, inviteResponse.StatusCode);

        var acceptResponse = await inviteeClient.PostAsync($"/workspace/{workspace.Id}/invite/accept", null);
        Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);

        var member = await db.Members.FirstOrDefaultAsync(m =>
            m.EntityId == workspace.Id &&
            m.EntityType == MemberEntityType.Workspace &&
            m.UserId == invitee.Id &&
            m.LeftAt == null);

        Assert.NotNull(member);
        Assert.Equal(MemberRole.Member, member.Role);
    }

    [Fact]
    public async Task Workspace_Decline_Invite()
    {
        var (factory, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var workspace = await ownerClient.GetWorkspaceAsync(db, owner);
        var (invitee, inviteeClient) = await factory.SetupAdditionalUserAsync(db);

        var inviteResponse = await ownerClient.PostAsync($"/workspace/{workspace.Id}/invite/{invitee.Id}", null);
        Assert.Equal(HttpStatusCode.OK, inviteResponse.StatusCode);

        var declineResponse = await inviteeClient.PostAsync($"/workspace/{workspace.Id}/invite/decline", null);
        Assert.Equal(HttpStatusCode.OK, declineResponse.StatusCode);

        var member = await db.Members.FirstOrDefaultAsync(m =>
            m.EntityId == workspace.Id &&
            m.EntityType == MemberEntityType.Workspace &&
            m.UserId == invitee.Id &&
            m.Role == MemberRole.Member);

        Assert.Null(member);
    }

    #endregion

    #region Member tests

    [Fact]
    public async Task Workspace_Member_Leave()
    {
        var (factory, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var workspace = await ownerClient.GetWorkspaceAsync(db, owner);
        var (invitee, inviteeClient) = await factory.SetupAdditionalUserAsync(db);

        await ownerClient.InviteAndAcceptAsync(inviteeClient, workspace.Id, invitee.Id);

        var leaveResponse = await inviteeClient.PostAsync($"/workspace/{workspace.Id}/member/leave", null);
        Assert.Equal(HttpStatusCode.OK, leaveResponse.StatusCode);

        var member = await db.Members.FirstOrDefaultAsync(m =>
            m.EntityId == workspace.Id &&
            m.EntityType == MemberEntityType.Workspace &&
            m.UserId == invitee.Id);

        Assert.True(member is null || member.LeftAt is not null);
    }

    [Fact]
    public async Task Workspace_Member_Kick()
    {
        var (factory, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var workspace = await ownerClient.GetWorkspaceAsync(db, owner);
        var (invitee, inviteeClient) = await factory.SetupAdditionalUserAsync(db);

        var memberId = await ownerClient.InviteAndAcceptAsync(inviteeClient, workspace.Id, invitee.Id);

        var kickResponse = await ownerClient.PostAsync($"/workspace/{workspace.Id}/member/kick/{memberId}", null);
        Assert.Equal(HttpStatusCode.OK, kickResponse.StatusCode);

        var member = await db.Members.FirstOrDefaultAsync(m =>
            m.EntityId == workspace.Id &&
            m.EntityType == MemberEntityType.Workspace &&
            m.UserId == invitee.Id);

        Assert.True(member is null || member.LeftAt is not null);
    }

    #endregion

    #region Failure tests

    [Fact]
    public async Task Workspace_Create_Project_Duplicate_Should_Fail()
    {
        var (_, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var name = Faker.Internet.DomainName();
        var request = new PostProjectRequestDTO
        {
            Name = name,
            Description = Faker.Lorem.Paragraph(1),
            Active = true,
            MaxMembers = 1,
            Public = true,
        };

        var response1 = await client.PostAsJsonAsync($"/workspace/{workspace.Id}/project", request);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var response2 = await client.PostAsJsonAsync($"/workspace/{workspace.Id}/project", request);
        Assert.True(response2.StatusCode == HttpStatusCode.Conflict || response2.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Workspace_Create_Goal_Duplicate_Should_Fail()
    {
        var (_, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var name = Faker.Internet.DomainName();
        var request = new PostGoalRequestDTO
        {
            Name = name,
            Description = Faker.Lorem.Paragraph(1),
            Active = true,
            Public = true,
        };

        var response1 = await client.PostAsJsonAsync($"/workspace/{workspace.Id}/goal", request);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var response2 = await client.PostAsJsonAsync($"/workspace/{workspace.Id}/goal", request);
        Assert.True(response2.StatusCode == HttpStatusCode.Conflict || response2.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Workspace_Create_Cursus_Duplicate_Should_Fail()
    {
        var (_, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var name = Faker.Internet.DomainName();
        var request = new PostCursusRequestDTO
        {
            Name = name,
            Description = Faker.Lorem.Paragraph(1),
            Active = true,
            Public = true,
        };

        var response1 = await client.PostAsJsonAsync($"/workspace/{workspace.Id}/cursus", request);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var response2 = await client.PostAsJsonAsync($"/workspace/{workspace.Id}/cursus", request);
        Assert.True(response2.StatusCode == HttpStatusCode.Conflict || response2.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Workspace_Create_Rubric_Duplicate_Should_Fail()
    {
        var (_, db, user, client) = await TestUtils.SetupAsync();
        var workspace = await client.GetWorkspaceAsync(db, user);
        var name = Faker.Internet.DomainName();
        var request = new PostRubricEntityRequestDTO
        {
            Name = name,
            Public = true,
            Variants = [new() { Kind = ReviewKinds.Peer, Required = 2 }],
        };

        var response1 = await client.PostAsJsonAsync($"/workspace/{workspace.Id}/rubric", request);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var response2 = await client.PostAsJsonAsync($"/workspace/{workspace.Id}/rubric", request);
        Assert.True(response2.StatusCode == HttpStatusCode.Conflict || response2.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Workspace_Invite_NonExistent_User_Should_Return_NotFound()
    {
        var (_, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var workspace = await ownerClient.GetWorkspaceAsync(db, owner);
        var nonExistentUserId = Guid.NewGuid();

        var response = await ownerClient.PostAsync($"/workspace/{workspace.Id}/invite/{nonExistentUserId}", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Workspace_Invite_User_Twice_Should_Return_Conflict()
    {
        var (factory, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var workspace = await ownerClient.GetWorkspaceAsync(db, owner);
        var (invitee, _) = await factory.SetupAdditionalUserAsync(db);

        var response1 = await ownerClient.PostAsync($"/workspace/{workspace.Id}/invite/{invitee.Id}", null);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var response2 = await ownerClient.PostAsync($"/workspace/{workspace.Id}/invite/{invitee.Id}", null);
        Assert.True(response2.StatusCode is HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Workspace_Uninvite_Self_Should_Fail()
    {
        var (_, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var workspace = await ownerClient.GetWorkspaceAsync(db, owner);

        var response = await ownerClient.DeleteAsync($"/workspace/{workspace.Id}/invite/{owner.Id}");
        Assert.True(response.StatusCode is HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Workspace_Cancel_Invite_For_Non_Invited_User_Should_Return_NotFound()
    {
        var (factory, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var workspace = await ownerClient.GetWorkspaceAsync(db, owner);
        var (otherUser, _) = await factory.SetupAdditionalUserAsync(db);

        var response = await ownerClient.DeleteAsync($"/workspace/{workspace.Id}/invite/{otherUser.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Workspace_Member_Leave_When_Not_A_Member_Should_Fail()
    {
        var (factory, db, _, _) = await TestUtils.SetupAsync();
        var (_, nonMemberClient) = await factory.SetupAdditionalUserAsync(db);
        var randomWorkspaceId = Guid.NewGuid();

        var response = await nonMemberClient.PostAsync($"/workspace/{randomWorkspaceId}/member/leave", null);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Workspace_Member_Kick_Non_Existent_Member_Should_Return_NotFound()
    {
        var (_, db, owner, ownerClient) = await TestUtils.SetupAsync();
        var workspace = await ownerClient.GetWorkspaceAsync(db, owner);
        var nonExistentMemberId = Guid.NewGuid();

        var response = await ownerClient.PostAsync($"/workspace/{workspace.Id}/member/kick/{nonExistentMemberId}", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}