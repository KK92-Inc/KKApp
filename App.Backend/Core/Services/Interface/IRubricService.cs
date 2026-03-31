// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Reviews;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IRubricService : IDomainService<Rubric>, ISlugQueryable<Rubric>
{
    /// <summary>
    /// Check if RUBRIC.md exists in the rubric's git repository.
    /// </summary>
    /// <param name="rubricId">The rubric ID.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>True if RUBRIC.md exists, false otherwise.</returns>
    public Task<bool> HasRubricMarkdownAsync(Guid rubricId, CancellationToken token = default);
}
