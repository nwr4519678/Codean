# 11 — Authentication Flow

**Last updated:** 2026-08-03

## 1. Supported methods

| Method | Notes |
|---|---|
| Email + password | BCrypt cost 12, 8-char minimum, optional complexity rules |
| Google OAuth 2.0 | Authorization Code + PKCE |
| Microsoft OAuth 2.0 | Authorization Code + PKCE |
| TOTP 2FA | RFC 6238, 6 digits, 30 s window, 10 backup codes |
| Refresh tokens | Opaque, 30 d, rotation on use, stored hashed at rest |

## 2. Token model

- **Access token:** JWT, RS256, 15 min, claims include `sub`, `email`, `roles[]`, `jti`.
- **Refresh token:** 64-byte random opaque string, hashed (SHA-256) at rest.
- Rotation: every refresh issues a new refresh token and revokes the old one. Reuse of a revoked refresh token immediately revokes the entire user session chain (theft detection).

## 3. Flows

### 3.1 Email + password

```
Client                Platform.Api                Platform.Infrastructure (DB)
  |                        |                                 |
  |-- POST /auth/login --->|                                 |
  |  {email, password}     |  -- validate --                 |
  |                        |  -- BCrypt verify -->            |
  |                        |  -- issue JWT + refresh         |
  |<-- 200 {tokens} ------|                                 |
  |                        |  -- store RefreshToken (hashed)  |
```

### 3.2 Refresh (with rotation)

```
Client                 Api                     DB
  |                     |                       |
  |-- POST /auth/refresh|                       |
  |  {refreshToken}     | -- hash + lookup --->|
  |                     | -- check active     |
  |                     | -- revoke old        |
  |                     | -- issue new         |
  |<-- 200 {tokens} ----|                       |
```

If the **revoked** token is presented again, all user sessions are revoked.

### 3.3 OAuth (Google / Microsoft)

```
Client                      Api                       Identity provider
  |                          |                              |
  |-- GET /auth/oauth/{p} ->|                              |
  |<-- 302 {redirect, state, code_challenge} -|            |
  |                                                                |
  |-- (user authorizes on provider) ----------|
  |                                                                |
  |-- GET /auth/oauth/callback?code=...&state=... ->|
  |                          | -- exchange code for tokens       |
  |                          | -- fetch profile                  |
  |                          | -- upsert User + UserLogin       |
  |<-- 200 {tokens, isNewUser} -|
```

State is stored in a short-lived signed cookie (`__Host-oauth-state`) and validated server-side.

### 3.4 2FA enrollment

```
1. POST /auth/2fa/setup       → { otpauthUrl, qrDataUrl }
   (User scans QR with Google Authenticator / Authy.)
2. POST /auth/2fa/enable {code} → { backupCodes: [...] }  (10 single-use)
3. Subsequent /auth/login:
   - If 2FA enabled, server returns 200 with {requires2fa: true, twoFactorToken}
   - Client calls POST /auth/2fa/verify {code, twoFactorToken}
   - On success, server returns the normal access + refresh pair.
```

### 3.5 Password reset

```
1. POST /auth/password/reset/request {email}  →  always 200 (no enumeration)
2. Email contains: https://app/auth/reset?token=...&uid=...
3. POST /auth/password/reset/confirm {token, newPassword}  → 200
4. All refresh tokens for the user are revoked.
```

## 4. Headers (incoming)

| Header | Required | Notes |
|---|---|---|
| `Authorization: Bearer <jwt>` | Yes for protected | |
| `X-Request-Id` | Optional | Echoed for correlation |

## 5. Errors

| Code | When |
|---|---|
| `auth.invalid_credentials` | Bad email/password |
| `auth.email_not_verified` | Email unverified at time of paid action |
| `auth.2fa_required` | Login needs 2FA step |
| `auth.2fa_invalid` | Wrong TOTP code |
| `auth.token_revoked` | Reuse of revoked refresh token |
| `auth.account_locked` | Too many failed attempts |

## 6. Rate limits

| Action | Limit |
|---|---|
| POST /auth/login | 5 / 15 min per IP + per email |
| POST /auth/register | 10 / hour per IP |
| POST /auth/refresh | 30 / hour per user |
| POST /auth/password/reset/request | 3 / hour per email |
| POST /auth/2fa/verify | 10 / 15 min per IP |

## 7. Session management

`GET /auth/sessions` returns all active refresh tokens for the user (device, IP, last-used).
`DELETE /auth/sessions/{id}` revokes a single session. `POST /auth/logout-all` revokes all.

## 8. Lockout & brute-force

- 5 failed logins within 15 min → 30 min lockout (per email + per IP).
- Lockout state is stored in Redis with TTL.
- All auth events are written to `audit_logs`.
