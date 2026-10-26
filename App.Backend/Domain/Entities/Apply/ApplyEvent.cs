// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities.Apply;

/// <summary>
/// What a <see cref="CheckIn"/> and an <see cref="OpenDay"/> have in common: a dated
/// session, somewhere, that applicants sign up for and that staff verify they showed up to.
///
/// This type is not an entity itself. Each concrete session gets a table of its own in the
/// <c>apply</c> schema, so the two can grow apart without a discriminator column.
/// </summary>
public abstract class ApplyEvent : BaseEntity
{
    /// <summary>
    /// A name for the session e.g: Open Day::2026::November
    /// </summary>
    [Column("name"), StringLength(255)]
    public required string Name { get; set; }

    /// <summary>
    /// What applicants can expect, e.g: what to bring.
    /// </summary>
    [Column("description"), StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Where it takes place, e.g: the address of the campus. Free text on purpose.
    /// </summary>
    [Column("location"), StringLength(500)]
    public required string Location { get; set; }

    /// <summary>
    /// The max amount of people that can sign up. Null means there is no limit.
    /// </summary>
    [Column("capacity")]
    public int? Capacity { get; set; }

    /// <summary>
    /// When it begins. Staff can start verifying attendance shortly before this.
    /// </summary>
    [Column("starts_at")]
    public DateTimeOffset StartsAt { get; set; }

    /// <summary>
    /// The last moment someone can still sign up. Never later than <see cref="StartsAt"/>.
    /// </summary>
    [Column("deadline_at")]
    public DateTimeOffset DeadlineAt { get; set; }
}
