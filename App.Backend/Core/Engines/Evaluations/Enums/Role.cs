// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Core.Engines.Evaluations.Enums;

/**
 * The role of the user in the context of the evaluation.
 * This is important for rules that need to differentiate between reviewers and reviewees.
 */
public enum Role
{
    /// <summary>
    /// They are being evaluated on whether they can review or not.
    /// </summary>
    Reviewer,

    /// <summary>
    /// They are being evaluated on whether they can request reviews or not.
    /// </summary>
    Reviewee
}
