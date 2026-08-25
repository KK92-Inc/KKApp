// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Git.Models.Requests;

public class PostCommitDTO()
{
    [Required, StringLength(1024, MinimumLength = 1)]
    [Description("Message accompanying the commit.")]
    public required string Message { get; set; }

    [Required, MinLength(1), MaxLength(100)]
    [Description("Files included in this commit.")]
    public required List<CommitFileDTO> Files { get; set; }
}

public class CommitFileDTO : IValidatableObject
{
    [Required, StringLength(4096, MinimumLength = 1)]
    [Description("Relative file path, e.g. src/Program.cs")]
    public required string Path { get; set; }

    [Required]
    [Description("Base64 encoded file content.")]
    public required string Content { get; set; }

    private const long MaxFileSizeBytes = 25 * 1024 * 1024;

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Convert.FromBase64String(Content).LongLength > MaxFileSizeBytes)
        {
            yield return new ValidationResult(
                $"File '{Path}' exceeds max size of {MaxFileSizeBytes} bytes.",
                [nameof(Content)]);
        }

        if (System.IO.Path.IsPathRooted(Path) || Path.Split('/', '\\').Any(seg => seg is ".." or "."))
        {
            yield return new ValidationResult(
                $"Path '{Path}' is not a valid relative repository path.",
                [nameof(Path)]);
        }

        // Reject characters that are illegal on common filesystems / git
        if (Path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
        {
            yield return new ValidationResult(
                $"Path '{Path}' contains invalid characters.",
                [nameof(Path)]);
        }
    }
}