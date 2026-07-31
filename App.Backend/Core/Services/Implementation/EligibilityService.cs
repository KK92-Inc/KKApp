using System.Linq.Expressions;
using App.Backend.Core.Query;
using App.Backend.Core.Services.Interface;
using App.Backend.Core.Services.Options;
using App.Backend.Database;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace App.Backend.Core.Services.Implementation;

public class EligibilityService(
    DatabaseContext context,
    TimeProvider time,
    IUserCursusService userCursusService,
    IOptions<SubscriptionOptions> options
) : IEligibilityService
{
    private readonly SubscriptionOptions _config = options.Value;

    public async Task EligibleForCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default)
    {
        var existing = await context.UserCursi.FirstOrDefaultAsync(
            uc => uc.UserId == userId && uc.CursusId == cursusId,
            token
        );

        if (existing is not null)
        {
            CheckLock(existing.UnlocksAt);
            if (existing.State is not EntityObjectState.Inactive)
            {
                throw existing.State switch
                {
                    EntityObjectState.Active => new ServiceException("Already subscribed to this cursus."),
                    EntityObjectState.Completed => new ServiceException("Cannot resubscribe to a completed cursus."),
                    EntityObjectState.Awaiting => new ServiceException("Subscription is awaiting approval."),
                    _ => new ServiceException("Invalid subscription state.")
                };
            }
        }
    }

    public async Task EligibleForGoalAsync(Guid userId, Guid goalId, CancellationToken token = default)
    {
        var existing = await context.UserGoals.FirstOrDefaultAsync(
            ug => ug.GoalId == goalId && ug.UserId == userId,
            token
        );

        if (existing is not null)
        {
            CheckLock(existing.UnlocksAt);
            ServiceException.ThrowIf(
                existing.State is not EntityObjectState.Inactive,
                "Already subscribed to this goal."
            );
        }

        // Hierarchy Restriction Check (Enforced only in Restricted mode)
        if (_config.Mode is ProgressionMode.Restricted)
        {
            var cursiWithGoal = context.CursusGoal
                .Where(cg => cg.GoalId == goalId)
                .Select(gp => gp.CursusId);

            if (await cursiWithGoal.AnyAsync(token))
            {
                var activeUserCursi = await (
                    from uc in context.UserCursi
                    join c in context.Cursi on uc.CursusId equals c.Id
                    where uc.UserId == userId
                       && uc.State != EntityObjectState.Inactive
                       && cursiWithGoal.Contains(uc.CursusId)
                    select new { UserCursus = uc, Cursus = c }
                ).ToListAsync(token);

                ServiceException.ThrowIf(
                    activeUserCursi.Count == 0,
                    "This goal is not a standalone goal. Subscribe to a cursus that contains this goal to access it."
                );

                bool isUnlockedInAnyCursus = false;
                foreach (var item in activeUserCursi)
                {
                    var (snapshot, states) = await userCursusService.GetTrackAsync(item.UserCursus.Id, userId, token);
                    var track = userCursusService.AssembleTrack(item.Cursus, snapshot, states);

                    var trackNode = track.Nodes.FirstOrDefault(n => n.GoalId == goalId);
                    if (trackNode is not null && trackNode.IsUnlocked)
                    {
                        isUnlockedInAnyCursus = true;
                        break;
                    }
                }

                ServiceException.ThrowIf(!isUnlockedInAnyCursus, "This goal is currently locked. Complete its prerequisites within your cursus to unlock it.");
            }
        }
    }

    public async Task EligibleForProjectAsync(Guid userId, Guid projectId, CancellationToken token = default)
    {
        var existing = await context.UserProjects.Where(
            up => up.ProjectId == projectId &&
            context.Members.Any(
                m => m.EntityType == MemberEntityType.UserProject &&
                m.EntityId == up.Id &&
                m.UserId == userId &&
                m.Role != MemberRole.Pending
            )
        ).FirstOrDefaultAsync(token);

        if (existing?.State is EntityObjectState.Completed)
            throw new ServiceException("Cannot resubscribe to a completed project.");

        if (existing is not null)
        {
            CheckLock(existing.UnlocksAt);
            ServiceException.ThrowIf(
                existing.State is not EntityObjectState.Inactive,
                "Already subscribed to this project."
            );
        }

        // Hierarchy Restriction check (Enforced only in Restricted mode)
        if (_config.Mode is ProgressionMode.Restricted)
        {
            var goalsWithProject = context.GoalProject
                .Where(gp => gp.ProjectId == projectId)
                .Select(gp => gp.GoalId);

            if (await goalsWithProject.AnyAsync(token))
            {
                bool subscribed = await context.UserGoals.AnyAsync(
                    ug => ug.UserId == userId &&
                    ug.State != EntityObjectState.Inactive &&
                    goalsWithProject.Contains(ug.GoalId),
                    token
                );

                ServiceException.ThrowIf(!subscribed, "Please subscribe to a goal with this project to access it.");
            }
        }
    }

    public async Task EligibleForRubricAsync(Guid userId, Guid rubricId, CancellationToken token = default)
    {
        // TODO: Wildcard and project-specific rubrics currently evaluate to true.
        // Future evaluation rule engine logic hooks in right here.
        await Task.CompletedTask;
    }

    public async Task<PaginatedList<User>> GetAllEligibleAsync(
        Guid id,
        EntityType type,
        ISorting sorting,
        IPagination pagination,
        CancellationToken token = default,
        params Expression<Func<User, bool>>?[] filters
    )
    {
        var now = time.GetUtcNow();
        var query = context.Users.AsNoTracking();
        query = type switch
        {
            EntityType.Cursus => ApplyCursusEligibility(query, id, now),
            EntityType.Goal => await ApplyGoalEligibilityAsync(query, id, now, token),
            EntityType.Project => await ApplyProjectEligibilityAsync(query, id, now, token),
            EntityType.Rubric => ApplyRubricEligibility(query, id),
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Unsupported entity type for eligibility.")
        };

        foreach (var filter in filters.Where(f => f is not null))
            query = query.Where(filter!);

        // Apply sorting and pagination via EF Core extensions
        return await query
            .Sort(sorting)
            .PaginateAsync(pagination, token);
    }

    // ========================================================================

    private IQueryable<User> ApplyCursusEligibility(IQueryable<User> query, Guid cursusId, DateTimeOffset now)
    {
        return query.Where(u => !context.UserCursi.Any(uc =>
            uc.UserId == u.Id &&
            uc.CursusId == cursusId &&
            (
                uc.State != EntityObjectState.Inactive ||
                (uc.UnlocksAt.HasValue && uc.UnlocksAt > now)
            )
        ));
    }

    private async Task<IQueryable<User>> ApplyGoalEligibilityAsync(
        IQueryable<User> query,
        Guid goalId,
        DateTimeOffset now,
        CancellationToken token
    )
    {
        query = query.Where(u => !context.UserGoals.Any(ug =>
            ug.UserId == u.Id &&
            ug.GoalId == goalId &&
            (
                ug.State != EntityObjectState.Inactive ||
                (ug.UnlocksAt.HasValue && ug.UnlocksAt > now)
            )
        ));

        if (_config.Mode is ProgressionMode.Restricted)
        {
            var parentCursusIds = await context.CursusGoal
                .Where(cg => cg.GoalId == goalId)
                .Select(cg => cg.CursusId)
                .ToListAsync(token);

            if (parentCursusIds.Count > 0)
            {
                query = query.Where(u => context.UserCursi.Any(uc =>
                    uc.UserId == u.Id &&
                    uc.State != EntityObjectState.Inactive &&
                    parentCursusIds.Contains(uc.CursusId)
                ));
            }
        }

        return query;
    }

    private async Task<IQueryable<User>> ApplyProjectEligibilityAsync(
        IQueryable<User> query,
        Guid projectId,
        DateTimeOffset now,
        CancellationToken token
    )
    {
        query = query.Where(u => !context.UserProjects.Any(up =>
            up.ProjectId == projectId &&
            context.Members.Any(m =>
                m.EntityType == MemberEntityType.UserProject &&
                m.EntityId == up.Id &&
                m.UserId == u.Id &&
                m.Role != MemberRole.Pending) &&
            (
                up.State != EntityObjectState.Inactive ||
                (up.UnlocksAt.HasValue && up.UnlocksAt > now)
            )
        ));

        if (_config.Mode is ProgressionMode.Restricted)
        {
            var parentGoalIds = await context.GoalProject
                .Where(gp => gp.ProjectId == projectId)
                .Select(gp => gp.GoalId)
                .ToListAsync(token);

            if (parentGoalIds.Count > 0)
            {
                query = query.Where(u => context.UserGoals.Any(ug =>
                    ug.UserId == u.Id &&
                    ug.State != EntityObjectState.Inactive &&
                    parentGoalIds.Contains(ug.GoalId)
                ));
            }
        }

        return query;
    }

    private IQueryable<User> ApplyRubricEligibility(IQueryable<User> query, Guid rubricId)
    {
        // Placeholder for future rule engine filtering
        return query;
    }

    private void CheckLock(DateTimeOffset? unlocksAt)
    {
        var now = time.GetUtcNow();
        if (unlocksAt.HasValue && now < unlocksAt.Value)
        {
            var remaining = unlocksAt.Value - now;
            var formatted = $"{(int)remaining.TotalHours}h {remaining.Minutes}m";
            throw new ServiceException($"Please wait {formatted} before resubscribing");
        }
    }
}