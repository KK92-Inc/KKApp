// ============================================================================
// Copyright (c) 2024 - W2Wizard.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Core.Query;
using App.Backend.API.Params;
using App.Backend.Core.Services.Implementation;
using App.Backend.Domain;
using App.Backend.Core.Services.Interface;
using App.Backend.Models;
using Keycloak.AuthServices.Authorization;
using App.Backend.Domain.Enums;
using NXTBackend.API.Core.Services.Interface;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Responses.Entities.Notifications;
using App.Backend.Models.Requests.Users;
using App.Backend.Models.Requests.Projects;
using App.Backend.Models.Requests.Goals;
using App.Backend.Models.Requests.Cursus;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("workspace")]
// [ProtectedResource("users"), Authorize]
public class WorkspaceController(
    ILogger<WorkspaceController> log,
    IWorkspaceService workspaceService,
    IProjectService projectService,
    IGoalService goalService,
    ICursusService cursusService
) : Controller
{
    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProtectedResource("workspaces", "workspaces:read")]
    [EndpointSummary("Get the workspace of the user")]
    [EndpointDescription("Retrieves the workspace of the currently authenticated user")]
    public async Task<ActionResult<WorkspaceDO>> GetWorkspace(CancellationToken token)
    {
        var space = await workspaceService.FindByUserId(User.GetSID());
        if (space is null) return NotFound();
        return Ok(new WorkspaceDO(space));
    }

    #region AddEntities
    [HttpPost("{workspace:guid}/cursus")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProtectedResource("workspaces", ["workspaces:write", "cursus:write"])]
    [EndpointSummary("Create a new cursus")]
    [EndpointDescription("Directly create a new project to be added to the workspace")]
    public async Task<ActionResult> AddCursus(
        Guid workspace,
        [FromBody] PostCursusRequestDTO dto,
        CancellationToken token
    )
    {
        var space = await workspaceService.FindByIdAsync(workspace, token);
        if (space is null)
            return NotFound();
        // For Root level workspaces (Organization owned), we must be staff
        if (space.Ownership is EntityOwnership.Organization && !User.IsInRole("Staff"))
            return Forbid();

        // If it's a personal workspace, ensure the SID matches the owner
        var id = User.GetSID();
        log.LogDebug("Owner: {OwnerId} => UserID: {Id}", space.OwnerId, id);
        if (space.OwnerId is not null && space.OwnerId != id)
            return Forbid();
        if (await cursusService.FindBySlugAsync(dto.Name.ToSlug()) is not null)
            return Conflict();

        await cursusService.CreateAsync(new()
        {
            Name = dto.Name,
            WorkspaceId = workspace,
            Description = dto.Description ?? string.Empty,
            Slug = dto.Name.ToSlug(),
            Track = dto.Track,
            Active = dto.Active,
            Public = dto.Public
        }, token);

        return NoContent();
    }

    [HttpPost("{workspace:guid}/goal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProtectedResource("workspaces", ["workspaces:write", "goals:write"])]
    [EndpointSummary("Create a new goal")]
    [EndpointDescription("Directly create a new goal to be added to the workspace")]
    public async Task<ActionResult> AddGoal(
        Guid workspace,
        [FromBody] PostGoalRequestDTO dto,
        CancellationToken token
    )
    {
        var space = await workspaceService.FindByIdAsync(workspace, token);
        if (space is null)
            return NotFound();
        if (space.Ownership is EntityOwnership.Organization && !User.IsInRole("Staff"))
            return Forbid();

        var id = User.GetSID();
        if (space.OwnerId is not null && space.OwnerId != id)
            return Forbid();
        if (await goalService.FindBySlugAsync(dto.Name.ToSlug()) is not null)
            return Conflict();

        await goalService.CreateAsync(new()
        {
            Name = dto.Name,
            WorkspaceId = workspace,
            Description = dto.Description ?? string.Empty,
            Slug = dto.Name.ToSlug(),
            Active = dto.Active,
            Public = dto.Public
        }, token);

        return NoContent();
    }

    [HttpPost("{workspace:guid}/project")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProtectedResource("workspaces", ["workspaces:write", "projects:write"])]
    [EndpointSummary("Create a new project")]
    [EndpointDescription("Directly create a new project to be added to the workspace")]
    public async Task<ActionResult> AddProject(
        Guid workspace,
        [FromBody] PostProjectRequestDTO dto,
        CancellationToken token
    )
    {
        var space = await workspaceService.FindByIdAsync(workspace, token);
        if (space is null)
            return NotFound();
        if (space.Ownership is EntityOwnership.Organization && !User.IsInRole("Staff"))
            return Forbid();

        var id = User.GetSID();
        if (space.OwnerId is not null && space.OwnerId != id)
            return Forbid();
        if (await projectService.FindBySlugAsync(dto.Name.ToSlug()) is not null)
            return Conflict();

        await projectService.CreateAsync(new()
        {
            Name = dto.Name,
            WorkspaceId = workspace,
            Description = dto.Description ?? string.Empty,
            Slug = dto.Name.ToSlug(),
            Active = dto.Active,
            Public = dto.Public
        }, token);

        return NoContent();
    }

    #endregion AddEntities
    #region AddExisitingEntities

    [Authorize(Policy = "IsStaff")]
    [HttpPut("{workspace:guid}/cursus/{cursus:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProtectedResource("workspaces", "workspaces:write")]
    [EndpointSummary("Add a new cursus")]
    [EndpointDescription("Add an exisiting cursus to the workspace")]
    public async Task<ActionResult> AddCursus(Guid workspace, Guid cursus, CancellationToken token)
    {
        var space = await workspaceService.FindByIdAsync(workspace, token);
        var targetCursus = await cursusService.FindByIdAsync(cursus, token);
        if (space is null || targetCursus is null)
            return NotFound();
        if (space.Ownership is EntityOwnership.Organization && !User.IsInRole("Staff"))
            return Forbid();

        var id = User.GetSID();
        if (space.OwnerId is not null && space.OwnerId != id)
            return Forbid();

        targetCursus.WorkspaceId = space.Id;
        await cursusService.UpdateAsync(targetCursus, token);
        return NoContent();
    }

    [Authorize(Policy = "IsStaff")]
    [HttpPut("{workspace:guid}/goal/{goal:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProtectedResource("workspacse", "workspaces:write")]
    [EndpointSummary("Add a new goal")]
    [EndpointDescription("Add an exisiting goal to the workspace")]
    public async Task<ActionResult> AddGoal(Guid workspace, Guid goal, CancellationToken token)
    {
        var space = await workspaceService.FindByIdAsync(workspace, token);
        var targetGoal = await goalService.FindByIdAsync(goal, token);
        if (space is null || targetGoal is null)
            return NotFound();

        targetGoal.WorkspaceId = space.Id;
        await goalService.UpdateAsync(targetGoal, token);
        return NoContent();
    }

    [Authorize(Policy = "IsStaff")]
    [HttpPut("{workspace:guid}/project/{project:guid}")]
    [ProtectedResource("workspaces", "workspaces:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Add a new project")]
    [EndpointDescription("Add an exisiting project to the workspace")]
    public async Task<ActionResult> AddProject(Guid workspace, Guid project, CancellationToken token)
    {
        var space = await workspaceService.FindByIdAsync(workspace, token);
        var targetProject = await projectService.FindByIdAsync(project, token);
        if (space is null || targetProject is null)
            return NotFound();

        targetProject.WorkspaceId = space.Id;
        await projectService.UpdateAsync(targetProject, token);
        return NoContent();
    }

    #endregion AddExisitingEntities
}
