// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Enums;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using App.Backend.Core.Query;
using System.Linq.Expressions;
using App.Backend.Domain.Entities.Events;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class EventService(DatabaseContext ctx, TimeProvider time) : BaseService<Event>(ctx), IEventService
{
    private readonly DatabaseContext context = ctx;

    public override async Task<PaginatedList<Event>> GetAllAsync(ISorting sorting, IPagination pagination, CancellationToken token = default, params Expression<Func<Event, bool>>?[] filters)
    {
        return await filters
            .Where(f => f is not null)
            .Aggregate(_dbSet.AsQueryable(), (c, filter) => c.Where(filter!))
            .Include(e => e.User)
            .Sort(sorting)
            .PaginateAsync(pagination, token);
    }

    public async Task<ILookup<Guid, User>> ParticipantsForEvents(IEnumerable<Guid> eventIds, CancellationToken token = default)
    {
        var ids = eventIds.ToArray();

        var rows = await context.UserEvent
            .Where(ev => ids.Contains(ev.EventId))
            .Select(ev => new { ev.EventId, ev.User })
            .ToListAsync(token);

        return rows.ToLookup(r => r.EventId, r => r.User);
    }

    public async Task<PaginatedList<(EventFeedback Feedback, Comment Comment)>> GetAllFeedbackAsync(
        Guid eventId,
        ISorting sorting,
        IPagination pagination,
        CancellationToken token = default)
    {
        return await context.EventFeedbacks
            .Where(f => f.EventId == eventId)
            .Sort(sorting)
            .Join(
                context.Comments,
                feedback => feedback.CommentId,
                comment => comment.Id,
                (feedback, comment) => ValueTuple.Create(feedback, comment)
            )
            .PaginateAsync(pagination, token);
    }

    public async Task<(EventFeedback Feedback, Comment Comment)> AddFeedbackAsync(Guid eventId, Guid userId, int rating, string comment, CancellationToken token = default)
    {
        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            var submitted = await context.EventFeedbacks
                .Where(f => f.EventId == eventId)
                .Join(
                    context.Comments,
                    feedback => feedback.CommentId,
                    c => c.Id,
                    (feedback, c) => c
                )
                .AnyAsync(c => c.UserId == userId, ct);

            ServiceException.ThrowIf(submitted, "You have already submitted feedback for this event.");

            var result = await context.EventFeedbacks.AddAsync(new()
            {
                Rating = rating,
                EventId = eventId
            }, ct);

            // Save first so a database-generated feedback ID is available to the comment.
            await context.SaveChangesAsync(ct);
            var commentResult = await context.Comments.AddAsync(new()
            {
                EntityId = result.Entity.Id,
                EntityType = nameof(EventFeedback),
                UserId = userId,
                Body = comment,
            }, ct);

            result.Entity.CommentId = commentResult.Entity.Id;
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return (result.Entity, commentResult.Entity);
        }, token);
    }

    public async Task<bool> Participates(Guid eventId, Guid userId, CancellationToken token = default)
    {
        return await context.UserEvent
            .AnyAsync(ev => ev.EventId == eventId && ev.UserId == userId, token);
    }

    public async Task<IEnumerable<User>> Participants(Guid eventId, CancellationToken token = default)
    {
        return await context.UserEvent
            .Where(ev => ev.EventId == eventId)
            .Select(ev => ev.User)
            .ToListAsync(token);
    }

    public async Task JoinAsync(Guid eventId, Guid userId, CancellationToken token = default)
    {
        await context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(token);

            var now = time.GetUtcNow();
            var status = await context.Events
                .Where(e => e.Id == eventId)
                .Select(e => new
                {
                    Event = e,
                    AttendeeCount = context.UserEvent.Count(ue => ue.EventId == e.Id),
                    Joined = context.UserEvent.Any(ue => ue.EventId == e.Id && ue.UserId == userId)
                })
                .FirstOrDefaultAsync(token);

            ServiceException.ThrowIf(status is null, "Event not found");
            ServiceException.ThrowIf(status!.Event.State is EventState.Rejected, "Event was rejected.");
            ServiceException.ThrowIf(status.Event.State is EventState.Finished, "Event has already completed.");
            ServiceException.ThrowIf(status.Event.UserId == userId, "You can't join your own event.");
            ServiceException.ThrowIf(status.Joined, "Already joined this event.");
            ServiceException.ThrowIf(status.Event.ClosesAt.HasValue && now > status.Event.ClosesAt.Value, "Registration for this event is closed.");
            ServiceException.ThrowIf(status.AttendeeCount >= status.Event.Capacity, "Unable to join, event is full.");

            await context.UserEvent.AddAsync(new UserEvent { EventId = eventId, UserId = userId }, token);

            var @event = status.Event;
            if (@event.State is EventState.Pending && @event.Threshold.HasValue && (status.AttendeeCount + 1) >= @event.Threshold.Value)
                @event.State = EventState.Accepted;

            await UpdateAsync(@event, token);
            await transaction.CommitAsync(token);
        });
    }

    public async Task LeaveAsync(Guid eventId, Guid userId, CancellationToken token = default)
    {
        var now = time.GetUtcNow();
        var status = await context.Events
            .Where(e => e.Id == eventId)
            .Select(e => new
            {
                Event = e,
                AttendeeCount = context.UserEvent.Count(ue => ue.EventId == e.Id),
                IsClosed = e.ClosesAt.HasValue && now > e.ClosesAt.Value,
                IsJoined = context.UserEvent.Any(ue => ue.EventId == e.Id && ue.UserId == userId)
            })
            .FirstOrDefaultAsync(token);

        ServiceException.ThrowIf(status is null, "Event not found");
        ServiceException.ThrowIf(status!.Event.State is EventState.Rejected, "Event was rejected.");
        ServiceException.ThrowIf(status.Event.State is EventState.Finished, "Event has already completed.");
        ServiceException.ThrowIf(!status.IsJoined, "You have not joined this event.");
        ServiceException.ThrowIf(status.IsClosed, "You can no longer leave this event as registration has closed.");

        await context.UserEvent
            .Where(ue => ue.EventId == eventId && ue.UserId == userId)
            .ExecuteDeleteAsync(token);

        var @event = status.Event;
        if (@event.State is EventState.Accepted && @event.Threshold.HasValue && (status.AttendeeCount - 1) < @event.Threshold.Value)
        {
            @event.State = EventState.Pending;
            await UpdateAsync(@event, token);
        }
    }
}
