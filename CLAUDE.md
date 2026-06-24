# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run API
dotnet run --project NexusBank.Api

# Run all tests
dotnet test

# Run a single test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Watch mode (auto-rebuild on save)
dotnet watch --project NexusBank.Api
```

## Architecture

NexusBank uses **Clean Architecture** with **CQRS via MediatR**. There is one solution with four projects — dependencies only flow inward:

```
Api → Application → Domain
Infrastructure → Application + Domain
```

### Layers

**`NexusBank.Domain`** — no NuGet packages, no external dependencies. Contains entities (`User`, `Account`, `Transaction`), value objects (`Money`, `Currency`), domain exceptions, and repository/service interfaces. This is the only place interfaces like `IAccountRepository` are defined.

**`NexusBank.Application`** — contains all use cases as CQRS handlers. Every operation is a `Command` (mutates state) or a `Query` (reads state), routed by MediatR. Handlers reference only Domain interfaces — never EF Core, never Stripe, never HTTP types. FluentValidation validators live alongside their command/query.

**`NexusBank.Infrastructure`** — implements Domain interfaces. EF Core `AppDbContext`, repository implementations, Stripe integration, Serilog setup. Nothing in Domain or Application imports from here; DI wiring in `Api/Extensions/` binds implementations to interfaces at startup.

**`NexusBank.Api`** — ASP.NET Core entry point. Controllers are intentionally thin: deserialize request → map to Command/Query → `mediator.Send()` → return result. Business logic must not live here. Cross-cutting concerns (auth, logging, idempotency) go in `Middleware/` or `Filters/`.

### CQRS conventions

- One folder per use case under the relevant domain concept (e.g. `Application/Users/Commands/CreateUser/`)
- Each folder holds: `*Command.cs` / `*Query.cs`, `*Handler.cs`, `*Validator.cs` (if needed), and a result DTO
- Queries return DTOs, not domain entities

### Key architectural rules

- Domain layer has **zero** NuGet packages
- Handlers never reference `HttpContext`, EF `DbContext`, or any infrastructure type directly — only interfaces defined in Domain
- Controllers never contain business logic; one action = one `mediator.Send()` call
