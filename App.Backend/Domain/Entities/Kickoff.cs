// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities;

/// <summary>
/// A kickoff describes a point in time when a group of users convert from
/// applicants to students and groups them together into cohorts.
/// 
/// E/g: You have 200 students apply for your school. You can choose to
/// either have them as applicants for a month as a trial period or just
/// straight off push them to become students.
/// </summary>
[Table("tbl_kickoffs")]
public class Kickoff : BaseEntity
{
    /// <summary>
    /// A name for the Kickoff e.g: Kickoff::2026::Summer
    /// </summary>
    [Column("name"), StringLength(255)]
    public required string Name { get; set; }

    /// <summary>
    /// The limit / max amount of users that can join this kickoff.
    /// </summary>
    [Column("capacity")]
    public int Capacity { get; set; }

    /// <summary>
    /// The scheduled date on when to invoke this kickoff.
    /// </summary>
    [Column("starts_at")]
    public DateTimeOffset StartsAt { get; set; }
}
