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

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("subscriptions")]
[ProtectedResource("subscriptions"), Authorize]
public class SubscriptionController(ILogger<SubscriptionController> log, ISubscriptionService subscriptions) : Controller
{
    // [HttpPost("goals/{id:guid}")]
    // [ProducesResponseType(StatusCodes.Status201Created)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status409Conflict)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Subscribe to goal")]
    // [EndpointDescription("Subscribe current user to a specific goal")]
    // public async Task<ActionResult> SubscribeToGoal(Guid id, [FromBody] SubscribeToGoalRequestDTO? request = null)
    // {
    //     var userId = User.GetSID();
    //     var subscribeRequest = request ?? new SubscribeToGoalRequestDTO { GoalId = id };
        
    //     // Check if already subscribed
    //     var isSubscribed = await subscriptions.IsUserSubscribedToGoalAsync(userId, id);
    //     if (isSubscribed)
    //         return Conflict(new { message = "User is already subscribed to this goal" });
        
    //     var userGoal = await subscriptions.SubscribeToGoalAsync(userId, subscribeRequest);
    //     return CreatedAtAction(nameof(GetUserSubscriptions), new { }, new UserGoalDO(userGoal));
    // }

    // [HttpPost("projects/{id:guid}")]
    // [ProducesResponseType(StatusCodes.Status201Created)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status409Conflict)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Subscribe to project")]
    // [EndpointDescription("Create user project instance for current user")]
    // public async Task<ActionResult> SubscribeToProject(Guid id, [FromBody] CreateUserProjectRequestDTO? request = null)
    // {
    //     var userId = User.GetSID();
        
    //     // Check if already subscribed
    //     var isSubscribed = await subscriptions.IsUserSubscribedToProjectAsync(userId, id);
    //     if (isSubscribed)
    //         return Conflict(new { message = "User is already subscribed to this project" });
        
    //     var subscribeRequest = request ?? new CreateUserProjectRequestDTO { ProjectId = id };
        
    //     // This would need IProjectService to create the user project
    //     // For now, return placeholder
    //     return CreatedAtAction(nameof(GetUserSubscriptions), new { }, new { message = "Project subscription created" });
    // }

    // [HttpPost("cursus/{id:guid}")]
    // [ProducesResponseType(StatusCodes.Status201Created)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status409Conflict)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Subscribe to cursus")]
    // [EndpointDescription("Subscribe current user to a specific cursus")]
    // public async Task<ActionResult> SubscribeToCursus(Guid id, [FromBody] SubscribeToCursusRequestDTO? request = null)
    // {
    //     var userId = User.GetSID();
    //     var subscribeRequest = request ?? new SubscribeToCursusRequestDTO { CursusId = id };
        
    //     // Check if already subscribed
    //     var isSubscribed = await subscriptions.IsUserSubscribedToCursusAsync(userId, id);
    //     if (isSubscribed)
    //         return Conflict(new { message = "User is already subscribed to this cursus" });
        
    //     // This would need ICursusService to create the user cursus
    //     // For now, return placeholder
    //     return CreatedAtAction(nameof(GetUserSubscriptions), new { }, new { message = "Cursus subscription created" });
    // }

    // [HttpGet]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Get user subscriptions")]
    // [EndpointDescription("Retrieve all subscriptions for current user (goals, projects, cursus)")]
    // public async Task<ActionResult> GetUserSubscriptions()
    // {
    //     var userId = User.GetSID();
        
    //     // Get user goals
    //     var userGoals = await subscriptions.GetUserGoalsAsync(userId);
        
    //     // Note: This would need additional services to get user projects and cursus
    //     // For now, return goals only
        
    //     var result = new
    //     {
    //         Goals = userGoals.Select(ug => new UserGoalDO(ug)),
    //         Projects = new List<object>(), // Placeholder
    //         Cursus = new List<object>()  // Placeholder
    //     };
        
    //     return Ok(result);
    // }

    // [HttpDelete("goals/{goalId:guid}")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Unsubscribe from goal")]
    // [EndpointDescription("Unsubscribe current user from a specific goal")]
    // public async Task<IActionResult> UnsubscribeFromGoal(Guid goalId)
    // {
    //     var userId = User.GetSID();
        
    //     // Find user goal by userId and goalId
    //     var userGoals = await subscriptions.GetUserGoalsAsync(userId);
    //     var userGoal = userGoals.FirstOrDefault(ug => ug.GoalId == goalId);
        
    //     if (userGoal == null)
    //         return NotFound();
        
    //     var success = await subscriptions.UnsubscribeFromGoalAsync(userGoal.Id, userId);
    //     return success ? NoContent() : NotFound();
    // }

    // [HttpDelete("projects/{projectId:guid}")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Unsubscribe from project")]
    // [EndpointDescription("Unsubscribe current user from a specific project")]
    // public async Task<IActionResult> UnsubscribeFromProject(Guid projectId)
    // {
    //     var userId = User.GetSID();
        
    //     // This would need IProjectService to handle user project deletion
    //     // For now, return placeholder
    //     return NoContent();
    // }

    // [HttpDelete("cursus/{cursusId:guid}")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Unsubscribe from cursus")]
    // [EndpointDescription("Unsubscribe current user from a specific cursus")]
    // public async Task<IActionResult> UnsubscribeFromCursus(Guid cursusId)
    // {
    //     var userId = User.GetSID();
        
    //     // This would need ICursusService to handle user cursus deletion
    //     // For now, return placeholder
    //     return NoContent();
    // }

    // [HttpGet("check/goals/{id:guid}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Check goal subscription")]
    // [EndpointDescription("Check if current user is subscribed to a specific goal")]
    // public async Task<ActionResult> CheckGoalSubscription(Guid id)
    // {
    //     var userId = User.GetSID();
    //     var isSubscribed = await subscriptions.IsUserSubscribedToGoalAsync(userId, id);
    //     return Ok(new { isSubscribed });
    // }

    // [HttpGet("check/projects/{id:guid}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Check project subscription")]
    // [EndpointDescription("Check if current user is subscribed to a specific project")]
    // public async Task<ActionResult> CheckProjectSubscription(Guid id)
    // {
    //     var userId = User.GetSID();
    //     var isSubscribed = await subscriptions.IsUserSubscribedToProjectAsync(userId, id);
    //     return Ok(new { isSubscribed });
    // }

    // [HttpGet("check/cursus/{id:guid}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Check cursus subscription")]
    // [EndpointDescription("Check if current user is subscribed to a specific cursus")]
    // public async Task<ActionResult> CheckCursusSubscription(Guid id)
    // {
    //     var userId = User.GetSID();
    //     var isSubscribed = await subscriptions.IsUserSubscribedToCursusAsync(userId, id);
    //     return Ok(new { isSubscribed });
    // }
}