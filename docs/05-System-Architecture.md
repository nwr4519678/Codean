# 05 — System Architecture

**Version:** 1.0
**Last updated:** 2026-08-03

---

## 1. High-level view

```
                                ┌─────────────────────┐
                                │   Cloudflare CDN    │
                                │  (WAF, DDoS, cache) │
                                └──────────┬──────────┘
                                           │
                ┌──────────────────────────┼──────────────────────────┐
                │                          │                          │
        ┌───────▼────────┐       ┌─────────▼──────────┐      ┌─────────▼─────────┐
        │  Next.js (Web) │       │  Next.js (Admin)   │      │   Marketing Site  │
        │  App Router    │       │                   │      │  (Next.js SSG)    │
        └───────┬────────┘       └─────────┬──────────┘      └─────────┬─────────┘
                │                          │                          │
                └──────────────────────────┼──────────────────────────┘
                                           │  HTTPS / JWT
                                           ▼
                              ┌──────────────────────────┐
                              │   API Gateway (YARP)     │
                              │  - Rate limit            │
                              │  - AuthN (JWT)           │
                              │  - Routing               │
                              └──────────┬───────────────┘
                                         │
        ┌─────────────────────┬──────────┼──────────┬─────────────────────┐
        │                     │          │          │                     │
 ┌──────▼──────┐       ┌──────▼──────┐  ...       ┌──▼──────────┐  ┌──────▼──────┐
 │  Identity   │       │  Courses    │           │  Payments   │  │  Live Svc   │
 │  Service    │       │  Service    │           │  Service    │  │  (Meet/     │
 │             │       │             │           │  (Paymob)   │  │   Teams)    │
 └──────┬──────┘       └──────┬──────┘           └──┬──────────┘  └──────┬──────┘
        │                     │                     │                    │
        └─────────────────────┴──────────┬──────────┴────────────────────┘
                                         │
                  ┌──────────────────────┼──────────────────────┐
                  │                      │                      │
           ┌──────▼──────┐        ┌──────▼──────┐         ┌──────▼──────┐
           │ PostgreSQL  │        │   Redis     │         │  Elastic    │
           │  (primary + │        │  (cache,    │         │  search     │
           │  replicas)  │        │  sessions,  │         │  (courses,  │
           │             │        │  rate limit)│         │  lessons)   │
           └──────┬──────┘        └──────┬──────┘         └──────┬──────┘
                  │                      │                      │
                  └──────────────────────┼──────────────────────┘
                                         │
                                ┌────────▼────────┐
                                │ Cloudflare R2   │
                                │ (videos, files) │
                                └─────────────────┘

   ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
   │ Platform.Worker │    │ Platform.Worker  │    │ SignalR Hub     │
   │ (Hangfire)      │    │ (Code Runner)   │    │ (real-time)     │
   └─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 2. Backend architecture

### 2.1 Clean Architecture layout

```
Platform.sln
├── src/
│   ├── Platform.Domain              ← entities, VOs, events, enums (NO deps)
│   ├── Platform.Application         ← use cases (CQRS), DTOs, validators, interfaces
│   ├── Platform.Infrastructure      ← EF Core, Redis, ES, external services
│   ├── Platform.Shared              ← cross-cutting: Result, exceptions, helpers
│   ├── Platform.Api                 ← REST + GraphQL + SignalR host
│   ├── Platform.Worker              ← Hangfire hosted service
│   └── Platform.Worker               ← standalone code-execution API
└── tests/
    ├── Platform.Domain.UnitTests
    ├── Platform.Application.UnitTests
    ├── Platform.Infrastructure.IntegrationTests   (Testcontainers)
    ├── Platform.Api.FunctionalTests              (WebApplicationFactory)
    └── Platform.ArchitectureTests                (NetArchTest)
```

### 2.2 Dependency rule

```
Domain  ←  Application  ←  Infrastructure
   ↑          ↑
   └──────────┴──  Api / Worker / Judge
```

- `Domain` references nothing.
- `Application` references only `Domain` + `Shared`.
- `Infrastructure` references everything below.

### 2.3 Cross-cutting patterns

| Pattern | Where | Why |
|---|---|---|
| **CQRS** | `Application/Features/*` | Separate read/write paths, scale independently |
| **MediatR** | `Application/Common/Behaviours` | Pipeline: logging, validation, caching, transactions |
| **Result<T>** | `Shared/Results` | Avoid exceptions for expected failures |
| **Specification** | `Domain/Specifications` | Reusable, composable queries |
| **Unit of Work** | `Infrastructure/Persistence` | Atomic transactions across aggregates |
| **Repository** | `Domain/Repositories` (interface) + `Infrastructure/Persistence/Repositories` (impl) | Testable persistence |
| **Domain Events** | `Domain/Events` | Decouple side effects |
| **Outbox** | `Infrastructure/Outbox` | Reliable async dispatch |
| **Options pattern** | `Api/Options` | Strongly-typed config |
| **Policy-based authz** | `Infrastructure/Authorization` | Centralized, testable rules |
| **Adapter (Anti-corruption layer)** | `Infrastructure/ExternalServices/Paymob` | Isolate vendor schema |

### 2.4 MediatR pipeline behaviours (order)

1. `LoggingBehaviour` — request start/end + duration
2. `ValidationBehaviour` — FluentValidation
3. `AuthorizationBehaviour` — policy check
4. `CachingBehaviour` — read-through cache (Redis)
5. `UnitOfWorkBehaviour` — open transaction for commands
6. `PerformanceBehaviour` — warn if > 500 ms
7. `UnhandledExceptionBehaviour` — last-resort error envelope

## 3. Frontend architecture

### 3.1 Stack

- **Next.js 15** (App Router) + **React 19** + **TypeScript (strict)**
- **Tailwind CSS** + **Shadcn UI** + **Radix UI**
- **TanStack Query** for server state
- **Zustand** for client state (small footprint)
- **React Hook Form** + **Zod** for forms
- **Monaco Editor** for code
- **Framer Motion** for animations
- **next-intl** for i18n (ar, en, fr)
- **next-pwa** for PWA / offline

### 3.2 Folder structure (apps/web)

```
src/
├── app/
│   ├── (marketing)/         ← public landing
│   ├── (auth)/              ← login, register, oauth callback
│   ├── (student)/           ← student-only routes
│   │   ├── dashboard/
│   │   ├── courses/
│   │   ├── live/[sessionId]/
│   │   └── ...
│   ├── (teacher)/           ← teacher-only routes
│   ├── (parent)/
│   ├── (admin)/
│   ├── api/                 ← BFF routes (thin)
│   └── layout.tsx
├── components/
│   ├── ui/                  ← Shadcn primitives
│   ├── domain/              ← CourseCard, LessonPlayer, CodeRunner
│   └── layouts/
├── lib/
│   ├── api/                 ← generated client
│   ├── auth/
│   ├── rbac/
│   ├── i18n/
│   └── utils/
├── hooks/
├── stores/                  ← Zustand
├── styles/
└── types/                   ← generated from OpenAPI
```

### 3.3 Rendering strategy

| Route | Strategy | Why |
|---|---|---|
| Landing, marketing | SSG | Speed, SEO |
| Course catalog | ISR (revalidate 60 s) | Fresh + cached |
| Course detail | SSR + revalidate | SEO + personalization |
| Lesson player | CSR (after auth) | Heavy state, no SEO need |
| Dashboard | CSR | User-specific |
| Admin | CSR | User-specific |
| Live session | CSR (SignalR) | Real-time |

## 4. Data flow — example: subscribe to Month 1

```
1. Student clicks "Subscribe"
2. Web → POST /api/v1/payments/orders  { courseId, monthId }
3. Api → MediatR CreateOrderCommand
4. Handler → IUnitOfWork.Begin, write Order(pending), commit
5. Handler → IPaymobClient.CreateIntention(order)
6. Returns clientSecret + checkoutUrl
7. Web → redirect to Paymob hosted checkout
8. Paymob → user pays
9. Paymob → POST /api/v1/webhooks/paymob  (HMAC signed)
10. Webhook controller → validate HMAC
11. → ProcessPaymobWebhookCommand
12. Handler → idempotency check (transaction id)
13. → mark Order = Paid
14. → grant MonthlyPackageAccess(userId, monthId, 365 days)
15. → emit SubscriptionActivated event
16. → handler: send email, in-app notification, generate invoice PDF (R2)
17. → outbox dispatcher: BI/event-bus
18. Web (open page after redirect) → GET /api/v1/subscriptions/me → shows active
```

## 5. Cross-cutting concerns

### 5.1 Logging (Serilog)
- JSON sink to stdout
- OpenSearch sink for search/retain
- Enrichers: `RequestId`, `CorrelationId`, `UserId`, `Tenant`
- Levels: Information (default), Warning (4xx), Error (5xx), Fatal (1xx)

### 5.2 Tracing (OpenTelemetry)
- ASP.NET Core, HttpClient, EF Core auto-instrumentation
- Exporters: OTLP → Jaeger / Tempo
- Trace context propagated to Paymob webhook (header)
- Frontend: OpenTelemetry JS → OTel collector

### 5.3 Metrics (Prometheus + Grafana)
- Per-endpoint RED metrics
- Per-handler metrics
- DB connection pool, Redis hit rate, ES query latency

### 5.4 Caching
- **L1 (in-memory):** `IMemoryCache` per pod for hot data
- **L2 (Redis):** shared, with tag-based invalidation
- **Read-through** via MediatR `CachingBehaviour`
- **Write-through** invalidation in `UnitOfWorkBehaviour` after commit
- Cache keys: `entity:{type}:{id}` and list/query keys

### 5.5 Resilience
- **Polly** policies on every external HTTP: retry (exp backoff), circuit breaker, timeout
- **Database retry on transient errors** (`EnableRetryOnFailure`)
- **Outbox pattern** for webhooks → state machine (so Paymob retries are safe)

## 6. Security architecture

- **Network:** private subnets for API/DB; only ALB / CDN in public subnets
- **WAF:** Cloudflare WAF rules + rate limit + bot management
- **AuthN:** Identity service issues JWT (RS256) + refresh token (httpOnly cookie, SameSite=Lax, Secure)
- **AuthZ:** policy-based (`RequireRole`, `RequirePermission`, `RequireSubscription`, `RequireCourseEnrollment`)
- **Secrets:** K8s Secrets backed by External Secrets Operator → Vault (v2)
- **PII:** hashed emails for analytics; payment data only via Paymob tokenization
- **Audit:** every privileged action → `audit_logs` table (append-only, 365-day retention)

## 7. Deployment architecture

See [`16-Deployment-Architecture.md`](./16-Deployment-Architecture.md). Short version:

- **Cloud:** AWS (primary), GCP (DR)
- **Container orchestration:** EKS
- **DB:** RDS Postgres (Multi-AZ) + 2 read replicas
- **Cache:** ElastiCache Redis cluster
- **Object storage:** Cloudflare R2
- **Search:** Elastic Cloud (or self-hosted ES on EKS)
- **CDN:** Cloudflare
- **CI/CD:** GitHub Actions → ECR → ArgoCD
- **Observability:** Grafana Cloud (Prometheus + Loki + Tempo)

## 8. Decisions log (key)

| ID | Decision | Rationale |
|---|---|---|
| ADR-001 | Clean Architecture + CQRS | Testability, separation, scale reads |
| ADR-002 | MediatR for in-process messaging | Decouples handlers, easy pipeline |
| ADR-003 | PostgreSQL over SQL Server | Cost, JSONB, extensions, OSS |
| ADR-004 | Redis cluster | Battle-tested, fast |
| ADR-005 | Cloudflare R2 over S3 | S3-compatible, no egress fees |
| ADR-006 | Hangfire for v1 | SQL-backed, easy; swap to RabbitMQ in v2 |
| ADR-007 | Next.js App Router | SSR + RSC + streaming, modern |
| ADR-008 | Shadcn UI | Own the code, theme it freely |
| ADR-009 | TanStack Query | De facto server-state lib for React |
| ADR-010 | Monaco editor | VS Code engine, ubiquitous |
| ADR-011 | Single binary subscription (Free/Subscribed) per the brief | Aligns with monthly-package model |
| ADR-012 | Paymob as the only PSP in v1 | Market fit, multi-method support |
| ADR-013 | Outbox + idempotency keys for webhooks | Reliability, exactly-once semantics |
