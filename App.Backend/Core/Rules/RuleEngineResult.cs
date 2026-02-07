// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Core.Rules;

/// <summary>
/// Result of an eligibility check.
/// </summary>
public record RuleEngineResult
{
    /// <summary>
    /// Whether the user is eligible.
    /// </summary>
    public required bool IsEligible { get; init; }

    /// <summary>
    /// Whether the rule was skipped.
    /// </summary>
    public bool IsSkipped { get; init; }

    /// <summary>
    /// Reasons why the user is not eligible (if applicable).
    /// </summary>
    public List<string> Reasons { get; init; } = [];

    /// <summary>
    /// Warning messages (e.g., for skipped rules).
    /// </summary>
    public List<string> Warnings { get; init; } = [];

    /// <summary>
    /// Creates a successful eligibility result.
    /// </summary>
    public static RuleEngineResult Success() => new() { IsEligible = true };

    /// <summary>
    /// Creates a failed eligibility result with a reason.
    /// </summary>
    public static RuleEngineResult Failure(string reason) => new()
    {
        IsEligible = false,
        Reasons = [reason]
    };

    /// <summary>
    /// Creates a failed eligibility result with multiple reasons.
    /// </summary>
    public static RuleEngineResult Failure(IEnumerable<string> reasons) => new()
    {
        IsEligible = false,
        Reasons = [.. reasons]
    };

    /// <summary>
    /// Creates a skipped result with a warning message.
    /// </summary>
    public static RuleEngineResult Skip(string warning) => new()
    {
        IsEligible = true,
        IsSkipped = true,
        Warnings = [warning]
    };

    /// <summary>
    /// Combines multiple eligibility results (all must pass).
    /// </summary>
    public static RuleEngineResult Combine(IEnumerable<RuleEngineResult> results)
    {
        var failures = results.Where(r => !r.IsEligible).SelectMany(r => r.Reasons).ToList();
        var warnings = results.SelectMany(r => r.Warnings).ToList();

        if (failures.Count > 0)
        {
            return new RuleEngineResult
            {
                IsEligible = false,
                Reasons = failures,
                Warnings = warnings
            };
        }

        return warnings.Count > 0
            ? new RuleEngineResult { IsEligible = true, Warnings = warnings }
            : Success();
    }
}
