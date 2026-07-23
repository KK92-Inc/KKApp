// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Keycloak.AuthServices.Authorization;
using App.Backend.Core.Services.Interface;
using App.Backend.Models.Requests.SshKeys;

// ============================================================================

namespace App.Backend.API.Controllers;

/// <summary>
/// Controller for repository / git tracked related entities
/// </summary>
[ApiController, Route("git")]
public class GitController(ILogger<GitController> log, IGitService git, IUserService userService) : Controller
{
    [HttpGet("{id:guid}/branches")]
    [EndpointSummary("List repository branches")]
    [EndpointDescription("Retrieves a list of branches in the git repository associated with this entity.")]
    [ProtectedResource("repository", "repository:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetBranches(Guid id, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);
        if (entity is null) return NotFound();

        log.LogDebug("Git Entity: {git}", entity);
        var tree = await git.GetBranchesAsync(entity.Owner, entity.Name, token);
        return tree is null ? NotFound() : Content(tree, "text/plain");
    }

    [HttpGet("{id:guid}/tree/{branch}")]
    [HttpGet("{id:guid}/tree/{branch}/{*path}")]
    [EndpointSummary("Get file tree from repository")]
    [EndpointDescription("Retrieves the file tree at the given branch and path in the git repository associated with this entity.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("repository", "repository:read")]
    public async Task<ActionResult<string>> GetTree(
        Guid id, string branch, string? path, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);
        if (entity is null) return NotFound();

        var tree = await git.GetTreeAsync(entity.Owner, entity.Name, branch, path ?? string.Empty, token);
        if (tree is null) return NotFound();

        return Content(tree, "text/plain");
    }

    [HttpGet("{id:guid}/blob/{branch}/{*path}")]
    [EndpointSummary("Get file content from repository")]
    [EndpointDescription("Retrieves the content of a file in the git repository associated with this entity.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("repository", "repository:read")]
    public async Task<ActionResult<string>> GetBlob(
        Guid id, string branch, string path, CancellationToken token)
    {

        var entity = await git.FindByIdAsync(id, token);
        if (entity is null) return NotFound();

        var text = await git.GetBlobAsync(entity.Owner, entity.Name, branch, path, token);
        if (text is null) return NotFound();

        return Ok(text);
    }

    [HttpPut("{id:guid}/commit/{branch}")]
    [EndpointSummary("Commit to a repository")]
    [EndpointDescription("Pushes a given commit to the remote repository")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("repository", "repository:write")]
    public async Task<ActionResult> Commit(
        Guid id, string branch, CommitDTO commit, CancellationToken token)
    {

        var entity = await git.FindByIdAsync(id, token);
        if (entity is null) return NotFound();

        var user = await userService.FindByIdAsync(User.GetSID());
        if (user is null) return Forbid();

        var success = await git.Commit(entity.Owner, entity.Name, branch, new()
        {
            Files = commit.Files,
            Message = commit.Message,
            Author = new(user.Login, user.Details?.Email ?? "N/A")
        }, token);

        return success ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/branches/{ref}/{child}")]
    [EndpointSummary("Create a branch")]
    [EndpointDescription("Creates a new branch in the git repository associated with this entity, pointing at the given ref (branch or SHA).")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("repository", "repository:write")]
    public async Task<IActionResult> CreateBranch(Guid id, string @ref, string child, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);
        if (entity is null)
            return NotFound();

        var success = await git.CreateBranchAsync(entity.Owner, entity.Name, @ref, child, token);
        return success ? Created() : Conflict();
    }

    [HttpDelete("{id:guid}/branches/{branch}")]
    [EndpointSummary("Delete a branch")]
    [EndpointDescription("Deletes a branch from the git repository associated with this entity.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("repository", "repository:write")]
    public async Task<IActionResult> DeleteBranch(Guid id, string branch, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);
        if (entity is null)
            return NotFound();

        var success = await git.DeleteBranchAsync(entity.Owner, entity.Name, branch, token);
        return success ? NoContent() : Conflict();
    }

    [HttpPost("{id:guid}/lock")]
    [EndpointSummary("Lock repository")]
    [EndpointDescription("Locks the git repository to reject all pushes.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("repository", "repository:lock")]
    public async Task<IActionResult> LockRepository(Guid id, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);

        log.LogDebug("Locking Git Entity: {git}", entity);
        if (entity is null)
            return NotFound();

        var success = await git.LockAsync(entity.Owner, entity.Name, token);
        return success ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/unlock")]
    [EndpointSummary("Unlock repository")]
    [EndpointDescription("Unlocks a previously locked git repository.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("repository", "repository:lock")]
    public async Task<IActionResult> UnlockRepository(Guid id, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);

        log.LogDebug("Unlocking Git Entity: {git}", entity);
        if (entity is null)
            return NotFound();

        var success = await git.UnlockAsync(entity.Owner, entity.Name, token);
        return success ? NoContent() : NotFound();
    }
}
