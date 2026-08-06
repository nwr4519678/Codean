# 03 — Non-Functional Requirements

**Version:** 1.0
**Last updated:** 2026-08-03

---

## 1. Performance

| ID | Target |
|---|---|
| NFR-PERF-01 | P95 page load (web) < 2.0 s on 4G |
| NFR-PERF-02 | API P95 latency < 200 ms (cached), < 500 ms (cold) |
| NFR-PERF-03 | P99 API latency < 1.5 s |
| NFR-PERF-04 | Sustained 10,000 RPS on read endpoints, 2,000 RPS on write |
| NFR-PERF-05 | Code execution queue: P95 < 8 s, P99 < 15 s |
| NFR-PERF-06 | Live session: join button → meeting page < 1.5 s |
| NFR-PERF-07 | First Contentful Paint < 1.0 s on repeat visits |
| NFR-PERF-08 | Time-to-Interactive < 3.5 s on 4G |
| NFR-PERF-09 | Video start time < 1.5 s (CDN pre-warm) |
| NFR-PERF-10 | Search results < 300 ms P95 |

## 2. Scalability

- **Users:** target 250k MAU students, 5k active teachers
- **Horizontal:** stateless API pods (HPA 3 → 50)
- **DB:** Postgres primary + 2 read replicas; Citus-ready for tenant sharding (v2)
- **Cache:** Redis cluster (3 masters + 3 replicas); 80%+ hit rate target
- **Queue:** Hangfire (SQL-backed) for v1; ready to swap to RabbitMQ
- **Object storage:** Cloudflare R2, lifecycle to infrequent access after 30 days
- **CDN:** Cloudflare in front of R2 + frontend
- **Background:** Hangfire workers auto-scale 2 → 20

## 3. Availability & Reliability

| ID | Target |
|---|---|
| NFR-AVAIL-01 | 99.9 % monthly uptime |
| NFR-AVAIL-02 | RPO ≤ 15 min (DB PITR + WAL streaming) |
| NFR-AVAIL-03 | RTO ≤ 60 min |
| NFR-AVAIL-04 | Zero-downtime deploys (rolling update, blue/green for DB migrations) |
| NFR-AVAIL-05 | Graceful degradation: if Redis is down, fall back to DB; if ES is down, fall back to SQL `LIKE` |

## 4. Security

- **HTTPS only** (HSTS, TLS 1.2+)
- **OWASP Top 10** baseline enforced (see [`20-Security-Checklist.md`](./20-Security-Checklist.md))
- **JWT** RS256 with 15-min TTL; refresh tokens hashed at rest (SHA-256)
- **Passwords** BCrypt cost 12+
- **Secrets** in Kubernetes Secrets / external vault (HashiCorp Vault v2)
- **Rate limiting** per IP + per user (token bucket)
- **Audit logs** for all privileged actions, retained 365 days
- **Encryption at rest** (R2 SSE, Postgres TDE)
- **Encryption in transit** (TLS 1.2+)
- **CSP, X-Frame-Options, X-Content-Type-Options** enforced
- **PII minimization** — emails hashed for analytics; payment data never stored (Paymob tokenization)

## 5. Maintainability

- **Clean Architecture** with strict dependency rule (Domain ← Application ← Infrastructure)
- **CQRS** via MediatR; one handler per use case
- **Result pattern** instead of exceptions for expected errors
- **Specification pattern** for reusable queries
- **Code coverage** ≥ 80 % on Domain & Application
- **Style:** EditorConfig + dotnet format + ESLint + Prettier; zero warnings
- **.editorconfig** + **Directory.Build.props** for shared compile settings
- **Public APIs** annotated with XML doc comments
- **Architecture tests** (NetArchTest) to enforce layer rules

## 6. Observability

- **Structured logging** (Serilog → JSON → OpenSearch)
- **Distributed tracing** (OpenTelemetry → Jaeger / Tempo)
- **Metrics** (Prometheus): RED (rate, errors, duration) + USE (utilization, saturation, errors)
- **Real User Monitoring** (web vitals → Grafana)
- **Synthetic checks** every 60 s from 3 regions
- **SLO dashboards** per service

## 7. Accessibility

- **WCAG 2.1 AA** compliant
- **Keyboard navigable** for all flows
- **Screen reader** tested with NVDA + VoiceOver
- **Contrast** ≥ 4.5:1 (text), 3:1 (large text/UI)
- **RTL** support throughout (Arabic)
- **Captions** on all platform-produced videos

## 8. Internationalization

- All strings externalized; **i18next** on web, **IStringLocalizer** on backend
- **Date/number formatting** via `Intl` (web) / `CultureInfo` (backend)
- **Time zones:** stored in UTC, displayed per user preference
- **Currency:** EGP default; multi-currency display

## 9. Compliance

- **GDPR** — right to access, rectify, erase; data export; consent banner
- **PDPL (Egypt)** — data residency, consent, breach notification
- **COPPA-style** minor protection — no behavioral ads, no public profiles under 16
- **PCI-DSS** scope minimized via Paymob hosted checkout (no PAN ever touches our servers)

## 10. Portability

- **Containerized** (Docker); runs on any K8s
- **No vendor lock-in** — abstractions for Paymob (so Stripe/PayPal can be added), Meet/Teams (so Zoom can be added)
- **12-factor app** — config via env, stateless, log to stdout

## 11. Cost efficiency

- **Paymob transaction fee** baked into pricing math
- **R2 egress** free — heavy video served via R2 + Cloudflare
- **Auto-scale to zero** in non-prod
- **Aggressive caching** to keep DB load flat

## 12. Testability

- See [`21-Testing-Strategy.md`](./21-Testing-Strategy.md)
- Unit (xUnit), integration (Testcontainers), functional (Playwright), load (k6)
- Mutation testing (Stryker.NET) on critical paths

## 13. Versioning & Compatibility

- API **versioned** via URL (`/api/v1/...`) + `X-API-Version` header
- **Deprecation policy:** 6 months notice, `Sunset` header, migration guide
- **DB migrations** are forward-only; backward-compatible within a release train

## 14. Disaster recovery

- **DB PITR** with 7-day retention
- **Object storage** cross-region replication
- **Backups:** daily full + hourly WAL; restore drill quarterly
- **Runbooks** for top 10 incident types
- **Game day** twice a year
