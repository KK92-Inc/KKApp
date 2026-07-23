// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Core.Services.Interface;
using Microsoft.Extensions.Options;
using Resend;
using App.Backend.Core.Services.Options;
using System.Text.Json;
using System.Net;
using Svix.Exceptions;
using App.Backend.Domain.Values.Webhooks.Resend;
using Wolverine;
using App.Backend.API.Bus.Messages;

// ============================================================================

namespace App.Backend.API.Controllers.Infra;

[AllowAnonymous, ApiController, Route("webhooks"), Tags("Webhook")]
public class EmailController(
    IUserService users,
    IResend resend,
    IOptions<WebhookOptions> options,
    ILogger<EmailController> logger,
    IMessageBus bus
) : ControllerBase
{
    [HttpPost("email/inbound")]
    [AllowAnonymous]
    public async Task<IActionResult> Receive([FromBody] JsonDocument json, CancellationToken ct)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var payload = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

        try
        {
            var webhook = new Svix.Webhook(options.Value.ResendSecret);
            webhook.Verify(payload, new WebHeaderCollection
            {
                ["svix-id"] = Request.Headers["svix-id"],
                ["svix-timestamp"] = Request.Headers["svix-timestamp"],
                ["svix-signature"] = Request.Headers["svix-signature"],
            });
        }
        catch (WebhookVerificationException)
        {
            logger.LogWarning("Unauthorized Webhook attempt");
            return Unauthorized();
        }

        var evt = JsonSerializer.Deserialize<EmailReceived>(payload)!;
        if (evt.Type != "email.received")
            return Ok();


        logger.LogDebug("{evt}", evt);
        foreach (var recipient in evt.Data.To)
        {
            var email = await resend.EmailRetrieveAsync(evt.Data.EmailId, ct);
            await bus.PublishAsync(new InboundEmailReceived(
                recipient,
                email.Content.Subject,
                email.Content.HtmlBody,
                email.Content.TextBody
            ));
        }

        return Ok();
    }
}