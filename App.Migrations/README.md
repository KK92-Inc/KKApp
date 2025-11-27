# App.Migrations

This project is a dedicated **Worker Service** responsible for managing database schemas.

## Purpose

In a distributed Aspire environment, we separate the act of *migrating* the database from the *running* of the API. This project:

1.  Starts up before the Backend API.
2.  Connects to the Postgres database.
3.  Applies any pending Entity Framework Core migrations.
4.  Exits successfully (or fails if migrations fail).

## Why separate?

- **Safety**: Ensures migrations happen in a controlled manner before the application starts serving traffic.
- **Scalability**: Prevents race conditions if multiple API replicas were to try migrating the DB simultaneously.
- **Aspire Integration**: It is defined as a resource in `apphost.cs` that the Backend API depends on (`.WaitFor(migrationService)`).
