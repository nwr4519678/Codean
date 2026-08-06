# 21 — Testing Strategy

**Last updated:** 2026-08-03

## 1. Test pyramid

```
            ┌──────────┐
            │   E2E    │   ~ 5% of tests
            │  (PW)    │   critical user journeys
            ├──────────┤
            │Functional│   ~ 15% of tests
            │ (WAF)    │   endpoint contracts
            ├──────────┤
            │Integration│  ~ 25% of tests
            │(Testcont.)│  DB, external adapters
            ├──────────┤
            │   Unit   │   ~ 55% of tests
            │ (xUnit)  │   handlers, validators, entities
            └──────────┘
```

## 2. Unit tests (xUnit + FluentAssertions + NSubstitute)

| Target | Focus |
|---|---|
| Domain entities | Invariants, factories, state transitions |
| Value objects | Equality, normalization, parsing |
| MediatR handlers | Orchestration, validation, mapping, side-effects via mocks |
| Validators | All FluentValidation rules including edge cases |
| Result/Error mappers | Code coverage + boundary cases |
| Money/DateRange/Etc. | Operator semantics, formatting |

Coverage gate: **≥ 80 %** for Domain and Application layers. Enforced in CI via `coverlet.collector` + `Codecov` (or `SonarQube`).

## 3. Integration tests (Testcontainers)

Each test spins up a fresh Postgres + Redis container, runs migrations, then exercises:
- Repository round-trips
- EF Core query behaviour
- Concurrency tokens
- Migration up/down

`Testcontainers.PostgreSql` and `Testcontainers.Redis` are used. Containers are reused across the test assembly via `IAsyncLifetime` + a `TestFixture` base.

## 4. Functional tests (WebApplicationFactory)

For each REST endpoint:
- Arrange: seed a user with the right roles + an entity
- Act: HTTP call via `HttpClient`
- Assert: status, body shape, DB side-effect

Covers:
- Auth flows (register, login, refresh, 2FA)
- Course CRUD
- Order creation + payment intent
- Live session create + join (with mocked provider)
- Code submission enqueue

## 5. Architecture tests (ArchUnitNET)

Rules:
- Domain does not depend on any other project
- Application does not depend on Infrastructure or Api
- Infrastructure does not depend on Api
- All controllers end with `Controller`
- All command handlers end with `CommandHandler`
- All query handlers end with `QueryHandler`
- All event handlers end with `EventHandler`
- No public class can start with `Impl` or `Manager`

Fails the build on violation.

## 6. Mutation tests (Stryker.NET, nightly)

Targets Domain + Application. Mutation score gate: **≥ 70 %**.

## 7. End-to-end tests (Playwright)

A small set (10–20 scenarios) covering critical user journeys:
1. New user signs up → email verify → enrolls in free course → watches lesson → completes.
2. Student subscribes to Month 1 via Paymob (sandbox) → access granted → attends live.
3. Teacher creates course + module + lesson + monthly package + publishes.
4. Student takes exam with anti-cheat enabled.
5. Student runs Python code, sees verdict, retries.
6. Parent links to child, views grades.
7. Support agent impersonates a user (with consent banner).

## 8. Load tests (k6)

Three scenarios run nightly against staging:

```
smoke:   50 VUs, 5 min, 100 RPS
load:    500 VUs, 15 min, 1000 RPS
spike:   0 → 2000 VUs in 30 s, hold 5 min
soak:    200 VUs, 4 h, 400 RPS
```

Pass criteria: P95 < 500 ms (cached), P99 < 1.5 s, error rate < 0.1 %.

## 9. Security tests

- **OWASP ZAP** baseline scan on staging every night.
- **Trivy** image scan in CI.
- **Snyk** for open-source deps.
- **Semgrep** for SAST on backend + frontend.

## 10. Contract tests (Pact, optional)

For the Paymob integration, we use **Pact** to verify the consumer (us) and provider (Paymob sandbox) agree on the JSON shape. Stored in `backend/tests/Platform.Infrastructure.ContractTests/`.

## 11. Test data management

- **Bogus** for fake data generation.
- **Respawn** to reset the DB between integration tests.
- **Per-test transactions** for unit-style DB tests.

## 12. Code coverage

- Tool: `coverlet.collector` + `ReportGenerator`.
- Threshold: 80 % line + branch on Domain + Application.
- Visible in PR via Codecov comment.
- Block merge below threshold.

## 13. CI / CD integration

- PR: unit + integration + architecture.
- Main: + load smoke + security scan.
- Nightly: full suite + mutation + load + E2E + security.

## 14. Local development

```bash
# Run a single test class
dotnet test --filter "FullyQualifiedName~PaymentOrderTests"

# With coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

## 15. Test conventions

- One test class per subject under test.
- Method name: `Method_State_ExpectedBehaviour`.
- AAA structure with a blank line between sections.
- One logical assertion per test (multi-assert allowed for invariants).
- No test depends on another test's order or side-effects.
