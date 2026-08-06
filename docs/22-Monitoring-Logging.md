# 22 — Monitoring & Logging

**Last updated:** 2026-08-03

## 1. The three pillars

### 1.1 Logs (Serilog → OpenSearch)

- JSON structured logs to stdout + OpenSearch.
- Per-request enrichment: `request_id`, `correlation_id`, `user_id`, `tenant_id`, `user_agent`, `ip`.
- Log levels: `Information` default, `Warning` for 4xx, `Error` for 5xx, `Fatal` for SEV-1.
- Sensitive data scrubbed at the source: passwords, tokens, PII fields with `[NotLogged]` attribute.
- Retention: 90 days hot in OpenSearch, 1 year in S3 (Parquet), 7 years in cold archive (financial compliance).

Example Serilog config:
```csharp
.WriteTo.Console(formatter: new CompactJsonFormatter())
.WriteTo.OpenSearch(opts => { opts.IndexFormat = "platform-logs-{0:yyyy.MM.dd}"; })
```

### 1.2 Metrics (Prometheus + Grafana)

Exposed at `/metrics` (only on the internal service port, never publicly).

Key metric families:

| Name | Type | Labels | Use |
|---|---|---|---|
| `http_requests_total` | counter | method, route, status | RED |
| `http_request_duration_seconds` | histogram | method, route | RED |
| `http_requests_in_flight` | gauge | route | saturation |
| `db_query_duration_seconds` | histogram | operation, table | DB perf |
| `redis_cache_hits_total` | counter | key_prefix | hit rate |
| `redis_cache_misses_total` | counter | key_prefix | hit rate |
| `mediator_handler_duration_seconds` | histogram | request | app perf |
| `mediator_handler_failures_total` | counter | request, error_code | reliability |
| `paymob_intention_duration_seconds` | histogram | status | provider health |
| `paymob_webhook_failures_total` | counter | reason | provider health |
| `hangfire_jobs_enqueued_total` | counter | job_type | queue depth |
| `hangfire_jobs_processing_seconds` | histogram | job_type | worker perf |
| `signin_attempts_total` | counter | result, mfa | security |

### 1.3 Traces (OpenTelemetry → Jaeger/Tempo)

Auto-instrumentation:
- ASP.NET Core (incoming requests)
- HttpClient (outgoing calls to Paymob, Google Meet, MS Teams, ES)
- EF Core (queries)
- SignalR (hub calls)

Custom spans:
- `Order.Create` — full request lifecycle including Paymob call
- `LiveSession.Join` — auth + token issue + redirect

Trace context is propagated through Hangfire jobs (`TraceContextEnrichment`).

Trace sampling: 100 % for errors, 10 % for the rest (head sampling at the collector).

## 2. Dashboards (Grafana)

| Dashboard | Audience | Panels |
|---|---|---|
| **Service overview** | Engineers | RED, saturation, recent deploys |
| **API health** | Engineers | Per-endpoint p50/p95/p99, top errors |
| **Database** | DBAs | Connections, slow queries, replication lag, bloat |
| **Cache** | Engineers | Hit rate, memory, eviction rate |
| **Paymob** | Finance | Transaction volume, success/fail, refund count |
| **Live sessions** | Teachers | Active sessions, join latency, provider health |
| **Auth** | Security | Login attempts, lockouts, 2FA enrollments |
| **Business KPIs** | Product | DAU, conversion, revenue, churn |

## 3. Alerts

| Alert | Condition | Severity | Response |
|---|---|---|---|
| `APILatencyHigh` | p95 latency > 1 s for 5 min | SEV-3 | Investigate DB, caches |
| `APIErrorRate` | 5xx rate > 1 % for 5 min | SEV-2 | Page on-call |
| `ServiceDown` | 0 successful probes for 2 min | SEV-1 | Page on-call immediately |
| `PaymobWebhookFailing` | webhook signature fail > 5 in 5 min | SEV-2 | Check secret rotation |
| `PostgresConnections` | used > 80 % of max | SEV-3 | Investigate, raise max |
| `ReplicasOut` | ready replicas < desired - 1 | SEV-3 | Investigate pods |
| `CertExpiringSoon` | cert < 14 d to expiry | SEV-4 | Renew |
| `BackupMissed` | no successful backup in 25 h | SEV-2 | Investigate |
| `PIIExportRateHigh` | > 100 GDPR exports in 1 h | SEV-3 | Possible scraping |

## 4. Health checks

- `/health/live` — process up, no deps. Returns 200.
- `/health/ready` — Postgres, Redis, ES, R2 (signed URL test) all green. Returns 200.
- `/health/startup` — same as ready but does not fail the pod until k8s `initialDelaySeconds` is past.

Used by:
- Kubernetes liveness / readiness / startup probes
- Load balancer target health
- Monitoring ping

## 5. Synthetic monitoring

- Playwright scripts run every 60 s from 3 regions (us-east-1, eu-west-1, me-central-1).
- Flow: open `/`, search a course, log in (test user), open the dashboard.
- Any failure opens a SEV-3 alert.

## 6. RUM (Real User Monitoring)

- Web Vitals (LCP, INP, CLS, TTFB) collected on every page via `next/web-vitals`.
- Sent to Grafana Cloud (Tempo) via a lightweight OTel SDK.
- Sampled at 10 %.

## 7. Backup & DR monitoring

- Daily DB backup success/fail → alert on fail.
- WAL stream lag → alert if > 5 min.
- Restore drill quarterly, results tracked.
- Backup retention: 7 d hot, 30 d warm, 365 d cold.

## 8. Cost monitoring

- Daily CloudWatch cost report → Slack.
- Per-namespace cost allocation tags.
- Top spend dashboard (R2 storage, RDS, ALB).

## 9. Tooling inventory

| Concern | Tool |
|---|---|
| Logs | Serilog + OpenSearch + Loki (mirror) |
| Metrics | prometheus-net + Grafana Cloud |
| Traces | OpenTelemetry + Tempo (or Jaeger) |
| Errors | Sentry (optional) |
| Uptime | Grafana Synthetic / Checkly |
| RUM | Web Vitals + OTel browser SDK |
| Cost | CloudZero / CloudHealth |
