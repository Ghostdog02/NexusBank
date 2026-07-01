# NexusBank

A modern banking platform built with Clean Architecture and CQRS. NexusBank provides account management, transaction processing, and KYC onboarding backed by Stripe Identity and Clerk authentication.

## Tech Stack

- **API** — ASP.NET Core 10, MediatR, FluentValidation
- **Database** — PostgreSQL via EF Core (Npgsql)
- **Auth** — Clerk (JWT + OAuth providers)
- **KYC** — Stripe Identity (progressive onboarding)
- **Mapping** — Mapperly (source-generated)
- **Infrastructure** — Docker, Docker Secrets

## Architecture

Clean Architecture with CQRS. Dependencies flow inward only:

```
Api → Application → Domain
Infrastructure → Application + Domain
```

### Layers

| Layer | Responsibility |
|---|---|
| `NexusBank.Domain` | Entities, value objects, repository interfaces. Zero NuGet dependencies. |
| `NexusBank.Application` | Use cases as MediatR commands/queries. References only Domain interfaces. |
| `NexusBank.Infrastructure` | EF Core, repository implementations, Stripe/Clerk integrations. |
| `NexusBank.Api` | ASP.NET Core entry point. Thin controllers — one action = one `mediator.Send()`. |

### CQRS conventions

Each use case lives in its own folder under the relevant domain concept:

```
Application/Users/Commands/CreateClerkUser/
    CreateClerkUserCommand.cs
    CreateClerkUserHandler.cs
```

Queries return DTOs mapped via Mapperly. Handlers never reference `DbContext`, `HttpContext`, or any infrastructure type — only interfaces defined in Domain.

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker + Docker Compose

### Running locally

```bash
# Copy and fill in secrets
cp .env.example .env.development

# Start PostgreSQL + API
docker compose up

# Or run API directly (requires a running Postgres)
dotnet run --project NexusBank.Api
```

### Useful commands

```bash
dotnet build          # Build all projects
dotnet test           # Run all tests
dotnet watch --project NexusBank.Api   # Hot reload
```

## License

Licensed under the Apache License 2.0. See [LICENSE](LICENSE) for details.
