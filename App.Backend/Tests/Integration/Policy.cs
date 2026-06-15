// Tests/Integration/Infrastructure/FakePolicyEvaluator.cs

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace App.Backend.Tests.Integration;

public class FakePolicyEvaluator : IPolicyEvaluator
{
    public async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var claims = new List<Claim>();

        if (context.Request.Headers.TryGetValue("X-Test-UserId", out var userId))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

        if (context.Request.Headers.TryGetValue("X-Test-Roles", out var roles))
            claims.AddRange(roles.ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => new Claim(ClaimTypes.Role, r.Trim())));

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        context.User = principal;

        return AuthenticateResult.Success(new AuthenticationTicket(principal, "Test"));
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
    {
        return Task.FromResult(PolicyAuthorizationResult.Success());
    }
}