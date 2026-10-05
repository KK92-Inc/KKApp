// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using App.Backend.API.Params;
using App.Backend.API.Bus.Messages.Trials;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Models.Requests.Trials;
using App.Backend.Models.Responses.Entities;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("trials"), Tags("Trials")]
public class TrialController(
    ITrialService service,
    IMessageBus bus,
    TimeProvider time,
    IAuthorizationService auth
) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List trials")]
    [EndpointDescription(@"
Returns a paginated list of trial periods with how many users are in each.
Use `filter[open]=true` to only get the trials that haven't ended yet,
i.e. the ones a user can still pick, and `filter[after]` / `filter[before]`
to look at a window of start dates.
    ")]
    public async Task<ActionResult<IEnumerable<TrialDO>>> GetAll(
        [FromQuery(Name = "filter[id]")] Guid? id,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[cursus]")] Guid? cursusId,
        [FromQuery(Name = "filter[open]")] bool? open,
        [FromQuery(Name = "filter[after]")] DateTimeOffset? after,
        [FromQuery(Name = "filter[before]")] DateTimeOffset? before,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        var now = time.GetUtcNow();
        Expression<Func<Trial, bool>>? window = null;
        if (open is true)
            window = t => t.EndedAt == null && t.EndsAt > now;
        else if (open is false)
            window = t => t.EndedAt != null || t.EndsAt <= now;

        var page = await service.GetAllAsync(sorting, pagination, token,
            id is null ? null : t => t.Id == id,
            string.IsNullOrWhiteSpace(name) ? null : t => EF.Functions.ILike(t.Name, $"%{name}%"),
            cursusId is null ? null : t => t.CursusId == cursusId,
            window,
            after is null ? null : t => t.StartsAt >= after,
            before is null ? null : t => t.StartsAt <= before
        );

        var counts = await service.CountParticipantsAsync([.. page.Items.Select(t => t.Id)], token);

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(t => new TrialDO(t, counts.GetValueOrDefault(t.Id))));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get a trial")]
    [EndpointDescription("Returns a single trial, including how many users are in it and whether it already started or ended.")]
    public async Task<ActionResult<TrialDO>> GetById(Guid id, CancellationToken token)
    {
        var trial = await service.FindByIdAsync(id, token);
        if (trial is null) return NotFound();

        var counts = await service.CountParticipantsAsync([id], token);
        return Ok(new TrialDO(trial, counts.GetValueOrDefault(id)));
    }

    [HttpPost]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Create a trial")]
    [EndpointDescription(@"
Creates a trial period.

When `startsAt` passes, its registered users are activated: their account is enabled and they are subscribed to the cursus.
When `endsAt` passes they are wrapped up and their account is disabled again.

A start in the past begins it on the next run of the trial job.
422 if it ends before it starts or in the past, or if the cursus doesn't exist.
    ")]
    public async Task<ActionResult<TrialDO>> Create([FromBody] PostTrialRequestDTO body, CancellationToken token)
    {
        var trial = await service.CreateAsync(new Trial
        {
            Name = body.Name.Trim(),
            CursusId = body.CursusId,
            Capacity = body.Capacity,
            StartsAt = body.StartsAt.ToUniversalTime(),
            EndsAt = body.EndsAt.ToUniversalTime(),
        }, token);

        return CreatedAtAction(nameof(GetById), new { id = trial.Id }, new TrialDO(trial, 0));
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
    [EndpointSummary("Update a trial")]
    [EndpointDescription(@"
Updates the name, cursus, capacity or dates. Omitted fields are left alone.
The capacity can't drop below the current participant count (409),
the start date and cursus can't change once the trial has started (409),
and the end date can't change once it has ended (409).

Moving the end of a running trial is how you end it early.
    ")]
    public async Task<ActionResult<TrialDO>> Update(Guid id, [FromBody] PatchTrialRequestDTO body, CancellationToken token)
    {
        var trial = await service.FindByIdAsync(id, token);
        if (trial is null) return NotFound();

        if (body.Name is not null) trial.Name = body.Name.Trim();
        if (body.CursusId is { } cursusId) trial.CursusId = cursusId;
        if (body.Capacity is { } capacity) trial.Capacity = capacity;
        if (body.StartsAt is { } startsAt) trial.StartsAt = startsAt.ToUniversalTime();
        if (body.EndsAt is { } endsAt) trial.EndsAt = endsAt.ToUniversalTime();
        await service.UpdateAsync(trial, token);

        var counts = await service.CountParticipantsAsync([id], token);
        return Ok(new TrialDO(trial, counts.GetValueOrDefault(id)));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Delete a trial")]
    [EndpointDescription("Deletes a trial. Fails with 409 while it has any participants, including the ones that finished or quit, since that is the record of who took part.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        var trial = await service.FindByIdAsync(id, token);
        if (trial is null) return NotFound();

        await service.DeleteAsync(trial, token);
        return NoContent();
    }

    [HttpGet("{id:guid}/users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List the participants of a trial")]
    [EndpointDescription("Returns a paginated list of the participations in a trial. Use `filter[outcome]=Pending` to see who still needs a verdict, and `filter[state]` to look at e.g. only the ones that quit.")]
    public async Task<ActionResult<IEnumerable<UserTrialDO>>> GetUsers(
        Guid id,
        [FromQuery(Name = "filter[state]")] UserTrialState? state,
        [FromQuery(Name = "filter[outcome]")] TrialOutcome? outcome,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        if (await service.FindByIdAsync(id, token) is null) return NotFound();

        var page = await service.GetParticipantsAsync(id, state, outcome, sorting, pagination, token);

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(ut => new UserTrialDO(ut)));
    }

    [HttpGet("{id:guid}/users/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get the participation of a user")]
    [EndpointDescription("Returns the participation of a user in a trial. 404 if they aren't in it. Staff can look up anyone, everyone else only themselves.")]
    public async Task<ActionResult<UserTrialDO>> GetUser(Guid id, Guid userId, CancellationToken token)
    {
        if (!await CanSeeAsync(userId)) return Forbid();

        var participation = await service.FindParticipantAsync(id, userId, token);
        return participation is null ? NotFound() : Ok(new UserTrialDO(participation));
    }

    [HttpPut("{id:guid}/users/{userId:guid}")]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Sign a user up for a trial")]
    [EndpointDescription("Signs an applicant up for a trial and returns their participation. Safe to repeat. Fails with 404 if the trial or user doesn't exist, 422 if the user isn't an applicant, and 409 if the trial is over or full, the user already quit it, or they are part of another trial that hasn't finished. If the trial is already running the user is activated right away.")]
    public async Task<ActionResult<UserTrialDO>> AddUser(Guid id, Guid userId, CancellationToken token)
    {
        var participation = await service.JoinAsync(id, userId, token);
        await ActivateIfRunningAsync(participation);
        return Ok(new UserTrialDO(participation));
    }

    [HttpDelete("{id:guid}/users/{userId:guid}")]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Take a user out of a trial")]
    [EndpointDescription("Takes a user out of a trial. If it hasn't started for them they are simply removed. If they are taking part they are marked as having quit, which is kept as a record, and their account is disabled again. 404 if they aren't in it, 409 if the trial is over for them already.")]
    public async Task<IActionResult> RemoveUser(Guid id, Guid userId, CancellationToken token)
    {
        var quit = await service.LeaveAsync(id, userId, token);
        if (quit is not null)
            await bus.PublishAsync(new DeactivateTrialParticipant(userId, id));

        return NoContent();
    }

    [HttpPatch("{id:guid}/users/{userId:guid}")]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Decide on a participant")]
    [EndpointDescription("Records whether a user was selected to become a student, and returns their participation. This is what lets the admission platform know the user may move on to pick a kickoff. 404 if they aren't in the trial, 409 if the trial isn't over for them yet. Can be changed again later.")]
    public async Task<ActionResult<UserTrialDO>> Decide(Guid id, Guid userId, [FromBody] PatchUserTrialRequestDTO body, CancellationToken token)
    {
        await service.SetOutcomeAsync(id, [userId], body.Outcome, token);

        var participation = await service.FindParticipantAsync(id, userId, token);
        return participation is null ? NotFound() : Ok(new UserTrialDO(participation));
    }

    [HttpPost("{id:guid}/users/outcome")]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Decide on several participants")]
    [EndpointDescription("Bulk version of deciding on a participant, so a whole cohort can be handled in one go. All or nothing: if any user isn't in the trial (404) or the trial isn't over for them yet (409), nobody is changed and the error names the offending users.")]
    public async Task<IActionResult> DecideMany(Guid id, [FromBody] TrialOutcomesRequestDTO body, CancellationToken token)
    {
        await service.SetOutcomeAsync(id, body.UserIds, body.Outcome, token);
        return NoContent();
    }

    [Tags("Users")]
    [HttpGet("~/users/{userId:guid}/trials")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get the trials of a user")]
    [EndpointDescription("Returns every trial a user has taken part in, newest first. Empty if they never did. This is how the admission platform finds out whether a user finished a trial and was selected. Staff can look up anyone, everyone else only themselves.")]
    public async Task<ActionResult<IEnumerable<UserTrialDO>>> GetByUser(Guid userId, CancellationToken token)
    {
        if (!await CanSeeAsync(userId)) return Forbid();

        var participations = await service.GetByUserAsync(userId, token);
        return Ok(participations.Select(ut => new UserTrialDO(ut)));
    }

    /// <summary>
    /// Staff can look at anyone, everyone else only at themselves.
    /// </summary>
    private async Task<bool> CanSeeAsync(Guid userId)
    {
        return userId == User.GetSID() || (await auth.AuthorizeAsync(User, "staff")).Succeeded;
    }

    /// <summary>
    /// A trial only starts once. Anyone who signs up after it started would never be activated
    /// by it, so they get queued for activation right away. The handler skips everyone who isn't
    /// registered, so passing along someone who was already in is harmless.
    /// </summary>
    private async Task ActivateIfRunningAsync(UserTrial participation)
    {
        if (participation.State is not UserTrialState.Registered) return;
        if (participation.Trial.StartedAt is null || participation.Trial.EndedAt is not null) return;

        await bus.PublishAsync(new ActivateTrialParticipant(participation.UserId, participation.TrialId));
    }
}
