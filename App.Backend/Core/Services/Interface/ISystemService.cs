// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Linq.Expressions;
using App.Backend.Core.Query;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

namespace App.Backend.Core.Services.Interface;

/// <summary>
/// Service that lets you check and initialize the entire application.
/// </summary>
public interface ISystemService
{
    /// <summary>
    /// Check if there is a systeme entry.
    /// </summary>
    /// <param name="token">The Cancellation Token to abort the request.</param>
    /// <returns>The system check or null.</returns>
    Task<Domain.Entities.System?> CheckAsync(CancellationToken token = default);

    /// <summary>
    /// Bootstrap the entire application process to create the necessary stuff.
    /// </summary>
    /// <param name="token">The Cancellation Token to abort the request.</param>
    Task<User> InitializeAsync(string Login, string Password, string Email, CancellationToken token = default);
}