// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;
using App.Backend.Models;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface ISpotlightService : IDomainService<Spotlight>
{
    /// <summary>
    /// Compute the cursus track for a user
    /// </summary>
    /// <param name="cursusId">The cursus ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="token">Cancellation token.</param>
    public Task Dismiss(Spotlight target, Guid userId, CancellationToken token = default);
}
