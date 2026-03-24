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
/// Operations on user cursus enrollments.
///
/// Supports two access patterns:
/// <list type="bullet">
///   <item>Nested: <c>GET /users/{userId}/cursus</c> — scoped listing and lookup by cursus ID</item>
///   <item>Direct: <c>GET /user-cursus/{id}</c> — lookup by UserCursus entity ID or track</item>
/// </list>
/// </summary>
[ApiController]
[Route("users/{userId:guid}/cursus"), Tags("UserCursus")]
[Authorize]
public class UserCursusController(
    IUserCursusService userCursusService,
    ICursusService cursusService
) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List user cursus enrollments")]
    [EndpointDescription("Returns all cursus enrollments for the specified user. Supports filtering by state and cursus name.")]
    public async Task<ActionResult<IEnumerable<UserCursusDO>>> GetByUser(
        Guid userId,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[state]")] EntityObjectState? state,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken token
    )
    {
        var page = await userCursusService.GetAllAsync(sorting, pagination, token,
            uc => uc.UserId == userId,
            name is null ? null : uc => uc.Cursus.Name.Contains(name),
            state is null ? null : uc => uc.State == state
        );
        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(uc => new UserCursusDO(uc)));
    }

    [HttpGet("{cursusId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user cursus by cursus ID")]
    [EndpointDescription("Finds the user's enrollment in a specific cursus by the cursus's own ID.")]
    public async Task<ActionResult<UserCursusDO>> GetByUserAndCursus(
        Guid userId, Guid cursusId, CancellationToken token
    )
    {
        var uc = await userCursusService.FindByUserAndCursusAsync(userId, cursusId, token);
        return uc is null ? NotFound() : Ok(new UserCursusDO(uc));
    }

    [HttpGet("/user-cursus/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user cursus by entity ID")]
    [EndpointDescription("Finds a user cursus enrollment directly by its own entity ID, without requiring a user context.")]
    public async Task<ActionResult<UserCursusDO>> GetById(Guid id, CancellationToken token)
    {
        var uc = await userCursusService.FindByIdAsync(id, token);
        return uc is null ? NotFound() : Ok(new UserCursusDO(uc));
    }

    [HttpGet("/user-cursus/{id:guid}/track")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user cursus track")]
    [EndpointDescription("Returns the user's personalized progress track for a cursus enrollment, resolved from the stored graph.")]
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