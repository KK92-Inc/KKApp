// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Resend;
using System.Text.Json;
using Wolverine.Attributes;
using Razor.Templating.Core;
using App.Backend.API.Notifications.Channels;
using App.Backend.API.Notifications;
using App.Backend.Core.Services.Interface;
using System.Threading.Channels;
using App.Backend.API.Bus.Messages;
using App.Backend.API.Notifications.Registers.Interface;
using App.Backend.API.Notifications.Variants;

// ============================================================================

[WolverineHandler]
public class NotificationHandler(
    IUserService userService,
    INotificationService notificationService,
    ILogger<NotificationHandler> logger,
    IBroadcastRegistry registry,
    IRazorTemplateEngine razor,
    IResend resend
)
{
    public async Task Handle(WelcomeUserNotification payload, CancellationToken token) => await Internal(payload, token);

    private async Task Internal(INotificationMessage notification, CancellationToken token)
    {
        logger.LogInformation("Handling notification: {@Notification}", notification);
        if (notification is IEmailChannel email)
        {
            logger.LogDebug("Submitting email for notification...");
            var mail = email.ToMail();
            var user = await userService.FindByIdAsync(notification.NotifiableId);
            if (user is not null && user.Details?.Email is not null)
            {
                await resend.EmailSendAsync(new()
                {
                    From = "portal@resend.dev", // TODO: Retrieve from configuration
                    Subject = mail.Subject,
                    To = user.Details.Email,
                    HtmlBody = await razor.RenderAsync(mail.View, mail.Model)
                }, token);
                logger.LogDebug("Submitting email for notification... [OK]");
            }
        }
        if (notification is IBroadcastChannel broadcast)
        {
            logger.LogDebug("Broadcasting notification...");

            var notifiableId = notification.NotifiableId;
            await registry.PublishAsync(notifiableId, broadcast.ToBroadcast(), token);

            logger.LogDebug("Broadcasting notification... [OK]");
        }
        if (notification is IDatabaseChannel message)
        {
            logger.LogDebug("Writing notification to database...");
            await notificationService.CreateAsync(new()
            {
                Descriptor = notification.Meta,
                ResourceId = notification.ResourceId,
                NotifiableId = notification.NotifiableId,
                Data = JsonSerializer.Serialize(message.ToDatabase())
            }, token);
            logger.LogDebug("Writing notification to database.... [OK]");
        }
    }
}
