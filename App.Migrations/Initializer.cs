// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using App.Backend.Database;

// ============================================================================

namespace Migrations;

public class Initializer(
    IServiceProvider sp,
    IHostEnvironment host,
    IHostApplicationLifetime lifetime
) : BackgroundService
{
    private readonly ActivitySource source = new(host.ApplicationName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = source.StartActivity(host.ApplicationName, ActivityKind.Client);

        try
        {
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            await RunMigrationAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        lifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(DatabaseContext context, CancellationToken cancellationToken)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await context.Database.MigrateAsync(cancellationToken);
        });
    }
}
