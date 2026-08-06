# 19 — CI/CD Pipeline

**Last updated:** 2026-08-03

## 1. Tooling

- **Source:** GitHub (`github.com/platform/platform`)
- **CI:** GitHub Actions
- **CD:** ArgoCD (GitOps)
- **Registry:** AWS ECR
- **Cache:** GitHub Actions cache + ECR layer cache

## 2. Branching

- `main` — production. Protected, requires PR + 1 review + green CI.
- `develop` — staging.
- Feature branches — `feat/`, `fix/`, `chore/`.
- Release tags — `vX.Y.Z` on `main`, signed with Sigstore cosign.

## 3. Workflows

### 3.1 `.github/workflows/backend-ci.yml`

Triggered on PR + push to `main`/`develop` affecting `backend/**`.

Jobs:
1. **restore + build** — `dotnet restore`, `dotnet build -c Release`.
2. **unit tests** — `dotnet test --filter "Category=Unit"` with coverage.
3. **integration tests** — Testcontainers (Postgres + Redis). ~3 min.
4. **architecture tests** — ArchUnitNET layer rules.
5. **lint** — `dotnet format --verify-no-changes` + CSharpier.
6. **mutation tests** (nightly) — Stryker.NET on Domain + Application.
7. **publish artifact** — `dotnet pack` → push to GitHub Packages.

### 3.2 `.github/workflows/backend-image.yml`

Triggered on push to `main` after CI passes.

Jobs:
1. **docker build** — multi-stage build with BuildKit cache.
2. **sign + push** — cosign sign + push to ECR.
3. **update values-prod.yaml** — bot PR with new image tag (or direct commit on `main`).

### 3.3 `.github/workflows/frontend-ci.yml`

Triggered on PR + push affecting `frontend/**`.

Jobs:
1. **install** — `npm ci`.
2. **typecheck** — `tsc --noEmit`.
3. **lint** — ESLint.
4. **test** — Vitest (when tests are added).
5. **build** — `next build` (ensures build succeeds).

### 3.4 `.github/workflows/release.yml`

Triggered on tag push `v*.*.*`.

Jobs:
1. Build API + Worker + Judge + Web images tagged with the version.
2. Generate SBOM (Syft) and SLSA provenance.
3. Sign with cosign.
4. Create GitHub release with changelog (release-please bot).
5. Trigger database migration job (`platform-migrations`) before rolling out API.

### 3.5 `.github/workflows/nightly.yml`

- `dotnet test --filter "Category=Long"`
- Load tests (k6) against staging
- Security scans (Trivy, OWASP ZAP, Semgrep)
- Snyk / GitHub Dependabot PR auto-merge
- Backups smoke test

## 4. Required secrets

| Secret | Used by |
|---|---|
| `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` | ECR push, ECS deploy |
| `COSIGN_KEY`, `COSIGN_PASSWORD` | Image signing |
| `PAYMOB_API_KEY`, `PAYMOB_HMAC` | Integration tests |
| `GOOGLE_MEET_SA_JSON` | Live-session tests |
| `MS_TEAMS_TENANT_ID`, `MS_TEAMS_CLIENT_ID`, `MS_TEAMS_CLIENT_SECRET` | Live-session tests |
| `CODECOV_TOKEN` | Coverage upload |
| `SONAR_TOKEN` | SonarQube (optional) |

## 5. Status badges

Top of `README.md`:
- `build` (CI on main)
- `coverage` (codecov)
- `release` (latest)
- `license`
- `security` (Snyk)

## 6. PR template

```markdown
## What does this PR do?
## Why?
## How has this been tested?
## Checklist
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated (if applicable)
- [ ] Docs updated
- [ ] Changelog updated
```

## 7. Branch protection

- Require PR review (1+)
- Require status checks: `build`, `test`, `lint`
- Require linear history
- Require signed commits
- Dismiss stale reviews on new push
- No force-pushes on `main`
