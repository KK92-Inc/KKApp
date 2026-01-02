// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core;
using App.Backend.Core.Rules;
using App.Backend.Core.Services.Implementation;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;
using App.Backend.Tests.Fixtures;
using App.Backend.Tests.Fixtures.Factory;

// ============================================================================

namespace App.Backend.Tests.Services;

public class ReviewServiceTests : ServiceTestBase
{
    private readonly ReviewService _sut;
    private readonly EligibilityService _eligibilityService;

    public ReviewServiceTests()
    {
        _eligibilityService = new EligibilityService(Context);
        _sut = new ReviewService(Context, _eligibilityService);
    }

    [Fact]
    public async Task RequestReviewAsync_WhenValid_ShouldCreateReview()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var userProject = await UserFactory.CreateUserProject(project.Id)
            .WithContext(Context)
            .GenerateAsync();
        userProject.State = EntityObjectState.Awaiting;
        await Context.SaveChangesAsync();

        // Add user as member
        var member = UserProjectFactory.CreateMember(userProject.Id, user.Id).Generate();
        Context.UserProjectMembers.Add(member);
        await Context.SaveChangesAsync();

        var rubric = await ReviewFactory.CreateRubric(creatorId: user.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await _sut.RequestReviewAsync(
            userProject.Id,
            rubric.Id,
            ReviewVariant.Peer,
            user.Id
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userProject.Id, result.UserProjectId);
        Assert.Equal(rubric.Id, result.RubricId);
        Assert.Equal(ReviewVariant.Peer, result.Kind);
        Assert.Equal(ReviewState.Pending, result.State);
        Assert.Null(result.ReviewerId);
    }

    [Fact]
    public async Task RequestReviewAsync_WhenSelfReview_ShouldAutoAssignReviewer()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var userProject = await UserFactory.CreateUserProject(project.Id)
            .WithContext(Context)
            .GenerateAsync();
        userProject.State = EntityObjectState.Awaiting;
        await Context.SaveChangesAsync();

        // Add user as member
        var member = UserProjectFactory.CreateMember(userProject.Id, user.Id).Generate();
        Context.UserProjectMembers.Add(member);
        await Context.SaveChangesAsync();

        var rubric = await ReviewFactory.CreateRubric(creatorId: user.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Act
        var result = await _sut.RequestReviewAsync(
            userProject.Id,
            rubric.Id,
            ReviewVariant.Self,
            user.Id
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ReviewVariant.Self, result.Kind);
        Assert.Equal(user.Id, result.ReviewerId);
    }

    [Fact]
    public async Task RequestReviewAsync_WhenProjectNotAwaiting_ShouldThrow()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var userProject = await UserFactory.CreateUserProject(project.Id)
            .WithContext(Context)
            .GenerateAsync();
        userProject.State = EntityObjectState.Active; // Not awaiting
        await Context.SaveChangesAsync();

        var rubric = await ReviewFactory.CreateRubric(creatorId: user.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(() =>
            _sut.RequestReviewAsync(userProject.Id, rubric.Id, ReviewVariant.Peer, user.Id)
        );
        Assert.Equal(422, exception.StatusCode);
    }

    [Fact]
    public async Task RequestReviewAsync_WhenDuplicateReview_ShouldThrow()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var userProject = await UserFactory.CreateUserProject(project.Id)
            .WithContext(Context)
            .GenerateAsync();
        userProject.State = EntityObjectState.Awaiting;
        await Context.SaveChangesAsync();

        var rubric = await ReviewFactory.CreateRubric(creatorId: user.Id)
            .WithContext(Context)
            .GenerateAsync();

        // Create first review
        await _sut.RequestReviewAsync(userProject.Id, rubric.Id, ReviewVariant.Peer, user.Id);

        // Act & Assert - Try to create duplicate
        var exception = await Assert.ThrowsAsync<ServiceException>(() =>
            _sut.RequestReviewAsync(userProject.Id, rubric.Id, ReviewVariant.Peer, user.Id)
        );
        Assert.Equal(409, exception.StatusCode);
    }

    [Fact]
    public async Task RequestReviewAsync_WhenRubricDoesNotSupportKind_ShouldThrow()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var userProject = await UserFactory.CreateUserProject(project.Id)
            .WithContext(Context)
            .GenerateAsync();
        userProject.State = EntityObjectState.Awaiting;
        await Context.SaveChangesAsync();

        // Rubric only supports Peer reviews
        var rubric = await ReviewFactory.CreateRubric(
                creatorId: user.Id,
                supportedKinds: ReviewKinds.Peer)
            .WithContext(Context)
            .GenerateAsync();

        // Act & Assert - Try to create an Async review
        var exception = await Assert.ThrowsAsync<ServiceException>(() =>
            _sut.RequestReviewAsync(userProject.Id, rubric.Id, ReviewVariant.Async, user.Id)
        );
        Assert.Equal(422, exception.StatusCode);
    }

    [Fact]
    public async Task AssignReviewerAsync_WhenValid_ShouldAssignReviewer()
    {
        // Arrange
        var creator = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var reviewer = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var userProject = await UserFactory.CreateUserProject(project.Id)
            .WithContext(Context)
            .GenerateAsync();

        var rubric = await ReviewFactory.CreateRubric(creatorId: creator.Id)
            .WithContext(Context)
            .GenerateAsync();

        var review = ReviewFactory.Create(userProject.Id, rubric.Id).Generate();
        review.State = ReviewState.Pending;
        review.Kind = ReviewVariant.Async;
        Context.Reviews.Add(review);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.AssignReviewerAsync(review.Id, reviewer.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(reviewer.Id, result.ReviewerId);
    }

    [Fact]
    public async Task StartReviewAsync_WhenPending_ShouldTransitionToInProgress()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var userProject = await UserFactory.CreateUserProject(project.Id)
            .WithContext(Context)
            .GenerateAsync();

        var rubric = await ReviewFactory.CreateRubric(creatorId: user.Id)
            .WithContext(Context)
            .GenerateAsync();

        var review = ReviewFactory.Create(userProject.Id, rubric.Id, user.Id).Generate();
        review.State = ReviewState.Pending;
        Context.Reviews.Add(review);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.StartReviewAsync(review.Id);

        // Assert
        Assert.Equal(ReviewState.InProgress, result.State);
    }

    [Fact]
    public async Task CompleteReviewAsync_WhenInProgress_ShouldTransitionToFinished()
    {
        // Arrange
        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();
        var userProject = await UserFactory.CreateUserProject(project.Id)
            .WithContext(Context)
            .GenerateAsync();

        var rubric = await ReviewFactory.CreateRubric(creatorId: user.Id)
            .WithContext(Context)
            .GenerateAsync();

        var review = ReviewFactory.Create(userProject.Id, rubric.Id, user.Id).Generate();
        review.State = ReviewState.InProgress;
        Context.Reviews.Add(review);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.CompleteReviewAsync(review.Id);

        // Assert
        Assert.Equal(ReviewState.Finished, result.State);
    }
}