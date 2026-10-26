// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Services.Interface.Apply;
using App.Backend.Database;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Values;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Core.Services.Implementation.Apply;

public class AdmissionService(DatabaseContext ctx) : IAdmissionService
{
    public async Task<AdmissionProgress> GetProgressAsync(string email, CancellationToken token = default)
    {
        var normalized = email.Trim().ToLowerInvariant();

        // The one where they were verified, otherwise the latest they signed up for.
        var checkIn = await ctx.CheckInRegistrations
            .AsNoTracking()
            .Include(r => r.Event)
            .Where(r => r.Email == normalized)
            .OrderByDescending(r => r.AttendedAt != null)
            .ThenByDescending(r => r.Event.StartsAt)
            .FirstOrDefaultAsync(token);

        var openDay = await ctx.OpenDayRegistrations
            .AsNoTracking()
            .Include(r => r.Event)
            .Where(r => r.Email == normalized)
            .OrderByDescending(r => r.AttendedAt != null)
            .ThenByDescending(r => r.Event.StartsAt)
            .FirstOrDefaultAsync(token);

        // The account only exists from the trial on, and is matched by the same email.
        var user = await ctx.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.Equals(normalized, StringComparison.CurrentCultureIgnoreCase), token);

        UserTrial? trial = null;
        if (user is not null)
        {
            // The one they were selected in, otherwise the latest.
            trial = await ctx.UserTrials
                .AsNoTracking()
                .Include(ut => ut.User)
                .Where(ut => ut.UserId == user.Id)
                .OrderByDescending(ut => ut.Outcome == TrialOutcome.Selected)
                .ThenByDescending(ut => ut.CreatedAt)
                .FirstOrDefaultAsync(token);
        }

        AdmissionStep next;
        if (user is not null && (user.Role is not UserRole.Applicant || user.KickoffId is not null))
            next = AdmissionStep.Done;
        else if (checkIn?.AttendedAt is null)
            next = AdmissionStep.CheckIn;
        else if (openDay?.AttendedAt is null)
            next = AdmissionStep.OpenDay;
        else if (trial?.Outcome is TrialOutcome.Selected)
            next = AdmissionStep.Kickoff;
        else
            next = AdmissionStep.Trial;

        return new AdmissionProgress(normalized, next, checkIn, openDay, user, trial);
    }
}
