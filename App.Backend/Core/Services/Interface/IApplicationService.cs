// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Reviews;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IApplicationService : IDomainService<Application>
{
    /// <summary>
    /// Get applications the user consented to.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<List<Application>> GetConsentedApplicationsAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// Rotate the secret for a client to a new one.
    /// </summary>
    /// <param name="id">The application ID</param>
    /// <param name="token">The cancellation token</param>
    /// <returns></returns>
    public Task<string> RotateClientSecretAsync(Application entity, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task RevokeAccess(Application entity, Guid userId, CancellationToken token = default);
}
