// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Security.Claims;
using App.Backend.Core;
using Keycloak.AuthServices.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ModelContextProtocol;

// ============================================================================

namespace App.Backend.API.MCP.Tools;

public abstract class MCPTool(IHttpContextAccessor http, IOptions<McpToolOptions> options, IAuthorizationService auth, ILogger log)
{
    protected McpToolOptions Options => options.Value;
    protected ClaimsPrincipal Caller => http.HttpContext?.User ?? throw new McpException("No authenticated caller.");

    /// <summary>
    /// Refuse unless the server is configured to allow this class of action.
    /// </summary>
    /// <param name="needed"></param>
    /// <exception cref="McpException"></exception>
    protected void Require(McpMode needed)
    {
        if (Options.Mode < needed)
            throw new McpException(
                $"The server is running in '{Options.Mode}' mode, this tool needs '{needed}'. " +
                "A human has to raise Mcp:Mode in the backend configuration.");
    }

    /// <summary>Same UMA check the controllers' [ProtectedResource] attribute performs.</summary>
    protected async Task RequirePermission(string resource, string scope)
    {
        var result = await auth.AuthorizeAsync(Caller, null, new DecisionRequirement(resource, scope));
        if (!result.Succeeded)
            throw new McpException($"Forbidden: you lack the '{scope}' permission.");
    }

    protected void Audit(string tool, object args) =>
        log.LogWarning("MCP audit: caller={Caller} tool={Tool} args={@Args}",
            Caller.FindFirstValue("preferred_username") ?? Caller.FindFirstValue(ClaimTypes.NameIdentifier),
            tool, args);

    /// <summary>
    /// McpException messages reach the model, other exceptions are masked.
    /// Translate ServiceException so the model sees "User not found" instead of "an error occurred".
    /// </summary>
    protected static async Task<T> Safe<T>(Func<Task<T>> action)
    {
        try { return await action(); }
        catch (ServiceException e) { throw new McpException(e.Message, e); }
    }
}
