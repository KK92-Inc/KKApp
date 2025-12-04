// ============================================================================
// Copyright (c) 2024 - W2Wizard.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;

// ============================================================================

namespace App.Backend.API.Schemas.Document;

internal sealed class InfoSchemeTransformer() : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext ctx, CancellationToken token)
    {
        document.Info = new()
        {
            Title = "NXT API",
            Version = "v1",
            Description = "The NXT API provides access to NXT's data. THis way you can programmatically subscribe a project and more!",
            Contact = new OpenApiContact
            {
                Email = "info@nextdemy.com",
                Name = "W2Wizard"
            }
        };
    }
}
