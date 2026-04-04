// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Entities.Projects;
using App.Backend.Core.Query;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class UserProjectService(DatabaseContext ctx) : BaseService<UserProject>(ctx), IUserProjectService
{
    private readonly DatabaseContext context = ctx;

    public async Task<UserProject?> FindByUserAndProjectAsync(Guid userId, Guid projectId, CancellationToken token = default)
    {
        var result = await context.UserProjects
            .AsNoTracking()
            .Join(context.Members.AsNoTracking(),
                userProject => userProject.Id,
                member => member.EntityId,
                (userProject, member) => new { userProject, member })
            .FirstOrDefaultAsync(
                joined => joined.userProject.ProjectId == projectId &&
                joined.member.UserId == userId &&
                joined.member.EntityType == MemberEntityType.UserProject,
            token);

        return result?.userProject;
    }

    public async Task<PaginatedList<UserProjectTransaction>> GetTransactionsAsync(Guid Id, ISorting sorting, IPagination pagination, CancellationToken token = default)
    {
        return await context.UserProjectTransactions
            .Where(t => t.UserProjectId == Id)
            .Include(t => t.User)
            .AsNoTracking()
            .Sort(sorting)
            .PaginateAsync(pagination, token);
    }
}
