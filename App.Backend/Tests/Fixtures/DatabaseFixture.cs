// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Tests.Fixtures;

/// <summary>
/// Base class for service tests that need a database context.
/// Provides automatic setup and teardown of an in-memory database.
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    protected readonly DatabaseContext Context;

    protected ServiceTestBase()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new DatabaseContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
