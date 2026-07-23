// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Handlers.Infra;

using Wolverine.Attributes;
using App.Backend.API.Bus.Messages;
using App.Backend.API.Notifications.Variants;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Enums;
using Wolverine;
using Resend;

// ============================================================================

[WolverineHandler]
public class InboundEmailHandler(ILogger<InboundEmailHandler> logger, IResend resend, IUserService users)
{
    public async Task Handle(InboundEmailReceived message, CancellationToken ct)
    {
        var login = message.recipient.Split('@')[0];
        var user = await users.FindByLoginAsync(login, ct);
        if (user?.Details?.Email is null)
        {
            logger.LogInformation("No forwarding configured for {Login}, dropping.", login);
            return;
        }

        await resend.EmailSendAsync(new()
        {
            From = message.recipient,
            To = [user.Details.Email],
            Subject = message.Subject,
            HtmlBody = message.Html,
            TextBody = message.Text,
        }, ct);
    }
}