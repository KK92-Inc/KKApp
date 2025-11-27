// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

// ============================================================================

namespace Backend.API.Infrastructure;

/// <summary>
/// Factory for creating <see cref="DatabaseContext"/> at design time.
/// This is used by EF Core tools (e.g., migrations) when they cannot
/// resolve the DbContext from the application's service provider.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

        // Use a placeholder connection string for design-time operations.
        // This connection string is only used for generating migrations,
        // not for actual database operations at runtime.
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
            ?? "Host=localhost;Port=52842;Username=postgres;Password=postgres;Database=data;";

        optionsBuilder.UseNpgsql(connectionString);
        return new DatabaseContext(optionsBuilder.Options);
    }
}
