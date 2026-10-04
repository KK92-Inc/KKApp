// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using App.Backend.API.Params;
using App.Backend.API.Bus.Messages.Kickoff;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Enums;
using App.Backend.Models.Requests.Kickoff;
using App.Backend.Models.Responses.Entities;
using KickoffEntity = App.Backend.Domain.Entities.Kickoff;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("kickoffs"), Tags("Kickoffs")]
// [ProtectedResource("kickoffs")]
public class KickoffController(
    IKickoffService service,
    IMessageBus bus,
    IAuthorizationService auth
) : Controller
{
    [HttpGet]
    // [ProtectedResource("kickoffs", "kickoffs:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List kickoffs")]
    [EndpointDescription("Returns a paginated list of kickoffs with how many users are in each. Use `filter[after]` / `filter[before]` to look at a window of start dates.")]
    public async Task<ActionResult<IEnumerable<KickoffDO>>> GetAll(
        [FromQuery(Name = "filter[id]")] Guid? id,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[after]")] DateTimeOffset? after,
        [FromQuery(Name = "filter[before]")] DateTimeOffset? before,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        var page = await service.GetAllAsync(sorting, pagination, token,
            id is null ? null : k => k.Id == id,
            string.IsNullOrWhiteSpace(name) ? null : k => EF.Functions.ILike(k.Name, $"%{name}%"),
            after is null ? null : k => k.StartsAt >= after,
            before is null ? null : k => k.StartsAt <= before
        );

        var counts = await service.CountMembersAsync([.. page.Items.Select(k => k.Id)], token);

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(k => new KickoffDO(k, counts.GetValueOrDefault(k.Id))));
    }

    [HttpGet("{id:guid}")]
    // [ProtectedResource("kickoffs", "kickoffs:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get a kickoff")]
    [EndpointDescription("Returns a single kickoff, including how many users are in it and whether it already started.")]
    public async Task<ActionResult<KickoffDO>> GetById(Guid id, CancellationToken token)
    {
        var kickoff = await service.FindByIdAsync(id, token);
        if (kickoff is null) return NotFound();

        var counts = await service.CountMembersAsync([id], token);
        return Ok(new KickoffDO(kickoff, counts.GetValueOrDefault(id)));
    }

    [HttpPost]
    [Authorize(Policy = "staff")]
    // [ProtectedResource("kickoffs", "kickoffs:write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Create a kickoff")]
    [EndpointDescription("Creates a kickoff, a launch date for a cohort. Once `startsAt` passes, its applicants are promoted to students. A date in the past starts it on the next run of the kickoff job.")]
    public async Task<ActionResult<KickoffDO>> Create([FromBody] PostKickoffRequestDTO body, CancellationToken token)
    {
        var kickoff = await service.CreateAsync(new KickoffEntity
        {
            Name = body.Name.Trim(),
            Capacity = body.Capacity,
            StartsAt = body.StartsAt,
        }, token);

        return CreatedAtAction(nameof(GetById), new { id = kickoff.Id }, new KickoffDO(kickoff, 0));
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "staff")]
    // [ProtectedResource("kickoffs", "kickoffs:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Update a kickoff")]
    [EndpointDescription("Updates the name, capacity or start date. Omitted fields are left alone. The capacity can't drop below the current member count, and the start date can't change once the kickoff has started (both 409).")]
    public async Task<ActionResult<KickoffDO>> Update(Guid id, [FromBody] PatchKickoffRequestDTO body, CancellationToken token)
    {
        var kickoff = await service.FindByIdAsync(id, token);
        if (kickoff is null) return NotFound();

        if (body.Name is not null) kickoff.Name = body.Name.Trim();
        if (body.Capacity is { } capacity) kickoff.Capacity = capacity;
        if (body.StartsAt is { } startsAt) kickoff.StartsAt = startsAt;
        await service.UpdateAsync(kickoff, token);

        var counts = await service.CountMembersAsync([id], token);
        return Ok(new KickoffDO(kickoff, counts.GetValueOrDefault(id)));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "staff")]
    // [ProtectedResource("kickoffs", "kickoffs:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Delete a kickoff")]
    [EndpointDescription("Deletes a kickoff. Fails with 409 while it still has users, remove them first.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        var kickoff = await service.FindByIdAsync(id, token);
        if (kickoff is null) return NotFound();

        await service.DeleteAsync(kickoff, token);
        return NoContent();
    }

    [HttpGet("{id:guid}/users")]
    // [ProtectedResource("kickoffs", "kickoffs:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List the users of a kickoff")]
    [EndpointDescription("Returns a paginated list of the users in a kickoff. Use `filter[role]=Applicant` to see who still has to be promoted.")]
    public async Task<ActionResult<IEnumerable<UserLightDO>>> GetUsers(
        Guid id,
        [FromQuery(Name = "filter[role]")] UserRole? role,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        if (await service.FindByIdAsync(id, token) is null) return NotFound();

        var page = await service.GetUsersAsync(id, role, sorting, pagination, token);

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(u => new UserLightDO(u)));
    }

    [HttpPut("{id:guid}/users/{userId:guid}")]
    [Authorize(Policy = "staff")]
    // [ProtectedResource("kickoffs", "kickoffs:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Add a user to a kickoff")]
    [EndpointDescription("Puts a user in a kickoff. Safe to repeat. Fails with 404 if the kickoff or user doesn't exist, 409 if the kickoff is full or the user already belongs to another kickoff, and 422 for staff. If the kickoff already started, an applicant is promoted right away.")]
    public async Task<IActionResult> AddUser(Guid id, Guid userId, CancellationToken token)
    {
        var kickoff = await service.AddUsersAsync(id, [userId], token);
        await PromoteIfStartedAsync(kickoff, [userId]);
        return NoContent();
    }

    [HttpDelete("{id:guid}/users/{userId:guid}")]
    [Authorize(Policy = "staff")]
    // [ProtectedResource("kickoffs", "kickoffs:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Remove a user from a kickoff")]
    [EndpointDescription("Takes a user out of a kickoff. 404 if they aren't in it. Their role is untouched: someone who was already promoted stays a student.")]
    public async Task<IActionResult> RemoveUser(Guid id, Guid userId, CancellationToken token)
    {
        await service.RemoveUsersAsync(id, [userId], token);
        return NoContent();
    }

    [HttpPost("{id:guid}/users")]
    [Authorize(Policy = "staff")]
    // [ProtectedResource("kickoffs", "kickoffs:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Add users to a kickoff")]
    [EndpointDescription("Bulk version of adding a user. All or nothing: if any user doesn't exist (404), is staff (422), is in another kickoff, or doesn't fit (409), nobody is added and the error names the offending users.")]
    public async Task<IActionResult> AddUsers(
        Guid id,
        [FromBody] KickoffUsersRequestDTO body,
        CancellationToken token
    )
    {
        var kickoff = await service.AddUsersAsync(id, body.UserIds, token);
        await PromoteIfStartedAsync(kickoff, body.UserIds);
        return NoContent();
    }

    [HttpPost("{id:guid}/users/remove")]
    [Authorize(Policy = "staff")]
    // [ProtectedResource("kickoffs", "kickoffs:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Remove users from a kickoff")]
    [EndpointDescription("Bulk version of removing a user. This is a POST because a request body on DELETE is unreliable through proxies and some clients. All or nothing: if any user isn't in the kickoff, nobody is removed (404).")]
    public async Task<IActionResult> RemoveUsers(
        Guid id,
        [FromBody] KickoffUsersRequestDTO body,
        CancellationToken token
    )
    {
        await service.RemoveUsersAsync(id, body.UserIds, token);
        return NoContent();
    }

    [Tags("Users")]
    [HttpGet("~/users/{userId:guid}/kickoff")]
    // [ProtectedResource("kickoffs", "kickoffs:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get the kickoff of a user")]
    [EndpointDescription("Returns the kickoff a user belongs to. 404 if they have none (e.g: staff, or someone who started directly). Staff can look up anyone, everyone else only themselves.")]
    public async Task<ActionResult<KickoffDO>> GetByUser(Guid userId, CancellationToken token)
    {
        if (userId != User.GetSID() && !(await auth.AuthorizeAsync(User, "staff")).Succeeded)
            return Forbid();

        var kickoff = await service.FindByUserAsync(userId, token);
        if (kickoff is null)
            return Problem(title: "The user has no kickoff.", statusCode: StatusCodes.Status404NotFound);

        var counts = await service.CountMembersAsync([kickoff.Id], token);
        return Ok(new KickoffDO(kickoff, counts.GetValueOrDefault(kickoff.Id)));
    }

    /// <summary>
    /// A kickoff only runs once. Anyone who joins after it started would never be promoted
    /// by it, so they get queued for promotion right away. The handler skips everyone who
    /// isn't an applicant of this kickoff, so passing along users that were already members is harmless.
    /// </summary>
    private async Task PromoteIfStartedAsync(KickoffEntity kickoff, IEnumerable<Guid> userIds)
    {
        if (kickoff.StartedAt is null) return;

        foreach (var userId in userIds.Distinct())
            await bus.PublishAsync(new PromoteApplicant(userId, kickoff.Id));
    }
}
