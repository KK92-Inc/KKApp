// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Git.Models.Requests;
using System.ComponentModel.DataAnnotations;

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
            
        if (value is not PostCommitDTO commit)
            return false;

        return commit.Files.Exists(f => f.Path == _targetPath);
    }

    private static string NormalizePath(string path) =>
        path.Trim().Replace('\\', '/').TrimStart('/');
}