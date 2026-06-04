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
    public Task<string?> RotateClientSecretAsync(Guid id, CancellationToken token = default);
}
