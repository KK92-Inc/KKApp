# PeerU

PeerU is a distributed application built with .NET Aspire, C#, and SvelteKit.

## Project Structure

This repository is organized into several key components:

- **`src/apphost.cs`**: The .NET Aspire AppHost entry point. This script defines the distributed application architecture, orchestrating containers (Postgres, etc.) and projects (Backend, Migrations) for local development.
- **`App.Backend/`**: The .NET Web API solution, following a Clean Architecture approach.
- **`App.Frontend/`**: The SvelteKit frontend application.
- **`App.Migrations/`**: A dedicated worker service responsible for applying Entity Framework Core database migrations on startup.
- **`App.Git/`**: Configuration and Docker setups for Git infrastructure (Gitaly, GitLab Shell) used within the platform.
- **`scripts/`**: Utility scripts for database management and other tasks.

## Getting Started

To run the entire solution using Aspire:

```bash
dotnet run --project src/apphost.cs
```

This will spin up the Postgres database, run migrations, start the Backend API, and launch the Frontend.
