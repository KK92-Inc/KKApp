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
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Responses.Entities.Notifications;
using App.Backend.Models.Requests.Users;
using App.Backend.Models.Requests.Projects;
using App.Backend.Models.Requests.Goals;
using App.Backend.Models.Requests.Cursus;
using App.Backend.Domain.Entities.Users;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("workspace")]
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
    [ProtectedResource("workspaces", ["workspaces:write", "cursus:write"])]
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


    #region TransferEntities

    [Authorize(Policy = "IsStaff")]
    [HttpPost("{from:guid}/transfer/cursus/{to:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("workspaces", ["workspaces:write", "cursus:write"])]
    [EndpointSummary("Transfer cursus between workspaces")]
    [EndpointDescription("Transfer one or more cursus from one workspace to another")]
    public async Task<ActionResult> TransferCursus(
        Guid from,
        Guid to,
        [FromBody] IEnumerable<Guid> cursusIds,
        CancellationToken token)
    {
        var source = await workspaceService.FindByIdAsync(from, token);
        var target = await workspaceService.FindByIdAsync(to, token);
        if (source is null || target is null)
            return NotFound();
        if (!cursusService.Exists(cursusIds))
            return Problem(detail: "Request contains invalid ID(s)");

        foreach (var id in cursusIds)
        {
            var cursus = await cursusService.FindByIdAsync(id, token);
            if (cursus is not null && cursus.WorkspaceId == from)
            {
                cursus.WorkspaceId = to;
                await cursusService.UpdateAsync(cursus, token);
            }
        }
        return NoContent();
    }

    [Authorize(Policy = "IsStaff")]
    [HttpPost("{from:guid}/transfer/goal/{to:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("workspaces", ["workspaces:write", "goals:write"])]
    [EndpointSummary("Transfer goals between workspaces")]
    [EndpointDescription("Transfer one or more goals from one workspace to another")]
    public async Task<ActionResult> TransferGoals(
        Guid from,
        Guid to,
        [FromBody] IEnumerable<Guid> goalIds,
        CancellationToken token)
    {
        var source = await workspaceService.FindByIdAsync(from, token);
        var target = await workspaceService.FindByIdAsync(to, token);
        if (source is null || target is null)
            return NotFound();
        if (!cursusService.Exists(goalIds))
            return Problem(detail: "Request contains invalid ID(s)");

        foreach (var id in goalIds)
        {
            var goal = await goalService.FindByIdAsync(id, token);
            if (goal is not null && goal.WorkspaceId == from)
            {
                goal.WorkspaceId = to;
                await goalService.UpdateAsync(goal, token);
            }
        }
        return NoContent();
    }

    [Authorize(Policy = "IsStaff")]
    [HttpPost("{from:guid}/transfer/project/{to:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("workspaces", ["workspaces:write", "projects:write"])]
    [EndpointSummary("Transfer projects between workspaces")]
    [EndpointDescription("Transfer one or more projects from one workspace to another")]
    public async Task<ActionResult> TransferProjects(
        Guid from,
        Guid to,
        [FromBody] IEnumerable<Guid> projectIds,
        CancellationToken token)
    {
        var source = await workspaceService.FindByIdAsync(from, token);
        var target = await workspaceService.FindByIdAsync(to, token);
        if (source is null || target is null)
            return NotFound();
        if (!cursusService.Exists(projectIds))
            return Problem(detail: "Request contains invalid ID(s)");

        foreach (var id in projectIds)
        {
            var project = await projectService.FindByIdAsync(id, token);
            if (project is not null && project.WorkspaceId == from)
            {
                project.WorkspaceId = to;
                await projectService.UpdateAsync(project, token);
            }
        }
        return NoContent();
    }

    #endregion TransferEntities

}
