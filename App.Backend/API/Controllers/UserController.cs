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
using System.ComponentModel;
using Keycloak.AuthServices.Sdk.Admin.Models;
using Keycloak.AuthServices.Sdk.Admin;
using App.Backend.Domain.Enums;
using Wolverine;
using App.Backend.API.Notifications.Variants;
using Keycloak.AuthServices.Sdk.Kiota.Admin;
using App.Backend.API.Utils;
using Keycloak.AuthServices.Sdk.Protection;
using Keycloak.AuthServices.Authorization.Requirements;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.API.Controllers;

/// <summary>
/// General user operations (admin/staff).
/// For authenticated-user-specific operations, see <see cref="AccountController"/>.
/// </summary>
[ApiController]
[Route("users"), Tags("Users")]
[Authorize]
public class UserController(
    IUserService users,
    IMessageBus bus,
    IAuthorizationService auth
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
            string.IsNullOrWhiteSpace(display) ? null : u => u.Display != null && EF.Functions.ILike(u.Display, $"%{display}%")
        );
        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(u => new UserDO(u)));
    }

    [HttpGet("eligible")]
    [ProtectedResource("users", "users:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all eligible users")]
    [EndpointDescription(@"
Retrieve a paginated list of all users who are elligible for a certain entity.
For example this could be finding all users who can subscribe to a project, goal or cursus.
Another one would be finding users eligible to be invited to a user project.

In regards to rubrics it will evaluate if user is elligible to conduct a review with that rubric.
    ")]
    public async Task<ActionResult<IEnumerable<UserDO>>> GetEligible(
        [FromQuery(Name = "filter[login]")] string? login,
        [FromQuery(Name = "filter[display]")] string? display,
        [FromQuery(Name = "filter[user_id]")] Guid? userId,
        [FromQuery(Name = "type[id]"), Required, Description("The ID of the entity to query")] Guid id,
        [FromQuery(Name = "type[entity]"), Required, Description("Defaults to Project")] EntityType type,
        [FromServices] IEligibilityService service,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken token
    )
    {
        if (id == Guid.Empty)
            return BadRequest(new ProblemDetails() { Title = "type[id] and type[entity] are required"});

        var page = await service.GetAllEligibleAsync(id, type, sorting, pagination, token,
            u => !userId.HasValue || u.Id == userId.Value,
            string.IsNullOrWhiteSpace(login) ? null : u => EF.Functions.ILike(u.Login, $"%{login}%"),
            string.IsNullOrWhiteSpace(display) ? null : u => EF.Functions.ILike(u.Display, $"%{display}%")
        );

        page.AppendHeaders(Response.Headers);
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


    [HttpPost]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Create a user")]
    [EndpointDescription("Provision a new user and create a Keycloak account for them.")]
    public async Task<ActionResult<UserDO>> CreateUser([FromBody] PostUserRequestDTO request, CancellationToken token)
    {
        var existing = await users.FindByLoginAsync(request.Login, token);
        if (existing is not null) return Conflict();

        var (user, password) = await users.CreateUserAsync(new()
        {
            Login = request.Login,
            Display = request.Login,
            Details = new()
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
            },
        }, token);

        await bus.PublishAsync(new WelcomeUserNotification(user!));
        Response.Headers.TryAdd("X-Password", password);
        return Ok(new UserDO(user));
    }

    [HttpPatch("{userId:guid}")]
    [RequireScope("user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get a user by ID")]
    [EndpointDescription("Retrieve a specific user by their unique identifier")]
    public async Task<ActionResult<UserDO>> Update(Guid userId, [FromBody] PatchUserRequestDTO body, CancellationToken token)
    {
        var self = userId == User.GetSID();
        var staff = await auth.AuthorizeAsync(User, "staff");
        if (!staff.Succeeded && !self)
            return Forbid();

        // Depending on who we're updating, inquire about the permission first.
        var requirement = new DecisionRequirement("users", self ? "user:profile:write" : "users:write");
        var result = await auth.AuthorizeAsync(User, null, requirement);
        if (!result.Succeeded) return Forbid();

        var user = await users.FindByIdAsync(userId, token);
        if (user is null) return NotFound();

        // Update root properties if provided
        user.AvatarUrl = body.AvatarUrl ?? user.AvatarUrl;
        user.Display = body.DisplayName ?? user.Display;

        if (body.Details is not null)
        {
            user.Details ??= new();
            var d = body.Details;
            user.Details.FirstName = d.FirstName ?? user.Details.FirstName;
            user.Details.LastName = d.LastName ?? user.Details.LastName;
            user.Details.Markdown = d.Markdown ?? user.Details.Markdown;
            user.Details.WebsiteUrl = d.WebsiteUrl ?? user.Details.WebsiteUrl;
            user.Details.LinkedinUrl = d.LinkedinUrl ?? user.Details.LinkedinUrl;
            user.Details.RedditUrl = d.RedditUrl ?? user.Details.RedditUrl;
            user.Details.GithubUrl = d.GithubUrl ?? user.Details.GithubUrl;
        }

        await users.UpdateAsync(user, token);
        return new UserDO(user);
    }
}
