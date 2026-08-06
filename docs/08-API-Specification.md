# 08 — API Specification

**Version:** 1.0
**Last updated:** 2026-08-03

---

## 1. Style & conventions

- **REST** over **GraphQL** (REST for writes & main flows, GraphQL for read-heavy
  dashboards with complex relations).
- **Base URL:** `https://api.platform.app`
- **Versioning:** URL segment `/api/v1/...` + header `X-API-Version`
- **Auth:** `Authorization: Bearer <jwt>` for users; `X-Api-Key` for service-to-service
- **Content-Type:** `application/json` (request & response) or `multipart/form-data` (uploads)
- **Idempotency:** `Idempotency-Key: <uuid>` header on POST/PUT
- **Errors:** RFC 7807 `application/problem+json`

```json
{
  "type": "https://api.platform.app/errors/payment-required",
  "title": "Subscription required",
  "status": 402,
  "code": "SUBSCRIPTION_REQUIRED",
  "detail": "You must purchase Month 1 to access this content.",
  "traceId": "9f1b…"
}
```

## 2. Common headers

| Header | Direction | Notes |
|---|---|---|
| `Authorization` | in | `Bearer <jwt>` |
| `Accept-Language` | in | `ar`, `en`, `fr` |
| `Idempotency-Key` | in | POST/PUT — replay safe |
| `X-Request-Id` | in/out | correlation id (generated if absent) |
| `X-Trace-Id` | out | OpenTelemetry trace id |
| `RateLimit-*` | out | per RFC draft-ietf-httpapi-ratelimit-headers |
| `Sunset` | out | when an endpoint is deprecated |

## 3. Pagination

Cursor-based:

```http
GET /api/v1/courses?limit=20&cursor=eyJpZCI6IjAx…
```

```json
{
  "data": [ ... ],
  "pagination": {
    "nextCursor": "eyJpZCI6IjAx…",
    "hasMore": true
  }
}
```

## 4. Auth endpoints (`/api/v1/auth`)

| Method | Path | Notes |
|---|---|---|
| POST | `/auth/register` | email + password, returns user + tokens |
| POST | `/auth/login` | returns access + refresh + user |
| POST | `/auth/login/oauth/{provider}` | Google / Microsoft; body = code |
| POST | `/auth/refresh` | rotates refresh token |
| POST | `/auth/logout` | revokes current refresh |
| POST | `/auth/logout-all` | revokes all user sessions |
| POST | `/auth/password/reset/request` | emails link |
| POST | `/auth/password/reset/confirm` | token + new password |
| POST | `/auth/email/verify/request` | resend |
| POST | `/auth/email/verify/confirm` | token |
| POST | `/auth/2fa/setup` | returns TOTP uri + qr (data url) |
| POST | `/auth/2fa/enable` | verify code, returns 10 backup codes |
| POST | `/auth/2fa/disable` | password + code |
| POST | `/auth/2fa/verify` | during login flow |
| GET  | `/auth/sessions` | list active sessions |
| DELETE | `/auth/sessions/{id}` | revoke specific |

## 5. Users & Profiles (`/api/v1/users`)

| Method | Path | Notes |
|---|---|---|
| GET | `/users/me` | current user |
| PATCH | `/users/me` | partial update |
| GET | `/users/me/profile` | |
| PATCH | `/users/me/profile` | |
| POST | `/users/me/avatar` | multipart, returns URL |
| GET | `/users/me/devices` | active sessions |
| DELETE | `/users/me` | GDPR delete (right to erase) |
| GET | `/users/{id}` | public profile (limited fields) |

## 6. Courses (`/api/v1/courses`)

| Method | Path | Notes |
|---|---|---|
| GET | `/courses` | filter by level, language, tag, price, free/paid; sort |
| POST | `/courses` | teacher only |
| GET | `/courses/{id}` | |
| PATCH | `/courses/{id}` | owner |
| DELETE | `/courses/{id}` | owner (soft) |
| POST | `/courses/{id}/publish` | owner |
| POST | `/courses/{id}/unpublish` | owner |
| POST | `/courses/{id}/cover` | owner, multipart |
| GET | `/courses/{id}/modules` | |
| POST | `/courses/{id}/modules` | |
| PATCH | `/courses/{id}/modules/{moduleId}` | |
| DELETE | `/courses/{id}/modules/{moduleId}` | |
| POST | `/courses/{id}/modules/reorder` | body: ordered module ids |
| GET | `/courses/{id}/modules/{moduleId}/lessons` | |
| POST | `/courses/{id}/modules/{moduleId}/lessons` | |
| GET | `/courses/{id}/lessons/{lessonId}` | includes versioned body |
| PATCH | `/courses/{id}/lessons/{lessonId}` | |
| DELETE | `/courses/{id}/lessons/{lessonId}` | |
| POST | `/courses/{id}/lessons/{lessonId}/reorder` | |
| GET | `/courses/{id}/lessons/{lessonId}/versions` | |
| POST | `/courses/{id}/lessons/{lessonId}/rollback/{version}` | |
| POST | `/courses/{id}/enroll` | self-enroll (free) |
| DELETE | `/courses/{id}/enroll` | unenroll |
| GET | `/courses/{id}/enrollments` | owner (teacher) |
| POST | `/courses/{id}/reviews` | student only |
| GET | `/courses/{id}/reviews` | paginated |

## 7. Monthly packages & subscriptions (`/api/v1`)

| Method | Path | Notes |
|---|---|---|
| GET | `/courses/{id}/packages` | list months for course |
| POST | `/courses/{id}/packages` | teacher creates Month N |
| PATCH | `/courses/{id}/packages/{pkgId}` | teacher updates |
| DELETE | `/courses/{id}/packages/{pkgId}` | soft |
| GET | `/subscriptions/me` | active packages + summary (isSubscribed flag) |
| GET | `/subscriptions/me/access/{courseId}` | access matrix per package |
| POST | `/payments/orders` | body: `{ packageId, couponCode?, referralCode? }` |
| GET | `/payments/orders/{id}` | |
| GET | `/payments/orders` | user's history |
| POST | `/payments/orders/{id}/cancel` | if pending |
| POST | `/webhooks/paymob` | provider → us; HMAC validated |
| GET | `/payments/invoices` | user |
| GET | `/payments/invoices/{id}/pdf` | signed URL |
| POST | `/payments/refunds` | platform admin only |
| GET | `/coupons/validate` | `?code=X&packageId=Y` |

## 8. Live sessions (`/api/v1/live`)

| Method | Path | Notes |
|---|---|---|
| GET | `/live/sessions` | filter course, month, time range |
| POST | `/live/sessions` | teacher only — body selects provider |
| GET | `/live/sessions/{id}` | metadata + join availability |
| PATCH | `/live/sessions/{id}` | teacher |
| DELETE | `/live/sessions/{id}` | teacher (cancels) |
| POST | `/live/sessions/{id}/start` | teacher — opens for joining |
| POST | `/live/sessions/{id}/end` | teacher — closes |
| POST | `/live/sessions/{id}/join` | returns brokered URL + short-lived token |
| GET | `/live/sessions/{id}/attendance` | teacher |
| GET | `/live/sessions/{id}/recording` | teacher; signed URL when available |
| POST | `/live/sessions/{id}/ics` | calendar invite for student |

## 9. Library & attachments (`/api/v1/library`)

| Method | Path | Notes |
|---|---|---|
| POST | `/library/attachments` | teacher — initiate upload (returns presigned R2 URL) |
| POST | `/library/attachments/{id}/complete` | teacher — finalize (transcode webhook) |
| GET | `/library/attachments/{id}/url` | signed URL (5 min) |
| DELETE | `/library/attachments/{id}` | teacher (soft) |
| GET | `/library/my` | student's accessible files |
| GET | `/library/{id}/progress` | last position, completed |
| POST | `/library/{id}/progress` | body: `{ positionSeconds, completed }` |

## 10. Assessments (`/api/v1/assessments`)

| Method | Path | Notes |
|---|---|---|
| GET | `/assessments` | filter by course, kind, status |
| POST | `/assignments` | teacher |
| GET | `/assignments/{id}` | (RBAC) |
| PATCH | `/assignments/{id}` | teacher |
| DELETE | `/assignments/{id}` | teacher |
| POST | `/exams` | teacher |
| GET | `/exams/{id}` | (start info) |
| POST | `/exams/{id}/start` | student — returns attempt |
| POST | `/exams/{id}/submit` | body: answers |
| POST | `/quizzes` | teacher |
| GET | `/quizzes/{id}` | |
| POST | `/quizzes/{id}/submit` | |
| GET | `/submissions/me` | student's |
| GET | `/submissions/{id}` | owner / teacher / parent |
| GET | `/assessments/{id}/submissions` | teacher queue |
| POST | `/submissions/{id}/grade` | teacher (manual) |
| POST | `/submissions/{id}/comments` | teacher (inline) |
| GET | `/submissions/{id}/history` | version history |
| GET | `/question-banks` | teacher |
| POST | `/question-banks` | teacher |
| GET | `/question-banks/{id}/questions` | teacher |
| POST | `/question-banks/{id}/questions` | teacher |
| POST | `/question-banks/{id}/questions/import` | CSV |
| POST | `/exams/{id}/generate` | random exam from bank |

## 11. Coding (`/api/v1/coding`)

| Method | Path | Notes |
|---|---|---|
| GET | `/problems` | filter language, difficulty, tag |
| POST | `/problems` | teacher |
| GET | `/problems/{id}` | includes sample I/O |
| PATCH | `/problems/{id}` | teacher |
| DELETE | `/problems/{id}` | teacher |
| POST | `/problems/{id}/testcases` | teacher |
| POST | `/submissions` | `{ problemId, language, source }` → enqueued |
| GET | `/submissions/{id}` | status polling |
| GET | `/submissions/me` | my history |
| GET | `/submissions/me/best` | best per problem |

## 12. Notifications (`/api/v1/notifications`)

| Method | Path | Notes |
|---|---|---|
| GET | `/notifications` | inbox |
| POST | `/notifications/{id}/read` | |
| POST | `/notifications/read-all` | |
| GET | `/notifications/preferences` | |
| PATCH | `/notifications/preferences` | |
| WS | `/hubs/notifications` | SignalR — real-time push |

## 13. Analytics (`/api/v1/analytics`)

| Method | Path | Notes |
|---|---|---|
| GET | `/analytics/teacher/overview` | revenue, students, engagement |
| GET | `/analytics/teacher/courses/{id}` | per course |
| GET | `/analytics/teacher/courses/{id}/heatmap` | lesson drop-off |
| GET | `/analytics/student/me` | progress, time-on-task |
| GET | `/analytics/admin/overview` | platform-wide |
| GET | `/analytics/admin/cohorts` | retention |
| GET | `/leaderboards/courses/{id}` | |
| GET | `/leaderboards/platform` | |

## 14. Gamification (`/api/v1/gamification`)

| Method | Path | Notes |
|---|---|---|
| GET | `/gamification/me` | xp, level, streak, badges |
| GET | `/gamification/me/xp` | history |
| GET | `/gamification/me/badges` | |
| GET | `/gamification/me/streak` | |
| GET | `/gamification/me/certificates` | |
| GET | `/certificates/{code}` | public verify (no auth) |
| GET | `/certificates/{code}/pdf` | |

## 15. WhatsApp Parent Notifications (`/api/v1/parent-notifications`)

| Method | Path | Notes |
|---|---|---|
| GET | `/parent-notifications/settings` | Teacher fetches global notification settings |
| PUT | `/parent-notifications/settings` | Teacher updates global notification settings |
| POST | `/parent-notifications/broadcast` | Teacher sends broadcast message to all/selected parents |
| GET | `/parent-notifications/students/{id}` | Fetch notification settings for specific student |
| PUT | `/parent-notifications/students/{id}` | Update notification settings for specific student |
| POST | `/parent-notifications/reports/trigger` | Manually trigger weekly/monthly reports |

## 16. Admin (`/api/v1/admin`)

| Method | Path | Notes |
|---|---|---|
| GET | `/admin/users` | filter & search |
| GET | `/admin/users/{id}` | |
| PATCH | `/admin/users/{id}/status` | suspend / activate |
| POST | `/admin/users/{id}/impersonate` | support only |
| GET | `/admin/teachers/applications` | pending |
| POST | `/admin/teachers/{id}/approve` | |
| POST | `/admin/teachers/{id}/reject` | |
| GET | `/admin/courses` | |
| GET | `/admin/payments` | |
| POST | `/admin/refunds` | |
| GET | `/admin/audit-logs` | search & filter |
| GET | `/admin/feature-flags` | |
| PATCH | `/admin/feature-flags/{key}` | |
| GET | `/admin/announcements` | |
| POST | `/admin/announcements` | |

## 17. Webhooks (inbound)

| Provider | Path | Signature |
|---|---|---|
| Paymob | `POST /api/v1/webhooks/paymob` | HMAC SHA-256, header `X-Paymob-Signature` |
| Google Meet | `POST /api/v1/webhooks/google` | OAuth2 + signed JWT |
| Microsoft Teams | `POST /api/v1/webhooks/teams` | `validationToken` handshake + signing |
| Cloudflare R2 | `POST /api/v1/webhooks/r2` | HMAC for object events (transcode complete) |

All inbound webhooks are processed via the **outbox pattern** for exactly-once
semantics, with `provider_event_id` as the idempotency key.

## 18. GraphQL surface (`/graphql`)

Read-heavy, relation-rich queries live in GraphQL. Examples:

```graphql
query TeacherOverview($teacherId: ID!) {
  teacher(id: $teacherId) {
    revenue(last: 30, unit: DAYS)
    courses {
      id
      title
      enrolled
      ratingAvg
      sessions(upcoming: true) { id, startAt, provider }
    }
    topPerformingStudents(limit: 10) { id, displayName, xp }
  }
}
```

```graphql
query StudentDashboard {
  me {
    xp
    level
    streak
    activePackages { id, course { id, title }, expiresAt }
    upcomingSessions { id, course { title }, startAt, provider }
    pendingAssignments(limit: 5) { id, title, dueAt }
  }
}
```

## 19. Rate limits

| Bucket | Limit |
|---|---|
| Auth (per IP) | 10 / min |
| Auth (per email) | 5 / 15 min |
| Reads (per user) | 600 / min |
| Writes (per user) | 120 / min |
| Code submissions (per user) | 30 / hour (configurable per problem) |
| Webhooks (per provider) | 1000 / min |

Returns `429` with `Retry-After` and `RateLimit-Reset`.

## 20. OpenAPI

Full machine-readable spec is generated from the C# controllers via
**Swashbuckle** + **NSwag** and committed under `backend/src/Platform.Api/OpenApi/`.
The frontend consumes it via `orval` to generate a typed client.
