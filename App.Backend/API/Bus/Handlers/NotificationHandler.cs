// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Wolverine;
using System.Text.Json;
using NXTBackend.API.Core.Services.Interface;
using Wolverine.Attributes;
using Razor.Templating.Core;
using Resend;
using System.Text.RegularExpressions;
using App.Backend.API.Notifications.Channels;
using App.Backend.API.Views.Models;
using App.Backend.API.Notifications;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;

// ============================================================================

[WolverineHandler]
public class NotificationHandler(
    INotificationService service,
    IUserService users,
    ILogger<NotificationHandler> logger,
    IRazorTemplateEngine render,
    Channel<IBroadcastMessage> broadcastChannel,
    IResend client) : Hub
{
    public async Task Handle(NotificationRequest notification)
    {
        logger.LogInformation("Handling notification: {@Notification}", notification);

        if (notification.Content is IEmailChannel email)
        {
            var mail = email.ToMail();
            logger.LogInformation("Notification is an email channel. Subject: {Subject}", mail.Subject);
            var entity = await users.FindByIdAsync(notification.Content.NotifiableId);
            // var entity = notification.Content.Meta switch
            // {
            //     NotificationMeta.User => await users.FindByIdAsync(notification.Content.NotifiableId),
            //     NotificationMeta.Organization => throw new NotImplementedException(),
            //     _ => null
            // };

            logger.LogDebug("Resolved entity: {@Entity}", entity);

            if (entity?.Details?.Email is not null)
            {
                logger.LogInformation("Sending email to {Email}", entity.Details.Email);
                await client.EmailSendAsync(new()
                {
                    // TODO: Convert it to what Resend wants
                    ReplyTo = mail.To.First().Address,
                    From = "portal@resend.dev",
                    Subject = mail.Subject,
                    HtmlBody = mail.Body
                });

                logger.LogInformation("Email sent to {Email}", entity.Details.Email);
            }
            else
            {
                logger.LogWarning("No email found for entity: {@Entity}", entity);
            }
        }

        if (notification.Content is IDatabaseChannel content)
        {
            logger.LogInformation("Notification is a database channel. NotifiableId: {NotifiableId}", notification.Content.NotifiableId);

            await service.CreateAsync(new()
            {
                Descriptor = notification.Content.Meta,
                ResourceId = notification.Content.ResourceId,
                NotifiableId = notification.Content.NotifiableId,
                Data = JsonSerializer.Serialize(content)
            });

            logger.LogInformation("Database notification created for NotifiableId: {NotifiableId}", notification.Content.NotifiableId);
        }

        if (notification.Content is IBroadcastChannel broadcast)
        {
            var message = broadcast.ToBroadcast();
            await broadcastChannel.Writer.WriteAsync(message);
        }

    }
}
