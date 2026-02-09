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
/// Operations on user goal subscriptions (sessions).
///
/// Supports two access patterns:
/// <list type="bullet">
///   <item>Nested: <c>/users/{userId}/goals</c> — query by user + goal IDs</item>
///   <item>Direct: <c>/user-goals/{id}</c> — query by UserGoal entity ID</item>
/// </list>
/// </summary>
[ApiController]
[Route("users/{userId:guid}/goals"), Tags("UserGoals")]
[Authorize]
public class UserGoalController(
    ILogger<UserGoalController> log,
    IUserGoalService userGoalService
) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List user goal subscriptions")]
    [EndpointDescription("Get all goal subscriptions for a specific user, with optional state filtering.")]
    public async Task<ActionResult<IEnumerable<UserGoalDO>>> GetByUser(
        Guid userId,
        [FromQuery(Name = "filter[state]")] EntityObjectState? state,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken token
    )
    {
        var page = await userGoalService.GetAllAsync(sorting, pagination, token,
            ug => ug.UserId == userId,
            state is null ? null : ug => ug.State == state
        );
        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(ug => new UserGoalDO(ug)));
    }

    [HttpGet("{goalId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user goal by goal ID")]
    [EndpointDescription("Find a user's specific goal subscription by user and goal ID.")]
    public async Task<ActionResult<UserGoalDO>> GetByUserAndGoal(
        Guid userId, Guid goalId, CancellationToken token
    )
    {
        var ug = await userGoalService.FindByUserAndGoalAsync(userId, goalId, token);
        return ug is null ? NotFound() : Ok(new UserGoalDO(ug));
    }

    [HttpGet("/user-goals/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user goal by entity ID")]
    [EndpointDescription("Find a user goal subscription directly by its entity ID.")]
    public async Task<ActionResult<UserGoalDO>> GetById(Guid id, CancellationToken token)
    {
        var ug = await userGoalService.FindByIdAsync(id, token);
        return ug is null ? NotFound() : Ok(new UserGoalDO(ug));
    }
}
