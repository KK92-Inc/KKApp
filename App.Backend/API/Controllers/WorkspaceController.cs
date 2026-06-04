// ============================================================================
// Copyright (c) 2024 - W2Wizard.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Keycloak.AuthServices.Authorization;

using App.Backend.Core;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Requests.Projects;
using App.Backend.Models.Requests.Goals;
using App.Backend.Models.Requests.Cursus;
using App.Backend.Models.Requests.Rubrics;
using App.Backend.Models.Responses.Entities.Projects;
using App.Backend.Models.Responses.Entities.Cursus;
using App.Backend.Models.Responses.Entities.Reviews;
using App.Backend.Models.Responses.Entities.Applications;
using App.Backend.Models.Requests.Applications;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("workspace")]
public class WorkspaceController(
    ILogger<WorkspaceController> log,
    IWorkspaceService service,
    IApplicationService applicationService,
    IProjectService projectService,
    IGoalService goalService,
    ICursusService cursusService,
    IRubricService rubricService
) : Controller
{
    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProtectedResource("workspaces", "workspaces:read")]
    [EndpointSummary("Get the workspace of the user")]
    [EndpointDescription("Retrieves the workspace of the currently authenticated user")]
    public async Task<ActionResult<WorkspaceDO>> GetWorkspace(CancellationToken token)
    {
        var space = await service.FindByUserId(User.GetSID());
        if (space is null) return NotFound();
        return Ok(new WorkspaceDO(space));
    }

    #region AddEntities
    [HttpPost("{workspace:guid}/cursus")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProtectedResource("workspaces", "workspaces:read")]
    [EndpointSummary("Create a new cursus")]
    [EndpointDescription("Directly create a new project to be added to the workspace")]
    public async Task<ActionResult<CursusDO>> AddCursus(
        Guid workspace,
        [FromBody] PostCursusRequestDTO dto,
        CancellationToken token
    )
    {
        var space = await service.FindByIdAsync(workspace, token);
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

        var cursus = await service.AddCursusAsync(space.Id, new()
        {
            Name = dto.Name,
            WorkspaceId = workspace,
            Description = dto.Description ?? string.Empty,
            Slug = dto.Name.ToSlug(),
            Variant = dto.Variant,
            CompletionMode = dto.CompletionMode,
            Active = dto.Active,
            Public = dto.Public
        }, token);

        return Ok(new CursusDO(cursus));
    }

    [HttpPost("{workspace:guid}/goal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    // [ProtectedResource("workspaces", ["workspaces:write", "goals:write"])]
    [EndpointSummary("Create a new goal")]
    [EndpointDescription("Directly create a new goal to be added to the workspace")]
    public async Task<ActionResult<GoalDO>> AddGoal(
        Guid workspace,
        [FromBody] PostGoalRequestDTO dto,
        CancellationToken token
    )
    {
        var space = await service.FindByIdAsync(workspace, token);
        if (space is null)
            return NotFound();
        if (space.Ownership is EntityOwnership.Organization && !User.IsInRole("Staff"))
            return Forbid();

        var id = User.GetSID();
        if (space.OwnerId is not null && space.OwnerId != id)
            return Forbid();
        if (await goalService.FindBySlugAsync(dto.Name.ToSlug()) is not null)
            return Conflict();

        var goal = await service.AddGoalAsync(space.Id, new()
        {
            Name = dto.Name,
            WorkspaceId = workspace,
            Description = dto.Description ?? string.Empty,
            Slug = dto.Name.ToSlug(),
            Active = dto.Active,
            Public = dto.Public,
        }, token);

        return Ok(new GoalDO(goal));
    }

    [HttpPost("{workspace:guid}/project")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProtectedResource("workspaces", ["workspaces:write"])]
    [EndpointSummary("Create a new project")]
    [EndpointDescription("Directly create a new project to be added to the workspace")]
    public async Task<ActionResult<ProjectDO>> AddProject(
        Guid workspace,
        [FromBody] PostProjectRequestDTO dto,
        CancellationToken token
    )
    {
        var space = await service.FindByIdAsync(workspace, token);
        if (space is null)
            return NotFound();
        if (space.Ownership is EntityOwnership.Organization && !User.IsInRole("Staff"))
            return Forbid();

        var id = User.GetSID();
        if (space.OwnerId is not null && space.OwnerId != id)
            return Forbid();
        if (await projectService.FindBySlugAsync(dto.Name.ToSlug()) is not null)
            return Conflict();

        var project = await service.AddProjectAsync(space.Id, new()
        {
            Name = dto.Name,
            WorkspaceId = workspace,
            Description = dto.Description ?? string.Empty,
            Slug = dto.Name.ToSlug(),
            Active = dto.Active,
            Public = dto.Public
        }, token);

        return Ok(new ProjectDO(project));
    }

    [HttpPost("{id:guid}/rubric")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    // [ProtectedResource("workspaces", ["workspaces:write", "rubrics:write"])]
    [EndpointSummary("Create a new rubric")]
    [EndpointDescription("Create a new rubric with an associated git repository")]
    public async Task<ActionResult<RubricDO>> AddRubric(Guid id, [FromBody] PostRubricEntityRequestDTO body, CancellationToken token)
    {
        var space = await service.FindByIdAsync(id, token);
        if (space is null)
            return NotFound();
        if (space.Ownership is EntityOwnership.Organization && !User.IsInRole("Staff"))
            return Forbid();

        var userId = User.GetSID();
        if (space.OwnerId is not null && space.OwnerId != userId)
            return Forbid();
        if (await rubricService.FindBySlugAsync(body.Name.ToSlug(), token) is not null)
            return Conflict();

        var rubric = await service.AddRubricAsync(space.Id, new()
        {
            Name = body.Name,
            Markdown = body.Markdown ?? string.Empty,
            Slug = body.Name.ToSlug(),
            CreatorId = userId,
            Public = body.Public,
            Enabled = body.Enabled,
        }, token);

        return Ok(new RubricDO(rubric));
    }

    [HttpPost("{id:guid}/application")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProtectedResource("workspaces", ["applications:write"])]
    [EndpointSummary("Create a new application")]
    [EndpointDescription("Create a new application with an associated git repository")]
    public async Task<IActionResult> AddApplication(Guid id, [FromBody] PostApplicationRequestDTO dto, CancellationToken token)
    {
        var space = await service.FindByIdAsync(id, token);
        if (space is null) return NotFound();

        var clientId = $"w2id-{Guid.CreateVersion7().ToString("N")[..12]}";
        var app = await applicationService.CreateAsync(new Application
        {
            Name = dto.Name,
            Description = dto.Description,
            ClientId = clientId,
            WorkspaceId = space.Id,
            RedirectUris = dto.RedirectUris,
        }, token);

        return Ok(new ApplicationDO(app));
    }

    [HttpPatch("{id:guid}/application/{appId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProtectedResource("workspaces", ["applications:write"])]
    [EndpointSummary("Update an existing application")]
    [EndpointDescription("Update an existing application and its associated client in Keycloak")]
    public async Task<IActionResult> UpdateApplication(Guid id, Guid appId, [FromBody] PatchApplicationRequestDTO dto, CancellationToken token)
    {
        var space = await service.FindByIdAsync(id, token);
        if (space is null) return NotFound();

        var app = await applicationService.FindByIdAsync(appId, token);
        if (app is null || app.WorkspaceId != space.Id)
            return NotFound();

        if (await applicationService.FindByIdAsync(appId, token) is Application existingApp && existingApp.Id != appId)
            return Conflict();

        app.Name = dto.Name ?? app.Name;
        app.Description = dto.Description ?? app.Description;
        app.RedirectUris = dto.RedirectUris ?? app.RedirectUris;
        await applicationService.UpdateAsync(app, token);
        return Ok(new ApplicationDO(app));
    }

    [HttpDelete("{id:guid}/application/{appId:guid}")]
    [ProtectedResource("workspaces", ["applications:delete"])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete an existing application")]
    [EndpointDescription("Delete an existing application and its associated client in Keycloak")]
    public async Task<IActionResult> DeleteApplication(Guid id, Guid appId, CancellationToken token)
    {
        var space = await service.FindByIdAsync(id, token);
        if (space is null) return NotFound();

        var app = await applicationService.FindByIdAsync(appId, token);
        if (app is null || app.WorkspaceId != space.Id)
            return NotFound();

        await applicationService.DeleteAsync(app, token);
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
        var source = await service.FindByIdAsync(from, token);
        var target = await service.FindByIdAsync(to, token);
        if (source is null || target is null)
            return NotFound();
        if (!await cursusService.ExistsAsync(cursusIds, token))
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
        var source = await service.FindByIdAsync(from, token);
        var target = await service.FindByIdAsync(to, token);
        if (source is null || target is null)
            return NotFound();
        if (!await goalService.ExistsAsync(goalIds, token))
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
    [ProtectedResource("workspaces", ["workspaces:write"])]
    [EndpointSummary("Transfer projects between workspaces")]
    [EndpointDescription("Transfer one or more projects from one workspace to another")]
    public async Task<ActionResult> TransferProjects(
        Guid from,
        Guid to,
        [FromBody] IEnumerable<Guid> projectIds,
        CancellationToken token)
    {
        if (from == to)
            return NoContent();
        if (!projectIds.Any())
            return NoContent();

        var source = await service.FindByIdAsync(from, token);
        var target = await service.FindByIdAsync(to, token);
        if (source is null || target is null)
            return NotFound();

        // 2. FIX: Validate against the correct service/table
        if (!await projectService.ExistsAsync(projectIds, token))
            return UnprocessableEntity(new ProblemDetails { Detail = "Request contains invalid ID(s)" });

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
