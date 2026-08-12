# Repository Instructions

## Boundaries

- `backend/` is the .NET 10 Clean Architecture system. Keep domain rules in `Platform.Domain`, CQRS handlers/validators/mappings in `Platform.Application`, HTTP concerns in `Platform.Api`, and persistence/external integrations in `Platform.Infrastructure`.
- `backend/Platform.slnx` contains five production projects and five test projects. `Platform.Judge` is the separate judge service and has its own unit tests.
- `frontend/apps/web` is the public/student/teacher React 19 + Vite app on port `5173`; its current entrypoint is `src/main.tsx` and its route/page implementation is in `src/App.tsx` and `src/pages.tsx`.
- `frontend/apps/admin` is a separate React 19 + Vite Admin console on port `5174`; it has its own authentication gate and must remain separate from the public app.
- `frontend/packages/*` are workspace libraries. `@platform/api` and the contract/config packages are typechecked libraries; the active Vite apps currently use local page fixtures for most screens, while the Admin login and teacher creation call the real API directly.
- `frontend/templates/*` contains imported reference projects, not active workspace packages. Do not add it to `frontend/pnpm-workspace.yaml`.
- `frontend/templates/lms/AGENTS.md` and its nested instructions apply only inside that template subtree.

## Backend Commands

Run from `backend/`:

```powershell
dotnet build Platform.slnx
dotnet test Platform.slnx --no-restore --maxcpucount:1
dotnet test tests/Platform.Application.UnitTests/Platform.Application.UnitTests.csproj --no-restore
dotnet test tests/Platform.Api.UnitTests/Platform.Api.UnitTests.csproj --no-restore
```

- The full suite is normally 242 tests across the five test projects. Use `--maxcpucount:1` when parallel MSBuild/test execution is unstable on the local machine.
- Run one test project with `dotnet test <path-to-csproj> --no-restore`; filter one test with `--filter FullyQualifiedName~<name>`.
- The API launch profile uses `http://localhost:5294` and `https://localhost:7130`; the README's older `5001/7001` ports are stale.
- API startup automatically runs `Database.MigrateAsync()` and development seeding. Do not manually reset the database just to apply the existing migration.
- Local API development requires PostgreSQL on `localhost:5432`; Redis is configured at `localhost:6379`. The development database settings are in `backend/src/Platform.Api/appsettings.Development.json`.
- Development seeding creates/repairs `admin@platform.com` with password `AdminPassword123!`; do not treat those credentials as production credentials.
- Start the API with `dotnet run --project src/Platform.Api --launch-profile http` from `backend/`.
- EF migration commands use the infrastructure project and API startup project: `dotnet ef database update --project src/Platform.Infrastructure --startup-project src/Platform.Api`.

## Frontend Commands

Run from `frontend/`:

```powershell
pnpm install
pnpm --filter @codean/web dev
pnpm --filter @codean/admin dev
pnpm --filter @codean/web build
pnpm --filter @codean/admin build
pnpm --filter @codean/web typecheck
pnpm --filter @codean/admin typecheck
pnpm --filter @platform/api typecheck
pnpm --filter @platform/contracts typecheck
pnpm --filter @platform/config typecheck
pnpm --filter @platform/ui typecheck
```

- The workspace uses pnpm `11.20.0` and requires Node `>=20`; use the root `frontend/pnpm-lock.yaml` and do not hand-edit it.
- `@codean/web` serves the public app at `http://localhost:5173`; `@codean/admin` serves the protected Admin app at `http://localhost:5174/login`.
- The web app's `/admin/*` route redirects to the Admin origin; do not add Admin pages back into the public app.
- Public registration is student-only. Teacher accounts are created through the authenticated Admin console via `POST /api/users` with the fixed `Teacher` role.
- Admin login uses the backend's actual flat `LoginResponse` and verifies `role === "Admin"` before storing the Admin session. Keep Admin token storage separate from public-app token storage.
- Both Vite apps require `src/vite-env.d.ts` for `import.meta.env` typing. Vite environment values use `VITE_*` names, notably `VITE_API_URL` and `VITE_ADMIN_URL`.
- The backend must be running at `http://localhost:5294` before testing real Admin login or teacher creation; a browser `Failed to fetch` usually means the API is not listening.
- `pnpm --filter @codean/web lint` is configured, but the current Vite app does not ship a local ESLint config. Prefer build/typecheck until that tooling is restored.

## Change Verification

- For backend changes, run the focused test project first, then `dotnet test Platform.slnx --no-restore --maxcpucount:1`.
- For frontend changes, run the affected app's `typecheck` and `build`; run both Vite builds when touching routing, shared styling, or workspace configuration.
- Keep API contract changes synchronized across backend DTOs/controllers and `frontend/packages/contracts`/`frontend/packages/api`; several older wrappers still describe the former Next.js API shapes.
- Do not commit secrets, local environment files, database dumps, or generated `dist/`, `bin/`, `obj/`, `.next/`, or `node_modules/` output.
