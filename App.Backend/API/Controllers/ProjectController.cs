// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
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
using App.Backend.Models.Responses.Entities.Projects;
using App.Backend.Models.Requests.Projects;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("projects")]
[ProtectedResource("projects"), Authorize]
public class ProjectController(ILogger<ProjectController> log, IProjectService projects, ISubscriptionService subscriptions) : Controller
{
    [HttpGet]
    [ProtectedResource("projects", "projects:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all projects")]
    [EndpointDescription("Retrieve a paginated list of all projects")]
    public async Task<ActionResult> GetAll(
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting
    )
    {
        var page = await projects.GetAllAsync(sorting, pagination);
        page.AppendHeaders(Request.Headers);
        return Ok(page.Items.Select(p => new ProjectDO(p)));
    }

    [HttpPost]
    [ProtectedResource("projects", "projects:write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Create a new project")]
    [EndpointDescription("Create a new project")]
    public async Task<ActionResult<ProjectDO>> Create([FromBody] PostProjectRequestDTO request)
    {
        var project = await projects.CreateAsync(new ()
        {
            Name = request.Name,
            Description = request.Description,
            Slug = request.Slug,
            Active = request.Active,
            Public = request.Public,
            Deprecated = request.Deprecated
        });

        return Created();
    }

    [HttpDelete]
    [ProtectedResource("projects", "projects:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Delete a project")]
    [EndpointDescription("Delete a project and its user instances")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var project = await projects.FindByIdAsync(id);
        if (project is null)
            return NotFound();

        await projects.DeleteAsync(project);
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
    public async Task<ActionResult<ProjectDO>> GetById(Guid id)
    {
        var project = await projects.FindByIdAsync(id);
        return project is null ? NotFound() : Ok(new ProjectDO(project));
    }

    [HttpPatch("{id:guid}")]
    [ProtectedResource("projects", "projects:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Update a project")]
    [EndpointDescription("Update project information")]
    public async Task<ActionResult<ProjectDO>> Update(Guid id, [FromBody] PatchProjectRequestDTO request)
    {
        var project = await projects.FindByIdAsync(id);
        if (project is null)
            return NotFound();

        project.Name = request.Name ?? project.Name;
        project.Description = request.Description ?? project.Description;
        project.Slug = request.Slug ?? project.Slug;
        project.Active = request.Active ?? project.Active;
        project.Public = request.Public ?? project.Public;
        project.Deprecated = request.Deprecated ?? project.Deprecated;
        await projects.UpdateAsync(project);
        return Ok(new ProjectDO(project));
    }
}