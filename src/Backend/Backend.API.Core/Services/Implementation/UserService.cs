// ============================================================================
// Copyright (c) 2025 - W2Inc.
// See README.md in the project root for license information.
// ============================================================================

using Backend.API.Infrastructure;
using Backend.API.Core.Services.Interface;
using Backend.API.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace Backend.API.Core.Services.Implementation;

public class UserService(DatabaseContext context) : BaseService<User>(context), IUserService
{
    public async Task<User?> FindByLoginAsync(string login)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<User?> FindByNameAsync(string displayName)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Display == displayName);
    }
}
