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

// ============================================================================

[WolverineHandler]
public class NotificationHandler(
    IUserService userService,
    INotificationService notificationService,
    ILogger<NotificationHandler> logger,
    Channel<BroadcastMessage> channel,
    IRazorTemplateEngine razor,
    IResend resend
)
{
    public async Task Handle(NotificationRequest notification, CancellationToken token)
    {
        logger.LogInformation("Handling notification: {@Notification}", notification);
        if (notification.Content is IEmailChannel email)
        {
            logger.LogDebug("Submitting email for notification...");
            var mail = email.ToMail();
            var user = await userService.FindByIdAsync(notification.Content.NotifiableId);
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
        if (notification.Content is IBroadcastChannel broadcast)
        {
            logger.LogDebug("Broadcasting notification...");
            await channel.Writer.WriteAsync(broadcast.ToBroadcast(), token);
            logger.LogDebug("Broadcasting notification... [OK]");
        }
        if (notification.Content is IDatabaseChannel message)
        {
            logger.LogDebug("Writing notification to database...");
            await notificationService.CreateAsync(new()
            {
                Descriptor = notification.Content.Meta,
                ResourceId = notification.Content.ResourceId,
                NotifiableId = notification.Content.NotifiableId,
                Data = JsonSerializer.Serialize(message.ToDatabase())
            }, token);
            logger.LogDebug("Writing notification to database.... [OK]");
        }
    }
}
