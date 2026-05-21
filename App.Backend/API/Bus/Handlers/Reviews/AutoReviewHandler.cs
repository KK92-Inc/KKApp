// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Handlers.Reviews;

using Wolverine.Attributes;
using App.Backend.Core.Services.Interface;
using App.Backend.API.Bus.Messages;
using App.Backend.Database;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using App.Backend.API.Bus.Handlers;

// ============================================================================

[WolverineHandler]
public class AutoReviewHandler(DatabaseContext context, ILogger<NotificationHandler> logger)
{
    public async Task Handle(RequestAutoReview msg, CancellationToken ct)
    {
        // TODO: Auto complete as they are not yet implemented...
        logger.LogDebug("Handler for auto review request received for review {ReviewId} and user project {UserProjectId}", msg.ReviewId, msg.UserProjectId);
        var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == msg.ReviewId, ct)
            ?? throw new InvalidOperationException("Review not found.");

        review.State = ReviewState.Finished;
        await context.SaveChangesAsync(ct);
    }
}
