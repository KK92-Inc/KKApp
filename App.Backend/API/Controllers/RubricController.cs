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

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("rubrics")]
[ProtectedResource("rubrics")]
public class RubricController(ILogger<RubricController> log, IRubricService service) : Controller
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
        [FromQuery(Name = "filter[public]")] bool? isPublic,
        [FromQuery(Name = "filter[creator_id]")] Guid? creatorId,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        var page = await service.GetAllAsync(sorting, pagination, token,
            id is null ? null : r => r.Id == id,
            string.IsNullOrWhiteSpace(name) ? null : r => EF.Functions.ILike(r.Name, $"%{name}%"),
            string.IsNullOrWhiteSpace(slug) ? null : r => r.Slug == slug,
            enabled is null ? null : r => r.Enabled == enabled,
            isPublic is null ? null : r => r.Public == isPublic,
            creatorId is null ? null : r => r.CreatorId == creatorId
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
    public async Task<ActionResult<RubricDO>> GetById(Guid id)
    {
        var rubric = await service.FindByIdAsync(id);
        return rubric is null ? NotFound() : Ok(new RubricDO(rubric));
    }

    [HttpPatch("{id:guid}")]
    [ProtectedResource("rubrics", "rubrics:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Update a rubric")]
    [EndpointDescription("Update rubric information")]
    public async Task<ActionResult<RubricDO>> Update(Guid id, [FromBody] PatchRubricEntityRequestDTO body, CancellationToken token)
    {
        var rubric = await service.FindByIdAsync(id, token);
        if (rubric is null)
            return NotFound();
        if (rubric.CreatorId != User.GetSID() && !User.IsInRole("admin"))
            return Forbid();

        rubric.Name = body.Name ?? rubric.Name;
        rubric.Markdown = body.Markdown ?? rubric.Markdown;
        rubric.Public = body.Public ?? rubric.Public;
        rubric.Enabled = body.Enabled ?? rubric.Enabled;
        rubric.ReviewerRules = body.ReviewerRules ?? rubric.ReviewerRules;
        rubric.RevieweeRules = body.RevieweeRules ?? rubric.RevieweeRules;

        await service.UpdateAsync(rubric, token);
        return Ok(new RubricDO(rubric));
    }

    [HttpPut("{id:guid}/variants")]
    [ProtectedResource("rubrics", "rubrics:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Set rubric variant configuration")]
    [EndpointDescription("Replaces the review kind composition for a rubric. Omitted kinds are disabled.")]
    public async Task<ActionResult<RubricDO>> PutRubricVariants(
        Guid id,
        [FromBody] PutRubricVariantsRequestDTO body,
        CancellationToken token
    )
    {
        var rubric = await service.FindByIdAsync(id, token);
        if (rubric is null)
            return NotFound();
        if (rubric.CreatorId != User.GetSID() && !User.IsInRole("admin"))
            return Forbid();

        rubric = await service.SetVariantsAsync(
            id,
            body.Variants.Select(v => new RubricVariant() {
                Kind = v.Kind,
                Count = v.Required
            }),
            token
        );

        if (rubric is null)
            return NotFound();

        return Ok(new RubricDO(rubric));
    }

    [Tags("Workspace")]
    [HttpDelete("{id:guid}")]
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
        if (rubric is null)
            return NotFound();

        await service.DeleteAsync(rubric, token);
        return NoContent();
    }
}
