// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Net;
using App.Backend.Core.Services.Interface;
using App.Backend.Core.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

/// <summary>
/// Typed HttpClient implementation for the repository management REST API.
/// </summary>
public class GitService : IGitService
{
    private readonly HttpClient _http;
    private readonly ILogger<GitService> _logger;

    public GitService(HttpClient http, IOptions<GitServiceOptions> options, ILogger<GitService> logger)
    {
        _http = http;
        _logger = logger;
        _http.BaseAddress = new Uri(options.Value.BaseUrl.TrimEnd('/') + "/");
    }

    // ========================================================================

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string owner, string name, CancellationToken token = default)
    {
        var response = await _http.GetAsync($"repo/{owner}/{name}", token);
        return response.StatusCode is HttpStatusCode.NoContent;
    }

    /// <inheritdoc />
    public async Task<bool> CreateAsync(string owner, string name, CancellationToken token = default)
    {
        var response = await _http.PostAsync($"repo/{owner}/{name}", null, token);

        return response.StatusCode switch
        {
            HttpStatusCode.Created => true,
            HttpStatusCode.Conflict => false,
            _ => throw new ServiceException(500, $"Unexpected status {response.StatusCode} creating repo {owner}/{name}")
        };
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string owner, string name, CancellationToken token = default)
    {
        var response = await _http.DeleteAsync($"repo/{owner}/{name}", token);

        return response.StatusCode switch
        {
            HttpStatusCode.NoContent => true,
            HttpStatusCode.NotFound => false,
            _ => throw new ServiceException(500, $"Unexpected status {response.StatusCode} deleting repo {owner}/{name}")
        };
    }

    /// <inheritdoc />
    public async Task<bool> RenameAsync(string owner, string name, string newName, CancellationToken token = default)
    {
        var response = await _http.PostAsync($"repo/{owner}/{name}/rename/{newName}", null, token);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => true,
            HttpStatusCode.NotFound or HttpStatusCode.Conflict => false,
            _ => throw new ServiceException(500, $"Unexpected status {response.StatusCode} renaming repo {owner}/{name} -> {newName}")
        };
    }

    /// <inheritdoc />
    public async Task<string?> GetTreeAsync(string owner, string name, string branch, string path = "", CancellationToken token = default)
    {
        var url = string.IsNullOrEmpty(path)
            ? $"repo/{owner}/{name}/tree/{branch}/"
            : $"repo/{owner}/{name}/tree/{branch}/{path}";

        var response = await _http.GetAsync(url, token);
        if (response.StatusCode is HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(token);
    }

    /// <inheritdoc />
    public async Task<string?> GetBlobAsync(string owner, string name, string branch, string path, CancellationToken token = default)
    {
        var response = await _http.GetAsync($"repo/{owner}/{name}/blob/{branch}/{path}", token);

        if (response.StatusCode is HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(token);
    }
}
