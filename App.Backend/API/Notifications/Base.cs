using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities.Notifications;

public abstract record BaseNotification
{
    public virtual Guid NotifiableId { get; init; }

    // This is where your enum shines
    public virtual NotificationVariant Descriptor { get; }

    // Polymorphic payload
    public virtual NotificationData? Data { get; }

    public virtual Guid? ResourceId => null;
}
