// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Serilog;
using Wolverine;
using Backend.API.Root;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Backend.API.Infrastructure;
// using Grpc.Net.Client;

// ============================================================================
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
Log.Information("Starting up!");

var builder = WebApplication.CreateBuilder(args);

// builder.AddServiceDefaults(); // TODO: Set it up...
var app = Services.Register(builder).Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers().RequireRateLimiting("AuthenticatedRateLimit"); ;
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
