// ============================================================================
// Copyright (c) 2024 - W2Wizard.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

// ============================================================================

namespace App.Backend.API.Schemas.Operation;

internal sealed class BasicResponsesOperationTransformer() : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Responses ??= [];
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
        operation.Responses.TryAdd("429", new OpenApiResponse { Description = "Too Many Requests" });
        operation.Responses.TryAdd("404", new OpenApiResponse { Description = "Not Found" });
        return Task.CompletedTask;
    }
}
