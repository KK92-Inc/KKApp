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
using App.Backend.Models.Requests.Application;
using App.Backend.API.Controllers.Interfaces;
using App.Backend.API.Notifications.Variants;
using Wolverine;
using App.Backend.Domain.Entities.Reviews;
using Keycloak.AuthServices.Sdk.Admin;
using App.Backend.API.Params;

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
    IUserService userService,
    ICursusService cursusService,
    IRubricService rubricService,
    IMemberService memberService,
    IMessageBus bus
) : Controller, IInviteController
{
    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProtectedResource("workspaces", "workspaces:read")]
    [EndpointSummary("Get the workspace of the user")]
    [EndpointDescription("Retrieves the workspace of the currently authenticated user")]
    public async Task<ActionResult<WorkspaceDO>> GetWorkspace(CancellationToken token)
    {
        var space = await service.FindByUserId(User.GetSID(), token);
        if (space is not null) return Ok(new WorkspaceDO(space));

        // If it is a new user, we just create it for them.
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

        // TODO: Make a migration for this as well
        // In case it's just not there e.g: Missing migration ?
        return Ok(new WorkspaceDO(await service.CreateAsync(new()
        {
            OwnerId = null,
            Ownership = EntityOwnership.Organization
        }, token)));
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
            Description = dto.Description,
            Slug = dto.Name.ToSlug(),
            Variant = dto.Variant,
            CompletionMode = dto.CompletionMode,
            Active = dto.Active,
            Public = dto.Public
        }, token);

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
    [ProtectedResource("projects", "projects:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
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
        if (await projectService.FindBySlugAsync(dto.Name.ToSlug(), token) is not null)
            return Conflict();

        var project = await service.AddProjectAsync(space.Id, new()
        {
            Name = dto.Name,
            WorkspaceId = workspace,
            Description = dto.Description,
            Slug = dto.Name.ToSlug(),
            Active = dto.Active,
            Public = dto.Public,
            MaxMembers = dto.MaxMembers
        }, token);

        return Ok(new ProjectDO(project));
    }

    [HttpPost("{id:guid}/rubric")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProtectedResource("rubrics", "rubrics:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
    [EndpointSummary("Create a new rubric")]
    [EndpointDescription("Create a new rubric to be added to the workspace")]
    public async Task<ActionResult<RubricDO>> AddRubric(Guid id, [FromBody] PostRubricEntityRequestDTO body, CancellationToken token)
    {
        var space = await service.FindByIdAsync(id, token);
        if (space is null)
            return NotFound();

        var isRoot = space.OwnerId is null;
        if (isRoot && !User.IsInRole("Staff"))
            return Forbid();

        var userId = User.GetSID();
        if (space.OwnerId is not null && space.OwnerId != userId)
            return Forbid();
        if (await rubricService.FindBySlugAsync(body.Name.ToSlug(), token) is not null)
            return Conflict();

        var rubric = await service.AddRubricAsync(space.Id, new()
        {
            Name = body.Name,
            Slug = body.Name.ToSlug(),
            CreatorId = userId,
            Public = body.Public,
            Enabled = body.Enabled,
        }, token);

        if (body.Variants is not null && body.Variants.Any())
        {
            rubric = await rubricService.SetVariantsAsync(
                rubric.Id,
                body.Variants.Select(v => new RubricVariant()
                {
                    Kind = v.Kind,
                    Count = v.Required
                }),
                token
            );
        }

        return Ok(new RubricDO(rubric));
    }

    [HttpGet("{id:guid}/application")]
    [ProtectedResource("applications", "applications:read")]
    [ProtectedResource("workspaces", "workspaces:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all applications")]
    [EndpointDescription("Retrieve a paginated list of all applications within a workspace")]
    public async Task<ActionResult<IEnumerable<ApplicationDO>>> GetAll(
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
            clientId is null ? null : a => a.KeycloakId == clientId
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(app => new ApplicationDO(app)));
    }

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

        var uniqueId = Guid.CreateVersion7().ToString("N")[..12];
        var app = await applicationService.CreateAsync(new Application
        {
            Name = dto.Name,
            Description = dto.Description,
            Enabled = dto.Enabled,
            ClientId = $"w2id-{dto.Name.ToSlug()}-{uniqueId}",
            WorkspaceId = space.Id,
            RedirectUris = dto.RedirectUris ?? [],
        }, token);

        return Created(new Uri($"/workspace/{space.Id}/application/{app.Id}", UriKind.Relative), new ApplicationDO(app));
    }

    [HttpPatch("{id:guid}/application/{appId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("applications", "applications:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
    [EndpointSummary("Update an existing application")]
    [EndpointDescription("Update an existing application metadata configuration and synchronize changes out to Keycloak.")]
    public async Task<IActionResult> UpdateApplication(Guid id, Guid appId, [FromBody] PatchApplicationRequestDTO dto, CancellationToken token)
    {
        var space = await service.FindByIdAsync(id, token);
        if (space is null) return NotFound();

        var app = await applicationService.FindByIdAsync(appId, token);
        if (app is null || app.WorkspaceId != space.Id)
            return NotFound();

        // Apply fields safely via null-coalescing operations
        app.Name = dto.Name ?? app.Name;
        app.Description = dto.Description ?? app.Description;
        app.RedirectUris = dto.RedirectUris ?? app.RedirectUris;

        await applicationService.UpdateAsync(app, token);
        return Ok(new ApplicationDO(app));
    }

    [HttpDelete("{id:guid}/application/{appId:guid}")]
    [ProtectedResource("applications", "applications:delete")]
    [ProtectedResource("workspaces", "workspaces:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete an existing application")]
    [EndpointDescription("Permanently delete an application registration and dismantle its linked client in Keycloak.")]
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

    [HttpPost("{id:guid}/application/{appId:guid}/secret/rotate")]
    [ProtectedResource("applications", "applications:write")]
    [ProtectedResource("workspaces", "workspaces:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Rotate client secret")]
    [EndpointDescription("Demotes the active secret to fallback 'rotated' status and issues a brand-new primary secret for zero-downtime migrations.")]
    public async Task<IActionResult> RotateApplicationSecret(Guid id, Guid appId, CancellationToken token)
    {
        var space = await service.FindByIdAsync(id, token);
        if (space is null) return NotFound();

        var app = await applicationService.FindByIdAsync(appId, token);
        if (app is null || app.WorkspaceId != space.Id)
            return NotFound();

        var secret = await applicationService.RotateClientSecretAsync(app.Id, token);
        Response.Headers.TryAdd("X-Client-Secret", secret ?? "undefined");
        return NoContent();
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

    [HttpGet("{id:guid}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get workspace members")]
    [EndpointDescription("Returns the paginated list of all members past and present")]
    public async Task<ActionResult<IEnumerable<MemberDO>>> GetMembers(
        Guid id,
        [FromQuery(Name = "filter[active]")] bool? active,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken token
    )
    {
        var page = await memberService.GetAllAsync(sorting, pagination, token,
            m => m.EntityType == MemberEntityType.Workspace,
            m => m.EntityId == id,
            active switch
            {
                true => m => m.LeftAt == null,
                false => m => m.LeftAt != null,
                null => null
            }
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(m => new MemberDO(m)));
    }

    [HttpPost("{id:guid}/invite/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [ProtectedResource("workspaces", "workspaces:write")]
    [EndpointSummary("Invite a user to a workspace")]
    [EndpointDescription("Invites another user to the workspace, granting them access to its projects and resources upon acceptance.")]
    public async Task<ActionResult<MemberDO>> InviteAsync(Guid Id, Guid userId, CancellationToken token)
    {
        var ws = await service.FindByIdAsync(Id, token);
        if (ws is null) return NotFound();

        // Workspace with Null as owner is the root workspace for admin/staff/global entities.
        // NOTE(W2): Ensure that JWT carries the correct claim in the JWT for the role to work.
        if (ws.OwnerId is null && !User.IsInRole("staff"))
            return Forbid();
        if (!await userService.ExistsAsync([userId], token))
            return NotFound();

        // Verify that requester is the leader.
        var actor = await memberService.FindByEntityAndUserId(Id, User.GetSID(), token);
        if (actor is null || actor.Role is not MemberRole.Leader)
            return Forbid();

        // Notify invite.
        var member = await memberService.InviteAsync(Id, userId, null, null, token);
        await bus.PublishAsync(new WorkspaceInviteNotification(userId, User.GetSID(), ws.Id));
        return Ok(new MemberDO(member));
    }

    [HttpDelete("{id:guid}/invite/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [ProtectedResource("workspaces", "workspaces:write")]
    [EndpointSummary("Cancel a pending invite")]
    [EndpointDescription("The session leader cancels a pending invitation before it is accepted.")]
    public async Task<ActionResult<MemberDO>> UninviteAsync(Guid Id, Guid userId, CancellationToken token)
    {
        var ws = await service.FindByIdAsync(Id, token);
        if (ws is null) return NotFound();
        if (ws.OwnerId is null && !User.IsInRole("staff"))
            return Forbid();
        if (!await userService.ExistsAsync([ userId ], token))
            return NotFound();

        // Verify that requester is the leader.
        var actor = await memberService.FindByEntityAndUserId(Id, User.GetSID(), token);
        if (actor is null || actor.Role is not MemberRole.Leader)
            return Forbid();
        if (actor.Id == userId)
            return UnprocessableEntity();

        var member = await memberService.UnInviteAsync(Id, userId, token);
        return Ok(new MemberDO(member));
    }

    [HttpPost("{id:guid}/invite/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Accept a workspace invite")]
    [EndpointDescription("Accept an invitation to join a workspace, granting access to its projects and resources.")]
    public async Task<ActionResult<MemberDO>> AcceptAsync(Guid Id, CancellationToken token)
    {
        var ws = await service.FindByIdAsync(Id, token);
        if (ws is null) return NotFound();
        if (ws.OwnerId is null && !User.IsInRole("staff"))
            return Forbid();

        var member = await memberService.FindByEntityAndUserId(Id, User.GetSID(), token);
        if (member is null) return NotFound();

        return Ok(new MemberDO(await memberService.AcceptAsync(member.Id, token)));
    }

    [HttpPost("{id:guid}/invite/decline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Decline a workspace invite")]
    [EndpointDescription("Declines the currently authenticated user's pending invitation to join the workspace.")]
    public async Task<ActionResult<MemberDO>> DeclineAsync(Guid Id, CancellationToken token)
    {
        var ws = await service.FindByIdAsync(Id, token);
        if (ws is null) return NotFound();
        if (ws.OwnerId is null && !User.IsInRole("staff"))
            return Forbid();

        var member = await memberService.FindByEntityAndUserId(Id, User.GetSID(), token);
        if (member is null) return NotFound();

        return Ok(new MemberDO(await memberService.DeclineAsync(member.Id, token)));
    }

    [HttpPost("{id:guid}/member/leave")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [ProtectedResource("workspaces", "workspaces:write")]
    [EndpointSummary("Leave a workspace")]
    [EndpointDescription("The user leaves a workspace.")]
    public async Task<ActionResult> LeaveAsync(Guid Id, CancellationToken token)
    {
        var ws = await service.FindByIdAsync(Id, token);
        if (ws is null) return NotFound();
        if (ws.OwnerId is null && !User.IsInRole("staff"))
            return Forbid();

        var member = await memberService.FindByEntityAndUserId(Id, User.GetSID(), token);
        if (member is null) return NotFound();

        return Ok(new MemberDO(await memberService.LeaveAsync(member.Id, token)));
    }

    [HttpPost("{id:guid}/member/kick/{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [ProtectedResource("workspaces", "workspaces:write")]
    [EndpointSummary("Kick a member from a workspace")]
    [EndpointDescription("Remove a member from a workspace.")]
    public async Task<ActionResult> KickAsync(Guid Id, Guid memberId, CancellationToken token)
    {
        var ws = await service.FindByIdAsync(Id, token);
        if (ws is null) return NotFound();
        if (ws.OwnerId is null && !User.IsInRole("staff"))
            return Forbid();

        // Verify that requester is the leader.
        var actor = await memberService.FindByEntityAndUserId(Id, User.GetSID(), token);
        if (actor is null || actor.Role is not MemberRole.Leader)
            return Forbid();

        var member = await memberService.KickAsync(memberId, token);
        return Ok(new MemberDO(member));
    }

    [Obsolete("Workspaces do not have a concept of session leadership like projects do")]
    public Task<ActionResult> TransferLeadershipAsync(Guid entityId, Guid newLeaderId, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}
