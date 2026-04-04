// ============================================================================
// Copyright (c) 2024 - W2Wizard.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.OpenApi;
using Microsoft.AspNetCore.OpenApi;

// ============================================================================

namespace App.Backend.API.Schemas.Document;

internal sealed class InfoDocumentTransformer() : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext ctx, CancellationToken token)
    {
        document.Info = new()
        {
            Title = "KK API",
            Version = "v1",
            Description = "Alpha version of the KK API. Subject to change without deprecation.",
            Contact = new OpenApiContact
            {
                Email = "not-available@kk92.net",
                Name = "W2Wizard"
            }
        };
    }
}
