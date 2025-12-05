FROM oven/bun:1 AS base
WORKDIR /app

FROM base AS build
COPY package.json bun.lock ./
RUN bun install --frozen-lockfile
COPY . .
RUN bun run build

FROM base AS final
WORKDIR /app
COPY --from=build /app/dist/app .
ENV PORT=51842
EXPOSE 51842
CMD ["./app"]
