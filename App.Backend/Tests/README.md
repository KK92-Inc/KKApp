# App.Backend.Tests

This project contains unit tests for the backend services.

## Running Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~SubscriptionServiceTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Project Structure

```
Tests/
├── Fixtures/           # Shared test fixtures (e.g., database setup)
├── Services/           # Service layer tests
│   └── SubscriptionServiceTests.cs
├── GlobalUsings.cs     # Global using statements
└── README.md
```

## Testing Patterns

### Unit Tests with In-Memory Database

We use EF Core's in-memory database provider for testing services that interact with the database. This allows for fast, isolated tests without needing a real database.

### Mocking Dependencies

Use Moq for mocking service dependencies:

```csharp
var mockService = new Mock<IUserProjectService>();
mockService.Setup(x => x.SomeMethod()).ReturnsAsync(expectedResult);
```

### Assertions with FluentAssertions

Use FluentAssertions for readable test assertions:

```csharp
result.Should().NotBeNull();
result.State.Should().Be(EntityObjectState.Active);
await act.Should().ThrowAsync<ServiceException>().WithMessage("Expected message");
```
