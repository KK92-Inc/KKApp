// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using App.Backend.Core.Services;
using App.Backend.Domain.Entities;

namespace NXTBackend.API.Core.Services.Interface;

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
    Task MarkAsReadAsync(Guid userId, IEnumerable<Guid>? notificationIds = null);

    /// <summary>
    /// Marks notifications as unread for a given user.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="notificationIds">Optional list of specific notification IDs to mark as unread. If null or empty, marks all notifications as unread.</param>
    Task MarkAsUnreadAsync(Guid userId, IEnumerable<Guid>? notificationIds = null);
}
