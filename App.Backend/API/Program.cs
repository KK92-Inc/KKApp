// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Serilog;
using App.Backend.API;

// ============================================================================

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
Log.Information("Starting up!");


var app = Services.Register(WebApplication.CreateBuilder(args)).Build();
app.MapOpenApi();
app.UseStatusCodePages();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseSerilogRequestLogging();
app.UseResponseCompression();
app.MapDefaultEndpoints();

app.MapControllers().RequireAuthorization();


app.Run();