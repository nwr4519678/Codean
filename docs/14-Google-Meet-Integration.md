# 14 — Google Meet Integration

**Last updated:** 2026-08-03

## 1. Why Google Meet (not a raw meeting URL)?

The platform **does not expose raw meeting URLs to students.** Instead, when a student clicks "Join" we:
1. Verify they're entitled (enrolled + subscribed).
2. Issue a **short-lived (15 min) token** server-side.
3. Return a **brokered join URL** that includes the token and the (server-only) meeting ID.

The teacher sees the meeting URL only once when they schedule; students only ever see our broker. This blocks meeting link leakage, prevents drive-by joiners, and gives us a log of every attempt.

## 2. Setup

1. Create a **service account** in Google Cloud.
2. Grant it `Calendar.events` scope + **domain-wide delegation** (impersonate the teacher).
3. Download the service account JSON key.
4. Store the JSON in `GoogleMeet:ServiceAccountJson` (k8s secret).
5. Set `GoogleMeet:ImpersonateUser` to the teacher's email (e.g. `teacher@school.org`).

## 3. Creating a meeting

```
POST /api/v1/live/sessions
{ courseId, title, startAt, endAt, provider: "GoogleMeet" }
```

The handler:
1. Validates the teacher owns the course.
2. Calls `IGoogleMeetProvider.CreateMeetingAsync(...)`:
   ```csharp
   var ev = new Event {
     Summary = title,
     Start = new EventDateTime { DateTimeDateTimeOffset = startAt },
     End   = new EventDateTime { DateTimeDateTimeOffset = endAt },
     ConferenceData = new ConferenceData {
       CreateRequest = new CreateConferenceRequest {
         RequestId = Guid.NewGuid().ToString("N"),
         ConferenceSolutionKey = new() { Type = "hangoutsMeet" }
       }
     }
   };
   var created = await calendar.Events.Insert(ev, "primary").ExecuteAsync();
   return new CreateMeetingResult(
     MeetId:        created.ConferenceData.ConferenceId,
     OrganizerUrl:  created.HangoutLink,    // server-only!
     ExpiresAt:     now + 24h
   );
   ```
3. Persists the meeting in `live_sessions` with `OrganizerUrl` set and `ProviderMeetingId` saved.
4. Returns the meeting ID to the teacher (without the link).

The `OrganizerUrl` is **never** returned to a student.

## 4. Joining (student flow)

```
Student clicks "Join" at start time
  → POST /api/v1/live/sessions/{id}/join
  → [Authorize] + [RequireEnrollment] + [RequireSubscription if paid]
  → Record LiveAttendance { outcome: Allowed }
  → Issue LiveSessionToken { tokenHash(SHA-256(secret+userId+sessionId+exp)),
                              expiresAt: now + 15min }
  → Return { joinUrl, expiresAt }
    where joinUrl = "{FrontendBase}/live/{id}?token=..."
```

The frontend then opens the joinUrl in an iframe. The `live/[id]/page.tsx` extracts the token, calls back to the API, and the API revalidates the token hash, then proxies a **single-use** URL to the meeting room (or embeds it via Google Meet's SDK in a future release).

## 5. Recordings

When the teacher ends the meeting, Google Meet stores the recording in their Drive. The `events.get` endpoint returns a `ConferenceData` with a `RecordingUri` once it processes. A Hangfire job polls this every 5 min after `endAt + 30 min`. When available, the recording URL is attached to the session and a notification is sent to all students.

## 6. Cancellation

If a teacher cancels before the meeting starts, the calendar event is deleted (`Events.Delete`). After the meeting has started, the meeting is simply left to end naturally; `LiveSession.Status` is set to `Cancelled`.

## 7. Security & access control

- Tokens are **single-purpose, single-user**; they cannot be reused by a different user.
- Token revocation list kept in Redis with 15-min TTL.
- All join attempts (allowed + denied) are written to `live_attendance` with IP, user-agent, and outcome.
- Rate-limit join attempts per user (10 / 5 min) to prevent token brute-force.
