// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class CursusService(DatabaseContext ctx) : BaseService<Cursus>(ctx), ICursusService
{
    private readonly DatabaseContext context = ctx;

    public async Task<Cursus?> FindBySlug(string slug)
    {
        await context.Comments.ToListAsync();
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Project>> GetCursusGoals(Guid goalId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Project>> GetCursusProjects(Guid cursusId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> GetCursusUsers(Guid cursusId)
    {
        throw new NotImplementedException();
    }
}