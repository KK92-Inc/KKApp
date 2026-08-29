// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================
// Internal-only HTTP API over bare git repositories on disk. Not exposed
// publicly - App.Backend.API is the only intended caller (see Git__BaseUrl
// in apphost.cs), so there is deliberately no authentication layer here.
// ============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using App.Git.Models.Requests;
using App.Git.Models.Responses;
using LibGit2Sharp;

// ============================================================================

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

var root = Path.GetFullPath(builder.Configuration["REPOSITORY_DIRECTORY"]
    ?? Path.Combine(Directory.GetCurrentDirectory(), "tmp", "repos"));

// ============================================================================

app.MapGet("/health", () => Results.Ok("OK"));
var repos = app.MapGroup("/repo/{owner}/{name}").WithTags("Repositories");

// ============================================================================
// CRUD Repository
// ============================================================================

repos.MapGet("/", (string owner, string name) =>
{
    var dir = Path.Combine(root, owner, name);
    return Repository.IsValid(dir) ? Results.NoContent() : Results.NotFound();
});

repos.MapPost("/", (string owner, string name) =>
{
    var dir = Path.Combine(root, owner, name);
    if (Repository.IsValid(dir)) return Results.Conflict();

    Repository.Init(dir, isBare: true);
    return Results.Created();
});

repos.MapDelete("/", (string owner, string name) =>
{
    var dir = Path.Combine(root, owner, name);
    if (!Repository.IsValid(dir)) return Results.NotFound();

    // NOTE(W2): Preserve the trashed directory for record sake.
    var id = Guid.CreateVersion7().ToString();
    var trash = Path.Combine(root, ".trash", owner, name, id);
    Directory.CreateDirectory(Path.GetDirectoryName(trash)!);
    Directory.Move(dir, trash);
    return Results.NoContent();
});

repos.MapPost("/rename/{target}", (string owner, string name, string target) =>
{
    var oldPath = Path.Combine(root, owner, name);
    var newPath = Path.Combine(root, owner, target);

    if (Repository.IsValid(newPath)) return Results.Conflict();
    if (!Repository.IsValid(oldPath)) return Results.NotFound();

    Directory.Move(oldPath, newPath);
    return Results.NoContent();
});

// ============================================================================
// CRUD Branch
// ============================================================================

var branches = repos.MapGroup("/branches").WithTags("Branches");
branches.MapGet("/", (string owner, string name) =>
{
    var dir = Path.Combine(root, owner, name);
    if (!Repository.IsValid(dir))
        return Results.NotFound();

    using var repo = new Repository(dir);
    var branches = repo.Branches
        .Where(b => !b.IsRemote)
        .Select(b => new BranchDTO(
            b.FriendlyName,
            b.IsCurrentRepositoryHead
        ));

    return Results.Ok(branches.ToArray());
});

branches.MapPost("/{ref}/{**child}", (string owner, string name, string @ref, string child) =>
{
    var dir = Path.Combine(root, owner, name);
    if (!Repository.IsValid(dir))
        return Results.NotFound();

    using var repo = new Repository(dir);
    var commit = repo.Lookup<Commit>(@ref);
    if (commit is null)
        return Results.NotFound($"Reference '{@ref}' not found");
    if (repo.Branches[child] is not null)
        return Results.Conflict($"Branch '{child}' already exists");

    repo.CreateBranch(child, commit);
    return Results.Created();
});

branches.MapDelete("/{**branch}", (string owner, string name, string branch) =>
{
    var dir = Path.Combine(root, owner, name);
    if (!Repository.IsValid(dir))
        return Results.NotFound();

    using var repo = new Repository(dir);
    var target = repo.Branches[branch];
    if (target is null) return Results.NotFound();
    if (target.IsCurrentRepositoryHead) return Results.UnprocessableEntity();

    repo.Branches.Remove(target);
    return Results.NoContent();
});

// ============================================================================
// Trees & Blobs
// ============================================================================

repos.MapGet("/tree/{branch}/{**subpath}", (string owner, string name, string branch, string? subpath) =>
{
    var dir = Path.Combine(root, owner, name);
    if (!Repository.IsValid(dir))
        return Results.NotFound();

    using var repo = new Repository(dir);
    var commit = repo.Lookup<Commit>(branch);
    if (commit is null) return Results.NotFound();

    var tree = commit.Tree;
    if (!string.IsNullOrEmpty(subpath))
    {
        var target = tree[subpath];
        if (target is null || target.TargetType is not TreeEntryTargetType.Tree)
            return Results.NotFound();
        tree = (Tree)target.Target;
    }

    var filter = new CommitFilter
    {
        IncludeReachableFrom = commit,
        SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time,
    };

    var result = new List<TreeDTO>();
    foreach (var entry in tree)
    {
        var entryPath = string.IsNullOrEmpty(subpath) ? entry.Name : $"{subpath}/{entry.Name}";
        var lastChange = repo.Commits.QueryBy(entryPath, filter).FirstOrDefault();

        var lastCommit = lastChange is null
            ? null
            : new CommitDTO(
                lastChange.Commit.Sha,
                lastChange.Commit.MessageShort,
                lastChange.Commit.Author.Name,
                lastChange.Commit.Author.When);

        result.Add(new TreeDTO(
            entry.Path,
            entry.TargetType is TreeEntryTargetType.Tree,
            entry.Target is Blob blob ? blob.Size : 0,
            lastCommit!)
        );
    }

    return Results.Ok(result);
}).WithTags("Trees");

repos.MapGet("/blob/{branch}/{**path}", (string owner, string name, string branch, string? path) =>
{
    var dir = Path.Combine(root, owner, name);
    if (!Repository.IsValid(dir))
        return Results.NotFound();

    using var repo = new Repository(dir);
    var commit = repo.Lookup<Commit>(branch);
    var entry = commit?.Tree[path];
    if (entry is null || entry.TargetType is not TreeEntryTargetType.Blob)
        return Results.NotFound();

    var blob = entry.Target.Peel<Blob>();
    using var stream = blob.GetContentStream();
    using var buffer = new MemoryStream();
    stream.CopyTo(buffer);

    return Results.Bytes(buffer.ToArray(), "application/octet-stream");
}).WithTags("Blobs");

// ============================================================================
// Commits
// ============================================================================

repos.MapPut("/commit/{branch}", (string owner, string name, string branch, PostCommitWithAuthorDTO payload, HttpResponse response) =>
{
    var dir = Path.Combine(root, owner, name);
    if (!Repository.IsValid(dir))
        return Results.NotFound();

    using var repo = new Repository(dir);

    var target = repo.Branches[branch];
    bool initial = repo.Info.IsHeadUnborn;
    if (!initial && target is null)
        return Results.NotFound($"Branch '{branch}' does not exist.");

    // Seed the TreeDefinition with existing files if this isn't an initial commit
    var definition = (!initial && target?.Tip is not null)
        ? TreeDefinition.From(target.Tip.Tree)
        : new TreeDefinition();

    var parents = initial ? Array.Empty<Commit>() : [target!.Tip];
    var files = payload.Files // Deduplicate (Last File Wins)
        .GroupBy(f => f.Path)
        .Select(g => g.Last());

    foreach (var file in files)
    {
        using var stream = new MemoryStream(Convert.FromBase64String(file.Content));
        var blob = repo.ObjectDatabase.CreateBlob(stream);
        definition.Add(file.Path, blob, Mode.NonExecutableFile);
    }

    var tree = repo.ObjectDatabase.CreateTree(definition);
    var signature = new Signature(payload.Author, payload.Email, DateTimeOffset.Now);
    var commit = repo.ObjectDatabase.CreateCommit(signature, signature, payload.Message, tree, parents, false);
    string refName = $"refs/heads/{branch}";

    if (initial)
    {
        repo.Refs.Add(refName, commit.Id);
        repo.Refs.UpdateTarget("HEAD", refName);
    }
    else
    {
        repo.Refs.Add(refName, commit.Id, allowOverwrite: true);
        if (repo.Head.Tip is null)
            repo.Refs.UpdateTarget("HEAD", refName);
    }

    response.Headers["x-sha"] = commit.Sha;
    return Results.NoContent();
}).WithTags("Commits");

// ============================================================================
// Hooks (Locking)
// ============================================================================
// Kept as direct file writes, same as the TS version - there's no git
// plumbing operation behind these, just dropping a hook script on disk.

const UnixFileMode mode =
    UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
    | UnixFileMode.GroupRead | UnixFileMode.GroupExecute
    | UnixFileMode.OtherRead | UnixFileMode.OtherExecute;

repos.MapPost("/lock", async (string owner, string name) =>
{
    var dir = Path.Combine(root, owner, name);
    if (!Repository.IsValid(dir)) return Results.NotFound();

    using var repo = new Repository(dir);
    var hook = Path.Combine(repo.Info.Path, "hooks", "pre-receive");
    await File.WriteAllTextAsync(hook, "#!/bin/sh\n\necho \" Push rejected: Repository is locked.\" >&2\nexit 1\n");
    if (OperatingSystem.IsLinux())
        File.SetUnixFileMode(hook, mode);
    else if (OperatingSystem.IsWindows()) // NOTE(W2): If some madman decides to do this, holy shit.
        File.SetAttributes(hook, FileAttributes.Normal);

    return Results.Ok();
}).WithTags("Hooks");

repos.MapPost("/unlock", (string owner, string name) =>
{
    var dir = Path.Combine(root, owner, name);
    if (!Repository.IsValid(dir)) return Results.NotFound();

    using var repo = new Repository(dir);
    var hook = Path.Combine(repo.Info.Path, "hooks", "pre-receive");
    if (File.Exists(hook)) File.Delete(hook);

    return Results.Ok();
}).WithTags("Hooks");

// ============================================================================

app.Run();

