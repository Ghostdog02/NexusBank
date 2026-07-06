# Application Layer — CQRS Patterns

## Folder structure
```
Application/
  Users/
    Commands/
      CreateClerkUser/   → CreateClerkUserCommand, Handler, Validator
      UpdateClerkUser/   → UpdateClerkUserCommand, Handler, Validator
      CloseClerkUser/    → CloseClerkUserCommand, Handler
      CreateStripeAccount/ → (Phase 1)
      HandleStripeAccountUpdated/ → (Phase 1)
    Queries/
      GetCurrentUser/    → Query, Handler, Dto, Mapper
  Common/
    Behaviours/          → ValidationBehaviour (pipeline)
    Services/            → ICurrentUserService, IStripeConnectService (Phase 1)
    Authorization/       → RoleRequirement
    Validators/          → shared validators
```

## Command pattern
- `record XCommand(...) : IRequest` (void) or `: IRequest<TResult>`
- `class XHandler(...) : IRequestHandler<XCommand>` or `IRequestHandler<XCommand, TResult>`
- Handler dependencies: only Domain interfaces (IUserRepository, ICurrentUserService, etc.)
- Handlers never reference EF, HttpContext, or Stripe SDK directly

## Interfaces defined in Application (not Domain)
- `ICurrentUserService` — resolves the authenticated Clerk user to a domain User
- `IStripeConnectService` — abstracts Stripe Connect account operations
