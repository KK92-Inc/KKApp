// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Tests.Fixtures;

/// <summary>
/// Fixture for creating an in-memory database context for testing.
/// </summary>
public class DatabaseFixture : IDisposable
{
    public DatabaseContext Context { get; private set; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new DatabaseContext(options);
        Context.Database.EnsureCreated();
    }

    /// <summary>
    /// Creates a fresh database context with a new in-memory database.
    /// Useful for tests that need isolation.
    /// </summary>
    public static DatabaseContext CreateFreshContext()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new DatabaseContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        Context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
