// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.MCP.Agent;

// ============================================================================

/// <summary>
/// Bound from the "Agent" configuration section.
/// </summary>
public class AgentOptions
{
    public const string SectionName = "Agent";

    /// <summary>
    /// Name of the connection string (`ConnectionStrings:qwen3-8b`) holding the Ollama endpoint and model,
    /// e.g. `Endpoint=http://localhost:11434;Model=qwen3:8b`. The AppHost injects it. The model must support tool calling.
    /// </summary>
    public const string ConnectionName = "qwen3-8b";

    /// <summary>
    /// Sent as Ollama's `think` flag when set. Leave null if your model rejects it.
    /// </summary>
    public bool? Think { get; set; }

    /// <summary>
    /// Context window (`num_ctx`). Tool results can be large.
    /// </summary>
    public int ContextLength { get; set; } = 8192;

    /// <summary>
    /// Maximum model round-trips for a single request.
    /// </summary>
    public int MaxSteps { get; set; } = 8;

    /// <summary>
    /// Timeout for a single request to Ollama.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;
}
