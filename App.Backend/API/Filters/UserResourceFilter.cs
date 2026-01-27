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
using App.Backend.Core.Services.Interface;
using App.Backend.API.Notifications.Variants;

namespace App.Backend.API.Filters;

// ============================================================================

/// <summary>
/// Action filter that ensures a user record exists in the database for the
/// authenticated Keycloak user. Implements Just-In-Time (JIT) provisioning.
/// </summary>
public class UserResourceFilter(
    IUserService users,
    IWorkspaceService workspaces,
    ILogger<UserResourceFilter> logger,
    IMessageBus bus
) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        var token = context.HttpContext.RequestAborted;
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
        if (await users.FindByIdAsync(userId, token) is null)
        {
            var first = user.FindFirstValue(ClaimTypes.GivenName);
            var last = user.FindFirstValue(ClaimTypes.Surname);
            var login = user.FindFirstValue("preferred_username")!;

            await users.CreateAsync(new ()
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
                },
            }, token);

            await workspaces.CreateAsync(new ()
            {
                OwnerId = userId,
                Ownership = EntityOwnership.User
            }, token);

            logger.LogInformation("Creating new user: {Login}", login);

            var firstName = first ?? login;
            var lastName = last ?? string.Empty;
            await bus.PublishAsync(new WelcomeUserNotification(userId, firstName, lastName));
        }

        await next();
    }
}
