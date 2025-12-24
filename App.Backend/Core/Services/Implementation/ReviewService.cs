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
using App.Backend.Domain.Entities.Reviews;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class ReviewService(DatabaseContext ctx) : BaseService<Review>(ctx), IReviewService
{
    private readonly DatabaseContext context = ctx;

    /// <summary>
    /// Create a new review.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public override async Task<Review> CreateAsync(Review entity, CancellationToken token = default)
    {
        // 1. Creating a review needs to:
        //   - Ensure that no existing review of the same kind exists for the user project.
    }


    // public async Task<Cursus?> FindBySlug(string slug)
    // {
    //     await context.Comments.ToListAsync();
    //     throw new NotImplementedException();
    // }

    // public Task<IEnumerable<Project>> GetCursusGoals(Guid goalId)
    // {
    //     throw new NotImplementedException();
    // }

    // public Task<IEnumerable<Project>> GetCursusProjects(Guid cursusId)
    // {
    //     throw new NotImplementedException();
    // }

    // public Task<IEnumerable<User>> GetCursusUsers(Guid cursusId)
    // {
    //     throw new NotImplementedException();
    // }
}