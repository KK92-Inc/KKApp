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
    /// <summary>
    /// Sets the variants for a rubric. This will replace all existing variants with the provided ones.
    /// Variants state what review kinds are supported and how many of each kind are required.
    /// Kinds omitted from the input are treated as non-required (count = 0).
    /// </summary>
    /// <param name="rubricId"></param>
    /// <param name="variants"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Rubric?> SetVariantsAsync(
        Guid rubricId,
        IEnumerable<(ReviewKinds Kind, int Required)> variants,
        CancellationToken token = default
    );
}
