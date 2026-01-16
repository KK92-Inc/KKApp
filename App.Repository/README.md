# App.Repository

A custom Git hosting solution built with Bun, providing both REST API and SSH server functionality for repository management. This enables the platform to host user project repositories with full Git support.

## 🎯 Overview

The repository service consists of two components:

| Component | Purpose | Port |
|-----------|---------|------|
| **Git API** | Internal REST API for repository management | 3000 |
| **Git SSH** | Public SSH server for git push/pull operations | 22 (2222 in dev) |

## 🏗️ Architecture

```
                    ┌─────────────────────┐
                    │     Backend API     │
                    └──────────┬──────────┘
                               │ HTTP (internal)
                               ▼
┌─────────────────────────────────────────────────────────┐
│                      Git API Server                     │
│  • Create/delete repositories                           │
│  • Browse tree structure                                │
│  • Read file contents (blobs)                           │
│  • Rename repositories                                  │
└─────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────┐
│                 Shared Volume: /home/git/repos          │
│         owner1/repo1.git, owner1/repo2.git, ...         │
└─────────────────────────────────────────────────────────┘
                               ▲
                               │
┌─────────────────────────────────────────────────────────┐
│                     Git SSH Server                      │
│  • SSH key authentication (via PostgreSQL)              │
│  • git-receive-pack (push)                              │
│  • git-upload-pack (pull/clone)                         │
└─────────────────────────────────────────────────────────┘
                               ▲
                               │ SSH (public)
                    ┌──────────┴──────────┐
                    │    Git Clients      │
                    │  (git push/pull)    │
                    └─────────────────────┘
```

## 📁 Project Structure

```
App.Repository/
├── src/
│   ├── api/
│   │   └── git.exe.ts        # REST API server
│   ├── ssh/
│   │   ├── auth.exe.ts       # SSH key authentication
│   │   └── shell.exe.ts      # Git shell for SSH sessions
│   └── http/                 # Shared HTTP utilities
├── config/
│   ├── sevice.toml           # Service configuration
│   └── sshd_config           # OpenSSH server config
├── build.ts                  # Build script (compiles to executables)
├── Dockerfile.api            # Git API container
├── Dockerfile.ssh            # Git SSH container
└── docker-compose.dev.yml    # Local development setup
```

## 🔌 API Endpoints

### Repository Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/repo/:owner/:name` | Check if repository exists |
| `POST` | `/repo/:owner/:name` | Create a new bare repository |
| `DELETE` | `/repo/:owner/:name` | Delete a repository |
| `POST` | `/repo/:owner/:name/rename/:new` | Rename a repository |

### Repository Browsing

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/repo/:owner/:name/tree/:branch/*` | List directory contents |
| `GET` | `/repo/:owner/:name/blob/:branch/*` | Get file contents |

### Response Codes

| Code | Meaning |
|------|---------|
| `200` | Success |
| `201` | Created |
| `204` | Success (no content) |
| `404` | Repository/path not found |
| `409` | Conflict (already exists) |

## 🔐 SSH Authentication

The SSH server uses a custom authentication flow:

1. User connects via SSH with their public key
2. `auth.exe` queries PostgreSQL to validate the key
3. If valid, connection is established with `shell.exe`
4. Git commands are executed in the repository context

### SSH Configuration

```
# config/sshd_config
AuthorizedKeysCommand /usr/local/bin/auth %u %t %k
AuthorizedKeysCommandUser root
ForceCommand /home/git/shell
```

## 🚀 Development

### Prerequisites

- [Bun](https://bun.sh/) v1.3+
- Docker (for full stack testing)

### Local Development

```bash
# Install dependencies
bun install

# Build executables
bun run build

# Output: ./output/api-git, ./output/ssh-shell, ./output/ssh-auth
```

### Configuration

Edit `config/sevice.toml`:

```toml
[ssh]
enabled = true
port = 22
max_timeout = 120
idle_timeout = 60

[api]
enabled = true
port = 3000
```

### Testing

```bash
# Run tests
bun test

# Or directly
bun run test.bun.ts
```

## 🐳 Docker

### Build Images

```bash
# Build both images
bun run build:docker

# Build and tag for registry
bun run build:docker:tag
```

### Docker Compose (Development)

```bash
docker compose -f docker-compose.dev.yml up
```

### Image Details

**Git API (`Dockerfile.api`)**
- Base: `debian:bookworm-slim`
- Includes: `git`, compiled `api-git` executable
- Exposes: Port 3000
- Volume: `/home/git/repos`

**Git SSH (`Dockerfile.ssh`)**
- Base: `debian:bookworm-slim`
- Includes: `openssh-server`, `git`, compiled executables
- Exposes: Port 22
- Volume: `/home/git/repos`
- Connects to: PostgreSQL for SSH key lookup

## 🔧 Integration with KKApp

The Git services integrate with the main application:

1. **Backend API** calls Git API to create/manage repositories
2. **Users** push/pull code via Git SSH using their registered SSH keys
3. **SSH keys** are stored in PostgreSQL and managed through the main API
4. Both services share a volume for repository storage

### Aspire Configuration

```csharp
var gitApi = builder.AddDockerfile("git-api", "./App.Repository", "Dockerfile.api")
    .WithVolume("git-repos", "/home/git/repos")
    .WithHttpEndpoint(port: 3000, targetPort: 3000);

var gitSsh = builder.AddDockerfile("git-ssh", "./App.Repository", "Dockerfile.ssh")
    .WithVolume("git-repos", "/home/git/repos")
    .WithEndpoint(port: 2222, targetPort: 22, scheme: "tcp")
    .WithReference(database);
```

## 📋 Git Operations

### Clone a Repository

```bash
git clone ssh://git@localhost:2222/owner/repo.git
```

### Push Changes

```bash
git remote add origin ssh://git@localhost:2222/owner/repo.git
git push -u origin main
```

## 📄 License

Copyright © 2025 W2Inc. All Rights Reserved.
