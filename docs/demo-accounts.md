# Local development account examples

These are examples for local development only. Do not use them in staging or production.

| Account | Email example | Password example | How to create |
|---|---|---|---|
| Student | `student@example.local` | `StudentPassword123!` | Public student registration |
| Teacher | `teacher@example.local` | `TeacherPassword123!` | Admin console → Create teacher |
| Admin | `admin@platform.com` | `AdminPassword123!` | Development seeder only |

The application detects the role returned by the API after email/password authentication. Users never choose a role at login. Teacher accounts are created only through the authenticated Admin console and the API ignores any client-supplied role for that operation.

For Supabase Auth, configure the web app with `VITE_SUPABASE_URL` and `VITE_SUPABASE_PUBLISHABLE_KEY` from a local, staging, or production environment file. Never commit the Supabase database connection string or service-role key to the repository.
