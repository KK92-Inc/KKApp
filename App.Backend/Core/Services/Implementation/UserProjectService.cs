// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
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

public class UserProjectService(DatabaseContext ctx) : BaseService<UserProject>(ctx), IUserProjectService
{
    private readonly DatabaseContext context = ctx;

    // public async Task<UserProjectTransaction?> RecordAsync(Guid upId, Guid? userId, UserProjectTransactionVariant type)
    // {
    //     var transaction = await ctx.UserProjectTransactions.AddAsync(new UserProjectTransaction()
    //     {
    //         UserId = userId,
    //         UserProjectId = upId,
    //         Type = type,
    //     });

    //     await ctx.SaveChangesAsync();
    //     return transaction.Entity;
    // }
}