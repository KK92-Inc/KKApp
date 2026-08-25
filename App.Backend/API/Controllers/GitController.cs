// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Keycloak.AuthServices.Authorization;
using App.Backend.Core.Services.Interface;
using App.Backend.Models.Requests.SshKeys;
using App.Backend.API.Utils;
using Microsoft.AspNetCore.Authorization;
using App.Git.Models.Responses;
using App.Git.Models.Requests;

// ============================================================================

namespace App.Backend.API.Controllers;

/// <summary>
/// Controller for repository / git tracked related entities
/// </summary>
[ApiController, Route("git")]
public class GitController(IMemberService memberService, IGitService git, IUserService userService, IAuthorizationService auth) : Controller
{
    [HttpGet("{id:guid}/branches")]
    [RequireScope("repository")]
    [ProtectedResource("repository", "repository:read")]
    [EndpointSummary("List repository branches")]
    [EndpointDescription("Retrieves a list of branches in the git repository associated with this entity.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BranchDTO[]>> GetBranches(Guid id, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);
        if (entity is null) return NotFound();

        var branches = await git.GetBranchesAsync(entity.Owner, entity.Name, token);
        return Ok(branches);
    }

    [HttpGet("{id:guid}/tree/{branch}")]
    [HttpGet("{id:guid}/tree/{branch}/{*path}")]
    [RequireScope("repository")]
    [ProtectedResource("repository", "repository:read")]
    [EndpointSummary("Get file tree from repository")]
    [EndpointDescription("Retrieves the file tree at the given branch and path in the git repository associated with this entity.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TreeDTO[]>> GetTree(Guid id, string branch, string? path, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);
        if (entity is null) return NotFound();

        var tree = await git.GetTreeAsync(entity.Owner, entity.Name, branch, path ?? string.Empty, token);
        if (tree is null) return NotFound();

        return Ok(tree);
    }

    [HttpGet("{id:guid}/blob/{branch}/{*path}")]
    [RequireScope("repository")]
    [ProtectedResource("repository", "repository:read")]
    [EndpointSummary("Get file content from repository")]
    [EndpointDescription("Retrieves the content of a file in the git repository associated with this entity.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetBlob(
        Guid id, string branch, string path, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);
        if (entity is null) return NotFound();

        var blob = await git.GetBlobAsync(entity.Owner, entity.Name, branch, path, token);
        if (blob is null) return NotFound();

        return File(blob, "application/octet-stream");
    }

    [HttpPut("{id:guid}/commit/{branch}")]
    [RequireScope("repository")]
    [ProtectedResource("repository", "repository:write")]
    [EndpointSummary("Commit to a repository")]
    [EndpointDescription("Pushes a given commit to the remote repository")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Commit(
        Guid id, string branch, PostCommitDTO commit, CancellationToken token)
    {
        var result = await auth.AuthorizeAsync(User, "staff");
        if (!result.Succeeded && !await Access(id, User.GetSID(), token))
            return Forbid();

        var entity = await git.FindByIdAsync(id, token);
        if (entity is null) return NotFound();

        var user = await userService.FindByIdAsync(User.GetSID(), token);
        if (user is null) return Forbid();

        var success = await git.Commit(entity.Owner, entity.Name, branch, new()
        {
            Author = user.Login,
            Email = user.Details?.Email ?? "N/A",
            Message = commit.Message,
            Files = commit.Files,
        }, token);

        return success ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/branches/{ref}/{child}")]
    [RequireScope("repository")]
    [ProtectedResource("repository", "repository:write")]
    [EndpointSummary("Create a branch")]
    [EndpointDescription("Creates a new branch in the git repository associated with this entity, pointing at the given ref (branch or SHA).")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateBranch(Guid id, string @ref, string child, CancellationToken token)
    {
        var result = await auth.AuthorizeAsync(User, "staff");
        if (!result.Succeeded && !await Access(id, User.GetSID(), token))
            return Forbid();

        var entity = await git.FindByIdAsync(id, token);
        if (entity is null) return NotFound();

        var success = await git.CreateBranchAsync(entity.Owner, entity.Name, @ref, child, token);
        return success ? Created() : Conflict();
    }

    [HttpDelete("{id:guid}/branches/{branch}")]
    [RequireScope("repository")]
    [ProtectedResource("repository", "repository:write")]
    [EndpointSummary("Delete a branch")]
    [EndpointDescription("Deletes a branch from the git repository associated with this entity.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBranch(Guid id, string branch, CancellationToken token)
    {
        var result = await auth.AuthorizeAsync(User, "staff");
        if (!result.Succeeded && !await Access(id, User.GetSID(), token))
            return Forbid();

        var entity = await git.FindByIdAsync(id, token);
        if (entity is null) return NotFound();

        var success = await git.DeleteBranchAsync(entity.Owner, entity.Name, branch, token);
        return success ? NoContent() : Conflict();
    }

    [HttpPost("{id:guid}/lock")]
    [Authorize(Policy = "staff")]
    [ProtectedResource("repository", "repository:lock")]
    [EndpointSummary("Lock repository")]
    [EndpointDescription("Locks the git repository to reject all pushes.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LockRepository(Guid id, CancellationToken token)
    {
        // NOTE(W2): Regular users should *NEVER* be able to lock their own repo
        // only a staff elevated service account or account may.
        var entity = await git.FindByIdAsync(id, token);
        if (entity is null) return NotFound();

        var success = await git.LockAsync(entity.Owner, entity.Name, token);
        return success ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/unlock")]
    [Authorize(Policy = "staff")]
    [EndpointSummary("Unlock repository")]
    [EndpointDescription("Unlocks a previously locked git repository.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProtectedResource("repository", "repository:lock")]
    public async Task<IActionResult> UnlockRepository(Guid id, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);
        if (entity is null) return NotFound();

        var success = await git.UnlockAsync(entity.Owner, entity.Name, token);
        return success ? NoContent() : NotFound();
    }

    private async Task<bool> Access(Guid id, Guid user, CancellationToken token)
    {
        return await memberService.FindByGitAndUserId(id, user, token) is not null;
    }
}
