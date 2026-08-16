// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.API.Params;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Users;
using App.Backend.Models.Responses.Entities.Reviews;
using App.Backend.Models.Requests.Reviews;
using App.Backend.Domain.Enums;
using App.Backend.Database;
using Microsoft.EntityFrameworkCore;
using ImTools;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.API.Bus.Messages;
using App.Backend.Core;
using Wolverine;
using System.ComponentModel;
using System.Linq.Expressions;
using App.Backend.API.Utils;
using Microsoft.AspNetCore.OutputCaching;
using App.Backend.Models.Requests.Users;
using Keycloak.AuthServices.Sdk.Kiota.Admin;
using App.Backend.API.Notifications.Variants;
using Keycloak.AuthServices.Sdk.Kiota.Admin.Models;
using App.Backend.Models.Requests.SshKeys;

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
        var account = await service.InitializeAsync(body.Login, body.Password, body.Email, token);
        await bus.PublishAsync(new WelcomeUserNotification(account!));
        return NoContent();
    }
}