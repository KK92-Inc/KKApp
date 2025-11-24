# Backend.API

## Purpose

This is the **API/Presentation** layer (also known as the Root or Web layer) of the Clean Architecture. It's the entry point of the application that exposes HTTP endpoints and handles web concerns.

## Responsibilities

- **API Controllers**: REST API endpoints and route definitions
- **Request/Response Models**: DTOs for API input/output
- **Middleware**: Request pipeline components (error handling, logging, CORS, etc.)
- **Filters**: Action filters, authorization filters, exception filters
- **Dependency Injection**: Service registration and DI container configuration
- **Configuration**: appsettings.json, environment-specific settings
- **Program.cs/Startup.cs**: Application entry point and configuration
- **API Documentation**: Swagger/OpenAPI configuration
- **Authentication/Authorization**: JWT configuration, policies, authentication schemes

## Dependencies

- **References Core**: For domain entities and interfaces
- **References Infrastructure**: For service implementations and data access
- **References Application** (if exists): For use cases/commands/queries

## Key Principles

- **Thin Controllers**: Controllers should only handle HTTP concerns and delegate to services
- **No Business Logic**: Business rules belong in Core
- **Dependency Direction**: Depends on inner layers, never the reverse
- **Framework Specific**: Can use ASP.NET Core features freely

## What NOT to Include

- Business logic (belongs in Core)
- Data access implementation (belongs in Infrastructure)
- Complex orchestration (consider Application layer for use cases)

## Example Structure

```
API/
├── Controllers/
├── Models/
│   ├── Requests/
│   └── Responses/
├── Middleware/
├── Filters/
├── Extensions/
├── Program.cs
└── appsettings.json
```

## Dependency Flow

```
API (Presentation)
    ↓
Infrastructure
    ↓
Core (Domain)
```

All dependencies point inward toward the Core layer.
