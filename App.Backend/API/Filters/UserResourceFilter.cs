// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
using NXTBackend.API.Core.Services.Interface;
using Wolverine;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities.Notifications;
using App.Backend.API.Notifications;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Entities;

namespace App.Backend.API.Filters;

// ============================================================================

/// <summary>
/// Action filter that ensures a user record exists in the database for the
/// authenticated Keycloak user. Implements Just-In-Time (JIT) provisioning.
/// </summary>
public class UserResourceFilter(
    DatabaseContext ctx,
    ILogger<UserResourceFilter> logger,
    IMessageBus bus
) : IAsyncResourceFilter
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

        // NOTE(W2): Sync with Keycloak
        // TODO: Set this up with webhooks ideally to avoid constant queries
        if (await ctx.Users.FirstOrDefaultAsync(u => u.Id == userId) is null)
        {
            var first = user.FindFirstValue(ClaimTypes.GivenName);
            var last = user.FindFirstValue(ClaimTypes.Surname);
            var login = user.FindFirstValue("preferred_username") ?? $"user_{userId:N}"[..16];

            var newUser = new User()
            {
                Id = userId,
                Login = login,
                Display = login,
                Details = new()
                {
                    UserId = userId,
                    Email = user.FindFirstValue(ClaimTypes.Email),
                    FirstName = first,
                    LastName = last
                }
            };

            var newWorkspace = new Workspace()
            {
                OwnerId = userId,
                Ownership = EntityOwnership.User
            };

            await ctx.Users.AddAsync(newUser);
            await ctx.Workspaces.AddAsync(newWorkspace);
            await ctx.SaveChangesAsync();

            logger.LogInformation("Creating new user: {Login}", login);

            var welcomeFirst = first ?? login;
            var welcomeLast = last ?? string.Empty;
            await bus.PublishAsync(new WelcomeUserNotification(userId, welcomeFirst, welcomeLast));
        }

        await next();
    }
}
