// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Core.Query;
using App.Backend.API.Params;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;
using App.Backend.Models;
using App.Backend.Models.Responses.Entities.Projects;
using Microsoft.EntityFrameworkCore;
using App.Backend.API.Controllers.Interfaces;
using Wolverine;
using App.Backend.API.Notifications.Variants;

// ============================================================================

namespace App.Backend.API.Controllers;

/// <summary>
/// Operations on user project sessions.
///
/// Supports two access patterns:
/// <list type="bullet">
///   <item>Nested: <c>GET /users/{userId}/projects</c> — scoped listing and lookup by project ID</item>
///   <item>Direct: <c>GET /user-projects/{id}</c> — lookup by UserProject entity ID, members, or transactions</item>
/// </list>
/// </summary>
[ApiController]
[Route("users/{userId:guid}/projects"), Tags("UserProjects")]
[Authorize]
public class UserProjectController(
    IUserProjectService service,
    IProjectService projectService,
    IMemberService memberService,
    IMessageBus bus
) : Controller, IInviteController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List user project sessions")]
    [EndpointDescription("Returns all active project sessions the user is a member of. Supports filtering by name, slug, and state.")]
    public async Task<ActionResult<IEnumerable<UserProjectDO>>> GetByUser(
        Guid userId,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[slug]")] string? slug,
        [FromQuery(Name = "filter[state]")] EntityObjectState? state,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken token
    )
    {
        // Members no longer exist as a navigation property on UserProject.
        // The membership predicate is now a correlated subquery against tbl_members,
        // expressed via the injected DbContext inside the service — the predicate
        // lambda is replaced by a dedicated service method that builds the correct query.
        var page = await service.GetAllAsync(sorting, pagination, token,
            // up => memberService.HasActiveMember(up.Id, userId),
            name is null ? null : up => up.Project.Name.Contains(name),
            slug is null ? null : up => up.Project.Slug == slug,
            state is null ? null : up => up.State == state
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(up => new UserProjectDO(up)));
    }

    [HttpGet("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user project by project ID")]
    [EndpointDescription("Finds the user's session for a specific project by the project's own ID.")]
    public async Task<ActionResult<UserProjectDO>> GetByUserAndProject(
        Guid userId, Guid projectId, CancellationToken token
    )
    {
        var up = await service.FindByUserAndProjectAsync(userId, projectId, token);
        return up is null ? NotFound() : Ok(new UserProjectDO(up));
    }

    [HttpGet("/user-projects/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user project by entity ID")]
    [EndpointDescription("Finds a user project session directly by its own entity ID, without requiring a user context.")]
    public async Task<ActionResult<UserProjectDO>> GetById(Guid id, CancellationToken token)
    {
        var up = await service.FindByIdAsync(id, token);
        return up is null ? NotFound() : Ok(new UserProjectDO(up));
    }

    // [HttpGet("/user-projects/{id:guid}/members")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Get project session members")]
    // [EndpointDescription("Returns all current and past members of the specified user project session.")]
    // public async Task<ActionResult<IEnumerable<MemberDO>>> GetMembers(Guid id, CancellationToken token)
    // {
    //     // up.Members is gone — fetch directly from the member service instead.
    //     // The session existence check is preserved: 404 if the session doesn't exist,
    //     // empty list if it exists but has no members (shouldn't happen in practice).
    //     var up = await userProjectService.FindByIdAsync(id, token);
    //     if (up is null) return NotFound();

    //     var members = await memberService.GetProjectMembersAsync(id, token);
    //     return Ok(members.Select(m => new MemberDO(m)));
    // }

    [HttpGet("/user-projects/{id:guid}/transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get project session transactions")]
    [EndpointDescription("Returns the paginated activity timeline of the specified user project session, ordered by the requested sort.")]
    public async Task<ActionResult<IEnumerable<UserProjectTransactionDO>>> GetTransactions(
        Guid id,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken token
    )
    {
        var page = await service.GetTransactionsAsync(id, sorting, pagination, token);
        if (page is null) return NotFound();

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(t => new UserProjectTransactionDO(t)));
    }

    [HttpPost("/user-projects/{id:guid}/invite/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Invite a user to a project session")]
    [EndpointDescription("The calling user (leader) invites another user to their active project session.")]
    public async Task<ActionResult<MemberDO>> InviteAsync(Guid Id, Guid userId, CancellationToken token)
    {
        var up = await service.FindByIdAsync(Id, token);
        if (up is null) return NotFound();
        if (up.State is not EntityObjectState.Active)
            return UnprocessableEntity();

        var member = await memberService.InviteAsync(
            up.Id,
            userId,
            up.GitInfoId,
            up.Project.MaxMembers,
        token);

        await service.LogTransactionAsync(
            up.Id,
            User.GetSID(),
            UserProjectTransactionVariant.MemberInvited,
        token);

        await bus.PublishAsync(new ProjectInviteNotification(userId, User.GetSID(), up.Id));
        return Ok(new MemberDO(member));
    }

    [HttpDelete("/user-projects/{id:guid}/invite/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Cancel a pending invite")]
    [EndpointDescription("The session leader cancels a pending invitation before it is accepted.")]
    public async Task<ActionResult<MemberDO>> UninviteAsync(Guid Id, Guid userId, CancellationToken token)
    {
        var up = await service.FindByIdAsync(Id, token);
        if (up is null) return NotFound();
        if (up.State is not EntityObjectState.Active)
            return UnprocessableEntity();

        var member = await memberService.UnInviteAsync(Id, userId, token);
        await service.LogTransactionAsync(
            up.Id,
            User.GetSID(),
            UserProjectTransactionVariant.MemberUninvited,
            token
        );

        return Ok(new MemberDO(member));
    }

    [HttpPost("/user-projects/{id:guid}/invite/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Accept a project invite")]
    [EndpointDescription("The invited user accepts a pending invitation to join a project session.")]
    public async Task<ActionResult<MemberDO>> AcceptAsync(Guid Id, CancellationToken token)
    {
        var up = await service.FindByIdAsync(Id, token);
        if (up is null) return NotFound();
        if (up.State is not EntityObjectState.Active)
            return UnprocessableEntity();

        var member = await memberService.FindByEntityAndUserId(Id, User.GetSID(), token);
        if (member is null) return NotFound();


        member = await memberService.AcceptAsync(member.Id, token);
        await service.LogTransactionAsync(
            up.Id,
            User.GetSID(),
            UserProjectTransactionVariant.MemberAccepted,
            token
        );

        return Ok(new MemberDO(member));
    }

    [HttpPost("/user-projects/{id:guid}/invite/decline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Decline a project invite")]
    [EndpointDescription("The invited user declines a pending invitation to join a project session.")]
    public async Task<ActionResult<MemberDO>> DeclineAsync(Guid Id, CancellationToken token)
    {
        var up = await service.FindByIdAsync(Id, token);
        if (up is null) return NotFound();
        if (up.State is not EntityObjectState.Active)
            return UnprocessableEntity();

        var member = await memberService.FindByEntityAndUserId(Id, User.GetSID(), token);
        if (member is null) return NotFound();


        member = await memberService.DeclineAsync(member.Id, token);
        await service.LogTransactionAsync(
            up.Id,
            User.GetSID(),
            UserProjectTransactionVariant.MemberAccepted,
            token
        );

        return Ok(new MemberDO(member));
    }

    [HttpPut("/user-projects/{id:guid}/member/transfer/{newLeaderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Transfer project leadership")]
    [EndpointDescription("Transfer leadership of a project session to another user.")]
    public async Task<ActionResult> TransferLeadershipAsync(Guid Id, Guid newLeaderId, CancellationToken token)
    {
        var up = await service.FindByIdAsync(Id, token);
        if (up is null) return NotFound();
        if (up.State is not EntityObjectState.Active)
            return UnprocessableEntity();

        // await memberService.SetRoleAsync(
        //     MemberEntityType.UserProject,
        //     Id,
        //     User.GetSID(),
        //     newLeaderId,
        //     token
        // );

        await service.LogTransactionAsync(
            up.Id,
            User.GetSID(),
            UserProjectTransactionVariant.LeadershipTransferred,
            token
        );

        return NoContent();
    }

    [HttpPost("/user-projects/{id:guid}/member/leave")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Leave a project")]
    [EndpointDescription("The user leaves a project session.")]
    public async Task<ActionResult> LeaveAsync(Guid Id, CancellationToken token)
    {
        var up = await service.FindByIdAsync(Id, token);
        if (up is null) return NotFound();
        if (up.State is not EntityObjectState.Active)
            return UnprocessableEntity();

        // await memberService.LeaveAsync(
        //     MemberEntityType.UserProject,
        //     Id,
        //     User.GetSID(),
        //     token
        // );

        await service.LogTransactionAsync(
            up.Id,
            User.GetSID(),
            UserProjectTransactionVariant.MemberLeft,
            token
        );

        return NoContent();
    }

    [HttpPost("/user-projects/{id:guid}/member/kick/{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Kick a member from a project")]
    [EndpointDescription("Remove a member from a project session.")]
    public async Task<ActionResult> KickAsync(Guid Id, Guid memberId, CancellationToken token)
    {
        var up = await service.FindByIdAsync(Id, token);
        if (up is null) return NotFound();
        if (up.State is not EntityObjectState.Active)
            return UnprocessableEntity();

        // await memberService.KickAsync(
        //     MemberEntityType.UserProject,
        //     Id,
        //     User.GetSID(),
        //     memberId,
        //     token
        // );

        await service.LogTransactionAsync(
            up.Id,
            User.GetSID(),
            UserProjectTransactionVariant.MemberKicked,
            token
        );

        return NoContent();
    }
}