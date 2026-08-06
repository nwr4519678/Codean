# 02 — Functional Requirements

**Version:** 1.0
**Status:** Approved for build
**Last updated:** 2026-08-03

> Each requirement has a stable ID (`FR-XXX`) used in the test plan and traceability matrix.

---

## 1. Authentication & Account

| ID | Requirement |
|---|---|
| FR-001 | Users register with email + password (BCrypt hash, cost ≥ 12) |
| FR-002 | Users log in with **email + password**, **Google OAuth**, or **Microsoft OAuth** |
| FR-003 | Password reset via email link (single-use, 30-min TTL) |
| FR-004 | Email verification mandatory before paid features unlock |
| FR-005 | Optional **TOTP 2FA** with backup codes (10 single-use) |
| FR-006 | "Remember me" extends refresh-token lifetime to 30 days |
| FR-007 | Users can list & revoke active sessions/devices |
| FR-008 | Failed-login lockout after 5 attempts in 15 min (per IP + per email) |
| FR-009 | All auth events written to `audit_logs` (login, logout, 2FA, password change) |
| FR-010 | JWT access token (15 min, RS256) + refresh token (rotated, 30 d) |
| FR-011 | Logout revokes refresh token immediately |
| FR-012 | Support agents can impersonate users (with explicit consent banner + audit) |

## 2. Roles & Permissions

| ID | Requirement |
|---|---|
| FR-020 | Roles: `Support`, `Student`, `Teacher`, `TeacherAssistant`, `SchoolAdmin`, `PlatformAdmin`, `SuperAdmin` |
| FR-021 | Permissions are policy-based (not hard-coded in controllers) |
| FR-022 | Role assignments are scoped: `global`, `school`, `course` |
| FR-023 | SuperAdmin can grant/revoke any permission |
| FR-024 | Permissions are cached in Redis; invalidation on change is event-driven |

## 3. Courses & Content

| ID | Requirement |
|---|---|
| FR-030 | Teachers create **Courses** with title, description, cover, tags, level |
| FR-031 | A course is a tree of **Modules → Lessons → Units** |
| FR-032 | A lesson may contain: markdown, video, PDF, PPTX, images, code blocks, Mermaid diagrams, interactive blocks, whiteboard, downloads |
| FR-033 | Lessons can be reordered via drag-and-drop; ordering persisted atomically |
| FR-034 | Teachers can mark lesson visibility: `free`, `subscribed`, `course-specific`, `group-specific` |
| FR-035 | A lesson can have prerequisites (other lessons in the same course) |
| FR-036 | Authors see revisions; rollback to any prior version is one click |
| FR-037 | Bulk import/export of content as JSON / SCORM-lite (v2) |
| FR-038 | Content is searchable (Elasticsearch) with typo tolerance |

## 4. Monthly Content Subscription

| ID | Requirement |
|---|---|
| FR-040 | A course is divided into **monthly packages** (Month 1, Month 2, ...) |
| FR-041 | A monthly package contains: live sessions, recorded videos, PDFs, assignments, exams, quizzes, resources |
| FR-042 | Students have a **binary** state: `Free` or `Subscribed` (per monthly package) |
| FR-043 | Each monthly package is **purchased independently** via Paymob |
| FR-044 | On payment success, the package activates and grants access for **365 days** |
| FR-045 | Purchased months remain accessible for the full year, even if user becomes Free again |
| FR-046 | Subscription expiry auto-reverts to `Free`; progress, grades, certificates are preserved |
| FR-047 | Coupons, promo codes, referral codes, scholarships are supported at checkout |
| FR-048 | Webhook signature (HMAC SHA-256) is validated before activating access |
| FR-049 | Each successful payment generates a PDF invoice and an email + in-app notification |
| FR-050 | Webhook is idempotent (same transaction ID processed exactly once) |

## 5. Live Sessions

| ID | Requirement |
|---|---|
| FR-060 | Teacher creates a live session: title, description, start time, duration, course/month, provider (`GoogleMeet` / `MicrosoftTeams`) |
| FR-061 | System creates the meeting via provider API and stores the **organizer URL** server-side only |
| FR-062 | Students see a "Join" button inside the platform 10 min before start time |
| FR-063 | Pressing Join opens a brokered URL that injects a short-lived (15 min) access token |
| FR-064 | Students must be (a) authenticated, (b) enrolled, (c) subscribed (if paid) to access |
| FR-065 | All join attempts (success + failure) are logged to `live_session_attendance` |
| FR-066 | Teacher can start/stop recording via provider API; recording URL is stored post-meeting |
| FR-067 | Calendar invites (`.ics`) are sent via email and added to in-app calendar |
| FR-068 | Reconnect logic: students who drop are auto-reissued a token on retry |
| FR-069 | If Meet fails, system falls back to Teams (configurable per teacher) |

## 6. Video Library

| ID | Requirement |
|---|---|
| FR-080 | Teachers upload: video, PDF, ZIP, PPTX, source code, images, files (≤ 2 GB per file) |
| FR-081 | Files are stored in **Cloudflare R2** with server-side encryption |
| FR-082 | Access is via signed URLs (5-min TTL) — no public bucket access |
| FR-083 | Visibility: `free`, `subscribed`, `course-specific`, `group-specific` |
| FR-084 | Per-video watch progress is tracked (last position + completed flag) |
| FR-085 | Video chapter markers are supported |
| FR-086 | Soft subtitles (WebVTT) supported; auto-generated captions in v2 |

## 7. Assignments

| ID | Requirement |
|---|---|
| FR-100 | Create: homework, coding homework, project, file submission, essay, MCQ, programming task |
| FR-101 | Hard deadlines + late-submission policy (none / grace period / capped %) |
| FR-102 | Automatic grading for MCQ + coding (against test cases) |
| FR-103 | Manual grading with rubrics (criteria + levels) |
| FR-104 | Inline comments on submissions |
| FR-105 | Version history (every re-submission stored) |
| FR-106 | Similarity detection (Moss-style fingerprinting) for text submissions |
| FR-107 | Plagiarism threshold + teacher review queue |

## 8. Exams

| ID | Requirement |
|---|---|
| FR-120 | Question bank: MCQ, T/F, programming, essay, fill-in-blank, matching, ordering |
| FR-121 | Question tagging (topic, difficulty, bloom level) |
| FR-122 | Random exam generation from bank with constraints (count, topics, difficulty mix) |
| FR-123 | Configurable timer (per exam, per section) |
| FR-124 | Anti-cheat: fullscreen enforcement, copy/paste lock, tab-switch detection, randomized question order, randomized answer order |
| FR-125 | Auto-grading (MCQ/T-F/matching/ordering/programming) + manual review queue |
| FR-126 | Detailed statistics per question (difficulty index, discrimination index) |
| FR-127 | PDF export of student exam result |
| FR-128 | Scheduled windows (open at, close at) |
| FR-129 | One-attempt or multi-attempt; best-of or last-attempt scoring |

## 9. Coding Environment

| ID | Requirement |
|---|---|
| FR-140 | In-browser editor powered by Monaco (VS Code engine) |
| FR-141 | Languages: Python, C, C++, Java, JavaScript, C# |
| FR-142 | IntelliSense / autocomplete per language |
| FR-144 | Per-submission CPU/memory/time limits (configurable per problem) |
| FR-145 | Test-case runner: pass/fail per case, output diff, expected vs actual |
| FR-147 | Auto-save drafts every 5 s |
| FR-148 | Submission history with verdict (AC, WA, TLE, MLE, RE, CE) |

## 10. Notifications

| ID | Requirement |
|---|---|
| FR-160 | Channels: in-app, email, push (web push via VAPID), SMS-ready, WhatsApp-ready |
| FR-161 | Per-user notification preferences (granular: per event type, per channel) |
| FR-162 | Templated notifications with i18n (ar, en, fr initially) |
| FR-163 | Batched digests (daily / weekly) to avoid spam |
| FR-164 | Real-time push via SignalR (live session start, new grade, etc.) |

## 11. Analytics

| ID | Requirement |
|---|---|
| FR-180 | Teacher analytics: revenue, subscribers, attendance, exam results, assignment completion, engagement, watch time, course popularity, retention, conversion |
| FR-181 | Student analytics: progress, time-on-task, streak, accuracy by topic |
| FR-182 | Admin analytics: platform-wide metrics, cohort retention, top courses |
| FR-183 | Heatmaps: which lessons cause drop-off |
| FR-184 | Leaderboards: course, school, platform |
| FR-185 | Export to CSV / PDF |

## 12. Gamification

| ID | Requirement |
|---|---|
| FR-200 | XP earned for: lesson completion, exercise pass, streak, perfect exam, peer help |
| FR-201 | Levels derived from XP (configurable curve) |
| FR-202 | Badges: course mastery, streak, first-100, etc. (extensible) |
| FR-203 | Daily streak counter (grace day 1/week) |
| FR-204 | Weekly challenges with bonus XP |
| FR-205 | Certificates (PDF) on course completion, signed & verifiable via public URL |
| FR-206 | Rewards store (v2) — redeem XP for coupons, free months |

## 13. WhatsApp Parent Notification System

| ID | Requirement |
|---|---|
| FR-220 | Collect 1-2 parent/guardian phone numbers during student registration |
| FR-221 | Teacher can toggle parent notifications globally or per student |
| FR-222 | Automated WhatsApp alerts: missed live class, upcoming homework, missing homework, exam schedules, exam results |
| FR-223 | Automated WhatsApp alerts: performance drops, new lessons/recordings, subscription expiring/renewed, payments |
| FR-224 | Automated reports (weekly/monthly) summarizing attendance, homework, exams, progress, upcoming events, teacher notes |
| FR-225 | Teacher broadcast controls: send to all parents or selected parents |
| FR-226 | Teacher can customize report templates and frequency |

## 14. Admin Panel

| ID | Requirement |
|---|---|
| FR-240 | Manage users, teachers, students, courses, payments, subscriptions, packages, coupons, announcements, logs, settings |
| FR-241 | Moderation queue for reported content |
| FR-242 | Refund flow (full / partial) with audit |
| FR-243 | System settings (feature flags, payment keys, email templates) |
| FR-244 | Audit log search with filters |

## 15. Internationalization & Accessibility

| ID | Requirement |
|---|---|
| FR-260 | Languages: Arabic (RTL), English, French (v1) |
| FR-261 | RTL-aware UI (CSS logical properties) |
| FR-262 | WCAG 2.1 AA compliance: contrast, focus, keyboard, ARIA |
| FR-263 | Currency display follows user preference (EGP default for MEA) |

## 16. Offline & PWA

| ID | Requirement |
|---|---|
| FR-280 | PWA install on supported browsers |
| FR-281 | Offline read of previously viewed lessons (service worker cache) |
| FR-282 | Background sync for draft submissions (queued, sent when online) |

## 17. Search

| ID | Requirement |
|---|---|
| FR-300 | Global search across courses, lessons, announcements (Elasticsearch) |
| FR-301 | Filters: course, type, free/paid, language, level |
| FR-302 | Typo tolerance + language-aware analyzer (Arabic) |

## 18. Subscription State

| ID | Requirement |
|---|---|
| FR-320 | Single global flag per user: `Free` or `Subscribed` |
| FR-321 | Per-month flags: which monthly packages the user owns + their expiry |
| FR-322 | Access checks are policy-based, not hard-coded (`SubscriptionAccessRequirement`) |
