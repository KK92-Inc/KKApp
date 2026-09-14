// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Query;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Events;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Relations;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IEventService : IDomainService<Event>
{
    /// <summary>
    /// Check if a user is participating in an event.
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<bool> Participates(Guid eventId, Guid userId, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<IEnumerable<User>> Participants(Guid eventId, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventIds"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<ILookup<Guid, User>> ParticipantsForEvents(IEnumerable<Guid> eventIds, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task JoinAsync(Guid eventId, Guid userId, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task LeaveAsync(Guid eventId, Guid userId, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="userId"></param>
    /// <param name="rating"></param>
    /// <param name="comment"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<(EventFeedback Feedback, Comment Comment)> AddFeedbackAsync(Guid eventId, Guid userId, int rating, string comment, CancellationToken token = default);

    /// <summary>
    /// Gets all the Feedback + Comments attached to a specific event.
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="sorting"></param>
    /// <param name="pagination"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<PaginatedList<(EventFeedback Feedback, Comment Comment)>> GetAllFeedbackAsync(
        Guid eventId,
        ISorting sorting,
        IPagination pagination,
        CancellationToken token = default
    );
}
