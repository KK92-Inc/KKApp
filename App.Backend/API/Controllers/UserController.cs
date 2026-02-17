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
using App.Backend.Models.Requests.Users;
using App.Backend.Domain.Entities.Users;

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

    [HttpPatch("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get a user by ID")]
    [EndpointDescription("Retrieve a specific user by their unique identifier")]
    public async Task<ActionResult<UserDO>> Update(Guid userId, [FromBody] PatchUserRequestDTO body, CancellationToken token)
    {
        // if (userId != User.GetSID() && !User.IsInRole("staff"))
        //     return Forbid();

        var user = await users.FindByIdAsync(userId, token);
        if (user is null) return NotFound();

        // Update root properties if provided
        if (body.AvatarUrl is not null) user.AvatarUrl = body.AvatarUrl;
        if (body.DisplayName is not null) user.Display = body.DisplayName;

        // Update details if provided
        if (body.Details is not null)
        {
            // Ensure the user has a Details object to update
            user.Details ??= new ();
            if (body.Details.FirstName is not null) user.Details.FirstName = body.Details.FirstName;
            if (body.Details.LastName is not null) user.Details.LastName = body.Details.LastName;
            if (body.Details.Markdown is not null) user.Details.Markdown = body.Details.Markdown;
            if (body.Details.WebsiteUrl is not null) user.Details.WebsiteUrl = body.Details.WebsiteUrl;
            if (body.Details.LinkedinUrl is not null) user.Details.LinkedinUrl = body.Details.LinkedinUrl;
            if (body.Details.RedditUrl is not null) user.Details.RedditUrl = body.Details.RedditUrl;
            if (body.Details.GithubUrl is not null) user.Details.GithubUrl = body.Details.GithubUrl;
        }

        await users.UpdateAsync(user, token);
        return new UserDO(user);
    }
}
