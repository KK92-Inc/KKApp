# App.Migrations

A dedicated .NET worker service responsible for applying Entity Framework Core database migrations. This service runs as part of the Aspire orchestration and ensures the database schema is up-to-date before the main API starts.

## 🎯 Purpose

In distributed systems, database migrations should be handled by a dedicated service rather than being applied during API startup. This approach:

- **Prevents race conditions** when multiple API instances start simultaneously
- **Separates concerns** between schema management and application logic
- **Enables controlled deployments** with explicit migration steps
- **Supports rollback strategies** independent of application deployment

## 🏗️ Architecture

```
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│   Aspire Host   │─────►│   Migrations    │─────►│   PostgreSQL    │
│                 │      │    Service      │      │                 │
└─────────────────┘      └─────────────────┘      └─────────────────┘
         │                       │
         │                       ▼
         │               ┌─────────────────┐
         └──────────────►│   Backend API   │ (waits for migrations)
                         └─────────────────┘
```

## 📁 Project Structure

```
App.Migrations/
├── Program.cs                              # Host configuration
├── Initializer.cs                          # Migration hosted service
├── Migrations.csproj                       # Project file
└── Migrations/
    ├── 20260104235452_Init.cs              # Initial migration
    ├── 20260104235452_Init.Designer.cs     # Migration metadata
    └── DatabaseContextModelSnapshot.cs     # Current model snapshot
```

## 🔧 How It Works

### Program.cs

Configures the host with the database context:

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Initializer>();
builder.Services.AddDbContextPool<DatabaseContext>(options =>
{
    var connection = builder.Configuration.GetConnectionString("peeru-db");
    options.UseNpgsql(connection, o => o.MigrationsAssembly("Migrations"));
});
builder.Build().Run();
```

### Initializer.cs

A `BackgroundService` that applies pending migrations on startup:

```csharp
public class Initializer : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await context.Database.MigrateAsync(stoppingToken);
    }
}
```

## 🚀 Running Migrations

### With Aspire (Automatic)

When running the Aspire host, migrations are applied automatically:

```bash
dotnet run apphost.cs
```

The Backend API waits for the migration service to complete before starting.

### Standalone

```bash
cd App.Migrations
dotnet run
```

### Environment Variables

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__peeru-db` | PostgreSQL connection string |

## 📦 Creating New Migrations

From the repository root:

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
    --project App.Migrations \
    --startup-project App.Migrations \
    --context DatabaseContext

# Example
dotnet ef migrations add AddUserPreferences \
    --project App.Migrations \
    --startup-project App.Migrations
```

### Migration Best Practices

1. **Use descriptive names** - `AddUserPreferences`, `CreateNotificationsTable`
2. **Keep migrations small** - One logical change per migration
3. **Test migrations** - Both `Up()` and `Down()` paths
4. **Never edit published migrations** - Create new ones instead

## 🔄 Rollback

To rollback to a specific migration:

```bash
dotnet ef database update <MigrationName> \
    --project App.Migrations \
    --startup-project App.Migrations
```

To rollback all migrations:

```bash
dotnet ef database update 0 \
    --project App.Migrations \
    --startup-project App.Migrations
```

## 🐳 Docker

The migration service is containerized and published as `ghcr.io/<owner>/kk-migration`.

It uses .NET's native container publishing:

```bash
dotnet publish App.Migrations/Migrations.csproj \
    --os linux --arch x64 \
    -c Release \
    -p:PublishProfile=DefaultContainer
```

## 📄 License

Copyright © 2025 W2Inc. All Rights Reserved.
