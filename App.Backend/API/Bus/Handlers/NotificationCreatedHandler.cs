using System.Text.Json;
using App.Backend.Database;
using App.Backend.Domain.Entities;
using NXTBackend.API.Core.Services.Interface;
using Wolverine;

public class CreateNotificationHandler(INotificationService service, IMessageContext bus)
{
    public async Task Handle(BaseNotification message)
    {
        var notification = await service.CreateAsync(new ()
        {
            Descriptor = message.Descriptor,
            ResourceId = message.ResourceId,
            NotifiableId = message.NotifiableId,
            Data = JsonSerializer.Serialize(message.Data)
        });

        await bus.PublishAsync(new CreatedNotification(notification));
    }
}
