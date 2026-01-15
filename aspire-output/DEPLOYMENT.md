# Deployment Guide

This directory contains production deployment files for the application.

## Overview

The application uses GitHub Container Registry (ghcr.io) to host Docker images. When you push to `main`/`master`, the CI/CD pipeline automatically:

1. Builds all Docker images
2. Pushes them to `ghcr.io/<your-username>/<image-name>`
3. Tags them with the commit SHA and `latest`

## Images Built

| Image | Source | Description |
|-------|--------|-------------|
| `backend` | .NET API | Backend REST API |
| `migration-job` | .NET Worker | Database migrations |
| `frontend-app` | Dockerfile | SvelteKit frontend |
| `git-api` | Dockerfile | Git repository API |
| `git-ssh` | Dockerfile | Git SSH server |

## Deployment Steps

### 1. Configure GitHub Repository

Make sure your repository has the correct permissions:

1. Go to **Settings** → **Actions** → **General**
2. Under "Workflow permissions", select **Read and write permissions**
3. Enable **Allow GitHub Actions to create and approve pull requests** (if you want auto-updates)

### 2. Trigger a Build

Push to `main`/`master` or manually trigger the workflow:

```bash
gh workflow run "Build and Publish Docker Images"
```

### 3. Set Up Production Environment

1. Copy the template:
   ```bash
   cp .env.template .env
   ```

2. Edit `.env` with your production secrets:
   ```bash
   # Required changes:
   REGISTRY_PREFIX=ghcr.io/your-github-username  # lowercase!
   POSTGRES_PWD=your-secure-password
   CACHE_PASSWORD=your-cache-password
   KC_CLIENT_SECRET=your-keycloak-secret
   ```

### 4. Login to GitHub Container Registry

```bash
# Create a Personal Access Token (PAT) with read:packages scope
# at https://github.com/settings/tokens

docker login ghcr.io -u YOUR_GITHUB_USERNAME
# Enter your PAT as the password
```

### 5. Deploy

```bash
# Pull latest images and start
docker compose -f docker-compose.prod.yaml pull
docker compose -f docker-compose.prod.yaml up -d

# View logs
docker compose -f docker-compose.prod.yaml logs -f
```

## Files

| File | Purpose |
|------|---------|
| `docker-compose.yaml` | Aspire-generated compose (local dev) |
| `docker-compose.prod.yaml` | Production compose using ghcr.io images |
| `.env.Production` | Auto-updated by CI with latest image tags |
| `.env.template` | Template for production secrets |

## Using Specific Versions

To pin to a specific version instead of `latest`:

```bash
# In .env
IMAGE_TAG=abc123def456  # Use the commit SHA
```

## Viewing Available Images

```bash
# List your packages
gh api user/packages?package_type=container

# Or visit: https://github.com/<username>?tab=packages
```

## Troubleshooting

### "unauthorized" when pulling images

1. Make sure you're logged in: `docker login ghcr.io`
2. Check the package visibility (Settings → Packages → make public or add collaborators)
3. Ensure your PAT has `read:packages` scope

### Images not found

1. Check the workflow ran successfully in the Actions tab
2. Verify the image name matches: `ghcr.io/<owner>/<name>:tag`
3. Owner must be lowercase!

### Migration job keeps restarting

The migration job is designed to run once and exit. Check logs:
```bash
docker compose -f docker-compose.prod.yaml logs migration-job
```
