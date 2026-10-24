// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using App.Backend.Core;
using App.Backend.Models.Requests.Agent;
using App.Backend.Models.Responses.Entities;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp.Models.Exceptions;
using OllamaChatRequest = OllamaSharp.Models.Chat.ChatRequest;
using OllamaRequestOptions = OllamaSharp.Models.RequestOptions;
using OllamaThinkValue = OllamaSharp.Models.Chat.ThinkValue;

// ============================================================================

namespace App.Backend.API.MCP.Agent;

public sealed class AgentService(
    IChatClient chat,
    IOptions<AgentOptions> options,
    IOptions<McpToolOptions> mcp,
    IHttpContextAccessor http,
    IServiceProvider services,
    TimeProvider time,
    ILogger<AgentService> log)
{
    private static readonly JsonElement EmptyObject = JsonDocument.Parse("{}").RootElement.Clone();

    // Some models inline their reasoning instead of using Ollama's separate field.
    private static readonly Regex ThinkBlock =
        new(@"<think>.*?</think>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

    public async Task<AgentReplyDO> ChatAsync(PostAgentChatRequestDTO request, CancellationToken ct)
    {
        Validate(request);

        var cfg = options.Value;
        var tools = AgentToolbox.Build(services);
        var byName = tools.ToDictionary(t => t.Name);

        var chatOptions = new ChatOptions
        {
            // Declared only: we run the tools ourselves (see class remarks).
            Tools = [.. tools.Select(t => (AITool)t.Function)],

            // Ollama specific knobs that IChatClient has no portable equivalent for.
            // OllamaSharp calls this once per request, so return a fresh object each time.
            RawRepresentationFactory = _ => new OllamaChatRequest
            {
                Options = new OllamaRequestOptions { NumCtx = cfg.ContextLength },
                Think = cfg.Think is { } think ? new OllamaThinkValue(think) : (OllamaThinkValue?)null,
            },
        };

        // The system prompt is built here, never taken from the client.
        var conversation = new List<ChatMessage> { new(ChatRole.System, SystemPrompt(tools)) };
        conversation.AddRange(ToChatMessages(request.Messages));
        var produced = new List<AgentMessageDTO>();

        async Task RunCallsAsync(IEnumerable<AgentToolCallDTO> calls, bool approved)
        {
            foreach (var call in calls)
            {
                var tool = byName.GetValueOrDefault(call.Name);
                string result;
                if (tool is null)
                    result = $"Error: unknown tool '{call.Name}'.";
                else if (!tool.ReadOnly && !approved)
                    result = "The human declined this action. Do not retry it; ask what they want instead.";
                else
                    result = await AgentToolbox.InvokeAsync(tool, call.Arguments, log, ct);

                conversation.Add(new ChatMessage(ChatRole.Tool, new List<AIContent>
                {
                    new FunctionResultContent($"call_{conversation.Count}", ToResult(result)),
                }));
                produced.Add(new AgentMessageDTO { Role = "tool", ToolName = call.Name, Content = result });
            }
        }

        AgentPendingCallDO Describe(AgentToolCallDTO call)
        {
            var tool = byName.GetValueOrDefault(call.Name);
            return new AgentPendingCallDO
            {
                Name = call.Name,
                Description = tool?.Description ?? "Unknown tool.",
                Arguments = call.Arguments,
                RequiresApproval = tool is { ReadOnly: false },
            };
        }

        // Resuming after the human decided.
        if (request.Decision is { } decision)
        {
            var waiting = request.Messages[^1];
            await RunCallsAsync(waiting.ToolCalls!, decision == AgentDecision.Approve);
        }

        for (var step = 0; step < cfg.MaxSteps; step++)
        {
            var response = await AskModelAsync(conversation, chatOptions, ct);
            conversation.AddRange(response.Messages);

            var message = ToDto(response);
            produced.Add(message);

            if (message.ToolCalls is not { Count: > 0 } calls)
                return new AgentReplyDO { Messages = produced };

            // Hold back the whole batch if any call changes data.
            if (calls.Any(c => byName.GetValueOrDefault(c.Name) is { ReadOnly: false }))
            {
                return new AgentReplyDO
                {
                    Messages = produced,
                    Pending = new AgentPendingDO { Calls = calls.Select(Describe).ToList() },
                };
            }

            await RunCallsAsync(calls, approved: true);
        }

        produced.Add(new AgentMessageDTO
        {
            Role = "assistant",
            Content = $"I stopped after {cfg.MaxSteps} steps without reaching an answer. Try asking in smaller steps.",
        });
        return new AgentReplyDO { Messages = produced };
    }

    // Model
    // ============================================================================

    /// <summary>
    /// Calls the model and turns provider failures into errors a human can act on.
    /// </summary>
    private async Task<ChatResponse> AskModelAsync(List<ChatMessage> conversation, ChatOptions chatOptions, CancellationToken ct)
    {
        var cfg = options.Value;
        var meta = chat.GetService<ChatClientMetadata>();
        try
        {
            return await chat.GetResponseAsync(conversation, chatOptions, ct);
        }
        catch (ModelDoesNotSupportToolsException)
        {
            throw new ServiceException(502, "The model can't use tools.",
                $"'{meta?.DefaultModelId}' doesn't support tool calling. Pick another model: `ollama show <model>` must list 'tools'.");
        }
        catch (OllamaException e)
        {
            log.LogError(e, "Ollama returned an error");
            throw new ServiceException(502, "The language model returned an error.", Shorten(e.Message));
        }
        catch (HttpRequestException e) when (e.StatusCode is null)
        {
            log.LogError(e, "Could not reach Ollama at {Url}", meta?.ProviderUri);
            throw new ServiceException(503, "The language model is unavailable.",
                $"Could not reach Ollama at {meta?.ProviderUri}. Is it running?");
        }
        catch (HttpRequestException e)
        {
            log.LogError(e, "Ollama request failed");
            throw new ServiceException(502, "The language model returned an error.", Shorten(e.Message));
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new ServiceException(504, "The language model timed out.",
                $"No answer within {cfg.TimeoutSeconds}s. Try a smaller model or a shorter question.");
        }
    }

    private static string Shorten(string text) => text.Length > 500 ? text[..500] : text;

    private static void Validate(PostAgentChatRequestDTO request)
    {
        var last = request.Messages[^1];

        if (request.Decision is null)
        {
            if (last.Role != "user")
                throw new ServiceException(400, "The conversation must end with a user message.");
            if (string.IsNullOrWhiteSpace(last.Content))
                throw new ServiceException(400, "The message is empty.");
            if (last.Content.Length > 4000)
                throw new ServiceException(400, "The message is too long (max 4000 characters).");
        }
        else if (last is not { Role: "assistant", ToolCalls.Count: > 0 })
        {
            throw new ServiceException(400, "There is nothing waiting for approval.");
        }
    }

    private string SystemPrompt(IReadOnlyList<AgentTool> tools)
    {
        var user = http.HttpContext?.User.FindFirstValue("preferred_username") ?? "a staff member";
        var writes = string.Join(", ", tools.Where(t => !t.ReadOnly).Select(t => t.Name));

        var approval = writes.Length == 0
            ? ""
            : $"""

            - These tools change data: {writes}. The system automatically pauses and asks the human to approve them, so do NOT ask for confirmation in text. When the request is clear, call the tool. Only ask a question when something is ambiguous (for example several users match).
            - When a request uses relative dates ("tomorrow", "for two weeks"), work out ISO 8601 timestamps from the current time below.
            """;

        return $"""
            You are the administrative assistant of the KKApp learning platform, talking to the staff member '{user}'.
            Current time (UTC): {time.GetUtcNow():O}. Tool mode: {mcp.Value.Mode}.

            Rules:
            - Use the provided tools for anything about users or freezes. Never invent data.
            - Never guess a user id. Find it with list_users first.
            - If a tool returns an error, explain it plainly. Do not retry blindly.
            - Reply in Markdown. Present lists of records as a Markdown table. Be concise.{approval}
            """;
    }

    /// <summary>
    /// Rebuilds the conversation from what the client sent. The DTOs carry no call ids, so they are
    /// invented and paired in order (a tool result answers the oldest unanswered call).
    /// </summary>
    private static List<ChatMessage> ToChatMessages(IEnumerable<AgentMessageDTO> messages)
    {
        var result = new List<ChatMessage>();
        var unanswered = new Queue<string>();
        var counter = 0;

        foreach (var m in messages)
        {
            switch (m.Role)
            {
                case "assistant":
                    var contents = new List<AIContent>();
                    if (!string.IsNullOrEmpty(m.Content))
                        contents.Add(new TextContent(m.Content));

                    foreach (var call in m.ToolCalls ?? new List<AgentToolCallDTO>())
                    {
                        var id = $"call_{counter++}";
                        unanswered.Enqueue(id);
                        contents.Add(new FunctionCallContent(id, call.Name, ToArguments(call.Arguments)));
                    }

                    result.Add(new ChatMessage(ChatRole.Assistant, contents));
                    break;

                case "tool":
                    var callId = unanswered.Count > 0 ? unanswered.Dequeue() : $"call_{counter++}";
                    result.Add(new ChatMessage(ChatRole.Tool, new List<AIContent>
                    {
                        new FunctionResultContent(callId, ToResult(m.Content)),
                    }));
                    break;

                default:
                    result.Add(new ChatMessage(ChatRole.User, m.Content));
                    break;
            }
        }

        return result;
    }

    private static AgentMessageDTO ToDto(ChatResponse response)
    {
        var calls = response.Messages
            .SelectMany(m => m.Contents)
            .OfType<FunctionCallContent>()
            .Select(c => new AgentToolCallDTO { Name = c.Name, Arguments = ToElement(c.Arguments) })
            .ToList();

        var content = ThinkBlock.Replace(response.Text, "").Trim();
        var hasCalls = calls.Count > 0;
        if (!hasCalls && content.Length == 0)
            content = "(The model returned an empty response.)";

        return new AgentMessageDTO { Role = "assistant", Content = content, ToolCalls = hasCalls ? calls : null };
    }

    private static JsonElement ToElement(IDictionary<string, object?>? arguments) =>
        arguments is { Count: > 0 } ? JsonSerializer.SerializeToElement(arguments) : EmptyObject;

    private static Dictionary<string, object?> ToArguments(JsonElement arguments) =>
        arguments.ValueKind == JsonValueKind.Object
            ? JsonSerializer.Deserialize<Dictionary<string, object?>>(arguments) ?? new Dictionary<string, object?>()
            : new Dictionary<string, object?>();

    /// <summary>
    /// OllamaSharp wraps a tool result as JSON. Hand it real JSON when we have it, otherwise the
    /// model would receive an escaped string inside a string.
    /// </summary>
    private static object ToResult(string content)
    {
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                return doc.RootElement.Clone();
        }
        catch (JsonException) { /* plain text, e.g. an error message */ }

        return content;
    }
}
