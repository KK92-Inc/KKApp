// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.Linq.Expressions;
using System.Security.Claims;
using App.Backend.API.Bus.Messages.Freeze;
using App.Backend.API.Params;
using App.Backend.Core;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using Keycloak.AuthServices.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Wolverine;

// ============================================================================

namespace App.Backend.API.MCP.Tools;

[McpServerToolType]

public sealed class UserTools(IHttpContextAccessor http, ILogger<UserTools> log) : MCPTool(http, log)
{
    [McpServerTool(Name = "whoami", ReadOnly = true)]
    [Description("Shows who the MCP server thinks you are (token subject, roles, scopes) and the current server mode. Use first when debugging.")]
    public WhoAmIResult WhoAmI()
    {
        return new(
        Caller.FindFirstValue(ClaimTypes.NameIdentifier),
        Caller.FindFirstValue("admin_user_id"),
        Caller.FindFirstValue("preferred_username"),
        Caller.FindFirstValue("azp"),
        Caller.FindAll(ClaimTypes.Role).Select(c => c.Value).Order().ToArray(),
        Caller.FindFirstValue("scope"),
        options.Value.Mode.ToString());
    }
}