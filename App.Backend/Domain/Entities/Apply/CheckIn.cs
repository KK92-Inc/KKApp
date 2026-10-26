// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Entities.Apply;

/// <summary>
/// The first step of admissions. A session where an applicant shows up in person and staff
/// verify they are who they say they are.
///
/// E/g: You run a check-in every Tuesday morning for 60 people. Someone who signed up and was
/// seen by staff there may then sign up for an <see cref="OpenDay"/>.
/// </summary>
[Table("checkin", Schema = "apply")]
[Index(nameof(StartsAt))]
public class CheckIn : ApplyEvent { }
