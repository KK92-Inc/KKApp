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
using App.Backend.API.Bus.Messages;
using App.Backend.Core;
using Wolverine;
using System.ComponentModel;
using System.Linq.Expressions;
using App.Backend.API.Utils;

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
    IRubricService rubricService,
    IMemberService memberService,
    IUserProjectService userProjects,
    IAuthorizationService auth,
    IMessageBus bus,
    DatabaseContext ctx
) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Query all reviews")]
    [EndpointDescription("Returns a paginated list of reviews")]
    public async Task<ActionResult<IEnumerable<ReviewDO>>> GetReviews(
        [FromQuery(Name = "filter[user_project_id]")] Guid? userProjectId,
        [FromQuery(Name = "filter[reviewer_id]"), Description("User conducting a review")] Guid? reviewerId,
        [FromQuery(Name = "filter[reviewee_id]"), Description("User receiving a review")] Guid? revieweeId,
        [FromQuery(Name = "filter[rubric_id]")] Guid? rubricId,
        [FromQuery(Name = "filter[kind]")] ReviewKinds? kind,
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
            r => !status.HasValue || r.State == status.Value,
            // NOTE(W2):TODO: In the future we might migrate this to a package.
            // For now this works as a nice but disgustingly leaky escape hatch.
            revieweeId.HasValue ? r => ctx.Members.Any(m =>
                  m.EntityType == MemberEntityType.UserProject &&
                  m.EntityId == r.UserProjectId &&
                  m.UserId == revieweeId.Value &&
                  m.LeftAt == null
            ) : null
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

    [HttpGet("{reviewId:guid}/{file}/annotations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get annotations for a specific file in a review")]
    [EndpointDescription("Returns the review with full details including reviewer and rubric.")]
    public async Task<ActionResult<IEnumerable<AnnotationDO>>> GetAnnotations(Guid reviewId, string file, CancellationToken token)
    {
        var review = await service.FindByIdAsync(reviewId, token);
        if (review is null) return NotFound("Review not found");

        var annotations = await service.GetAnnotationsAsync(reviewId, file, token);
        return Ok(annotations.Select(a => new AnnotationDO(a)));
    }

    [HttpPut("{reviewId:guid}/{file}/annotations")]
    [RequireScope("evaluation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get annotations for a specific file in a review")]
    [EndpointDescription("Returns the review with full details including reviewer and rubric.")]
    public async Task<ActionResult<IEnumerable<AnnotationDO>>> SetAnnotations(Guid reviewId, string file, CancellationToken token)
    {
        var review = await service.FindByIdAsync(reviewId, token);
        if (review is null) return NotFound("Review not found");

        var annotations = await service.SetAnnotationsAsync(reviewId, User.GetSID(), file, [], token);
        return Ok(annotations.Select(a => new AnnotationDO(a)));
    }

    [HttpPost]
    [RequireScope("evaluation")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Request a review for a user project")]
    [EndpointDescription("Creates review entries for the specified kinds. Self reviews are auto-assigned to the requesting user.")]
    public async Task<ActionResult<IEnumerable<ReviewDO>>> RequestReviews([FromBody] PostReviewRequestDTO dto, CancellationToken token)
    {
        var requester = User.GetSID();
        var reviews = await service.RequestReviewAsync(
            dto.UserProjectId,
            requester,
            dto.Ref,
            token
        );

        foreach (var review in reviews)
        {
            object message = review.Kind switch
            {
                ReviewKinds.Self => new RequestSelfReview(review.Id, dto.UserProjectId, requester),
                ReviewKinds.Peer => new RequestPeerReview(review.Id, dto.UserProjectId),
                ReviewKinds.Async => new RequestAsyncReview(review.Id, dto.UserProjectId),
                // TODO: Implement auto review flow and remove this case
                // ReviewKinds.Auto => new RequestAutoReview(review.Id, dto.UserProjectId),
                _ => throw new ServiceException(500, $"Unhandled review kind: {review.Kind}")
            };
            await bus.PublishAsync(message);
        }

        return Ok(reviews.Select(r => new ReviewDO(r)));
    }

    [HttpGet("user-project/{userProjectId:guid}/status")]
    [EndpointSummary("Get review progress for a user project")]
    public async Task<ActionResult<ReviewProgressDO>> GetProgress(Guid userProjectId, CancellationToken token)
    {
        var userProject = await userProjects.FindByIdAsync(userProjectId, token);
        if (userProject is null) return NotFound(new ProblemDetails { Title = "User project not found." });

        var rubric = await rubricService.FindByProjectId(userProject.ProjectId, token);
        if (rubric is null) return NotFound(new ProblemDetails { Title = "No rubric found for the project associated with this user project." });

        var reviews = userProject.Reviews.ToList();
        return Ok(new ReviewProgressDO(rubric)
        {
            Variants = [.. rubric.Variants
               .Where(v => v.Count > 0)
               .Select(v => new ReviewVariantProgressDO()
               {
                   Kind = v.Kind,
                   Required = v.Count,
                   Finished = reviews.Count(r => r.Kind == v.Kind && r.State is ReviewState.Finished),
                   Active = reviews.Count(r => r.Kind == v.Kind && r.State is ReviewState.InProgress),
               })]
        });
    }

    [HttpPost("{reviewId:guid}/assign/{reviewerId:guid}")]
    [RequireScope("evaluation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Assign a reviewer to a pending review")]
    [EndpointDescription("Assigns the specified user as reviewer for the review. Validates that the reviewer meets the rubric's eligibility requirements.")]
    public async Task<ActionResult<ReviewDO>> AssignReviewer(Guid reviewId, Guid reviewerId, CancellationToken token)
    {
        // NOTE(W2): You can always assign yourself but not someone else, unless you're staff.
        var result = await auth.AuthorizeAsync(User, "staff");
        if (!result.Succeeded && reviewerId != User.GetSID())
            return Forbid();

        var review = await service.AssignReviewerAsync(reviewId, reviewerId, token);
        return Ok(new ReviewDO(review));
    }

    [HttpPost("{reviewId:guid}/start")]
    [RequireScope("evaluation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Start a review")]
    [EndpointDescription("Transitions the review to InProgress and assigns the current user as the reviewer.")]
    public async Task<ActionResult<ReviewDO>> StartReview(Guid reviewId, CancellationToken token)
    {
        var review = await service.FindByIdAsync(reviewId, token);
        if (review is null) return NotFound();

        // NOTE(W2): The reviewer decides when to start, unless you're staff.
        var result = await auth.AuthorizeAsync(User, "staff");
        if (!result.Succeeded && review.ReviewerId != User.GetSID())
            return Forbid();

        review = await service.StartReviewAsync(review.Id, token);
        return Ok(new ReviewDO(review));
    }

    [HttpPost("{reviewId:guid}/complete")]
    [RequireScope("evaluation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Complete a review")]
    [EndpointDescription("Transitions the review to Finished. The review content should be included in the request body.")]
    public async Task<ActionResult<ReviewDO>> CompleteReview(Guid reviewId, CancellationToken token)
    {
        var review = await service.FindByIdAsync(reviewId, token);
        if (review is null) return NotFound();

        // NOTE(W2): The reviewer decides when to complete, unless you're staff.
        var result = await auth.AuthorizeAsync(User, "staff");
        if (!result.Succeeded && review.ReviewerId != User.GetSID())
            return Forbid();

        review = await service.CompleteReviewAsync(review.Id, token);
        await bus.PublishAsync(new ReviewCompletionMessage(review.Id));
        return Ok(new ReviewDO(review));
    }

    [HttpDelete("{reviewId:guid}")]
    [RequireScope("evaluation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Cancel a review")]
    [EndpointDescription("Cancels the review with the specified ID.")]
    public async Task<ActionResult> CancelReview(Guid reviewId, CancellationToken token)
    {
        var review = await service.FindByIdAsync(reviewId, token);
        if (review is null) return NotFound();
        
        var actorId = User.GetSID();

        var isLeader = false;
        var isReviewer = review.ReviewerId == actorId;
        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isReviewer && !isStaff.Succeeded)
        {
            var member = await memberService.FindByEntityAndUserId(review.UserProjectId, actorId, token);
            isLeader = member?.Role is MemberRole.Leader;
        }

        if (!isReviewer && !isStaff.Succeeded && !isLeader)
            return Forbid();

        await service.CancelReviewAsync(reviewId, token);
        return NoContent();
    }
}
