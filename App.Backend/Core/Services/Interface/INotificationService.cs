// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using App.Backend.Domain.Entities;

namespace App.Backend.Core.Services.Interface;

/// <summary>
/// Service for managing notifications
/// </summary>
public interface INotificationService : IDomainService<Notification>
{
    /// <summary>
    /// Marks notifications as read for a given user.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="notificationIds">Optional list of specific notification IDs to mark as read. If null or empty, marks all notifications as read.</param>
    Task MarkAsAsync(Guid userId, IEnumerable<Guid>? guids = null, bool read = true, CancellationToken token = default);

    /// <summary>
    /// Constructs the feed
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    // Task<Notification> GetNotificationFeed(Guid userId, CancellationToken token = default);
}
