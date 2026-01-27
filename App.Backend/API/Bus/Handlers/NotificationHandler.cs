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

// ============================================================================

[WolverineHandler]
public class NotificationHandler(INotificationService service, IRazorTemplateEngine render, IResend client)
{
    public async Task Handle(BaseNotification message)
    {
        var notification = await service.CreateAsync(new()
        {
            Descriptor = message.Descriptor,
            ResourceId = message.ResourceId,
            NotifiableId = message.NotifiableId,
            Data = JsonSerializer.Serialize(message.Descriptor)
        });

        // Pattern matching: message could implement any combination of the interfaces
        if (message is IEmailChannel<BaseViewModel> email)
        {
            await client.EmailSendAsync(new()
            {
                Subject = email.Subject,
                HtmlBody = await render.RenderAsync(email.View, email.Model)
            });
        }

        if (message is IDefaultChannel)
        {
            // Handle default channel logic here
        }

        if (message is IFeedChannel)
        {
            // Handle feed channel logic here
        }
    }
}
