using System.Diagnostics;
using App.Backend.Core.Services.Interface;
using App.Backend.Database;
using App.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Backend.Tests.Integration;

public class LocalProcessGitService(DatabaseContext db) : IGitService
{
    private readonly string _baseRepoPath = Path.Combine(Path.GetTempPath(), "repos");
    

    private string GetRepoPath(string owner, string name) => 
        Path.Combine(_baseRepoPath, owner, name);

    public async Task<Git?> FindByIdAsync(Guid id, CancellationToken token = default)
    {
        // Assuming your Git entity is stored in this DbSet
        return await db.GitInfo.FirstOrDefaultAsync(g => g.Id == id, token);
    }

    public Task<bool> ExistsAsync(string owner, string name, CancellationToken token = default)
    {
        return Task.FromResult(Directory.Exists(GetRepoPath(owner, name)));
    }

    public async Task<bool> CreateAsync(string owner, string name, CancellationToken token = default)
    {
        var repoPath = GetRepoPath(owner, name);
        if (Directory.Exists(repoPath)) return false; // 409 Conflict

        Directory.CreateDirectory(repoPath);

        var result = await RunGitAsync(repoPath, token, "init", "--bare");
        return result.ExitCode == 0;
    }

    public Task<bool> DeleteAsync(string owner, string name, CancellationToken token = default)
    {
        var repoPath = GetRepoPath(owner, name);
        if (!Directory.Exists(repoPath)) return Task.FromResult(false); // 404 Not Found

        ForceDeleteDirectory(repoPath);
        return Task.FromResult(true);
    }

    public Task<bool> RenameAsync(string owner, string name, string newName, CancellationToken token = default)
    {
        var oldPath = GetRepoPath(owner, name);
        var newPath = GetRepoPath(owner, newName);

        if (!Directory.Exists(oldPath)) return Task.FromResult(false); // 404
        if (Directory.Exists(newPath)) return Task.FromResult(false);  // 409

        Directory.Move(oldPath, newPath);
        return Task.FromResult(true);
    }

    public async Task<bool> CreateBranchAsync(string owner, string name, string @ref, string child, CancellationToken token = default)
    {
        var repoPath = GetRepoPath(owner, name);
        if (!Directory.Exists(repoPath)) return false;

        var result = await RunGitAsync(repoPath, token, "branch", child, @ref);
        return result.ExitCode == 0;
    }

    public async Task<bool> DeleteBranchAsync(string owner, string name, string branch, CancellationToken token = default)
    {
        var repoPath = GetRepoPath(owner, name);
        if (!Directory.Exists(repoPath)) return false;

        // 1. Determine default branch
        var headResult = await RunGitAsync(repoPath, token, "symbolic-ref", "--short", "HEAD");
        var defaultBranch = headResult.ExitCode == 0 ? headResult.StdOut.Trim() : "master";

        // 2. Protect default branch
        if (string.Equals(branch, defaultBranch, StringComparison.OrdinalIgnoreCase))
        {
            return false; // Forbidden (matches the Bun server 403 behavior)
        }

        // 3. Delete branch
        var result = await RunGitAsync(repoPath, token, "branch", "-D", branch);
        return result.ExitCode == 0;
    }

    public async Task<string?> GetTreeAsync(string owner, string name, string branch, string path = "", CancellationToken token = default)
    {
        var repoPath = GetRepoPath(owner, name);
        if (!Directory.Exists(repoPath)) return null;

        var treeish = string.IsNullOrEmpty(path) ? branch : $"{branch}:{path}";
        var result = await RunGitAsync(repoPath, token, "ls-tree", "-l", treeish);
        
        return result.ExitCode == 0 ? result.StdOut : null;
    }

    public async Task<string?> GetBlobAsync(string owner, string name, string branch, string path, CancellationToken token = default)
    {
        var repoPath = GetRepoPath(owner, name);
        if (!Directory.Exists(repoPath) || string.IsNullOrEmpty(path)) return null;

        var processInfo = CreateGitProcess(repoPath, "show", $"{branch}:{path}");
        using var process = Process.Start(processInfo);
        if (process == null) return null;

        // Note: Reading raw BaseStream ensures binary files aren't corrupted by string encoding
        using var memoryStream = new MemoryStream();
        await process.StandardOutput.BaseStream.CopyToAsync(memoryStream, token);
        await process.WaitForExitAsync(token);

        if (process.ExitCode != 0) return null;

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public async Task<bool> SetBlobAsync(string owner, string name, string branch, string path, string content, CancellationToken token = default)
    {
        var repoPath = GetRepoPath(owner, name);
        if (!Directory.Exists(repoPath) || string.IsNullOrEmpty(path)) return false;

        var tempPath = Path.Combine(repoPath, ".temp_blob");
        var bytes = Convert.FromBase64String(content);
        await File.WriteAllBytesAsync(tempPath, bytes, token);

        try
        {
            var addResult = await RunGitAsync(repoPath, token, "add", path);
            if (addResult.ExitCode != 0) return false;

            var commitResult = await RunGitAsync(repoPath, token, "commit", "-m", $"Update {path}");
            return commitResult.ExitCode == 0;
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    public async Task<string> GetBranchesAsync(string owner, string name, CancellationToken token = default)
    {
        var repoPath = GetRepoPath(owner, name);
        if (!Directory.Exists(repoPath)) return string.Empty;

        var result = await RunGitAsync(repoPath, token, "branch", "--format=%(if)%(HEAD)%(then)*%(end)%(refname:short)");
        return result.StdOut;
    }

    public async Task<bool> LockAsync(string owner, string name, CancellationToken token = default)
    {
        var repoPath = GetRepoPath(owner, name);
        if (!Directory.Exists(repoPath)) return false;

        var hooksDir = Path.Combine(repoPath, "hooks");
        Directory.CreateDirectory(hooksDir); // Ensure it exists

        var hookPath = Path.Combine(hooksDir, "pre-receive");
        var script = "#!/bin/sh\n\necho \" Push rejected: Repository is locked for evaluation.\" >&2\nexit 1\n";

        await File.WriteAllTextAsync(hookPath, script, token);

        // Make executable (works natively in .NET 7+ on Unix/Linux systems)
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(hookPath, 
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupExecute | 
                UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
        }

        return true;
    }

    public Task<bool> UnlockAsync(string owner, string name, CancellationToken token = default)
    {
        var repoPath = GetRepoPath(owner, name);
        if (!Directory.Exists(repoPath)) return Task.FromResult(false);

        var hookPath = Path.Combine(repoPath, "hooks", "pre-receive");
        if (File.Exists(hookPath))
        {
            File.Delete(hookPath);
        }

        return Task.FromResult(true);
    }

    // ========================================================================
    // Private Helpers
    // ========================================================================

    private async Task<(int ExitCode, string StdOut, string StdErr)> RunGitAsync(string repoPath, CancellationToken token, params string[] args)
    {
        var processInfo = CreateGitProcess(repoPath, args);
        using var process = Process.Start(processInfo);
        if (process == null) return (-1, "", "Failed to start process");

        var stdoutTask = process.StandardOutput.ReadToEndAsync(token);
        var stderrTask = process.StandardError.ReadToEndAsync(token);

        await process.WaitForExitAsync(token);

        return (process.ExitCode, await stdoutTask, await stderrTask);
    }

    private ProcessStartInfo CreateGitProcess(string repoPath, params string[] args)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = repoPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in args)
        {
            processInfo.ArgumentList.Add(arg);
        }

        return processInfo;
    }

    private static void ForceDeleteDirectory(string path)
    {
        var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };

        // Git deeply nests read-only objects. We must strip the read-only attribute before deleting.
        foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
        {
            info.Attributes = FileAttributes.Normal;
        }

        directory.Delete(true);
    }
}