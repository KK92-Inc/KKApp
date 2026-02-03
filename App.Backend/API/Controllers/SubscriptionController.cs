// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Core.Services.Interface;
using App.Backend.Models;
using Keycloak.AuthServices.Authorization;
using App.Backend.Models.Responses.Entities;
using JasperFx.RuntimeCompiler.Scenarios;
using App.Backend.Models.Responses.Entities.Projects;
using App.Backend.Models.Responses.Entities.Cursus;

// ============================================================================

namespace App.Backend.API.Controllers;

[Authorize]
[ApiController, Route("subscribe")]
public class SubscriptionController(
    ILogger<SubscriptionController> log,
    ISubscriptionService service,
    IUserService userService,
    IProjectService projectService,
    IGoalService goalService,
    ICursusService cursusService
) : Controller
{
    [HttpPost("{userId:guid}/cursus/{goalId:guid}")]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Subscribe a user to a cursus")]
    [EndpointDescription("Enroll the specified user in the cursus associated with the provided goalId.")]
    public async Task<ActionResult<UserCursusDO>> SubscribeUserToCursus(Guid userId, Guid goalId, CancellationToken token)
    {
        var id = User.GetSID();
        if (id != userId && !User.IsInRole("staff"))
            return Forbid();

        var userCursus = await service.SubscribeToCursusAsync(userId, goalId, token);
        return Ok(new UserCursusDO(userCursus));
    }

    [HttpDelete("{userId:guid}/cursus/{goalId:guid}")]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Unsubscribe a user from a cursus")]
    [EndpointDescription("Remove the specified user's enrollment from the cursus associated with the provided goalId.")]
    public async Task<ActionResult> UnsubscribeUserToCursus(Guid userId, Guid goalId, CancellationToken token)
    {
        var id = User.GetSID();
        if (id != userId && !User.IsInRole("staff"))
            return Forbid();

        await service.UnsubscribeFromCursusAsync(userId, goalId, token);
        return NoContent();
    }

    [HttpPost("{userId:guid}/goals/{goalId:guid}")]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Subscribe a user to a goal")]
    [EndpointDescription("Create a subscription for the specified user to the goal identified by goalId.")]
    public async Task<ActionResult<UserGoalDO>> SubscribeUserToGoal(Guid userId, Guid goalId, CancellationToken token)
    {
        var id = User.GetSID();
        if (id != userId && !User.IsInRole("staff"))
            return Forbid();

        var userGoal = await service.SubscribeToGoalAsync(userId, goalId, token);
        return Ok(new UserGoalDO(userGoal));
    }

    [HttpDelete("{userId:guid}/goals/{goalId:guid}")]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Unsubscribe a user from a goal")]
    [EndpointDescription("Remove the specified user's subscription to the goal identified by goalId.")]
    public async Task<ActionResult> UnsubscribeUserToGoal(Guid userId, Guid goalId, CancellationToken token)
    {
        await service.UnsubscribeFromGoalAsync(userId, goalId, token);
        return NoContent();
    }

    [HttpPost("{userId:guid}/projects/{goalId:guid}")]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Subscribe a user to a project")]
    [EndpointDescription("Create a subscription for the specified user to the project identified by goalId.")]
    public async Task<ActionResult<UserProjectDO>> SubscribeUserToProject(Guid userId, Guid goalId, CancellationToken token)
    {
        var userProject = await service.SubscribeToProjectAsync(userId, goalId, token);
        return Ok(new UserProjectDO(userProject));
    }

    [HttpDelete("{userId:guid}/projects/{goalId:guid}")]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Unsubscribe a user from a project")]
    [EndpointDescription("Remove the specified user's subscription to the project identified by goalId.")]
    public async Task<ActionResult> UnsubscribeUserToProject(Guid userId, Guid goalId, CancellationToken token)
    {
        await service.UnsubscribeFromProjectAsync(userId, goalId, token);
        return NoContent();
    }

    // private void IsAllowed(Guid actor, Guid target)
    // {

    // }
}
