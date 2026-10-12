// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.MCP;

// ============================================================================

/// <summary>
/// How much damage the tools are allowed to do. Ordered: each level includes the previous one.
/// </summary>
public enum McpMode
{
    /// <summary>
    /// Only tools that read data.
    /// </summary>
    ReadOnly = 0,

    /// <summary>
    /// Reversible changes, e.g: freeze / unfreeze.
    /// </summary>
    Write = 1,

    /// <summary>
    /// Irreversible changes, e.g: anonymize.
    /// </summary>
    Destructive = 2,
}

// ============================================================================

/// <summary>
/// Bound from the "Mcp" configuration section. Defaults to <see cref="McpMode.ReadOnly"/>.
/// Applies to every caller of the tools: external MCP clients AND the built-in agent.
/// </summary>
public class McpToolOptions
{
    public const string SectionName = "Mcp";

    public McpMode Mode { get; set; } = McpMode.ReadOnly;
}
