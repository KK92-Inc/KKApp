// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Apply;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

/// <summary>
/// Open days, the second step of admissions. See <see cref="IApplyEventService{TEvent,TRegistration}"/>.
/// </summary>
/// <remarks>
/// Signing up requires having been verified at a check-in, unless <c>Admissions:RequireCheckInForOpenDay</c> is switched off.
/// </remarks>
public interface IOpenDayService : IApplyEventService<OpenDay, OpenDayRegistration>
{
}
