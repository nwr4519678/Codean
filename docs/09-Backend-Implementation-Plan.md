# 09 — Backend Implementation Plan

**Stack:** ASP.NET Core **10**, Clean Architecture, CQRS, PostgreSQL, Redis, Hangfire
**Status:** Approved for build
**Last updated:** 2026-08-03

---

## 1. Solution layout

```
backend/
├── Platform.slnx
├── global.json                              # pins .NET 10 SDK
├── Directory.Build.props                    # shared compile flags (LangVersion, Nullable, etc.)
├── Directory.Packages.props                 # central package versions
├── src/
│   ├── Platform.Domain/                     # entities, VOs, events (no deps)
│   ├── Platform.Application/                # CQRS handlers, DTOs, behaviours, abstractions
│   ├── Platform.Infrastructure/             # EF Core, Redis, Paymob, Google Meet, Teams, R2, Search
│   ├── Platform.Shared/                     # Result, Error, exceptions, Serilog wiring
│   ├── Platform.Api/                        # REST + GraphQL + SignalR host
│   ├── Platform.Worker/                     # Hangfire background jobs
│   └── Platform.Worker/                      # Standalone sandboxed code-execution service
└── tests/
    ├── Platform.Domain.UnitTests/
    ├── Platform.Application.UnitTests/
    └── Platform.ArchitectureTests/         # layer + dependency rules
```

## 2. Build phases

### Phase 0 — Foundations (DONE)
- Solution scaffold, central package management
- Domain entities (User, Course, Module, Lesson, Order, Payment, LiveSession, Assessment, Problem, Badge, Notification, etc.)
- Result pattern + Error type
- MediatR + pipeline behaviours (Logging, Validation, Performance, UnitOfWork, UnhandledException)
- DbContext + entity configurations (snake_case, JSONB, enums)
- JWT token service, BCrypt password hasher, TOTP service
- Paymob client (intention + HMAC + verification)
- Google Meet + Microsoft Teams providers
- Cloudflare R2 storage
- Elasticsearch service
- SMTP email sender
- DI extension per layer
- PlatformApplicationBuilder — single composition entry-point

### Phase 1 — Auth (DONE)
- Login / Register / Refresh / Logout / LogoutAll
- Email verify (request/confirm)
- Password reset (request/confirm)
- 2FA setup / enable / disable / verify
- Session listing + revoke
- OAuth (Google / Microsoft) — adapter interfaces done, controllers in Phase 3

### Phase 2 — Commerce (DONE)
- MonthlyPackage CRUD
- Order + Payment lifecycle
- Coupon validation + redemption
- Webhook handler with HMAC validation + idempotency

### Phase 3 — Courses & Content (NEXT)
- Course / Module / Lesson / Version CRUD
- Lesson ordering (atomic)
- Attachment upload via R2 (presigned PUT → complete → DB row)
- Enrollment
- Course reviews

### Phase 4 — Live Sessions
- LiveSession CRUD (teacher)
- Provider adapter selection (Meet / Teams)
- Brokered join URL with short-lived token
- Attendance logging
- Recording retrieval
- iCal generation

### Phase 5 — Assessments
- Assignment / Exam / Quiz CRUD
- Question bank + random generation
- Submission + answer persistence
- Auto-grading for MCQ / T-F / matching / ordering
- Manual grading with rubrics
- Anti-cheat flags
- PDF export of result

### Phase 6 — Coding Environment
- Problem CRUD
- Test cases
- Verdict polling
- Submission history

### Phase 7 — Notifications & Gamification
- Notification entity + dispatcher
- XP / level / streak / badge events
- Certificate issue + verify
- Push notifications (VAPID)

### Phase 8 — Analytics & Admin
- Teacher analytics aggregation queries
- Student progress
- Admin platform metrics
- Audit log search
- Feature flags

## 3. Test strategy

| Layer | Type | Tool | Target |
|---|---|---|---|
| Domain | Unit | xUnit + FluentAssertions | Invariants, factories, state transitions |
| Application | Unit | xUnit + NSubstitute | Handler behavior, validation, mapping |
| Infrastructure | Integration | Testcontainers (Postgres + Redis) | Repository + external adapters |
| Api | Functional | `WebApplicationFactory` + Respawn | Endpoint contracts, auth flows |
| Architecture | ArchUnit | TngTech.ArchUnitNET | Layer dependencies, naming rules |

CI runs unit + integration on every push; functional + load on PR and main.

## 4. Background jobs (Hangfire)

| Job | Cadence |
|---|---|
| `Recurring:JwtCleanup` | Daily — drop revoked/expired refresh tokens |
| `Recurring:PackageAccessExpiry` | Hourly — emit `SubscriptionExpired` notifications |
| `Recurring:StreakRollover` | Daily 00:00 UTC |
| `Recurring:CertificateVerify` | Daily — verify stored cert URLs still resolve |
| `FireAndForget:SearchReindex` | After save in `courses`, `lessons` |
| `FireAndForget:EmailSend` | From notification dispatch |
| `FireAndForget:PaymobVerify` | Defence in depth after webhook |
| `Delayed:LiveSessionStartReminder` | T-15 min before session |

## 5. Configuration

Configuration is layered (each overrides the previous):

1. `appsettings.json` — defaults
2. `appsettings.{Environment}.json` — environment overrides
3. Environment variables (12-factor)
4. Kubernetes Secrets / External Secrets Operator → Vault

Strongly-typed `IOptions<T>` per concern. Validation via `ValidateDataAnnotations` and `ValidateOnStart`.

## 6. Observability hooks

- **Logging:** Serilog → JSON → stdout + OpenSearch
- **Metrics:** `prometheus-net.AspNetCore` exposes `/metrics`
- **Tracing:** OpenTelemetry → OTLP → Jaeger / Tempo
- **Health:** `AspNetCore.HealthChecks` at `/health/live`, `/health/ready`

## 7. Build & run

```bash
# Restore
dotnet restore

# Apply migrations (after first migration is added)
dotnet ef database update \
  --project src/Platform.Infrastructure \
  --startup-project src/Platform.Api

# Run API
dotnet run --project src/Platform.Api

# Run worker (separate process)
dotnet run --project src/Platform.Worker
```
