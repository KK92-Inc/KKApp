// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.API.Params;
using App.Backend.Core.Services.Interface;
using Keycloak.AuthServices.Authorization;
using Microsoft.EntityFrameworkCore;
using App.Backend.Models.Responses.Entities.Reviews;
using App.Backend.Models.Requests.Rubrics;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.API.Utils;
using App.Backend.Domain.Enums;
using System.Linq.Expressions;
using App.Backend.Database;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("rubrics")]
[ProtectedResource("rubrics")]
public class RubricController(
    IRubricService service,
    IAuthorizationService auth,
    DatabaseContext ctx,
    IMemberService memberService
) : Controller
{
    [HttpGet]
    [ProtectedResource("rubrics", "rubrics:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all rubrics")]
    [EndpointDescription("Retrieve a paginated list of all rubrics")]
    public async Task<ActionResult<IEnumerable<RubricDO>>> GetAll(
        [FromQuery(Name = "filter[id]")] Guid? id,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[slug]")] string? slug,
        [FromQuery(Name = "filter[enabled]")] bool? enabled,
        [FromQuery(Name = "filter[workspace_id]")] Guid? workspace,
        [FromQuery(Name = "filter[project_id]")] Guid? projectId,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        // TODO: Delete this nasty escape hatch.

        var userId = User.GetSID();
        var staff = await auth.AuthorizeAsync(User, "staff");
        Expression<Func<Rubric, bool>>? visibility = staff.Succeeded
            ? null
            : r => r.Public || ctx.Members.Any(m =>
                m.EntityType == MemberEntityType.Workspace &&
                m.EntityId == r.WorkspaceId &&
                m.UserId == userId &&
                m.LeftAt == null
            );

        var page = await service.GetAllAsync(sorting, pagination, token,
            id is null ? null : r => r.Id == id,
            workspace is null ? null : n => n.WorkspaceId == workspace,
            !projectId.HasValue ? null : n => n.ProjectId == projectId,
            string.IsNullOrWhiteSpace(name) ? null : r => EF.Functions.ILike(r.Name, $"%{name}%"),
            string.IsNullOrWhiteSpace(slug) ? null : r => r.Slug == slug,
            enabled is null ? null : r => r.Enabled == enabled,
            visibility
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(r => new RubricDO(r)));
    }

    [HttpGet("{id:guid}")]
    [ProtectedResource("rubrics", "rubrics:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query a rubric")]
    [EndpointDescription("Retrieve a specific rubric by ID")]
    public async Task<ActionResult<RubricDO>> GetById(Guid id, CancellationToken token)
    {
        var rubric = await service.FindByIdAsync(id, token);
        return rubric is null ? NotFound() : Ok(new RubricDO(rubric));
    }

    [HttpPatch("{id:guid}")]
    [RequireScope("workspace")]
    [ProtectedResource("rubrics", "rubrics:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Update a rubric")]
    [EndpointDescription("Update rubric information")]
    public async Task<ActionResult<RubricDO>> Update(Guid id, [FromBody] PatchRubricRequestDTO body, CancellationToken token)
    {
        var rubric = await service.FindByIdAsync(id, token);
        if (rubric is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await memberService.FindByEntityAndUserId(rubric.WorkspaceId, User.GetSID(), token);
            if (member is null) return Forbid();
        }

        rubric.Name = body.Name.GetValueOrDefault(rubric.Name);
        rubric.Public = body.Public.GetValueOrDefault(rubric.Public);
        rubric.Enabled = body.Enabled.GetValueOrDefault(rubric.Enabled);

        List<(ReviewKinds Kind, int Count)>? variants = body.Variants is not null
            ? body.Variants.Where(v => v.Required > 0).Select(v => (v.Kind, v.Required)).ToList()
            : null;

        await service.UpdateRubricAsync(rubric, variants, token);
        return Ok(new RubricDO(rubric));
    }

    [Tags("Workspace")]
    [HttpPost("{id:guid}/deprecate")]
    [RequireScope("workspace")]
    [ProtectedResource("rubrics", "rubrics:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Deprecate a rubric")]
    public async Task<IActionResult> Deprecate(Guid id, CancellationToken token)
    {
        var rubric = await service.FindByIdAsync(id, token);
        if (rubric is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await memberService.FindByEntityAndUserId(rubric.WorkspaceId, User.GetSID(), token);
            if (member is null) return Forbid();
        }

        rubric.Deprecated = true;
        await service.UpdateAsync(rubric, token);
        return NoContent();
    }

    [Tags("Workspace")]
    [HttpPost("{id:guid}/undeprecate")]
    [RequireScope("workspace")]
    [ProtectedResource("rubrics", "rubrics:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Undeprecate a rubric")]
    public async Task<IActionResult> Undeprecate(Guid id, CancellationToken token)
    {
        var rubric = await service.FindByIdAsync(id, token);
        if (rubric is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await memberService.FindByEntityAndUserId(rubric.WorkspaceId, User.GetSID(), token);
            if (member is null) return Forbid();
        }

        rubric.Deprecated = false;
        await service.UpdateAsync(rubric, token);
        return NoContent();
    }
}