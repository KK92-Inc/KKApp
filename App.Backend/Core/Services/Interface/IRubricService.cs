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
    /// Returns the rubric that matches the specified project.
    /// <para>
    /// There can be two possible rubrics at a time:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>A project-specific rubric, where <c>ProjectId</c> equals the project's ID.</description>
    ///   </item>
    ///   <item>
    ///     <description>A default rubric, where <c>ProjectId</c> is <c>null</c>.</description>
    ///   </item>
    /// </list>
    /// <para>
    /// The project-specific rubric takes precedence over the default rubric.
    /// If both exist, only the project-specific rubric is returned.
    /// </para>
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Rubric?> FindByProjectId(Guid projectId, CancellationToken token = default);

    /// <summary>
    /// Sets the variants for a rubric. This will replace all existing variants with the provided ones.
    /// Variants state what review kinds are supported and how many of each kind are required.
    /// Kinds omitted from the input are treated as non-required (count = 0).
    /// </summary>
    /// <param name="rubricId"></param>
    /// <param name="variants"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Rubric> SetVariantsAsync(Guid id, IEnumerable<RubricVariant> variants, CancellationToken token = default);

    /// <summary>
    /// Returns the review variants configured for the specified rubric.
    /// Variants state what review kinds are supported and how many of each kind are required.
    /// </summary>
    /// <param name="rubricId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<IEnumerable<RubricVariant>?> GetVariantsAsync(Guid id, CancellationToken token = default);
}
