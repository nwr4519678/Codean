# 10 — Frontend Implementation Plan

**Stack:** **Next.js 15 + React 19** + TypeScript (strict) + Tailwind CSS + Shadcn UI + TanStack Query + Monaco
**Status:** Approved for build
**Last updated:** 2026-08-03

---

## 1. Monorepo layout

```
frontend/
├── package.json                # npm workspaces root
├── tsconfig.json               # base TS config
├── apps/
│   ├── web/                    # main app (students, teachers, parents)
│   └── admin/                  # platform admin panel (separate Next.js project)
└── packages/
    ├── ui/                     # @platform/ui — shadcn primitives, branded components
    ├── api/                    # @platform/api — typed HTTP client, auto-refresh
    └── types/                  # @platform/types — shared enums and DTOs
```

## 2. Render strategy

| Route | Strategy | Why |
|---|---|---|
| `/`, `/explore`, `/courses/[id]` | ISR (revalidate 60 s) | SEO + freshness |
| `/auth/*` | CSR (after mount) | Form-heavy, no SEO need |
| `/dashboard/**` | CSR | User-specific, behind auth |
| `/live/[sessionId]` | CSR (SignalR hub) | Real-time |
| `/play/[lessonId]` | CSR | State-heavy, Monaco editor |

## 3. State management

- **Server state:** TanStack Query v5 (`@tanstack/react-query`)
  - Stale-while-revalidate cache
  - Background refetch
  - Infinite queries for feeds
- **Client state:** Zustand (`zustand`) — only for ephemeral UI state (sidebar, theme, modals)
- **Form state:** React Hook Form + Zod resolver
- **Auth state:** custom hook on top of `getApi()` + `localStorage`

## 4. Component conventions

- Server components by default; only mark `"use client"` when needed.
- Co-locate styles via Tailwind utility classes (no separate CSS files).
- Brand atoms live in `packages/ui`; composite business components live in `apps/web/src/components/domain`.
- Every form is wrapped with `react-hook-form` + `zod` schema in the same file as the form.

## 5. Theming & design system

- Tailwind 3.4 with custom CSS variables (HSL).
- `darkMode: ["class"]` driven by `next-themes`.
- Glassmorphism utility classes (`.glass`, `.glass-strong`).
- Gradient text utility (`.gradient-text`).
- RTL support: `[dir="rtl"] .flip-rtl` selector flips icons; logical CSS properties used throughout.

## 6. i18n

- Lightweight dictionary in `src/lib/i18n.tsx` for v1.
- Supports `en`, `ar` (RTL), `fr` (LTR).
- Translation function supports named placeholders: `t("live.startsAt", { time: "7 PM" })`.
- For full management UI, swap to `next-intl` later without touching call sites.

## 7. Accessibility (WCAG 2.1 AA)

- All interactive elements have a focus ring (`.focus-ring`).
- Form inputs have associated `<label>`s.
- Color contrast meets 4.5:1 for body text.
- Modals trap focus and restore on close.
- All icons are `aria-hidden` or have `aria-label`.
- Live regions for toasts (`role="status"`).

## 8. PWA

- `next-pwa` integration.
- Manifest at `/manifest.webmanifest` (light + dark theme_color).
- Service worker for offline read of last-viewed lessons.
- Add to Home Screen support on iOS / Android.

## 9. Performance budgets

- First Contentful Paint < 1.0 s on repeat
- Time to Interactive < 3.5 s on 4G
- Total JS < 250 KB gzipped for the home page
- No layout shift (CLS = 0)
- Images served via `next/image` with R2 + Cloudflare CDN

## 10. Build phases

### Phase 0 — Foundations (DONE)
- Next.js 15 + React 19 + TS strict + Tailwind + Shadcn primitives
- Auth pages (login, register)
- Dashboard shell (sidebar, topbar, theme toggle)
- Student dashboard (stats, continue learning, upcoming)
- Course explorer (mock data)
- i18n provider (en/ar/fr)
- API client with auto-refresh

### Phase 1 — Courses & Lessons
- Course detail (modules, lessons, packages, subscribe CTA)
- Lesson player (markdown render, video, Mermaid diagrams, whiteboard)
- Watch progress sync
- Bookmarks / notes

### Phase 2 — Live Sessions
- Live join page (brokered URL fetch + redirect)
- Calendar view
- Reminders + notifications

### Phase 3 — Coding
- Problem list + filters
- Submission history + verdicts
- Editorial / hints

### Phase 4 — Assessments
- Assignment submission
- Exam UI (timer, anti-cheat, randomized)
- Quiz UI

### Phase 5 — Teacher Tools
- Course builder (drag-and-drop)
- Question bank
- Live scheduler
- Analytics dashboards
- Revenue / payouts

### Phase 6 — WhatsApp Parent Notifications UI
- Teacher settings dashboard for WhatsApp features
- Per-student notification toggles
- Broadcast messaging interface
- Report template editor

### Phase 7 — Admin
- Moderation queue
- Audit log search
- Feature flag management
- System settings
