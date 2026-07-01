# NexusBank — Stripe BaaS Build Roadmap

## Dependency Chain

```
Clerk user synced → Stripe Connect account → KYC verified → FinancialAccount → Cards + Funding → Transactions → Transfers
```

---

## Phase 1 — Stripe Connect Account
> Foundation for Treasury and Issuing. Nothing else works without this.

- `POST /users/me/stripe-account` — create a Stripe Custom Connect account, store `stripeAccountId` on user
- Handle webhook `account.updated` to track onboarding state changes

---

## Phase 2 — KYC / Identity Verification
> Stripe blocks Treasury and Issuing until KYC passes. This is the critical gate.

- `POST /users/me/onboarding-link` — create an Account Link (Stripe-hosted onboarding), return URL to client
- Handle webhook `account.updated` — update `kycStatus` on user (pending → verified → rejected)
- Gate all downstream features behind `kycStatus == verified`

---

## Phase 3 — Financial Account (Treasury)
> Opens a real financial account once KYC clears.

- `POST /accounts` — create `stripe.treasury.financial_accounts` on user's Connect account, store `financialAccountId`
- `GET /accounts/me` — fetch live balance (`cash_balance`) from Stripe
- Handle webhook `treasury.financial_account.features_status_updated`

---

## Phase 4 — Funding (Payments / InboundTransfer)
> Users need a way to put money in.

- `POST /accounts/me/fund` — collect external bank via `SetupIntent`, trigger `InboundTransfer`
- Or accept card top-ups via Stripe Payments (`PaymentIntent`)

---

## Phase 5 — Card Issuance (Issuing)
> Issue a virtual card backed by the FinancialAccount.

- `POST /accounts/me/cards` — create `stripe.issuing.card` tied to `financialAccountId`
- `GET /accounts/me/cards` — list cards with status
- Handle webhook `issuing_authorization.request` — real-time auth decisions
- Handle webhook `issuing_transaction.created` — spend tracking

---

## Phase 6 — Transactions & History
> Unified transaction feed.

- `GET /accounts/me/transactions` — pull from `stripe.treasury.transactions` and `stripe.issuing.transactions`, merge and paginate
- Store locally for fast reads or fetch live from Stripe depending on latency tolerance

---

## Phase 7 — Payments / Outbound Transfers
> Move money out.

- `POST /accounts/me/transfers` — `OutboundPayment` (to external account) or `OutboundTransfer` (between financial accounts)
- Handle webhooks `treasury.outbound_payment.*` for status tracking
