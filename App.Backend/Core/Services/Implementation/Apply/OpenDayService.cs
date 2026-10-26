// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Services.Interface;
using App.Backend.Core.Services.Interface.Apply;
using App.Backend.Core.Services.Options;
using App.Backend.Database;
using App.Backend.Domain.Entities.Apply;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// ============================================================================

namespace App.Backend.Core.Services.Implementation.Apply;

public class OpenDayService(DatabaseContext ctx, TimeProvider time, IOptions<AdmissionOptions> options) : ApplyEventService<OpenDay, OpenDayRegistration>(ctx, time, options), IOpenDayService
{
    protected override string Label => "open day";

    protected override string LockScope => "openday";

    protected override async Task EnsureEligibleAsync(string email, CancellationToken token)
    {
        if (!Rules.RequireCheckInForOpenDay)
            return;

        var verified = await Context.CheckInRegistrations.AnyAsync(r => r.Email == email && r.AttendedAt != null, token);
        ServiceException.ThrowIf(!verified, 409, $"{email} hasn't been verified at a check-in yet, which an open day requires.");
    }
}
