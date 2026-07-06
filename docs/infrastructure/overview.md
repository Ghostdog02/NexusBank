# Infrastructure Layer

## Persistence
- ORM: EF Core 10 + Npgsql + snake_case naming convention
- DbContext: `NexusDbContext` with DbSets for User, UserIdentity, UserProfile
- Configurations in `Persistence/Configurations/` (IEntityTypeConfiguration<T>)
- Repositories in `Persistence/Repositories/` implement Domain interfaces

## External services
- Swan.io BaaS (StrawberryShake-generated typed GraphQL client) — `SwanService` implements `ISwanService`; see `docs/infrastructure/swan-integration.md`
- Svix — used in Api layer for Clerk webhook signature verification

## NuGet packages (Infrastructure)
- Npgsql.EntityFrameworkCore.PostgreSQL 10.0.2
- EFCore.NamingConventions 10.0.1
- Microsoft.EntityFrameworkCore.Design 10.0.9
- DotNetEnv 3.2.0
- StrawberryShake

## DI wiring (Program.cs)
- `IUserRepository` → `UserRepository` (Scoped)
- `ICurrentUserService` → `CurrentUserService` (Scoped, lives in Api)
- `ISwanService` → `SwanService` (Scoped)
- `SwanTokenProvider` (Singleton, backed by `IMemoryCache`)
