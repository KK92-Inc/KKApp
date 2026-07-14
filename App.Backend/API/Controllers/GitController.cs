// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using System.Text;
using App.Backend.Domain.Entities;

namespace App.Backend.API.Controllers;

[ApiController]
[Route("git")]
public class GitController(ILogger<GitController> log, IGitService git) : Controller
{
    [HttpGet("{id:guid}/branches")]
    [EndpointSummary("List repository branches")]
    [EndpointDescription("Retrieves a list of branches in the git repository associated with this entity.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetBranches(Guid id, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);
        log.LogDebug("Git Entity: {git}", entity);
        if (entity is null) return NotFound();

        var tree = await git.GetBranchesAsync(entity.Owner, entity.Name, token);
        if (tree is null)
            return NotFound();

        return Content(tree, "text/plain");
    }

    [HttpGet("{id:guid}/tree/{branch}")]
    [HttpGet("{id:guid}/tree/{branch}/{*path}")]
    [EndpointSummary("Get file tree from repository")]
    [EndpointDescription("Retrieves the file tree at the given branch and path in the git repository associated with this entity.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetTree(
        Guid id,  string branch, string? path, CancellationToken token)
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
    public async Task<ActionResult<string>> GetBlob(
        Guid id, string branch, string path, CancellationToken token)
    {

        var entity = await git.FindByIdAsync(id, token);
        if (entity is null) return NotFound();

        var text = await git.GetBlobAsync(entity.Owner, entity.Name, branch, path, token);
        if (text is null) return NotFound();

        return Ok(text);
    }

    [HttpPost("{id:guid}/branches/{ref}/{child}")]
    [EndpointSummary("Create a branch")]
    [EndpointDescription("Creates a new branch in the git repository associated with this entity, pointing at the given ref (branch or SHA).")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateBranch(Guid id, string @ref, string child, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);
        if (entity is null)
            return NotFound();

        var success = await git.CreateBranchAsync(entity.Owner, entity.Name, @ref, child, token);
        if (!success) return Conflict();
        return Created();
    }

    [HttpDelete("{id:guid}/branches/{branch}")]
    [EndpointSummary("Delete a branch")]
    [EndpointDescription("Deletes a branch from the git repository associated with this entity.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBranch(Guid id, string branch, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);
        if (entity is null)
            return NotFound();

        var success = await git.DeleteBranchAsync(entity.Owner, entity.Name, branch, token);
        if (!success) return Conflict();
        return NoContent();
    }

    [HttpPost("{id:guid}/lock")]
    [EndpointSummary("Lock repository")]
    [EndpointDescription("Locks the git repository to reject all pushes and prevent modifications (e.g., during an evaluation).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LockRepository(Guid id, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);

        log.LogDebug("Locking Git Entity: {git}", entity);
        if (entity is null)
            return NotFound();

        var success = await git.LockAsync(entity.Owner, entity.Name, token);

        if (!success)
            return NotFound("Underlying repository not found.");

        return NoContent();
    }

    [HttpPost("{id:guid}/unlock")]
    [EndpointSummary("Unlock repository")]
    [EndpointDescription("Unlocks a previously locked git repository, allowing pushes and modifications again.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlockRepository(Guid id, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);

        log.LogDebug("Unlocking Git Entity: {git}", entity);
        if (entity is null)
            return NotFound();

        var success = await git.UnlockAsync(entity.Owner, entity.Name, token);

        if (!success)
            return NotFound("Underlying repository not found.");

        return NoContent();
    }
}
