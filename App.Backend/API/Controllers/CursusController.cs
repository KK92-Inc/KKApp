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

    [HttpGet("{id:guid}/track/user/{userId:guid}")]
    [ProtectedResource("cursus", "cursus:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get user's cursus track state")]
    [EndpointDescription("Retrieve the cursus track with the authenticated user's progress state computed per goal node. Goals the user hasn't started default to Inactive.")]
    public async Task<ActionResult<UserCursusTrackDO>> GetMyTrack(Guid id, Guid userId, CancellationToken token)
    {
        var cursus = await cursusService.FindByIdAsync(id, token);
        if (cursus is null)
            return NotFound();

        var relations = await cursusService.GetTrackAsync(id, token);
        var userStates = await cursusService.GetTrackForUserAsync(id, userId, token);

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
        // TODO: Support Partial once I have time
        if (cursus.Variant is not CursusVariant.Static)
            return UnprocessableEntity("Track can only be set on static cursi");

        // All referenced goal IDs exist...
        var goalIds = dto.Nodes.Select(n => n.GoalId).Distinct().ToList();
        if (!await goalService.ExistsAsync(goalIds, token))
            return UnprocessableEntity("One or more goal IDs are invalid");

        // Parent references point to goals within this track...
        var goalIdSet = goalIds.ToHashSet();
        foreach (var node in dto.Nodes)
            if (node.ParentId is not null && !goalIdSet.Contains(node.ParentId.Value))
                return UnprocessableEntity($"Parent goal ID {node.ParentId} is not part of this track");

        // No duplicate goal IDs
        if (goalIds.Count != dto.Nodes.Count)
            return UnprocessableEntity("Duplicate goals are not allowed in a track");

        // Groups: all members of a group must share the same parent
        var choices = dto.Nodes.Where(n => n.Group.HasValue);
        foreach (var group in choices.GroupBy(n => n.Group!.Value))
        {
            var parents = group.Select(n => n.ParentId).Distinct().ToList();
            if (parents.Count > 1)
                return UnprocessableEntity($"All goals in choice group {group.Key} must share the same parent");
        }

        var nodes = dto.Nodes.Select(n => new CursusGoal
        {
            CursusId = id,
            GoalId = n.GoalId,
            ParentGoalId = n.ParentId,
            ChoiceGroup = n.Group
        });

        var created = await cursusService.SetTrackAsync(id, nodes, token);
        var track = await cursusService.GetTrackAsync(id, token);
        return Ok(CursusTrackDO.FromRelations(cursus, track));
    }
}
