# Local frontend and backend with external services

The local frontend and ASP.NET Core API can run without a local PostgreSQL or
Redis installation. Development configuration intentionally leaves database
connection strings empty so the API cannot accidentally use a local database.

## Configure the local API

Run these commands from `backend/` and replace the values with secrets from your
own providers. `dotnet user-secrets` stores them outside the repository.

```powershell
$supabaseConnection = 'Host=<supabase-pooler-host>;Port=5432;Database=postgres;Username=<supabase-username>;Password=<supabase-password>;SSL Mode=Require;Trust Server Certificate=true;Maximum Pool Size=10'

dotnet user-secrets set 'ConnectionStrings:DefaultConnection' $supabaseConnection --project src/Platform.Api
dotnet user-secrets set 'ConnectionStrings:Hangfire' $supabaseConnection --project src/Platform.Api
dotnet user-secrets set 'Clerk:Authority' 'https://<your-clerk-instance>.clerk.accounts.dev' --project src/Platform.Api
dotnet user-secrets set 'RunMigrationsOnStartup' 'true' --project src/Platform.Api
dotnet user-secrets set 'SeedDevelopmentAdmin' 'false' --project src/Platform.Api
```

`DefaultConnection` is the application database. `Hangfire` may use the same
Supabase PostgreSQL database; Hangfire stores its tables in its own `hangfire`
schema.

Start the API:

```powershell
dotnet run --project src/Platform.Api --launch-profile http
```

The API applies pending migrations and seeds reference roles on startup. This
does not reset or delete existing Supabase data. Development admin seeding is
disabled for this external-database workflow.

## Local frontend

Create `frontend/apps/web/.env.local` (this file is ignored by Git):

```text
VITE_API_URL=http://localhost:5294
VITE_CLERK_PUBLISHABLE_KEY=pk_test_<your-publishable-key>
VITE_ADMIN_URL=http://localhost:5174
```

Run the web app with `pnpm --filter @codean/web dev`, or use the production
preview after building it. The local API allows both `localhost` and
`127.0.0.1` Vite origins.

## Services that are not local requirements

- Supabase PostgreSQL is the database provider.
- Clerk is the authentication provider.
- Hangfire uses the Supabase PostgreSQL connection and does not require Redis.
- The current HybridCache falls back to in-process memory; Redis is optional and
  is not required to run the local frontend/backend pair.
- Judge execution uses the configured external OnlineCompiler API when a judge
  API key is supplied.

SMTP remains degraded until a real SMTP provider is configured. That warning
does not prevent the API, authentication, or dashboard from running.
