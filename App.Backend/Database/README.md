# Backend Database

This project (`Backend.API.Database`) handles data persistence and infrastructure.

## Responsibilities

- **DbContext**: The Entity Framework Core `DatabaseContext` definition.
- **Configuration**: Entity configurations and relationships.
- **Interceptors**: Database interceptors (e.g., for auditing or soft deletes).

This layer is responsible for talking directly to the PostgreSQL database.
