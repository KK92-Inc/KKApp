// ============================================================================
// Copyright (c) 2024 - W2Wizard.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;
using Backend.API.Core.Query;
using Backend.API.Root.Params;
using Backend.API.Core.Services.Implementation;
using Backend.API.Domain;
using Backend.API.Core.Services.Interface;
using Backend.API.Models.Entities;

// ============================================================================

namespace Backend.API.Root.Controllers;

[Route("user")]
[ApiController]
public class UserController(
    ILogger<UserController> log,
    IUserService userService
) : Controller
{
    [EndpointSummary("Get the currently authenticated user.")]
    [EndpointDescription("When authenticated it's useful to know who you currently are logged in as.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [HttpGet("/users/current"), OutputCache(PolicyName = "1m")]
    public async Task<ActionResult<string>> Current(
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination
    )
    {
        var result = await userService.GetAllAsync(pagination, sorting, u => u.Display == "hello");
        return Ok("Ok!");
        // var user = await userService.FindByIdAsync(User.GetSID());
        // return user is null ? Forbid() : Ok(new UserDO(user));
    }


    [EndpointSummary("Get all users")]
    [EndpointDescription("Returns all users to you, lets you filter as well.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [HttpGet("/users/"), OutputCache(PolicyName = "1m")]
    public async Task<ActionResult<UserDO>> GetAll(
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        [FromQuery(Name = "filter[display]")] string? displayName
    )
    {
        var page = await userService.GetAllAsync(pagination, sorting,
            u => u.Display == displayName
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(e => new UserDO(e)));
    }
}
