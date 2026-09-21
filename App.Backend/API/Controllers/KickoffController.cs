// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.API.Params;
using Keycloak.AuthServices.Authorization;
using App.Backend.Models.Requests.Kickoff;
using App.Backend.Models.Responses.Entities;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("kickoffs")]
[ProtectedResource("kickoffs")]
public class KickoffController(
    IAuthorizationService auth
) : Controller
{
    [HttpGet]
    [ProtectedResource("kickoffs", "kickoffs:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all kickoffs")]
    [EndpointDescription("Retrieve a paginated list of all kickoffs")]
    public Task<ActionResult<IEnumerable<KickoffDO>>> GetAll(
        [FromQuery(Name = "filter[id]")] Guid? id,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[after]")] DateTimeOffset? after,
        [FromQuery(Name = "filter[before]")] DateTimeOffset? before,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        // TODO: Build filters -> service.GetAllAsync(...) -> page.AppendHeaders(Response.Headers)
        throw new NotImplementedException();
    }

    [HttpGet("{id:guid}")]
    [ProtectedResource("kickoffs", "kickoffs:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query a kickoff")]
    [EndpointDescription("Retrieve a specific kickoff by ID")]
    public Task<ActionResult<KickoffDO>> GetById(Guid id, CancellationToken token)
    {
        // TODO: service.FindByIdAsync(id) -> 404 if null
        throw new NotImplementedException();
    }

    [HttpPost]
    [ProtectedResource("kickoffs", "kickoffs:write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Create a kickoff")]
    [EndpointDescription("Create a new kickoff (cohort)")]
    public Task<ActionResult<KickoffDO>> Create([FromBody] PostKickoffRequestDTO body, CancellationToken token)
    {
        // TODO: Create entity -> service.CreateAsync -> CreatedAtAction(nameof(GetById), ...)
        throw new NotImplementedException();
    }

    [HttpPatch("{id:guid}")]
    [ProtectedResource("kickoffs", "kickoffs:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Update a kickoff")]
    [EndpointDescription("Update kickoff information")]
    public Task<ActionResult<KickoffDO>> Update(Guid id, [FromBody] PatchKickoffRequestDTO body, CancellationToken token)
    {
        // TODO: Apply only the non-null fields from body.
        // TODO: 409 if the new Capacity is lower than the current member count.
        throw new NotImplementedException();
    }

    [HttpDelete("{id:guid}")]
    [ProtectedResource("kickoffs", "kickoffs:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Delete a kickoff")]
    [EndpointDescription("Delete a kickoff. Fails with 409 if it still has members.")]
    public Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        // TODO: 404 if missing, 409 if it still has members (FK is Restrict), else delete.
        throw new NotImplementedException();
    }

    [HttpGet("{id:guid}/users")]
    [ProtectedResource("kickoffs", "kickoffs:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all users of a kickoff")]
    [EndpointDescription("Retrieve a paginated list of the users that belong to a kickoff")]
    public Task<ActionResult<IEnumerable<UserLightDO>>> GetUsers(
        Guid id,
        [FromQuery(Name = "filter[processed]")] bool? processed,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        // TODO: 404 if kickoff is missing. filter[processed] => ProcessedAt != null / == null
        throw new NotImplementedException();
    }

    [HttpPut("{id:guid}/users/{userId:guid}")]
    [ProtectedResource("kickoffs", "kickoffs:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Add a user to a kickoff")]
    [EndpointDescription("Idempotent. 409 if the kickoff is full or the user already belongs to another kickoff.")]
    public Task<ActionResult<UserLightDO>> AddUser(Guid id, Guid userId, CancellationToken token)
    {
        // TODO: 404 if kickoff/user missing.
        // TODO: Already in this kickoff => return existing (idempotent), in another => 409.
        // TODO: Capacity check inside a transaction (see notes on concurrency).
        throw new NotImplementedException();
    }

    [HttpDelete("{id:guid}/users/{userId:guid}")]
    [ProtectedResource("kickoffs", "kickoffs:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Remove a user from a kickoff")]
    public Task<IActionResult> RemoveUser(Guid id, Guid userId, CancellationToken token)
    {
        // TODO: 404 if the user is not a member of this kickoff.
        throw new NotImplementedException();
    }

    [HttpPost("{id:guid}/users")]
    [ProtectedResource("kickoffs", "kickoffs:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Add multiple users to a kickoff")]
    [EndpointDescription("Bulk add. Per-user problems are reported in the result; exceeding capacity rejects the whole batch with 409.")]
    public Task<ActionResult<IEnumerable<Guid>>> AddUsers(
        Guid id,
        [FromBody] IEnumerable<Guid> body,
        CancellationToken token
    )
    {
        // TODO: Distinct() the ids, single query for existing users + existing memberships (no N+1).
        // TODO: One transaction, one SaveChanges (AddRange), capacity check up front.
        throw new NotImplementedException();
    }

    [HttpDelete("{id:guid}/users/remove")]
    [ProtectedResource("kickoffs", "kickoffs:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Remove multiple users from a kickoff")]
    [EndpointDescription("Bulk remove. POST instead of DELETE because a request body on DELETE is unreliable through proxies and some clients.")]
    public Task<ActionResult<IEnumerable<Guid>>> RemoveUsers(
        Guid id,
        [FromBody] IEnumerable<Guid> body,
        CancellationToken token
    )
    {
        // TODO: ExecuteDeleteAsync where KickoffId == id && UserIds.Contains(UserId), report the rest as NotMembers.
        throw new NotImplementedException();
    }

    [Tags("users")]
    [HttpGet("~/users/{userId:guid}/kickoff")]
    [ProtectedResource("kickoffs", "kickoffs:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query the kickoff of a user")]
    [EndpointDescription("Retrieve the kickoff a user belongs to. 404 if the user has none (e.g: staff).")]
    public Task<ActionResult<KickoffDO>> GetByUser(Guid userId, CancellationToken token)
    {
        // TODO: Staff can read anyone, everyone else only themselves (User.GetSID() == userId).
        throw new NotImplementedException();
    }
}