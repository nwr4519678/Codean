# 16 — Deployment Architecture

**Last updated:** 2026-08-03

## 1. Topology

```
                            ┌──────────────────────────┐
                            │   Cloudflare (WAF + CDN) │
                            │   - DDoS                 │
                            │   - Rate-limit           │
                            │   - TLS termination     │
                            └──────────┬───────────────┘
                                       │
       ┌───────────────────────────────┼─────────────────────────────────┐
       │                               │                                 │
┌──────▼──────┐                ┌───────▼─────────┐                 ┌──────▼──────┐
│ Next.js Web │                │ Next.js Admin   │                 │ Marketing  │
│ (Vercel /   │                │ (Vercel /       │                 │ (Cloudflare│
│  k8s)       │                │  k8s)           │                 │  Pages)    │
└──────┬──────┘                └────────┬────────┘                 └────────────┘
       │                                │
       └─────────────┬──────────────────┘
                     │ HTTPS
                     ▼
            ┌────────────────────┐
            │   API Gateway      │   ◄── YARP / Cloudflare Worker
            │   /api/v1, /graphql│
            │   /hubs/notifications
            └────────┬───────────┘
                     │
       ┌─────────────┼─────────────────────────────┐
       │             │                             │
┌──────▼──────┐ ┌────▼──────┐                ┌──────▼──────┐
│ API pods    │ │ Worker    │                │ Judge       │
│ (k8s)       │ │ (k8s)     │                │ (k8s,       │
│ stateless   │ │ Hangfire  │                │  spot nodes)│
│ 3+ replicas │ │ 2+ repl.  │                │  autoscale  │
└──────┬──────┘ └────┬──────┘                └──────┬──────┘
       │             │                             │
       └─────────────┼─────────────────────────────┘
                     │
       ┌─────────────┼─────────────┐
       │             │             │
┌──────▼──────┐ ┌────▼──────┐ ┌────▼──────┐
│ PostgreSQL  │ │ Redis     │ │ Elastic   │
│ RDS Multi-AZ│ │ Elasti-   │ │ Cloud /   │
│ + read      │ │ Cache     │ │ self-host │
│ replicas    │ │ Cluster   │ │ on k8s    │
└─────────────┘ └───────────┘ └───────────┘
       │
┌──────▼─────────────────────┐
│ Cloudflare R2 (S3-compat)  │
│ Videos, PDFs, attachments │
└────────────────────────────┘
```

## 2. Cloud

**Primary:** AWS (`us-east-1`)
- RDS Postgres Multi-AZ + 2 read replicas
- ElastiCache Redis cluster
- ALB for ingress
- Secrets Manager + External Secrets Operator

**Static / Edge:** Cloudflare
- CDN + WAF
- R2 for object storage (zero egress)
- Pages for marketing site (optional)
- Turnstile for bot protection on auth forms

**Observability:** Grafana Cloud (managed Prometheus + Loki + Tempo) or self-hosted.

## 3. Regions

- v1 launch: `me-central-1` (MEA) for primary, `eu-west-1` (EU) for DR.
- Read replicas in `eu-west-1` for GDPR data residency.
- CDN edges global.

## 4. Environments

| Env | Purpose | Infra |
|---|---|---|
| `local` | Dev on a single box | docker-compose |
| `dev` | Shared dev cluster | EKS small tier |
| `staging` | Pre-prod | EKS mid tier, full data |
| `prod` | Live | EKS production tier, multi-AZ |

## 5. Data flow at runtime

- All incoming traffic → Cloudflare → ALB → API pods.
- API reads from Redis (cache) or Postgres primary; reads prefer read replicas.
- Long-running work → enqueue Hangfire → Worker pods.
- File uploads → frontend asks API for presigned R2 URL → browser PUTs directly to R2.

## 6. Capacity planning

| Component | Initial | Target at 250k MAU |
|---|---|---|
| API pods | 3 | 3 → 50 (HPA 60% CPU) |
| Worker pods | 2 | 2 → 20 |
| Postgres | db.t4g.medium | db.r6g.2xlarge + 2 read replicas |
| Redis | cache.t4g.small | cache.r6g.large cluster |
| R2 storage | 100 GB | 50 TB |

## 7. DR strategy

- **RPO:** 15 min (Postgres PITR + WAL stream to S3)
- **RTO:** 60 min (warmer standby in `eu-west-1`)
- **Backup tests:** quarterly restore drill
- **Game days:** twice a year — inject failure and time the recovery
