// ============================================================================
// Copyright (c) 2024 - W2Wizard.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;

// ============================================================================

namespace backend.api.root.Controllers;

[Route("user")]
[ApiController]
public class UserController(ILogger<UserController> log) : Controller
{
    [HttpGet("/users/current")]
    [EndpointSummary("Get the currently authenticated user.")]
    [EndpointDescription("When authenticated it's useful to know who you currently are logged in as.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> CurrentUserAsync()
    {
        return Ok("Ok!");
        // var user = await userService.FindByIdAsync(User.GetSID());
        // return user is null ? Forbid() : Ok(new UserDO(user));
    }
}
