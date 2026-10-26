// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Entities.Apply;

/// <summary>
/// The second step of admissions. A larger session where applicants who passed a <see cref="CheckIn"/>
/// get to see the school before committing to a <see cref="Trial"/>.
///
/// E/g: You run an open day once a month for up to 200 people.
/// </summary>
[Table("openday", Schema = "apply")]
[Index(nameof(StartsAt))]
public class OpenDay : ApplyEvent { }
