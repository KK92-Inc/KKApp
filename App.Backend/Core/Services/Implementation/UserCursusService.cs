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

public class UserCursusService(DatabaseContext ctx) : BaseService<UserCursus>(ctx), IUserCursusService
{
    public async Task<UserCursus?> FindByUserAndCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default)
    {
        return await Query(false).FirstOrDefaultAsync(
            uc => uc.UserId == userId && uc.CursusId == cursusId, token
        );
    }
}
