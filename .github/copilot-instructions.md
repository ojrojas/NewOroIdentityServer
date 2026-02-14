# Project Guidelines

## Code Style
- Use C# 12 features with `net10.0` target framework.
- Follow standard .NET naming conventions (PascalCase for classes, camelCase for variables).
- Use global usings in `GlobalUsings.cs` for common namespaces (e.g., `System`, `Microsoft.Extensions.DependencyInjection`).
- Reference key files: [src/Services/OroIdentityServer/OroIdentityServer.Core/GlobalUsings.cs](src/Services/OroIdentityServer/OroIdentityServer.Core/GlobalUsings.cs)

## Architecture
- Follow Domain-Driven Design (DDD) with layered architecture: Core (Domain), Application, Infrastructure, Server (Presentation).
- Use CQRS pattern via OroCQRS for commands/queries separation.
- Integrate with .NET Aspire using AppHost for service orchestration.
- Major components: Entities in Core, Handlers in Application, Repositories in Infrastructure, APIs in Server.
- Data flows: Commands → Handlers → Domain Services → Repositories → DbContext.
- Example structure: [src/Services/OroIdentityServer/](src/Services/OroIdentityServer/) with Core/Application/Infrastructure/Server layers.

## Build and Test
- Restore: `dotnet restore`
- Build: `dotnet build`
- Test: `dotnet test` (uses xUnit, Moq, EF Core InMemory)
- Run AppHost: `dotnet run` in [src/AppHost/](src/AppHost/)
- Run Service: `dotnet run` in service's Server project
- Pack NuGets: `dotnet pack` (for local packages like OroKernel.Shared)

## Project Conventions
- Entities inherit from `BaseEntity<T, TId>` (e.g., `BaseEntity<User, Guid>`) for strong-typed IDs.
- Use `AuditableDbContext` for automatic auditing with `AuditEntry` records.
- CQRS handlers in separate folders: Commands/Queries under Application layer.
- Register handlers with `builder.Services.AddCqrsHandlers()`.
- Value objects inherit from `BaseValueObject` with equality overrides.
- Example: User entity in [src/Services/OroIdentityServer/OroIdentityServer.Core/Entities/](src/Services/OroIdentityServer/OroIdentityServer.Core/Entities/)

## Integration Points
- Use OroServiceDefaults for observability: `builder.AddServiceDefaults()` adds OpenTelemetry, health checks (/alive, /health), resilience, and service discovery.
- Domain events via OroCQRS notifications for cross-service communication.
- Identity integration with `ClaimsUserInfoService` for auditing user context.
- External dependencies: ASP.NET Core Identity, JWT, OpenID Connect.

## Security
- Implement ASP.NET Core Identity for authentication/authorization.
- Use role-based access with claims.
- Automatic auditing captures user actions via `AuditableDbContext`.
- Multi-tenancy with `TenantId` for data isolation.
- Secure APIs with JWT Bearer tokens and OpenID Connect.
- Sensitive areas: User credentials, audit logs, tenant data.

### Identity Server — Missing / TODO (summary)
- Refresh token rotation & revocation
- Client auth on /token (confidential clients)
- ID Token (scope=openid)
- Discovery (.well-known) + JWKS
- Introspection & revocation RFC compliance
- Secure signing key management (rotation)
- Userinfo, logout, consent, tests

(See full TODO section tracked in repository documentation: `.github/IDENTITY_SERVER_TODO.md`)