// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Core.Engines.Reviews;

public enum ReviewRoundOutcome
{
    /// <summary>Still waiting on reviews.</summary>
    Pending,

    /// <summary>Every slot finished with a passing verdict.</summary>
    Passed,

    /// <summary>At least one finished slot gave a failing verdict.</summary>
    Failed
}

/// <summary>
/// Pure decision logic for a review round: given the slots of a round, has it
/// passed, failed or is it still waiting? Kept free of I/O so it is trivially testable.
/// </summary>
/// <remarks>
/// The slots of a round are created from the rubric at request time, so "all slots"
/// is the requirement. Editing the rubric afterwards cannot change an open round.
/// </remarks>
public static class ReviewRoundResolver
{
    public static ReviewRoundOutcome Resolve(IEnumerable<Review> slots)
    {
        var live = slots.Where(r => r.State is not ReviewState.Cancelled).ToList();
        if (live.Count is 0)
            return ReviewRoundOutcome.Pending;

        // NOTE: One "no" fails the round immediately, we don't wait for the rest.
        // TODO: Decide if Auto (LLM) verdicts should be able to veto, or only inform.
        if (live.Any(r => r.State is ReviewState.Finished && r.Passed is false))
            return ReviewRoundOutcome.Failed;

        return live.All(r => r.State is ReviewState.Finished && r.Passed is true)
            ? ReviewRoundOutcome.Passed
            : ReviewRoundOutcome.Pending;
    }
}
