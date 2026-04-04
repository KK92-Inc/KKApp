// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.OpenApi;
using Microsoft.AspNetCore.OpenApi;
using App.Backend.Domain.Rules.Evaluations.Composites;

// ============================================================================

namespace App.Backend.API.Schemas.Schema;

/// <summary>
/// <para>
/// Fix for: https://github.com/openapi-ts/openapi-typescript/issues/1565
/// </para>
/// This transformer breaks the circular references in the Rule schema by
/// replacing them with a generic "object" schema.
/// </summary>
internal sealed class BreakRuleCircularRefTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken token)
    {
        var type = context.JsonTypeInfo.Type;
        var any = new OpenApiSchema { Type = JsonSchemaType.Object };
        var isRule = type == typeof(AllOfRule) || type == typeof(AnyOfRule);

        // AllOfRule / AnyOfRule: rules[] items → Rule (circular)
        if (isRule && schema.Properties?.TryGetValue("rules", out var rule) == true && rule is OpenApiSchema concrete)
            concrete.Items = any;

        // NotRule: rule → Rule (circular)
        if (type == typeof(NotRule) && schema.Properties?.ContainsKey("rule") == true)
            schema.Properties["rule"] = any;
        return Task.CompletedTask;
    }
}