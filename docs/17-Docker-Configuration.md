# 17 — Docker Configuration

**Last updated:** 2026-08-03

## 1. Multi-stage Dockerfile (shared by API / Worker / Judge)

Each runtime image uses the same `Dockerfile` template located at `infra/docker/Dockerfile.runtime`. The build arg `PROJECT` selects the source project to publish.

### 1.1 Build context

```
infra/docker/
├── Dockerfile.runtime       # shared multi-stage build
├── Dockerfile.web           # Next.js web build
└── docker-entrypoint.sh     # generic entrypoint (migrations + start)
```

### 1.2 Runtime image

```dockerfile
# syntax=docker/dockerfile:1.7
ARG PROJECT=Platform.Api

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG PROJECT
WORKDIR /src
COPY backend/global.json backend/Directory.Build.props backend/Directory.Packages.props ./
COPY backend/src/Platform.Shared/   ./src/Platform.Shared/
COPY backend/src/Platform.Domain/   ./src/Platform.Domain/
COPY backend/src/Platform.Application/ ./src/Platform.Application/
COPY backend/src/Platform.Infrastructure/ ./src/Platform.Infrastructure/
COPY backend/src/Platform.Api/      ./src/Platform.Api/
COPY backend/src/Platform.Worker/   ./src/Platform.Worker/
COPY backend/src/Platform.Worker/    ./src/Platform.Worker/
RUN dotnet restore src/${PROJECT}/${PROJECT}.csproj
RUN dotnet publish src/${PROJECT}/${PROJECT}.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN groupadd --gid 1001 platform && useradd --uid 1001 -g platform platform
COPY --from=build /app .
COPY infra/docker/docker-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/docker-entrypoint.sh
USER platform
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080
ENTRYPOINT ["/usr/local/bin/docker-entrypoint.sh"]
```

### 1.3 Web image (Next.js)

```dockerfile
FROM node:22-alpine AS deps
WORKDIR /app
COPY frontend/package.json frontend/package-lock.json* ./
COPY frontend/apps/web/package.json ./apps/web/
COPY frontend/packages ./packages
RUN npm ci

FROM node:22-alpine AS build
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY frontend/ .
ENV NEXT_TELEMETRY_DISABLED=1
RUN npm run build --workspace=apps/web

FROM node:22-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production NEXT_TELEMETRY_DISABLED=1
RUN addgroup -g 1001 -S nodejs && adduser -S nextjs -u 1001
COPY --from=build /app/apps/web/.next ./apps/web/.next
COPY --from=build /app/apps/web/public ./apps/web/public
COPY --from=build /app/apps/web/package.json ./apps/web/package.json
COPY --from=build /app/node_modules ./node_modules
USER nextjs
EXPOSE 3000
CMD ["npx", "next", "start", "-p", "3000", "apps/web"]
```

## 2. Local docker-compose

`infra/docker-compose/docker-compose.yml` brings up the full stack for dev:

```yaml
version: "3.9"

services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: platform
      POSTGRES_PASSWORD: platform
      POSTGRES_DB: platform
    ports: ["5432:5432"]
    volumes: [pgdata:/var/lib/postgresql/data]

  redis:
    image: redis:7-alpine
    command: ["redis-server", "--appendonly", "yes"]
    ports: ["6379:6379"]
    volumes: [redisdata:/data]

  opensearch:
    image: opensearchproject/opensearch:2.18.0
    environment:
      discovery.type: single-node
      plugins.security.disabled: "true"
      OPENSEARCH_JAVA_OPTS: "-Xms512m -Xmx512m"
    ports: ["9200:9200"]
    volumes: [osdata:/usr/share/opensearch/data]

  minio:
    image: minio/minio:latest
    command: server /data --console-address ":9001"
    environment:
      MINIO_ROOT_USER: minio
      MINIO_ROOT_PASSWORD: minio12345
    ports: ["9000:9000", "9001:9001"]
    volumes: [miniodata:/data]

  api:
    build:
      context: ../..
      dockerfile: infra/docker/Dockerfile.runtime
      args: { PROJECT: Platform.Api }
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__Postgres: Host=postgres;Database=platform;Username=platform;Password=platform
      Redis__Configuration: redis:6379
      Elasticsearch__Url: http://opensearch:9200
      CloudflareR2__AccountId: minio
      CloudflareR2__AccessKey: minio
      CloudflareR2__SecretKey: minio12345
      CloudflareR2__Bucket: platform
      Paymob__ApiKey: ${PAYMOB_API_KEY:-test}
      Paymob__HmacSecret: ${PAYMOB_HMAC:-test}
    depends_on: [postgres, redis, opensearch, minio]
    ports: ["8080:8080"]

  worker:
    build:
      context: ../..
      dockerfile: infra/docker/Dockerfile.runtime
      args: { PROJECT: Platform.Worker }
    environment:
      ConnectionStrings__Postgres: Host=postgres;Database=platform;Username=platform;Password=platform
      Redis__Configuration: redis:6379
    depends_on: [postgres, redis]
    command: ["Platform.Worker"]

  judge:
    build:
      context: ../..
      dockerfile: infra/docker/Dockerfile.runtime
      args: { PROJECT: Platform.Worker }
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__Postgres: Host=postgres;Database=platform;Username=platform;Password=platform
    depends_on: [postgres]
    ports: ["8081:8080"]

  web:
    build:
      context: ../..
      dockerfile: infra/docker/Dockerfile.web
    environment:
      NEXT_PUBLIC_API_URL: http://localhost:8080
    depends_on: [api]
    ports: ["3000:3000"]

volumes:
  pgdata:
  redisdata:
  osdata:
  miniodata:
```

Bring it up:

```bash
docker compose -f infra/docker-compose/docker-compose.yml up -d
```

## 3. Image registry & tagging

- ECR for primary registry (`<account>.dkr.ecr.us-east-1.amazonaws.com/platform/api:<sha>`).
- GitHub Actions builds & pushes on every merge to `main`.
- Tags: `<sha>` (immutable), `latest` (latest build), `v1.2.3` (release).
