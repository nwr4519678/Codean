# Supabase Auth setup

The repository does not contain Supabase credentials. Configure them through environment variables:

```env
VITE_SUPABASE_URL=https://<project-ref>.supabase.co
VITE_SUPABASE_PUBLISHABLE_KEY=sb_publishable_...
```

Never put the PostgreSQL connection string, database password, or Supabase service-role key in frontend code, Vercel client variables, source control, or screenshots. The database password supplied during setup should be rotated before using the project outside local development.

## Google and Microsoft

In Supabase Dashboard → Authentication → Providers, enable Google and Azure (Microsoft), then configure the provider client IDs/secrets. Add these redirect URLs:

- `http://localhost:5173/auth/callback`
- the production web origin plus `/auth/callback`

The application’s role is never selected by the user. After authentication, the platform resolves the account by email and reads the role from the platform database. Public signup creates Student accounts only. Teacher accounts are created by an authenticated Admin through the Admin console.

The current backend issues its own access/refresh token pair. Completing a Supabase OAuth migration requires the server-side Supabase token exchange and account-linking endpoint before OAuth sessions can access protected platform APIs; do not bypass that boundary by storing Supabase service credentials in the browser.
