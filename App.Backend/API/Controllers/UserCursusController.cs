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
using App.Backend.Models.Responses.Entities.Cursus;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.API.Controllers;

/// <summary>
/// Operations on user cursus enrollments (sessions).
///
/// Supports two access patterns:
/// <list type="bullet">
///   <item>Nested: <c>/users/{userId}/cursus</c> — query by user + cursus IDs</item>
///   <item>Direct: <c>/user-cursus/{id}</c> — query by UserCursus entity ID</item>
/// </list>
/// </summary>
[ApiController]
[Route("users/{userId:guid}/cursus"), Tags("UserCursus")]
[Authorize]
public class UserCursusController(
    ILogger<UserCursusController> log,
    IUserCursusService userCursusService,
    ICursusService cursusService
) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List user cursus enrollments")]
    [EndpointDescription("Get all cursus enrollments for a specific user, with optional state filtering.")]
    public async Task<ActionResult<IEnumerable<UserCursusDO>>> GetByUser(
        Guid userId,
        [FromQuery(Name = "filter[state]")] EntityObjectState? state,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken token
    )
    {
        var page = await userCursusService.GetAllAsync(sorting, pagination, token,
            uc => uc.UserId == userId,
            state is null ? null : uc => uc.State == state
        );
        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(uc => new UserCursusDO(uc)));
    }

    [HttpGet("{cursusId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user cursus by cursus ID")]
    [EndpointDescription("Find a user's specific cursus enrollment by user and cursus ID.")]
    public async Task<ActionResult<UserCursusDO>> GetByUserAndCursus(
        Guid userId, Guid cursusId, CancellationToken token
    )
    {
        var uc = await userCursusService.FindByUserAndCursusAsync(userId, cursusId, token);
        return uc is null ? NotFound() : Ok(new UserCursusDO(uc));
    }

    [HttpGet("/user-cursus/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user cursus by entity ID")]
    [EndpointDescription("Find a user cursus enrollment directly by its entity ID.")]
    public async Task<ActionResult<UserCursusDO>> GetById(Guid id, CancellationToken token)
    {
        var uc = await userCursusService.FindByIdAsync(id, token);
        return uc is null ? NotFound() : Ok(new UserCursusDO(uc));
    }

    [HttpGet("/user-cursus/{id:guid}/track")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user cursus track")]
    [EndpointDescription("Retrieve the user's personalized track/progress for a cursus enrollment.")]
    public async Task<ActionResult<UserCursusTrackDO>> GetTrack(Guid id, CancellationToken token)
    {
        var uc = await userCursusService.FindByIdAsync(id, token);
        if (uc is null)
            return NotFound();

        var cursus = await cursusService.FindByIdAsync(uc.CursusId, token);
        if (cursus is null)
            return NotFound();

        var relations = await cursusService.GetTrackAsync(uc.CursusId, token);
        var userStates = await cursusService.GetTrackForUserAsync(uc.CursusId, uc.UserId, token);
        return Ok(UserCursusTrackDO.FromRelations(cursus, uc.UserId, relations, userStates));
    }
}
