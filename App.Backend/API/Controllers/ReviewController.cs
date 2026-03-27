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
using App.Backend.Models.Requests.Reviews;
using App.Backend.Domain.Enums;
using App.Backend.Database;
using Microsoft.EntityFrameworkCore;
using ImTools;
using App.Backend.Domain.Entities.Reviews;

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
    IReviewService service,
    IUserProjectService userProjects,
    DatabaseContext ctx
) : Controller
{
    // GET /reviews
    // GET /reviews/{reviewId}
    // GET /reviews/user/{userId}/project/{projectId}

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all reviews")]
    [EndpointDescription("Returns a paginated list of reviews")]
    public async Task<ActionResult<IEnumerable<ReviewDO>>> GetReviews(
        [FromQuery(Name = "filter[user_project_id]")] Guid? userProjectId,
        [FromQuery(Name = "filter[reviewer_id]")] Guid? reviewerId,
        [FromQuery(Name = "filter[rubric_id]")] Guid? rubricId,
        [FromQuery(Name = "filter[kind]")] ReviewVariant? kind,
        [FromQuery(Name = "filter[status]")] ReviewState? status,
        [FromQuery] Pagination pagination,
        [FromQuery] Sorting sorting,
        CancellationToken token
    )
    {
        var page = await service.GetAllAsync(sorting, pagination, token,
            r => !userProjectId.HasValue || r.UserProjectId == userProjectId.Value,
            r => !reviewerId.HasValue || r.ReviewerId == reviewerId.Value,
            r => !rubricId.HasValue || r.RubricId == rubricId.Value,
            r => !kind.HasValue || r.Kind == kind.Value,
            r => !status.HasValue || r.State == status.Value
        );

        page.AppendHeaders(Response.Headers);
        return Ok(page.Items.Select(r => new ReviewDO(r)));
    }


    [HttpGet("{reviewId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get a single review by its ID")]
    [EndpointDescription("Returns the review with full details including reviewer and rubric.")]
    public async Task<ActionResult<ReviewDO>> GetReviewById(Guid reviewId, CancellationToken token)
    {
        var review = await service.FindByIdAsync(reviewId, token);
        if (review is null)
            return NotFound("Review not found");
        return Ok(new ReviewDO(review));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Request one or more reviews for a user project")]
    [EndpointDescription("Creates review entries for the specified kinds. Self reviews are auto-assigned to the requesting user.")]
    public async Task<ActionResult<IEnumerable<ReviewDO>>> RequestReviews(
        [FromBody] PostReviewRequestDTO dto,
        CancellationToken token
    )
    {
        var requestingUserId = User.GetSID();
        var reviews = await Task.WhenAll(dto.Kinds.Select(kind =>
            service.RequestReviewAsync(
                dto.UserProjectId,
                dto.RubricId,
                kind,
                requestingUserId,
                token
            )
        ));

        return Ok(reviews.Select(r => new ReviewDO(r)));
    }

    [HttpPost("{reviewId:guid}/assign/{reviewerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Assign a reviewer to a pending review")]
    [EndpointDescription("Assigns the specified user as reviewer for the review. Validates that the reviewer meets the rubric's eligibility requirements.")]
    public async Task<ActionResult<ReviewDO>> AssignReviewer(Guid reviewId, Guid reviewerId, CancellationToken token)
    {
        var review = await service.AssignReviewerAsync(reviewId, reviewerId, token);
        return Ok(new ReviewDO(review));
    }

    [HttpPost("{reviewId:guid}/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Start a review")]
    [EndpointDescription("Transitions the review to InProgress and assigns the current user as the reviewer.")]
    public async Task<ActionResult<ReviewDO>> StartReview(Guid reviewId, CancellationToken token)
    {
        var review = await service.StartReviewAsync(reviewId, token);
        return Ok(new ReviewDO(review));
    }

    [HttpPost("{reviewId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Complete a review")]
    [EndpointDescription("Transitions the review to Finished. The review content should be included in the request body.")]
    public async Task<ActionResult<ReviewDO>> CompleteReview(Guid reviewId, CancellationToken token)
    {
        var review = await service.CompleteReviewAsync(reviewId, token);
        return Ok(new ReviewDO(review));
    }

    [HttpDelete("{reviewId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Cancel a review")]
    [EndpointDescription("Cancels the review with the specified ID.")]
    public async Task<ActionResult> CancelReview(Guid reviewId, CancellationToken token)
    {
        await service.CancelReviewAsync(reviewId, token);
        return NoContent();
    }

    // [HttpGet("{userProjectId:guid}")]
    // [HttpGet("{userId:guid}/{projectId:guid}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)] // Added for better REST compliance
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Get reviews for a specific user and project")]
    // [EndpointDescription("Returns the reviews for the currently authenticated user and the specified project.")]
    // public async Task<ActionResult<IEnumerable<ReviewDO>>> GetReviewsOnUserProject(
    //     Guid? userProjectId,
    //     Guid? userId,
    //     Guid? projectId,
    //     [FromQuery] Pagination pagination,
    //     [FromQuery] Sorting sorting,
    //     CancellationToken token
    // )
    // {
    //     UserProject? session = null;
    //     if (userProjectId.HasValue)
    //         session = await userProjects.FindByIdAsync(userProjectId.Value);
    //     else if (userId.HasValue && projectId.HasValue)
    //         session = await userProjects.FindByUserAndProjectAsync(userId.Value, projectId.Value);

    //     if (session is null)
    //         return NotFound("The specified user-project relationship was not found.");

    //     // 3. Fetch the reviews using the session ID
    //     var page = await reviews.GetAllAsync(sorting, pagination, token,
    //         r => r.UserProjectId == session.Id
    //     );

    //     page.AppendHeaders(Response.Headers);
    //     return Ok(page.Items.Select(r => new ReviewDO(r)));
    // }

    // [HttpGet("rubrics/{userProjectId:guid}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Get available rubrics for a user project")]
    // [EndpointDescription("Returns enabled rubrics that can be used for reviewing the specified user project.")]
    // public async Task<ActionResult<IEnumerable<RubricDO>>> GetRubricsForProject(
    //     Guid userProjectId,
    //     CancellationToken token
    // )
    // {
    //     var up = await userProjects.FindByIdAsync(userProjectId, token);
    //     if (up is null)
    //         return NotFound("User project not found");

    //     // If the user project has a specific rubric assigned, return just that
    //     // Otherwise return all enabled public rubrics
    //     var rubrics = up.RubricId.HasValue
    //         ? await ctx.Rubrics
    //             .Where(r => r.Id == up.RubricId.Value && r.Enabled)
    //             .AsNoTracking()
    //             .ToListAsync(token)
    //         : await ctx.Rubrics
    //             .Where(r => r.Enabled && r.Public)
    //             .AsNoTracking()
    //             .ToListAsync(token);

    //     return Ok(rubrics.Select(r => new RubricDO(r)));
    // }

    // [HttpPut("{reviewId:guid}/assign/{reviewerId:guid}")]
    // [EndpointSummary("Assign a reviewer to a pending review")]
    // public async Task<ActionResult<ReviewDO>> AssignReviewer(Guid reviewId, Guid reviewerId, CancellationToken token)
    // {
    //     var review = await reviews.AssignReviewerAsync(reviewId, reviewerId, token);
    //     return Ok(new ReviewDO(review));
    // }

    // [HttpGet("by-id/{reviewId:guid}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Get a single review by its ID")]
    // [EndpointDescription("Returns the review with full details including reviewer and rubric.")]
    // public async Task<ActionResult<ReviewDO>> GetReviewById(Guid reviewId, CancellationToken token)
    // {
    //     var review = await ctx.Reviews
    //         .Include(r => r.Reviewer)
    //         .Include(r => r.Rubric)
    //         .Include(r => r.UserProject)
    //             .ThenInclude(up => up.Project)
    //         .AsNoTracking()
    //         .FirstOrDefaultAsync(r => r.Id == reviewId, token);

    //     if (review is null)
    //         return NotFound("Review not found");

    //     return Ok(new ReviewDO(review));
    // }

    // [HttpPost("{reviewId:guid}/start")]
    // [EndpointSummary("Transition a review to InProgress")]
    // public async Task<ActionResult<ReviewDO>> StartReview(Guid reviewId, CancellationToken token)
    // {
    //     var review = await reviews.StartReviewAsync(reviewId, token);
    //     return Ok(new ReviewDO(review));
    // }

    // [HttpPost("{reviewId:guid}/complete")]
    // [EndpointSummary("Transition a review to Finished")]
    // public async Task<ActionResult<ReviewDO>> CompleteReview(Guid reviewId, CancellationToken token)
    // {
    //     var review = await reviews.CompleteReviewAsync(reviewId, token);
    //     return Ok(new ReviewDO(review));
    // }

    // [HttpPost("request")]
    // [ProducesResponseType(StatusCodes.Status201Created)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status409Conflict)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Request one or more reviews for a user project")]
    // [EndpointDescription("Creates review entries for the specified kinds. Self reviews are auto-assigned to the requesting user.")]
    // public async Task<ActionResult<IEnumerable<ReviewDO>>> RequestReviews(
    //     [FromBody] PostReviewRequestDTO dto,
    //     CancellationToken token
    // )
    // {
    //     var requestingUserId = User.GetSID();
    //     var created = new List<ReviewDO>();

    //     foreach (var kind in dto.Kinds)
    //     {
    //         var review = await reviews.RequestReviewAsync(
    //             dto.UserProjectId,
    //             dto.RubricId,
    //             kind,
    //             requestingUserId,
    //             token
    //         );
    //         created.Add(new ReviewDO(review));
    //     }

    //     return Created(string.Empty, created);
    // }

    // [HttpPost("{reviewId:guid}/pickup")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    // [ProducesErrorResponseType(typeof(ProblemDetails))]
    // [EndpointSummary("Pick up a pending review as the current user")]
    // [EndpointDescription("Assigns the current user as reviewer and transitions the review to InProgress.")]
    // public async Task<ActionResult<ReviewDO>> PickupReview(Guid reviewId, CancellationToken token)
    // {
    //     var reviewerId = User.GetSID();
    //     var review = await reviews.AssignReviewerAsync(reviewId, reviewerId, token);
    //     review = await reviews.StartReviewAsync(reviewId, token);
    //     return Ok(new ReviewDO(review));
    // }

    // [HttpDelete("{reviewId:guid}")]
    // [EndpointSummary("Cancel a pending review")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    // public async Task<IActionResult> CancelReview(Guid reviewId, CancellationToken token)
    // {
    //     await reviews.CancelReviewAsync(reviewId, token);
    //     return NoContent();
    // }
}
