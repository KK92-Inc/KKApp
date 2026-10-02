using App.Backend.Core.Engines.Reviews;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;

namespace App.Backend.Tests.Unit;

public class ReviewRoundResolverTests
{
    private static Review Slot(ReviewKinds kind, ReviewState state, bool? passed = null) =>
        new() { Kind = kind, State = state, Passed = passed };

    [Fact]
    public void EmptyRound_IsPending()
    {
        Assert.Equal(ReviewRoundOutcome.Pending, ReviewRoundResolver.Resolve([]));
    }

    [Fact]
    public void AllSlotsFinishedAndPassed_Passes()
    {
        var outcome = ReviewRoundResolver.Resolve([
            Slot(ReviewKinds.Self, ReviewState.Finished, true),
            Slot(ReviewKinds.Peer, ReviewState.Finished, true),
            Slot(ReviewKinds.Peer, ReviewState.Finished, true),
        ]);

        Assert.Equal(ReviewRoundOutcome.Passed, outcome);
    }

    [Fact]
    public void MissingReviews_StayPending()
    {
        var outcome = ReviewRoundResolver.Resolve([
            Slot(ReviewKinds.Self, ReviewState.Finished, true),
            Slot(ReviewKinds.Peer, ReviewState.Finished, true),
            Slot(ReviewKinds.Peer, ReviewState.Pending),
        ]);

        Assert.Equal(ReviewRoundOutcome.Pending, outcome);
    }

    [Fact]
    public void InProgressReview_StaysPending()
    {
        var outcome = ReviewRoundResolver.Resolve([
            Slot(ReviewKinds.Peer, ReviewState.Finished, true),
            Slot(ReviewKinds.Peer, ReviewState.InProgress),
        ]);

        Assert.Equal(ReviewRoundOutcome.Pending, outcome);
    }

    [Fact]
    public void SingleFail_FailsImmediately_EvenWithOpenSlots()
    {
        var outcome = ReviewRoundResolver.Resolve([
            Slot(ReviewKinds.Self, ReviewState.Finished, true),
            Slot(ReviewKinds.Peer, ReviewState.Finished, false),
            Slot(ReviewKinds.Peer, ReviewState.Pending),
        ]);

        Assert.Equal(ReviewRoundOutcome.Failed, outcome);
    }

    [Fact]
    public void CancelledSlots_AreIgnored()
    {
        var outcome = ReviewRoundResolver.Resolve([
            Slot(ReviewKinds.Peer, ReviewState.Finished, true),
            Slot(ReviewKinds.Peer, ReviewState.Cancelled),
        ]);

        Assert.Equal(ReviewRoundOutcome.Passed, outcome);
    }

    [Fact]
    public void FinishedWithoutVerdict_DoesNotPass()
    {
        var outcome = ReviewRoundResolver.Resolve([
            Slot(ReviewKinds.Peer, ReviewState.Finished, null),
        ]);

        Assert.Equal(ReviewRoundOutcome.Pending, outcome);
    }
}
