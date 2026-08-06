# 13 — Paymob Integration

**Last updated:** 2026-08-03

## 1. Overview

Paymob is the only payment service provider (PSP) in v1. The platform uses Paymob's **hosted checkout** flow — the customer is redirected to Paymob, completes payment there, and returns. Card data never touches our servers, keeping us out of PCI-DSS scope beyond SAQ-A.

## 2. Endpoints used

| Purpose | Paymob endpoint | Direction |
|---|---|---|
| Create payment intention | `POST /v1/intention/` | Outbound |
| Verify transaction | `GET /api/acceptance/transactions/{id}` | Outbound (defence in depth) |
| Webhook (transaction processed) | `POST /api/v1/webhooks/paymob` | Inbound |
| Webhook (tokenization) | `POST /api/v1/webhooks/paymob` | Inbound |

## 3. Happy path (purchase monthly package)

```
1. Student clicks "Subscribe" on Month 1 (packageId = X).
2. Web → POST /api/v1/payments/orders  { packageId: X, couponCode? }
3. Api → MediatR CreateOrderCommand
4. Handler:
     a. Check idempotency-key from request header
     b. Load MonthlyPackage
     c. Validate coupon (if any), apply discount
     d. Create Order { status: Pending, amount, total, idempotencyKey }
     e. SaveChanges (transaction)
     f. Call IPaymobClient.CreateIntention(...) → { clientSecret, checkoutUrl, paymobOrderId }
     g. Update Order.PaymobOrderId
     h. Return { orderId, checkoutUrl }
5. Web → window.location = checkoutUrl
6. Student pays on Paymob hosted page
7. Paymob → POST /api/v1/webhooks/paymob
     Body = { type, obj: { id, success, order: { id }, amount_cents, currency, ... } }
     Headers = X-Paymob-Signature: <hmac_sha256>
8. Api/WebhookController:
     a. Read raw body (no model binding yet)
     b. Compute HMAC of body with HmacSecret, compare in constant time
     c. If invalid → 401
     d. If valid → ProcessPaymobWebhookCommand
9. Handler:
     a. Parse payload, extract transactionId, orderId, success
     b. Idempotency: Payment.ProviderTransactionId unique
     c. If success: Order.MarkPaid(paymentId), create UserPackageAccess with 365-day TTL, emit events:
        - Invoice.Issue (number, PDF, R2)
        - Email (invoice + receipt)
        - In-app notification
        - Analytics: revenue +1
     d. If failure: Order.MarkFailed, no access granted
10. Web (on return) → GET /api/v1/subscriptions/me → shows active
```

## 4. Webhook security

- HMAC SHA-256 over the **raw** request body.
- Constant-time comparison (Polly/BCL `CryptographicOperations.FixedTimeEquals`).
- Header name: `X-Paymob-Signature`.
- Secret: stored in `Paymob:HmacSecret` config (k8s secret / vault).
- Replay protection: idempotency on `transaction.id` — same transaction processed at most once.

## 5. Idempotency

Both sides of the flow are idempotent:

- **Create order:** `Idempotency-Key` header (or generated). Same key returns the same order, never creates a duplicate.
- **Webhook:** `Payment.ProviderTransactionId` unique index. Re-processing the same webhook is a no-op.

## 6. Payment methods supported

Cards (Visa, Mastercard, Meeza), mobile wallets (Vodafone Cash, Orange Cash, Etisalat Cash), BNPL (ValU, Sympl), Apple Pay if enabled on the merchant account. The hosted page lets the customer pick — we only configure the integration IDs.

## 7. Coupons & discounts

Discounts are applied **before** creating the Paymob intention so Paymob is told the final amount. Coupon redemption is recorded in `commerce.coupon_redemptions` and `coupon.used_count` is incremented only on `Order.MarkPaid`.

## 8. Refunds

1. Admin calls `POST /admin/refunds { paymentId, amount, reason }`.
2. Backend creates a `Refund` row with `Status = pending`.
3. Hangfire job `ProcessRefund` calls Paymob's refund API.
4. Result updates `Refund.Status`.
5. If full refund, the related `UserPackageAccess` is revoked.

## 9. Testing

- Use Paymob's **test card** `4987654321098769` with any future expiry and CVV `123`.
- Webhook can be triggered manually from the Paymob dashboard.
- Local development: use `ngrok` or similar to expose your local API.

## 10. Failure handling

| Failure | Behavior |
|---|---|
| Intention call fails | Return `Error.Provider`, user can retry |
| Webhook signature invalid | Return 401, log full payload at Warn |
| Webhook processed twice | Idempotency check, no-op |
| Order stuck in Pending > 1 h | Hangfire sweep job marks as `Failed`, notifies user |
| Network timeout on Paymob call | Polly retry (3x exp backoff) + circuit breaker |
