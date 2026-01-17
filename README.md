# KKApp

A distributed peer-to-peer learning platform built with .NET Aspire, featuring a modern SvelteKit frontend, .NET Web API backend, and custom Git hosting infrastructure.

> [!CAUTION]
> This project is unlicensed, unfinished and still work in progress. It is quite literally unusable at it's current stage.

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                              .NET Aspire AppHost                             │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────┐       ┌──────────────┐       ┌──────────────────────────┐  │
│  │   Frontend   │──────►│   Backend    │──────►│  Git API   │   Git SSH   │  │
│  │  (SvelteKit) │       │  (.NET API)  │       │   (Bun)    │    (Bun)    │  │
│  └──────┬───────┘       └──────┬───────┘       └─────┬──────┴──────┬──────┘  │
│         │                      │                     │             │         │
│         │                      │                     │             │         │
│         │                      ▼                     ▼             │         │
│         │               ┌──────────────┐      ┌──────────────┐     │         │
│         │               │    Valkey    │      │  PostgreSQL  │◄────┘         │
│         │               │   (Cache)    │      └──────────────┘               │
│         │               └──────────────┘                                     │
│         │                      ▲                                             │
│         │                      │                                             │
│         │               ┌──────┴───────┐                                     │
│         └──────────────►│   Keycloak   │                                     │
│                         │    (Auth)    │                                     │
│                         └──────────────┘                                     │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

## 📁 Project Structure

| Directory | Description |
|-----------|-------------|
| [`App.Backend/`](App.Backend/README.md) | .NET Web API following Clean Architecture |
| [`App.Frontend/`](App.Frontend/README.md) | SvelteKit frontend application |
| [`App.Migrations/`](App.Migrations/README.md) | EF Core database migration service |
| [`App.Repository/`](App.Repository/README.md) | Custom Git hosting (API + SSH servers) |
| `aspire-output/` | Generated Docker Compose files for deployment |
| `config/` | Keycloak realm configuration files |

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (Preview)
- [Bun](https://bun.sh/) (v1.3+)
- [Docker](https://www.docker.com/) & Docker Compose
- [.NET Aspire workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/KKApp.git
   cd KKApp
   ```

2. **Install the Aspire workload** (if not already installed)
   ```bash
   dotnet workload install aspire
   ```

3. **Set up user secrets** for local development
   ```bash
   dotnet user-secrets set "Parameters:postgres-usr" "postgres"
   dotnet user-secrets set "Parameters:postgres-pwd" "your-secure-password"
   dotnet user-secrets set "Parameters:kc-client-secret" "your-keycloak-secret"
   ```

4. **Install frontend dependencies**
   ```bash
   cd App.Frontend && bun install && cd ..
   ```

### Running the Application

Start the entire distributed application using Aspire:

```bash
dotnet run apphost.cs
```

This orchestrates:
- **PostgreSQL** database with persistent volume
- **Valkey** (Redis-compatible) cache
- **Keycloak** identity provider with pre-configured realms
- **Database migrations** (runs automatically on startup)
- **Backend API** (.NET)
- **Frontend** (SvelteKit dev server)
- **Git servers** (API + SSH)

The Aspire dashboard will be available at `https://localhost:17225` where you can monitor all services.

### Individual Service URLs (Development)

| Service | URL |
|---------|-----|
| Frontend | http://localhost:51842 |
| Backend API | http://localhost:3001 |
| Keycloak Admin | http://localhost:8080 (admin/admin) |
| PostgreSQL | localhost:52843 |
| Git SSH | localhost:2222 |

## 🐳 Deployment

### Using Docker Compose

Pre-generated Docker Compose files are available in `aspire-output/`:

```bash
cd aspire-output

# Development deployment
docker compose up -d

# Production deployment
docker compose -f docker-compose.prod.yaml up -d
```

### Container Images

Container images are automatically built and published to GitHub Container Registry on pushes to `main`:

- `ghcr.io/<owner>/kk-backend` - Backend API
- `ghcr.io/<owner>/kk-frontend` - Frontend application
- `ghcr.io/<owner>/kk-migration` - Database migrations
- `ghcr.io/<owner>/kk-git-api` - Git REST API
- `ghcr.io/<owner>/kk-git-ssh` - Git SSH server

## 🔧 Development

### Backend Development

```bash
# Run only the backend API
dotnet run --project App.Backend/API/App.Backend.API.csproj
```

### Frontend Development

```bash
cd App.Frontend
bun install
bun run dev
```

### Database Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> --project App.Migrations

# Apply migrations manually
dotnet run --project App.Migrations
```

### Running Tests

```bash
# Backend tests
dotnet test App.Backend/Tests/App.Backend.Tests.csproj
```

## 📚 Tech Stack

| Layer | Technology |
|-------|------------|
| Orchestration | .NET Aspire |
| Backend | .NET 10, ASP.NET Core, Entity Framework Core |
| Frontend | SvelteKit 2, Svelte 5, TailwindCSS 4, TypeScript |
| Database | PostgreSQL 17 |
| Cache | Valkey (Redis-compatible) |
| Auth | Keycloak |
| Git Hosting | Custom Bun-based servers |
| Messaging | Wolverine |
| Scheduling | Quartz.NET |

## 📄 License

Copyright © 2025 W2Inc. All Rights Reserved.

See [LICENSE](LICENSE) for details.
