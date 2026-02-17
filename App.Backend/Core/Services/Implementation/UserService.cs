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

public class UserService(DatabaseContext ctx) : BaseService<User>(ctx), IUserService
{
    public async Task<User?> FindByLoginAsync(string login, CancellationToken token = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<User?> FindByNameAsync(string displayName, CancellationToken token = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Display == displayName);
    }
}
