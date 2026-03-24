// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Core.Query;
using App.Backend.API.Params;
using App.Backend.Core.Services.Interface;
using App.Backend.Models;
using App.Backend.Models.Responses.Entities;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.API.Controllers;

/// <summary>
/// Operations on user goal subscriptions.
///
/// Supports two access patterns:
/// <list type="bullet">
///   <item>Nested: <c>GET /users/{userId}/goals</c> — scoped listing and lookup by goal ID</item>
///   <item>Direct: <c>GET /user-goals/{id}</c> — lookup by UserGoal entity ID</item>
/// </list>
/// </summary>
[ApiController]
[Route("users/{userId:guid}/goals"), Tags("UserGoals")]
[Authorize]
public class UserGoalController(IUserGoalService userGoalService) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List user goal subscriptions")]
    [EndpointDescription("Returns all goal subscriptions for the specified user. Supports filtering by state and goal name.")]
    public async Task<ActionResult<IEnumerable<UserGoalDO>>> GetByUser(
        Guid userId,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[state]")] EntityObjectState? state,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken token
    )
    {
        var page = await userGoalService.GetAllAsync(sorting, pagination, token,
            ug => ug.UserId == userId,
            name is null ? null : ug => ug.Goal.Name.Contains(name),
            state is null ? null : ug => ug.State == state
        );
        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(ug => new UserGoalDO(ug)));
    }

    [HttpGet("{goalId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user goal by goal ID")]
    [EndpointDescription("Finds the user's subscription to a specific goal by the goal's own ID.")]
    public async Task<ActionResult<UserGoalDO>> GetByUserAndGoal(
        Guid userId, Guid goalId, CancellationToken token
    )
    {
        var ug = await userGoalService.FindByUserAndGoalAsync(userId, goalId, token);
        return ug is null ? NotFound() : Ok(new UserGoalDO(ug));
    }

    [HttpGet("/user-goals/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user goal by entity ID")]
    [EndpointDescription("Finds a user goal subscription directly by its own entity ID, without requiring a user context.")]
    public async Task<ActionResult<UserGoalDO>> GetById(Guid id, CancellationToken token)
    {
        var ug = await userGoalService.FindByIdAsync(id, token);
        return ug is null ? NotFound() : Ok(new UserGoalDO(ug));
    }
}