// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Query;
using App.Backend.Core.Services.Interface;
using App.Backend.Database;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using LibGit2Sharp;
using Renci.SshNet;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class GitService(DatabaseContext ctx, ISshClient client) : BaseService<Git>(ctx), IGitService
{
    public async Task AddCollaboratorAsync(Git entity, User user, EntityPermission depth, CancellationToken token = default)
    {
        var permission = depth switch
        {
            EntityPermission.Read => "read-only",
            EntityPermission.Write => "read-write",
            _ => "read-only"
        };

        var cmd = client.CreateCommand($"repo collab add {entity.Owner}/{entity.Name} {user.Login} {permission}");
        await cmd.ExecuteAsync(token);

        // TODO: Store collaborator info in the database

        if (cmd.ExitStatus != 0)
            throw new ServiceException($"Failed to add collaborator: {cmd.Error}");
    }

    public async Task RemoveCollaboratorAsync(Git entity, User user, CancellationToken token = default)
    {
        var cmd = client.CreateCommand($"repo collab remove {entity.Owner}/{entity.Name} {user.Login}");
        await cmd.ExecuteAsync(token);

        // TODO: Update collaborator info in the database

        if (cmd.ExitStatus != 0)
            throw new ServiceException($"Failed to remove collaborator: {cmd.Error}");
    }

    public async Task AddUserPublicKeyAsync(User user, string publicKey, CancellationToken token = default)
    {
        // Command: user add-pubkey <username> <public_key>
        var cmd = client.CreateCommand($"user add-pubkey {user.Login} {publicKey}");
        await cmd.ExecuteAsync(token);

        if (cmd.ExitStatus != 0)
            throw new ServiceException($"Failed to add user public key: {cmd.Error}");
    }

    public async Task RemoveUserPublicKeyAsync(User user, string publicKey, CancellationToken token = default)
    {
        // Command inferred from 'user add-pubkey' pattern.
        var cmd = client.CreateCommand($"user remove-pubkey {user.Login} {publicKey}");
        await cmd.ExecuteAsync(token);

        if (cmd.ExitStatus != 0)
            throw new ServiceException($"Failed to remove user public key: {cmd.Error}");
    }

    public async Task SetRepositoryVisibilityAsync(Git git, bool visible, CancellationToken token = default)
    {
        // Command: repo private <repo> <true|false>
        // visible = true implies private = false
        var isPrivate = !visible;
        var cmd = client.CreateCommand($"repo private {git.Owner}/{git.Name} {isPrivate.ToString().ToLower()}");
        await cmd.ExecuteAsync(token);

        if (cmd.ExitStatus != 0)
            throw new ServiceException($"Failed to set repository visibility: {cmd.Error}");
    }

    public async Task<IEnumerable<GitBranch>> GetBranchesAsync(Git git, IPagination pagination, CancellationToken token = default)
    {
        using var repo = new LibGit2Sharp.Repository();

        return Enumerable.Empty<GitBranch>();
        // return await Task.Run(() =>
        // {
        //     using var repo = new LibGit2Sharp.Repository(git.Path);
        //     return repo.Branches
        //         .Select(b => new GitBranch
        //         {
        //             Name = b.FriendlyName,
        //             IsRemote = b.IsRemote
        //         })
        //         .Skip(pagination.Page * pagination.Size)
        //         .Take(pagination.Size)
        //         .ToList();
        // }, token);
    }

    public async Task<IEnumerable<GitCommit>> GetCommitsAsync(Git git, IPagination pagination, string reference = "HEAD", CancellationToken token = default)
    {
        return Enumerable.Empty<GitCommit>();
        // return await Task.Run(() =>
        // {
        //     using var repo = new LibGit2Sharp.Repository(git.Path);
        //     var filter = new LibGit2Sharp.CommitFilter
        //     {
        //         SortBy = LibGit2Sharp.CommitSortStrategies.Time
        //     };

        //     return repo.Commits.QueryBy(filter)
        //         .Skip(pagination.Offset)
        //         .Take(pagination.Limit)
        //         .Select(c => new GitCommit
        //         {
        //             Hash = c.Sha,
        //             Message = c.MessageShort,
        //             AuthorName = c.Author.Name,
        //             AuthorEmail = c.Author.Email,
        //             Date = c.Author.When.DateTime
        //         })
        //         .ToList();
        // }, token);
    }

    public async Task<GitFile?> GetFileAsync(Git git, string path, string reference = "HEAD", CancellationToken token = default)
    {
        Repository.Init("./lel", true);
        return null;
        // return await Task.Run(() =>
        // {
        //     using var repo = new LibGit2Sharp.Repository(git.Path);
            
        //     // Find the commit object
        //     var commit = repo.Lookup<LibGit2Sharp.Commit>(reference);
        //     if (commit == null) return null;

        //     // Find the entry in the tree
        //     var treeEntry = commit[path];
        //     if (treeEntry == null || treeEntry.TargetType != LibGit2Sharp.TreeEntryTargetType.Blob)
        //         return null;

        //     var blob = (LibGit2Sharp.Blob)treeEntry.Target;
            
        //     // Read content
        //     using var contentStream = blob.GetContentStream();
        //     using var reader = new StreamReader(contentStream);
            
        //     return new GitFile
        //     {
        //         Path = path,
        //         Content = reader.ReadToEnd()
        //     };
        // }, token);
    }

    public async Task<IEnumerable<GitFile>> ListFilesAsync(Git git, IPagination pagination, string directory = "", string reference = "HEAD", bool recursive = false, CancellationToken token = default)
    {
        return Enumerable.Empty<GitFile>();
        // return await Task.Run(() =>
        // {
        //     using var repo = new LibGit2Sharp.Repository(git.Path);
        //     var commit = repo.Lookup<LibGit2Sharp.Commit>(reference);
        //     if (commit == null) return Enumerable.Empty<GitFile>();

        //     var tree = commit.Tree;

        //     // If a directory is specified, navigate to that subtree
        //     if (!string.IsNullOrEmpty(directory))
        //     {
        //         var entry = commit[directory];
        //         if (entry == null || entry.TargetType != LibGit2Sharp.TreeEntryTargetType.Tree)
        //             return Enumerable.Empty<GitFile>();
                
        //         tree = (LibGit2Sharp.Tree)entry.Target;
        //     }

        //     var files = new List<GitFile>();

        //     if (recursive)
        //     {
        //         foreach (var item in tree)
        //         {
        //             if (item.TargetType == LibGit2Sharp.TreeEntryTargetType.Blob)
        //             {
        //                 files.Add(new GitFile { Path = item.Path });
        //             }
        //         }
        //     }
        //     else
        //     {
        //         // Non-recursive: just list immediate children
        //         foreach (var item in tree)
        //         {
        //             files.Add(new GitFile { Path = item.Path });
        //         }
        //     }

        //     return files
        //         .Skip(pagination.Offset)
        //         .Take(pagination.Limit);
        // }, token);
    }
}