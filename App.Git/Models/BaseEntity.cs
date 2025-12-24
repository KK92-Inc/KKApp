// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Git.Models;

/// <summary>
/// Base entity with timestamps and UUID primary key.
/// </summary>
public abstract class BaseEntity
{
    [Column("id", Order = 0), Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
