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
using System.Linq.Expressions;
using App.Backend.API.Utils;
using App.Backend.Core;

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
    IMemberService memberService,
    IAuthorizationService auth
) : Controller, IInviteController
{
    private async Task<bool> Access(Guid entityId, CancellationToken token)
    {
        var staff = await auth.AuthorizeAsync(User, "staff");
        if (staff.Succeeded) return true;

        var current = await memberService.FindByEntityAndUserId(entityId, User.GetSID(), token);
        return current is not null && current.LeftAt is null && current.Role is MemberRole.Leader;
    }

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
        var page = await service.GetAllAsync(sorting, pagination, token,
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

    [HttpGet("/user-projects/{id:guid}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get project session members")]
    [EndpointDescription("Returns the paginated list of all members past and present")]
    public async Task<ActionResult<IEnumerable<MemberDO>>> GetMembers(
        Guid id,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        [FromQuery(Name = "filter[active]")] bool? active,
        CancellationToken token
    )
    {
        var page = await memberService.GetAllAsync(sorting, pagination, token,
            m => m.EntityType == MemberEntityType.UserProject,
            m => m.EntityId == id,
            active switch
            {
                true => m => m.LeftAt == null,
                false => m => m.LeftAt != null,
                null => null
            }
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(m => new MemberDO(m)));
    }

    [HttpPost("/user-projects/{id:guid}/invite/{userId:guid}")]
    [RequireScope("workspace")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Invite a user to a project session")]
    [EndpointDescription("The calling user (leader or staff) invites another user to their active project session.")]
    public async Task<ActionResult<MemberDO>> InviteAsync(Guid id, Guid userId, CancellationToken token)
    {
        if (!await Access(id, token))
            return Forbid();

        var up = await service.FindByIdAsync(id, token);
        if (up is null) return NotFound();
        if (up.State is EntityObjectState.Completed)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project completed; you cannot modify members anymore."
            });
        }
        if (up.State is not EntityObjectState.Active)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project is not in an active state."
            });
        }
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

        // await bus.PublishAsync(new InviteNotification(userId, User.GetSID(), up.Id));
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
    [EndpointDescription("The session leader or staff cancels a pending invitation before it is accepted.")]
    public async Task<ActionResult<MemberDO>> UninviteAsync(Guid id, Guid userId, CancellationToken token)
    {
        if (!await Access(id, token))
            return Forbid();

        var up = await service.FindByIdAsync(id, token);
        if (up is null) return NotFound();
        if (up.State is EntityObjectState.Completed)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project completed; you cannot modify members anymore."
            });
        }
        if (up.State is not EntityObjectState.Active)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project is not in an active state."
            });
        }

        var member = await memberService.FindByEntityAndUserId(id, userId, token);
        if (member is null) return NotFound();

        member = await memberService.UnInviteAsync(id, member.Id, token);

        await service.LogTransactionAsync(
            up.Id,
            User.GetSID(),
            UserProjectTransactionVariant.MemberUninvited,
            token
        );

        // await bus.PublishAsync(new InviteCancelledNotification(userId, User.GetSID(), up.Id));
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
    public async Task<ActionResult<MemberDO>> AcceptAsync(Guid id, CancellationToken token)
    {
        var up = await service.FindByIdAsync(id, token);
        if (up is null) return NotFound();
        if (up.State is EntityObjectState.Completed)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project completed; you cannot join members anymore."
            });
        }
        if (up.State is not EntityObjectState.Active)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project is not in an active state."
            });
        }

        var member = await memberService.FindByEntityAndUserId(id, User.GetSID(), token);
        if (member is null) return NotFound();

        member = await memberService.AcceptAsync(member.Id, token);

        await service.LogTransactionAsync(
            up.Id,
            User.GetSID(),
            UserProjectTransactionVariant.MemberAccepted,
            token
        );

        // await bus.PublishAsync(new InviteRespondedNotification(User.GetSID(), up.Id, true));
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
    public async Task<ActionResult<MemberDO>> DeclineAsync(Guid id, CancellationToken token)
    {
        var up = await service.FindByIdAsync(id, token);
        if (up is null) return NotFound();
        if (up.State is EntityObjectState.Completed)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project completed; you cannot modify members anymore."
            });
        }
        if (up.State is not EntityObjectState.Active)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project is not in an active state."
            });
        }

        var member = await memberService.FindByEntityAndUserId(id, User.GetSID(), token);
        if (member is null) return NotFound();

        member = await memberService.DeclineAsync(member.Id, token);

        await service.LogTransactionAsync(
            up.Id,
            User.GetSID(),
            UserProjectTransactionVariant.MemberDeclined,
            token
        );

        // await bus.PublishAsync(new InviteRespondedNotification(User.GetSID(), up.Id, false));
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
    public async Task<ActionResult> TransferLeadershipAsync(Guid id, Guid newLeaderId, CancellationToken token)
    {
        if (!await Access(id, token))
            return Forbid();

        var up = await service.FindByIdAsync(id, token);
        if (up is null) return NotFound();
        if (up.State is EntityObjectState.Completed)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project completed; you cannot modify members anymore."
            });
        }
        if (up.State is not EntityObjectState.Active)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project is not in an active state."
            });
        }

        var targetMember = await memberService.FindByEntityAndUserId(id, newLeaderId, token);
        if (targetMember is null || targetMember.LeftAt is not null)
            return NotFound();

        // If the calling user is the current leader, demote them to regular Member
        var currentLeaderMember = await memberService.FindByEntityAndUserId(id, User.GetSID(), token);
        if (currentLeaderMember is not null && currentLeaderMember.Role == MemberRole.Leader)
        {
            await memberService.SetRoleAsync(currentLeaderMember.Id, MemberRole.Member, token);
        }

        await memberService.SetRoleAsync(targetMember.Id, MemberRole.Leader, token);

        await service.LogTransactionAsync(
            up.Id,
            User.GetSID(),
            UserProjectTransactionVariant.LeadershipTransferred,
            token
        );

        // await bus.PublishAsync(new LeadershipTransferredNotification(newLeaderId, User.GetSID(), up.Id));
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
    public async Task<ActionResult> LeaveAsync(Guid id, CancellationToken token)
    {
        var up = await service.FindByIdAsync(id, token);
        if (up is null) return NotFound();
        if (up.State is EntityObjectState.Completed)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project completed; you cannot leave members anymore."
            });
        }
        if (up.State is not EntityObjectState.Active)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project is not in an active state."
            });
        }

        var member = await memberService.FindByEntityAndUserId(id, User.GetSID(), token);
        if (member is null || member.LeftAt is not null) return NotFound();

        await memberService.LeaveAsync(member.Id, token);

        await service.LogTransactionAsync(
            up.Id,
            User.GetSID(),
            UserProjectTransactionVariant.MemberLeft,
            token
        );

        // await bus.PublishAsync(new MemberLeftNotification(User.GetSID(), up.Id));
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
    [EndpointDescription("Remove a member from a project session. Requires Leader role or staff authorization.")]
    public async Task<ActionResult> KickAsync(Guid id, Guid memberId, CancellationToken token)
    {
        if (!await Access(id, token))
            return Forbid();

        var up = await service.FindByIdAsync(id, token);
        if (up is null) return NotFound();
        if (up.State is EntityObjectState.Completed)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project completed; you cannot kick members anymore."
            });
        }
        if (up.State is not EntityObjectState.Active)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Project is not in an active state."
            });
        }

        var member = await memberService.FindByIdAsync(memberId, token);
        if (member is null || member.EntityId != id || member.LeftAt is not null)
            return NotFound();

        await memberService.KickAsync(member.Id, token);

        await service.LogTransactionAsync(
            up.Id,
            User.GetSID(),
            UserProjectTransactionVariant.MemberKicked,
            token
        );

        // await bus.PublishAsync(new MemberKickedNotification(member.UserId, User.GetSID(), up.Id));
        return NoContent();
    }
}