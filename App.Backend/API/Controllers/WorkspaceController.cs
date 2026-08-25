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
using App.Backend.Domain.Entities;
using App.Backend.Models.Requests.Application;
using App.Backend.API.Controllers.Interfaces;
using App.Backend.API.Notifications.Variants;
using Wolverine;
using App.Backend.Domain.Entities.Reviews;
using Keycloak.AuthServices.Sdk.Admin;
using App.Backend.API.Params;
using App.Backend.API.Utils;
using App.Backend.Domain.Relations;
using App.Git.Models.Requests;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("workspace")]
public class WorkspaceController(
    ILogger<WorkspaceController> log,
    IAuthorizationService auth,
    IWorkspaceService service,
    IApplicationService applicationService,
    IProjectService projectService,
    IGoalService goalService,
    IUserService userService,
    ICursusService cursusService,
    IRubricService rubricService,
    IMemberService memberService,
    IGitService gitService,
    IMessageBus bus
) : Controller
{
    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [RequireScope("workspace")]
    [ProtectedResource("workspaces", "workspaces:read")]
    [EndpointSummary("Get the workspace of the user")]
    [EndpointDescription("Retrieves the workspace of the currently authenticated user")]
    public async Task<ActionResult<WorkspaceDO>> GetWorkspace(CancellationToken token)
    {
        var space = await service.FindByUserId(User.GetSID(), token);
        if (space is not null) return Ok(new WorkspaceDO(space));

        // Just in case
        return Ok(new WorkspaceDO(await service.CreateAsync(new()
        {
            OwnerId = User.GetSID(),
            Ownership = EntityOwnership.User
        }, token)));
    }

    [HttpGet("root")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProtectedResource("workspaces", "workspaces:read")]
    [EndpointSummary("Get the workspace of the root")]
    [EndpointDescription(
@"The root workspace is a staff managed workspace that contains the campus's curated entities
such as official cursi, projects or rubrics.
    ")]
    public async Task<ActionResult<WorkspaceDO>> GetSystemWorkspace(CancellationToken token)
    {
        var space = await service.GetRootWorkspace(token);
        if (space is not null) return Ok(new WorkspaceDO(space));

        // Just in case
        return Ok(new WorkspaceDO(await service.CreateAsync(new()
        {
            OwnerId = null,
            Ownership = EntityOwnership.Organization
        }, token)));
    }

    [HttpGet("user/{id:guid}")]
    [RequireScope("workspace")]
    [ProtectedResource("workspaces", "workspaces:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Get the workspace of a user")]
    [EndpointDescription("Retrieves the workspace of a user")]
    public async Task<ActionResult<WorkspaceDO>> Get(Guid id, CancellationToken token)
    {
        var space = await service.FindByUserId(id, token);
        return space is null ? NotFound() : Ok(new WorkspaceDO(space));
    }

    [HttpPost("{workspace:guid}/cursus")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProtectedResource("cursus", "cursus:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
    [EndpointSummary("Create a new cursus")]
    [EndpointDescription("Create a new cursus to be added to the workspace")]
    public async Task<ActionResult<CursusDO>> AddCursus(Guid workspace, [FromBody] PostCursusRequestDTO body, CancellationToken token)
    {
        var space = await service.FindByIdAsync(workspace, token);
        if (space is null) return NotFound();

        var userId = User.GetSID(); // Must be admin for app workspace.
        var authorized = await auth.AuthorizeAsync(User, "staff");
        if (space.OwnerId != userId || (space.OwnerId is null && !authorized.Succeeded))
            return Forbid();

        if (await cursusService.FindBySlugAsync(body.Name.ToSlug(), token) is not null)
            return Conflict();

        var cursus = new Cursus()
        {
            WorkspaceId = workspace,
            Name = body.Name,
            Description = body.Description,
            Slug = body.Name.ToSlug(),
            Variant = body.Variant,
            CompletionMode = body.Mode,
            Active = body.Active,
            Public = body.Public
        };

        var nodes = body.Track.Nodes.Select(n => new CursusGoal
        {
            CursusId = cursus.Id,
            GoalId = n.GoalId,
            ParentGoalId = n.ParentId,
            ChoiceGroup = n.Group
        });

        cursus = await service.AddCursusAsync(space.Id, cursus, nodes, token);
        return Ok(new CursusDO(cursus));
    }

    [HttpPost("{workspace:guid}/goal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProtectedResource("goals", "goals:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
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
            Description = dto.Description,
            Slug = dto.Name.ToSlug(),
            Active = dto.Active,
            Public = dto.Public,
        }, token);

        return Ok(new GoalDO(goal));
    }

    [HttpPost("{workspace:guid}/project")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [RequireScope("workspace")]
    [ProtectedResource("projects", "projects:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
    [EndpointSummary("Create a new project")]
    [EndpointDescription("Directly create a new project to be added to the workspace")]
    public async Task<ActionResult<ProjectDO>> AddProject(
        Guid workspace,
        [FromBody] PostProjectRequestDTO body,
        CancellationToken token
    )
    {
        var space = await service.FindByIdAsync(workspace, token);
        if (space is null) return NotFound();

        var id = User.GetSID();
        var result = await auth.AuthorizeAsync(User, "staff");
        if (space.OwnerId is null && !result.Succeeded)
            return Forbid();
        if (space.OwnerId is not null && space.OwnerId != id)
            return Forbid();
        if (await projectService.FindBySlugAsync(body.Name.ToSlug(), token) is not null)
            return Conflict();

        var user = await userService.FindByIdAsync(id, token);
        if (user is null) return Forbid();

        var commit = new PostCommitWithAuthorDTO()
        {
            Files = body.Commit.Files,
            Message = body.Commit.Message,
            Author = user.Login,
            Email = "N/A"
        };

        var project = await service.AddProjectAsync(space.Id, new()
        {
            Name = body.Name,
            WorkspaceId = workspace,
            Description = body.Description,
            Slug = body.Name.ToSlug(),
            Active = body.Active,
            Public = body.Public,
            MaxMembers = body.MaxMembers,
        }, commit, token);

        return Ok(new ProjectDO(project));
    }

    [HttpPost("{workspace:guid}/rubric")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProtectedResource("rubrics", "rubrics:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
    [EndpointSummary("Create a new rubric")]
    [EndpointDescription("Create a new rubric to be added to the workspace")]
    public async Task<ActionResult<RubricDO>> AddRubric(Guid workspace, [FromBody] PostRubricRequestDTO body, CancellationToken token)
    {
        var space = await service.FindByIdAsync(workspace, token);
        if (space is null)
            return NotFound();

        var result = await auth.AuthorizeAsync(User, "staff");
        if (space.OwnerId is null && !result.Succeeded)
            return Forbid();

        var userId = User.GetSID();
        if (space.OwnerId is not null && space.OwnerId != userId)
            return Forbid();
        if (await rubricService.FindBySlugAsync(body.Name.ToSlug(), token) is not null)
            return Conflict();

        var rubric = new Rubric
        {
            Name = body.Name,
            Slug = body.Name.ToSlug(),
            ProjectId = body.ProjectId,
            Public = body.Public,
            Enabled = body.Enabled,
            Variants = [.. body.Variants
                .Where(v => v.Required > 0)
                .Select(v => new RubricVariant
                {
                    Kind = v.Kind,
                    Count = v.Required
                })]
        };

        var user = await userService.FindByIdAsync(userId, token);
        if (user is null) return Forbid();

        var commit = new PostCommitWithAuthorDTO()
        {
            Files = body.Commit.Files,
            Message = body.Commit.Message,
            Author = user.Login,
            Email = "N/A"
        };

        var created = await service.AddRubricAsync(space.Id, rubric, commit, token);
        return Ok(new RubricDO(created));
    }

    [Tags("Application")]
    [HttpGet("{id:guid}/application")]
    [ProtectedResource("applications", "applications:read")]
    [ProtectedResource("workspaces", "workspaces:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all applications")]
    [EndpointDescription("Retrieve a paginated list of all applications within a workspace")]
    public async Task<ActionResult<IEnumerable<ApplicationDO>>> GetAllApplications(
        Guid id,
        [FromQuery(Name = "filter[id]")] Guid? appId,
        [FromQuery(Name = "filter[client_id]")] Guid? clientId,
        [FromQuery] Sorting sorting,
        [FromQuery] Pagination pagination,
        CancellationToken token
    )
    {
        var page = await applicationService.GetAllAsync(sorting, pagination, token,
            a => a.WorkspaceId == id,
            appId is null ? null : a => a.Id == appId,
            clientId is null ? null : a => a.ClientId == clientId.ToString()
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(app => new ApplicationDO(app)));
    }

    [Tags("Application")]
    [HttpPost("{id:guid}/application")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("applications", "applications:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
    [EndpointSummary("Create a new application")]
    [EndpointDescription("Create a new application client linked to this workspace and fetch its initial credential secret.")]
    public async Task<IActionResult> AddApplication(Guid id, [FromBody] PostApplicationRequestDTO dto, CancellationToken token)
    {
        var space = await service.FindByIdAsync(id, token);
        if (space is null) return NotFound();

        var app = await applicationService.CreateAsync(new Application
        {
            Name = dto.Name,
            Description = dto.Description,
            Enabled = dto.Enabled,
            ClientId = $"w2id-{dto.Name.ToSlug()}-{Guid.CreateVersion7()}",
            WorkspaceId = space.Id,
            Scopes = dto.Scopes,
            RedirectUris = dto.RedirectUris
        }, token);

        return Created(new Uri($"/workspace/{space.Id}/application/{app.Id}", UriKind.Relative), new ApplicationDO(app));
    }

    [Tags("Application")]
    [HttpPatch("/application/{appId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("applications", "applications:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
    [EndpointSummary("Update an existing application")]
    [EndpointDescription("Update an existing application metadata configuration and synchronize changes out to Keycloak.")]
    public async Task<IActionResult> UpdateApplication(Guid appId, [FromBody] PatchApplicationRequestDTO dto, CancellationToken token)
    {
        var app = await applicationService.FindByIdAsync(appId, token);
        if (app is null) return NotFound();

        app.Name = dto.Name ?? app.Name;
        app.Enabled = dto.Enabled ?? app.Enabled;
        app.Description = dto.Description ?? app.Description;
        app.RedirectUris = dto.RedirectUris ?? app.RedirectUris;
        app.Scopes = dto.Scopes ?? app.Scopes;

        await applicationService.UpdateAsync(app, token);
        return Ok(new ApplicationDO(app));
    }

    [Tags("Application")]
    [HttpDelete("/application/{appId:guid}")]
    [ProtectedResource("applications", "applications:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete an existing application")]
    [EndpointDescription("Permanently delete an application registration and dismantle its linked client in Keycloak.")]
    public async Task<IActionResult> DeleteApplication(Guid appId, CancellationToken token)
    {
        var app = await applicationService.FindByIdAsync(appId, token);
        if (app is null) return NotFound();

        if (app.Workspace.OwnerId != User.GetSID())
            return Forbid();

        await applicationService.DeleteAsync(app, token);
        return NoContent();
    }

    [Tags("Application")]
    [HttpPost("/application/{appId:guid}/secret/rotate")]
    [ProtectedResource("applications", "applications:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Rotate client secret")]
    [EndpointDescription("Rotate / request a new secret")]
    public async Task<IActionResult> RotateApplicationSecret(Guid appId, CancellationToken token)
    {
        var app = await applicationService.FindByIdAsync(appId, token);
        if (app is null) return NotFound();

        if (app.Workspace.OwnerId != User.GetSID())
            return Forbid();

        var secret = await applicationService.RotateClientSecretAsync(app, token);
        Response.Headers.TryAdd("X-Client-Secret", secret);
        return NoContent();
    }

    [Tags("Application")]
    [HttpDelete("/application/{appId:guid}/consent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Revoke consent for a application")]
    [EndpointDescription("Revokes the consent you have given towards a application.")]
    public async Task<IActionResult> RevokeConsent(Guid appId, CancellationToken token)
    {
        var app = await applicationService.FindByIdAsync(appId, token);
        if (app is null) return NotFound();

        await applicationService.RevokeAccess(app, User.GetSID(), token);
        return NoContent();
    }

    [Tags("Application")]
    [HttpGet("/application/consented")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Query consented applications")]
    [EndpointDescription("Retrieve a list of all applications the current user has granted access to.")]
    public async Task<ActionResult<IEnumerable<ApplicationDO>>> GetConsentedApplications(CancellationToken token)
    {
        var apps = await applicationService.GetConsentedApplicationsAsync(User.GetSID(), token);
        return Ok(apps.Select(app => new ApplicationDO(app)));
    }

    [Authorize(Policy = "IsStaff")]
    [HttpPost("{from:guid}/transfer/cursus/{to:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProtectedResource("cursus", "cursus:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
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
            return UnprocessableEntity(new ProblemDetails { Detail = "Request contains invalid ID(s)" });

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
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProtectedResource("goals", "goals:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
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
            return UnprocessableEntity(new ProblemDetails { Detail = "Request contains invalid ID(s)" });

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
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProtectedResource("projects", "projects:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
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
}
