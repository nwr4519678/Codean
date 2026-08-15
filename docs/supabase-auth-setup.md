# Supabase Auth setup

The repository does not contain Supabase credentials. Configure them through environment variables:

```env
VITE_SUPABASE_URL=https://<project-ref>.supabase.co
VITE_SUPABASE_PUBLISHABLE_KEY=sb_publishable_...

# MonsterASP/API only (server secret; never expose to Vercel/browser)
Supabase__Url=https://<project-ref>.supabase.co
Supabase__ServiceRoleKey=<Supabase secret key>
```

Never put the PostgreSQL connection string, database password, or Supabase service-role key in frontend code, Vercel client variables, source control, or screenshots. The database password supplied during setup should be rotated before using the project outside local development.

## Google and Microsoft

In Supabase Dashboard → Authentication → Providers, enable Google and Azure (Microsoft), then configure the provider client IDs/secrets. Add these redirect URLs:

- `http://localhost:5173/auth/login`
- `https://codean-web.vercel.app/auth/login`
- `https://codean-admin.vercel.app/login`

The application’s role is never selected by the user. After authentication, the platform resolves the account by email and reads the role from the platform database. Public signup creates Student accounts only. Teacher accounts are created by an authenticated Admin through the Admin console.

The backend now validates Supabase Auth JWTs and provisions the application profile by email. The service-role key is only needed for the Admin console's teacher-creation operation. If it is not configured, ordinary student login works but teacher creation intentionally returns a configuration error.

For the two Vercel projects, set these production variables in both projects:

```env
VITE_API_URL=https://codean.runasp.net
VITE_SUPABASE_URL=https://<project-ref>.supabase.co
VITE_SUPABASE_PUBLISHABLE_KEY=sb_publishable_...
VITE_ADMIN_URL=https://codean-admin.vercel.app # web project only
VITE_PUBLIC_APP_URL=https://codean-web.vercel.app # admin project only
```

In Supabase Authentication → URL Configuration, allow the production URLs used by the web and admin apps, plus their local development URLs. In Authentication → Providers, enable Google and Azure (Microsoft); each provider must use Supabase's callback URL shown in the dashboard, while the application redirect URL remains in Supabase's allow list.
