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

// ============================================================================

class User : BaseEntity
{
	public string Username { get; set; } = string.Empty;
}

namespace backend.api.root.Controllers;

[Route("user")]
[ApiController]
public class UserController(ILogger<UserController> log) : Controller
{
    [HttpGet("/users/current")]
    [EndpointSummary("Get the currently authenticated user.")]
    [EndpointDescription("When authenticated it's useful to know who you currently are logged in as.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> Current()
    {
        return Ok("Ok!");
        // var user = await userService.FindByIdAsync(User.GetSID());
        // return user is null ? Forbid() : Ok(new UserDO(user));
    }

	public async Task<ActionResult> Users(
		PaginationParams Pagination,
		[FromQuery(Name = "filter[username]")] string? username
	)
	{
		
		var tempshit = new UserService<User>();
		var items = await tempshit.GetAllAsync(
			Filter.With<User>(username, u => u.Username == username)
			Filter.With<User>(username, u => u.Username == username)
			Filter.With<User>(username, u => u.Username == username)
		);
		return Ok(items);
	}
}
