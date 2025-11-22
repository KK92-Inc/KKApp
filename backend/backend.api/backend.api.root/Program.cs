// ============================================================================
// Copyright (c) 2025 - W2Inc.
// See README.md in the project root for license information.
// ============================================================================

using Serilog;
using backend.api.root;

// ============================================================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up!");
var app = Services.Register(WebApplication.CreateBuilder(args)).Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.UseHttpsRedirection();
app.Run();
