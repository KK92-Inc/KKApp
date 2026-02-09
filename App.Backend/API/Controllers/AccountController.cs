// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Core.Query;
using App.Backend.API.Params;
using App.Backend.Core.Services.Interface;
using App.Backend.Models;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Responses.Entities.Notifications;
using App.Backend.API.Notifications.Registers.Interface;

// ============================================================================

namespace App.Backend.API.Controllers;

/// <summary>
/// Operations for the currently authenticated user.
/// For general user operations (admin/staff), see <see cref="UserController"/>.
/// </summary>
[ApiController]
[Route("account"), Tags("Account")]
[Authorize]
public class AccountController(
    ILogger<AccountController> log,
    IUserService users,
    INotificationService notifications
) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get the current user")]
    [EndpointDescription("Returns the profile of the currently authenticated user.")]
    public async Task<ActionResult<UserDO>> GetCurrent()
    {
        var user = await users.FindByIdAsync(User.GetSID());
        return user is null ? Forbid() : Ok(new UserDO(user));
    }

    [HttpGet("notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get current user's notifications")]
    [EndpointDescription("Retrieve a paginated list of notifications for the authenticated user.")]
    public async Task<ActionResult<IEnumerable<NotificationDO>>> GetNotifications(
        [FromQuery(Name = "filter[read]")] bool read,
        [FromQuery(Name = "filter[variant]")] NotificationMeta inclusive,
        [FromQuery(Name = "filter[not[variant]]")] NotificationMeta exclusive,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken cancellationToken
    )
    {
        var page = await notifications.GetAllAsync(sorting, pagination, cancellationToken,
            n => n.NotifiableId == User.GetSID(),
            n => read ? n.ReadAt != null : n.ReadAt == null
            // n => (n.Descriptor & inclusive) != 0,
            // n => (n.Descriptor & exclusive) == 0
        );

        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(e => new NotificationDO(e)));
    }

    [HttpGet("stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Stream real-time events")]
    [EndpointDescription("Open an SSE connection to receive real-time notifications and events for the authenticated user.")]
    public async Task<IResult> StreamEvents(IBroadcastRegistry registry, CancellationToken token)
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

    [HttpGet("spotlights")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get active spotlights")]
    [EndpointDescription("Retrieve the list of active spotlight notifications for the authenticated user.")]
    public async Task<ActionResult<IEnumerable<SpotlightNotificationDO>>> GetSpotlights()
    {
        // TODO: Implement spotlights service
        return Ok(Array.Empty<SpotlightNotificationDO>());
    }

    [HttpDelete("spotlights/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Dismiss a spotlight")]
    [EndpointDescription("Mark a spotlight notification as dismissed so it won't be shown again.")]
    public async Task<IActionResult> DismissSpotlight(Guid id)
    {
        // TODO: Implement spotlights service
        return NoContent();
    }
}
