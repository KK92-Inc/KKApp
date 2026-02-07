// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Enums;

/// <summary>
/// Flags enum representing the types of reviews a rubric supports.
/// A rubric can support multiple review types.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReviewKinds
{
    /// <summary>
    /// Supports self-reflection reviews.
    /// </summary>
    Self = 1 << 0,

    /// <summary>
    /// Supports peer reviews (in-person).
    /// </summary>
    Peer = 1 << 1,

    /// <summary>
    /// Supports async reviews (remote, any timezone).
    /// </summary>
    Async = 1 << 2,

    /// <summary>
    /// Supports automated reviews (LLM, tests, etc.).
    /// </summary>
    Auto = 1 << 3,

    /// <summary>
    /// Supports all review types.
    /// </summary>
    All = Self | Peer | Async | Auto
}
