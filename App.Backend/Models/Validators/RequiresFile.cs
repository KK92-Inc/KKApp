// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Values.Misc;

// ============================================================================

namespace App.Backend.Models.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class RequiresFileAttribute : ValidationAttribute
{
    private readonly string _targetPath;

    public RequiresFileAttribute(string targetPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
        _targetPath = NormalizePath(targetPath);
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;
            
        if (value is not IEnumerable<CommitFile> files)
            return false;

        return files.Any(file =>
        {
            return file?.Path is not null &&
                        string.Equals(NormalizePath(file.Path), _targetPath, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static string NormalizePath(string path) =>
        path.Trim().Replace('\\', '/').TrimStart('/');
}