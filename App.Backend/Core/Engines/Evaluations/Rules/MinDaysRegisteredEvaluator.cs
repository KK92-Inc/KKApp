// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Rules.Evaluations;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Core.Engines.Evaluations.Rules;

public sealed class MinDaysRegisteredEvaluator : IRuleEvaluator<MinDaysRegisteredRule>
{
    public async Task<Result> EvaluateAsync(MinDaysRegisteredRule rule, Context ctx, CancellationToken ct)
    {
        var days = (int)(ctx.Now - ctx.User.CreatedAt).TotalDays;

        return days >= rule.Days
            ? Result.Success()
            : Result.Failure(rule.Description
                ?? $"Must have been registered for at least {rule.Days} day(s) (you have {days}).");
    }
}