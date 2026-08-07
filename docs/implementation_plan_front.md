# Frontend: Enterprise Educational Platform — ✅ FINAL APPROVED v5 (10/10)

> Plan locked. Execution started.

---

## Non-Negotiable Requirements

### 🌐 Internationalization (i18n) — Full Requirement
- [x] Full English + Arabic localization via **next-intl**
- [x] RTL/LTR auto switching (CSS `dir` attribute on `<html>`)
- [x] Locale-aware routing: `/en/dashboard`, `/ar/dashboard`
- [x] Language switcher accessible from every page
- [x] Persist selected language in **Cookie** (server-readable for SSR)
- [x] Detect browser language on first visit → redirect to correct locale
- [x] Localized dates, numbers, currency via `Intl` API + next-intl
- [x] Localized metadata & SEO (`generateMetadata` per locale)
- [x] Translate ALL texts, alerts, validation errors, toast messages
- [x] OpenGraph / Twitter Card metadata per locale

### 📱 Responsive & Accessibility — Full Requirement
| Breakpoint | Range | Layout |
|---|---|---|
| Mobile S | 320px – 480px | Single column, stacked |
| Mobile L | 481px – 640px | Single column, larger touch targets |
| Tablet | 641px – 1024px | 2-col grid, collapsible sidebar |
| Laptop | 1025px – 1280px | Full sidebar + content |
| Desktop | 1281px – 1536px | Wide sidebar + multi-col content |
| Ultra-wide | 1537px+ | Max-width container centered |

- [x] **Sidebar → Vaul Drawer** on mobile (≤1024px)
- [x] **Tables → Cards / horizontal scroll** on mobile
- [x] **Navbar → hamburger menu** on mobile
- [x] Touch-friendly buttons (min 44×44px tap targets)
- [x] Monaco Editor with mobile-compatible config (read-only hint on small screens)
- [x] Video.js responsive player (aspect-ratio: 16/9, 100% width)
- [x] Landing page fully responsive (hero, bento grid, pricing)
- [x] Dashboard auto-rearranges cards (1→2→3→4 columns)
- [x] **WCAG 2.2 AA** accessibility
- [x] Full keyboard navigation
- [x] Screen reader support (aria-labels, semantic HTML)
- [x] Playwright responsive tests (375px, 768px, 1280px, 1920px viewports)

---

## Complete Tech Stack

### Foundation
| Tool | Role |
|---|---|
| **Next.js 16** | App Router, RSC, SSR, image optimization |
| **React 19** | Concurrent features, Server Actions |
| **TypeScript 5.7** | Type safety |
| **Tailwind CSS v4** | Utility-first, mobile-first |
| **pnpm workspaces** | Package manager |
| **Turborepo** | Build cache, parallel builds, fast CI/CD |
| **next/font** | Inter + Geist (LTR) / Noto Sans Arabic (RTL) |
| **next/image** | All images — optimized, lazy, responsive |

### i18n
| Tool | Role |
|---|---|
| **next-intl** | Translation management, locale routing |
| **Intl API** | Date, number, currency formatting per locale |
| **js-cookie / cookies-next** | Persist locale in cookie (SSR-readable) |
| **Tailwind `rtl:` variants** | RTL layout overrides |

### Component Libraries

#### Core
| Library | Purpose |
|---|---|
| **shadcn/ui** | Core headless primitives (Button, Dialog, Table, Form, Toast, etc.) |
| **Origin UI** | Extra polished copy-paste components (stats, badges, avatars, inputs) |
| **react-aria** | Complex accessible components — date pickers, comboboxes (WCAG 2.2) |
| **Lucide React** | Icon set |
| **Recharts** | Charts & analytics dashboards |
| **Storybook** | Component docs on `packages/ui` |

#### ✨ Premium Animation / Visual Libraries (all copy-paste, lazy loaded)
| Library | Specific Components Used |
|---|---|
| **Aceternity UI** | `BackgroundBeams`, `Spotlight`, `AnimatedCards`, `FloatingNavbar`, `InfiniteMovingCards`, `TextGenerateEffect`, `TypewriterEffect`, `GlowingStars`, `MovingBorder`, `TracingBeam` |
| **Magic UI** | `Particles`, `BentoGrid`, `Marquee`, `BorderBeam`, `NumberTicker`, `ShimmerButton`, `AnimatedList`, `HeroVideoDialog`, `BlurFade`, `Meteors` |
| **Tailark** | Pre-built page sections: Hero variants, Feature grids, Pricing tables, CTA banners, Stats sections, Testimonial blocks |
| **React Bits** | `TrueFocus`, `CrosshairCursor`, animated backgrounds, lightweight text effects (zero Framer Motion dependency) |

#### Usage Assignment
| Page / Section | Libraries Used |
|---|---|
| Landing Hero | Aceternity `Spotlight` + `BackgroundBeams` + Magic UI `Particles` |
| Landing Features | Magic UI `BentoGrid` + Tailark Feature Grid |
| Landing Testimonials | Aceternity `InfiniteMovingCards` + Magic UI `Marquee` |
| Landing Stats | Magic UI `NumberTicker` + Tailark Stats Section |
| Landing CTA | Magic UI `BorderBeam` + Tailark CTA Banner |
| Landing Pricing | Tailark Pricing Table |
| Course Cards | Aceternity `MovingBorder` on hover |
| Course Detail Hero | Aceternity `TracingBeam` sidebar |
| Auth Pages | Magic UI `BlurFade` entrance animation |
| Dashboard | Magic UI `AnimatedList` for activity feed |
| Navbar (public) | Aceternity `FloatingNavbar` |
| Text Headings | Aceternity `TextGenerateEffect` / Magic UI `BlurFade` |
| Backgrounds | React Bits animated backgrounds (lightweight) |

### Data & State
| Tool | Role |
|---|---|
| **TanStack Query v5** | Server state, caching, prefetching |
| **TanStack Table v8** | Headless data tables |
| **Zustand** | Auth, theme, sidebar, locale stores |
| **React Hook Form + Zod** | Form validation (localized error messages) |
| **Axios** | HTTP client + JWT refresh |
| **nuqs** | URL state (locale-aware) |
| **MSW** | API mocking |

### Rich Content & Heavy Libraries (ALL lazy via `next/dynamic`)
| Tool | Loaded On | Bundle Impact |
|---|---|---|
| **Monaco Editor** | Judge pages only | ~2MB isolated |
| **Video.js** | Lesson player only | ~200KB isolated |
| **Tiptap** | Course/homework editor only | ~150KB isolated |
| **Aceternity UI** | Landing + marketing pages only | Copy-paste, no extra dep |
| **Magic UI** | Landing + marketing pages only | Copy-paste, no extra dep |
| **Tailark sections** | Landing page only | Copy-paste, no extra dep |

> ⚠️ Bundle rule: Home page (`/`) must stay under **150KB** gzipped. Run `@next/bundle-analyzer` before every merge.

### File Uploads
- **react-dropzone → Axios → Backend → Cloudflare R2**

### UX
| Tool | Role |
|---|---|
| **Motion** | Transitions, scroll reveals |
| **Vaul** | Mobile sidebar drawer |
| **embla-carousel** | Carousels |
| **cmdk** | Cmd+K palette |
| **Sonner** | Localized toasts |
| **date-fns** | Date formatting |
| **next-themes** | Dark mode |

### Observability
| Tool | Role |
|---|---|
| **Sentry** | Error monitoring from day 1 |
| **PostHog** | Behavioral analytics |

### Quality
| Tool | Role |
|---|---|
| **Playwright** | E2E + responsive tests (4 viewports) |
| **Oxlint** | Linting |
| **Prettier** | Formatting |
| **@next/bundle-analyzer** | Bundle audit |

### SEO
| Feature | Implementation |
|---|---|
| `generateMetadata()` | Every page, per locale |
| OpenGraph | Landing, catalog, detail |
| Twitter Cards | Landing, detail |
| JSON-LD | Course (Course schema), Landing (Organization schema) |
| Sitemap | `app/sitemap.ts` — dynamic + localized |
| Robots | `app/robots.ts` |
| Canonical | Per locale |

### PWA
| Tool | Role |
|---|---|
| **next-pwa** | Offline caching, install prompt |

---

## Complete Monorepo Structure

```
frontend/
│
├── turbo.json
├── pnpm-workspace.yaml
├── package.json
│
├── apps/
│   ├── web/
│   │   ├── app/
│   │   │   ├── [locale]/                    ← en / ar
│   │   │   │   ├── (public)/
│   │   │   │   │   ├── layout.tsx           ← PublicHeader + Footer
│   │   │   │   │   ├── page.tsx             ← Landing
│   │   │   │   │   ├── courses/
│   │   │   │   │   │   ├── page.tsx
│   │   │   │   │   │   └── [id]/page.tsx
│   │   │   │   │   └── pricing/page.tsx
│   │   │   │   │
│   │   │   │   ├── (auth)/
│   │   │   │   │   ├── layout.tsx           ← Centered card
│   │   │   │   │   ├── login/page.tsx
│   │   │   │   │   ├── register/page.tsx
│   │   │   │   │   ├── forgot-password/page.tsx
│   │   │   │   │   ├── reset-password/page.tsx
│   │   │   │   │   ├── verify-email/page.tsx
│   │   │   │   │   └── 2fa/page.tsx
│   │   │   │   │
│   │   │   │   ├── (student)/
│   │   │   │   │   ├── layout.tsx           ← Sidebar(drawer on mobile) + Header
│   │   │   │   │   ├── dashboard/
│   │   │   │   │   │   ├── page.tsx
│   │   │   │   │   │   └── loading.tsx
│   │   │   │   │   ├── learn/[courseId]/
│   │   │   │   │   │   ├── page.tsx
│   │   │   │   │   │   ├── loading.tsx
│   │   │   │   │   │   ├── exam/[examId]/page.tsx
│   │   │   │   │   │   └── homework/[id]/page.tsx
│   │   │   │   │   ├── judge/
│   │   │   │   │   │   ├── page.tsx
│   │   │   │   │   │   ├── loading.tsx
│   │   │   │   │   │   └── [challengeId]/page.tsx
│   │   │   │   │   ├── live/
│   │   │   │   │   │   ├── page.tsx
│   │   │   │   │   │   └── [sessionId]/page.tsx
│   │   │   │   │   ├── notifications/page.tsx
│   │   │   │   │   ├── profile/page.tsx
│   │   │   │   │   ├── settings/page.tsx
│   │   │   │   │   └── billing/page.tsx
│   │   │   │   │
│   │   │   │   ├── (teacher)/
│   │   │   │   │   ├── layout.tsx           ← TeacherSidebar + Header
│   │   │   │   │   ├── dashboard/page.tsx
│   │   │   │   │   └── courses/
│   │   │   │   │       ├── page.tsx
│   │   │   │   │       ├── new/page.tsx
│   │   │   │   │       └── [id]/
│   │   │   │   │           ├── edit/page.tsx
│   │   │   │   │           └── modules/page.tsx
│   │   │   │   │
│   │   │   │   ├── checkout/
│   │   │   │   │   ├── page.tsx
│   │   │   │   │   ├── success/page.tsx
│   │   │   │   │   ├── failed/page.tsx
│   │   │   │   │   └── pending/page.tsx
│   │   │   │   │
│   │   │   │   └── layout.tsx               ← next-intl provider + dir attr
│   │   │   │
│   │   │   ├── api/uploadthing/route.ts
│   │   │   ├── sitemap.ts
│   │   │   ├── robots.ts
│   │   │   ├── manifest.ts
│   │   │   ├── layout.tsx                   ← Root: html, Sentry, Providers
│   │   │   ├── globals.css
│   │   │   ├── not-found.tsx
│   │   │   ├── error.tsx
│   │   │   ├── forbidden.tsx
│   │   │   ├── unauthorized.tsx
│   │   │   └── maintenance.tsx
│   │   │
│   │   ├── components/
│   │   │   ├── layout/
│   │   │   │   ├── StudentSidebar.tsx       ← Vaul Drawer on mobile
│   │   │   │   ├── TeacherSidebar.tsx
│   │   │   │   ├── PublicHeader.tsx
│   │   │   │   ├── DashboardHeader.tsx
│   │   │   │   ├── MobileNav.tsx
│   │   │   │   ├── Footer.tsx
│   │   │   │   └── Breadcrumbs.tsx
│   │   │   ├── shared/
│   │   │   │   ├── PageHeader.tsx
│   │   │   │   ├── SectionHeader.tsx
│   │   │   │   ├── PageContainer.tsx
│   │   │   │   ├── DataTableToolbar.tsx
│   │   │   │   ├── FilterBar.tsx
│   │   │   │   ├── StatCard.tsx
│   │   │   │   └── MetricCard.tsx
│   │   │   ├── marketing/
│   │   │   │   ├── Hero.tsx
│   │   │   │   ├── Features.tsx
│   │   │   │   ├── Pricing.tsx
│   │   │   │   ├── Testimonials.tsx
│   │   │   │   └── FAQ.tsx
│   │   │   ├── skeletons/
│   │   │   │   ├── CourseCardSkeleton.tsx
│   │   │   │   ├── DashboardSkeleton.tsx
│   │   │   │   ├── TableSkeleton.tsx
│   │   │   │   └── ProfileSkeleton.tsx
│   │   │   ├── empty/
│   │   │   │   ├── NoCourses.tsx
│   │   │   │   ├── NoNotifications.tsx
│   │   │   │   ├── NoResults.tsx
│   │   │   │   └── NoPayments.tsx
│   │   │   └── common/
│   │   │       ├── CommandPalette.tsx
│   │   │       ├── ThemeToggle.tsx
│   │   │       ├── LanguageSwitcher.tsx
│   │   │       └── UserMenu.tsx
│   │   │
│   │   ├── features/
│   │   │   ├── auth/
│   │   │   ├── courses/
│   │   │   ├── learning/
│   │   │   ├── judge/
│   │   │   ├── live-sessions/
│   │   │   ├── notifications/
│   │   │   ├── billing/
│   │   │   │   ├── checkout/
│   │   │   │   ├── payment/
│   │   │   │   ├── subscriptions/
│   │   │   │   └── invoices/
│   │   │   └── profile/
│   │   │
│   │   ├── guards/
│   │   │   ├── RoleGuard.tsx
│   │   │   ├── PermissionGuard.tsx
│   │   │   └── FeatureGuard.tsx
│   │   │
│   │   ├── hooks/
│   │   ├── lib/
│   │   ├── providers/
│   │   ├── store/
│   │   ├── i18n/
│   │   │   ├── messages/en.json
│   │   │   └── messages/ar.json
│   │   ├── middleware.ts
│   │   ├── next.config.ts
│   │   └── package.json
│   │
│   └── admin/
│       ├── app/[locale]/
│       │   ├── page.tsx
│       │   ├── users/page.tsx
│       │   ├── courses/page.tsx
│       │   ├── audit-logs/page.tsx
│       │   ├── subscriptions/page.tsx
│       │   └── layout.tsx
│       ├── components/
│       ├── features/
│       ├── guards/
│       ├── middleware.ts
│       └── package.json
│
└── packages/
    ├── ui/
    ├── design-system/
    ├── api/
    │   └── src/
    │       ├── client.ts
    │       ├── auth/     { api.ts, hooks.ts, keys.ts, types.ts }
    │       ├── courses/  { api.ts, hooks.ts, keys.ts, types.ts }
    │       ├── learning/ { api.ts, hooks.ts, keys.ts, types.ts }
    │       ├── judge/    { api.ts, hooks.ts, keys.ts, types.ts }
    │       ├── payments/ { api.ts, hooks.ts, keys.ts, types.ts }
    │       ├── users/    { api.ts, hooks.ts, keys.ts, types.ts }
    │       ├── notifications/ { api.ts, hooks.ts, keys.ts, types.ts }
    │       └── analytics/    { api.ts, hooks.ts, keys.ts, types.ts }
    ├── contracts/
    └── config/
        └── src/
            ├── routes.ts
            ├── navigation.ts
            ├── permissions.ts
            ├── roles.ts
            ├── api-urls.ts
            ├── pagination.ts
            ├── feature-flags.ts
            └── theme.ts
```

---

## Execution Phases

- [ ] **Phase 1** — Scaffold: Turborepo + pnpm + Next.js 16 (web + admin) + all packages
- [ ] **Phase 2** — Foundation: Axios, contracts, config, Zustand, middleware, guards, design tokens, error pages, SEO files
- [ ] **Phase 3** — Shared components: layouts, shared, skeletons, empty states
- [ ] **Phase 4** — Auth pages
- [ ] **Phase 5** — Landing + Public (responsive, SEO, Magic UI)
- [ ] **Phase 6** — Checkout / Payment flow (Paymob)
- [ ] **Phase 7** — Student app
- [ ] **Phase 8** — Judge (Monaco lazy)
- [ ] **Phase 9** — Teacher app (Tiptap + react-dropzone)
- [ ] **Phase 10** — Admin app
- [ ] **Phase 11** — Polish: i18n, RTL, responsive audit, Playwright, bundle analysis, a11y, PWA

---

## Verification

```bash
pnpm turbo run typecheck
pnpm turbo run lint
pnpm turbo run build
pnpm turbo run test:e2e         # 4 viewports: 375 / 768 / 1280 / 1920
pnpm --filter ui storybook
```
