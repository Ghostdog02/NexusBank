# API Endpoints

## Auth
All endpoints require `[Authorize(Policy = "AuthenticatedUser")]` (Clerk JWT).
Webhook endpoints are unauthenticated but signature-verified.

## Users
- `GET /users/me` — returns CurrentUserDto (profile data)
- `POST /users/me/stripe-account` — (Phase 1) creates Stripe Connect account, returns StripeAccountId

## Webhooks
- `POST /webhooks/clerk` — Svix-verified Clerk events (user.created / user.updated / user.deleted)
- `POST /webhooks/stripe` — (Phase 1) Stripe-signed events (account.updated)

## Health
- `GET /health` — liveness
- `GET /health/ready` — readiness (DB check)
