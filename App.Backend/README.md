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

## Routes + Permissions

1. Implement the DTOs
2. Implement the Services (Interface + Implementation)
3. Implement the controller / endpoint functions (with the proper attributes applied)

Users:
- GET /users/current : [None] : Gets the current user
- GET /users/current/notifications : [None] : Gets the current user's notifications
- GET /users/current/spotlights : [None] : Gets the current user's spotlight events
- DELETE /users/current/spotlights/{id:guid} : [None] : Dismiss the given spotlight
- GET /users : [users:read] : Query all users
- POST /users : [users:write] : Create a new user
- DELETE /users : [users:delete] : Delete a user
- GET /users/{id:guid} : [users:read] : Query a user
- PATCH /users/{id:guid} : [None] : Update a users / user's details
  - This route is a bit unique, user domain is split in two (user + details)
  - I want to have a single route that lets me update (display, avatar_url) and the fields within the Details Domain via a single DTO
  - If User.GetSID() !== id from route, return forbid *unless* user is a staff