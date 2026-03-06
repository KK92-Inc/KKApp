// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.API.Params;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Users;
using App.Backend.Models.Responses.Entities.Reviews;

// ============================================================================

namespace App.Backend.API.Controllers;

/// <summary>
/// Operations for the currently authenticated user.
/// For general user operations (admin/staff), see <see cref="UserController"/>.
/// </summary>
[ApiController]
[Route("reviews"), Tags("Reviews")]
[Authorize]
public class ReviewController(
    ILogger<ReviewController> log,
    IReviewService reviews,
    IUserProjectService userProjects
) : Controller
{
    [HttpGet("{userProjectId:guid}")]
    [HttpGet("{userId:guid}/{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Added for better REST compliance
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get reviews for a specific user and project")]
    [EndpointDescription("Returns the reviews for the currently authenticated user and the specified project.")]
    public async Task<ActionResult<IEnumerable<ReviewDO>>> GetReviewsOnUserProject(
        Guid? userProjectId,
        Guid? userId,
        Guid? projectId,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken token
    )
    {
        UserProject? session = null;
        if (userProjectId.HasValue)
            session = await userProjects.FindByIdAsync(userProjectId.Value);
        else if (userId.HasValue && projectId.HasValue)
            session = await userProjects.FindByUserAndProjectAsync(userId.Value, projectId.Value);

        if (session is null)
            return NotFound("The specified user-project relationship was not found.");

        // 3. Fetch the reviews using the session ID
        var page = await reviews.GetAllAsync(sorting, pagination, token,
            r => r.UserProjectId == session.Id
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(r => new ReviewDO(r)));
    }

    [HttpPut("{reviewId:guid}/assign/{reviewerId:guid}")]
    [EndpointSummary("Assign a reviewer to a pending review")]
    public async Task<ActionResult<ReviewDO>> AssignReviewer(Guid reviewId, Guid reviewerId, CancellationToken token)
    {
        var review = await reviews.AssignReviewerAsync(reviewId, reviewerId, token);
        return Ok(new ReviewDO(review));
    }

    [HttpPost("{reviewId:guid}/start")]
    [EndpointSummary("Transition a review to InProgress")]
    public async Task<ActionResult<ReviewDO>> StartReview(Guid reviewId, CancellationToken token)
    {
        var review = await reviews.StartReviewAsync(reviewId, token);
        return Ok(new ReviewDO(review));
    }

    [HttpPost("{reviewId:guid}/complete")]
    [EndpointSummary("Transition a review to Finished")]
    public async Task<ActionResult<ReviewDO>> CompleteReview(Guid reviewId, CancellationToken token)
    {
        var review = await reviews.CompleteReviewAsync(reviewId, token);
        return Ok(new ReviewDO(review));
    }

    [HttpDelete("{reviewId:guid}")]
    [EndpointSummary("Cancel a review")]
    public async Task<ActionResult<ReviewDO>> CancelReview(Guid reviewId, CancellationToken token)
    {

        throw new NotImplementedException("Canceling reviews is not implemented yet.");
        // var review = await reviews.CancelReviewAsync(reviewId, token);
        // return Ok(new ReviewDO(review));
    }
}
