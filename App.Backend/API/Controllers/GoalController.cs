// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Core.Query;
using App.Backend.API.Params;
using App.Backend.Core.Services.Implementation;
using App.Backend.Core.Services.Interface;
using App.Backend.Models;
using Keycloak.AuthServices.Authorization;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Requests.Goals;
using App.Backend.Models.Responses.Entities.Projects;

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
    public async Task<ActionResult> GetAll(
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination
    )
    {
        var page = await goalService.GetAllAsync(sorting, pagination);
        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(g => new GoalDO(g)));
    }

    [HttpDelete]
    [ProtectedResource("goals", "goals:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Delete a goal")]
    [EndpointDescription("Delete a goal and its associations")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var goal = await goalService.FindByIdAsync(id);
        if (goal is null)
            return NotFound();
        await goalService.DeleteAsync(goal);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
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
    [ProtectedResource("goals", "goals:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get goal projects")]
    [EndpointDescription("Retrieve projects associated with a goal")]
    public async Task<ActionResult> GetGoalProjects(Guid id)
    {
        var projects = await goalService.GetProjectsAsync(id);
        return Ok(projects.Select(p => new ProjectDO(p)));
    }

    [HttpPost("{id:guid}/projects")]
    // [ProtectedResource("goals", "goals:write")]
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

    // [HttpPost("{id:guid}/subscribe")]
    // [ProducesResponseType(StatusCodes.Status201Created)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Subscribe to goal")]
    // [EndpointDescription("Subscribe current user to a goal")]
    // public async Task<ActionResult> SubscribeToGoal(Guid id, [FromBody] SubscribeToGoalRequestDTO? request = null)
    // {
    //     var userId = User.GetSID();
    //     var subscribeRequest = request ?? new SubscribeToGoalRequestDTO { GoalId = id };

    //     var userGoal = await subscriptions.SubscribeToGoalAsync(userId, subscribeRequest);
    //     return CreatedAtAction(nameof(GetUserGoals), new { }, new UserGoalDO(userGoal));
    // }

    // [HttpGet("my-goals")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Get user goals")]
    // [EndpointDescription("Retrieve goals subscribed by current user")]
    // public async Task<ActionResult> GetUserGoals()
    // {
    //     var userId = User.GetSID();
    //     var userGoals = await subscriptions.GetUserGoalsAsync(userId);
    //     return Ok(userGoals.Select(ug => new UserGoalDO(ug)));
    // }

    // [HttpDelete("subscriptions/{id:guid}")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Unsubscribe from goal")]
    // [EndpointDescription("Unsubscribe current user from a goal")]
    // public async Task<IActionResult> UnsubscribeFromGoal(Guid id)
    // {
    //     var userId = User.GetSID();
    //     var success = await subscriptions.UnsubscribeFromGoalAsync(id, userId);
    //     return success ? NoContent() : NotFound();
    // }

    // [HttpPost("{id:guid}/collaborators")]
    // [ProducesResponseType(StatusCodes.Status201Created)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Add goal collaborator")]
    // [EndpointDescription("Add a user as collaborator to a goal")]
    // public async Task<ActionResult> AddCollaborator(Guid id, [FromBody] AddCollaboratorRequestDTO request)
    // {
    //     var collaborator = await subscriptions.AddGoalCollaboratorAsync(request.UserId, id);
    //     return CreatedAtAction(nameof(GetCollaborators), new { id = id }, new GoalCollaboratorDO(collaborator));
    // }

    // [HttpGet("{id:guid}/collaborators")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Get goal collaborators")]
    // [EndpointDescription("Retrieve collaborators for a goal")]
    // public async Task<ActionResult> GetCollaborators(Guid id)
    // {
    //     var collaborators = await subscriptions.GetGoalCollaboratorsAsync(id);
    //     return Ok(collaborators.Select(c => new GoalCollaboratorDO(c)));
    // }

    // [HttpDelete("{id:guid}/collaborators/{userId:guid}")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Remove goal collaborator")]
    // [EndpointDescription("Remove a user as collaborator from a goal")]
    // public async Task<IActionResult> RemoveCollaborator(Guid id, Guid userId)
    // {
    //     var success = await subscriptions.RemoveGoalCollaboratorAsync(userId, id);
    //     return success ? NoContent() : NotFound();
    // }
}
