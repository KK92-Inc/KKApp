// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities.Reviews;

/// <summary>
/// Represents an annotation made on a review, which can be of different
/// kinds such as text, suggestion, or drawing.
/// 
/// Each annotation is associated with a specific review and an author (user). 
/// The annotation contains information about the file path, reference, line
/// and column numbers, and the actual data of the annotation in JSON format.
/// </summary>
[Table("tbl_annotation"), Index(nameof(ReviewId))]
public class Annotation : BaseEntity
{
    [Column("review_id")]
    public Guid ReviewId { get; set; }

    [Column("author_id")]
    public Guid AuthorId { get; set; }

    [Column("kind")]
    public AnnotationKind Kind { get; set; }

    [Column("file_path")]
    public string FilePath { get; set; } = string.Empty;

    [Column("data", TypeName = "jsonb")]
    public required string Data { get; set; }

    [ForeignKey(nameof(ReviewId))]
    public virtual Review Review { get; set; } = null!;

    [ForeignKey(nameof(AuthorId))]
    public virtual User Author { get; set; } = null!;
}