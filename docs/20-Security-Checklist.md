# 20 — Security Checklist

**Last updated:** 2026-08-03

> Audit-grade checklist covering OWASP Top 10, GDPR/PDPL, PCI-DSS scope, and platform-specific threats.

## 1. OWASP Top 10 (2021)

| # | Risk | Status | Mitigation |
|---|---|---|---|
| A01 | Broken Access Control | ✅ | Policy-based AuthZ + scoped roles + ownership checks |
| A02 | Cryptographic Failures | ✅ | TLS 1.2+ in transit, R2 SSE, JWT RS256, BCrypt cost 12 |
| A03 | Injection | ✅ | EF Core parameterized queries; raw SQL only with `FromSqlInterpolated`; FTS via ES |
| A04 | Insecure Design | ✅ | Threat modelling per feature; security review in PR template |
| A05 | Security Misconfiguration | ✅ | Hardened base images, secret manager, k8s PSP, no default creds |
| A06 | Vulnerable Components | ✅ | Dependabot + Snyk, central package versions, weekly scans |
| A07 | Identification & Auth Failures | ✅ | JWT + refresh + 2FA + lockout + audit log + session mgmt |
| A08 | Software & Data Integrity | ✅ | Cosign image signing, SLSA provenance, integrity check on paymob webhooks |
| A09 | Security Logging Failures | ✅ | Serilog → OpenSearch, structured fields, audit_logs for privileged actions |
| A10 | Server-Side Request Forgery | ✅ | No SSRF surface; outbound HTTP only to allowlisted providers (Paymob, Google, MS) |

## 2. Headers

- `Strict-Transport-Security: max-age=31536000; includeSubDomains; preload`
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Permissions-Policy: camera=(), microphone=(), geolocation=()`
- `Content-Security-Policy: default-src 'self'; script-src 'self' 'nonce-{n}'; style-src 'self' 'unsafe-inline'; img-src 'self' https://*.r2.cloudflarestorage.com data:; connect-src 'self' https://api.platform.app wss://api.platform.app; font-src 'self'; frame-ancestors 'none'`
- `Cross-Origin-Opener-Policy: same-origin`
- `Cross-Origin-Resource-Policy: same-origin`

## 3. Secrets

- **Never** in source control, **never** in logs.
- Stored in **AWS Secrets Manager** for prod.
- Mounted into k8s pods via External Secrets Operator.
- JWT signing keys generated per environment, rotated every 90 days.

## 4. AuthN/AuthZ

- BCrypt cost 12 (work factor 12) — ~250 ms / hash.
- JWT RS256, 15 min TTL, 30 d refresh tokens.
- Refresh-token rotation; reuse of revoked token revokes the entire user session chain.
- TOTP 2FA with 10 single-use backup codes.
- Account lockout after 5 failed logins in 15 min.
- Per-endpoint + per-action authorization via `[Authorize(Policy="...")]` and MediatR behaviour.

## 5. Webhooks

- All inbound webhooks verify an HMAC SHA-256 signature.
- Constant-time comparison (`CryptographicOperations.FixedTimeEquals`).
- Idempotency via `provider_event_id` (unique index).
- Replay window: 5 min (older webhooks dropped).

## 6. Rate limits

See [`08-API-Specification.md`](./08-API-Specification.md) §19.

## 7. PII

- Email hashed (HMAC) for analytics joins.
- Free-text fields (bio, comments) length-limited and HTML-stripped on output.
- PII export: `GET /users/me/export` returns a JSON archive.
- PII erase: `DELETE /users/me` (GDPR right to be forgotten) — soft-delete + hard-purge of PII fields, retained financial records per PCI-DSS / tax law.

## 8. Live sessions

- Meeting URL **never** returned to students.
- Join URL contains a single-use, short-lived token (15 min).
- Join attempts (allowed + denied) logged with IP, UA.
- Per-user join rate limit.

## 9. Code runner

- Each submission runs in a fresh **gVisor / Docker rootless** container.
- `seccomp` profile blocks dangerous syscalls.
- Network: `network_mode: none` by default.
- CPU/memory/time limits per problem.
- Dropped Linux capabilities.
- Read-only root filesystem; tmpfs for /tmp.

## 10. Dependency hygiene

- **Central Package Management** — single source of truth for versions.
- Renovate or Dependabot auto-creates PRs on new advisories.
- Snyk scan on every PR.
- Block on `critical` / `high` advisories.

## 11. Audit log

Every privileged action emits an `audit_logs` row:
- `actor_user_id` (nullable for system)
- `action` (e.g. `payments.refund.processed`)
- `subject_type`, `subject_id`
- `metadata` (jsonb)
- `ip`, `user_agent`
- `created_at`

Retention: 365 days hot, 2 years cold in S3.

## 12. Penetration testing

- Annual third-party pen test (mandatory for production launch).
- Quarterly internal red-team.
- Bug bounty programme (v1.5) via HackerOne / Bugcrowd.

## 13. Compliance

- **GDPR** — data processing agreement, DPO appointed, sub-processor list published.
- **PDPL (Egypt)** — data residency in MEA, consent banner, breach notification within 72 h.
- **PCI-DSS** — out of scope (SAQ-A) thanks to Paymob hosted checkout; no PAN/CVV ever touches our servers.
- **COPPA-style** — under-16 accounts have public profiles disabled; no behavioural ads; parental consent for data collection.

## 14. Incident response

- Pager rotation: 24/7 on-call for SEV-1.
- Runbooks in `docs/runbooks/` (TODO).
- Game days twice a year.
- Postmortems within 5 business days; published internally.
- Comms templates pre-drafted for customer, regulator, press.
