# 01 — Product Requirements Document (PRD)

**Project:** Platform — Enterprise Programming Education SaaS
**Version:** 1.0
**Status:** Approved for build
**Owner:** Product / Engineering
**Last updated:** 2026-08-03

---

## 1. Vision

Become the **operating system for programming teachers** in the Baccalaureate ecosystem
(and beyond): a single platform where teachers author curricula, run live classes,
grade work, sell monthly content packages, and where students learn, practice, get
graded, and earn credentials — with the polish of a modern consumer product and the
rigor of an enterprise platform.

## 2. Mission

Replace the patchwork of Google Classroom + Drive + WhatsApp + YouTube that teachers
currently glue together. Provide an **all-in-one learning + commerce + community**
platform that scales to **tens of thousands of teachers** and **hundreds of thousands
of students**.

## 3. Target users & personas

| Persona | Goals | Pain points we solve |
|---|---|---|
| **Teacher** | Author courses, schedule live classes, grade at scale, monetize | Fragmented tools, no payment flow, manual grading |
| **Teacher Assistant** | Help grade, answer questions, manage groups | No shared workspace with teacher today |
| **Student** | Learn programming, practice, get feedback, get certificates | No unified curriculum, no instant feedback on code |
| **Parent** | Track child progress, pay, communicate | No transparency, manual payment collection |
| **School Admin** | Onboard teachers/students, monitor school performance | No admin tools in current solutions |
| **Platform Admin** | Operate the SaaS, moderate content, manage revenue | None — we're building it |
| **Super Admin** | Multi-tenant control, billing for schools | None — we're building it |
| **Support** | Resolve tickets, view audit logs | No tooling today |

## 4. Scope

### In scope (v1)

- Course authoring (modules, lessons, units, challenges, homework, projects, quizzes, exams)
- Monthly content subscription (binary Free / Subscribed) via **Paymob**
- Live sessions via **Google Meet** and **Microsoft Teams**
- In-browser coding environment (Python, C, C++, Java, JavaScript, C#)
- Assignments, exams, quizzes with auto + manual grading
- Video library + downloads (Cloudflare R2)
- Notifications (in-app, email, push, SMS-ready, WhatsApp-ready)
- Analytics dashboards for teachers, students, and admins
- Gamification (XP, levels, badges, streaks, leaderboards, certificates)
- WhatsApp Parent Notification System
- Admin panel (RBAC, content moderation, finance, audit)
- PWA, dark/light mode, accessibility

### Out of scope (v1)

- Native iOS/Android apps (PWA only)
- Self-hosted on-premise deployment (cloud-only)
- AI tutoring assistant (planned for v2 — see [`24-MVP-vs-Production-vs-Future.md`](./24-MVP-vs-Production-vs-Future.md))
- White-label / multi-tenant school skins (planned for v2)

## 5. Differentiators

1. **Monthly content model** — teachers sell **Month 1, Month 2, ...** packages
   independently. Purchased months remain accessible for **1 year** even if subscription
   lapses. No all-or-nothing bundles.
2. **First-class coding environment** — Monaco editor + sandboxed runner + IntelliSense
   inside the same platform (no external Judge0 / Replit).
3. **Brokered live access** — students join via platform-issued short-lived tokens;
   teacher meeting links are never exposed.
4. **Teacher-as-business** — revenue, analytics, payouts, coupons, scholarships,
   referral codes all built in.
5. **Multi-role from day one** — Support / Student / Teacher / TA / School Admin /
   Platform Admin / Super Admin, each with explicit permissions.

## 6. Success metrics (12-month targets)

| Metric | Target |
|---|---|
| MAU students | 250,000 |
| Active teachers | 5,000 |
| Monthly paying students | 60,000 |
| Avg. course completion (paid) | 45 % |
| P95 page load (web) | < 2.0 s |
| Uptime | 99.9 % |
| NPS | 50+ |
| Monthly churn (paid) | < 6 % |
| LTV : CAC | > 3.5 |
| Avg. revenue per teacher | $400 / month |

## 7. Constraints

- **Region:** primary deployment in EU/MEA; data residency in EU
- **Compliance:** GDPR, Egypt PDPL, OWASP Top 10 baseline
- **Payments:** Paymob (Egypt) — Visa, MC, Meeza, Vodafone/Orange/Etisalat Cash,
  ValU, Sympl, Apple Pay (if available)
- **Stack lock-in:** **ASP.NET Core on .NET 10** + **React 19** (no node-only or .NET Framework)

## 8. Risks

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| Paymob API changes | Medium | High | Adapter pattern, contract tests, monitor changelog |
| Live provider rate limits | Medium | High | Provider fallback (Meet → Teams), queue + retry |
| Content piracy | High | High | Signed short-lived URLs, watermarking, DRM-lite on videos |
| Scale spike on exam day | High | Medium | Auto-scale, pre-warm, read replicas, CDN |
| PII breach | Low | Critical | Encryption at rest/in transit, audit logs, DPO review |

## 9. Stakeholders

- **Exec sponsor:** CEO
- **Product owner:** Head of Product
- **Tech lead:** Principal Engineer
- **Design lead:** Principal Designer
- **QA lead:** Head of QA
- **DevOps lead:** Head of Platform

## 10. Release strategy

- **MVP (Q1):** auth, courses, lessons, video, basic live (Meet), Paymob subscription,
  one coding language (Python), admin minimal — see [`24-MVP-vs-Production-vs-Future.md`](./24-MVP-vs-Production-vs-Future.md)
- **Production (Q2–Q3):** full course tools, exams/assignments, full coding environment,
  Teams integration, gamification, WhatsApp Parent Notification System
- **V2 (Q4+):** AI tutor, school multi-tenant, mobile, advanced analytics
