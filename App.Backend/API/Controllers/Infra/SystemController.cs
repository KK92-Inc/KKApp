// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Wolverine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Core.Services.Interface;
using App.Backend.API.Notifications.Variants;
using App.Backend.Models.Requests;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("system")]
public class SystemController(ISystemService service, IMessageBus bus) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    [ExcludeFromDescription]
    public async Task<IActionResult> Query(CancellationToken token)
    {
        var entry = await service.CheckAsync(token);
        return entry is null ? NoContent() : Forbid();
    }

    [HttpPost]
    [AllowAnonymous]
    [ExcludeFromDescription]
    public async Task<IActionResult> Bootstrap([FromBody] SystemInitDTO body, CancellationToken token)
    {
        var account = await service.InitializeAsync(body.Login, body.Firstname, body.Lastname, body.Email, token);
        await bus.PublishAsync(new WelcomeUserNotification(account!));
        return NoContent();
    }
}