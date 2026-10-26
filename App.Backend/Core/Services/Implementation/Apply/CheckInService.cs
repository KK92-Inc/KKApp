// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Services.Interface.Apply;
using App.Backend.Core.Services.Options;
using App.Backend.Database;
using App.Backend.Domain.Entities.Apply;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// ============================================================================

namespace App.Backend.Core.Services.Implementation.Apply;

public class CheckInService(
    DatabaseContext ctx,
    TimeProvider time,
    IOptions<AdmissionOptions> options
) : ApplyEventService<CheckIn, CheckInRegistration>(ctx, time, options), ICheckInService
{
    protected override string Label => "check-in";

    protected override string LockScope => "checkin";

    // The open day is the next step. Once someone signed up for one on the strength of this check-in,
    // that sign-up has to go first, or it would be left standing on nothing.
    protected override async Task EnsureCanRevokeAsync(string email, CancellationToken token)
    {
        var signedUp = await Context.OpenDayRegistrations.AnyAsync(r => r.Email == email, token);
        ServiceException.ThrowIf(signedUp, 409, $"{email} already signed up for an open day, remove that sign-up first.");
    }
}
