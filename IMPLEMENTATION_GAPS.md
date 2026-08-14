# Implementation Gaps

## Executive Summary

* Total gaps: 20
* Critical: 2
* High: 12
* Medium: 5
* Low: 1

### Current Status

* NOT COMPLETE

The repository compiles and its current backend unit/architecture tests pass, but the active user-facing product is not complete. The public Vite app contains local fixture workflows and fake success states, the admin console is mostly fixture-driven, several infrastructure adapters intentionally do nothing, and the production deployment path does not match the checked-in projects. The backend has useful implemented foundations, but end-to-end behavior, resource authorization, cache correctness, payment entitlement, concurrent background processing, and integration coverage remain incomplete.

### Verification performed

* `dotnet build Platform.slnx --no-restore --maxcpucount:1`: passed, 0 warnings, 0 errors.
* `dotnet test Platform.slnx --no-restore --maxcpucount:1`: passed, 242/242 tests.
* `pnpm --filter @codean/web typecheck` and production build: passed.
* `pnpm --filter @codean/admin typecheck` and production build: passed.
* Frontend verification emitted pnpm warnings because `pnpm.peerDependencyRules` is configured in `frontend/package.json`, where current pnpm ignores it.
* No production database/provider, Docker, Kubernetes, browser E2E, integration, DAST, dependency-scan, or load-test run was available.

---

# Critical Missing Implementations

## GAP-001 — Active public web workflows are fixture-only

**Priority:** P0

**Type:** Partial Implementation / Missing Integration

**Location:**

```text
frontend/apps/web/src/pages.tsx
frontend/apps/web/src/App.tsx
```

**Current State:** The active app renders local course/challenge/assessment/live/profile data. Login navigates based on a selected role instead of calling authentication; checkout navigates directly to success; homework/exam/settings/course-editor actions only update local state; code execution displays a hard-coded Accepted result.

**Missing:** Real API calls, authenticated route state, server-backed data loading, mutation persistence, error/loading/empty states, and confirmation that displayed success reflects the backend.

**Why It Matters:** Real students and teachers cannot reliably register, sign in, enroll, pay, submit code, submit assessments, update profiles, or manage courses. The UI can claim operations succeeded while changing no server state.

**Required Implementation:** Wire the active Vite app to `@platform/api` and the backend contracts. Implement auth/session bootstrap, protected route guards, catalog/course/progress flows, assessment submission, judge polling, checkout redirect/error states, live-session access, profile/settings mutations, and teacher CRUD. Replace every fixture and fake success branch in production routes.

**Affected Components:** `frontend/apps/web/src/App.tsx`, `frontend/apps/web/src/pages.tsx`, `frontend/packages/api`, `frontend/packages/contracts`, backend API contracts/controllers.

**Acceptance Criteria:** Browser tests against a running API prove registration, login/logout, catalog retrieval, enrollment/progress, checkout initiation, code submission/status, assessment submission, teacher course mutation, profile update, and failure handling. Refresh shows persisted server state; no production route reports success without a successful API response.

## GAP-002 — Production secrets and default credentials are not removed from runtime paths

**Priority:** P0

**Type:** Security / Infrastructure

**Location:**

```text
backend/src/Platform.Api/appsettings.json
backend/src/Platform.Infrastructure/Configuration/SecurityOptions.cs
backend/src/Platform.Infrastructure/Persistence/DatabaseSeeder.cs
frontend/apps/admin/src/App.tsx
```

**Current State:** Tracked configuration contains a database password and default JWT signing material. Security options contain weak fallback secrets. Development admin credentials are rendered in the admin login page and seeded automatically in Development.

**Missing:** Secret-store-only production configuration, placeholder rejection, credential rotation, history cleanup, and build-time protection against shipping development hints.

**Why It Matters:** A deployment that consumes these values can expose the database and allow token forgery. Admin credentials can leak through the shipped UI.

**Required Implementation:** Remove secrets/defaults from tracked files and history, rotate exposed values, bind production options from managed secrets, validate key length/placeholder patterns on startup, and condition development credential hints on a local-only build that cannot ship to production.

**Affected Components:** API settings/options/startup, Kubernetes External Secrets, Compose, admin login, CI secret scanning.

**Acceptance Criteria:** Clean checkout and built frontend contain no usable credential or private key; production startup fails on missing/placeholder secrets; rotated secrets work in staging; secret scans pass.

---

# High Priority Missing Implementations

## GAP-003 — Admin console data and operations are mostly fabricated

**Priority:** P1

**Type:** Partial Implementation

**Location:** `frontend/apps/admin/src/App.tsx:142-184`

**Current State:** Dashboard metrics, users, course moderation, subscriptions, and audit logs are hard-coded. Teacher creation is the only major screen action calling the API.

**Missing:** API queries, pagination/filtering, server mutations for status/role/course/subscription/audit operations, and operational error states.

**Why It Matters:** Administrators operate on false data and cannot manage the platform reliably.

**Required Implementation:** Use typed API hooks for analytics, users, courses, commerce, and audit logs. Implement loading/empty/error states, pagination, authorization failures, and server-confirmed mutations.

**Affected Components:** Admin app, `frontend/packages/api`, analytics/users/commerce controllers.

**Acceptance Criteria:** Every displayed record originates from API data; all admin mutations persist and update from the response; unauthorized access is denied and tested.

## GAP-004 — Production Docker and CI target missing or obsolete projects

**Priority:** P1

**Type:** Infrastructure / Integration

**Location:** `infra/docker/Dockerfile.runtime:9-17`, `infra/docker/Dockerfile.web:1-29`, `infra/docker/docker-compose/docker-compose.yml:134-159`, `.github/workflows/backend-ci.yml:98-102`

**Current State:** Runtime Docker copies missing `Platform.Shared`/`Platform.Worker` projects and Compose builds `Platform.Worker`. The web image expects Next.js `.next` output while active apps are Vite. CI references a missing `Platform.Infrastructure.IntegrationTests` project.

**Missing:** One authoritative build/deploy model matching the repository.

**Why It Matters:** Local builds can pass while CI and image builds fail, preventing release.

**Required Implementation:** Align Dockerfiles, Compose, CI, Helm, and Kubernetes with the actual active projects and pnpm/Vite apps, or add the missing projects intentionally. Add clean-checkout image-build tests and immutable image tagging.

**Affected Components:** Dockerfiles, Compose, GitHub workflows, frontend package scripts, Kubernetes/Helm image references.

**Acceptance Criteria:** All declared CI project paths exist; API/judge/worker and web/admin images build from a clean checkout; containers start and pass health checks; rendered manifests contain real immutable image references.

## GAP-005 — Cache read contract is functionally broken

**Priority:** P1

**Type:** Bug

**Location:** `backend/src/Platform.Infrastructure/Caching/Hybrid/HybridCacheService.cs:29-39`

**Current State:** `GetAsync` invokes `GetOrCreateAsync` but discards the returned value and always returns the local default.

**Missing:** A correct cache read implementation and behavioral coverage.

**Why It Matters:** Cache hits are never returned, increasing database load and invalidating cache-dependent behavior.

**Required Implementation:** Return the cache operation result using a supported missing-value sentinel or a raw backend read; add L1/L2, miss, expiration, invalidation, and failure-fallback tests.

**Affected Components:** `HybridCacheService`, `CachingBehaviour`, cache integration tests.

**Acceptance Criteria:** Set/get returns the exact value; missing keys return null; tag invalidation removes entries; cache outage behavior is defined and measured.

## GAP-006 — Payment success does not complete the entitlement workflow

**Priority:** P1

**Type:** Partial Implementation / Bug

**Location:** `backend/src/Platform.Application/Features/Commerce/Commands/PaymobWebhookHandlers.cs:66-107`

**Current State:** The handler verifies HMAC/provider status, marks payment paid, and creates an invoice. The injected subscription repository is not used in the shown success path. Amount/order/currency reconciliation against the stored payment is not enforced in the handler.

**Missing:** Atomic payment-to-subscription activation, metadata reconciliation, uniqueness/concurrency protection, and complete webhook contract tests.

**Why It Matters:** A customer may pay without receiving access, or an inconsistent signed callback may activate the wrong order/value.

**Required Implementation:** Match transaction/order/amount/currency to the stored payment, update payment, subscription entitlement, and invoice in one transaction, enforce unique provider identifiers, and make duplicate/concurrent callbacks idempotent.

**Affected Components:** Paymob handler/controller, `Payment`, `StudentSubscription`, `Invoice` mappings/migration, commerce tests.

**Acceptance Criteria:** One successful callback creates exactly one paid payment, one invoice, and the expected active entitlement; duplicate/concurrent callbacks are harmless; mismatches are rejected and audited.

## GAP-007 — Outbox processing lacks cross-process claiming

**Priority:** P1

**Type:** Bug / Background Processing

**Location:** `backend/src/Platform.Infrastructure/Messaging/OutboxProcessor.cs:30-63`

**Current State:** Multiple processors can select the same unprocessed rows before any processor marks them processed.

**Missing:** Atomic claim/lease, handler idempotency, dead-letter visibility, and poison-message metrics.

**Why It Matters:** Replicas can send duplicate emails/notifications or repeat external side effects.

**Required Implementation:** Claim rows with PostgreSQL locking/lease fields, process with bounded retry/backoff, persist failure state, and require idempotency keys for side-effect handlers.

**Affected Components:** Outbox entity/migration, processor, scheduler, event handlers, operational metrics.

**Acceptance Criteria:** Concurrent processor test proves one claim per message; crash/retry tests preserve delivery; exhausted messages are visible and alertable.

## GAP-008 — Resource-level authorization is incomplete

**Priority:** P1

**Type:** Security / Missing Authorization

**Location:** `backend/src/Platform.Application/Features/Assessment/Commands/ExamAttemptHandlers.cs:125-137`, `backend/src/Platform.Application/Features/LiveSessions/Commands/LiveSessionHandlers.cs:253-275`, `backend/src/Platform.Api/Controllers/LiveSessionsController.cs:37-50`

**Current State:** Exam-attempt retrieval checks existence but not current-user ownership. Session attendance retrieval checks authentication but not teacher/session ownership. Live-session schedule/cancel uses `announcements.manage`, not a dedicated live-session permission. Cancellation starts provider deletion fire-and-forget and persists cancellation immediately.

**Missing:** Consistent resource authorization policies, dedicated permissions, awaited provider cleanup/reconciliation, and authorization tests.

**Why It Matters:** Users may read another student's attempt or a session roster, and cancellation can report success while the external meeting remains active.

**Required Implementation:** Add ownership/course enrollment/teacher/admin checks to every resource query and mutation, define live-session permissions, await provider cleanup or enqueue a durable reconciliation job, and return a state reflecting cleanup outcome.

**Affected Components:** Exam attempt handlers, live-session handlers/controllers, permission catalog/policies, tests.

**Acceptance Criteria:** Cross-user/resource matrix tests return 403/404 as designed; only authorized teachers/admins see rosters; cancellation is durable, observable, and eventually reconciles provider state.

## GAP-009 — Notification channels are registered as no-op implementations

**Priority:** P1

**Type:** Missing Integration

**Location:** `backend/src/Platform.Infrastructure/Notifications/Email/SmtpEmailSender.cs:80-93`, `backend/src/Platform.Infrastructure/DependencyInjection.cs:85-88`

**Current State:** Push, SMS, and WhatsApp interfaces resolve successfully but their `SendAsync` methods immediately complete without sending anything. Email failures are logged and swallowed.

**Missing:** Real configured providers or an explicit feature-disabled behavior, durable delivery/retry state, and caller-visible failure semantics.

**Why It Matters:** The product advertises multi-channel notifications and authentication flows depend on email verification/reset messages; silent success can leave users unable to access accounts or miss critical alerts.

**Required Implementation:** Implement the selected production providers, configure secrets and templates, persist delivery attempts through the outbox, classify permanent/transient failures, and expose operational status. If a channel is not in release scope, remove it from advertised behavior and fail explicitly.

**Affected Components:** Notification contracts/handlers, provider adapters, outbox, settings, health checks, tests.

**Acceptance Criteria:** Email verification/reset and notification delivery are proven end-to-end in staging; disabled channels cannot report success; retries and permanent failures are observable.

## GAP-010 — Live meeting provider deletion and recording contracts are placeholders

**Priority:** P1

**Type:** Partial Integration

**Location:** `backend/src/Platform.Infrastructure/LiveSessions/GoogleMeet/GoogleMeetProvider.cs:79-83`, `backend/src/Platform.Infrastructure/LiveSessions/MicrosoftTeams/MicrosoftTeamsProvider.cs:82-83`

**Current State:** Delete returns `true` without calling either provider; recording retrieval always returns null. The API describes recordings and cancellation as supported.

**Missing:** Provider event deletion, recording lookup/storage, and state reconciliation.

**Why It Matters:** Cancelled meetings may remain active and advertised recording functionality cannot work.

**Required Implementation:** Delete the provider event/meeting using the stored provider identifier, implement recording retrieval or explicitly remove recording claims, and make cancellation durable/retryable.

**Affected Components:** Google/Teams adapters, live-session handlers/entities, storage, jobs, tests.

**Acceptance Criteria:** Provider contract tests verify create/delete/recording behavior; cancellation removes or reconciles the remote meeting; recording URLs are valid signed URLs or the feature is removed from scope.

## GAP-011 — Production startup does not validate required configuration

**Priority:** P1

**Type:** Infrastructure

**Location:** `backend/src/Platform.Api/DependencyInjection.cs:44-66`, `backend/src/Platform.Infrastructure/Configuration/SecurityOptions.cs`

**Current State:** Options are bound without `ValidateOnStart`; weak JWT defaults and blank external-service options can allow startup.

**Missing:** Environment-aware validation for database, Redis, JWT, SMTP, R2, Paymob, judge, search, and live providers.

**Why It Matters:** Misconfigured deployments start and fail later on first use, creating partial outages and unsafe fallback behavior.

**Required Implementation:** Add typed validation, reject placeholders/defaults in Production, and provide redacted startup diagnostics.

**Affected Components:** Options classes, startup DI, deployment secrets, configuration tests.

**Acceptance Criteria:** Production configuration tests fail fast for every missing/placeholder requirement; Development remains runnable without weakening Production validation.

## GAP-012 — Migrations are coupled to every replica startup and errors are suppressed in containers

**Priority:** P1

**Type:** Infrastructure / Deployment

**Location:** `backend/src/Platform.Api/Program.cs:43-55`, `infra/docker/docker-entrypoint.sh:6-9`

**Current State:** API startup calls `MigrateAsync`; entrypoint has a second migration path and uses `|| true`.

**Missing:** A single locked migration release job, compatibility policy, backup gate, and failure propagation.

**Why It Matters:** Rolling replicas can race migrations; a failed migration can be hidden while the app starts against an invalid schema.

**Required Implementation:** Move migration to a release/init job, use database locking, fail deployment on errors, and test expand/contract rollback behavior.

**Affected Components:** API startup, entrypoint, CI/CD, Helm/Kubernetes jobs, migration runbooks.

**Acceptance Criteria:** Concurrent rollout cannot double-apply migrations; failed migration blocks release; rollback procedure is tested against a production-like database.

## GAP-013 — Observability endpoints and diagnostics are incomplete

**Priority:** P1

**Type:** Infrastructure

**Location:** `backend/src/Platform.Api/Extensions/HealthCheckExtensions.cs:43-73`, `infra/k8s/10-api.yaml:20-23`

**Current State:** Kubernetes references `/metrics`, but no registration was found in the active API. Full health output includes exception messages. No complete correlation/trace/alert path was demonstrated.

**Missing:** Metrics endpoint, safe diagnostic separation, correlation IDs, background/provider metrics, and alerting.

**Why It Matters:** Operators cannot reliably detect or diagnose failures, and public health responses can disclose infrastructure details.

**Required Implementation:** Register Prometheus/OTel metrics, redact public health output, keep detailed diagnostics internal, propagate correlation IDs, and define alerts for API, jobs, outbox, providers, cache, and dependencies.

**Affected Components:** API middleware/extensions, OpenTelemetry setup, Kubernetes ServiceMonitor, dashboards/alerts.

**Acceptance Criteria:** Scrape and failure-drill tests prove metrics and alerts; public health contains no exception/secret text; a request can be followed through API → job → provider.

## GAP-014 — Integration and end-to-end test suites are absent or placeholders

**Priority:** P1

**Type:** Missing Test Suite

**Location:** `backend/tests/`, `.github/workflows/backend-ci.yml:69-102`, `.github/workflows/frontend-ci.yml:34-35`

**Current State:** The solution contains unit/architecture tests only; CI references a missing infrastructure integration test project. Frontend CI prints “no tests yet” and exits successfully.

**Missing:** Real HTTP/EF/Redis tests, authorization matrix, webhook/provider contracts, browser journeys, accessibility, failure-path, and load tests.

**Why It Matters:** Passing unit tests does not prove the runtime wiring, persistence, security boundaries, or product workflows.

**Required Implementation:** Add disposable dependency-backed integration tests, API tests, security tests, provider contract tests, Playwright/Vitest coverage, and make critical failures fail CI.

**Affected Components:** Backend test projects, frontend apps, CI workflows, test fixtures.

**Acceptance Criteria:** CI executes non-placeholder tests for critical journeys and rejects regressions; integration tests cover migrations, authorization, payments, outbox, caching, and provider failure behavior.

---

# Medium Priority Missing Implementations

## GAP-015 — Access and refresh tokens are stored in browser localStorage

**Priority:** P2

**Type:** Security Hardening

**Location:** `frontend/apps/admin/src/App.tsx:35-86`, `frontend/packages/api/src/client.ts:5-43`

**Current State:** Both apps store bearer access/refresh tokens in localStorage.

**Missing:** HttpOnly refresh-cookie strategy, CSRF protection, and browser security tests.

**Why It Matters:** Any XSS can exfiltrate refresh credentials and persist account compromise.

**Required Implementation:** Use secure SameSite HttpOnly refresh cookies, short-lived access tokens, CSRF defenses, rotation, and logout/revocation tests.

**Affected Components:** Auth API, frontend client, admin auth provider, security headers.

**Acceptance Criteria:** JavaScript cannot read refresh tokens; CSRF and replay tests pass; multi-tab refresh/revocation behavior is defined.

## GAP-016 — Authentication abuse limits are not distributed or identity-aware

**Priority:** P2

**Type:** Reliability / Security

**Location:** `backend/src/Platform.Api/DependencyInjection.cs:120-160`

**Current State:** In-process fixed-window policies exist, but no distributed partitioning or refresh-token policy is visible.

**Missing:** Per-account/IP limits across replicas, proxy-aware identity, `Retry-After`, and abuse telemetry.

**Why It Matters:** Attackers can distribute attempts across replicas and legitimate users may be unfairly throttled by shared IPs.

**Required Implementation:** Configure a distributed limiter with explicit partitions and tests for login, registration, reset, refresh, and judge submission.

**Acceptance Criteria:** Limits hold across replicas and do not disclose account existence; rejection responses and metrics are consistent.

## GAP-017 — External provider resilience and failure semantics are incomplete

**Priority:** P2

**Type:** Integration / Reliability

**Location:** `backend/src/Platform.Infrastructure/Judge/JudgeHttpClient.cs`, `backend/src/Platform.Infrastructure/Payments/Paymob/PaymobClient.cs:118-132`, infrastructure HTTP registrations

**Current State:** Timeouts exist for the judge client, but bounded resilience policies and provider-specific state recovery are not demonstrated. Paymob verification swallows exceptions and returns false.

**Missing:** Transient/permanent classification, circuit breakers, idempotency-aware retries, durable reconciliation, and clear user-visible pending states.

**Why It Matters:** Outages become ambiguous failed payments/submissions or long request stalls.

**Required Implementation:** Add bounded policies per provider, never retry non-idempotent payment creation blindly, persist pending states, and reconcile asynchronously.

**Acceptance Criteria:** Outage/timeout/recovery tests prove bounded latency and correct state transitions without duplicate charges or submissions.

## GAP-018 — Upload safety and ownership validation are incomplete

**Priority:** P2

**Type:** Partial Implementation / Security

**Location:** `backend/src/Platform.Infrastructure/Storage/CloudflareR2/CloudflareR2Storage.cs:43-57`, avatar handlers/controllers

**Current State:** Presigned PUT URLs are generated from caller-provided key/content type. The storage adapter does not enforce file size, content inspection, key ownership, or post-upload verification.

**Missing:** Server-owned object keys, MIME/magic-byte validation, size limits enforced at storage, malware scanning policy, and orphan cleanup.

**Why It Matters:** Users may upload unwanted content, overwrite another object, or create unreferenced private files.

**Required Implementation:** Generate keys from authenticated user/resource IDs, whitelist types and size, require post-upload metadata verification, persist ownership, and clean orphaned uploads.

**Acceptance Criteria:** Cross-user key attacks fail; oversized/incorrect files cannot be accepted; private objects require authorized signed URLs; cleanup is tested.

## GAP-019 — Entity ownership/concurrency and workflow invariants are not comprehensively enforced

**Priority:** P2

**Type:** Persistence / Domain

**Location:** `backend/src/Platform.Application/Features/Assessment/Commands/ExamAttemptHandlers.cs`, live-session handlers, payment handlers, EF model/migrations

**Current State:** Several handlers rely on read-then-write checks without an explicit concurrency token or all required unique constraints; exam start, answer submission, attendance, and webhook operations can race.

**Missing:** Optimistic concurrency/version columns where needed, unique constraints for idempotent operations, transaction boundaries, and database-backed race tests.

**Why It Matters:** Duplicate attempts/attendance/answers or lost updates can corrupt learner progress and commerce state.

**Required Implementation:** Define aggregate invariants, add concurrency/unique constraints and migrations, use transactions, and translate conflicts into safe API responses.

**Acceptance Criteria:** Concurrent test suites prove one valid state transition per idempotency key and no lost updates; migrations apply cleanly to existing data.

---

# Low Priority Missing Implementations

## GAP-020 — Release documentation and package/deployment model are stale

**Priority:** P3

**Type:** Documentation / Tooling

**Location:** `README.md`, `frontend/package.json`, `infra/docker/*`, `docs/*`

**Current State:** README ports and some framework claims are stale; docs describe planned integrations/tests as if present; pnpm settings generate warnings; deployment manifests contain mutable tags/placeholders.

**Missing:** One authoritative setup/deploy/runbook set and clean package-manager configuration.

**Why It Matters:** Operators and new developers can follow instructions that do not run or misread planned features as complete.

**Required Implementation:** Update docs to active Vite/.NET commands and actual ports, move pnpm settings to supported config, remove stale claims, pin image versions, and add backup/rollback/troubleshooting runbooks.

**Affected Components:** README, docs, package configuration, CI, manifests.

**Acceptance Criteria:** A new developer can build/run the stack from a clean checkout; operators can deploy/rollback/restore using documented commands with no placeholders.

---

# Feature Completion Matrix

| Feature | Entry Point | Business Logic | Persistence | Validation | Authorization | Integration | Tests | Status |
| ------- | ----------- | -------------- | ----------- | ---------- | ------------- | ----------- | ----- | ------ |
| Authentication | AuthController | Present | Present | Present | Partial | Email provider partial | Unit only | Partially Complete |
| User/admin management | UsersController/Admin app | Present | Present | Present | Present in API | Admin UI partial | Unit/controller only | Partially Complete |
| Course catalog/content | Courses/Modules/Lessons controllers | Present | Present | Present | Present in API | Public web fixture-only | Unit only | Partially Complete |
| Student progress | StudentProgressController | Present | Present | Present | Partial resource checks | Public web not wired | Unit only | Partially Complete |
| Exams/homework | Exams/Attempts/Homeworks controllers | Partial | Present | Present | Attempt ownership gaps | Public web fixture-only | Unit only | Partially Complete |
| Coding judge | CodingChallengesController/Judge service | Present | Present | Present | Partial | Judge provider present; resilience incomplete | Unit only | Partially Complete |
| Commerce/payments | Subscriptions/Paymob controllers | Partial | Payment/invoice present; entitlement path incomplete | Partial | Webhook auth present; reconciliation incomplete | Unit/handler only | Broken |
| Notifications | NotificationsController/outbox | In-app present | Present | Present | Partial | Push/SMS/WhatsApp no-op; email failure swallowed | Unit only | Partially Complete |
| Live sessions | LiveSessionsController | Present | Present | Present | Resource/permission gaps | Create present; delete/recordings placeholder | Unit only | Partially Complete |
| Search/storage | Infrastructure adapters | Partial | Search index/storage interfaces present | Upload safety partial | Ownership partial | ES/R2 clients present | No integration | Partially Complete |
| Public web app | Vite routes/pages | Fixture UI | None for most screens | Browser-only | No real auth guard | API package unused | No E2E | Broken |
| Admin console | Vite routes/Admin API | Teacher create only | Teacher create | Basic form validation | Login role check | Most screens fixture-only | No E2E | Partially Complete |
| Deployment/operations | CI/Docker/Helm/K8s | Partial | Migration startup unsafe | No config fail-fast | Network/auth intent | Image/project mismatch | No deploy test | Broken |

---

# File-Level Implementation Gaps

| File | Problem | Required Work | Priority |
| ---- | ------- | ------------- | -------- |
| `frontend/apps/web/src/pages.tsx` | Fake auth, checkout, judge, assessment, live, profile, teacher flows | Replace fixtures and local success branches with typed API workflows | P0 |
| `frontend/apps/admin/src/App.tsx` | Fabricated dashboard/users/courses/subscriptions/audit data; dev credentials in UI | Wire queries/mutations and remove credential hints | P1/P0 |
| `backend/src/Platform.Infrastructure/Caching/Hybrid/HybridCacheService.cs` | `GetAsync` discards cache result | Implement correct read and tests | P1 |
| `backend/src/Platform.Application/Features/Commerce/Commands/PaymobWebhookHandlers.cs` | Subscription repository unused in success path; weak reconciliation | Complete atomic entitlement workflow | P1 |
| `backend/src/Platform.Infrastructure/Messaging/OutboxProcessor.cs` | No atomic row claim/lease | Add concurrency-safe claiming and idempotency | P1 |
| `backend/src/Platform.Application/Features/Assessment/Commands/ExamAttemptHandlers.cs` | Get-attempt lacks owner check | Add resource authorization and tests | P1 |
| `backend/src/Platform.Application/Features/LiveSessions/Commands/LiveSessionHandlers.cs` | Attendance roster authorization incomplete; deletion fire-and-forget | Add owner checks and durable provider cleanup | P1 |
| `backend/src/Platform.Infrastructure/Notifications/Email/SmtpEmailSender.cs` | Push/SMS/WhatsApp are no-op; email errors swallowed | Implement providers or explicit disabled behavior and delivery tracking | P1 |
| `backend/src/Platform.Infrastructure/LiveSessions/GoogleMeet/GoogleMeetProvider.cs` | Delete always true; recording always null | Implement provider operations or remove claims | P1 |
| `backend/src/Platform.Infrastructure/LiveSessions/MicrosoftTeams/MicrosoftTeamsProvider.cs` | Delete always true; recording always null | Implement Graph operations or remove claims | P1 |
| `backend/src/Platform.Api/Program.cs` | Migrations run per replica | Move to controlled migration job | P1 |
| `infra/docker/docker-entrypoint.sh` | Migration failure suppressed | Fail closed and remove duplicate migration path | P1 |
| `infra/docker/Dockerfile.runtime` | Copies missing projects | Align with actual solution | P1 |
| `infra/docker/Dockerfile.web` | Builds stale Next.js app | Build active Vite apps or restore intended Next app | P1 |
| `.github/workflows/backend-ci.yml` | Missing integration project path | Add project or correct CI job | P1 |
| `.github/workflows/frontend-ci.yml` | Placeholder test step | Add real frontend tests and fail on regressions | P1 |
| `backend/src/Platform.Api/DependencyInjection.cs` | No strict options validation; broad CORS/rate limits | Add production validation, origin allowlist, distributed abuse controls | P1/P2 |
| `backend/src/Platform.Api/Extensions/HealthCheckExtensions.cs` | Health leaks exception messages | Redact public output and separate diagnostics | P1 |
| `infra/k8s/10-api.yaml` | Scrapes unregistered `/metrics` | Register endpoint and add metrics/alerts | P1 |
| `backend/src/Platform.Infrastructure/Storage/CloudflareR2/CloudflareR2Storage.cs` | Presigned upload trusts caller key/type | Add ownership, size/type, verification, cleanup | P2 |
| `frontend/packages/api/src/client.ts` | Tokens in localStorage | Implement safer refresh-token transport and CSRF | P2 |

---

# Missing Components

* API-backed public web application state/auth provider and protected-route implementation.
* API-backed admin query/mutation hooks for analytics, users, courses, subscriptions, and audit logs.
* Database-backed integration test project referenced by CI.
* Browser E2E and accessibility test suite for both Vite apps.
* Distributed outbox claim/lease and poison-message/dead-letter handling.
* Payment entitlement reconciler and strict webhook contract/concurrency tests.
* Production notification providers for any advertised SMS, WhatsApp, or push channel, or an explicit feature-disabled contract.
* Real Google Meet/Teams deletion and recording adapters, or removal of unsupported API claims.
* Production configuration validator and secret rotation/bootstrap process.
* Registered metrics endpoint, dashboards, alerts, and correlation propagation.
* Upload validation/ownership scanner and orphan cleanup process.
* Backup/restore and migration/rollback jobs/runbooks.

---

# Broken Implementations

* `HybridCacheService.GetAsync`: always returns default because the cache result is discarded. Required fix: return the actual cached value and test hit/miss behavior. **P1**
* `PaymobWebhookHandlers`: marks payment/invoice state but does not visibly activate the injected subscription repository and does not reconcile all callback metadata. Required fix: atomic entitlement transaction and strict matching. **P1**
* `GoogleMeetProvider.DeleteMeetingAsync` and `MicrosoftTeamsProvider.DeleteMeetingAsync`: always return success without remote deletion. Required fix: provider delete or explicit asynchronous reconciliation. **P1**
* `GoogleMeetProvider.GetRecordingUrlAsync` and `MicrosoftTeamsProvider.GetRecordingUrlAsync`: always return null despite the live-session API describing recordings. Required fix: implement recording retrieval/storage or remove the feature. **P1**
* `WebPushSender`, `NoopSmsSender`, and `NoopWhatsAppSender`: report successful completion without delivery. Required fix: real provider or explicit failure/disabled state. **P1**
* `CancelLiveSessionHandler`: fire-and-forgets provider deletion and persists cancellation. Required fix: await/reconcile the external operation. **P1**
* `GetExamAttemptByIdHandler`: returns an attempt after existence check without verifying current-user ownership. Required fix: ownership/admin authorization. **P1**
* `GetSessionAttendanceHandler`: returns roster after authentication but does not verify teacher/session ownership. Required fix: resource authorization. **P1**
* Public web auth/checkout/judge/assessment flows: local navigation/state creates false success. Required fix: API-backed state machines and error handling. **P0**
* Admin dashboard and management screens: hard-coded records and metrics. Required fix: real API queries/mutations. **P1**
* `infra/docker/docker-entrypoint.sh`: ignores migration failure using `|| true`. Required fix: fail closed and centralize migrations. **P1**

---

# Production Requirements Still Missing

Only requirements directly implied by the implemented product and deployment model are listed:

1. Real API-backed public and admin user journeys.
2. Removal and rotation of committed/default secrets.
3. A deployable, internally consistent Docker/CI/frontend build model.
4. Correct cache reads and concurrency-safe outbox/payment/assessment workflows.
5. Resource-level authorization for attempts, attendance, profiles, and mutations.
6. Real or explicitly disabled notification/live-session integrations.
7. Production configuration validation and safe migration execution.
8. Integration, API, security, browser, and failure-path tests for critical workflows.
9. Working metrics/health diagnostics and operational alerts.
10. Upload ownership/validation and payment/entitlement reconciliation.
11. Backup/restore and rollback procedures for the existing PostgreSQL-backed system.

---

# Final Release Gate

## Must Implement Before Release

1. Remove and rotate committed database/JWT/default credentials; enforce production secret validation.
2. Replace fixture-only public web auth, catalog, checkout, assessment, judge, live, profile, and teacher flows with real API integrations.
3. Replace fixture-only admin views with authorized API queries and server-confirmed mutations; remove displayed development credentials.
4. Align Dockerfiles, Compose, CI, Helm, Kubernetes, and active Vite/.NET projects; eliminate missing project references and placeholders.
5. Fix cache reads and make outbox processing concurrency-safe and idempotent.
6. Complete Paymob payment-to-subscription entitlement activation with strict reconciliation and concurrency tests.
7. Enforce resource-level authorization for exam attempts, live attendance, profile access, and all teacher/admin mutations.
8. Implement advertised notification and live-provider operations, or explicitly remove unsupported channels/features from the product contract.
9. Add production options validation, safe migration execution, working metrics, redacted health diagnostics, and alerts.
10. Add database/API/integration/security/browser/failure-path tests and remove placeholder CI test steps.
11. Validate uploads, define backup/restore and rollback procedures, and prove them in staging.

### Release status

The project is **NOT COMPLETE** and is not a release candidate. Passing builds and 242 unit/architecture tests prove compilation and selected business logic only; they do not prove the missing runtime integrations and product workflows listed above.
