// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;

namespace App.Backend.API.Filters;

// ============================================================================

/// <summary>
/// Action filter that ensures a user record exists in the database for the
/// authenticated Keycloak user. Implements Just-In-Time (JIT) provisioning.
/// </summary>
public class UserResourceFilter(DatabaseContext ctx, ILogger<UserResourceFilter> logger) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        if (user.FindFirstValue(ClaimTypes.NameIdentifier) is not string sub)
        {
            await next();
            return;
        }

        if (!Guid.TryParse(sub, out var userId))
        {
            await next();
            return;
        }

        if (await ctx.Users.FirstOrDefaultAsync(u => u.Id == userId) is null)
        {
            var login = user.FindFirstValue("preferred_username") ?? $"user_{userId:N}"[..16];
            logger.LogInformation("Creating new user {UserId} with login {Login}", userId, login);
            await ctx.Users.AddAsync(new()
            {
                Id = userId,
                Login = login,
                Display = login,
                Details = new ()
                {
                    Email = user.FindFirstValue(ClaimTypes.Email),
                    FirstName = user.FindFirstValue(ClaimTypes.GivenName),
                    LastName = user.FindFirstValue(ClaimTypes.Surname)
                }
            });

            await ctx.SaveChangesAsync();
        }

        await next();
    }
}
