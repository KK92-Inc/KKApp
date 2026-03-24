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
        return await context.UserProjects
            .AsNoTracking()
            .Include(up => up.Members)
            .FirstOrDefaultAsync(
                up => up.ProjectId == projectId &&
                up.Members.Any(m => m.UserId == userId &&
                m.Role != UserProjectRole.Pending
        ), token);
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
