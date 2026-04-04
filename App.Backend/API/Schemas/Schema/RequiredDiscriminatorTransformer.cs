// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.OpenApi;
using Microsoft.AspNetCore.OpenApi;

// ============================================================================

namespace App.Backend.API.Schemas.Schema;

/// <summary>
/// This transformer breaks forces the discriminator property ($type) to be required
/// on all schemas that have it, which is necessary for
/// correct type generation on the frontend.
/// </summary>
internal sealed class RequiredDiscriminatorTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken token)
    {
        // Make $type required on all discriminator types
        if (schema.Properties?.ContainsKey("$type") == true)
        {
            schema.Required ??= new HashSet<string>();
            schema.Required.Add("$type");
        }

        return Task.CompletedTask;
    }
}