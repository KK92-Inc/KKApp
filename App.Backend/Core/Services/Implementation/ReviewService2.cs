// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class ReviewService2(DatabaseContext ctx, IRuleService rules) : BaseService<Review>(ctx), IReviewService
{
    private readonly DatabaseContext _context = ctx;

    /// <inheritdoc />
    public async Task<Review> RequestReviewAsync(Guid userProjectId, Guid rubricId, Guid initiatorId, ReviewVariant[] variants, CancellationToken token = default)
    {
        // Implementation steps:
        // 1. Validate that the user project exists and is in a valid state for review
        // 2. Validate that the rubric exists and supports the requested review variants
        // 3. Validate that the initiator is eligible to request the review based on the rubric's rules

        var userProject = await _context.UserProjects
            .Include(up => up.Project)
            .FirstOrDefaultAsync(up => up.Id == userProjectId)
            ?? throw new ServiceException("User project not found.");

        
        ServiceException.ThrowIf(userProject.GitInfo is null, "Project has nothing submitted for review.");
        ServiceException.ThrowIf(userProject.State is not (EntityObjectState.Awaiting or EntityObjectState.Completed), "Associated project not found.");
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<Review> AssignReviewerAsync(Guid reviewId, Guid reviewerId, CancellationToken token = default)
    {

    }

    /// <inheritdoc />
    public async Task<Review> StartReviewAsync(Guid reviewId, CancellationToken token = default)
    {

    }

    /// <inheritdoc />
    public async Task<Review> CompleteReviewAsync(Guid reviewId, CancellationToken token = default)
    {

    }

    /// <inheritdoc />
    public async Task CancelReviewAsync(Guid reviewId, CancellationToken token = default)
    {

    }

    /// <inheritdoc />
    public async Task<IEnumerable<Review>> GetPendingReviewsAsync(
        Guid userProjectId,
        CancellationToken token = default)
    {

    }

    /// <inheritdoc />
    public async Task<IEnumerable<Review>> GetReviewerAssignmentsAsync(
        Guid reviewerId,
        CancellationToken token = default)
    {

    }
}
