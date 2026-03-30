// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Rules.Evaluations;

// ============================================================================

namespace App.Backend.Core.Engines.Evaluations.Rules;

/// <summary>
/// Checks if the user is in the same timezone as the subject project members.
/// NOTE: Currently timezone info is not stored in user profiles, so this rule
/// always passes. When timezone support is added to user profiles, this evaluator
/// should be updated to perform actual timezone comparisons.
/// </summary>
public sealed class SameTimezoneEvaluator : IRuleEvaluator<SameTimezoneRule>
{
    public Task<Result> EvaluateAsync(SameTimezoneRule rule, Context ctx, CancellationToken ct)
    {
        // TODO: Implement when timezone field is added to user profiles.
        // For now, skip with a warning so admins know this rule is not enforced.
        return Task.FromResult(Result.Skip("Timezone matching is not yet implemented."));
    }
}
