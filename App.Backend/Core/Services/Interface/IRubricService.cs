// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;
using App.Backend.Models.Requests.Rubrics;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IRubricService : IDomainService<Rubric>, ISlugQueryable<Rubric>
{
    public Task<Rubric> UpdateRubricAsync(Rubric entity, IEnumerable<(ReviewKinds Kind, int Count)>? variants = null, CancellationToken token = default);
    public Task<Rubric?> FindByProjectId(Guid projectId, CancellationToken token = default);
}
