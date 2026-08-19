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

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("rubrics")]
[ProtectedResource("rubrics")]
public class RubricController(
    IRubricService service,
    IAuthorizationService auth,
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
        // [FromQuery(Name = "filter[enabled]")] bool? enabled,
        [FromQuery(Name = "filter[workspace_id]")] Guid? workspace,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        var page = await service.GetAllAsync(sorting, pagination, token,
            id is null ? null : r => r.Id == id,
            workspace is null ? null : n => n.WorkspaceId == workspace,
            string.IsNullOrWhiteSpace(name) ? null : r => EF.Functions.ILike(r.Name, $"%{name}%"),
            string.IsNullOrWhiteSpace(slug) ? null : r => r.Slug == slug
            // enabled is null ? null : r => r.Enabled == enabled,
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
        if (body.Variants is not null)
        {
            rubric.Variants = [.. body.Variants
                .Where(v => v.Required > 0)
                .Select(v => new RubricVariant
                {
                    RubricId = id,
                    Kind = v.Kind,
                    Count = v.Required
                })];
        }

        await service.UpdateAsync(rubric, token);
        return Ok(new RubricDO(rubric));
    }

    [Tags("Workspace")]
    [HttpDelete("{id:guid}")]
    [RequireScope("workspace")]
    [ProtectedResource("rubrics", "rubrics:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Delete a rubric")]
    [EndpointDescription("Delete a rubric")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        var rubric = await service.FindByIdAsync(id, token);
        if (rubric is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await memberService.FindByEntityAndUserId(rubric.WorkspaceId, User.GetSID(), token);
            if (member is null) return Forbid();
        }

        await service.DeleteAsync(rubric, token);
        return NoContent();
    }
}