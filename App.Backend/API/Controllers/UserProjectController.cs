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
using App.Backend.Models.Responses.Entities.Projects;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.API.Controllers;

/// <summary>
/// Operations on user project sessions.
///
/// Supports two access patterns:
/// <list type="bullet">
///   <item>Nested: <c>/users/{userId}/projects</c> — query by user + project IDs</item>
///   <item>Direct: <c>/user-projects/{id}</c> — query by UserProject entity ID</item>
/// </list>
/// </summary>
[ApiController]
[Route("users/{userId:guid}/projects"), Tags("UserProjects")]
[Authorize]
public class UserProjectController(
    ILogger<UserProjectController> log,
    IUserProjectService userProjectService
) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List user project sessions")]
    [EndpointDescription("Get all project sessions for a specific user, with optional state filtering.")]
    public async Task<ActionResult<IEnumerable<UserProjectDO>>> GetByUser(
        Guid userId,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[slug]")] string? slug,
        [FromQuery(Name = "filter[state]")] EntityObjectState? state,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken token
    )
    {
        var page = await userProjectService.GetAllAsync(sorting, pagination, token,
            up => up.Members.Any(m => m.UserId == userId),
            state is null ? null : up => up.State == state
        );
        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(up => new UserProjectDO(up)));
    }

    [HttpGet("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user project by project ID")]
    [EndpointDescription("Find a user's specific project session by user and project ID.")]
    public async Task<ActionResult<UserProjectDO>> GetByUserAndProject(
        Guid userId, Guid projectId, CancellationToken token
    )
    {
        var up = await userProjectService.FindByUserAndProjectAsync(userId, projectId, token);
        return up is null ? NotFound() : Ok(new UserProjectDO(up));
    }

    [HttpGet("/user-projects/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user project by entity ID")]
    [EndpointDescription("Find a user project session directly by its entity ID.")]
    public async Task<ActionResult<UserProjectDO>> GetById(Guid id, CancellationToken token)
    {
        var up = await userProjectService.FindByIdAsync(id, token);
        return up is null ? NotFound() : Ok(new UserProjectDO(up));
    }

    [HttpGet("/user-projects/{id:guid}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get project session members")]
    [EndpointDescription("Retrieve all members participating in a user project session.")]
    public async Task<ActionResult<IEnumerable<UserProjectMemberDO>>> GetMembers(Guid id, CancellationToken token)
    {
        var up = await userProjectService.Query(false)
            .Where(p => p.Id == id)
            .Select(p => p.Members)
            .FirstOrDefaultAsync(token);

        return up is null ? NotFound() : Ok(up.Select(m => new UserProjectMemberDO(m)));
    }
}
