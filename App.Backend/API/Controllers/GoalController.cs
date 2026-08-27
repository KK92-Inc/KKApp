// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using App.Backend.API.Params;
using App.Backend.Core.Services.Interface;
using Keycloak.AuthServices.Authorization;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Requests.Goals;
using App.Backend.Models.Responses.Entities.Projects;
using App.Backend.API.Utils;
using App.Backend.Database;
using System.Linq.Expressions;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("goals")]
[ProtectedResource("goals"), Authorize]
public class GoalController(
    IAuthorizationService auth,
    IGoalService goals,
    IMemberService members,
    DatabaseContext ctx,
    IProjectService projects
) : Controller
{
    [HttpGet]
    [ProtectedResource("goals", "goals:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all goals")]
    [EndpointDescription("Retrieve a paginated list of all goals")]
    public async Task<ActionResult<IEnumerable<GoalDO>>> GetAll(
        [FromQuery(Name = "filter[id]")] Guid? id,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[slug]")] string? slug,
        [FromQuery(Name = "filter[workspace_id]")] Guid? workspace,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        // TODO: Delete this nasty escape hatch.

        var userId = User.GetSID();
        var staff = await auth.AuthorizeAsync(User, "staff");
        Expression<Func<Goal, bool>>? visibility = staff.Succeeded
            ? null
            : g => g.Public || ctx.Members.Any(m =>
                m.EntityType == MemberEntityType.Workspace &&
                m.EntityId == g.WorkspaceId &&
                m.UserId == userId &&
                m.LeftAt == null
            );

        var page = await goals.GetAllAsync(sorting, pagination, token,
            id is null ? null : n => n.Id == id,
            workspace is null ? null : n => n.WorkspaceId == workspace,
            string.IsNullOrWhiteSpace(slug) ? null : n => n.Slug == slug,
            string.IsNullOrWhiteSpace(name) ? null : g => EF.Functions.ILike(g.Name, $"%{name}%"),
            visibility
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(g => new GoalDO(g)));
    }

    [HttpPost("{id:guid}/deprecate")]
    [RequireScope("workspace")]
    [ProtectedResource("goals", "goals:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Deprecate a goal")]
    public async Task<IActionResult> Deprecate(Guid id, CancellationToken token)
    {
        var goal = await goals.FindByIdAsync(id, token);
        if (goal is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await members.FindByEntityAndUserId(goal.WorkspaceId, User.GetSID(), token);
            if (member is null) return Forbid();
        }

        goal.Deprecated = true;
        await goals.UpdateAsync(goal, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/undeprecate")]
    [RequireScope("workspace")]
    [ProtectedResource("goals", "goals:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Undeprecate a goal")]
    public async Task<IActionResult> Undeprecate(Guid id, CancellationToken token)
    {
        var goal = await goals.FindByIdAsync(id, token);
        if (goal is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await members.FindByEntityAndUserId(goal.WorkspaceId, User.GetSID(), token);
            if (member is null) return Forbid();
        }

        goal.Deprecated = false;
        await goals.UpdateAsync(goal, token);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [RequireScope("workspace")]
    [ProtectedResource("goals", "goals:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query a goal")]
    [EndpointDescription("Retrieve a specific goal by ID")]
    public async Task<ActionResult<GoalDO>> GetById(Guid id, CancellationToken token)
    {
        var goal = await goals.FindByIdAsync(id, token);
        return goal is null ? NotFound() : Ok(new GoalDO(goal));
    }

    [HttpPatch("{id:guid}")]
    [RequireScope("workspace")]
    [ProtectedResource("goals", "goals:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Update a goal")]
    [EndpointDescription("Update goal and project associations")]
    public async Task<ActionResult<GoalDO>> Update(Guid id, [FromBody] PatchGoalRequestDTO request, CancellationToken token)
    {
        var goal = await goals.FindByIdAsync(id, token);
        if (goal is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await members.FindByEntityAndUserId(goal.WorkspaceId, User.GetSID(), token);
            if (member is null) return Forbid();
        }

        goal.Name = request.Name ?? goal.Name;
        goal.Description = request.Description ?? goal.Description;
        await goals.SetProjectsAsync(goal.Id, request.Projects, token);
        await goals.UpdateAsync(goal, token);
        return Ok(new GoalDO(goal));
    }

    [HttpGet("{id:guid}/projects")]
    [RequireScope("workspace")]
    [ProtectedResource("goals", "goals:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get goal projects")]
    [EndpointDescription("Retrieve projects associated with a goal")]
    public async Task<ActionResult<IEnumerable<ProjectDO>>> GetProjects(Guid id, CancellationToken token)
    {
        var projects = await goals.GetProjectsAsync(id, token);
        return Ok(projects.Select(p => new ProjectDO(p)));
    }

    [HttpPut("{id:guid}/projects")]
    [RequireScope("workspace")]
    [ProtectedResource("goals", "goals:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Add projects to a goal")]
    [EndpointDescription("Add projects to be part of a goal")]
    public async Task<ActionResult> SetProjects(Guid id, [FromBody] IEnumerable<Guid> ids, CancellationToken token
    )
    {
        // TODO: Configurable somehow, maybe we want a goal to have 20 ?
        const int MAX_PROJECT = 5;
        if (ids.Count() > MAX_PROJECT)
            return UnprocessableEntity($"Too many projects, max: {MAX_PROJECT}");
        if (!await projects.ExistsAsync(ids, token))
            return UnprocessableEntity("One or more projects not found");

        var goal = await goals.SetProjectsAsync(id, ids, token);
        if (goal is null)
            return NotFound();
        return NoContent();
    }
}
