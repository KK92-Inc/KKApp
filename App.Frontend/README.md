# App.Frontend

A modern, performant frontend application built with SvelteKit 2, Svelte 5, and TailwindCSS 4. This application provides the user interface for the KKApp peer-to-peer learning platform.

## 🛠️ Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| [SvelteKit](https://svelte.dev/docs/kit) | 2.x | Full-stack framework |
| [Svelte](https://svelte.dev) | 5.x | UI components (runes) |
| [TailwindCSS](https://tailwindcss.com) | 4.x | Styling |
| [Bun](https://bun.sh) | 1.x | Runtime & package manager |
| [Vite](https://vite.dev) | 8.x | Build tool |
| [TypeScript](https://www.typescriptlang.org) | 5.x | Type safety |

## 📁 Project Structure

```
App.Frontend/
├── src/
│   ├── app.html          # HTML template
│   ├── app.css           # Global styles (Tailwind)
│   ├── app.d.ts          # Global type declarations
│   ├── hooks.server.ts   # Server hooks (auth middleware)
│   ├── lib/
│   │   ├── api/          # Generated OpenAPI types & client
│   │   ├── components/   # Reusable UI components
│   │   ├── oauth.ts      # Keycloak OAuth integration
│   │   ├── redis.ts      # Redis/Valkey session store
│   │   ├── log.ts        # Logging utilities
│   │   └── utils.ts      # Helper functions
│   └── routes/
│       ├── +layout.svelte
│       ├── +page.svelte
│       ├── auth/         # Authentication routes
│       └── ...           # Feature routes
├── static/               # Static assets
├── scripts/              # Build & utility scripts
└── package.json
```

## 🚀 Getting Started

### Prerequisites

- [Bun](https://bun.sh/) v1.3 or later

### Installation

```bash
bun install
```

### Development

```bash
bun run dev
```

The development server starts at `http://localhost:5174`.

### Building

```bash
bun run build
```

Produces a compiled executable in `dist/app` using SvelteKit's Bun adapter.

### Preview Production Build

```bash
bun run preview
```

## 🔐 Authentication

Authentication is handled via **Keycloak** using OAuth 2.0 / OpenID Connect:

- OAuth flow implemented in [`src/lib/oauth.ts`](src/lib/oauth.ts)
- Session management via Valkey/Redis
- JWT token validation using `jose`

### Environment Variables

Create a `.env` file (see `.env.ci` for reference):

```env
# Application
PORT=5174
ORIGIN=http://localhost:${PORT}
API=http://localhost:3001
SECRET="your-session-secret"

# Keycloak
KC_ORIGIN=http://localhost:8080/
KC_REALM=student
KC_ID=intra
KC_SECRET=your-client-secret
KC_COOKIE=kc.session
KC_CALLBACK=${ORIGIN}/auth/callback

# Public (exposed to client)
PUBLIC_NAME=App
PUBLIC_S3_BUCKET="images"
PUBLIC_S3_ENDPOINT="https://s3.example.com"
```

## 🔗 Backend Integration

The frontend communicates with the backend API using [`openapi-fetch`](https://openapi-ts.dev/openapi-fetch/):

```bash
# Regenerate API types from backend OpenAPI spec
bun run backend:types
```

This generates TypeScript types in `src/lib/api/api.d.ts` from the backend's OpenAPI specification.

## 🎨 UI Components

Built with:

- [bits-ui](https://bits-ui.com/) - Headless UI primitives
- [Lucide Svelte](https://lucide.dev/guide/packages/lucide-svelte) - Icons
- [PaneForge](https://paneforge.com/) - Resizable panes
- [tailwind-variants](https://www.tailwind-variants.org/) - Component variants
- [mode-watcher](https://github.com/svecosystem/mode-watcher) - Dark mode

## 📜 Scripts

| Script | Description |
|--------|-------------|
| `bun run dev` | Start development server |
| `bun run build` | Build for production |
| `bun run preview` | Preview production build |
| `bun run check` | Run Svelte type checking |
| `bun run check:watch` | Type checking in watch mode |
| `bun run lint` | Run ESLint & Prettier check |
| `bun run format` | Format code with Prettier |
| `bun run backend:types` | Generate API types from backend |

## 🐳 Docker

The application is containerized for production deployment:

```dockerfile
FROM oven/bun:1 AS build
# ... build steps ...

FROM oven/bun:1 AS final
COPY --from=build /app/dist/app .
ENV PORT=51842
EXPOSE 51842
CMD ["./app"]
```

Build the image:

```bash
docker build -t kk-frontend .
```

## 🔧 Configuration Files

| File | Purpose |
|------|---------|
| `svelte.config.js` | SvelteKit configuration |
| `vite.config.ts` | Vite build configuration |
| `tsconfig.json` | TypeScript configuration |
| `eslint.config.js` | ESLint rules |
| `components.json` | shadcn-svelte configuration |
| `bunfig.toml` | Bun configuration |

## 📄 License

Copyright © 2025 W2Inc. All Rights Reserved.
