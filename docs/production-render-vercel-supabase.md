# Production deployment: Vercel + Render + Supabase

## Vercel

Create two Vercel projects from this repository:

- Web: root directory `frontend/apps/web`, build command `pnpm build`, output directory `dist`.
- Admin: root directory `frontend/apps/admin`, build command `pnpm build`, output directory `dist`.

Set `VITE_API_URL` to the public Render API URL in both projects. The checked-in `vercel.json` files preserve client-side routing on refresh.

## Render

The root `render.yaml` defines the API and judge worker. Connect the repository as a Blueprint and provide the `sync: false` values from the Render dashboard. Set `ConnectionStrings__DefaultConnection` to the Supabase pooled PostgreSQL connection string and configure `Cors__AllowedOrigins` as a comma-separated list containing both Vercel origins.

Run database migrations as an explicit release step before the API is made public:

```bash
dotnet ef database update --project backend/src/Platform.Infrastructure --startup-project backend/src/Platform.Api
```

Production startup does not run migrations automatically. Health checks are available at `/health/live` and `/health/ready`.
