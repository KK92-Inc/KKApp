// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Migrations;
using Microsoft.EntityFrameworkCore;
using App.Backend.Database;
using Microsoft.Extensions.Hosting;

// ============================================================================

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Initializer>();

// builder.AddServiceDefaults();
builder.Services.AddDbContextPool<DatabaseContext>(options =>
{
    var connection = builder.Configuration.GetConnectionString("peeru-db");
    options.UseNpgsql(connection, o => o.MigrationsAssembly("Migrations"));
    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

builder.Build().Run();
