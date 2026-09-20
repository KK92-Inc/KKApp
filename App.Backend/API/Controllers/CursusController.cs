// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.API.Params;
using App.Backend.Core.Services.Interface;
using Keycloak.AuthServices.Authorization;
using App.Backend.Models.Responses.Entities.Cursi;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Entities;
using App.Backend.API.Utils;
using App.Backend.Models.Requests.Cursus;
using System.Linq.Expressions;
using App.Backend.Database;
using System.ComponentModel;

// ============================================================================

namespace App.Backend.API.Controllers;

[Route("cursus")]
[ApiController, Authorize]
public class CursusController(
    IAuthorizationService auth,
    ICursusService service,
    IMemberService members,
    DatabaseContext ctx
) : Controller
{
    [HttpGet]
    [RequireScope("workspace")]
    [ProtectedResource("cursus", "cursus:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all cursus")]
    [EndpointDescription("Retrieve a paginated list of all cursus")]
    public async Task<ActionResult<IEnumerable<CursusDO>>> GetAll(
        [FromQuery(Name = "filter[id]")] Guid? id,
        [FromQuery(Name = "filter[workspace_id]")] Guid? workspace,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[slug]")] string? slug,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        // TODO: Delete this nasty escape hatch.

        var userId = User.GetSID();
        var staff = await auth.AuthorizeAsync(User, "staff");
        Expression<Func<Cursus, bool>>? visibility = staff.Succeeded
            ? null
            : c => c.Public || ctx.Members.Any(m =>
                m.EntityType == MemberEntityType.Workspace &&
                m.EntityId == c.WorkspaceId &&
                m.UserId == userId &&
                m.LeftAt == null
            );

        var page = await service.GetAllAsync(sorting, pagination, token,
            id is null ? null : n => n.Id == id,
            workspace is null ? null : n => n.WorkspaceId == workspace,
            string.IsNullOrWhiteSpace(name) ? null : n => EF.Functions.ILike(n.Name, $"%{name}%"),
            string.IsNullOrWhiteSpace(slug) ? null : n => n.Slug == slug,
            visibility
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(c => new CursusDO(c)));
    }

    [HttpPost("{id:guid}/deprecate")]
    [RequireScope("workspace")]
    [ProtectedResource("cursus", "cursus:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Deprecate a cursus")]
    public async Task<IActionResult> Deprecate(Guid id, CancellationToken token)
    {
        var cursus = await service.FindByIdAsync(id, token);
        if (cursus is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await members.FindByEntityAndUserId(cursus.WorkspaceId, User.GetSID(), token);
            if (member is null) return Forbid();
        }

        cursus.Deprecated = true;
        await service.UpdateAsync(cursus, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/undeprecate")]
    [RequireScope("workspace")]
    [ProtectedResource("cursus", "cursus:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Undeprecate a cursus")]
    public async Task<IActionResult> Undeprecate(Guid id, CancellationToken token)
    {
        var cursus = await service.FindByIdAsync(id, token);
        if (cursus is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await members.FindByEntityAndUserId(cursus.WorkspaceId, User.GetSID(), token);
            if (member is null) return Forbid();
        }

        cursus.Deprecated = false;
        await service.UpdateAsync(cursus, token);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [RequireScope("workspace")]
    [ProtectedResource("cursus", "cursus:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query a cursus")]
    [EndpointDescription("Retrieve a specific cursus by ID")]
    public async Task<ActionResult<CursusDO>> GetById(
        Guid id,
        [FromQuery(Name = "access[user_id]"), Description("Additionally queries if the user has access.")]
        Guid? userId,
        CancellationToken token
    )
    {
        var cursus = await service.FindByIdAsync(id, token);
        if (cursus is null) return NotFound();

        if (userId.HasValue)
        {
            var isStaff = await auth.AuthorizeAsync(User, "staff");
            if (!isStaff.Succeeded)
            {
                var member = await members.FindByEntityAndUserId(cursus.WorkspaceId, User.GetSID(), token);
                if (member is null) return Forbid();
            }

        }

        return Ok(new CursusDO(cursus));
    }

    [HttpGet("{id:guid}/track")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get cursus track")]
    [EndpointDescription("Retrieve the hierarchical goal tree of a static cursus.")]
    public async Task<ActionResult<CursusTrackDO>> GetTrack(Guid id, CancellationToken token)
    {
        var cursus = await service.FindByIdAsync(id, token);
        if (cursus is null) return NotFound();
        var track = await service.GetTrackAsync(id, token);
        return Ok(service.AssembleTrack(cursus, track));
    }

    /// <summary>
    /// Replaces a cursus's track. Existing subscribers are unaffected - this only
    /// changes what future subscribers are cloned into. See SubscriptionService for
    /// where that clone happens.
    /// </summary>
    [HttpPost("{id:guid}/track")]
    [ProtectedResource("cursus", "cursus:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Replace cursus track")]
    [EndpointDescription("Fully replaces the hierarchical goal track for a static cursus. Existing subscribers are not affected.")]
    public async Task<ActionResult<CursusTrackDO>> SetTrack(Guid id, [FromBody] PutCursusTrackRequestDTO body, CancellationToken token)
    {
        var cursus = await service.FindByIdAsync(id, token);
        if (cursus is null) return NotFound();
    
        await service.ValidateTrackAsync([.. body.Nodes.Select(n => (n.GoalId, n.ParentId))], token);
        var nodes = body.Nodes.Select(n => new CursusGoal
        {
            CursusId = id,
            GoalId = n.GoalId,
            ParentGoalId = n.ParentId,
        });

        var track = await service.SetTrackAsync(id, nodes, token);
        return Ok(service.AssembleTrack(cursus, track));
    }
}
