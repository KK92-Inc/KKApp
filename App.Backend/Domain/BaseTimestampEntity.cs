// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain;

/// <summary>
/// Base entity which provides timestamp properties for all entities.
/// </summary>
// [PrimaryKey(nameof(Id))]
public abstract class BaseTimestampEntity
{
    /// <summary>
    /// Created at timestamp
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Updated at timestamp
    /// </summary>
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
