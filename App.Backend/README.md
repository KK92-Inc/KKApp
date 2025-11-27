# App.Backend

This folder contains the core backend logic for PeerU, structured as a modular .NET solution.

## Architecture

The backend follows a Clean Architecture / Onion Architecture pattern to separate concerns:

- **`API/`**: The entry point of the application. Contains Controllers, Program.cs, and API configuration.
- **`Core/`**: Contains the business logic, service interfaces, and implementations. It depends on Domain and Database.
- **`Database/`**: Contains the Entity Framework Core `DbContext` and database infrastructure configuration.
- **`Domain/`**: The core of the application. Contains Entities, Enums, and business rules. It has no dependencies on other layers.
- **`Models/`**: Contains Data Transfer Objects (DTOs) used for API requests and responses.

## Dependencies

- **API** -> Core, Models
- **Core** -> Domain, Database, Models
- **Database** -> Domain
- **Models** -> Domain
