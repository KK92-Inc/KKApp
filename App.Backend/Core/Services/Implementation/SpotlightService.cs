// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Enums;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using App.Backend.Core.Query;
using System.Linq.Expressions;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class SpotlightService(DatabaseContext ctx) : BaseService<Spotlight>(ctx), ISpotlightService
{
    private readonly DatabaseContext context = ctx;

    public override async Task<PaginatedList<Spotlight>> GetAllAsync(ISorting sorting, IPagination pagination, CancellationToken token = default, params Expression<Func<Spotlight, bool>>?[] filters)
    {
        return await filters
            .Where(f => f is not null)
            .Aggregate(_dbSet.AsQueryable(), (c, filter) => c.Where(filter!))
            .Where(s => !context.SpotlightDismissals.Any(d => d.SpotlightId == s.Id))
            .Sort(sorting)
            .PaginateAsync(pagination, token);
    }

    /// <inheritdoc />
    public async Task Dismiss(Spotlight target, Guid userId, CancellationToken token = default)
    {
        await context.SpotlightDismissals.AddAsync(new ()
        {
            SpotlightId = target.Id,
            UserId = userId
        }, token);

        await context.SaveChangesAsync(token);
    }
}
