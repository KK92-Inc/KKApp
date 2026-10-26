// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

// ============================================================================

namespace App.Backend.Domain.Values;

/// <summary>
/// How many people signed up for a session, and how many of them were verified as present.
/// </summary>
public readonly record struct ApplyCounts(int Registered, int Attended);
