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
using App.Backend.Models.Responses.Entities.Goals;
using App.Backend.Domain.Entities;

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

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(c => new CursusDO(c)));
    }

    [Tags("Workspace")]
    [HttpDelete("{id:guid}")]
    [ProtectedResource("cursus", "cursus:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Delete a cursus")]
    [EndpointDescription("Delete a cursus and its user instances")]
    public async Task<IActionResult> Delete(Guid id)
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get cursus track")]
    [EndpointDescription("Retrieve the hierarchical goal tree of a static cursus.")]
    public async Task<ActionResult<CursusTrackDO>> GetTrack(Guid id, CancellationToken token)
    {
        var cursus = await cursusService.FindByIdAsync(id, token);
        if (cursus is null) return NotFound();

        var track = await cursusService.GetTrackAsync(id, token);
        return Ok(cursusService.AssembleTrack(cursus, track));
    }

    [HttpPost("{id:guid}/track")]
    [ProtectedResource("cursus", "cursus:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Replace cursus track")]
    [EndpointDescription("Fully replaces the hierarchical goal track for a static cursus.")]
    public async Task<ActionResult<CursusTrackDO>> ReplaceTrack(
        Guid id,
        [FromBody] PostCursusTrackRequestDTO dto,
        CancellationToken token)
    {
        var cursus = await cursusService.FindByIdAsync(id, token);
        if (cursus is null) return NotFound();

        if (cursus.Variant != CursusVariant.Static)
            return UnprocessableEntity("Track can only be set on static cursi");

        var error = await cursusService.ValidateTrackAsync(
            dto.Nodes.Select(n => (n.GoalId, n.ParentId, n.Group)).ToList(), token);

        if (error is not null)
            return UnprocessableEntity(error);

        var nodes = dto.Nodes.Select(n => new CursusGoal
        {
            CursusId = id,
            GoalId = n.GoalId,
            ParentGoalId = n.ParentId,
            ChoiceGroup = n.Group
        });

        var track = await cursusService.ReplaceTrackAsync(id, nodes, token);
        return Ok(cursusService.AssembleTrack(cursus, track));
    }
}
