# 12 — Authorization Flow

**Last updated:** 2026-08-03

## 1. Model

Three concentric layers, evaluated in order:

1. **Role** (`Student`, `Teacher`, …) — coarse, scoped (global / school / course)
2. **Policy** — fine-grained rule (`CanGrade`, `CanIssueCertificate`, …)
3. **Resource ownership** — must own / be assigned to the target

## 2. Policy catalog

| Policy | Allowed roles |
|---|---|
| `courses.read` | All authenticated |
| `courses.write` | Teacher, TeacherAssistant (scoped), PlatformAdmin |
| `lessons.publish` | Teacher (owner) |
| `assignments.grade` | Teacher, TeacherAssistant (scoped) |
| `exams.create` | Teacher, PlatformAdmin |
| `live.schedule` | Teacher (owner), PlatformAdmin |
| `payments.refund` | PlatformAdmin |
| `users.suspend` | PlatformAdmin, SuperAdmin |
| `coupons.create` | Teacher, PlatformAdmin |
| `admin.audit.read` | PlatformAdmin, SuperAdmin, Support |

Policies are declared as `IAuthorizationRequirement` and evaluated by `IAuthorizationHandler`s.

## 3. Scoping rules

```
UserRole {
  roleId      ->  Teacher
  scopeType   ->  Course | School | Global
  scopeId     ->  course or school id
}
```

When a `Teacher` has only a course-scoped role, attempting to act on another course is denied with `403 Forbidden`. PlatformAdmin can override scoping.

## 4. Subscription access

`[RequireSubscription]` is a custom authorization attribute that:

1. Loads the request's `courseId`.
2. Checks `UserPackageAccess` for an active row covering that course's package.
3. Resolves "active" = `(now < ExpiresAt) && (RevokedAt is null)`.
4. Returns `402 SUBSCRIPTION_REQUIRED` if missing.

Free lessons (`LessonVisibility = Free`) are exempted from the check.

## 5. Enforcement points

- **MediatR pipeline** (`AuthorizationBehaviour`) — runs before the handler.
- **Controllers** — `[Authorize(Policy = "...")]` attributes for whole-endpoint.
- **Endpoints** that need it programmatically — call `IAuthorizationService.AuthorizeAsync`.

## 6. Decisions log

| ID | Decision | Rationale |
|---|---|---|
| ADR-001 | Use `IAuthorizationHandler` over custom attributes | Better testability, multiple rules per policy |
| ADR-002 | Store role scopes in DB, not in JWT | Allows revocation mid-session without token roundtrip |
| ADR-003 | Subscription check is a policy, not a hard-coded `if` | Same rule applies in REST, GraphQL, and Hangfire jobs |
