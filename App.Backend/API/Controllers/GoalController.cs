// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
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
public class GoalController(ILogger<GoalController> log, IGoalService goals, ISubscriptionService subscriptions) : Controller
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
        var page = await goals.GetAllAsync(sorting, pagination);
        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(g => new GoalDO(g)));
    }

    [HttpPost]
    [ProtectedResource("goals", "goals:write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Create a new goal")]
    [EndpointDescription("Create a new goal with optional project associations")]
    public async Task<ActionResult<GoalDO>> Create([FromBody] PostGoalRequestDTO request)
    {
        await goals.CreateAsync(new () 
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description ?? string.Empty,
        });

        // TODO: Project associations

        return Created();
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
        var goal = await goals.FindByIdAsync(id);
        if (goal is null)
            return NotFound();
        await goals.DeleteAsync(goal);
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
        var goal = await goals.FindByIdAsync(id);
        return goal is null ? NotFound() : Ok(new GoalDO(goal));
    }

    [HttpGet("slug/{slug}")]
    [ProtectedResource("goals", "goals:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query a goal by slug")]
    [EndpointDescription("Retrieve a specific goal by slug")]
    public async Task<ActionResult<GoalDO>> GetBySlug(string slug)
    {
        var goal = await goals.FindBySlugAsync(slug);
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
        var goal = await goals.FindByIdAsync(id);
        if (goal is null)
            return NotFound();

        
        goal.Name = request.Name ?? goal.Name;
        goal.Description = request.Description ?? goal.Description;
        goal.Slug = request.Slug ?? goal.Slug;
        await goals.UpdateAsync(goal);
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
        var projects = await goals.GetProjectsAsync(id);
        return Ok(projects.Select(p => new ProjectDO(p)));
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
