# Backend Database

This project (`Backend.API.Database`) handles data persistence and infrastructure.

## Responsibilities

- **DbContext**: The Entity Framework Core `DatabaseContext` definition.
- **Configuration**: Entity configurations and relationships.
- **Interceptors**: Database interceptors (e.g., for auditing or soft deletes).
- **Extensions**: Reusable helpers on the context, e.g. `WithAdvisoryLockAsync` to make requests that "check, then change" something (a capacity, a uniqueness rule) take turns instead of racing.

This layer is responsible for talking directly to the PostgreSQL database.
