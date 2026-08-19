// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.OpenApi;
using Microsoft.AspNetCore.OpenApi;
using App.Backend.Models;

// ============================================================================

namespace App.Backend.API.Schemas.Schema;

/// <summary>
/// <para>
/// System.Text.Json's schema exporter has no idea what to do with a custom
/// struct like <see cref="Optional{T}"/> hidden behind a custom converter, so
/// it emits an essentially empty "anything goes" schema for it. This
/// transformer finds every property whose CLR type is <c>Optional&lt;T&gt;</c>
/// and replaces the generated schema with the schema for <c>T</c> itself —
/// via <see cref="OpenApiSchemaTransformerContext.GetOrCreateSchemaAsync"/>,
/// new in .NET 10 — so the property ends up looking exactly like a plain,
/// non-nullable <c>T</c> that just happens to be optional.
/// </para>
/// <para>
/// Deliberately scoped to primitives/scalars (string, bool, numbers, enums,
/// DateTime, Guid, arrays of those, ...) — that's the 99% case. Nested DTOs
/// (<c>Optional&lt;SomeComplexDto&gt;</c>) come back from
/// <c>GetOrCreateSchemaAsync</c> as a referenced/component schema rather than
/// an inline shape, and copying that correctly needs to key off exactly how
/// your installed Microsoft.OpenApi version represents a schema reference —
/// which changed across the 2.x rewrite and isn't worth guessing at here. If
/// you never put a complex type in <c>Optional&lt;T&gt;</c>, you'll never hit
/// this; if you accidentally do, this throws instead of silently emitting a
/// wrong/empty schema.
/// </para>
/// <para>
/// We deliberately don't touch the parent schema's "required" list here:
/// <c>Optional&lt;T&gt;</c> properties are never declared with the C#
/// <c>required</c> keyword and are never constructor-bound, so the exporter
/// never adds them to "required" in the first place — there's nothing to
/// strip.
/// </para>
/// </summary>
internal sealed class OptionalSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var propertyType = context.JsonPropertyInfo?.PropertyType;
        if (propertyType is null || !propertyType.IsGenericType || propertyType.GetGenericTypeDefinition() != typeof(Optional<>))
            return;

        var innerType = propertyType.GetGenericArguments()[0];
        var innerSchema = await context.GetOrCreateSchemaAsync(innerType, context.ParameterDescription, cancellationToken);
        if (innerSchema.Properties is { Count: > 0 } || innerSchema.Type is JsonSchemaType.Object)
        {
            throw new NotSupportedException(
                $"Optional<{innerType.Name}> on property '{context.JsonPropertyInfo!.Name}' looks like a " +
                "complex/nested type. OptionalSchemaTransformer only supports Optional<T> for primitive/scalar " +
                "T (string, bool, numbers, enums, DateTime, Guid, and arrays of those). Extend this transformer " +
                "if you actually need nested-DTO support.");
        }

        // Scalar / enum / array-of-scalar shape: copy over "what kind of
        // value is this", but leave anything already set on `schema` by
        // earlier transformers alone (Description from [Description],
        // MinLength/MaxLength from [StringLength], Pattern from
        // [RegularExpression], etc. are all applied to *this* property
        // schema before user transformers run, and none of that should be
        // clobbered).
        schema.Type = innerSchema.Type;
        schema.Format = innerSchema.Format;
        schema.Enum = innerSchema.Enum;
        schema.Items = innerSchema.Items;
    }
}