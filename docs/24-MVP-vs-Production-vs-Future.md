# 24 — MVP vs Production vs Future

**Last updated:** 2026-08-03

> Concrete scope tiers for the Platform product. Each tier is independently shippable; later tiers are additive.

---

## MVP (Q1 — first 90 days)

**Goal:** prove the core loop: teacher publishes → student subscribes → student learns → student pays.

### In
- **Auth** — email + password, Google + Microsoft OAuth, JWT + refresh, 2FA (TOTP), email verify, password reset, lockout
- **Course CRUD** — title, description, cover, level, language, tags
- **Modules + Lessons** — text, video, code blocks
- **Video library** — upload via Cloudflare R2, signed download URLs, watch progress
- **Live sessions** — Google Meet, brokered join, attendance log
- **Monthly packages** — Month 1, 2, 3 per course, each priced independently
- **Paymob integration** — card, mobile wallets, BNPL; HMAC-validated webhook
- **One coding language** — Python
- **Student dashboard** — my courses, continue learning, upcoming live sessions
- **Teacher dashboard** — revenue, subscribers, simple analytics
- **Admin panel** — users, courses, payments, logs
- **Postgres + Redis** in production
- **Docker** + docker-compose for dev
- **CI** for build + tests + security scan
- **Deploy to staging**

### Out
- Teams live sessions (Q2)
- Multiple coding languages (Q2)
- Full assessments (Q2)
- Gamification (Q2)
- WhatsApp Parent Notifications (Q2)
- Search (Q3)
- PWA polish (Q2)
- Mobile app (v2)

### Team
- 1 PM, 2 BE, 1 FE, 1 designer, 1 DevOps, 1 QA, 1 SRE (part-time)

### Risk
- **Paymob API differences** — adapter pattern + contract tests mitigate.
- **R2 egress at scale** — Cloudflare CDN fronts it.
- **First-100 teacher onboarding** — hand-hold via CSM, weekly office hours.

---

## Production (Q2–Q3)

**Goal:** scale to 100k MAU, 25k paying students, full feature parity with the brief.

### Adds
- Full assignment / exam / quiz lifecycle
- Question bank + random exam generation
- Anti-cheat for exams
- All 6 coding languages
- Microsoft Teams live sessions
- Recording playback
- Gamification (XP, levels, streaks, badges, leaderboards, certificates)
- WhatsApp Parent Notifications
- Search (Elasticsearch)
- Analytics dashboards (teacher, student, platform)
- PWA + offline
- K8s deploy to production (multi-AZ)
- DR setup
- SLO monitoring + alerting
- Penetration test
- Multi-region (EU + MEA)

### Out
- AI features (v2)
- White-label (v2)
- Native mobile (v2)
- Marketplace (v2)

### Team
- + 2 BE, + 1 FE, + 1 ML (for analytics), + 1 data engineer, + 2 SRE, + 2 CSM

---

## V2 (Q4+)

**Goal:** differentiate with AI, expand TAM.

### Adds
- AI tutor (RAG over course content)
- AI problem generator
- AI code review
- Voice/video transcription
- White-label / school multi-tenant
- Native iOS / Android (React Native)
- B2B sales motion
- Marketplace
- Public API + webhooks
- SOC 2 Type II
- International expansion

### Out
- AR/VR (v3)
- Open-source (v3)

---

## Decision log (scope)

| Decision | Why |
|---|---|
| Single PSP in v1 (Paymob) | Reduces compliance + integration scope |
| Binary Free / Subscribed | Aligns with the monthly-package model in the brief |
| One language in MVP (Python) | Validates the judging pipeline; multi-language is mechanical to add |
| No native mobile in v1 | PWA covers 80 % of mobile needs at 20 % of the cost |
| No AI in v1 | Quality control + cost predictability |
| No multi-tenant white-label in v1 | Single brand is the focus; enterprise upsell is v2 |
| No self-hosted in v1 | Operational complexity — single-tenant cloud only |
| No ads | Revenue is subscriptions only |
| Live meetings brokered, not raw URLs | Prevents leak, enables per-student access logging |
