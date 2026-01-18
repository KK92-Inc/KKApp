FROM oven/bun:1 AS build
WORKDIR /app

# Install dependencies with Bun and build
COPY package.json bun.lock ./
RUN bun install --frozen-lockfile
COPY . .
RUN bun run build

FROM oven/bun:1 AS final
WORKDIR /app
ENV NODE_ENV=production
ENV PORT=8000

# Copy built output and node_modules from build stage
COPY --from=build /app/build /app/build
COPY --from=build /app/node_modules /app/node_modules
COPY package.json bun.lock ./

CMD ["bun", "./build/index.js"]
