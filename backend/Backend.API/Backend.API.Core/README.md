# Backend.API.Core

## Purpose

This is the **Core** layer (also known as Domain layer) of the Clean Architecture. It contains the enterprise business logic and domain entities that are independent of any external concerns.

## Responsibilities

- **Domain Entities**: Core business objects with their properties and business logic
- **Value Objects**: Immutable objects that represent concepts without identity
- **Domain Events**: Events that represent something that happened in the domain
- **Interfaces**: Abstractions for repositories and services (implemented in outer layers)
- **Enums**: Domain-specific enumerations
- **Exceptions**: Domain-specific exceptions
- **Specifications**: Business rules and validation logic

## Key Principles

- **No Dependencies**: This layer should have NO dependencies on other projects or external frameworks
- **Framework Independent**: Business logic should not depend on ASP.NET, EF Core, or any other framework
- **Database Independent**: No knowledge of how data is persisted
- **UI Independent**: No knowledge of how data is presented
- **Testable**: Easy to unit test without any external dependencies

## What NOT to Include

- Database context or migrations
- API controllers or routes
- External service implementations
- Framework-specific code
- Infrastructure concerns (logging, caching, etc.)

## Example Structure

```
Core/
├── Entities/
├── ValueObjects/
├── Interfaces/
├── Enums/
├── Exceptions/
└── Specifications/
```
