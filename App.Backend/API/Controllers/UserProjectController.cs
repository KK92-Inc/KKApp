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

// ============================================================================

namespace App.Backend.API.Controllers;

/// <summary>
/// Operations on user project sessions.
///
/// Supports two access patterns:
/// <list type="bullet">
///   <item>Nested: <c>GET /users/{userId}/projects</c> — scoped listing and lookup by project ID</item>
///   <item>Direct: <c>GET /user-projects/{id}</c> — lookup by UserProject entity ID, members, or transactions</item>
/// </list>
/// </summary>
[ApiController]
[Route("users/{userId:guid}/projects"), Tags("UserProjects")]
[Authorize]
public class UserProjectController(
    IUserProjectService userProjectService
) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List user project sessions")]
    [EndpointDescription("Returns all active project sessions the user is a member of. Supports filtering by name, slug, and state.")]
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
            up => up.Members.Any(m => m.UserId == userId && m.Role != UserProjectRole.Pending && m.LeftAt == null),
            name is null ? null : up => up.Project.Name.Contains(name),
            slug is null ? null : up => up.Project.Slug == slug,
            state is null ? null : up => up.State == state
        );
        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(up => new UserProjectDO(up)));
    }

    [HttpGet("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user project by project ID")]
    [EndpointDescription("Finds the user's session for a specific project by the project's own ID.")]
    public async Task<ActionResult<UserProjectDO>> GetByUserAndProject(
        Guid userId, Guid projectId, CancellationToken token
    )
    {
        var up = await userProjectService.FindByUserAndProjectAsync(userId, projectId, token);
        return up is null ? NotFound() : Ok(new UserProjectDO(up));
    }

    [HttpGet("/user-projects/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user project by entity ID")]
    [EndpointDescription("Finds a user project session directly by its own entity ID, without requiring a user context.")]
    public async Task<ActionResult<UserProjectDO>> GetById(Guid id, CancellationToken token)
    {
        var up = await userProjectService.FindByIdAsync(id, token);
        return up is null ? NotFound() : Ok(new UserProjectDO(up));
    }

    [HttpGet("/user-projects/{id:guid}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get project session members")]
    [EndpointDescription("Returns all current and past members of the specified user project session.")]
    public async Task<ActionResult<IEnumerable<UserProjectMemberDO>>> GetMembers(Guid id, CancellationToken token)
    {
        var up = await userProjectService.FindByIdAsync(id, token);
        if (up is null) return NotFound();

        return Ok(up.Members.Select(m => new UserProjectMemberDO(m)));
    }

    [HttpGet("/user-projects/{id:guid}/transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get project session transactions")]
    [EndpointDescription("Returns the paginated activity timeline of the specified user project session, ordered by the requested sort.")]
    public async Task<ActionResult<IEnumerable<UserProjectTransactionDO>>> GetTransactions(
        Guid id,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken token
    )
    {
        var page = await userProjectService.GetTransactionsAsync(id, sorting, pagination, token);
        if (page is null) return NotFound();

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(t => new UserProjectTransactionDO(t)));
    }
}