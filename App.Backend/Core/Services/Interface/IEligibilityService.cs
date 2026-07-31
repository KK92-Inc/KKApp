using System.Linq.Expressions;
using App.Backend.Core.Query;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

namespace App.Backend.Core.Services.Interface;

/// <summary>
/// Evaluates whether users are eligible to interact with or subscribe to domain entities.
/// </summary>
public interface IEligibilityService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cursusId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task EligibleForCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="goalId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task EligibleForGoalAsync(Guid userId, Guid goalId, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="projectId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task EligibleForProjectAsync(Guid userId, Guid projectId, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="rubricId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task EligibleForRubricAsync(Guid userId, Guid rubricId, CancellationToken token = default);

    /// <summary>
    /// Filters users who are eligible for a certain entity.
    /// Eligiblity refers to act with the entity like for example a project
    /// is to subscribe same for goals or cursi.
    /// 
    /// While for entities like rubrics it means can they review with it.
    /// It depends on the context of what the purpose the entity serves.
    /// </summary>
    /// <param name="id">The entity id.</param>
    /// <param name="type">The entity type to narrow it down.</param>
    /// <param name="sorting">The sorting options.</param>
    /// <param name="pagination">The pagination options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="filters">The filters to apply.</param>
    /// <returns>A paginated list of entities.</returns>
    Task<PaginatedList<User>> GetAllEligibleAsync(
        Guid id,
        EntityType type,
        ISorting sorting,
        IPagination pagination,
        CancellationToken token = default,
        params Expression<Func<User, bool>>?[] filters
    );
}