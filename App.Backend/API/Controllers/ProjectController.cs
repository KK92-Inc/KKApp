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
using Microsoft.EntityFrameworkCore;
using App.Backend.Models.Responses.Entities.Projects;
using App.Backend.Models.Requests.Projects;
using App.Backend.Models.Responses.Entities.Reviews;
using App.Backend.API.Utils;
using App.Backend.Database;
using System.Linq.Expressions;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("projects")]
[ProtectedResource("projects"), Authorize]
public class ProjectController(
    IProjectService service,
    IMemberService memberService,
    IAuthorizationService auth,
    DatabaseContext ctx,
    IRubricService rubricService
) : Controller
{
    [HttpGet]
    [ProtectedResource("projects", "projects:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all projects")]
    [EndpointDescription("Retrieve a paginated list of all projects")]
    public async Task<ActionResult<IEnumerable<ProjectDO>>> GetAll(
        [FromQuery(Name = "filter[id]")] Guid? id,
        [FromQuery(Name = "filter[workspace_id]")] Guid? workspace,
        [FromQuery(Name = "filter[enabled]")] bool? enabled,
        [FromQuery(Name = "filter[name]")] string? name,
        [FromQuery(Name = "filter[slug]")] string? slug,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        // TODO: Delete this nasty escape hatch.
        var userId = User.GetSID();
        var staff = await auth.AuthorizeAsync(User, "staff");
        Expression<Func<Project, bool>>? visibility = staff.Succeeded
            ? null
            : p => p.Public || ctx.Members.Any(m =>
                m.EntityType == MemberEntityType.Workspace &&
                m.EntityId == p.WorkspaceId &&
                m.UserId == userId &&
                m.LeftAt == null
            );

        var page = await service.GetAllAsync(sorting, pagination, token,
            id is null ? null : n => n.Id == id,
            enabled is null ? null : n => n.Active == enabled,
            workspace is null ? null : n => n.WorkspaceId == workspace,
            string.IsNullOrWhiteSpace(name) ? null : n => EF.Functions.ILike(n.Name, $"%{name}%"),
            string.IsNullOrWhiteSpace(slug) ? null : n => n.Slug == slug,
            visibility
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(c => new ProjectDO(c)));
    }

    [HttpPost("{id:guid}/deprecate")]
    [RequireScope("workspace")]
    [ProtectedResource("projects", "projects:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Deprecate a project")]
    [EndpointDescription("Mark a project as deprecated")]
    public async Task<IActionResult> Deprecate(Guid id, CancellationToken token)
    {
        var project = await service.FindByIdAsync(id, token);
        if (project is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await memberService.FindByEntityAndUserId(project.WorkspaceId, User.GetSID(), token);
            if (member is null) return Forbid();
        }

        project.Deprecated = true;
        await service.UpdateAsync(project, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/undeprecate")]
    [RequireScope("workspace")]
    [ProtectedResource("projects", "projects:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Undeprecate a project")]
    [EndpointDescription("Restore a project by setting deprecated to false")]
    public async Task<IActionResult> Undeprecate(Guid id, CancellationToken token)
    {
        var project = await service.FindByIdAsync(id, token);
        if (project is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await memberService.FindByEntityAndUserId(project.WorkspaceId, User.GetSID(), token);
            if (member is null) return Forbid();
        }

        project.Deprecated = false;
        await service.UpdateAsync(project, token);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [ProtectedResource("projects", "projects:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query a project")]
    [EndpointDescription("Retrieve a specific project by ID")]
    public async Task<ActionResult<ProjectDO>> GetById(Guid id, CancellationToken token)
    {
        var project = await service.FindByIdAsync(id, token);
        return project is null ? NotFound() : Ok(new ProjectDO(project));
    }

    [HttpGet("{id:guid}/rubric")]
    [ProtectedResource("projects", "projects:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get the rubric for this project")]
    [EndpointDescription("A project may be targeted by a rurbic. This can be either the wildcard or a specific one.")]
    public async Task<ActionResult<RubricDO>> GetRubric(Guid id, CancellationToken token)
    {
        var project = await service.FindByIdAsync(id, token);
        if (project is null)
            return NotFound();

        var sorting = new Sorting()
        {
            OrderBy = "ProjectId",
            Order = Order.Ascending
        };

        // NOTE(W2): There are *at most* 2 rubrics, a wildcard or a project specific one.
        // Thus the pagination doesn't matter here.
        var page = await rubricService.GetAllAsync(sorting, new Pagination(), token,
            r => r.ProjectId == id || r.ProjectId == null
        );

        var rubric = page.Items.FirstOrDefault();
        if (rubric is null)
            return NotFound();
        return Ok(new RubricDO(rubric));
    }

    [HttpPatch("{id:guid}")]
    [RequireScope("workspace")]
    [ProtectedResource("projects", "projects:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Update a project")]
    [EndpointDescription("Update project information")]
    public async Task<ActionResult<ProjectDO>> Update(Guid id, [FromBody] PatchProjectRequestDTO request, CancellationToken token)
    {
        // 1. Grab the .Succeeded boolean
        var project = await service.FindByIdAsync(id, token);
        if (project is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await memberService.FindByEntityAndUserId(project.WorkspaceId, User.GetSID(), token);
            if (member is null) return Forbid();
        }

        project.Name = request.Name ?? project.Name;
        project.Description = request.Description ?? project.Description;
        project.Active = request.Active ?? project.Active;
        project.Public = request.Public ?? project.Public;
        project.MaxMembers = request.MaxMembers ?? project.MaxMembers;

        await service.UpdateAsync(project, token);
        return Ok(new ProjectDO(project));
    }
}
