// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// ============================================================================

namespace App.Backend.API.Controllers;

/// <summary>
/// Controller for Git repository operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GitController(IGitService gitService, ILogger<GitController> logger) : ControllerBase
{
    /// <summary>
    /// Gets the Git server information.
    /// </summary>
    [HttpGet("info")]
    [AllowAnonymous]
    public ActionResult<object> GetInfo()
    {
        return Ok(new
        {
            HttpBaseUrl = gitService.HttpBaseUrl,
            Status = "Available"
        });
    }

    /// <summary>
    /// Lists files in a repository.
    /// </summary>
    /// <param name="repoName">The repository name.</param>
    /// <param name="path">The path within the repository.</param>
    /// <param name="reference">The git reference (branch/tag/commit).</param>
    /// <param name="recursive">Whether to list recursively.</param>
    /// <param name="token">Cancellation token.</param>
    [HttpGet("repos/{repoName}/files")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<GitFile>>> ListFiles(
        string repoName,
        [FromQuery] string path = "",
        [FromQuery] string reference = "HEAD",
        [FromQuery] bool recursive = false,
        CancellationToken token = default)
    {
        var localPath = Path.Combine(Path.GetTempPath(), "git-repos", repoName);

        // Clone if not exists locally
        if (!Directory.Exists(localPath))
        {
            try
            {
                await gitService.CloneAsync(repoName, localPath, null, token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to clone repository {RepoName}", repoName);
                return NotFound(new { Message = $"Repository '{repoName}' not found or inaccessible" });
            }
        }

        var files = await gitService.ListFilesAsync(localPath, path, reference, recursive, token);
        return Ok(files);
    }

    /// <summary>
    /// Gets a file from a repository.
    /// </summary>
    /// <param name="repoName">The repository name.</param>
    /// <param name="filePath">The file path within the repository.</param>
    /// <param name="reference">The git reference (branch/tag/commit).</param>
    /// <param name="token">Cancellation token.</param>
    [HttpGet("repos/{repoName}/files/{**filePath}")]
    [AllowAnonymous]
    public async Task<ActionResult<GitFile>> GetFile(
        string repoName,
        string filePath,
        [FromQuery] string reference = "HEAD",
        CancellationToken token = default)
    {
        var localPath = Path.Combine(Path.GetTempPath(), "git-repos", repoName);

        // Clone if not exists locally
        if (!Directory.Exists(localPath))
        {
            try
            {
                await gitService.CloneAsync(repoName, localPath, null, token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to clone repository {RepoName}", repoName);
                return NotFound(new { Message = $"Repository '{repoName}' not found or inaccessible" });
            }
        }

        var file = await gitService.GetFileAsync(localPath, filePath, reference, token);

        if (file == null)
        {
            return NotFound(new { Message = $"File '{filePath}' not found in repository '{repoName}'" });
        }

        return Ok(file);
    }

    /// <summary>
    /// Gets commits from a repository.
    /// </summary>
    /// <param name="repoName">The repository name.</param>
    /// <param name="reference">The git reference to start from.</param>
    /// <param name="maxCount">Maximum number of commits to return.</param>
    /// <param name="token">Cancellation token.</param>
    [HttpGet("repos/{repoName}/commits")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<GitCommit>>> GetCommits(
        string repoName,
        [FromQuery] string reference = "HEAD",
        [FromQuery] int maxCount = 50,
        CancellationToken token = default)
    {
        var localPath = Path.Combine(Path.GetTempPath(), "git-repos", repoName);

        // Clone if not exists locally
        if (!Directory.Exists(localPath))
        {
            try
            {
                await gitService.CloneAsync(repoName, localPath, null, token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to clone repository {RepoName}", repoName);
                return NotFound(new { Message = $"Repository '{repoName}' not found or inaccessible" });
            }
        }

        var commits = await gitService.GetCommitsAsync(localPath, reference, maxCount, token);
        return Ok(commits);
    }

    /// <summary>
    /// Gets branches from a repository.
    /// </summary>
    /// <param name="repoName">The repository name.</param>
    /// <param name="token">Cancellation token.</param>
    [HttpGet("repos/{repoName}/branches")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<GitBranch>>> GetBranches(
        string repoName,
        CancellationToken token = default)
    {
        var localPath = Path.Combine(Path.GetTempPath(), "git-repos", repoName);

        // Clone if not exists locally
        if (!Directory.Exists(localPath))
        {
            try
            {
                await gitService.CloneAsync(repoName, localPath, null, token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to clone repository {RepoName}", repoName);
                return NotFound(new { Message = $"Repository '{repoName}' not found or inaccessible" });
            }
        }

        var branches = await gitService.GetBranchesAsync(localPath, token);
        return Ok(branches);
    }

    /// <summary>
    /// Creates a new repository.
    /// </summary>
    /// <param name="request">The create request.</param>
    /// <param name="token">Cancellation token.</param>
    [HttpPost("repos")]
    [Authorize]
    public async Task<ActionResult<object>> CreateRepository(
        [FromBody] CreateRepositoryRequest request,
        CancellationToken token = default)
    {
        try
        {
            var url = await gitService.CreateRepositoryAsync(
                request.Name,
                request.Description,
                request.IsPrivate,
                token);

            return Created(url, new
            {
                Name = request.Name,
                Url = url,
                CloneUrl = $"{url}.git"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create repository {RepoName}", request.Name);
            return BadRequest(new { Message = $"Failed to create repository: {ex.Message}" });
        }
    }
}

/// <summary>
/// Request to create a new repository.
/// </summary>
public record CreateRepositoryRequest(string Name, string? Description = null, bool IsPrivate = false);
