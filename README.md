<div align="center">

# 🎓 Codean Platform

### A Production-Grade E-Learning Platform built on Clean Architecture & CQRS

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7-DC382D?logo=redis)](https://redis.io/)
[![Tests](https://img.shields.io/badge/Tests-242%20Passing-brightgreen?logo=xunit)](https://github.com/nwr4519678/Codean)
[![Build](https://img.shields.io/badge/Build-Passing-brightgreen?logo=github-actions)](https://github.com/nwr4519678/Codean)
[![License](https://img.shields.io/badge/License-MIT-blue)](LICENSE)

**Codean** is a comprehensive online teaching platform supporting courses, live virtual sessions, coding judge challenges, subscription commerce, and real-time communication — all powered by a robust .NET 10 backend following Clean Architecture, CQRS, and Domain-Driven Design principles.

</div>

---

## 📚 Table of Contents

- [Architecture Overview](#architecture-overview)
- [Tech Stack](#tech-stack)
- [Bounded Contexts (8 Domains)](#bounded-contexts)
- [Key Features](#key-features)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Configuration Reference](#configuration-reference)
- [API Reference](#api-reference)
- [Caching Strategy](#caching-strategy)
- [Rate Limiting Policies](#rate-limiting-policies)
- [Testing](#testing)
- [Database Schema](#database-schema)

---

## Architecture Overview

The backend follows **Clean Architecture** with strict dependency rules enforced by architecture tests:

```
┌─────────────────────────────────────────────────────────────────┐
│                         Platform.Api                            │
│   Controllers · Middleware · OpenAPI · Hangfire Dashboard       │
├─────────────────────────────────────────────────────────────────┤
│                    Platform.Application                         │
│   CQRS Handlers · Validators · MediatR Pipeline Behaviors      │
│   Caching · Rate Limiting · Pagination · Domain Mapping        │
├─────────────────────────────────────────────────────────────────┤
│                      Platform.Domain                            │
│   Entities · Aggregates · Value Objects · Domain Events        │
│   Result<T> · Error · IRepository<T>                           │
├─────────────────────────────────────────────────────────────────┤
│                   Platform.Infrastructure                       │
│   EF Core · PostgreSQL · Redis · Hangfire · HybridCache        │
│   JWT · Paymob · Google Meet · MS Teams · Cloudflare R2        │
├─────────────────────────────────────────────────────────────────┤
│                     Platform.Judge                              │
│   Judge0 HTTP Client · Language Registry · Test Case Runner    │
└─────────────────────────────────────────────────────────────────┘
```

### Design Principles
- **Clean Architecture** — strict inward dependency flow; Domain has zero external dependencies
- **CQRS** via MediatR — Commands mutate state, Queries read it; no shared handlers
- **Domain-Driven Design** — 8 Bounded Contexts with rich domain models
- **Result<T> Pattern** — typed error handling without exceptions crossing boundaries
- **Outbox Pattern** — reliable event delivery via `outbox_messages` table
- **Repository Pattern** — `IRepository<T>` abstracts persistence; testable by design

---

## Tech Stack

| Layer | Technology |
|---|---|
| **Runtime** | .NET 10 (C# 13) |
| **API Framework** | ASP.NET Core 10 Web API |
| **ORM** | Entity Framework Core 10 |
| **Database** | PostgreSQL 16 |
| **Caching** | .NET 10 HybridCache (L1 in-process + L2 Redis) |
| **Message Bus** | Outbox Pattern (PostgreSQL-backed) |
| **Background Jobs** | Hangfire (PostgreSQL storage) |
| **Authentication** | JWT Bearer + 2FA (TOTP) + Refresh Tokens |
| **Payments** | Paymob (Egyptian gateway) + HMAC Webhook Idempotency |
| **Live Sessions** | Google Meet API + Microsoft Teams API |
| **Code Judge** | Judge0 (self-hosted or cloud) |
| **File Storage** | Cloudflare R2 (S3-compatible) |
| **Email** | SMTP via abstracted `IEmailService` |
| **Logging** | Serilog (structured) + OpenTelemetry-ready |
| **API Docs** | Scalar (OpenAPI 3.1) |
| **Rate Limiting** | ASP.NET Core `RateLimiter` middleware |
| **Validation** | FluentValidation 11 |
| **Mapping** | Manual extension methods (no AutoMapper reflection overhead) |
| **Testing** | xUnit 3 + NSubstitute + FluentAssertions + NetArchTest |

---

## Bounded Contexts

The platform is organized into **8 bounded contexts**, each with its own CQRS handlers, validators, mappings, and API controllers:

### 1. 🔐 Identity & Administration
Full authentication lifecycle: registration, login, 2FA, lockout, session management, token refresh, password reset, profile management for Students, Teachers, and Admins.

**Commands:** `RegisterUser`, `LoginUser`, `VerifyEmail`, `Enable2FA`, `Confirm2FA`, `ChangePassword`, `ResetPassword`, `RevokeToken`, `RevokeSession`, `UpdateProfile`, `UploadUserAvatar`, `SetUserStatus`, `CreateTeacherProfile`, `CreateStudentProfile`

**Queries:** `GetCurrentUser`, `GetActiveSessions`, `GetUsersPaged`, `GetTeacherProfiles`, `GetStudentProfiles`

### 2. 📖 Learning Content
Full course lifecycle: creation, publishing, archiving. Structured content with Modules, Lessons, Resources, and Videos. Student progress tracking.

**Commands:** `CreateCourse`, `UpdateCourse`, `PublishCourse`, `ArchiveCourse`, `CreateCourseModule`, `UpdateCourseModule`, `DeleteCourseModule`, `CreateLesson`, `UpdateLesson`, `DeleteLesson`, `RecordStudentProgress`

**Queries:** `GetCourseById`, `GetCoursesPaged`, `GetCourseModules`, `GetLessons`, `GetStudentProgress`

### 3. 📝 Assessment & Evaluation
Full exam and homework lifecycle with question banks, student attempts, auto-grading, and result reporting.

**Commands:** `CreateExam`, `UpdateExam`, `PublishExam`, `StartExamAttempt`, `SubmitExamAttempt`, `CreateHomework`, `SubmitHomework`

**Queries:** `GetExamById`, `GetExamsPaged`, `GetExamAttempt`, `GetExamResults`, `GetHomeworkById`, `GetStudentHomework`

### 4. ⚙️ Judge Engine
Async code submission pipeline using Judge0. Submissions receive an immediate `202 Accepted` with a status URL. A Hangfire reconciler polls pending submissions and updates results.

**Pattern:** `POST /challenges/{id}/submit` → `202 Accepted { statusUrl }` → `GET /submissions/{id}` → `{ status: "Accepted|WrongAnswer|..." }`

**Commands:** `CreateChallenge`, `UpdateChallenge`, `SubmitSolution`

**Queries:** `GetChallengeById`, `GetChallengesPaged`, `GetSubmissionStatus`, `GetStudentSubmissions`

### 5. 💳 Commerce & Subscriptions
Subscription plan management, Paymob payment gateway integration, HMAC-verified webhooks, and student subscription lifecycle.

**Flow:** `POST /subscriptions/checkout` → Paymob hosted page → `POST /webhooks/paymob` (HMAC verified, idempotent) → subscription activated

**Commands:** `CreateSubscriptionPlan`, `UpdateSubscriptionPlan`, `InitiateCheckout`, `ProcessPaymobWebhook`, `ActivateSubscription`

**Queries:** `GetSubscriptionPlans`, `GetSubscriptionPlanById`, `GetStudentSubscription`, `GetStudentInvoices`

### 6. 📢 Communication
Announcements (per-course or platform-wide, with pin support) and multi-channel notifications (in-app, email, push) with history and read-state tracking.

**Commands:** `CreateAnnouncement`, `UpdateAnnouncement`, `DeleteAnnouncement`, `SendNotification`, `MarkNotificationsRead`

**Queries:** `GetAnnouncementsPaged`, `GetNotifications`, `GetUnreadNotificationCount`

### 7. 📹 Live Virtual Sessions
Schedule and manage live sessions via **Google Meet** or **Microsoft Teams** using the Provider Strategy Pattern. Attendance recording and reporting.

**Commands:** `ScheduleLiveSession`, `CancelLiveSession`, `RecordAttendance`

**Queries:** `GetSessionsByTeacher`, `GetSessionAttendance`

### 8. 📊 Analytics & System Audit
Platform-wide KPI dashboard (students, revenue, course engagement) and full tamper-evident audit log of all administrative actions.

**Queries:** `GetPlatformOverview`, `GetAuditLogsPaged`

---

## Key Features

### 🔑 Authentication & Security
- **JWT Bearer** with configurable expiry + **Refresh Token** rotation
- **2FA via TOTP** (Google Authenticator compatible) with enable/disable/confirm lifecycle
- **Account lockout** after configurable failed login attempts
- **JWT Token Blacklisting** via Redis (JTI-based) — instant revocation
- **Session tracking** — per-device sessions visible to the user, individually revocable
- **Password hashing** via ASP.NET Core Identity `IPasswordHasher`
- **HMAC webhook signature verification** for Paymob payment events

### ⚡ Caching (HybridCache)
.NET 10 `HybridCache` provides stampede-protected **2-tier caching** (L1 in-process + L2 Redis). All read-heavy queries implement `ICacheableRequest` and are automatically cached by the `CachingBehaviour` MediatR pipeline. Write commands invalidate cache via **tag-based group invalidation** (`RemoveByTagAsync`).

See [Caching Strategy](#caching-strategy) for the full TTL and tag mapping table.

### 🛡️ Rate Limiting
6 tailored fixed-window policies protect sensitive endpoints. See [Rate Limiting Policies](#rate-limiting-policies).

### 📦 Outbox Pattern
All domain events are persisted to the `outbox_messages` PostgreSQL table within the same database transaction before being dispatched. This guarantees at-least-once delivery with zero risk of message loss on process restart.

### 🔄 Async Judge Engine
Code submissions follow the **async polling pattern**:
1. Client posts a solution → receives `202 Accepted` + `{ statusUrl }` immediately
2. Client polls `statusUrl` for the result
3. A Hangfire `ProcessJudgeResultsJob` runs every 30s to reconcile any pending submissions by querying Judge0 and updating results in PostgreSQL

---

## Project Structure

```
Platform/
├── backend/
│   ├── src/
│   │   ├── Platform.Api/                    # ASP.NET Core Web API
│   │   │   ├── Controllers/                 # 22 API controllers
│   │   │   ├── Middleware/                  # Exception handler, request logging
│   │   │   ├── Extensions/                  # Serilog, Hangfire, health check setup
│   │   │   ├── OpenApi/                     # Scalar OpenAPI configuration
│   │   │   ├── Authorization/               # Hangfire admin filter, policies
│   │   │   └── DependencyInjection.cs       # CORS, Rate Limiting, JWT wiring
│   │   │
│   │   ├── Platform.Application/            # Business logic (CQRS)
│   │   │   ├── Common/
│   │   │   │   ├── Abstractions/            # ICacheService, ICurrentUser, IClock, IUnitOfWork
│   │   │   │   ├── Behaviors/               # ValidationBehaviour, CachingBehaviour
│   │   │   │   ├── Caching/                 # ICacheableRequest, CacheKeys, CacheTags
│   │   │   │   └── Pagination/              # PagedList<T>
│   │   │   └── Features/
│   │   │       ├── Analytics/               # Bounded context
│   │   │       ├── Assessment/              # Bounded context
│   │   │       ├── Commerce/                # Bounded context
│   │   │       ├── Communication/           # Bounded context
│   │   │       ├── Judge/                   # Bounded context
│   │   │       ├── Learning/                # Bounded context
│   │   │       ├── LiveSessions/            # Bounded context
│   │   │       └── Users/                   # Identity bounded context
│   │   │
│   │   ├── Platform.Domain/                 # Domain model
│   │   │   ├── Entities/                    # 58 EF Core entity types
│   │   │   ├── Primitives/                  # AggregateRoot, Entity, DomainEvent
│   │   │   └── Results/                     # Result<T>, Error, ErrorType
│   │   │
│   │   ├── Platform.Infrastructure/         # External services & persistence
│   │   │   ├── Authentication/              # JWT generation & validation
│   │   │   ├── Caching/                     # HybridCacheService (L1+L2)
│   │   │   ├── Identity/                    # CurrentUser, PasswordHasher
│   │   │   ├── Jobs/                        # Hangfire recurring jobs
│   │   │   ├── Judge/                       # Judge0 HTTP client
│   │   │   ├── LiveSessions/                # Google Meet & MS Teams providers
│   │   │   ├── Messaging/                   # Outbox publisher, job scheduler
│   │   │   ├── Notifications/               # Email service (SMTP)
│   │   │   ├── Payments/                    # Paymob API client
│   │   │   ├── Persistence/                 # AppDbContext, Repository, UnitOfWork
│   │   │   ├── Security/                    # Token blacklist, HMAC verification
│   │   │   └── Storage/                     # Cloudflare R2 file service
│   │   │
│   │   └── Platform.Judge/                  # Code execution & test case runner
│   │       ├── Execution/                   # Submission processor
│   │       ├── Languages/                   # Language registry
│   │       └── TestCases/                   # Test case evaluator
│   │
│   └── tests/
│       ├── Platform.Domain.UnitTests/        # 26 domain model tests
│       ├── Platform.Application.UnitTests/   # 183 handler & validator tests
│       ├── Platform.Api.UnitTests/           # 11 controller tests
│       ├── Platform.Judge.UnitTests/         # 19 judge engine tests
│       └── Platform.ArchitectureTests/       # 3 clean architecture enforcement tests
│
└── docs/
    ├── implementation_plan.md
    └── application_audit.md
```

---

## Getting Started

### Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 10.0+ |
| PostgreSQL | 16+ |
| Redis | 7+ |
| Judge0 | self-hosted or cloud |

### 1. Clone the Repository

```bash
git clone https://github.com/nwr4519678/Codean.git
cd Codean
```

### 2. Configure Environment

Copy and edit the development settings:

```bash
cd backend/src/Platform.Api
```

Edit `appsettings.Development.json` with your local values (see [Configuration Reference](#configuration-reference)).

### 3. Apply Database Migrations

```bash
dotnet ef database update \
  --project backend/src/Platform.Infrastructure \
  --startup-project backend/src/Platform.Api
```

This creates **60 tables** in PostgreSQL including all domain tables and the Outbox.

### 4. Run the API

```bash
cd backend
dotnet run --project src/Platform.Api
```

The API will be available at:
- **HTTPS:** `https://localhost:7001`
- **HTTP:** `http://localhost:5001`
- **Scalar UI:** `https://localhost:7001/scalar`
- **Hangfire Dashboard:** `https://localhost:7001/hangfire` *(Admin role required)*
- **Health Checks:** `https://localhost:7001/health`

### 5. Run Tests

```bash
cd backend
dotnet test Platform.slnx
```

Expected output: **242 tests passing, 0 failed**.

---

## Configuration Reference

### `appsettings.json` / `appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=PlatformDb;Username=postgres;Password=your_password",
    "Hangfire":          "Host=localhost;Port=5432;Database=PlatformDb;Username=postgres;Password=your_password",
    "Redis":             "localhost:6379"
  },
  "Jwt": {
    "Secret":           "your-256-bit-secret-key-here",
    "Issuer":           "PlatformApi",
    "Audience":         "PlatformClients",
    "ExpiryMinutes":    60,
    "RefreshExpiryDays": 30
  },
  "Paymob": {
    "ApiKey":       "your_paymob_api_key",
    "HmacSecret":   "your_paymob_hmac_secret",
    "IntegrationId": 12345
  },
  "Judge": {
    "BaseUrl":  "http://your-judge0-instance",
    "ApiKey":   "your_judge0_api_key"
  },
  "GoogleMeet": {
    "ClientId":     "your_google_client_id",
    "ClientSecret": "your_google_client_secret"
  },
  "MicrosoftTeams": {
    "TenantId":     "your_tenant_id",
    "ClientId":     "your_client_id",
    "ClientSecret": "your_client_secret"
  },
  "CloudflareR2": {
    "AccountId":       "your_account_id",
    "AccessKeyId":     "your_access_key",
    "SecretAccessKey": "your_secret_key",
    "BucketName":      "platform-assets"
  },
  "Email": {
    "Host":        "smtp.your-provider.com",
    "Port":        587,
    "Username":    "noreply@yourdomain.com",
    "Password":    "your_smtp_password",
    "DisplayName": "Codean Platform"
  },
  "HybridCache": {
    "MaximumPayloadBytes": 10485760,
    "DefaultLocalExpiry":  "00:02:00",
    "DefaultRemoteExpiry": "00:15:00"
  }
}
```

---

## API Reference

All endpoints return RFC 7807 Problem Details on error. All protected endpoints require `Authorization: Bearer {token}`.

### Authentication — `/api/auth`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/register` | ❌ | Register a new user |
| `POST` | `/login` | ❌ | Login (returns access + refresh token) |
| `POST` | `/refresh` | ❌ | Refresh access token |
| `POST` | `/verify-email` | ❌ | Verify email address |
| `POST` | `/enable-2fa` | ✅ | Enable TOTP 2FA (returns QR code URI) |
| `POST` | `/confirm-2fa` | ✅ | Confirm 2FA setup with TOTP code |
| `POST` | `/change-password` | ✅ | Change authenticated user's password |
| `POST` | `/forgot-password` | ❌ | Request password reset email |
| `POST` | `/reset-password` | ❌ | Reset password with token |
| `POST` | `/revoke-token` | ✅ | Blacklist current JWT immediately |
| `GET` | `/sessions` | ✅ | List active sessions |
| `DELETE` | `/sessions/{id}` | ✅ | Revoke a specific session |

### Courses — `/api/courses`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/` | ❌ | Get courses (paged, filterable) |
| `GET` | `/{id}` | ❌ | Get course detail with modules & lessons |
| `POST` | `/` | ✅ Teacher | Create a new course |
| `PUT` | `/{id}` | ✅ Teacher | Update course details |
| `POST` | `/{id}/publish` | ✅ Teacher | Publish course (make visible to students) |
| `POST` | `/{id}/archive` | ✅ Teacher | Archive course (unpublish) |

### Coding Challenges — `/api/challenges`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/` | ✅ | Get challenges (paged) |
| `GET` | `/{id}` | ✅ | Get challenge detail |
| `POST` | `/` | ✅ Teacher | Create challenge |
| `POST` | `/{id}/submit` | ✅ Student | Submit solution (202 + polling URL) — **Rate limited: 10/min** |

### Subscriptions & Commerce — `/api/subscriptions`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/checkout` | ✅ Student | Initiate Paymob checkout — **Rate limited: 5/15min** |
| `GET` | `/my-subscription` | ✅ Student | Get current active subscription |
| `GET` | `/invoices` | ✅ Student | Get student invoice history |

### Live Sessions — `/api/live-sessions`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/` | ✅ Teacher | Schedule a session (Google Meet or MS Teams) |
| `DELETE` | `/{id}` | ✅ Teacher | Cancel a session |
| `GET` | `/my-sessions` | ✅ Teacher | Get teacher's sessions |
| `POST` | `/{id}/attendance` | ✅ Teacher | Record student attendance |
| `GET` | `/{id}/attendance` | ✅ Teacher | Get attendance report |

> Full OpenAPI specification available at `/scalar` when running in Development mode.

---

## Caching Strategy

The platform uses `.NET 10 HybridCache` — **L1 (in-process MemoryCache) + L2 (Redis)** — with automatic stampede protection. All read queries implementing `ICacheableRequest` are intercepted by `CachingBehaviour`.

### Cache TTL & Tag Invalidation Table

| Domain | Query | TTL | Invalidation Tag | Invalidated By |
|---|---|---|---|---|
| Identity | `GetCurrentUser` | 5 min | — | `ChangePassword`, `UpdateProfile` |
| Identity | `GetActiveSessions` | 2 min | — | `RevokeSession` |
| Learning | `GetCourseById` | 10 min | `tag:learning:course:{id}` | `UpdateCourse`, `PublishCourse`, `ArchiveCourse`, any Module/Lesson mutation |
| Learning | `GetCoursesPaged` | 5 min | `tag:learning:courses_list` | `CreateCourse`, `UpdateCourse`, `PublishCourse`, `ArchiveCourse` |
| Commerce | `GetSubscriptionPlanById` | 15 min | `tag:commerce:plan:{id}` | `UpdateSubscriptionPlan` |
| Commerce | `GetSubscriptionPlans` | 15 min | `tag:commerce:plans_list` | `CreateSubscriptionPlan`, `UpdateSubscriptionPlan` |
| Communication | `GetAnnouncementsPaged` | 5 min | `tag:comm:announcements_list` | `CreateAnnouncement`, `UpdateAnnouncement`, `DeleteAnnouncement` |
| Analytics | `GetPlatformOverview` | 10 min | `tag:analytics:overview` | Hangfire daily job |

**Cache invalidation is atomic with the database write** — `RemoveByTagAsync` is called immediately after `SaveChangesAsync` succeeds.

---

## Rate Limiting Policies

ASP.NET Core `RateLimiter` middleware with **fixed-window** algorithm. All policies return `429 Too Many Requests` with `Retry-After` header.

| Policy | Endpoints | Window | Limit | Queue |
|---|---|---|---|---|
| `auth_login` | `POST /api/auth/login` | 15 min | 10 | 0 |
| `auth_register` | `POST /api/auth/register` | 1 hour | 5 | 0 |
| `auth_sensitive` | Password reset / change | 15 min | 5 | 0 |
| `code_submission` | `POST /api/challenges/{id}/submit` | 1 min | 10 | 0 |
| `payment_checkout` | `POST /api/subscriptions/checkout` | 15 min | 5 | 0 |
| `api_read_general` | General GET endpoints | 1 min | 100 | 10 |

---

## Testing

The test suite covers all layers with **242 tests across 5 test projects**:

```
Test summary: total: 242, failed: 0, succeeded: 242, skipped: 0
```

| Project | Count | Covers |
|---|---|---|
| `Platform.Domain.UnitTests` | 26 | Entity invariants, Result<T>, Error types, AggregateRoot |
| `Platform.Application.UnitTests` | 183 | All CQRS handlers, all FluentValidation validators, CachingBehaviour, ValidationBehaviour |
| `Platform.Api.UnitTests` | 11 | Controller action results, response shapes |
| `Platform.Judge.UnitTests` | 19 | Judge Engine, language registry, test case evaluation |
| `Platform.ArchitectureTests` | 3 | Clean Architecture dependency rules via NetArchTest |

### Architecture Tests
The architecture tests enforce:
1. **Domain** has no dependency on Application, Infrastructure, or Api
2. **Application** has no dependency on Infrastructure or Api
3. **Infrastructure** does not reference Api directly

---

## Database Schema

All **60 tables** are created by the `InitialCreate` EF Core migration. Key tables by domain:

| Domain | Tables |
|---|---|
| Identity | `Users`, `Roles`, `RefreshTokens`, `UserSessions`, `AdminProfiles`, `TeacherProfiles`, `StudentProfiles`, `TeacherSettings` |
| Learning | `Courses`, `CourseModules`, `Lessons`, `LessonResources`, `LessonVideos`, `StudentProgress` |
| Assessment | `Exams`, `ExamQuestions`, `ExamAttempts`, `ExamAnswers`, `ExamResults`, `Homework`, `HomeworkQuestions`, `HomeworkSubmissions` |
| Judge | `CodingChallenges`, `CodingSubmissions`, `QuestionBanks`, `Questions`, `QuestionTypes`, `QuestionChoices` |
| Commerce | `SubscriptionPlans`, `StudentSubscriptions`, `Payments`, `Invoices` |
| Communication | `Announcements`, `Notifications`, `NotificationHistory`, `NotificationTemplates`, `ParentNotifications` |
| Live Sessions | `LiveSessions`, `LiveAttendance`, `LiveMeetingProviders` |
| Analytics | `AuditLogs`, `AnalyticsSnapshots`, `SystemLogs`, `Certificates` |
| Infrastructure | `Files`, `Tags`, `Settings`, `SearchIndex`, `outbox_messages`, `__EFMigrationsHistory` |

---

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit changes with descriptive messages
4. Push to the branch and open a Pull Request against `main`

---

## License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

<div align="center">
Built with ❤️ by the Codean Team · Powered by .NET 10 + PostgreSQL + Redis
</div>
