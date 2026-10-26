// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Apply;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

/// <summary>
/// Check-ins, the first step of admissions. See <see cref="IApplyEventService{TEvent,TRegistration}"/>.
/// </summary>
public interface ICheckInService : IApplyEventService<CheckIn, CheckInRegistration> { }
