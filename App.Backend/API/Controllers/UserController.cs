// ============================================================================
// Copyright (c) 2024 - W2Wizard.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Core.Query;
using App.Backend.API.Params;
using App.Backend.Core.Services.Interface;
using App.Backend.Models;
using Keycloak.AuthServices.Authorization;
using App.Backend.Models.Responses.Entities;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.API.Controllers;

/// <summary>
/// General user operations (admin/staff).
/// For authenticated-user-specific operations, see <see cref="AccountController"/>.
/// </summary>
[ApiController]
[Route("users"), Tags("Users")]
[ProtectedResource("users"), Authorize]
public class UserController(
    ILogger<UserController> log,
    IUserService users
) : Controller
{
    [HttpGet]
    [ProtectedResource("users", "users:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all users")]
    [EndpointDescription("Retrieve a paginated list of all users")]
    public async Task<ActionResult<IEnumerable<UserDO>>> GetAll(
        [FromQuery(Name = "filter[login]")] string? login,
        [FromQuery(Name = "filter[display]")] string? display,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken token
    )
    {
        var page = await users.GetAllAsync(sorting, pagination, token,
            string.IsNullOrWhiteSpace(login) ? null : u => EF.Functions.ILike(u.Login, $"%{login}%"),
            string.IsNullOrWhiteSpace(display) ? null : u => EF.Functions.ILike(u.Display, $"%{display}%")
        );
        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(u => new UserDO(u)));
    }

    [HttpGet("{userId:guid}")]
    [ProtectedResource("users", "users:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get a user by ID")]
    [EndpointDescription("Retrieve a specific user by their unique identifier")]
    public async Task<ActionResult<UserDO>> GetById(Guid userId, CancellationToken token)
    {
        var user = await users.FindByIdAsync(userId, token);
        return user is null ? NotFound() : Ok(new UserDO(user));
    }
}
