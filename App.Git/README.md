# App.Git

A custom Git server with REST API for repository management, built with ASP.NET Core minimal APIs.

## Features

- **Repository Management**: Create, list, update, and delete git repositories
- **User Authentication**: JWT Bearer token authentication (compatible with Keycloak, Auth0, etc.)
- **SSH Key Management**: Users can add/remove SSH public keys
- **Collaborator Management**: Add/remove collaborators with different permission levels
- **Git Data Access**: Query commits, branches, list files, and get file contents via REST API
- **Visibility Controls**: Private, Internal, and Public repository visibility

## API Endpoints

### Repositories (`/repos`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/repos` | List all accessible repositories |
| POST | `/repos` | Create a new repository |
| GET | `/repos/{owner}/{name}` | Get repository details |
| PATCH | `/repos/{owner}/{name}` | Update repository settings |
| DELETE | `/repos/{owner}/{name}` | Delete a repository |
| GET | `/repos/{owner}/{name}/commits` | List commits |
| GET | `/repos/{owner}/{name}/commits/{sha}` | Get a specific commit |
| GET | `/repos/{owner}/{name}/branches` | List branches |
| GET | `/repos/{owner}/{name}/tree/{path}` | List files in directory |
| GET | `/repos/{owner}/{name}/blob/{path}` | Get file content (base64) |

### Collaborators (`/repos/{owner}/{name}/collaborators`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/repos/{owner}/{name}/collaborators` | List collaborators |
| PUT | `/repos/{owner}/{name}/collaborators` | Add/update collaborator |
| DELETE | `/repos/{owner}/{name}/collaborators/{username}` | Remove collaborator |

### Users (`/user`, `/users`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/user` | Get current authenticated user |
| GET | `/users/{username}` | Get user by username |

### SSH Keys (`/user/keys`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/user/keys` | List SSH keys |
| POST | `/user/keys` | Add SSH key |
| DELETE | `/user/keys/{id}` | Delete SSH key |

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=gitserver;Username=postgres;Password=postgres"
  },
  "Git": {
    "RepositoriesPath": "/var/lib/git/repositories"
  },
  "Auth": {
    "Authority": "https://your-keycloak.example.com/realms/your-realm",
    "Audience": "git-server"
  }
}
```

## Setup

### 1. Database Setup
Create a PostgreSQL database and run migrations:

```bash
# Install EF Core tools if not already installed
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add Initial

# Apply migrations
dotnet ef database update
```

### 2. Repository Storage
Ensure the configured `RepositoriesPath` directory exists and is writable:

```bash
sudo mkdir -p /var/lib/git/repositories
sudo chown -R $USER:$USER /var/lib/git/repositories
```

### 3. Authentication Provider
Configure your JWT authentication provider (Keycloak, Auth0, etc.):

1. Create a client for the git-server
2. Update `Auth:Authority` with your realm URL
3. Update `Auth:Audience` with your client ID

### 4. Run the Server

```bash
dotnet run
```

The API will be available at `http://localhost:5000` (or your configured port).

## Permission Levels

| Level | Description |
|-------|-------------|
| `read` | Can view repository content |
| `write` | Can push to repository |
| `admin` | Can manage settings and collaborators |

## Visibility Levels

| Level | Description |
|-------|-------------|
| `private` | Only owner and collaborators can access |
| `internal` | All authenticated users can view |
| `public` | Anyone can view (no auth required) |

## Example Usage

### Create a Repository
```bash
curl -X POST http://localhost:5000/repos \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "my-repo", "description": "My first repo", "visibility": "private"}'
```

### Add SSH Key
```bash
curl -X POST http://localhost:5000/user/keys \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title": "My Laptop", "publicKey": "ssh-ed25519 AAAA... user@host"}'
```

### Add Collaborator
```bash
curl -X PUT http://localhost:5000/repos/owner/repo/collaborators \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"username": "collaborator", "permission": "write"}'
```

### Get File Content
```bash
curl http://localhost:5000/repos/owner/repo/blob/src/main.py?branch=main \
  -H "Authorization: Bearer $TOKEN"
```

## Dependencies

- **LibGit2Sharp**: Native git operations
- **Npgsql.EntityFrameworkCore.PostgreSQL**: PostgreSQL database provider
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT authentication

## Architecture

```
App.Git/
├── Data/
│   └── GitDbContext.cs          # EF Core database context
├── Endpoints/
│   ├── DTOs.cs                   # Request/Response DTOs
│   ├── RepositoryEndpoints.cs    # Repository API
│   ├── CollaboratorEndpoints.cs  # Collaborator API
│   ├── SshKeyEndpoints.cs        # SSH Key API
│   └── UserEndpoints.cs          # User API
├── Models/
│   ├── BaseEntity.cs             # Base entity with timestamps
│   ├── User.cs                   # User entity
│   ├── Repository.cs             # Repository entity
│   ├── SshKey.cs                 # SSH Key entity
│   └── Collaborator.cs           # Collaborator entity
├── Services/
│   ├── GitService.cs             # Git operations (LibGit2Sharp)
│   ├── GitDTOs.cs                # Git data transfer objects
│   ├── RepositoryService.cs      # Repository business logic
│   ├── UserService.cs            # User management
│   ├── SshKeyService.cs          # SSH key management
│   └── CollaboratorService.cs    # Collaborator management
└── Program.cs                    # Application entry point
```