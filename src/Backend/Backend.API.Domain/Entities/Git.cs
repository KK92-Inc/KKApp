// ============================================================================
// Copyright (c) 2025 - W2Inc.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace Backend.API.Domain.Entities;

[Table("tbl_git")]
public class GitInfo : BaseEntity
{
    public string? Branch { get; set; }
    public string? CommitHash { get; set; }
    public string? CommitMessage { get; set; }
    public DateTimeOffset? CommitDate { get; set; }
}