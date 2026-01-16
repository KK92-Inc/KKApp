# App.Backend

The .NET Web API backend for KKApp, built following Clean Architecture principles. This project provides RESTful APIs for managing users, projects, curricula (cursi), goals, and peer reviews.

## 🏗️ Architecture

The backend is organized into multiple projects following the Clean Architecture pattern:

```
App.Backend/
├── API/           # Presentation layer - Controllers, filters, OpenAPI
├── Core/          # Application layer - Business logic, services, rules
├── Database/      # Infrastructure layer - EF Core DbContext, interceptors
├── Domain/        # Domain layer - Entities, enums, value objects
├── Models/        # DTOs - Request/Response models
└── Tests/         # Unit and integration tests
```

### Layer Dependencies

```
┌─────────────┐
│     API     │  ← Controllers, Filters, Jobs, OpenAPI Schemas
├─────────────┤
│    Core     │  ← Services, Business Rules, Queries
├─────────────┤
│   Models    │  ← Request/Response DTOs
├─────────────┤
│  Database   │  ← DbContext, Interceptors, Migrations
├─────────────┤
│   Domain    │  ← Entities, Enums, Relations (no dependencies)
└─────────────┘
```

## 📁 Project Breakdown

### API (`App.Backend.API`)

The presentation layer containing:

- **Controllers/** - REST API endpoints
  - `UserController` - User management and profiles
  - `ProjectController` - Project CRUD operations
  - `CursusController` - Curriculum management
  - `GoalController` - Learning goal management
  - `SubscriptionController` - User subscriptions

- **Filters/** - Action filters including `KeycloakUser` for auth context
- **Jobs/** - Background jobs using Quartz.NET
- **Params/** - Query parameters (pagination, filtering)
- **Schemas/** - OpenAPI document and operation transformers

### Core (`App.Backend.Core`)

The application/business logic layer:

- **Services/** - Business logic implementation
  - `Interface/` - Service contracts
  - `Implementation/` - Concrete implementations
- **Rules/** - Domain validation rules
- **Query/** - Query specifications and filters

### Database (`App.Backend.Database`)

The data access layer:

- `DatabaseContext.cs` - EF Core DbContext with all DbSets
- **Interceptors/** - EF Core interceptors (soft delete, auditing)
- `ISoftDeletable.cs` - Interface for soft-deletable entities

### Domain (`App.Backend.Domain`)

The core domain layer (no external dependencies):

- **Entities/** - Domain entities
  - `Users/` - User, SshKey, Details
  - `Projects/` - Project, Git
  - `Reviews/` - Review, Rubric
  - `Cursus`, `Goal`, `Comment`, `Notification`, `Spotlight`
- **Enums/** - Domain enumerations
- **Relations/** - Join entities (UserProject, UserCursus, UserGoal, etc.)
- **Rules/** - Domain-level validation rules

### Models (`App.Backend.Models`)

Data Transfer Objects:

- **Requests/** - Input DTOs for API endpoints
- **Responses/** - Output DTOs for API responses
- `RequestDTO.cs` / `ResponseDTO.cs` - Base DTO classes

### Tests (`App.Backend.Tests`)

- **Fixtures/** - Test fixtures and data builders
- **Services/** - Service unit tests

## 🔐 Authentication & Authorization

The API uses **Keycloak** for authentication with JWT bearer tokens:

```csharp
// Keycloak configuration
services.AddKeycloakWebApiAuthentication(configuration, options =>
{
    options.Audience = "intra";
    options.RequireHttpsMetadata = false;
});

// Authorization policies
services.AddAuthorizationBuilder()
    .AddPolicy("IsStaff", b => b.RequireClaim(ClaimTypes.Role, "staff"))
    .AddPolicy("IsDeveloper", b => b.RequireClaim(ClaimTypes.Role, "developer"));
```

## 🚀 Running the API

### With Aspire (Recommended)

From the repository root:

```bash
dotnet run apphost.cs
```

### Standalone

```bash
cd App.Backend/API
dotnet run
```

The API will be available at `http://localhost:3001`.

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__peeru-db` | PostgreSQL connection string | - |
| `ConnectionStrings__cache` | Valkey/Redis connection string | - |
| `Keycloak__*` | Keycloak configuration | See appsettings.json |

## 📖 API Documentation

The API exposes an OpenAPI specification at `/openapi/v1.json`.

When running with Aspire, a Scalar API reference is available for interactive exploration.

### Example Endpoints

```http
# Users
GET    /api/users              # List users (paginated)
GET    /api/users/{id}         # Get user by ID
PUT    /api/users/{id}         # Update user

# Projects
GET    /api/projects           # List projects
POST   /api/projects           # Create project
GET    /api/projects/{id}      # Get project details

# Curricula
GET    /api/cursi              # List curricula
GET    /api/cursi/{id}/goals   # Get goals for curriculum

# Goals
GET    /api/goals              # List goals
GET    /api/goals/{id}         # Get goal details
```

## 🧪 Testing

```bash
cd App.Backend/Tests
dotnet test
```

## 📦 Key Dependencies

| Package | Purpose |
|---------|---------|
| `Keycloak.AuthServices.*` | Keycloak integration |
| `Wolverine` | Message bus / CQRS |
| `Quartz` | Background job scheduling |
| `Serilog` | Structured logging |
| `Renci.SshNet` | SSH key management |

## 📄 License

Copyright © 2025 W2Inc. All Rights Reserved.
