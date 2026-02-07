// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Messages;

// ============================================================================

/// <summary>
/// Transport model
/// </summary>
/// <param name="Subject"></param>
/// <param name="View"></param>
/// <param name="Model"></param>
public sealed record EmailMessage(string Subject, string View, object? Model);
