// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Entities.Apply;

/// <summary>
/// An applicant's sign-up for a <see cref="CheckIn"/>.
/// </summary>
[Table("checkin_registration", Schema = "apply")]
[Index(nameof(EventId), nameof(Email), IsUnique = true)]
[Index(nameof(Email))]
public class CheckInRegistration : ApplyRegistration<CheckIn> { }
