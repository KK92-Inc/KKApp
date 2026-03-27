// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Rules;

namespace App.Backend.Core.Rules.Evaluators;

/// <summary>
/// Evaluates if a user is in the same timezone as required.
/// Currently returns Skip as timezone info may not be available.
/// </summary>
public class SameTimezoneEvaluator : IRuleEvaluator<SameTimezoneRule>
{
    public Task<RuleEngineResult> EvaluateAsync(SameTimezoneRule rule, RuleContext ctx, CancellationToken ct)
    {
        // TODO: Implement once timezone info is available on User entity
        return Task.FromResult(RuleEngineResult.Skip("Timezone information not available"));
    }
}
