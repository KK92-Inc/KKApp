using App.Backend.Models.Responses.Entities.Notifications;

namespace App.Backend.API.Notifications.Channels;

/// <summary>
/// Interface that mandates that the notification is to be stored in the database.
/// </summary>
/// <remarks>These notifications can be viewed on the notification dashboard.</remarks>
public interface IDatabaseChannel
{
    /// <summary>
    /// Converts this notification into a <see cref="NotificationData"/> payload
    /// to be serialised and stored in the database.
    /// </summary>
    public NotificationData ToDatabase();
}
