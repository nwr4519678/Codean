# 04 — User Stories

**Version:** 1.0
**Last updated:** 2026-08-03

> Format: *As a `<role>`, I want `<capability>`, so that `<value>`.*
> Each story is sized (S/M/L/XL) and tagged for traceability.

---

## Student

| ID | Story | Size | Tags |
|---|---|---|---|
| US-S-01 | As a **student**, I want to **sign up with email** so that I can access free content. | S | auth |
| US-S-02 | As a **student**, I want to **sign in with Google** so that I don't have to remember a password. | S | auth |
| US-S-03 | As a **student**, I want to **enable 2FA** so that my account is safer. | M | auth, 2fa |
| US-S-04 | As a **student**, I want to **browse the catalog** so that I can find a course. | M | catalog |
| US-S-05 | As a **student**, I want to **enroll in a free course** so that I can start learning immediately. | S | enrollment |
| US-S-06 | As a **student**, I want to **pay for Month 1** so that I can access paid content. | M | payments |
| US-S-07 | As a **student**, I want to **apply a coupon** so that I get a discount. | S | payments |
| US-S-08 | As a **student**, I want to **watch a video lesson** so that I learn the topic. | S | lessons, video |
| US-S-09 | As a **student**, I want to **track my progress** so that I know how far I've come. | S | progress |
| US-S-10 | As a **student**, I want to **join a live session** so that I attend class online. | M | live |
| US-S-11 | As a **student**, I want to **submit a coding assignment** so that I get a grade. | M | assignments, code |
| US-S-12 | As a **student**, I want to **see auto-graded test results** so that I know what failed. | S | assignments, code |
| US-S-13 | As a **student**, I want to **take a timed exam** so that I get certified. | L | exams |
| US-S-14 | As a **student**, I want to **earn XP, levels, badges** so that I stay motivated. | M | gamification |
| US-S-15 | As a **student**, I want to **see my certificate** so that I can share it. | S | certificates |
| US-S-16 | As a **student**, I want to **download a PDF invoice** so that I can expense it. | S | payments |
| US-S-17 | As a **student**, I want to **get push notifications** so that I don't miss live sessions. | M | notifications |
| US-S-18 | As a **student**, I want to **use the platform on my phone (PWA)** so that I learn on the go. | M | pwa |
| US-S-19 | As a **student**, I want to **search across courses and lessons** so that I find content fast. | S | search |
| US-S-20 | As a **student**, I want to **view my subscription status** so that I know what's active. | S | subscriptions |

## Teacher

| ID | Story | Size | Tags |
|---|---|---|---|
| US-T-01 | As a **teacher**, I want to **create a course** so that I can publish content. | M | courses |
| US-T-02 | As a **teacher**, I want to **add modules, lessons, units** so that I structure my curriculum. | M | courses |
| US-T-03 | As a **teacher**, I want to **upload videos and PDFs** so that I share materials. | S | library |
| US-T-04 | As a **teacher**, I want to **create a monthly package (Month 1, Month 2, ...)** so that I can monetize progressively. | M | monetization |
| US-T-05 | As a **teacher**, I want to **set price and visibility per package** so that I control access. | S | monetization |
| US-T-06 | As a **teacher**, I want to **schedule a live session** so that my students can attend. | M | live |
| US-T-07 | As a **teacher**, I want to **grade submissions with a rubric** so that grading is consistent. | M | grading |
| US-T-08 | As a **teacher**, I want to **create an exam with a question bank** so that I randomize per student. | L | exams |
| US-T-09 | As a **teacher**, I want to **view revenue analytics** so that I track my income. | M | analytics |
| US-T-10 | As a **teacher**, I want to **see student progress per course** so that I identify who needs help. | M | analytics |
| US-T-11 | As a **teacher**, I want to **send an announcement** so that I update all enrolled students. | S | comms |
| US-T-12 | As a **teacher**, I want to **create coupons and promo codes** so that I run campaigns. | M | monetization |
| US-T-13 | As a **teacher**, I want to **issue scholarships** so that I support students in need. | S | monetization |
| US-T-14 | As a **teacher**, I want to **set up my payout method** so that I receive revenue. | M | payments |
| US-T-15 | As a **teacher**, I want to **bulk-import questions from CSV** so that I save time. | S | exams |

## Teacher Assistant

| ID | Story | Size | Tags |
|---|---|---|---|
| US-TA-01 | As a **TA**, I want to **grade submissions in courses I'm assigned to** so that I help the teacher. | M | grading |
| US-TA-02 | As a **TA**, I want to **see only my assigned courses** so that I don't see others' data. | S | rbac |
| US-TA-03 | As a **TA**, I want to **reply to student questions** so that I support learning. | S | comms |

## School Admin

| ID | Story | Size | Tags |
|---|---|---|---|
| US-SA-01 | As a **school admin**, I want to **invite teachers to my school** so that they can publish. | M | schools |
| US-SA-02 | As a **school admin**, I want to **view aggregated performance** so that I report to the principal. | M | analytics |
| US-SA-03 | As a **school admin**, I want to **issue scholarships from the school's budget** so that we support students. | M | monetization |
| US-SA-04 | As a **school admin**, I want to **set school-wide policies** (e.g. anti-cheat) so that they apply across courses. | M | admin |

## Platform Admin

| ID | Story | Size | Tags |
|---|---|---|---|
| US-PA-01 | As a **platform admin**, I want to **approve teacher applications** so that we vet quality. | M | moderation |
| US-PA-02 | As a **platform admin**, I want to **refund a payment** so that I resolve disputes. | S | payments |
| US-PA-03 | As a **platform admin**, I want to **manage the question of the day banner** so that I drive engagement. | S | content |
| US-PA-04 | As a **platform admin**, I want to **search audit logs** so that I investigate incidents. | M | security |
| US-PA-05 | As a **platform admin**, I want to **toggle feature flags** so that we can roll out gradually. | M | ops |

## Super Admin

| ID | Story | Size | Tags |
|---|---|---|---|
| US-SU-01 | As a **super admin**, I want to **create platform admins** so that I delegate. | S | rbac |
| US-SU-02 | As a **super admin**, I want to **manage payment provider keys** so that we can switch providers. | S | ops |
| US-SU-03 | As a **super admin**, I want to **view infrastructure metrics** so that I oversee health. | M | ops |

## Support

| ID | Story | Size | Tags |
|---|---|---|---|
| US-SUP-01 | As a **support agent**, I want to **impersonate a user** so that I can reproduce issues. | M | security, audit |
| US-SUP-02 | As a **support agent**, I want to **view a user's order history** so that I resolve payment issues. | S | payments |
| US-SUP-03 | As a **support agent**, I want to **reset a user's 2FA** so that I unblock them. | S | auth |
| US-SUP-04 | As a **support agent**, I want to **view audit logs for a user** so that I investigate. | S | security |

## Parent

| ID | Story | Size | Tags |
|---|---|---|---|
| US-P-01 | As a **parent**, I want to **link to my child's account** so that I can monitor progress. | M | parent |
| US-P-02 | As a **parent**, I want to **see grades and attendance** so that I stay informed. | S | parent |
| US-P-03 | As a **parent**, I want to **pay for my child's subscription** so that they keep learning. | M | payments |
| US-P-04 | As a **parent**, I want to **get a weekly performance email** so that I don't have to log in. | S | comms |
| US-P-05 | As a **parent**, I want to **unlink from my child's account** so that I respect their privacy. | S | parent |
