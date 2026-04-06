// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using System.Text;

namespace App.Backend.API.Controllers;

[ApiController]
[Route("git")]
public class GitController(ILogger<GitController> log, IGitService git) : Controller
{
    [HttpGet("{id:guid}/branches")]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetTree(Guid id, string branch, string? path, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);
        log.LogDebug("Git Entity: {git}", entity);
        if (entity is null) return NotFound();

        var tree = await git.GetTreeAsync(entity.Owner, entity.Name, branch, path ?? string.Empty, token);
        if (tree is null)
            return NotFound();

        return Content(tree, "text/plain");
    }

    [HttpGet("{id:guid}/blob/{branch}/{*path}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetBlob(Guid id, string branch, string path, CancellationToken token)
    {
        var entity = await git.FindByIdAsync(id, token);
        if (entity is null)
            return NotFound();

        var text = await git.GetBlobAsync(entity.Owner, entity.Name, branch, path, token);
        if (text is null) return NotFound();
        return Ok(text ?? string.Empty);
    }
}
