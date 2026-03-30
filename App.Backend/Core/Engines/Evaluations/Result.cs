// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Core.Engines.Evaluations;

public record Result
{
    public required bool IsSuccess { get; init; }
    public bool IsSkipped { get; init; }
    public IReadOnlyList<string> Reasons  { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];

    // ── Factories ──────────────────────────────────────────────────────────────

    public static Result Success() => new() { IsSuccess = true };

    public static Result Failure(string reason) => new()
    {
        IsSuccess = false,
        Reasons   = [reason]
    };

    public static Result Failure(IEnumerable<string> reasons) => new()
    {
        IsSuccess = false,
        Reasons   = [..reasons]
    };

    /// <summary>
    /// Rule was skipped (e.g. not applicable for this role/context).
    /// Treated as a pass, but callers can inspect it separately.
    /// </summary>
    public static Result Skip(string warning) => new()
    {
        IsSuccess = true,
        IsSkipped = true,
        Warnings  = [warning]
    };

    // ── Aggregation ────────────────────────────────────────────────────────────

    /// <summary>
    /// Merges a set of results (logical AND).
    /// Always collects every failure so the caller can surface them all at once.
    /// A combined result is only marked skipped when every individual result was skipped.
    /// </summary>
    public static Result Combine(IEnumerable<Result> results)
    {
        var list     = results.ToList();
        var failures = list.Where(r => !r.IsSuccess).SelectMany(r => r.Reasons).ToList();
        var warnings = list.SelectMany(r => r.Warnings).ToList();

        if (failures.Count > 0)
            return new Result { IsSuccess = false, Reasons = failures, Warnings = warnings };

        return new Result
        {
            IsSuccess = true,
            IsSkipped = list.Count > 0 && list.All(r => r.IsSkipped),
            Warnings  = warnings
        };
    }
}