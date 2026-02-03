// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Relations;
using App.Backend.Models;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface ISubscriptionService
{
    /// <summary>
    /// Subscribe a user to a goal.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="request">The subscription request.</param>
    /// <returns>The created user goal.</returns>
    public Task<UserGoal> SubscribeToGoalAsync(Guid userId, Guid goalId, CancellationToken token);

    /// <summary>
    /// Subscribe a user to a project.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public Task<UserProject> SubscribeToProjectAsync(Guid userId, Guid projectId, CancellationToken token);

    /// <summary>
    /// Subscribe a user to a cursus.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cursusId"></param>
    /// <returns></returns>
    public Task<UserCursus> SubscribeToCursusAsync(Guid userId, Guid cursusId, CancellationToken token);

    /// <summary>
    /// Unsubscribe a user from a project.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public Task UnsubscribeFromProjectAsync(Guid userId, Guid projectId, CancellationToken token);

    /// <summary>
    /// Unsubscribe a user from a goal.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="goalId"></param>
    /// <returns></returns>
    public Task UnsubscribeFromGoalAsync(Guid userId, Guid goalId, CancellationToken token);

    /// <summary>
    /// Unsubscribe a user from a cursus.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cursusId"></param>
    /// <returns></returns>
    public Task UnsubscribeFromCursusAsync(Guid userId, Guid cursusId, CancellationToken token);
}
