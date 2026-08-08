# Frontend architecture audit — 2026-08-07

## Scorecard

| Category | Score | Finding |
|---|---:|---|
| Architecture | 6/10 | Sensible workspace split, but planned feature boundaries are incomplete. |
| Scalability | 5/10 | React Query exists, yet pagination, prefetching, and feature APIs are inconsistent. |
| Maintainability | 5/10 | Shared primitives exist but pages duplicate UI and own data-state handling. |
| Performance | 6/10 | Monaco and Tiptap are lazy loaded; no measured bundle-budget enforcement. |
| UI consistency | 6/10 | Design tokens are present, but screens are at different levels of polish. |
| UX | 4/10 | Several forms/actions are simulated or static and have incomplete error/empty states. |
| Accessibility | 5/10 | Some semantic structure exists; no comprehensive keyboard, focus, or responsive testing. |
| API integration | 4/10 | Corrected routes in this pass; several required backend capabilities are not exposed. |
| Security | 5/10 | Refresh queuing exists, but tokens remain in localStorage and route authorization is client-side. |
| Responsive design | 6/10 | Major grids are responsive; coverage has not been verified at plan viewports. |
| Code quality | 6/10 | Strict TypeScript passes; no active ESLint/Oxlint configuration. |
| Developer experience | 5/10 | Turborepo/typecheck/build work; Storybook, lint, and E2E command coverage are absent. |

## Plan comparison

Completed or substantially present: workspace packages, Next App Router, locale prefixes/RTL document direction, TanStack Query provider, theme provider, API client, core auth routes, course detail/list, basic role guards, error/not-found pages, sitemap/robots/manifest, and dynamic Monaco/Tiptap imports.

Partially implemented: responsive layouts, auth refresh, SEO metadata (only landing page), skeletons, teacher pages, admin pages, payments, localization, feature/permission guards, React Query mutations, PWA, analytics/observability, and Playwright.

Missing: shared public header/footer, pricing/testimonial/FAQ components, checkout failed/pending states, exam/homework routes, a notification empty state, table skeletons, profile skeleton, dedicated error boundaries per route, all planned admin i18n routing, Storybook, MSW, Sentry/PostHog initialization, PWA integration, bundle analyzer enforcement, and four-viewport E2E coverage.

## Backend integration audit

Correctly connected after this pass: auth endpoints, courses, progress, audit logs, user listing/role updates, analytics overview, individual challenge details/submissions, subscription plans, checkout initiation, notifications, and public profile lookup.

Corrected frontend route mappings: student/teacher profile paths, challenge/submission paths, plans, checkout, Paymob webhook path, and notification `me`/read paths. Checkout now sends only `planId`, the only field accepted by the backend.

Backend capabilities not currently exposed for the planned UI: a coding-challenge catalog, student-specific course enrollment, course-module listing by course, student live-session listing/detail, admin subscription reporting, and current-user profile GET routes. The frontend deliberately no longer substitutes fake data for the challenge catalog.

DTO mismatch risks: subscription-plan, checkout, and subscription response contracts in `packages/contracts` do not match the current backend response shapes. They were not regenerated or changed, per the backend/contract constraint; live API verification should precede wiring their full UI fields.

## Changes made in this pass

- Fixed immutable backend route consumption in `packages/config` and API clients.
- Removed fabricated challenge, audit-log, user fallback data; empty states now render instead.
- Replaced admin overview fabricated metrics with the existing analytics query.
- Hardened Axios 401 refresh flow against missing request config and refresh-loop retries.
- Removed the unsafe challenge-list request because no corresponding backend endpoint exists.

## Verification

`pnpm --dir frontend typecheck`, web production build, and admin production build passed on 2026-08-07. The web landing route first-load JavaScript is 108 kB in the production build output, under the plan's 150 kB target.

## Prioritized remaining work

1. Add the missing backend read endpoints or explicitly remove the dependent product screens from scope; the frontend cannot truthfully implement them without a source of data.
2. Align versioned frontend contracts with the backend through the approved contract workflow, then complete plans/checkout/subscription UI.
3. Replace the remaining static/simulated screens (pricing, checkout selection, live sessions, admin courses/subscriptions, settings and teacher editing flows) with existing API-backed or explicit unavailable states.
4. Add error boundaries, reusable skeleton/empty-state primitives, debounced search/pagination, accessibility tests, lint tooling, and the required E2E viewports.
