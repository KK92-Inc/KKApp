// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using App.Backend.API.Params;
using App.Backend.Domain.Entities.Apply;
using App.Backend.Models.Requests.Apply;
using App.Backend.Models.Responses.Entities.Apply;
using App.Backend.Core.Services.Interface.Apply;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("opendays"), Tags("Open Days")]
public class OpenDayController(IOpenDayService service) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List open days")]
    [EndpointDescription(@"Returns a paginated list of open days with how many people signed up and how many of those were verified as present.")]
    public async Task<ActionResult<IEnumerable<ApplyEventDO>>> GetAll(
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
            id is null ? null : c => c.Id == id,
            string.IsNullOrWhiteSpace(name) ? null : c => EF.Functions.ILike(c.Name, $"%{name}%"),
            after is null ? null : c => c.StartsAt >= after,
            before is null ? null : c => c.StartsAt <= before
        );

        var counts = await service.CountAsync([.. page.Items.Select(c => c.Id)], token);

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(c =>
        {
            var count = counts.GetValueOrDefault(c.Id);
            return new ApplyEventDO(c, count.Registered, count.Attended);
        }));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get an open day")]
    [EndpointDescription("Returns a single open day, including how many people signed up and how many were verified as present.")]
    public async Task<ActionResult<ApplyEventDO>> GetById(Guid id, CancellationToken token)
    {
        var openDay = await service.FindByIdAsync(id, token);
        if (openDay is null) return NotFound();

        var counts = await service.CountAsync([id], token);
        var count = counts.GetValueOrDefault(openDay.Id);
        return Ok(new ApplyEventDO(openDay, count.Registered, count.Attended));
    }

    [HttpPost]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Create an open day")]
    [EndpointDescription(@"
Creates an open day, the second step of admissions.

The capacity is optional, without one anyone can sign up. `deadlineAt` is the last moment to sign up and
can't be after `startsAt`, 422 otherwise.
    ")]
    public async Task<ActionResult<ApplyEventDO>> Create([FromBody] PostApplyEventRequestDTO body, CancellationToken token)
    {
        var openDay = await service.CreateAsync(new OpenDay
        {
            Name = body.Name.Trim(),
            Description = body.Description?.Trim() ?? string.Empty,
            Location = body.Location.Trim(),
            Capacity = body.Capacity,
            StartsAt = body.StartsAt.ToUniversalTime(),
            DeadlineAt = body.DeadlineAt.ToUniversalTime(),
        }, token);

        return CreatedAtAction(nameof(GetById), new { id = openDay.Id }, new ApplyEventDO(openDay, 0, 0));
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Update an open day")]
    [EndpointDescription(@"
Updates an open day. Send `unlimited: true` to remove the capacity.
The capacity can't drop below the amount of people already signed up (409), and the deadline
can't end up after the start (422).
    ")]
    public async Task<ActionResult<ApplyEventDO>> Update(Guid id, [FromBody] PatchApplyEventRequestDTO body, CancellationToken token)
    {
        var openDay = await service.FindByIdAsync(id, token);
        if (openDay is null) return NotFound();

        if (body.Name is not null) openDay.Name = body.Name.Trim();
        if (body.Description is not null) openDay.Description = body.Description.Trim();
        if (body.Location is not null) openDay.Location = body.Location.Trim();
        if (body.Unlimited is true) openDay.Capacity = null;
        else if (body.Capacity is { } capacity) openDay.Capacity = capacity;
        if (body.StartsAt is { } startsAt) openDay.StartsAt = startsAt.ToUniversalTime();
        if (body.DeadlineAt is { } deadlineAt) openDay.DeadlineAt = deadlineAt.ToUniversalTime();
        await service.UpdateAsync(openDay, token);

        var counts = await service.CountAsync([id], token);
        var count = counts.GetValueOrDefault(openDay.Id);
        return Ok(new ApplyEventDO(openDay, count.Registered, count.Attended));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Delete an open day")]
    [EndpointDescription("Deletes an open day together with the sign-ups that never turned into attendance. Fails (409) once anyone was verified as present.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        var openDay = await service.FindByIdAsync(id, token);
        if (openDay is null) return NotFound();

        await service.DeleteAsync(openDay, token);
        return NoContent();
    }

    [HttpGet("{id:guid}/registrations")]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List the sign-ups of an open day")]
    [EndpointDescription("Returns a paginated list of who signed up.")]
    public async Task<ActionResult<IEnumerable<ApplyRegistrationDO>>> GetRegistrations(
        Guid id,
        [FromQuery(Name = "filter[email]")] string? email,
        [FromQuery(Name = "filter[attended]")] bool? attended,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        if (await service.FindByIdAsync(id, token) is null)
            return NotFound();

        var page = await service.GetRegistrationsAsync(id, email, attended, sorting, pagination, token);
        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(r => new ApplyRegistrationDO(r)));
    }

    [HttpPost("{id:guid}/registrations")]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Sign someone up for an open day")]
    [EndpointDescription("Signs an applicant up by email and returns the sign-up. Safe to repeat. Fails with 409 if they weren't verified at a check-in yet (unless `Admissions:RequireCheckInForOpenDay` is off), the deadline passed, the open day is full, the person was already verified at another open day, or they are signed up for one that hasn't started.")]
    public async Task<ActionResult<ApplyRegistrationDO>> Register(Guid id, [FromBody] PostApplyRegistrationRequestDTO body, CancellationToken token)
    {
        var registration = await service.RegisterAsync(id, body.Email, body.FirstName, body.LastName, token);
        return Ok(new ApplyRegistrationDO(registration));
    }

    [HttpDelete("{id:guid}/registrations/{registrationId:guid}")]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Remove a sign-up")]
    [EndpointDescription("Removes a sign-up and hands the seat back. 409 once the person was verified as present, take that back first if it was a mistake.")]
    public async Task<IActionResult> Unregister(Guid id, Guid registrationId, CancellationToken token)
    {
        await service.UnregisterAsync(id, registrationId, token);
        return NoContent();
    }

    [HttpPut("{id:guid}/registrations/{registrationId:guid}/attendance")]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Verify someone was there")]
    [EndpointDescription(@"
Records that this person was at the open day, or with `attended: false` takes that back.
The time and who did it are recorded by the server. Safe to repeat, the first verification stays on record.

409 if it is too early (it opens shortly before the start) or too late (a day after).
    ")]
    public async Task<ActionResult<ApplyRegistrationDO>> SetAttendance(Guid id, Guid registrationId, [FromBody] PutApplyAttendanceRequestDTO body, CancellationToken token)
    {
        var registration = await service.SetAttendanceAsync(id, registrationId, body.Attended, User.GetSID(), token);
        return Ok(new ApplyRegistrationDO(registration));
    }
}
