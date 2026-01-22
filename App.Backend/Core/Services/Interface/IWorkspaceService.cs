// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IWorkspaceService : IDomainService<Workspace>
{
    /// <summary>
    /// The root workspace is where all the official / system wide
    /// cursi, goals and projects reside.
    ///
    /// For example projects curated by staff/faculty are placed here.
    /// </summary>
    /// <returns>The root workspace.</returns>
    public Task<Workspace> GetRootWorkspace();

    /// <summary>
    /// Find the workspace by user id.
    /// </summary>
    /// <param name="login">The login.</param>
    /// <returns>The user.</returns>
    public Task<Workspace?> FindByUserId(Guid id);
}
