# KKApp MCP Server + Agent

Experimental. Lets staff administer the platform with prompts instead of a dedicated UI.

```
MCP/
  ToolOptions.cs      Mcp:Mode  ReadOnly (default) | Write | Destructive
  Tools/              One class per area, derive from MCPTool, mark with [McpServerToolType]
  Results/            Small records tools return (keep them slim, the model reads them)
  Agent/              The built-in agent used by the frontend (/agent/chat): AgentService + AgentToolbox
```

Two ways in, one set of tools:

| Entry | Who drives the model | Auth |
|---|---|---|
| `POST /mcp` | An external MCP client (VS Code, Claude Desktop, Inspector...) | Staff bearer token |
| `POST /agent/chat` | The backend itself, through `IChatClient` (OllamaSharp -> local Ollama) | Staff bearer token (forwarded by the frontend) |

Both run the *same* `[McpServerTool]` methods, in-process, as the calling staff member, behind the same `Mcp:Mode` gate.
Add a tool once and both entry points get it.

## Adding a tool

1. Add a public method to a class in `Tools/` with `[McpServerTool(Name = "snake_case", ReadOnly = true|false)]` and a `[Description]`.
2. `ReadOnly = true` only if it never changes data. The agent asks a human to approve every non read-only call.
3. Gate writes with `Require(McpMode.Write)` (or `Destructive`), wrap in `Safe(...)`, throw `McpException` for errors the model should see.

## Agent configuration (`appsettings.json` -> `Agent`)

| Key | Default | |
|---|---|---|
| `BaseUrl` | `http://localhost:11434` | Ollama |
| `Model` | `qwen3:8b` | Must support tool calling (`ollama show <model>` lists `tools`) |
| `Think` | unset | `false` speeds up thinking models (qwen3). Leave unset if your model rejects it |
| `ContextLength` | `8192` | Sent as `num_ctx` |
| `MaxSteps` | `8` | Max model round-trips per request |
| `TimeoutSeconds` | `300` | Per request to Ollama |

## Changing the model provider

`AgentService` only knows `Microsoft.Extensions.AI.IChatClient`. The single registration lives in `Services.cs` (`RegisterMcp`):
swap the `OllamaSharp.OllamaApiClient` for any other `IChatClient` and nothing else changes.
The `RawRepresentationFactory` in `AgentService` (num_ctx, think) is the only Ollama specific bit; other providers ignore it or need their own.
