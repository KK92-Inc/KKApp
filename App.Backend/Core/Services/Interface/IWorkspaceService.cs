// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Reviews;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IWorkspaceService : IDomainService<Workspace>, IUserQueryable<Workspace>
{
    /// <summary>
    /// The root workspace is where all the official / system wide
    /// cursi, goals and projects reside.
    ///
    /// For example projects curated by staff/faculty are placed here.
    /// </summary>
    /// <returns>The root workspace.</returns>
    public Task<Workspace> GetRootWorkspace(CancellationToken token = default);

    /// <summary>
    /// Creates a new project into the specified workspace.
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="project"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Project> AddProjectAsync(Guid workspaceId, Project project, CancellationToken token = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="project"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Goal> AddGoalAsync(Guid workspaceId, Goal goal, CancellationToken token = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="project"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Cursus> AddCursusAsync(Guid workspaceId, Cursus cursus, CancellationToken token = default);

    /// <summary>
    /// Creates a new rubric with an associated git repository into the specified workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="rubric">The rubric to create.</param>
    /// <param name="creatorId">The creator user ID.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The created rubric.</returns>
    public Task<Rubric> AddRubricAsync(Guid workspaceId, Rubric rubric, Guid creatorId, CancellationToken token = default);

}
