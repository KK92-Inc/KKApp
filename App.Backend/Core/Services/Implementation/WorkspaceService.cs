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
using App.Backend.Domain.Entities;
using System.Net;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class WorkspaceService(DatabaseContext ctx) : BaseService<Workspace>(ctx), IWorkspaceService
{
    private readonly DatabaseContext _context = ctx;

    public async Task<Workspace?> FindByUserId(Guid id)
    {
        return await _context.Workspaces
            .FirstOrDefaultAsync(w => w.OwnerId == id && w.Ownership == EntityOwnership.User);
    }

    public async Task<Workspace> GetRootWorkspace()
    {
        var root = await _context.Workspaces.FirstOrDefaultAsync(w => w.Ownership == EntityOwnership.Organization);
        return root ?? throw new ServiceException(501, "Environment is missing a root workspace");
    }
}
