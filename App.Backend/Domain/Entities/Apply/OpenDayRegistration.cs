// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Entities.Apply;

/// <summary>
/// An applicant's sign-up for an <see cref="OpenDay"/>.
/// </summary>
[Table("openday_registration", Schema = "apply")]
[Index(nameof(EventId), nameof(Email), IsUnique = true)]
[Index(nameof(Email))]
public class OpenDayRegistration : ApplyRegistration<OpenDay> { }
