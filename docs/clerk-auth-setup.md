# Clerk authentication deployment

The web and admin Vite applications use Clerk for user authentication. Supabase remains the PostgreSQL provider; it is no longer used by the browser for login, signup, password reset, or OAuth.

## Vercel variables (both Web and Admin)

```text
VITE_API_URL=https://codean.runasp.net
VITE_CLERK_PUBLISHABLE_KEY=pk_test_...
```

Add `VITE_ADMIN_URL` to Web and `VITE_PUBLIC_APP_URL` to Admin as appropriate. The publishable key is safe for browser use, but never add a Clerk secret key to Vercel frontend variables.

## MonsterASP variables

```text
Clerk__Authority=https://<your-clerk-instance>.clerk.accounts.dev
Clerk__Audience=<your-clerk-jwt-audience-if-configured>
```

`Clerk__Authority` must exactly match the issuer shown by the Clerk JWT template. `Clerk__Audience` may be omitted when the Clerk token has no audience claim. Configure Clerk's allowed origins and redirect URLs for:

```text
https://codean-web.vercel.app
https://codean-admin.vercel.app
```

The API validates the Clerk token through the authority's OpenID metadata, then provisions or updates the matching local profile in Supabase PostgreSQL. Public registration is always a Student; only an authenticated Admin can create a Teacher.

## Security

Rotate any Clerk secret key that was pasted into chat or committed locally. Store it only in the MonsterASP secret settings if server-side Clerk management APIs are later enabled.
