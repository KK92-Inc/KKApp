// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Relations;
using App.Backend.Models;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

/// <summary>
/// Manages user subscriptions to various domain entities like Goals, Projects, and Cursuses.
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Subscribes a user to a goal.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="goalId">The unique identifier of the goal.</param>
    /// <param name="token">A token to monitor for cancellation requests.</param>
    /// <returns>The created user goal.</returns>
    Task<UserGoal> SubscribeToGoalAsync(Guid userId, Guid goalId, CancellationToken token = default);

    /// <summary>
    /// Subscribes a user to a project.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="token">A token to monitor for cancellation requests.</param>
    /// <returns>The created user project.</returns>
    Task<UserProject> SubscribeToProjectAsync(Guid userId, Guid projectId, CancellationToken token = default);

    /// <summary>
    /// Subscribes a user to a cursus.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cursusId">The unique identifier of the cursus.</param>
    /// <param name="token">A token to monitor for cancellation requests.</param>
    /// <returns>The created user cursus.</returns>
    Task<UserCursus> SubscribeToCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default);

    /// <summary>
    /// Unsubscribes a user from a project.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="token">A token to monitor for cancellation requests.</param>
    Task<UserProject> UnsubscribeFromProjectAsync(Guid userId, Guid projectId, CancellationToken token = default);

    /// <summary>
    /// Unsubscribes a user from a goal.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="goalId">The unique identifier of the goal.</param>
    /// <param name="token">A token to monitor for cancellation requests.</param>
    Task<UserGoal> UnsubscribeFromGoalAsync(Guid userId, Guid goalId, CancellationToken token = default);

    /// <summary>
    /// Unsubscribes a user from a cursus.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cursusId">The unique identifier of the cursus.</param>
    /// <param name="token">A token to monitor for cancellation requests.</param>
    Task<UserCursus> UnsubscribeFromCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default);
}