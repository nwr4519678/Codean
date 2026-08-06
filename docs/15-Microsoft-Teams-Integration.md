# 15 — Microsoft Teams Integration

**Last updated:** 2026-08-03

## 1. Setup

1. Register an app in **Microsoft Entra ID** (Azure AD).
2. Grant it `Calendars.ReadWrite`, `OnlineMeetings.ReadWrite` (application permissions).
3. Create a client secret. Store in `MicrosoftTeams:ClientSecret`.
4. Set tenant ID and client ID.
5. Set `MicrosoftTeams:OrganizerUserId` to the UPN of the organizer account (typically the platform's shared service account: `live@platform.app`).

## 2. Creating a meeting

```csharp
var payload = new {
  startDateTime = startAt.UtcDateTime.ToString("o"),
  endDateTime   = endAt.UtcDateTime.ToString("o"),
  subject       = title,
  isOnlineMeeting = true,
  onlineMeetingProvider = "teamsForBusiness"
};
POST https://graph.microsoft.com/v1.0/users/{organizerUserId}/events
Authorization: Bearer <token>
```

Response contains the meeting `id` and `onlineMeeting.joinUrl`. The `joinUrl` is stored server-side only and never returned to students.

## 3. Authentication

We use the **client credentials** flow against Microsoft Identity Platform:

```csharp
var app = ConfidentialClientApplicationBuilder.Create(clientId)
  .WithClientSecret(clientSecret)
  .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
  .Build();
var token = await app.AcquireTokenForClient(
  new[] { "https://graph.microsoft.com/.default" }
).ExecuteAsync();
```

Access tokens are cached in-memory for 50 min (tokens last 60).

## 4. Joining

Identical to Google Meet — students POST `/api/v1/live/sessions/{id}/join`, get a brokered URL with a short-lived token. The Teams join experience is rendered via Microsoft's Communication Services Calling SDK in an iframe in a future release (v2).

## 5. Recordings

`POST /communications/callRecords/{id}` returns the recording URL after the meeting ends. A Hangfire job polls every 5 min after `endAt + 30 min`.

## 6. Fallback chain

If `MicrosoftTeams:OrganizerUserId` is unset, the platform falls back to Google Meet for the same session. This is per-session — the teacher chooses provider at scheduling time.

## 7. Anti-abuse

Same as Google Meet:
- Token single-purpose, single-user
- Join attempts logged in `live_attendance` with outcome
- Rate-limited per user
- Meeting URLs never exposed to clients
