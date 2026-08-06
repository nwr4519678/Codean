# 06 — Database Design

**Engine:** PostgreSQL 16
**Version:** 1.0
**Last updated:** 2026-08-03

---

## 1. Conventions

- **Naming:** `snake_case` for tables & columns; PascalCase in C# mapped via EF Core
- **PKs:** `uuid` (v7 in v2) for distributed insert scalability; `bigint` only for high-volume append-only tables (`audit_logs`, `events_outbox`)
- **Time:** all timestamps `timestamptz` UTC; `created_at`, `updated_at`, `deleted_at` (soft delete)
- **Money:** `numeric(18,4)` + ISO 4217 currency code; never floats
- **Enums:** Postgres native `enum` types
- **JSON:** `jsonb` for semi-structured (provider payloads, settings) with GIN indexes when queried
- **Soft delete:** `deleted_at` + filtered indexes; hard delete only for GDPR erase
- **Audit:** every table has `created_at`, `created_by`, `updated_at`, `updated_by`

## 2. Schema layout

- **Schemas:** `auth`, `identity`, `courses`, `content`, `commerce`, `live`, `assessments`, `coding`, `social`, `gamification`, `admin`, `audit`
- **Read replicas:** tables that are read-heavy get materialized views refreshed every 5 min (leaderboards, analytics)

## 3. Core tables (excerpt — full DDL in migrations)

### 3.1 `identity.users`

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `email` | citext | unique, citext for case-insensitive |
| `email_normalized` | citext | lower(email), indexed |
| `email_verified_at` | timestamptz | nullable |
| `password_hash` | text | BCrypt |
| `two_factor_enabled` | bool | |
| `two_factor_secret` | text | encrypted at rest (PG TDE) |
| `locale` | text | default 'en' |
| `timezone` | text | default 'Africa/Cairo' |
| `status` | identity.user_status | enum: Active, Suspended, Deactivated |
| `last_login_at` | timestamptz | |
| `created_at` | timestamptz | |
| `updated_at` | timestamptz | |
| `deleted_at` | timestamptz | soft delete |

Indexes: `email_normalized` (unique partial where `deleted_at is null`), `status`

### 3.2 `identity.user_profiles`

| Column | Type | Notes |
|---|---|---|
| `user_id` | uuid | PK, FK |
| `first_name` | text | |
| `last_name` | text | |
| `display_name` | text | |
| `avatar_url` | text | R2 signed |
| `bio` | text | |
| `birth_date` | date | nullable |
| `country_code` | char(2) | |

### 3.3 `identity.roles` / `identity.user_roles`

Standard RBAC tables. Roles are global but can be scoped:
- `identity.user_roles`: `(user_id, role, scope_type, scope_id)`
  - `scope_type` ∈ `global`, `school`, `course`
  - Example: user has `Teacher` global, `TeacherAssistant` scoped to course X

### 3.4 `identity.user_logins` (OAuth)

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `user_id` | uuid | FK |
| `provider` | text | `google`, `microsoft` |
| `provider_user_id` | text | |
| `provider_email` | citext | |
| `created_at` | timestamptz | |

Unique `(provider, provider_user_id)`.

### 3.5 `identity.refresh_tokens`

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `user_id` | uuid | FK |
| `token_hash` | text | SHA-256, unique |
| `issued_at` | timestamptz | |
| `expires_at` | timestamptz | |
| `revoked_at` | timestamptz | |
| `replaced_by` | uuid | rotation chain |
| `user_agent` | text | |
| `ip` | inet | |

Index: `(user_id, revoked_at)`, unique on `token_hash`.

### 3.6 `identity.audit_logs`

| Column | Type | Notes |
|---|---|---|
| `id` | bigserial | PK (high volume) |
| `actor_user_id` | uuid | nullable (system events) |
| `action` | text | e.g. `user.login.success` |
| `subject_type` | text | e.g. `Order` |
| `subject_id` | text | |
| `metadata` | jsonb | |
| `ip` | inet | |
| `user_agent` | text | |
| `created_at` | timestamptz | default now() |

Partitioned monthly. Retained 365 days hot, then archived to S3/R2.

## 4. Courses & Content

### 4.1 `courses.courses`

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `teacher_id` | uuid | FK |
| `school_id` | uuid | nullable, FK |
| `slug` | text | unique |
| `title` | text | |
| `subtitle` | text | |
| `description` | text | |
| `cover_url` | text | R2 |
| `language` | text | ISO 639-1 |
| `level` | courses.level | Beginner, Intermediate, Advanced |
| `status` | courses.status | Draft, Published, Archived |
| `tags` | text[] | |
| `enrollment_count` | int | denormalized |
| `rating_avg` | numeric(3,2) | |
| `rating_count` | int | |
| `published_at` | timestamptz | nullable |
| `created_at`/`updated_at`/`deleted_at` | timestamptz | |

Indexes: `teacher_id`, `status, published_at desc`, GIN on `tags`, trigram on `title`.

### 4.2 `courses.modules`

| Column | Type |
|---|---|
| `id` | uuid |
| `course_id` | uuid FK |
| `title` | text |
| `order` | int |
| `month_number` | smallint | ← which month this module belongs to |
| `created_at`/`updated_at` | timestamptz |

Unique `(course_id, order)`.

### 4.3 `courses.lessons`

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `module_id` | uuid | FK |
| `title` | text | |
| `order` | int | |
| `kind` | courses.lesson_kind | Video, Reading, Quiz, Coding, Mixed |
| `visibility` | courses.visibility | Free, Subscribed, CourseSpecific, GroupSpecific |
| `estimated_minutes` | smallint | |
| `prerequisites` | uuid[] | |
| `created_at`/`updated_at` | timestamptz | |

### 4.4 `content.lesson_versions`

Append-only — full snapshot per save. Cheap because content is small + compressed.

| Column | Type |
|---|---|
| `id` | uuid |
| `lesson_id` | uuid |
| `version` | int |
| `body_md` | text |
| `body_json` | jsonb | ← structured blocks (Mermaid, code, video refs) |
| `created_by` | uuid |
| `created_at` | timestamptz |

Unique `(lesson_id, version)`.

### 4.5 `content.attachments`

| Column | Type |
|---|---|
| `id` | uuid |
| `lesson_id` | uuid |
| `kind` | content.attachment_kind | Video, PDF, PPTX, ZIP, Image, Source |
| `storage_key` | text | R2 key |
| `size_bytes` | bigint |
| `duration_seconds` | int | for video |
| `checksum` | text | sha256 |
| `created_at` | timestamptz |

### 4.6 `courses.enrollments`

| Column | Type |
|---|---|
| `id` | uuid |
| `user_id` | uuid |
| `course_id` | uuid |
| `enrolled_at` | timestamptz |
| `source` | text | free, paid, scholarship, gift |
| `progress_pct` | numeric(5,2) |
| `last_active_at` | timestamptz |
| `completed_at` | timestamptz |

Unique `(user_id, course_id)`.

## 5. Commerce — Subscriptions, Orders, Payments

### 5.1 `commerce.monthly_packages`

A **monthly package** is the unit of sale. Each course has many (Month 1, Month 2, ...).

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `course_id` | uuid | FK |
| `month_number` | smallint | 1, 2, 3, ... |
| `title` | text | e.g. "Month 1 — Intro" |
| `price` | numeric(18,4) | |
| `currency` | char(3) | EGP |
| `access_duration_days` | smallint | default 365 |
| `status` | commerce.package_status | Draft, Active, Archived |
| `created_at`/`updated_at` | timestamptz | |

Unique `(course_id, month_number)`.

### 5.2 `commerce.user_package_access`

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `user_id` | uuid | FK |
| `package_id` | uuid | FK |
| `granted_at` | timestamptz | |
| `expires_at` | timestamptz | |
| `source` | commerce.access_source | Payment, Scholarship, Gift, Admin |
| `order_id` | uuid | nullable, FK |
| `revoked_at` | timestamptz | nullable |

Indexes: `(user_id, expires_at desc)`, `(user_id, package_id, revoked_at)`.

> **The "binary" subscription state** is derived:
> `user.is_subscribed = exists(access where now() < expires_at and revoked_at is null)`

### 5.3 `commerce.orders`

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `user_id` | uuid | FK |
| `package_id` | uuid | FK |
| `amount` | numeric(18,4) | |
| `currency` | char(3) | |
| `discount_amount` | numeric(18,4) | |
| `total_amount` | numeric(18,4) | |
| `status` | commerce.order_status | Pending, Paid, Failed, Refunded, Cancelled |
| `paymob_order_id` | text | nullable |
| `idempotency_key` | text | unique |
| `metadata` | jsonb | |
| `created_at`/`updated_at` | timestamptz | |

### 5.4 `commerce.payments`

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `order_id` | uuid | FK |
| `provider` | text | "paymob" |
| `provider_transaction_id` | text | unique, idempotency anchor |
| `method` | text | Card, Meeza, VodafoneCash, etc. |
| `amount` | numeric(18,4) | |
| `currency` | char(3) | |
| `status` | commerce.payment_status | |
| `raw_payload` | jsonb | full webhook body |
| `created_at` | timestamptz | |

### 5.5 `commerce.refunds`

| Column | Type |
|---|---|
| `id` | uuid |
| `payment_id` | uuid |
| `amount` | numeric(18,4) |
| `reason` | text |
| `requested_by` | uuid |
| `status` | commerce.refund_status |
| `created_at`/`updated_at` | timestamptz |

### 5.6 `commerce.coupons`

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `code` | citext | unique |
| `kind` | commerce.coupon_kind | Percent, Fixed |
| `value` | numeric(18,4) | |
| `applies_to` | text | course_id, teacher_id, or null (= any) |
| `max_uses` | int | nullable |
| `used_count` | int | |
| `valid_from` / `valid_to` | timestamptz | |
| `status` | commerce.coupon_status | Active, Paused, Expired |

### 5.7 `commerce.referral_codes`

| Column | Type |
|---|---|
| `id` | uuid |
| `user_id` | uuid (referrer) |
| `code` | citext unique |
| `reward_type` | text | Discount, Cash |
| `reward_value` | numeric(18,4) |
| `created_at` | timestamptz |

### 5.8 `commerce.invoices`

| Column | Type |
|---|---|
| `id` | uuid |
| `order_id` | uuid |
| `number` | text | e.g. INV-2026-000123 |
| `pdf_url` | text | R2 |
| `issued_at` | timestamptz |

## 6. Live Sessions

### 6.1 `live.sessions`

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `course_id` | uuid | FK |
| `teacher_id` | uuid | FK |
| `month_number` | smallint | |
| `title` | text | |
| `description` | text | |
| `provider` | live.provider | GoogleMeet, MicrosoftTeams |
| `organizer_url` | text | **server-side only** |
| `start_at` | timestamptz | |
| `end_at` | timestamptz | |
| `status` | live.session_status | Scheduled, Live, Ended, Cancelled |
| `recording_url` | text | nullable |

### 6.2 `live.attendance`

| Column | Type |
|---|---|
| `id` | uuid |
| `session_id` | uuid |
| `user_id` | uuid |
| `joined_at` | timestamptz |
| `left_at` | timestamptz |
| `duration_seconds` | int |
| `ip` | inet |
| `user_agent` | text |
| `outcome` | live.attendance_outcome | Allowed, Denied |

Index: `(session_id, user_id)`, `(user_id, joined_at desc)`.

## 7. Assessments

### 7.1 `assessments.assignments` / `assessments.exams` / `assessments.quizzes`

Common shape:

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `course_id` | uuid | FK |
| `module_id` | uuid | nullable, FK |
| `lesson_id` | uuid | nullable, FK |
| `title` | text | |
| `kind` | text | homework, project, exam, quiz |
| `description_md` | text | |
| `total_points` | numeric(8,2) | |
| `open_at` / `close_at` | timestamptz | |
| `time_limit_minutes` | int | nullable |
| `attempts_allowed` | int | |
| `grading_method` | text | auto, manual, hybrid |
| `settings` | jsonb | anti-cheat flags, randomization, etc. |
| `created_at`/`updated_at` | timestamptz | |

### 7.2 `assessments.questions` (question bank)

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `bank_id` | uuid | which bank |
| `kind` | assessments.question_kind | MCQ, TF, Programming, Essay, FIB, Matching, Ordering |
| `prompt_md` | text | |
| `prompt_json` | jsonb | structured for each kind |
| `correct_answer` | jsonb | for auto-graded |
| `points` | numeric(8,2) | |
| `difficulty` | smallint | 1–5 |
| `bloom_level` | text | remember, understand, ... |
| `topic_tags` | text[] | |
| `created_by` | uuid | |
| `created_at`/`updated_at` | timestamptz | |

### 7.3 `assessments.submissions`

| Column | Type |
|---|---|
| `id` | uuid |
| `assessment_id` | uuid |
| `user_id` | uuid |
| `attempt_number` | int |
| `submitted_at` | timestamptz |
| `status` | assessments.submission_status | Draft, Submitted, Graded, Returned |
| `auto_score` | numeric(8,2) nullable |
| `manual_score` | numeric(8,2) nullable |
| `final_score` | numeric(8,2) nullable |
| `grader_user_id` | uuid nullable |
| `graded_at` | timestamptz nullable |
| `rubric_scores` | jsonb nullable |
| `comments_md` | text nullable |
| `similarity_score` | numeric(5,2) nullable |
| `created_at`/`updated_at` | timestamptz |

### 7.4 `assessments.answers` (per question in a submission)

| Column | Type |
|---|---|
| `id` | uuid |
| `submission_id` | uuid |
| `question_id` | uuid |
| `answer_json` | jsonb |
| `is_correct` | bool nullable |
| `points_awarded` | numeric(8,2) |
| `comments` | text |

## 8. Coding

### 8.1 `coding.problems`

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `title` | text | |
| `statement_md` | text | |
| `languages` | text[] | allowed languages |
| `time_limit_ms` | int | |
| `memory_limit_mb` | int | |
| `created_by` | uuid | |
| `created_at`/`updated_at` | timestamptz | |

### 8.2 `coding.test_cases`

| Column | Type |
|---|---|
| `id` | uuid |
| `problem_id` | uuid |
| `order` | int |
| `input` | text |
| `expected_output` | text |
| `is_sample` | bool |
| `weight` | numeric(5,2) |

### 8.3 `coding.submissions`

| Column | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `user_id` | uuid | |
| `problem_id` | uuid | |
| `language` | text | |
| `source_code` | text | |
| `verdict` | coding.verdict | AC, WA, TLE, MLE, RE, CE, Queued, Running |
| `runtime_ms` | int | |
| `memory_kb` | int | |
| `test_results` | jsonb | per-case |
| `submitted_at` | timestamptz | |

Index: `(user_id, problem_id, submitted_at desc)`.

## 9. Gamification

### 9.1 `gamification.xp_events` (append-only)

| Column | Type |
|---|---|
| `id` | bigserial |
| `user_id` | uuid |
| `event_type` | text |
| `xp` | int |
| `source_id` | text | e.g. lesson_id |
| `created_at` | timestamptz |

Partitioned monthly.

### 9.2 `gamification.user_levels` (materialized)

| Column | Type |
|---|---|
| `user_id` | uuid PK |
| `xp_total` | bigint |
| `level` | int |
| `updated_at` | timestamptz |

### 9.3 `gamification.user_badges`

| Column | Type |
|---|---|
| `user_id` | uuid |
| `badge_code` | text |
| `awarded_at` | timestamptz |

PK `(user_id, badge_code)`.

### 9.4 `gamification.streaks`

| Column | Type |
|---|---|
| `user_id` | uuid PK |
| `current_streak` | int |
| `longest_streak` | int |
| `last_active_date` | date |
| `grace_used_this_week` | bool |

### 9.5 `gamification.certificates`

| Column | Type |
|---|---|
| `id` | uuid |
| `user_id` | uuid |
| `course_id` | uuid |
| `verification_code` | text unique |
| `pdf_url` | text |
| `issued_at` | timestamptz |

## 10. Notifications

### 10.1 `notifications.notifications`

| Column | Type |
|---|---|
| `id` | uuid |
| `user_id` | uuid |
| `kind` | text |
| `title` | text |
| `body` | text |
| `payload` | jsonb |
| `read_at` | timestamptz nullable |
| `created_at` | timestamptz |

### 10.2 `notifications.preferences`

| Column | Type |
|---|---|
| `user_id` | uuid |
| `event_type` | text |
| `in_app` | bool |
| `email` | bool |
| `push` | bool |
| `sms` | bool |

PK `(user_id, event_type)`.

## 11. Search

Search is Elasticsearch-side, mirrored by triggers from Postgres:
- `courses`, `lessons`, `users` (display name only)
- Indexed async via outbox → Kafka (v2) or Hangfire (v1)

## 12. Indexes (high-value)

```sql
-- commerce
CREATE INDEX ON commerce.orders (user_id, created_at DESC);
CREATE INDEX ON commerce.user_package_access (user_id, expires_at DESC)
  WHERE revoked_at IS NULL;
CREATE INDEX ON commerce.payments (provider_transaction_id);

-- live
CREATE INDEX ON live.sessions (course_id, start_at);
CREATE INDEX ON live.attendance (user_id, joined_at DESC);

-- assessments
CREATE INDEX ON assessments.submissions (assessment_id, user_id);
CREATE INDEX ON assessments.submissions (user_id, submitted_at DESC);

-- coding
CREATE INDEX ON coding.submissions (user_id, problem_id, submitted_at DESC);
CREATE INDEX ON coding.submissions (verdict) WHERE verdict = 'AC';

-- gamification
CREATE INDEX ON gamification.xp_events (user_id, created_at DESC);

-- audit
CREATE INDEX ON identity.audit_logs (actor_user_id, created_at DESC);
CREATE INDEX ON identity.audit_logs (action, created_at DESC);
```

## 13. Partitioning

- `identity.audit_logs` — monthly
- `gamification.xp_events` — monthly
- `coding.submissions` — quarterly (after 90 days)
- `notifications.notifications` — quarterly (read notifications archived)

## 14. Backup & PITR

- Daily full snapshot, retain 7 days
- WAL streaming to S3, retain 7 days → PITR 7 days
- Cross-region snapshot daily
- Quarterly restore drill

## 15. Future — multi-tenant

- Add `tenant_id` to every table (default `00000000-0000-0000-0000-000000000000`)
- RLS policy: `tenant_id = current_setting('app.tenant_id')::uuid`
- For very large tenants, consider Citus sharding on `tenant_id`
