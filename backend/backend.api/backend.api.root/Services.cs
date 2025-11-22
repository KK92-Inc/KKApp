// ============================================================================
// Copyright (c) 2025 - W2Inc.
// See README.md in the project root for license information.
// ============================================================================

using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace backend.api.root;

// ============================================================================

public static class Services
{
    public static WebApplicationBuilder Register(WebApplicationBuilder builder)
    {
        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddControllers();
        builder.Services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(new ExpressionTemplate(
                "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
                theme: TemplateTheme.Code
            )));
        return builder;
    }
}
