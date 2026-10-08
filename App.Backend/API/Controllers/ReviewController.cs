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
using App.Backend.Domain.Values;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("reviews"), Tags("Reviews")]
[Authorize]
public class ReviewController(
    IReviewService service,
    IMemberService memberService,
    IOnsiteNetworkService onsite,
    IUserProjectService userProjects,
    IAuthorizationService auth,
    IMessageBus bus,
    TimeProvider time,
    DatabaseContext ctx
) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List reviews")]
    [EndpointDescription("Returns a paginated list of reviews, optionally filtered by project session, reviewer, reviewee, rubric, kind or state. Each review is a summary; use the review endpoints for annotations.")]
    public async Task<ActionResult<IEnumerable<ReviewDO>>> GetReviews(
        [FromQuery(Name = "filter[user_project_id]")] Guid? userProjectId,
        [FromQuery(Name = "filter[reviewer_id]"), Description("User conducting a review")] Guid? reviewerId,
        [FromQuery(Name = "filter[reviewee_id]"), Description("User receiving a review")] Guid? revieweeId,
        [FromQuery(Name = "filter[rubric_id]")] Guid? rubricId,
        [FromQuery(Name = "filter[kind]")] ReviewKinds? kind,
        [FromQuery(Name = "filter[not[kind]]")] ReviewKinds? notKind,
        [FromQuery(Name = "filter[status]")] ReviewState? status,
        [FromQuery(Name = "filter[not[status]]")] ReviewState? notStatus,
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
            r => !notKind.HasValue || r.Kind != notKind.Value,
            r => !status.HasValue || r.State == status.Value,
            r => !notStatus.HasValue || r.State != notStatus.Value,
            // TODO: Delete this nasty escape hatch.
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
    [EndpointSummary("Get a review")]
    [EndpointDescription("Returns a single review: its kind, state, verdict, the commit (`sha`) it evaluated, and brief info on the project session, reviewer and rubric.")]
    public async Task<ActionResult<ReviewDO>> GetReviewById(Guid reviewId, CancellationToken token)
    {
        var review = await service.FindByIdAsync(reviewId, token);
        if (review is null)
            return NotFound();
        return Ok(new ReviewDO(review));
    }

    [HttpGet("{reviewId:guid}/annotations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List a review's annotations")]
    [EndpointDescription(@"
Returns the annotations written by the reviewer, in creation order. The reviewer is returned once at the top level,
since a review only has one author.

An annotation is either a `Comment` on a line range of a file, or the `Conclusion` summing up the whole review.
Use `filter[file]` to get the comments on one file and `filter[type]` to get only comments or only the conclusion.
    ")]
    public async Task<ActionResult<ReviewAnnotationDO>> GetAnnotations(
        Guid reviewId,
        [FromQuery(Name = "filter[file]"), Description("Get the annotations made on a specific file")] string? file,
        [FromQuery(Name = "filter[type]")] AnnotationKind? type,
        CancellationToken token
    )
    {
        var review = await service.FindByIdAsync(reviewId, token);
        if (review is null) return NotFound();

        var query = ctx.Annotations.AsNoTracking().Where(a => a.ReviewId == reviewId);
        if (type.HasValue) query = query.Where(a => a.Kind == type.Value);

        var rows = await query.OrderBy(a => a.Id).ToListAsync(token);
        if (!string.IsNullOrWhiteSpace(file)) // in-memory, since Data is opaque to EF
            rows = [.. rows.Where(a => a.Data is CommentAnnotationData c && c.Filepath == file)];

        return Ok(new ReviewAnnotationDO(review, rows.Select(a => a.Data)));
    }

    [HttpPost("~/user-project/{userProjectId:guid}/reviews/pull")]
    [RequireScope("evaluation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Request an evaluation round")]
    [EndpointDescription(@"
Requests an evaluation of the project session (a 'pull'). Only the team leader can do this.

Locks the session and opens a round with the review slots the rubric requires. The current commit of the
default branch is pinned as the round's `sha`, so every review of the round evaluates exactly that commit.
All slots must pass for the session to be marked as completed.
    ")]
    public async Task<ActionResult<IEnumerable<ReviewDO>>> PullReview(Guid userProjectId, CancellationToken token)
    {
        var requester = User.GetSID();
        var round = await service.PullReviewAsync(
            userProjectId,
            requester,
            token
        );

        // Lets us handle review feedback i.e: Launch a agent to clone and review the project.
        // Schedule notifications to evaluators, ...
        foreach (var review in round.Reviews)
        {
            object message = review.Kind switch
            {
                ReviewKinds.Self => new RequestSelfReview(review.Id, userProjectId, requester),
                ReviewKinds.Peer => new RequestPeerReview(review.Id, userProjectId),
                ReviewKinds.Async => new RequestAsyncReview(review.Id, userProjectId),
                ReviewKinds.Auto => new RequestAutoReview(review.Id, userProjectId),
                _ => throw new ServiceException(500, $"Unhandled review kind: {review.Kind}")
            };
            await bus.PublishAsync(message);
        }

        return Ok(round.Reviews.Select(r => new ReviewDO(r)));
    }

    [HttpPost("~/user-project/{userProjectId:guid}/reviews/push")]
    [RequireScope("evaluation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Give a review")]
    [EndpointDescription(@"
Offers to review a project session (a 'push'), without waiting to be assigned.
If the session has an open round, a free slot of the requested kind is claimed. Otherwise an advisory review is
created: feedback that never counts towards completing the project.

The reviewed commit is the current head of the project's default branch, recorded as the review's `sha`.

Claiming starts a clock: start the review within 2 days and finish it within 24 hours of starting, otherwise
it is released for someone else. Self reviews have no deadline while pending.
Reviews as the requesting user unless a different reviewer is specified, which requires staff.
")]
    public async Task<ActionResult<ReviewDO>> PushReview(Guid userProjectId, [FromBody] PostPushReviewRequestDTO dto, CancellationToken token)
    {
        var actorId = User.GetSID();
        var reviewerId = dto.ReviewerId ?? actorId;

        // NOTE(W2): You can always give a review as yourself but not someone else, unless you're staff.
        if (reviewerId != actorId)
        {
            var result = await auth.AuthorizeAsync(User, "staff");
            if (!result.Succeeded) return Forbid();
        }

        var review = await service.PushReviewAsync(
            userProjectId,
            dto.Kind,
            reviewerId,
            token
        );

        return CreatedAtAction(nameof(PushReview), new { reviewId = review.Id }, new ReviewDO(review));
    }

    [HttpGet("~/user-project/{userProjectId:guid}/reviews/rounds")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List evaluation rounds of a project session")]
    [EndpointDescription("Returns every evaluation attempt of the session, oldest first. Each round includes the commit (`sha`) it evaluated and the state and verdict of its review slots.")]
    public async Task<ActionResult<IEnumerable<ReviewRoundDO>>> GetRounds(Guid userProjectId, CancellationToken token)
    {
        var userProject = await userProjects.FindByIdAsync(userProjectId, token);
        if (userProject is null) return NotFound(new ProblemDetails { Title = "User project not found." });

        var rounds = await service.GetRoundsAsync(userProjectId, token);
        return Ok(rounds.Select(r => new ReviewRoundDO(r)));
    }

    [HttpGet("rounds/{roundId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("List evaluation rounds of a project session")]
    [EndpointDescription("Returns every evaluation attempt of the session, oldest first. Each round includes the commit (`sha`) it evaluated and the state and verdict of its review slots.")]
    public async Task<ActionResult<ReviewRoundDO>> GetRound(Guid roundId, CancellationToken token)
    {
        var round = await service.FindRoundByIdAsync(roundId, token);
        if (round is null) return NotFound(new ProblemDetails { Title = "User project not found." });
        return Ok(new ReviewRoundDO(round));
    }

    [HttpDelete("rounds/{roundId:guid}")]
    [RequireScope("evaluation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Cancel an open evaluation round")]
    [EndpointDescription("Cancels the unfinished reviews of the round and unlocks the project. Only the team leader or staff can do this.")]
    public async Task<ActionResult> CancelRound(Guid roundId, CancellationToken token)
    {
        var round = await service.FindRoundByIdAsync(roundId, token);
        if (round is null) return NotFound();

        var isStaff = await auth.AuthorizeAsync(User, "staff");
        if (!isStaff.Succeeded)
        {
            var member = await memberService.FindByEntityAndUserId(round.UserProjectId, User.GetSID(), token);
            if (member?.Role is not MemberRole.Leader)
                return Forbid();
        }

        await service.CancelRoundAsync(roundId, token);
        return NoContent();
    }

    [HttpPost("{reviewId:guid}/assign/{reviewerId:guid}")]
    [RequireScope("evaluation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Assign a reviewer to a pending review")]
    [EndpointDescription("Assigns the user as reviewer of a pending review. Anyone can assign themselves; assigning someone else requires staff. The reviewer must meet the rubric's eligibility rules.")]
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
    [EndpointDescription("Moves a pending review to InProgress. Only the reviewer (or staff) can start it, and Peer reviews must be started from onsite.")]
    public async Task<ActionResult<ReviewDO>> StartReview(Guid reviewId, CancellationToken token)
    {
        var review = await service.FindByIdAsync(reviewId, token);
        if (review is null) return NotFound();

        // NOTE(W2): The reviewer decides when to start, unless you're staff.
        var result = await auth.AuthorizeAsync(User, "staff");
        if (!result.Succeeded && review.ReviewerId != User.GetSID())
            return Forbid();

        if (review.Kind == ReviewKinds.Peer && !onsite.IsOnsite(HttpContext.Connection.RemoteIpAddress))
            return Problem(title: "Peer reviews must be started from onsite.", statusCode: StatusCodes.Status422UnprocessableEntity);

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
    [EndpointDescription("Transitions the review to Finished and records the verdict. Reviews that are part of an evaluation round must include `passed`: \"Do you think this project is a pass?\". When every review of the round is finished and passed the project is completed; a single fail closes the round and the team has to request a new one.")]
    public async Task<ActionResult<ReviewDO>> CompleteReview(Guid reviewId, [FromBody] PostCompleteReviewRequestDTO dto, CancellationToken token)
    {
        var review = await service.FindByIdAsync(reviewId, token);
        if (review is null) return NotFound();

        var reviewerId = review.ReviewerId;
        if (!reviewerId.HasValue)
            return Problem(title: "No Evaluator assigned to this review", statusCode: 422);

        // NOTE(W2): The reviewer decides when to complete, unless you're staff.
        var result = await auth.AuthorizeAsync(User, "staff");
        if (!result.Succeeded && review.ReviewerId != User.GetSID())
            return Forbid();

        review = await service.CompleteReviewAsync(review.Id, dto.Passed, dto.Annotations.Select(d => new Annotation
        {
            Kind = d.Kind,
            AuthorId = reviewerId.Value,
            ReviewId = reviewId,
            Data = d
        }), token);

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

        await service.StopReviewAsync(reviewId, token);
        return NoContent();
    }
}