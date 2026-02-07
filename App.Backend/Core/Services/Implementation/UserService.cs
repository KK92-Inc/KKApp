// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Users;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Entities.Projects;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

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
