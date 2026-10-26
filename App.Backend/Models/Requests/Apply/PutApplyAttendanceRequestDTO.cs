// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;

// ============================================================================

namespace App.Backend.Models.Requests.Apply;

/// <summary>
/// Request DTO for recording whether someone was seen at a check-in or an open day.
/// </summary>
public class PutApplyAttendanceRequestDTO
{
    [Description("True if a staff member saw this person there, false to take that back.")]
    public required bool Attended { get; init; }
}
