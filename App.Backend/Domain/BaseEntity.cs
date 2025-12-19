// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain;


// NOTE: https://learn.microsoft.com/en-us/ef/core/modeling/generated-properties?tabs=data-annotations

/// <summary>
/// Base entity for all entities in the system.
/// </summary>
// [PrimaryKey(nameof(Id))]

public abstract class BaseEntity : BaseTimestampEntity
{
    // This can easily be modified to be BaseEntity<T> and public T Id to support different key types.
    // Using non-generic integer types for simplicity
    [Column("id", Order = 0), Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
}
