# Identity Server — Missing / TODO (detailed)

This file expands the brief TODO in `copilot-instructions.md` into actionable tasks, priorities, and file references to track progress on finishing OAuth2.1 / OpenID Connect support.

## High priority (must-have / security)
- [x] Refresh token grant & rotation
  - Implement refresh grant endpoint handling in `ConnectEndpoints`.
  - Rotate refresh token on use, revoke previous refresh token, update `UserSession` status.
  - Add unit + integration tests for rotation and reuse/replay protection.
  - Files: `Server/Services/TokenService.cs`, `Server/Endpoints/ConnectEndpoints.cs`, `Application/UserSession/*`.

- [x] Client authentication on `/token`
  - Support HTTP Basic auth and form POST for confidential clients; validate `client_secret`.
  - Reject public client requests requiring client authentication when secret is expected.
  - Files: `Server/Endpoints/ConnectEndpoints.cs`, `Application/Application/Queries`.

- [x] ID Token (OpenID Connect)
  - Issue `id_token` when `scope=openid` with standard claims (iss, sub, aud, exp, iat, auth_time, nonce).
  - Sign ID tokens; support `nonce` verification at authentication time.
  - Files: `Server/Services/TokenService.cs`.

- [x] Discovery & JWKS
  - Publish `/.well-known/openid-configuration` and `/jwks.json` (key id support for rotation).
  - Files: new `Server/Endpoints/OpenIdEndpoints.cs`.

 - [x] Token revocation & introspection
  - Status: core functionality implemented: `/connect/introspect`, `/connect/revoke`, DB-backed `RevokedJti` persistence, `IRevocationService` (DB + in-memory), and token revocation logic in `TokenService`.
  - Notes: Introspection and revocation flow are functional; recommend adding RFC-7662 exact field formatting, additional tests (end-to-end), and review in-memory service (has unimplemented explicit interface methods).
  - Files: `Server/Services/TokenService.cs`, `Server/Services/RevocationServiceDb.cs`, `Server/Services/InMemoryRevocationService.cs`, `Infraestructure/Data/EntityConfigurations/RevokedJtiEntityConfiguration.cs`, `Application/Token/*`.

 - [x] Secure signing key management
  - Remove plaintext signing keys from `appsettings` for production; support asymmetric keys and rotation.
  - Publish corresponding `kid` in JWKS and expose key rotation policy.

## Medium priority (spec completeness & UX)
 - [x] `userinfo` endpoint
 - [ ] End session / logout endpoints (front/back-channel)
  - Status: **implemented**. Front-channel `/connect/logout` and back-channel `/connect/backchannel_logout` endpoints added to `ConnectEndpoints.cs`. They deactivate `UserSession` via `UpdateUserSessionCommand` and revoke JWT `jti` values via `IRevocationService`.
  - Tests: basic integration test added under `OroIdentityServer.Infraestructure.Tests/Logout/LogoutIntegrationTests.cs` verifying session deactivation via command handler.
 - [ ] Consent UI and consent persistence
 - [x] Consent UI and consent persistence
  - Status: **implemented (basic)**. Server-side consent persistence (`Consent` entity), `POST /connect/consent` and `GET /connect/consent` endpoints, application commands/queries and infra repository added. Recommend adding a UI prompt and E2E tests.

## Low priority / optional
- [ ] Device Authorization Grant
- [ ] Token Exchange (RFC 8693)
- [ ] Private Key JWT / mTLS client auth

## Security & operational items
- [ ] Enforce PKCE for public clients (consider disallowing `plain`).
  - Note: PKCE verification is implemented in `TokenService`. Added `EnforcePkcePlain` option and policy to reject `plain` when enabled.
  - Status: **partial** — PKCE `plain` enforcement available in configuration; recommend adding tests and policy coverage in endpoints.
- [ ] Rate limiting / brute-force protection on `/token` and `/authorize`.
- [ ] Logging, telemetry and alerts for auth flows.

## Tests & QA
- [ ] Unit tests for PKCE S256/plain, token creation, refresh rotation, revocation.
  - Note: some unit/integration tests exist (application & infrastructure projects). Several infra tests were added for revocation, but the test suite requires stabilization; many broader E2E tests are still missing.
- [ ] Integration tests: end-to-end Authorization Code + PKCE, refresh token rotation, revocation/introspection.
- [ ] E2E and sample client artifacts (Postman collection, example apps).

## Files / areas to update
- Server: `Server/Services/TokenService.cs`, `Server/Endpoints/ConnectEndpoints.cs`, `Server/Endpoints/OpenIdEndpoints.cs`.
- Application: `Application/Token/*`, `Application/UserSession/*`, `Application/Application/*` (client auth).
- Infra: persistence for reference tokens and revocation storage.
- Config: move secrets to secure stores (KeyVault / environment / managed identity).

## Short-term roadmap (recommended)
1. Implement **refresh token rotation + tests** (high priority).  (COMPLETE)
2. Add client authentication and validate confidential clients on `/token`. (COMPLETE)
3. Implement ID Token issuance and OIDC discovery/JWKS. (COMPLETE)
4. Harden introspection & revocation; add revocation support for JWTs. (COMPLETE — recommend additional RFC-7662 formatting and E2E tests)

---
