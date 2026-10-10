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

public abstract class MCPTool(IHttpContextAccessor http, ILogger<MCPTool> log)
{
    protected ClaimsPrincipal Caller => http.HttpContext?.User ?? throw new McpException("No authenticated caller.");

    protected void Audit(string tool, object args) =>
    log.LogWarning("MCP audit: caller={Caller} tool={Tool} args={@Args}",
        Caller.FindFirstValue("preferred_username") ?? Caller.FindFirstValue(ClaimTypes.NameIdentifier), tool, args);
}