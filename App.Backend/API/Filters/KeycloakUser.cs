// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Security.Claims;
using App.Backend.Core.Services.Interface;
using App.Backend.Database;
using App.Backend.Domain.Entities.Users;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace App.Backend.API.Filters;

// ============================================================================

/// <summary>
/// Action filter that ensures a user record exists in the database for the
/// authenticated Keycloak user. Implements Just-In-Time (JIT) provisioning.
/// </summary>
public class KeycloakUser(DatabaseContext db, ILogger<KeycloakUser> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        // Skip if not authenticated
        if (user.Identity?.IsAuthenticated is not true)
        {
            await next();
            return;
        }

        // Get the user's subject ID from Keycloak (this is the `sub` claim)
        var subClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subClaim, out var userId))
        {
            await next();
            return;
        }

        // Check if user exists (use a fast EXISTS query)
        var exists = await db.Users.AnyAsync(u => u.Id == userId);
        if (!exists)
        {
            // Extract claims from the JWT
            var login = user.FindFirstValue("preferred_username")
                     ?? user.FindFirstValue(ClaimTypes.Name)
                     ?? user.FindFirstValue(ClaimTypes.Email)
                     ?? $"user_{userId:N}"[..16];

            var displayName = user.FindFirstValue("name")
                           ?? user.FindFirstValue(ClaimTypes.GivenName);

            var email = user.FindFirstValue(ClaimTypes.Email);

            // Create the user
            var newUser = new User
            {
                Id = userId,
                Login = login,
                Display = displayName,
                AvatarUrl = null,
            };

            db.Users.Add(newUser);

            try
            {
                await db.SaveChangesAsync();
                logger.LogInformation("Provisioned new user {UserId} with login {Login}", userId, login);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // Race condition: another request created the user simultaneously
                // This is fine, just log and continue
                logger.LogDebug("User {UserId} was created by concurrent request", userId);
                db.Entry(newUser).State = EntityState.Detached;
            }
        }

        await next();
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // PostgreSQL unique constraint violation code is 23505
        return ex.InnerException?.Message.Contains("23505") == true
            || ex.InnerException?.Message.Contains("duplicate key") == true;
    }
}
