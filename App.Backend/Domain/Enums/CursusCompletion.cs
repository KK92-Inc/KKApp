// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Domain.Enums;

/// <summary>
/// Controls how subscription chain enforcement works.
/// </summary>
public enum CursusCompletion
{
    /// <summary>
    /// Anyone can subscribe to any cursus, goal, or project independently.
    /// No hierarchy enforcement.
    /// </summary>
    Free,

    /// <summary>
    /// Subscriptions enforce the hierarchy when relationships exist:
    /// <list type="bullet">
    ///   <item>To subscribe to a goal that belongs to a cursus, the user must be enrolled in that cursus.</item>
    ///   <item>To subscribe to a project that belongs to a goal, the user must be subscribed to that goal.</item>
    /// </list>
    /// Orphan entities (not linked to any parent) can still be subscribed to freely.
    /// </summary>
    Restricted,
}
