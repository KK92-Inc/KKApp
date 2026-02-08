// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Core.Query;
using App.Backend.API.Params;
using App.Backend.Core.Services.Implementation;
using App.Backend.Core.Services.Interface;
using App.Backend.Models;
using Keycloak.AuthServices.Authorization;
using App.Backend.Models.Responses.Entities.Cursus;
using App.Backend.Models.Requests.Cursus;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.API.Controllers;

[Route("cursus")]
[ApiController, Authorize]
public class CursusController(
    ILogger<CursusController> log,
    ICursusService cursusService,
    IGoalService goalService,
    IWorkspaceService workspace
) : Controller
{
    [HttpGet]
    [ProtectedResource("cursus", "cursus:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all cursus")]
    [EndpointDescription("Retrieve a paginated list of all cursus")]
    public async Task<ActionResult<IEnumerable<CursusDO>>> GetAll(
        [FromQuery(Name = "filter[id]")] Guid? id,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[slug]")] string? slug,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        var page = await cursusService.GetAllAsync(sorting, pagination, token,
            id is null ? null : n => n.Id == id,
            string.IsNullOrWhiteSpace(name) ? null : n => EF.Functions.ILike(n.Name, $"%{name}%"),
            string.IsNullOrWhiteSpace(slug) ? null : n => n.Slug == slug
        );

        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(c => new CursusDO(c)));
    }

    [HttpDelete]
    [ProtectedResource("cursus", "cursus:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Delete a cursus")]
    [EndpointDescription("Delete a cursus and its user instances")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var cursus = await cursusService.FindByIdAsync(id);
        if (cursus is null)
            return NotFound();

        await cursusService.DeleteAsync(cursus);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [ProtectedResource("cursus", "cursus:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query a cursus")]
    [EndpointDescription("Retrieve a specific cursus by ID")]
    public async Task<ActionResult<CursusDO>> GetById(Guid id)
    {
        var cursus = await cursusService.FindByIdAsync(id);
        return cursus is null ? NotFound() : Ok(new CursusDO(cursus));
    }

    [HttpGet("{id:guid}/track")]
    // [ProtectedResource("cursus", "cursus:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get cursus track")]
    [EndpointDescription("Retrieve the hierarchical track (goal tree) of a fixed cursus")]
    public async Task<ActionResult<CursusTrackDO>> GetTrack(Guid id, CancellationToken token)
    {
        var cursus = await cursusService.FindByIdAsync(id, token);
        if (cursus is null)
            return NotFound();

        var relations = await cursusService.GetTrackAsync(id, token);
        return Ok(CursusTrackDO.FromRelations(cursus, relations));
    }

    [HttpGet("{id:guid}/track/me")]
    // [ProtectedResource("cursus", "cursus:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user's cursus track state")]
    [EndpointDescription("Retrieve the cursus track with the authenticated user's progress state computed per goal node. Goals the user hasn't started default to Inactive.")]
    public async Task<ActionResult<UserCursusTrackDO>> GetMyTrack(Guid id, CancellationToken token)
    {
        var cursus = await cursusService.FindByIdAsync(id, token);
        if (cursus is null)
            return NotFound();

        var userId = User.GetSID();
        var relations = await cursusService.GetTrackAsync(id, token);
        var userStates = await cursusService.GetUserTrackStatesAsync(id, userId, token);

        return Ok(UserCursusTrackDO.FromRelations(cursus, userId, relations, userStates));
    }

    [HttpPost("{id:guid}/track")]
    // [ProtectedResource("cursus", "cursus:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Set cursus track")]
    [EndpointDescription("Set or replace the hierarchical track (goal tree) for a fixed cursus. This is a full replacement of the existing track.")]
    public async Task<ActionResult<CursusTrackDO>> SetTrack(
        Guid id,
        [FromBody] PostCursusTrackRequestDTO dto,
        CancellationToken token
    )
    {
        var cursus = await cursusService.FindByIdAsync(id, token);
        if (cursus is null)
            return NotFound();

        if (cursus.Variant != CursusVariant.Static)
            return Problem(
                statusCode: 400,
                detail: "Track can only be set on Fixed cursus types. Dynamic cursus do not have predefined tracks."
            );

        // Validate all referenced goal IDs exist
        var goalIds = dto.Nodes.Select(n => n.GoalId).Distinct().ToList();
        if (!await goalService.ExistsAsync(goalIds, token))
            return Problem(statusCode: 400, detail: "One or more goal IDs are invalid");

        // Validate parent references point to goals within this track
        var goalIdSet = goalIds.ToHashSet();
        foreach (var node in dto.Nodes)
        {
            if (node.ParentGoalId.HasValue && !goalIdSet.Contains(node.ParentGoalId.Value))
                return Problem(
                    statusCode: 400,
                    detail: $"Parent goal ID {node.ParentGoalId} is not part of this track"
                );
        }

        // Validate no duplicate goal IDs
        if (goalIds.Count != dto.Nodes.Count)
            return Problem(statusCode: 400, detail: "Duplicate goal IDs are not allowed in a track");

        // Map DTO nodes to CursusGoal entities
        var nodes = dto.Nodes.Select(n => new CursusGoal
        {
            CursusId = id,
            GoalId = n.GoalId,
            Position = n.Position,
            ParentGoalId = n.ParentGoalId
        });

        var created = await cursusService.SetTrackAsync(id, nodes, token);

        // Reload with Goal navigation properties for the response
        var track = await cursusService.GetTrackAsync(id, token);
        return Ok(CursusTrackDO.FromRelations(cursus, track));
    }
}
//     [HttpGet("slug/{slug}")]
//     [ProtectedResource("cursus", "cursus:read")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status403Forbidden)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [ProducesErrorResponseType(typeof(ProblemDetails))]
//     [EndpointSummary("Query a cursus by slug")]
//     [EndpointDescription("Retrieve a specific cursus by slug")]
//     public async Task<ActionResult<CursusDO>> GetBySlug(string slug)
//     {
//         var cursus = await cursusService.FindBySlugAsync(slug);
//         return cursus is null ? NotFound() : Ok(new CursusDO(cursus));
//     }

//     [HttpPatch("{id:guid}")]
//     [ProtectedResource("cursus", "cursus:write")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status403Forbidden)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [ProducesResponseType(StatusCodes.Status400BadRequest)]
//     [ProducesErrorResponseType(typeof(ProblemDetails))]
//     [EndpointSummary("Update a cursus")]
//     [EndpointDescription("Update cursus information and track data")]
//     public async Task<ActionResult<CursusDO>> Update(Guid id, [FromBody] UpdateCursusRequestDTO request)
//     {
//         var cursus = await cursusService.UpdateCursusAsync(id, request);
//         return cursus is null ? NotFound() : Ok(new CursusDO(cursus));
//     }

//     [HttpGet("{id:guid}/goals")]
//     [ProtectedResource("cursus", "cursus:read")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status403Forbidden)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [ProducesErrorResponseType(typeof(ProblemDetails))]
//     [EndpointSummary("Get cursus goals")]
//     [EndpointDescription("Retrieve goals associated with a cursus from track data")]
//     public async Task<ActionResult> GetCursusGoals(Guid id)
//     {
//         var goalIds = await cursusService.GetCursusGoalsAsync(id);
//         return Ok(goalIds);
//     }

//     [HttpPost("{id:guid}/subscribe")]
//     [ProducesResponseType(StatusCodes.Status201Created)]
//     [ProducesResponseType(StatusCodes.Status403Forbidden)]
//     [ProducesResponseType(StatusCodes.Status400BadRequest)]
//     [ProducesErrorResponseType(typeof(ProblemDetails))]
//     [EndpointSummary("Subscribe to cursus")]
//     [EndpointDescription("Subscribe current user to a cursus")]
//     public async Task<ActionResult> SubscribeToCursus(Guid id, [FromBody] SubscribeToCursusRequestDTO? request = null)
//     {
//         var userId = User.GetSID();
//         var subscribeRequest = request ?? new SubscribeToCursusRequestDTO { CursusId = id };

//         var userCursus = await cursusService.SubscribeToCursusAsync(userId, subscribeRequest);
//         return CreatedAtAction(nameof(GetUserCursi), new { }, new UserCursusDO(userCursus));
//     }

//     [HttpGet("my-cursus")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status403Forbidden)]
//     [ProducesErrorResponseType(typeof(ProblemDetails))]
//     [EndpointSummary("Get user cursus")]
//     [EndpointDescription("Retrieve cursus subscribed by current user")]
//     public async Task<ActionResult> GetUserCursi()
//     {
//         var userId = User.GetSID();
//         var userCursi = await cursusService.GetUserCursiAsync(userId);
//         return Ok(userCursi.Select(uc => new UserCursusDO(uc)));
//     }

//     [HttpDelete("user-cursus/{id:guid}")]
//     [ProducesResponseType(StatusCodes.Status204NoContent)]
//     [ProducesResponseType(StatusCodes.Status403Forbidden)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [ProducesErrorResponseType(typeof(ProblemDetails))]
//     [EndpointSummary("Unsubscribe from cursus")]
//     [EndpointDescription("Unsubscribe current user from a cursus")]
//     public async Task<IActionResult> UnsubscribeFromCursus(Guid id)
//     {
//         var userId = User.GetSID();
//         var success = await cursusService.UnsubscribeFromCursusAsync(id, userId);
//         return success ? NoContent() : NotFound();
//     }

//     [HttpPost("{id:guid}/collaborators")]
//     [ProducesResponseType(StatusCodes.Status201Created)]
//     [ProducesResponseType(StatusCodes.Status403Forbidden)]
//     [ProducesResponseType(StatusCodes.Status400BadRequest)]
//     [ProducesErrorResponseType(typeof(ProblemDetails))]
//     [EndpointSummary("Add cursus collaborator")]
//     [EndpointDescription("Add a user as collaborator to a cursus")]
//     public async Task<ActionResult> AddCollaborator(Guid id, [FromBody] AddCollaboratorRequestDTO request)
//     {
//         var collaborator = await subscriptions.AddCursusCollaboratorAsync(request.UserId, id);
//         return CreatedAtAction(nameof(GetCollaborators), new { id = id }, new CursusCollaboratorDO(collaborator));
//     }

//     [HttpGet("{id:guid}/collaborators")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status403Forbidden)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [ProducesErrorResponseType(typeof(ProblemDetails))]
//     [EndpointSummary("Get cursus collaborators")]
//     [EndpointDescription("Retrieve collaborators for a cursus")]
//     public async Task<ActionResult> GetCollaborators(Guid id)
//     {
//         var collaborators = await subscriptions.GetCursusCollaboratorsAsync(id);
//         return Ok(collaborators.Select(c => new CursusCollaboratorDO(c)));
//     }

//     [HttpDelete("{id:guid}/collaborators/{userId:guid}")]
//     [ProducesResponseType(StatusCodes.Status204NoContent)]
//     [ProducesResponseType(StatusCodes.Status403Forbidden)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [ProducesErrorResponseType(typeof(ProblemDetails))]
//     [EndpointSummary("Remove cursus collaborator")]
//     [EndpointDescription("Remove a user as collaborator from a cursus")]
//     public async Task<IActionResult> RemoveCollaborator(Guid id, Guid userId)
//     {
//         var success = await subscriptions.RemoveCursusCollaboratorAsync(userId, id);
//         return success ? NoContent() : NotFound();
//     }
// }
