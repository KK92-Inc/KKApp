// ============================================================================
// Copyright (c) 2024 - W2Wizard.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.OpenApi;
using Microsoft.AspNetCore.OpenApi;

// ============================================================================

namespace App.Backend.API.Schemas.Operation;

/// <summary>
/// This transformer adds basic common responses (401, 403, 404, 429) to all operations,
/// which are not automatically added by ASP.NET Core and are
/// necessary for correct type generation on the frontend.
/// </summary>
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
