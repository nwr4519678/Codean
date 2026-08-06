# Platform — Enterprise Programming Education Platform

An end-to-end, production-grade SaaS for programming teachers, students, and the ecosystem
around them. Combines the best of Google Classroom, Microsoft Teams, Moodle, Coursera,
Udemy, Khan Academy, Discord, Notion, VS Code, Duolingo, and LeetCode into a single
cohesive learning experience.

---

## ✨ Highlights

- **Monorepo** with Clean Architecture backend (ASP.NET Core) + Next.js 14 frontend
- **CQRS + MediatR** + **EF Core (PostgreSQL)** + **Redis** + **Elasticsearch**
- **REST + GraphQL** APIs, JWT + Refresh tokens, 2FA, role-based authorization
- **Monthly Content Subscription** model with Paymob (binary: Free / Subscribed)
- **Live sessions** via Google Meet & Microsoft Teams with brokered access
- **Video library**, **assignments**, **exams**, **quizzes**, **rubrics**, **anti-cheat**
- **Gamification** (XP, levels, badges, streaks, leaderboards, certificates)
- **WhatsApp Parent Notifications**, **admin panel**, **teacher analytics**
- **PWA**, **dark/light mode**, **glassmorphism**, **accessibility (WCAG 2.1 AA)**
- **Docker**, **Kubernetes (Helm)**, **CI/CD (GitHub Actions)**, **Hangfire**, **SignalR**

---

## 🗂 Repository Layout

```
Platform/
├── docs/                    # 24 design + planning documents
├── backend/                 # ASP.NET Core solution (Clean Architecture)
│   ├── src/
│   │   ├── Platform.Domain         # Entities, VOs, domain events, enums
│   │   ├── Platform.Application    # CQRS, DTOs, validators, interfaces
│   │   ├── Platform.Infrastructure # EF Core, Redis, ES, Paymob, Meet/Teams
│   │   ├── Platform.Shared         # Cross-cutting, Result pattern, exceptions
│   │   ├── Platform.Api            # REST + GraphQL endpoints
│   │   └── Platform.Worker         # Hangfire background jobs
│   └── tests/               # Unit, integration, functional tests
├── frontend/                # Next.js 14 (App Router) + TypeScript
│   ├── apps/web             # Main student/teacher/parent app
│   ├── apps/admin           # Admin panel
│   └── packages/
│       ├── ui               # Shadcn UI design system
│       ├── api              # Generated API client
│       └── types            # Shared TS types
├── infra/
│   ├── docker/              # Dockerfiles
│   ├── docker-compose/      # Local stack (Postgres, Redis, ES, etc.)
│   ├── k8s/                 # Raw manifests
│   └── helm/                # Helm chart
└── .github/workflows/       # CI/CD pipelines
```

---

## 🚀 Quick start

```bash
# Local dev stack
docker compose -f infra/docker-compose/docker-compose.yml up -d

# Backend
cd backend
dotnet restore
dotnet ef database update --project src/Platform.Infrastructure --startup-project src/Platform.Api
dotnet run --project src/Platform.Api

# Frontend
cd frontend
pnpm install
pnpm dev
```

See [`docs/16-Deployment-Architecture.md`](./docs/16-Deployment-Architecture.md) for full
deployment topology and [`docs/23-Roadmap.md`](./docs/23-Roadmap.md) for the build plan.

---

## 📚 Documentation index

| # | Document | Purpose |
|---|----------|---------|
| 01 | [PRD](./docs/01-PRD.md) | Product vision, scope, success metrics |
| 02 | [Functional Requirements](./docs/02-Functional-Requirements.md) | What the system does |
| 03 | [Non-Functional Requirements](./docs/03-Non-Functional-Requirements.md) | How well it does it |
| 04 | [User Stories](./docs/04-User-Stories.md) | Acceptance criteria per role |
| 05 | [System Architecture](./docs/05-System-Architecture.md) | Components, data flow, patterns |
| 06 | [Database Design](./docs/06-Database-Design.md) | Schema, indexes, partitions |
| 07 | [ER Diagram](./docs/07-ER-Diagram.md) | Mermaid entity-relationship |
| 08 | [API Specification](./docs/08-API-Specification.md) | REST + GraphQL contract |
| 09 | [Backend Implementation Plan](./docs/09-Backend-Implementation-Plan.md) | Build phases |
| 10 | [Frontend Implementation Plan](./docs/10-Frontend-Implementation-Plan.md) | Build phases |
| 11 | [Authentication Flow](./docs/11-Authentication-Flow.md) | JWT + refresh + 2FA |
| 12 | [Authorization Flow](./docs/12-Authorization-Flow.md) | RBAC + policies |
| 13 | [Paymob Integration](./docs/13-Paymob-Integration.md) | Webhook + signature + flows |
| 14 | [Google Meet Integration](./docs/14-Google-Meet-Integration.md) | Calendar API + Meet |
| 15 | [Microsoft Teams Integration](./docs/15-Microsoft-Teams-Integration.md) | Graph + Teams |
| 16 | [Deployment Architecture](./docs/16-Deployment-Architecture.md) | Topology |
| 17 | [Docker Configuration](./docs/17-Docker-Configuration.md) | Multi-stage builds |
| 18 | [Kubernetes Deployment](./docs/18-Kubernetes-Deployment.md) | Manifests + Helm |
| 19 | [CI/CD Pipeline](./docs/19-CI-CD-Pipeline.md) | GitHub Actions |
| 20 | [Security Checklist](./docs/20-Security-Checklist.md) | OWASP + extras |
| 21 | [Testing Strategy](./docs/21-Testing-Strategy.md) | Unit/integration/E2E/load |
| 22 | [Monitoring & Logging](./docs/22-Monitoring-Logging.md) | OTel, Prometheus, ELK |
| 23 | [Roadmap](./docs/23-Roadmap.md) | MVP → Production → Future |
| 24 | [MVP vs Production vs Future](./docs/24-MVP-vs-Production-vs-Future.md) | Scope tiers |
| 25 | [AI Content Package Architecture](./docs/25-AI-Content-Package-Architecture.md) | QPack Importer |

---

## 🛡 License

Proprietary. © 2026 Platform. All rights reserved.
