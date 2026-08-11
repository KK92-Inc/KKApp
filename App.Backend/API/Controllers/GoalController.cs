// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using App.Backend.Core.Query;
using App.Backend.API.Params;
using App.Backend.Core.Services.Implementation;
using App.Backend.Core.Services.Interface;
using App.Backend.Models;
using Keycloak.AuthServices.Authorization;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Requests.Goals;
using App.Backend.Models.Responses.Entities.Projects;
using App.Backend.Domain.Relations;
using App.Backend.API.Utils;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("goals")]
[ProtectedResource("goals"), Authorize]
public class GoalController(
    ILogger<GoalController> log,
    IGoalService goalService,
    IProjectService projectService,
    IWorkspaceService workspace,
    ISubscriptionService subscriptions
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
        var page = await goalService.GetAllAsync(sorting, pagination, token,
            id is null ? null : n => n.Id == id,
            workspace is null ? null : n => n.WorkspaceId == workspace,
            string.IsNullOrWhiteSpace(slug) ? null : n => n.Slug == slug,
            string.IsNullOrWhiteSpace(name) ? null : g => EF.Functions.ILike(g.Name, $"%{name}%")
        );
        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(g => new GoalDO(g)));
    }

    [Tags("Workspace")]
    [HttpDelete("{id:guid}")]
    [RequireScope("workspace")]
    [ProtectedResource("goals", "goals:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Delete a goal")]
    [EndpointDescription("Delete a goal and its associations")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var goal = await goalService.FindByIdAsync(id);
        if (goal is null)
            return NotFound();
        await goalService.DeleteAsync(goal);
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
    public async Task<ActionResult<GoalDO>> GetById(Guid id)
    {
        var goal = await goalService.FindByIdAsync(id);
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
    public async Task<ActionResult<GoalDO>> Update(Guid id, [FromBody] PatchGoalRequestDTO request)
    {
        var goal = await goalService.FindByIdAsync(id);
        if (goal is null)
            return NotFound();


        goal.Name = request.Name ?? goal.Name;
        goal.Description = request.Description ?? goal.Description;
        // goal.Slug = request.Name?.ToSlug() ?? goal.Slug;
        await goalService.UpdateAsync(goal);
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
    public async Task<ActionResult<IEnumerable<ProjectDO>>> GetGoalProjects(Guid id)
    {
        var projects = await goalService.GetProjectsAsync(id);
        return Ok(projects.Select(p => new ProjectDO(p)));
    }

    [HttpPost("{id:guid}/projects")]
    [RequireScope("workspace")]
    [ProtectedResource("goals", "goals:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Add projects to a goal")]
    [EndpointDescription("Add projects to be part of a goal")]
    public async Task<ActionResult> AddGoalProjects(
        Guid id,
        [FromBody] IEnumerable<Guid> projectIds,
        CancellationToken token
    )
    {
        // TODO: Configurable somehow, maybe we want a goal to have 20 ?
        const int MAX_PROJECT = 5;
        if (projectIds.Count() > MAX_PROJECT)
            return UnprocessableEntity($"Too many projects, max: {MAX_PROJECT}");
        if (!await projectService.ExistsAsync(projectIds, token))
            return UnprocessableEntity("One or more projects not found");

        var goal = await goalService.SetProjectsAsync(id, projectIds, token);
        if (goal is null)
            return NotFound();
        return NoContent();
    }
}
