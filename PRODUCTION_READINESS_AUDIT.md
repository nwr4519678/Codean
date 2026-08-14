# Production Readiness Audit

## Executive Summary

* Overall readiness score: 35/100
* Current status:

  * NOT READY

The repository has a coherent .NET layered structure, passing backend unit/architecture tests, health-check endpoints, JWT/session primitives, and Kubernetes intent. Those strengths do not compensate for release blockers in secret management and deployment integrity. The active Vite frontend is largely fixture-driven, the production Docker/CI path is inconsistent with the checked-in solution, CORS is unrestricted, and several reliability/observability controls are incomplete or misleading. The score reflects “buildable in the current developer environment,” not “safe and usable for real users.”

### Verification performed

* `dotnet build Platform.slnx --no-restore --maxcpucount:1`: passed, 0 warnings, 0 errors.
* `dotnet test Platform.slnx --no-restore --maxcpucount:1`: passed, 242/242 tests.
* `pnpm --filter @codean/web typecheck` and `build`: passed.
* `pnpm --filter @codean/admin typecheck` and `build`: passed.
* The frontend commands emitted pnpm warnings because `pnpm.peerDependencyRules` is still embedded in `package.json` rather than workspace configuration.
* No production database, external-provider, Kubernetes, Docker image, penetration, dependency-audit, browser E2E, or load-test verification was available in this audit run.

---

## Release Blockers

### [P0-001] Production secrets are committed in application configuration

**Location:** `backend/src/Platform.Api/appsettings.json:3-16`

**Problem:** A PostgreSQL password and the default JWT signing secret are committed in source control. The same JWT secret is duplicated in `Jwt.Secret` and `Jwt.SigningKeys` and defaults also exist in `backend/src/Platform.Infrastructure/Configuration/SecurityOptions.cs:8-20`.

**Impact:** Anyone with repository access can authenticate or mint/validate tokens and access the database if the values are reused. Rotation is not demonstrated, and accidental deployment of the default configuration would compromise every account.

**Required Fix:** Remove all real/default credentials and signing material from tracked configuration. Require production secrets through a managed secret store/environment injection, use strong startup validation that rejects placeholders/defaults, rotate the exposed database and JWT credentials, and audit repository history for prior exposure.

**Acceptance Criteria:** A clean checkout contains no usable database password, JWT secret/private key, provider credential, or default admin credential; production startup fails closed when required secrets are absent or placeholder-like; rotated credentials are verified in staging; repository and CI secret scans pass.

---

## High Priority Issues

### [P1-001] Production deployment artifacts do not match the active solution

**Location:** `infra/docker/Dockerfile.runtime:9-17`, `infra/docker/Dockerfile.web:1-29`, `infra/docker/docker-compose/docker-compose.yml:134-159`, `.github/workflows/backend-ci.yml:98-102`

**Problem:** The runtime Dockerfile copies `Platform.Shared` and `Platform.Worker`, but those projects are not present in `backend/src`; Compose builds `Platform.Worker`. The web Dockerfile builds a Next.js app with npm and `.next`, while the active apps are Vite React apps using pnpm and `dist`. CI also references the missing `Platform.Infrastructure.IntegrationTests` project.

**Impact:** The documented container deployment and CI integration job cannot reliably produce the artifacts described by the repository. A release can pass local builds while failing in image build, integration CI, or startup.

**Required Fix:** Choose the actual deployment model and align Dockerfiles, Compose, CI, Helm, and Kubernetes with the checked-in projects and package manager. Add the integration project or remove/fix the CI job. Build immutable SHA-tagged images and validate them in CI.

**Acceptance Criteria:** `docker build` succeeds for API, worker/judge (or the explicitly supported services), and both Vite apps; Compose starts the documented stack; every CI project path exists; a clean CI run completes without placeholders.

### [P1-002] The public web product is not wired to backend behavior

**Location:** `frontend/apps/web/src/pages.tsx:116-120,127,165,168-181,193,209,230`

**Problem:** Login only navigates based on a selected role, checkout only navigates to success, code execution writes a hard-coded “Accepted” result, and major course, assessment, live-session, profile, and settings screens use local fixtures/state. The API package exists but is not used by the active public app.

**Impact:** Real users can see fabricated data, bypass payment/submission semantics in the UI, lose changes on refresh, and believe operations succeeded when no backend mutation occurred. This makes the public product unusable for its advertised core workflows.

**Required Fix:** Integrate the active web app with the typed API client, implement authenticated route guards and error/loading/empty states, and verify every mutation against backend responses. Remove hard-coded success states and fixture data from production paths.

**Acceptance Criteria:** Browser E2E tests prove registration/login, catalog load, enrollment, progress, checkout redirect, code submission/status, teacher workflows, and logout against a deployed API; refresh preserves only server-confirmed state; failed API calls are visible and safe.

### [P1-003] Unrestricted CORS exposes the API to arbitrary origins

**Location:** `backend/src/Platform.Api/DependencyInjection.cs:33-42`, `backend/src/Platform.Api/Program.cs:34`

**Problem:** The only policy is named `AllowAll` and permits any origin, header, and method.

**Impact:** Any website can issue cross-origin requests to the API. The current bearer-token design reduces cookie-CSRF risk, but broad cross-origin exposure increases token misuse/XSS blast radius and makes future credentialed flows unsafe.

**Required Fix:** Bind an explicit allowlist of production web/admin origins per environment, reject wildcard origins in production, and add tests for allowed and denied origins.

**Acceptance Criteria:** Production CORS allows only the approved HTTPS web and admin origins; unknown origins receive no CORS headers; staging and local origins are configured without source edits.

### [P1-004] Automatic migrations run inside every API startup

**Location:** `backend/src/Platform.Api/Program.cs:43-55`

**Problem:** Every API replica calls `MigrateAsync()` during startup. Kubernetes runs multiple replicas and rolling updates, while the entrypoint also has a second migration mechanism that suppresses errors with `|| true` (`infra/docker/docker-entrypoint.sh:6-9`).

**Impact:** Deployments can race on schema changes, hold readiness hostage, or run an incompatible migration during a rolling release. Suppressed migration errors can let an application start against an invalid schema.

**Required Fix:** Move migrations to a single, audited release job/init step with locking, backup/rollback policy, and compatibility checks. Never suppress migration errors; make readiness reflect schema compatibility.

**Acceptance Criteria:** Two concurrent application replicas never independently migrate; failed migration blocks rollout and is observable; rollback and expand/contract migration procedures are tested in staging.

### [P1-005] Cache reads always return the default value

**Location:** `backend/src/Platform.Infrastructure/Caching/Hybrid/HybridCacheService.cs:29-39`

**Problem:** `GetAsync` initializes `result` to default, invokes `GetOrCreateAsync` without assigning its return value, then returns the unchanged default.

**Impact:** Cache hits are discarded, so read paths repeatedly query the database and any behavior relying on cached values can be incorrect. At scale this increases latency, database load, and the likelihood of outage amplification.

**Required Fix:** Implement a real cache read or assign/return the cache operation result using a supported sentinel strategy. Add hit/miss and invalidation tests.

**Acceptance Criteria:** A set-then-get test returns the stored value across the intended L1/L2 path; missing keys return null; cache failures have a deliberate fallback policy; load tests show the expected reduction in database reads.

### [P1-006] Outbox processing is unsafe with multiple workers/replicas

**Location:** `backend/src/Platform.Infrastructure/Messaging/OutboxProcessor.cs:30-63`

**Problem:** Pending rows are selected without claim/lease/row locking, then dispatched before being marked processed. Multiple API replicas or overlapping jobs can select and dispatch the same message.

**Impact:** At-least-once delivery becomes uncontrolled duplicate delivery. Email, notifications, external calls, or other non-idempotent handlers can execute multiple times.

**Required Fix:** Add an atomic claim/lease (`FOR UPDATE SKIP LOCKED` or equivalent), handler-level idempotency keys, bounded exponential backoff, dead-letter visibility, and metrics/alerts for poison messages.

**Acceptance Criteria:** Concurrent processors cannot claim the same message; retries survive process termination; duplicate delivery is harmless for every handler; exhausted messages are visible and actionable.

### [P1-007] Production configuration is not validated and has unsafe fallback defaults

**Location:** `backend/src/Platform.Infrastructure/Configuration/SecurityOptions.cs:8-22`, `backend/src/Platform.Api/DependencyInjection.cs:54-66`

**Problem:** Options are bound without `ValidateOnStart`; JWT code silently accepts fallback/default values, and several external integrations default to blank or localhost-style settings.

**Impact:** A misconfigured production deployment can start and fail later on authentication, payments, email, storage, or judge operations. This is difficult to diagnose and may create insecure partial functionality.

**Required Fix:** Add environment-aware options validation for JWT, database, Redis, SMTP, R2, Paymob, judge, and provider integrations. Reject placeholders, wildcard origins, weak keys, and missing production endpoints at startup.

**Acceptance Criteria:** A production configuration test fails startup for every missing/placeholder required secret and endpoint; development-only defaults are unavailable in Production; staging startup logs a redacted configuration summary.

### [P1-008] Admin UI exposes development credentials and most admin data is fabricated

**Location:** `frontend/apps/admin/src/App.tsx:120,142-184`

**Problem:** The login screen renders `admin@platform.com / AdminPassword123!`, while dashboard, users, courses, subscriptions, and audit-log views use hard-coded values. Only teacher creation calls the API.

**Impact:** Credentials can be copied by users or screenshots, and administrators may make decisions from stale/fake data. The console is not an operational control plane.

**Required Fix:** Remove credential hints from all builds, wire every admin view to authorized APIs, add pagination/error states, and use server-confirmed mutations.

**Acceptance Criteria:** Production bundles contain no development credentials; all displayed records come from API responses; unauthorized and failed requests are handled; admin E2E tests cover the core console workflows.

### [P1-009] Health and metrics contracts are incomplete and leak provider errors

**Location:** `backend/src/Platform.Api/Extensions/HealthCheckExtensions.cs:43-73`, `infra/k8s/10-api.yaml:20-23`

**Problem:** Kubernetes scrapes `/metrics`, but no metrics endpoint registration was found. The full health endpoint serializes exception messages, and the readiness set omits database migration/schema compatibility and several critical external dependencies.

**Impact:** Monitoring can report false health or no data, while unauthenticated health responses may disclose infrastructure/provider details. Operators lack reliable alert signals during incidents.

**Required Fix:** Register and protect an actual metrics endpoint, redact exception details from public health output, separate internal diagnostics from public liveness/readiness, and define alert thresholds/SLOs.

**Acceptance Criteria:** Prometheus scrape succeeds with useful request/job/database metrics; public health contains no exception/secret text; readiness fails for required dependencies; alerts fire in staging failure drills.

### [P1-010] Payment activation is not demonstrably transactionally complete

**Location:** `backend/src/Platform.Application/Features/Commerce/Commands/PaymobWebhookHandlers.cs:66-107`

**Problem:** The handler verifies the provider and marks the payment paid, but the shown success path creates an invoice without adding/updating the injected subscription repository. It also does not visibly compare the webhook amount/order/currency against the stored payment before activation.

**Impact:** A paid webhook can leave the customer without entitlement, and mismatched payment metadata could activate the wrong amount/order if provider or integration assumptions fail.

**Required Fix:** Atomically reconcile payment identity, amount, currency, merchant order, and subscription entitlement under a database transaction with unique constraints and concurrency handling.

**Acceptance Criteria:** Successful webhook produces exactly one paid payment, invoice, and active entitlement; duplicate/concurrent webhooks are safe; mismatched amount/order/currency are rejected and audited.

---

## Medium Priority Issues

### [P2-001] Frontend token storage is XSS-sensitive

**Location:** `frontend/apps/admin/src/App.tsx:35-86`, `frontend/packages/api/src/client.ts:5-43`

**Problem:** Access and refresh tokens are stored in `localStorage`.

**Impact:** Any successful XSS can exfiltrate long-lived refresh credentials. This is a material defense-in-depth weakness for an education/admin product.

**Required Fix:** Prefer secure, HttpOnly, SameSite refresh cookies with CSRF protection and short-lived access tokens; harden CSP and add XSS/security tests.

**Acceptance Criteria:** Refresh tokens are not readable by JavaScript, CSRF is tested, token rotation/revocation works across tabs/devices, and CSP is compatible with the deployed frontend.

### [P2-002] Authentication rate limits are fixed-window and not clearly distributed

**Location:** `backend/src/Platform.Api/DependencyInjection.cs:120-160`

**Problem:** Sensitive policies are in-process fixed windows with no visible partitioning by IP/account and no distributed limiter configuration.

**Impact:** Attackers can distribute brute-force attempts across replicas or avoid useful per-account controls; legitimate shared-IP users can be unfairly throttled.

**Required Fix:** Use distributed, identity-aware limits with account lockout telemetry, `Retry-After`, proxy-aware client identity, and abuse monitoring.

**Acceptance Criteria:** Limits hold across replicas and are tested for account/IP dimensions without account enumeration.

### [P2-003] External client resilience is incomplete

**Location:** `backend/src/Platform.Infrastructure/DependencyInjection.cs:93-108`, `backend/src/Platform.Infrastructure/Payments/Paymob/PaymobClient.cs:118-132`

**Problem:** Judge and provider clients have timeouts but no visible circuit breaker/health-aware retry policy; Paymob verification swallows all exceptions and returns false.

**Impact:** Provider outages can cause slow request failure, noisy retries elsewhere, or ambiguous payment states.

**Required Fix:** Add bounded, idempotency-aware resilience policies and explicit transient/permanent error classification. Never retry non-idempotent payment creation blindly.

**Acceptance Criteria:** Timeout, outage, and recovery tests demonstrate bounded latency and correct payment/judge state transitions.

### [P2-004] Integration, security, browser E2E, and load coverage is missing

**Location:** `backend/tests/` and `.github/workflows/frontend-ci.yml:34-35`

**Problem:** The checked-in test tree contains unit/architecture tests but no integration-test project; frontend CI explicitly runs `echo "no tests yet"`.

**Impact:** Endpoint routing, EF mappings, authorization, migrations, CORS, browser workflows, payment webhooks, and failure paths are not protected by automated tests.

**Required Fix:** Add database-backed API tests, authorization/security tests, provider contract tests, browser E2E tests, and representative load/failure tests. Make CI fail on regressions.

**Acceptance Criteria:** Critical user journeys and failure scenarios run in CI against disposable dependencies with coverage thresholds and no placeholder test step.

### [P2-005] Deployment uses mutable/latest placeholders and lacks recovery evidence

**Location:** `infra/helm/platform/values.yaml:4-7`, `infra/k8s/10-api.yaml:34`, `infra/k8s/30-ingress.yaml:11-18`, `infra/helm/platform/values-prod.yaml:6,35-36`

**Problem:** Manifests contain `latest`, `REPLACE_ME`, placeholder AWS ARNs, and no checked-in backup/restore job or rollback runbook.

**Impact:** Releases are not reproducible and production can deploy the wrong image or fail to provision ingress/security controls. Recovery readiness is unverified.

**Required Fix:** Require immutable digests, environment-specific rendered-manifest validation, backup retention/RPO/RTO definitions, restore drills, and a tested rollback process.

**Acceptance Criteria:** A release artifact is reproducible from a commit SHA; rendered production manifests contain no placeholders; backup restore and rollback are exercised and recorded.

---

## Low Priority Improvements

* [P3-001] Update README ports and commands to match the current launch profile (`README.md:299-320` vs repository launch settings).
* [P3-002] Move pnpm peer-dependency settings out of `frontend/package.json` to the supported workspace configuration to remove CI warnings.
* [P3-003] Add explicit request correlation IDs and propagate trace context through background jobs and provider calls.
* [P3-004] Add pagination/search debouncing, route-level error boundaries, accessible focus handling, and loading/empty states to active Vite apps.
* [P3-005] Pin all container image versions, including development Compose images, and publish SBOM/vulnerability policy results as release gates.
* [P3-006] Remove stale documentation that describes Next.js, nonexistent projects, or planned integrations as implemented.

---

## Security Audit

| Area | Status | Findings |
| ----------------- | ------ | -------- |
| Authentication | At risk | JWT/session primitives exist, but committed signing secrets, unsafe defaults, localStorage tokens, and public development credentials require remediation. |
| Authorization | Partially implemented | Permission attributes and architecture tests exist; broad endpoint/E2E authorization coverage is missing. |
| Secrets | Failed | Database and JWT credentials are committed; Compose includes weak test credentials and an embedded RSA private key. |
| Input Validation | Partial | FluentValidation is present, but API-wide limits, metadata reconciliation, upload/security tests, and frontend integration are incomplete. |
| API Security | Failed for release | CORS allows every origin; distributed abuse controls and complete endpoint tests are absent. |
| Database Security | At risk | PostgreSQL credentials are committed; migration startup races and backup/restore evidence are missing. |
| Dependencies | Unknown | Trivy is configured in CI, but no scan result was available from this audit run and no release threshold is enforced. |
| Network Security | Partial | Kubernetes network policies and HTTPS ingress intent exist; local Compose exposes databases and OpenSearch without production-safe boundaries. |
| Logging | Partial | Serilog and exception logging exist; correlation, audit completeness, redaction, and provider error policy require verification. |
| File Handling | Unknown | R2 abstraction exists, but upload size/type/content scanning and abuse tests were not demonstrated. |

---

## Architecture Audit

The backend separates API, application, domain, infrastructure, and judge concerns. CQRS handlers, validators, permission policies, repositories, outbox messaging, health checks, and provider adapters are reasonable foundations. The passing architecture tests support that dependency direction at a basic level.

Risks are operational rather than a need for wholesale rewrite: the API process owns migrations and Hangfire scheduling while multiple replicas are expected; outbox claiming is not concurrency-safe; the cache abstraction is incorrect; provider failures are collapsed into generic results; and the active frontend is disconnected from the application contracts. The repository also contains two competing deployment/application models (the checked-in Vite apps versus stale Next.js Docker/CI assumptions). The recommended architecture is to retain the current layers, establish one authoritative runtime model, isolate release migrations, and make cross-process work idempotent and observable.

---

## Database Audit

* PostgreSQL and EF Core migration wiring exist, but migrations run on every API startup and are duplicated by the container entrypoint.
* The outbox query lacks atomic claiming/leases, so duplicate processing is possible with multiple replicas.
* Payment webhook processing needs database-enforced uniqueness and a transaction covering payment, entitlement, and invoice state.
* No production backup, restore, retention, RPO, or RTO evidence is present in the executable deployment configuration.
* Index/query review at scale was not demonstrated; the cache defect currently defeats intended read reduction.
* Timestamps use UTC in many handlers, but a complete timezone/business-calendar policy was not verified.

---

## API Audit

Problematic or incomplete API areas:

* All controllers inherit authentication/permission patterns, but the active public web app does not call them, so API behavior is not represented in the user experience.
* `POST /api/webhooks/paymob` is anonymous by necessity and uses HMAC, but payment/order/amount/currency reconciliation and concurrent idempotency need stronger guarantees.
* `GET /health` returns component exception messages and `/metrics` is referenced by Kubernetes but not registered in the inspected API.
* Sensitive auth endpoints have local fixed-window policies; refresh-token protection is not visibly rate-limited.
* Pagination exists for several query DTOs, but endpoint-wide maximum page size, sorting/filter validation, and integration assertions were not verified.
* Judge HTTP calls catch failures and return synthetic failed/null responses; state-machine and retry semantics need explicit contract tests.

---

## Testing Gap Analysis

| Area | Existing Coverage | Missing Coverage | Priority |
| ---- | ----------------- | ---------------- | -------- |
| Domain | 26 unit tests | Broader invariants and persistence constraints | P2 |
| Application | 183 unit/handler/validator tests | Real EF transactions, concurrency, provider failures | P1 |
| API | 11 controller tests | Full HTTP pipeline, auth matrix, CORS, health, rate limits | P1 |
| Architecture | 3 tests | Runtime/deployment consistency checks | P2 |
| Judge | 19 unit tests | Service integration, resource abuse, timeout/retry tests | P1 |
| Database | No checked-in integration project | Migrations, indexes, constraints, backup/restore, concurrency | P1 |
| Payments | Handler tests exist | Signed webhook contract, duplicate/mismatch/provider outage tests | P1 |
| Frontend | Typecheck/build only | Unit, browser E2E, accessibility, API failure and auth flows | P1 |
| Performance | Plans/docs only | Baseline/load/soak results and cache effectiveness | P2 |
| Security | Static scan configured in CI | Actual scan results, DAST, dependency gate, penetration test | P1 |

---

## DevOps / Deployment Audit

* Build: backend and active Vite builds pass locally; Docker build was not successful/verified and contains stale project assumptions.
* Tests: backend unit suite passes; integration CI references a missing project and frontend tests are a placeholder.
* CI: Trivy/SBOM intent exists, but scans are not a release gate and there is no complete deploy/rollback workflow.
* CD: Helm/Kubernetes manifests contain mutable tags and placeholders; no verified promotion process exists.
* Docker: API runtime is non-root, but the web image targets the wrong framework and migration errors are suppressed.
* Secrets: AWS External Secrets intent exists in Kubernetes, but source defaults and Compose credentials remain unsafe.
* Configuration: no fail-fast production validation was found.
* Health checks: live/ready endpoints exist, but metrics and redaction/readiness scope are incomplete.
* Monitoring/logging: structured logging exists; metrics, traces, alerts, and correlation evidence are incomplete.
* Backups/rollback/disaster recovery: no executable backup/restore or tested runbook was found.

---

## Observability Audit

Missing or insufficient controls include a confirmed Prometheus metrics endpoint, request/job/provider latency and error metrics, correlation IDs across API/outbox/Hangfire/provider calls, distributed traces despite OTel packages being present, alerts tied to SLOs, dead-letter/poison outbox alerts, cache hit/miss metrics, and safe health diagnostics. Full health currently includes raw exception messages and should be treated as internal-only.

---

## Documentation Audit

Before release, update the README to current Vite apps, ports, pnpm commands, and actual deployment flow. Add an environment variable/secret inventory, staging and production deployment guide, migration/rollback policy, backup/restore runbook, observability and alert runbook, incident response/security contact, API authentication and webhook contract documentation, and a clear distinction between imported templates, fixtures, and production features. Remove claims that tests, metrics, E2E, or integrations exist when they are only plans.

## Final Production Checklist

### Security
- [ ] No critical security vulnerabilities
- [ ] Authentication hardened
- [ ] Authorization verified
- [ ] Secrets removed from source
- [ ] HTTPS enforced
- [ ] Security headers configured
- [ ] Rate limiting configured
- [ ] Input validation complete

### Reliability
- [ ] Critical failures handled
- [ ] Timeouts configured
- [ ] External failures handled
- [ ] Graceful shutdown implemented
- [ ] Database failure behavior verified

### Database
- [ ] Migrations production-safe
- [ ] Indexes reviewed
- [ ] Transactions reviewed
- [ ] Data integrity verified
- [ ] Backup strategy defined

### Performance
- [ ] Critical queries optimized
- [ ] N+1 problems eliminated
- [ ] Pagination implemented where required
- [ ] Memory usage reviewed
- [ ] Concurrency reviewed

### Testing
- [ ] Unit tests
- [ ] Integration tests
- [ ] API tests
- [ ] Security tests
- [ ] Failure-path tests
- [ ] End-to-end tests

### Observability
- [ ] Structured logging
- [ ] Correlation IDs
- [ ] Health checks
- [ ] Metrics
- [ ] Error monitoring

### Deployment
- [ ] Production configuration
- [ ] CI/CD
- [ ] Docker/container validation
- [ ] Secrets management
- [ ] Rollback strategy
- [ ] Backup/recovery strategy

### Documentation
- [ ] README
- [ ] Setup guide
- [ ] Deployment guide
- [ ] Environment configuration
- [ ] API documentation
- [ ] Troubleshooting guide

## Release Decision

### ❌ NOT READY

There are P0/P1 issues that must be fixed before production.

**Remaining blockers:** 1

**P1 issues:** 10

**P2 issues:** 5

**P3 issues:** 6

**Recommended next action:** Rotate the committed database/JWT credentials and remove them from configuration/history, then make the deployment model internally consistent and wire the active web/admin workflows to real APIs. Re-run Docker/CI, integration, browser E2E, security, and staging failure tests before reassessing the release gate.
