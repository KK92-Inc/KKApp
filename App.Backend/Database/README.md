# Backend.API.Infrastructure

## Purpose

This is the **Infrastructure** layer of the Clean Architecture. It contains implementations of interfaces defined in the Core layer and handles all external concerns like databases, file systems, web services, etc.

## Responsibilities

- **Repository Implementations**: Concrete implementations of repository interfaces from Core
- **Database Context**: Entity Framework Core DbContext and configurations
- **Migrations**: Database schema migrations
- **External Services**: Third-party API integrations, email services, SMS, etc.
- **Identity & Authentication**: User management, authentication providers
- **Caching**: Redis, in-memory cache implementations
- **File Storage**: Local file system, Azure Blob Storage, AWS S3, etc.
- **Message Queues**: RabbitMQ, Azure Service Bus implementations

## Dependencies

- **References Core**: Implements interfaces defined in the Core layer
- **External Libraries**: Entity Framework Core, Dapper, Azure SDK, etc.

## Key Principles

- **Implements Abstractions**: All implementations should satisfy interfaces from Core
- **Framework Dependent**: Can use framework-specific features
- **Replaceable**: Should be easy to swap implementations (e.g., SQL Server → PostgreSQL)

## What NOT to Include

- Business logic (belongs in Core)
- API controllers (belongs in API/Presentation)
- Application-specific orchestration (belongs in Application layer if you have one)

## Example Structure

```
Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   └── Migrations/
├── Repositories/
├── Services/
├── Identity/
└── External/
```
