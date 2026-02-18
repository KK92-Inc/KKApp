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
using App.Backend.Models.Requests.SshKeys;
using App.Backend.Domain.Entities.Users;

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
    INotificationService notifications,
    ISpotlightService spotlights
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
        [FromQuery(Name = "filter[read]")] bool? read,
        [FromQuery(Name = "filter[variant]")] NotificationMeta? inclusive,
        [FromQuery(Name = "filter[not[variant]]")] NotificationMeta? exclusive,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken cancellationToken
    )
    {
        var page = await notifications.GetAllAsync(sorting, pagination, cancellationToken,
            n => n.NotifiableId == User.GetSID(),
            read is null ? null : n => n.ReadAt.HasValue,
            inclusive is null ? null : n => (n.Descriptor & inclusive) != 0,
            exclusive is null ? null : n => (n.Descriptor & exclusive) == 0
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
    public async Task<ActionResult<IEnumerable<SpotlightNotificationDO>>> GetSpotlights(
        [FromQuery(Name = "filter[id]")] Guid? id,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken cancellationToken
    )
    {
        var page = await spotlights.GetAllAsync(sorting, pagination, cancellationToken,
            id is null ? null : s => s.Id == id.Value
        );

        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(s => new SpotlightNotificationDO(s)));
    }

    [HttpDelete("spotlights/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Dismiss a spotlight")]
    [EndpointDescription("Mark a spotlight notification as dismissed so it won't be shown again.")]
    public async Task<IActionResult> DismissSpotlight(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement spotlights service
        var spotlight = await spotlights.FindByIdAsync(id, cancellationToken);
        if (spotlight is null) return NotFound();
        await spotlights.Dismiss(spotlight, User.GetSID(), cancellationToken);
        return NoContent();
    }

    [HttpGet("ssh-keys")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List SSH keys")]
    [EndpointDescription("Retrieve the SSH keys associated with the authenticated user.")]
    public async Task<ActionResult<IEnumerable<SshKeyResponseDO>>> GetSshKeys()
    {
        var user = await users.FindByIdAsync(User.GetSID());
        if (user is null) return Forbid();
        return Ok(user.SshKeys.Select(k => new SshKeyResponseDO(k)));
    }

    [HttpDelete("ssh-keys/{fingerprint}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Remove an SSH key")]
    [EndpointDescription("Delete an SSH key by its fingerprint for the authenticated user.")]
    public async Task<IActionResult> RemoveSshKey(string fingerprint)
    {
        var user = await users.FindByIdAsync(User.GetSID());
        if (user is null) return Forbid();

        var key = user.SshKeys.FirstOrDefault(k => k.Fingerprint == fingerprint);
        if (key is null) return NotFound();

        user.SshKeys.Remove(key);
        await users.UpdateAsync(user);
        return NoContent();
    }

    [HttpPost("ssh-keys")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Add an SSH key")]
    [EndpointDescription("Add a new SSH public key for the authenticated user.")]
    public async Task<ActionResult> AddSshKey([FromBody] PostSshKeyRequestDTO data)
    {
        var user = await users.FindByIdAsync(User.GetSID());
        if (user is null) return Forbid();

        var parts = data.PublicKey.Trim().Split(' ', 3);
        if (parts.Length < 2)
            return Problem("Invalid SSH public key format.", statusCode: 422);

        user.SshKeys.Add(new()
        {
            Title = data.Title,
            KeyType = parts[0],
            KeyBlob = parts[1],
        });

        await users.UpdateAsync(user);
        return NoContent();
    }
}
