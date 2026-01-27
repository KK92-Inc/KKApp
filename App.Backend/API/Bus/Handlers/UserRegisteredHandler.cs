// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

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
        // log.LogInformation("Rendered HTML body for welcome email: {HtmlBody}", htmlBody);
    }
}
