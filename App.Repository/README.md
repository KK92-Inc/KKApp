# app.repository

To install dependencies:

```bash
bun install
```

To run:

```bash
bun run index.ts
```

This project was created using `bun init` in bun v1.3.5. [Bun](https://bun.com) is a fast all-in-one JavaScript runtime.


```
                    ┌─────────────────────────────────────────┐
                    │            Docker Network               │
                    │                                         │
   Public Internet  │   ┌──────────────┐                      │
        │           │   │  SSH Server  │◄─── Port 22 (public) │
        │           │   │  Container   │                      │
        ▼           │   └──────┬───────┘                      │
   ┌─────────┐      │          │                              │
   │ Traefik │      │          │ Shared Volume                │
   │ / Nginx │      │          │ /repos                       │
   └────┬────┘      │          │                              │
        │           │   ┌──────┴───────┐                      │
        │           │   │  HTTP API    │◄─── Port 3000        │
        └───────────┼──►│  Container   │     (internal only)  │
                    │   └──────────────┘                      │
                    │                                         │
                    └─────────────────────────────────────────┘
```

# Build executables only
bun --bun run build

# Build executables + Docker images
bun --bun run build -- --docker

# Build with custom tag
bun --bun run build -- --docker --tag=v1.0.0

# Or use docker-compose directly
docker compose up -d

Architecture:

- git-ssh:latest — Public SSH server on port 2222
- git-api:latest — Internal API server (only accessible within Docker network)
- Both share the /home/git/repos volume
-
# Build and start both services
docker compose -f docker-compose.dev.yml up --build

# Or run detached
docker compose -f docker-compose.dev.yml up -d --build

# Test API
curl http://localhost:3000/repo/wizard/example

# Test SSH
ssh -p 2222 git@localhost
