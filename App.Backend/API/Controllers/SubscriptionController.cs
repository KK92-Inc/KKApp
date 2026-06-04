// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Core.Services.Interface;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Responses.Entities.Projects;
using App.Backend.Models.Responses.Entities.Cursus;

// ============================================================================

namespace App.Backend.API.Controllers;

/// <summary>
/// Handles subscribing and unsubscribing users to/from cursi, goals, and projects.
/// Subscription creates/reactivates enrollment records; unsubscription deactivates them.
/// </summary>
[Authorize]
[ApiController]
[Route("subscribe"), Tags("Subscriptions")]
public class SubscriptionController(
    ILogger<SubscriptionController> log,
    ISubscriptionService service,
    ICursusService cursusService,
    IGoalService goalService,
    IProjectService projectService
) : Controller
{
    // ========================================================================
    // Cursus
    // ========================================================================

    [HttpPost("{userId:guid}/cursus/{cursusId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Subscribe a user to a cursus")]
    [EndpointDescription("Enroll the specified user in the given cursus. Staff can enroll other users.")]
    public async Task<ActionResult<UserCursusDO>> SubscribeToCursus(Guid userId, Guid cursusId, CancellationToken token)
    {
        if (User.GetSID() != userId && !User.IsInRole("Staff"))
            return Forbid();

        var cursus = await cursusService.FindByIdAsync(cursusId, token);
        if (cursus is null) return NotFound();
        if (cursus.Deprecated || !cursus.Active)
        {
            return UnprocessableEntity(new ProblemDetails()
            {
                Title = "Cursus is unavailable",
                Detail = "This cursus is unavailable and cannot be subscribed to."
            });
        }
        var userCursus = await service.SubscribeToCursusAsync(userId, cursusId, token);
        return Ok(new UserCursusDO(userCursus));
    }

    [HttpDelete("{userId:guid}/cursus/{cursusId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Unsubscribe a user from a cursus")]
    [EndpointDescription("Remove the specified user's enrollment from the given cursus. Staff can unenroll other users.")]
    public async Task<ActionResult<UserCursusDO>> UnsubscribeFromCursus(Guid userId, Guid cursusId, CancellationToken token)
    {
        if (User.GetSID() != userId && !User.IsInRole("Staff"))
            return Forbid();

        var cursus = await cursusService.FindByIdAsync(cursusId, token);
        if (cursus is null)
            return NotFound();

        var userCursus = await service.UnsubscribeFromCursusAsync(userId, cursusId, token);
        return Ok(new UserCursusDO(userCursus));
    }

    // ========================================================================
    // Goals
    // ========================================================================

    [HttpPost("{userId:guid}/goals/{goalId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Subscribe a user to a goal")]
    [EndpointDescription("Create a subscription for the specified user to the given goal. Staff can enroll other users.")]
    public async Task<ActionResult<UserGoalDO>> SubscribeToGoal(Guid userId, Guid goalId, CancellationToken token)
    {
        if (User.GetSID() != userId && !User.IsInRole("Staff"))
            return Forbid();

        var goal = await goalService.FindByIdAsync(goalId, token);
        if (goal is null) return NotFound();
        if (goal.Deprecated || !goal.Active)
        {
            return UnprocessableEntity(new ProblemDetails()
            {
                Title = "Goal is unavailable",
                Detail = "This goal is unavailable and cannot be subscribed to."
            });
        }

        var userGoal = await service.SubscribeToGoalAsync(userId, goalId, token);
        return Ok(new UserGoalDO(userGoal));
    }

    [HttpDelete("{userId:guid}/goals/{goalId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Unsubscribe a user from a goal")]
    [EndpointDescription("Remove the specified user's subscription to the given goal. Staff can unenroll other users.")]
    public async Task<ActionResult<UserGoalDO>> UnsubscribeFromGoal(Guid userId, Guid goalId, CancellationToken token)
    {
        if (User.GetSID() != userId && !User.IsInRole("Staff"))
            return Forbid();

        var goal = await goalService.FindByIdAsync(goalId, token);
        if (goal is null)
            return NotFound();

        var userGoal = await service.UnsubscribeFromGoalAsync(userId, goalId, token);
        return Ok(new UserGoalDO(userGoal));
    }

    // ========================================================================
    // Projects
    // ========================================================================

    [HttpPost("{userId:guid}/projects/{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Subscribe a user to a project")]
    [EndpointDescription("Create a project session for the specified user. Staff can enroll other users.")]
    public async Task<ActionResult<UserProjectDO>> SubscribeToProject(Guid userId, Guid projectId, CancellationToken token)
    {
        if (User.GetSID() != userId && !User.IsInRole("Staff"))
            return Forbid();

        var project = await projectService.FindByIdAsync(projectId, token);
        if (project is null) return NotFound();
        if (project.Deprecated || !project.Active)
        {
            return UnprocessableEntity(new ProblemDetails()
            {
                Title = "Project is unavailable",
                Detail = "This project is unavailable and cannot be subscribed to."
            });
        }

        var userProject = await service.SubscribeToProjectAsync(userId, projectId, token);
        return Ok(new UserProjectDO(userProject));
    }

    [HttpDelete("{userId:guid}/projects/{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Unsubscribe a user from a project")]
    [EndpointDescription("Remove the specified user from the project session. Staff can unenroll other users.")]
    public async Task<ActionResult<UserProjectDO>> UnsubscribeFromProject(Guid userId, Guid projectId, CancellationToken token)
    {
        var project = await projectService.FindByIdAsync(projectId, token);
        if (project is null)
            return NotFound();
        if (User.GetSID() != userId && !User.IsInRole("Staff"))
            return Forbid();

        var userProject = await service.UnsubscribeFromProjectAsync(userId, projectId, token);
        return Ok(new UserProjectDO(userProject));
    }
}
