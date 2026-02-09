// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class UserGoalService(DatabaseContext ctx) : BaseService<UserGoal>(ctx), IUserGoalService
{
    public async Task<UserGoal?> FindByUserAndGoalAsync(Guid userId, Guid goalId, CancellationToken token = default)
    {
        return await Query(false).FirstOrDefaultAsync(
            ug => ug.UserId == userId && ug.GoalId == goalId, token
        );
    }
}
