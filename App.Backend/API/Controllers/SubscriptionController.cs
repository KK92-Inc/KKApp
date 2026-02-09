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
    ISubscriptionService service
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
        if (!IsAllowed(userId))
            return Forbid();

        var userCursus = await service.SubscribeToCursusAsync(userId, cursusId, token);
        return Ok(new UserCursusDO(userCursus));
    }

    [HttpDelete("{userId:guid}/cursus/{cursusId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Unsubscribe a user from a cursus")]
    [EndpointDescription("Remove the specified user's enrollment from the given cursus. Staff can unenroll other users.")]
    public async Task<ActionResult> UnsubscribeFromCursus(Guid userId, Guid cursusId, CancellationToken token)
    {
        if (!IsAllowed(userId))
            return Forbid();

        await service.UnsubscribeFromCursusAsync(userId, cursusId, token);
        return NoContent();
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
        if (!IsAllowed(userId))
            return Forbid();

        var userGoal = await service.SubscribeToGoalAsync(userId, goalId, token);
        return Ok(new UserGoalDO(userGoal));
    }

    [HttpDelete("{userId:guid}/goals/{goalId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Unsubscribe a user from a goal")]
    [EndpointDescription("Remove the specified user's subscription to the given goal. Staff can unenroll other users.")]
    public async Task<ActionResult> UnsubscribeFromGoal(Guid userId, Guid goalId, CancellationToken token)
    {
        if (!IsAllowed(userId))
            return Forbid();

        await service.UnsubscribeFromGoalAsync(userId, goalId, token);
        return NoContent();
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
        if (!IsAllowed(userId))
            return Forbid();

        var userProject = await service.SubscribeToProjectAsync(userId, projectId, token);
        return Ok(new UserProjectDO(userProject));
    }

    [HttpDelete("{userId:guid}/projects/{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Unsubscribe a user from a project")]
    [EndpointDescription("Remove the specified user from the project session. Staff can unenroll other users.")]
    public async Task<ActionResult> UnsubscribeFromProject(Guid userId, Guid projectId, CancellationToken token)
    {
        if (!IsAllowed(userId))
            return Forbid();

        await service.UnsubscribeFromProjectAsync(userId, projectId, token);
        return NoContent();
    }

    // ========================================================================
    // Helpers
    // ========================================================================

    /// <summary>
    /// Check if the current user is allowed to act on behalf of the target user.
    /// Users can act on themselves; staff can act on anyone.
    /// </summary>
    private bool IsAllowed(Guid targetUserId) => User.GetSID() == targetUserId || User.IsInRole("staff");
}
