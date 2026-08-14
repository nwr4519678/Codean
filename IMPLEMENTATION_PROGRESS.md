# Implementation Progress

Updated 2026-08-14 after the production-readiness implementation pass.

| GAP | Status | Evidence / remaining work |
|-----|--------|---------------------------|
| GAP-001 | IN PROGRESS | Web auth now uses `@platform/api`, workspace routes require a valid API session, and teacher course CRUD/curriculum screens now use real course/module/lesson endpoints. Student catalog/dashboard/learning pages still contain fixtures. |
| GAP-002 | IMPLEMENTED | Production config fails closed for JWT/database/CORS; committed secrets removed from appsettings and local compose RSA material. |
| GAP-003 | IN PROGRESS | Admin login, overview analytics, and users list now use real API endpoints with isolated admin token storage; courses/subscriptions/audit screens still need API-backed data. |
| GAP-004 | IMPLEMENTED | Render Blueprint, Vercel SPA configs, Vite Docker image, corrected .NET runtime image, and current pnpm CI are checked in. Docker image build awaits Docker Desktop. |
| GAP-005 | IMPLEMENTED | Hybrid cache read result is returned and cache miss behavior is preserved. |
| GAP-006 | IMPLEMENTED | Paymob webhook verifies HMAC/provider state, validates order/amount/currency, is idempotent, creates subscription/invoice transactionally, and has passing commerce tests. |
| GAP-007 | IN PROGRESS | Outbox claims, expiry, worker identity, and migration are implemented; a multi-worker integration race test still requires PostgreSQL. |
| GAP-008 | IMPLEMENTED | Live-session permission is separated from announcements; exam-attempt and attendance ownership checks are enforced. |
| GAP-009 | BLOCKED | Email is configurable; SMS/WhatsApp adapters remain explicit no-op implementations and require selected production providers/credentials. |
| GAP-010 | IN PROGRESS | Live-session ownership and awaited provider cleanup are fixed; provider recording/meeting flows still need end-to-end Google/Teams credentials. |
| GAP-011 | IMPLEMENTED | JWT startup validation and fail-closed signing-key behavior are in place. |
| GAP-012 | IMPLEMENTED | Production startup no longer auto-migrates; Render deployment documentation specifies an explicit migration release step. |
| GAP-013 | IMPLEMENTED | Correlation IDs, redacted health details, liveness/readiness endpoints, and a minimal Prometheus-compatible `/metrics` endpoint are implemented. |
| GAP-014 | IN PROGRESS | Backend suite is green at 242/242; both frontend typecheck/builds are green; browser/API integration coverage is still outstanding. |
| GAP-015 | IN PROGRESS | Role selection is removed and API login now resolves the role returned for the email. Supabase environment documentation is added; OAuth token exchange/account linking and HttpOnly cookie transport still require coordinated backend work. |
| GAP-016 | IN PROGRESS | Redis-backed infrastructure exists; distributed rate-limit production validation remains outstanding. |
| GAP-017 | IN PROGRESS | Existing provider clients and retry paths remain; full failure-injection verification is outstanding. |
| GAP-018 | IN PROGRESS | Upload/storage integration is present; production object-storage policy and ownership tests remain outstanding. |
| GAP-019 | IN PROGRESS | Payment transaction and outbox claiming safeguards are implemented; database-backed race/constraint verification remains outstanding. |
| GAP-020 | IMPLEMENTED | Render/Vercel/Supabase deployment runbook, corrected workspace CI, Compose config, and environment conventions are checked in. |

## Verification record

- `dotnet test backend/Platform.slnx --no-restore --maxcpucount:1`: 242 passed.
- `pnpm --filter @codean/web typecheck`: passed.
- `pnpm --filter @codean/web build`: passed.
- `pnpm --filter @codean/admin typecheck`: passed.
- `pnpm --filter @codean/admin build`: passed.
- `docker compose -f infra/docker-compose/docker-compose.yml config --quiet`: passed.
- Docker image build checks were attempted but Docker Desktop was not running on the workstation.
