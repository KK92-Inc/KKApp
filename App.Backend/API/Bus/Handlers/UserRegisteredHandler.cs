// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.API.Notifications;
using App.Backend.Database;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Logging;
using NXTBackend.API.Core.Services.Interface;
using Razor.Templating.Core;
using Wolverine;
using Wolverine.Attributes;

// ============================================================================

[WolverineHandler]
public class UserRegisteredHandler
{
    public async Task Handle(UserRegistered message, IMessageBus bus, ILogger<UserRegisteredHandler> log)
    {
        // bus.PublishAsync(new WelcomeUserNotification(message.UserId, "H"));
        // var htmlBody = await render.RenderAsync("~/Views/Welcome.cshtml");
        log.LogInformation("Rendered HTML body for welcome email: {HtmlBody}", htmlBody);
    }
}
