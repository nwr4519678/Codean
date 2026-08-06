# 23 — Roadmap

**Last updated:** 2026-08-03

## Vision

> *Become the operating system for programming teachers in the Baccalaureate ecosystem and beyond.*

## Now → 12 months

### Q1 — MVP (months 1–3)
- ✅ Auth: email + Google + Microsoft + 2FA
- ✅ Course authoring (CRUD + module/lesson tree)
- ✅ Video library (Cloudflare R2, signed URLs)
- ✅ Live sessions via Google Meet (Teams deferred to Q2)
- ✅ Paymob subscription (binary Free / Subscribed, monthly packages)
- ✅ Basic student dashboard + course player
- ✅ Basic teacher dashboard
- ✅ Admin panel (user mgmt, payment overview)
- ✅ Postgres + Redis + R2
- ✅ Docker compose for local dev
- ✅ CI: build, unit, integration, security scan
- ✅ Deploy to staging

### Q2 — Production launch (months 4–6)
- Course builder: drag-and-drop reorder
- Assignments, exams, quizzes (full)
- Question bank with random exam generation
- Auto-grading for MCQ
- Manual grading with rubrics
- Anti-cheat (fullscreen, copy/paste lock, tab-switch)
- Microsoft Teams live sessions
- Gamification (XP, levels, streaks, badges, certificates)
- WhatsApp Parent Notification System
- Email + push notifications
- PWA + offline
- K8s deploy to production
- CDN setup
- Backup + DR

### Q3 — Scale (months 7–9)
- Multi-region (EU + MEA)
- Search (Elasticsearch) for courses/lessons
- Analytics dashboards (teacher, student, platform)
- Recording playback
- Coupon + referral + scholarship flows
- Teacher payouts (Stripe Connect)
- Mobile-responsive PWA polish
- Performance: code-split, prefetch, virtualized lists

### Q4 — V2 (months 10–12)
- AI tutor (RAG over course content, hint generation, code review)
- School multi-tenant skins
- Native iOS / Android (React Native)
- Advanced analytics (cohort retention, ML-based recommendations)
- Whiteboard live collaboration (CRDT)
- GraphQL subscriptions for live collaboration
- Public API + webhooks for partners

## 12–24 months

- **AI tutor** — RAG + LLM-powered hint, code review, lesson summarization.
- **AI problem generator** — auto-generate practice problems from topics.
- **Voice/video live transcription** + auto-generated captions.
- **Adaptive learning** — personalize lesson order based on performance.
- **Marketplace** — third-party courses with revenue share.
- **B2B** — school district contracts with bulk billing and admin tools.
- **International expansion** — LATAM, South Asia.
- **Compliance** — SOC 2 Type II, ISO 27001, HIPAA-ready.

## 24–36 months

- **Studio** — teachers record and produce courses in-platform with AI-assisted editing.
- **On-device AI** — privacy-preserving local LLMs for student practice.
- **VR/AR coding** — immersive pair-programming for advanced courses.
- **Open-source** — extract the platform's core LMS as OSS for schools to self-host.

## Success milestones

| Month | MAU | Paying students | Revenue (USD) |
|---|---|---|---|
| 3 | 5,000 | 1,000 | 5k MRR |
| 6 | 30,000 | 6,000 | 30k MRR |
| 9 | 100,000 | 25,000 | 120k MRR |
| 12 | 250,000 | 60,000 | 300k MRR |
| 24 | 1,000,000 | 250,000 | 1.5M MRR |

## Non-goals (deliberate)

- **Native iOS / Android in v1** — PWA is enough. Native is v2.
- **Self-hosted** — single-tenant cloud is the only product. On-prem is an enterprise upsell.
- **Non-programming subjects** — math, languages, etc. Focus wins.
- **Blockchain / crypto** — no Web3 features planned.
- **Ads** — no ads. Revenue is subscriptions only.
- **In-browser code execution & automated grading** — canceled to simplify scope.
