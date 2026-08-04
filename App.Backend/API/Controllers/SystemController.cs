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

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("system")]
public class SystemController(
    DatabaseContext db, // NOTE(W2): Kinda shit, but honestly it's for 1 query.
    IUserService user,
    IWorkspaceService workspace,
    IMessageBus bus,
    KeycloakAdminApiClient keycloak
) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    [ExcludeFromDescription]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status418ImATeapot)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query the system status")]
    [EndpointDescription("Used to initilize / bootstrap the application on inital startup")]
    public async Task<IActionResult> Query(CancellationToken token)
    {
        var complete = await db.System.FirstOrDefaultAsync(token);
        return complete is null ? NoContent() : Problem(statusCode: 418);
    }

    [HttpPost]
    [ExcludeFromDescription]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Set the system status")]
    [EndpointDescription("Used to initilize / bootstrap the application on inital startup")]
    public async Task<IActionResult> Bootstrap([FromBody] PostUserRequestDTO body, CancellationToken token)
    {
        var complete = await db.System.FirstOrDefaultAsync(token);
        if (complete is not null) return Forbid();

        var id = Guid.CreateVersion7();
        await keycloak.Admin.Realms["admin"].Users.PostAsync(new()
        {
            Id = id.ToString(),
            Username = body.Login,
            Email = body.Email,
            Enabled = true,
            EmailVerified = false,
            // RealmRoles = [""],
            // TODO: Force 2FA at all times, configure it ?
            RequiredActions = ["UPDATE_PASSWORD"],
        }, null, token);

        var account = await user.CreateAsync(new()
        {
            Id = id,
            Login = body.Login,
            Display = body.Login,
            Details = new()
            {
                UserId = id,
                Email = body.Email,
                FirstName = body.FirstName,
                LastName = body.LastName,
            },
        }, token);

        await workspace.CreateAsync(new() { Ownership = EntityOwnership.Organization }, token);
        await workspace.CreateAsync(new() { OwnerId = id, Ownership = EntityOwnership.User }, token);
        await bus.PublishAsync(new WelcomeUserNotification(account!));
        return NoContent();
    }
}