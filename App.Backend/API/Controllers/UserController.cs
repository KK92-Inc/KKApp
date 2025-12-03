// ============================================================================
// Copyright (c) 2024 - W2Wizard.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Core.Query;
using App.Backend.API.Params;
using App.Backend.Core.Services.Implementation;
using App.Backend.Domain;
using App.Backend.Core.Services.Interface;
using App.Backend.Models.Entities;
using Keycloak.AuthServices.Authorization;
using App.Backend.Domain.Enums;
using NXTBackend.API.Core.Services.Interface;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("users")]
[ProtectedResource("users")]
public class UserController(ILogger<UserController> log, IUserService users) : Controller
{
    [HttpGet("/users/current")]
    [ProtectedResource("users", "user:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get the currently authenticated user.")]
    [EndpointDescription("When authenticated it's useful to know who you currently are logged in as.")]
    public async Task<ActionResult<UserDO>> Current()
    {
        var user = await users.FindByIdAsync(User.GetSID());
        return user is null ? Forbid() : Ok(new UserDO(user));
    }

    [HttpGet("/users/current/notifications")]
    [ProtectedResource("users", "user:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get the currently authenticated user.")]
    [EndpointDescription("When authenticated it's useful to know who you currently are logged in as.")]
    public async Task<ActionResult<UserDO>> CurrentNotifications(
        [FromQuery(Name = "filter[read]")] bool read,
        [FromQuery(Name = "filter[variant]")] NotifiableVariant inclusive,
        [FromQuery(Name = "filter[not[variant]]")] NotifiableVariant exclusive,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        INotificationService notifications
    )
    {
        var page = await notifications.GetAllAsync(pagination, sorting,
            n => read ? n.ReadAt != null : n.ReadAt == null,
            n => (n.Descriptor & inclusive) != 0,
            n => (n.Descriptor & exclusive) == 0
        );

        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(e => new NotificationDO(e)));
    }

    [HttpGet("/users/")]
    [ProtectedResource("users", "user:view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get the currently authenticated user.")]
    [EndpointDescription("When authenticated it's useful to know who you currently are logged in as.")]
    public async Task<ActionResult<UserDO>> CurrentNotifications(
    [FromQuery(Name = "filter[read]")] bool read,
    [FromQuery(Name = "filter[variant]")] NotifiableVariant inclusive,
    [FromQuery(Name = "filter[not[variant]]")] NotifiableVariant exclusive,
    [FromQuery] Pagination pagination,
    [FromQuery] Sorting sorting,
    INotificationService notifications
)
    {
        var page = await notifications.GetAllAsync(pagination, sorting,
            n => read ? n.ReadAt != null : n.ReadAt == null,
            n => (n.Descriptor & inclusive) != 0,
            n => (n.Descriptor & exclusive) == 0
        );

        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(e => new NotificationDO(e)));
    }
}


// [ProtectedResource("users", "user:list")]
// [EndpointSummary("Get the currently authenticated user.")]
// [EndpointDescription("When authenticated it's useful to know who you currently are logged in as.")]
// [ProducesResponseType(StatusCodes.Status200OK)]
// [ProducesErrorResponseType(typeof(ProblemDetails))]
// [HttpGet("/users/current"), OutputCache(PolicyName = "1m")]
// public async Task<ActionResult<string>> Current(
//     [FromQuery] Sorting sorting,
//     [FromQuery] Pagination pagination
// )
// {
//     var result = await userService.GetAllAsync(pagination, sorting, u => u.Display == "hello");
//     return Ok("Ok!");
//     // var user = await userService.FindByIdAsync(User.GetSID());
//     // return user is null ? Forbid() : Ok(new UserDO(user));
// }


// [ProtectedResource("users", "user:list")]
// [EndpointSummary("Get all users")]
// [EndpointDescription("Returns all users to you, lets you filter as well.")]
// [ProducesResponseType(StatusCodes.Status200OK)]
// [ProducesErrorResponseType(typeof(ProblemDetails))]
// [HttpGet("/users/"), OutputCache(PolicyName = "1m")]
// public async Task<ActionResult<UserDO>> GetAll(
//     [FromQuery] Sorting sorting,
//     [FromQuery] Pagination pagination,
//     [FromQuery(Name = "filter[display]")] string? displayName
// )
// {
//     log.LogInformation("Getting all users with display name filter: {displayName}", displayName);
//     var page = await userService.GetAllAsync(pagination, sorting,
//         u => u.Display == displayName
//     );

//     page.AppendHeaders(Response.Headers);
//     return Ok(page.Items.Select(e => new UserDO(e)));
// }
