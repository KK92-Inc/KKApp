// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.AI;
using ModelContextProtocol;
using ModelContextProtocol.Server;

// ============================================================================

namespace App.Backend.API.MCP.Agent;

/// <summary>
/// A callable tool plus whether it is safe to run without asking a human.
/// </summary>
public sealed record AgentTool(AIFunction Function, bool ReadOnly)
{
    public string Name => Function.Name;
    public string Description => Function.Description;
}

/// <summary>
/// Exposes the very same [McpServerTool] methods that /mcp serves to the built-in agent, in-process.
/// Add a tool to a class under MCP/Tools and it shows up in both places.
/// </summary>
public static class AgentToolbox
{
    private const int MaxResultChars = 12_000;

    private sealed record Descriptor(Type Type, MethodInfo Method, McpServerToolAttribute Attribute);

    private static readonly Lazy<IReadOnlyList<Descriptor>> Catalog = new(Discover);

    private static IReadOnlyList<Descriptor> Discover() => typeof(AgentToolbox).Assembly
        .GetTypes()
        .Where(t => t is { IsAbstract: false, IsClass: true } && t.GetCustomAttribute<McpServerToolTypeAttribute>() is not null)
        .SelectMany(t => t
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(m => (Method: m, Attribute: m.GetCustomAttribute<McpServerToolAttribute>()))
            .Where(x => x.Attribute is not null)
            .Select(x => new Descriptor(t, x.Method, x.Attribute!)))
        .ToList();

    /// <summary>
    /// Builds the tools for the current request. Tool classes are created from the request's
    /// service provider so they see the same scoped services and HttpContext (the caller's token).
    /// </summary>
    public static IReadOnlyList<AgentTool> Build(IServiceProvider services)
    {
        var instances = new Dictionary<Type, object>();
        var tools = new List<AgentTool>();

        foreach (var d in Catalog.Value)
        {
            object? target = null;
            if (!d.Method.IsStatic && !instances.TryGetValue(d.Type, out target))
            {
                target = ActivatorUtilities.CreateInstance(services, d.Type);
                instances[d.Type] = target;
            }

            var function = AIFunctionFactory.Create(d.Method, target, new AIFunctionFactoryOptions
            {
                Name = d.Attribute.Name ?? d.Method.Name,
            });

            tools.Add(new AgentTool(function, d.Attribute.ReadOnly));
        }

        return tools;
    }

    // Invocation
    // ============================================================================

    /// <summary>
    /// Runs a tool with the model's arguments. Never throws for tool failures: the error text is
    /// returned so the model can explain it (or correct its arguments).
    /// </summary>
    public static async Task<string> InvokeAsync(AgentTool tool, JsonElement arguments, ILogger log, CancellationToken ct)
    {
        var args = new AIFunctionArguments();
        if (arguments.ValueKind == JsonValueKind.Object)
            foreach (var property in arguments.EnumerateObject())
                args[property.Name] = property.Value.Clone();

        try
        {
            var result = await tool.Function.InvokeAsync(args, ct);
            return Truncate(Stringify(result));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (McpException e)
        {
            return $"Error: {e.Message}";
        }
        catch (Exception e) when (e is ArgumentException or JsonException or FormatException or InvalidCastException)
        {
            return $"Error: invalid arguments for {tool.Name}. {e.Message}";
        }
        catch (Exception e)
        {
            log.LogError(e, "Agent tool {Tool} failed", tool.Name);
            return "Error: the tool failed unexpectedly.";
        }
    }

    private static string Stringify(object? result) => result switch
    {
        null => "null",
        string s => s,
        JsonElement element => element.GetRawText(),
        _ => JsonSerializer.Serialize(result),
    };

    private static string Truncate(string value) =>
        value.Length <= MaxResultChars ? value : value[..MaxResultChars] + "\n...[truncated]";
}
