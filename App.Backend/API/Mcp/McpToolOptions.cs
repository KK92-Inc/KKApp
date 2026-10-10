// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Mcp;

// ============================================================================

/// <summary>
/// How much damage the MCP tools are allowed to do. Ordered: each level includes the previous one.
/// </summary>
public enum McpMode
{
    /// <summary>Only tools that read data.</summary>
    ReadOnly = 0,

    /// <summary>Reversible changes (freeze / unfreeze).</summary>
    Write = 1,

    /// <summary>Irreversible changes (anonymize).</summary>
    Destructive = 2,
}

/// <summary>
/// Bound from the "Mcp" configuration section. Defaults to <see cref="McpMode.ReadOnly"/>.
/// </summary>
public class McpToolOptions
{
    public const string SectionName = "Mcp";

    public McpMode Mode { get; set; } = McpMode.ReadOnly;
}
