// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Wolverine.Attributes;
using App.Backend.API.Bus.Messages.Kickoff;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.API.Bus.Handlers.Kickoff;

/// <summary>
/// Handles a starting kickoff.
///
/// Kickoffs essentially just activate user accounts and promote them to
/// actual students into a campus. Unless they were already students then
/// nothing changes.
/// </summary>
[WolverineHandler]
public class KickoffHandler(IKickoffService kickoffs, ILogger<KickoffHandler> log)
{
    public async Task<IEnumerable<object>> Handle(StartKickoff message, CancellationToken ct)
    {
        // Missing, or moved to a later date after the job picked it up.
        if (!await kickoffs.EnsureStartedAsync(message.KickoffId, ct))
            return [];

        var applicants = await kickoffs.GetUserIdsAsync(message.KickoffId, UserRole.Applicant, ct);
        log.LogInformation("Kickoff {KickoffId} started, promoting {Count} applicant(s)", message.KickoffId, applicants.Count);

        return applicants.Select(id => new PromoteApplicant(id, message.KickoffId));
    }
}
