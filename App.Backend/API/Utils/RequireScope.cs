// ============================================================================
// Copyright (c) 2024 - W2Wizard.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Authorization;

// ============================================================================

namespace App.Backend.API.Utils;

/// <summary>
/// Attribute to limit access to clients if they don't have the scope.
/// First Party Clients are excluded.
/// </summary>
public class RequireScopeAttribute : AuthorizeAttribute
{
    public RequireScopeAttribute(string scope) => Policy = $"scope:{scope}";
}

public class RequireScopeRequirement(string scope) : IAuthorizationRequirement
{
    public string Scope { get; } = scope;
}

public class RequireScopeHandler : AuthorizationHandler<RequireScopeRequirement>
{
    // TODO: Make it Configurable / retrieve via options.
    private static readonly HashSet<string> FirstPartyClients = ["intra"];

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext ctx, RequireScopeRequirement requirement)
    {
        var azp = ctx.User.FindFirst("azp")?.Value;
        Console.WriteLine(azp);
        if (azp is not null && FirstPartyClients.Contains(azp))
        {
            ctx.Succeed(requirement);
            return Task.CompletedTask;
        }

        var scopes = ctx.User.FindFirst("scope")?.Value?.Split(' ') ?? [];
        if (scopes.Contains(requirement.Scope))
            ctx.Succeed(requirement);

        return Task.CompletedTask;
    }
}