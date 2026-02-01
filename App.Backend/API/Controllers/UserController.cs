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
using Microsoft.AspNetCore.Http;
using App.Backend.Core.Services.Implementation;
using System.Net.ServerSentEvents;
using App.Backend.Domain;
using App.Backend.Core.Services.Interface;
using App.Backend.Models;
using Keycloak.AuthServices.Authorization;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Responses.Entities.Notifications;
using App.Backend.Models.Requests.Users;
using System.Threading.Channels;
using App.Backend.API.Notifications.Channels;
using App.Backend.API.Bus.Messages;
using System.Runtime.CompilerServices;
using App.Backend.API.Notifications.Registers.Interface;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("users")]
// [ProtectedResource("users"), Authorize]
public class UserController(
    ILogger<UserController> log,
    IUserService users,
    INotificationService notifications
) : Controller
{
    [HttpGet("/users/current")]
    // [ProtectedResource("users", "users:read")]
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
    // [ProtectedResource("users", "users:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get the currently authenticated user.")]
    [EndpointDescription("When authenticated it's useful to know who you currently are logged in as.")]
    public async Task<ActionResult<IEnumerable<NotificationDO>>> CurrentNotifications(
        [FromQuery(Name = "filter[read]")] bool read,
        [FromQuery(Name = "filter[variant]")] NotificationMeta inclusive,
        [FromQuery(Name = "filter[not[variant]]")] NotificationMeta exclusive,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken cancellationToken
    )
    {
        // await notifications.CreateAsync(new ()
        // {
        //     NotifiableId = User.GetSID(),
        //     Type = nameof(MessageDO)
        // });

        var page = await notifications.GetAllAsync(sorting, pagination, cancellationToken,
            n => read ? n.ReadAt != null : n.ReadAt == null
            // n => (n.Descriptor & inclusive) != 0,
            // n => (n.Descriptor & exclusive) == 0
        );

        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(e => new NotificationDO(e)));
    }

    [HttpGet("/users/current/stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get the currently authenticated user.")]
    [EndpointDescription("When authenticated it's useful to know who you currently are logged in as.")]
    public async Task<IResult> StreamData(IBroadcastRegistry registry, CancellationToken token)
    {
        return TypedResults.ServerSentEvents(GetChannel(User.GetSID(), token));

        async IAsyncEnumerable<SseItem<object>> GetChannel(Guid id, [EnumeratorCancellation] CancellationToken token)
        {
            // Initial heartbeat/sync
            yield return new SseItem<object>("ping", "heartbeat");
            await foreach (var message in registry.SubscribeAsync(id, token))
            {
                yield return new SseItem<object>(message.Payload, message.Event)
                {
                    ReconnectionInterval = TimeSpan.FromSeconds(30)
                };
            }
        }
    }

    [HttpGet("/users/current/spotlights")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get the currently authenticated user.")]
    [EndpointDescription("When authenticated it's useful to know who you currently are logged in as.")]
    public async Task<ActionResult<SpotlightNotificationDO>> CurrentSpotlights()
    {
        // TODO: Implement spotlights service and remove this placeholder
        return Ok();
    }

    [HttpDelete("/users/current/spotlights/{id:guid}")]
    [EndpointSummary("Dismiss a spotlighted event")]
    [EndpointDescription("If users dismiss a spotlight event, they won't shown in the future.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DismissSpotlight(Guid id)
    {
        // TODO: Implement spotlights service and remove this placeholder
        return Ok();
    }

    [HttpGet]
    [ProtectedResource("users", "users:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all users")]
    [EndpointDescription("Retrieve a paginated list of all users")]
    public async Task<ActionResult> GetAll(
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting
    )
    {
        var page = await users.GetAllAsync(sorting, pagination);
        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(u => new UserDO(u)));
    }

    [HttpPost]
    [ProtectedResource("users", "users:write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Create a new user")]
    [EndpointDescription("Create a new user with optional details")]
    public async Task<ActionResult<UserDO>> Create([FromBody] PostUserRequestDTO request)
    {
        var user = await users.CreateAsync(new()
        {
            Login = request.Login,
            Display = request.DisplayName,
            AvatarUrl = request.AvatarUrl,
            Details = null
        });
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new UserDO(user));
    }

    [HttpDelete("{id:guid}")]
    [ProtectedResource("users", "users:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Delete a user")]
    [EndpointDescription("Delete a user and their details")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null)
            return NotFound();
        await users.DeleteAsync(user);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [ProtectedResource("users", "users:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query a user")]
    [EndpointDescription("Retrieve a specific user by ID")]
    public async Task<ActionResult<UserDO>> GetById(Guid id)
    {
        var user = await users.FindByIdAsync(id);
        return user is null ? NotFound() : Ok(new UserDO(user));
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Update a user")]
    [EndpointDescription("Update user and details via a single DTO")]
    public async Task<ActionResult<UserDO>> Update(Guid id, [FromBody] PatchUserRequestDTO request)
    {
        var currentUserId = User.GetSID();

        // Authorization check: user can only update their own profile unless they are staff
        if (User.GetSID() != id && !User.Claims.Any(c => c.Type == "role" && c.Value == "staff"))
            return Forbid();

        return Ok();
        // var user = await users.UpdateUserAsync(id, request);
        // return user is null ? NotFound() : Ok(new UserDO(user));
    }

    [HttpPost("{id:guid}/projects/{projectId:guid}/subscribe")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Subscribe to project")]
    [EndpointDescription("Create a user project instance for current user")]
    public async Task<ActionResult> SubscribeToProject(Guid id, Guid projectId)
    {
        return Created();
    }

    [HttpDelete("{id:guid}/projects/{projectId:guid}/unsubscribe")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user projects")]
    [EndpointDescription("Retrieve project instances subscribed by current user")]
    public async Task<ActionResult> UnsubscribeFromProject(Guid id, Guid projectId)
    {


        return NoContent();
    }
}
