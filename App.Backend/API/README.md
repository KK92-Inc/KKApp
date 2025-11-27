# Backend API

This project (`Backend.API`) is the entry point for the PeerU backend services.

## Responsibilities

- **Hosting**: Configures the HTTP request pipeline and dependency injection container.
- **Controllers**: Handles incoming HTTP requests and maps them to service calls.
- **Configuration**: Manages `appsettings.json` and environment variables.
- **Observability**: Configures OpenTelemetry, logging (Serilog), and metrics.

## Key Files

- `Program.cs`: The application bootstrap.
- `Controllers/`: API endpoints.
