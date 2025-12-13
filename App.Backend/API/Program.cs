// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Serilog;
using App.Backend.API;

// ============================================================================

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
Log.Information("Starting up!");

var app = Services.Register(WebApplication.CreateBuilder(args)).Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStatusCodePages();
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();
app.UseResponseCompression();

// app.MapControllers().RequireAuthorization();
app.MapControllers();

// app.UseRouting();
// app.UseAuthentication();
// app.UseAuthorization();

// app.UseHttpsRedirection();
// app.MapControllers()
//     .RequireAuthorization()
//     .RequireRateLimiting("AuthenticatedRateLimit");

app.Run();
