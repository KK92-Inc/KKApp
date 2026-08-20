// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.API.Params;
using App.Backend.Core.Services.Interface;
using Keycloak.AuthServices.Authorization;
using App.Backend.Models.Responses.Entities.Cursus;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using App.Backend.Models.Responses.Entities.Goals;
using App.Backend.Domain.Entities;
using Wolverine;
using App.Backend.API.Utils;
using App.Backend.Models.Requests.Cursus;

// ============================================================================

namespace App.Backend.API.Controllers;

[Route("cursus")]
[ApiController, Authorize]
public class CursusController(IAuthorizationService auth, ICursusService service, IMemberService members) : Controller
{
    [HttpGet]
    [RequireScope("workspace")]
    [ProtectedResource("cursus", "cursus:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all cursus")]
    [EndpointDescription("Retrieve a paginated list of all cursus")]
    public async Task<ActionResult<IEnumerable<CursusDO>>> GetAll(
        [FromQuery(Name = "filter[id]")] Guid? id,
        [FromQuery(Name = "filter[workspace_id]")] Guid? workspace,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[slug]")] string? slug,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        var page = await service.GetAllAsync(sorting, pagination, token,
            id is null ? null : n => n.Id == id,
            workspace is null ? null : n => n.WorkspaceId == workspace,
            string.IsNullOrWhiteSpace(name) ? null : n => EF.Functions.ILike(n.Name, $"%{name}%"),
            string.IsNullOrWhiteSpace(slug) ? null : n => n.Slug == slug
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(c => new CursusDO(c)));
    }

    [Tags("Workspace")]
    [HttpDelete("{id:guid}")]
    [RequireScope("workspace")]
    [ProtectedResource("cursus", "cursus:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Delete a cursus")]
    [EndpointDescription("Delete a cursus and its user instances")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        var cursus = await service.FindByIdAsync(id, token);
        if (cursus is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await members.FindByEntityAndUserId(cursus.WorkspaceId, User.GetSID(), token);
            if (member is null) return Forbid();
        }

        await service.DeleteAsync(cursus, token);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [RequireScope("workspace")]
    [ProtectedResource("cursus", "cursus:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query a cursus")]
    [EndpointDescription("Retrieve a specific cursus by ID")]
    public async Task<ActionResult<CursusDO>> GetById(Guid id, CancellationToken token)
    {
        var cursus = await service.FindByIdAsync(id, token);
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
        var cursus = await service.FindByIdAsync(id, token);
        if (cursus is null) return NotFound();
        var track = await service.GetTrackAsync(id, token);
        return Ok(AssembleTrack(cursus, track));
    }

    [HttpPost("{id:guid}/track")]
    [ProtectedResource("cursus", "cursus:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Replace cursus track")]
    [EndpointDescription("Fully replaces the hierarchical goal track for a static cursus.")]
    public async Task<ActionResult<CursusTrackDO>> SetTrack(Guid id, [FromBody] PostCursusTrackRequestDTO body, CancellationToken token)
    {
        var cursus = await service.FindByIdAsync(id, token);
        if (cursus is null) return NotFound();

        if (cursus.Variant is CursusVariant.Static)
            return UnprocessableEntity(new ProblemDetails() { Title = "Track can only be set on static cursi" });

        await service.ValidateTrackAsync([.. body.Nodes.Select(n => (n.GoalId, n.ParentId, n.Group))], token);
        var nodes = body.Nodes.Select(n => new CursusGoal
        {
            CursusId = id,
            GoalId = n.GoalId,
            ParentGoalId = n.ParentId,
            ChoiceGroup = n.Group
        });

        var track = await service.SetTrackAsync(id, nodes, token);
        return Ok(AssembleTrack(cursus, track));
    }

    private static CursusTrackDO AssembleTrack(Cursus cursus, IReadOnlyList<CursusGoal> goals)
    {
        var entries = goals.Select(g => (
            Node: new Models.Responses.Entities.Cursus.CursusTrackNodeDO { Goal = new GoalLightDO(g.Goal), ChoiceGroup = g.ChoiceGroup },
            g.GoalId,
            g.ParentGoalId
        )).ToList();

        var byId = entries.ToDictionary(e => e.GoalId, e => e.Node);
        var roots = new List<Models.Responses.Entities.Cursus.CursusTrackNodeDO>();

        foreach (var (node, _, parentId) in entries)
        {
            if (parentId is not null && byId.TryGetValue(parentId.Value, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        return new CursusTrackDO
        {
            CursusId = cursus.Id,
            Variant = cursus.Variant,
            CompletionMode = cursus.CompletionMode,
            Nodes = roots
        };
    }
}
