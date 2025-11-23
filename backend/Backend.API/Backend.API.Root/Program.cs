// ============================================================================
// Copyright (c) 2025 - W2Inc.
// See README.md in the project root for license information.
// ============================================================================

using Serilog;
using Wolverine;
using Backend.API.Root;

// ============================================================================
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
Log.Information("Starting up!");

var app = Services.Register(WebApplication.CreateBuilder(args)).Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/issues", async (CreateIssue command, IMessageBus bus) =>
{
    await bus.InvokeAsync(command);
    return Results.Ok("Issue creation initiated");
});

app.MapControllers();
app.UseHttpsRedirection();
app.Run();
