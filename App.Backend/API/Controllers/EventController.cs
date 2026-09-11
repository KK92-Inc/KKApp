// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.API.Params;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Requests.Event;
using System.Linq.Expressions;
using App.Backend.Domain.Entities.Events;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.API.Controllers;

[Route("events")]
[ApiController, Authorize]
public class EventController(IAuthorizationService auth, IEventService service) : Controller
{
    [HttpGet]
    // TODO: We *can* implement the scope but I'd wait for details.
    // [RequireScope("event")]
    // [ProtectedResource("events", "events:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all events")]
    [EndpointDescription("Retrieve a paginated list of all events")]
    public async Task<ActionResult<IEnumerable<EventDO>>> GetAll(
        [FromQuery(Name = "filter[id]")] Guid? id,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[state]")] EventState? state,
        [FromQuery(Name = "filter[not[state]]")] EventState? notState,
        [FromQuery(Name = "filter[year]"), Range(2000, 2077)] int? year,
        [FromQuery(Name = "filter[month]"), Range(1, 12)] int? month,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        Expression<Func<Event, bool>>? date = null;

        if (year.HasValue || month.HasValue)
        {
            var target = year ?? DateTimeOffset.UtcNow.Year;
            var start = month.HasValue
                ? new DateTimeOffset(target, month.Value, 1, 0, 0, 0, TimeSpan.Zero)
                : new DateTimeOffset(target, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var end = month.HasValue ? start.AddMonths(1) : start.AddYears(1);
            date = e => e.StartsAt < end && e.EndsAt >= start;
        }

        var page = await service.GetAllAsync(sorting, pagination, token,
            id is null ? null : e => e.Id == id,
            state is null ? null : e => e.State == state,
            notState is null ? null : e => e.State != notState,
            date,
            string.IsNullOrWhiteSpace(name) ? null : e => EF.Functions.ILike(e.Name, $"%{name}%")
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(e => new EventDO(e)));
    }

    [HttpPost("{id:guid}/feedback")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Add feedback to an event")]
    [EndpointDescription("Submits a rating and comment for a specific event")]
    public async Task<ActionResult<EventFeedbackDO>> AddFeedback(
        Guid id,
        [FromBody] PostEventFeedbackRequestDTO request,
        CancellationToken token
    )
    {
        var userId = User.GetSID();
        var @event = await service.FindByIdAsync(id, token);
        if (@event is null) return NotFound();

        if (@event.State is not EventState.Completed)
            return Problem(title: "Event is not yet completed", statusCode: 422);
        if (!await service.Participates(id, userId, token))
            return Forbid();

        var (feedback, comment) = await service.AddFeedbackAsync(id, userId, request.Rating, request.Comment, token);
        return CreatedAtAction(nameof(GetFeedback), new { id }, new EventFeedbackDO(feedback, comment));
    }

    [HttpGet("{id:guid}/feedback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query event feedback")]
    [EndpointDescription("Retrieve a paginated list of feedback for a specific event")]
    public async Task<ActionResult<IEnumerable<EventFeedbackDO>>> GetFeedback(
        Guid id,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        var page = await service.GetAllFeedbackAsync(id, sorting, pagination, token);
        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(tuple => new EventFeedbackDO(tuple.Feedback, tuple.Comment)));
    }

    [HttpGet("{id:guid}/users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query event participants")]
    [EndpointDescription("Retrieve list of registered users for a specific event")]
    public async Task<ActionResult<IEnumerable<UserDO>>> GetParticipants(Guid id, CancellationToken token)
    {
        var @event = await service.FindByIdAsync(id, token);
        if (@event is null) return NotFound();

        var users = await service.Participants(id, token);
        return Ok(users.Select(u => new UserDO(u)));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Create a new event")]
    [EndpointDescription("Creates a new campus event created by the requesting user")]
    public async Task<ActionResult<EventDO>> Create(
        [FromBody] PostEventRequestDTO body,
        [FromQuery(Name = "event[pending]")] bool? pending,
        CancellationToken token
    )
    {
        var userId = User.GetSID();
        var initial = EventState.Pending;

        if (pending.HasValue && !pending.Value)
        {
            var staff = await auth.AuthorizeAsync(User, "staff");
            if (staff.Succeeded) initial = EventState.Accepted;
        }

        var createdEvent = await service.CreateAsync(new()
        {
            UserId = userId,
            Name = body.Name,
            Markdown = body.Markdown,
            // NOTE(W2): Basically it makes no sense to ask for a threshold...
            // If you say it *will* happen then don't ask for a certain amount to join.
            State = initial,
            Description = body.Description,
            Thumbnail = body.Thumbnail,
            Threshold = pending.HasValue ? null : body.Threshold,
            Capacity = body.Capacity,
            StartsAt = body.StartsAt,
            EndsAt = body.EndsAt,
            ClosesAt = body.ClosesAt,
        }, token);

        return CreatedAtAction(
            nameof(Create),
            new { filter_id = createdEvent.Id },
            new EventDO(createdEvent)
        );
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Reject/Cancel an event")]
    [EndpointDescription("Reject/Cancel an existing event by ID")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        var @event = await service.FindByIdAsync(id, token);
        if (@event is null) return NotFound();

        var userId = User.GetSID();
        var staff = await auth.AuthorizeAsync(User, "staff");
        if (@event.UserId != userId && !staff.Succeeded)
            return Forbid();

        // Prevent already completed events from being cancelled.
        // Accepted ones can still be cancelled for maybe unexpected reasons.
        if (@event.State is EventState.Completed)
            return Problem(title: "Event is already completed", statusCode: 422);
        if (@event.State is EventState.Rejected)
            return Problem(title: "Event is already rejected", statusCode: 422);

        @event.State = EventState.Rejected;
        await service.UpdateAsync(@event, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/join")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Join an event")]
    [EndpointDescription("Registers the authenticated user for an event")]
    public async Task<IActionResult> Join(Guid id, CancellationToken token)
    {
        var @event = await service.FindByIdAsync(id, token);
        if (@event is null) return NotFound();

        if (@event.State is EventState.Completed)
            return Problem(title: "Event is already completed", statusCode: 422);
        if (@event.State is EventState.Rejected)
            return Problem(title: "Event was rejected", statusCode: 422);

        var userId = User.GetSID();
        await service.JoinAsync(id, userId, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/leave")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Leave an event")]
    [EndpointDescription("Unregisters the authenticated user from an event")]
    public async Task<IActionResult> Leave(Guid id, CancellationToken token)
    {
        var @event = await service.FindByIdAsync(id, token);
        if (@event is null) return NotFound();

        if (@event.State is EventState.Completed)
            return Problem(title: "Event is already completed", statusCode: 422);
        if (@event.State is EventState.Rejected)
            return Problem(title: "Event was rejected", statusCode: 422);

        var userId = User.GetSID();
        await service.LeaveAsync(id, userId, token);
        return NoContent();
    }
}