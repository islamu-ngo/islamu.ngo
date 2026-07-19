<!-- ABOUTME: Describes the first-party ISLAMU website authentication, authorization, and trust boundaries. -->
<!-- ABOUTME: Separates enforced runtime controls from integration foundations and deployment responsibilities. -->

# Security Model

> **Audience:** Operators | Contributors | AI agents
> **Status:** Active
> **Owner:** Security
> **Last Verified:** 2026-07-19
> **Source Anchors:** `src/Explore.API/Extensions/AuthenticationExtensions.cs`, `src/Explore.Blazor/Program.cs`, `src/Explore.Blazor/Extensions/YarpProxyExtensions.cs`, `src/Event.Web.BffHosting/Authentication/EventBffAuthenticationExtensions.cs`, `src/Explore.Blazor.Client/Services/Http/BffAntiforgeryMessageHandler.cs`, `src/Explore.Application/Authorization/ResourceKinds.cs`, `src/Explore.Infrastructure/Services/RuntimeAuthorizationProvider.cs`, `src/Explore.Infrastructure/Services/FallbackAuthorizationService.cs`

## Runtime Boundary

The first-party ISLAMU website uses a Backend for Frontend (BFF) boundary:

1. Explore.Blazor owns the browser OIDC login and HttpOnly cookie session.
2. Browser API calls use the same-origin /api proxy.
3. The server-side YARP path acquires and forwards the access token; browser code does not store bearer tokens.
4. Explore.API validates the bearer token and endpoint policy.
5. Application handlers perform resource-level authorization before persistence or provider work.
6. API resource assemblers publish authorized actions as HAL links. The UI must show an action only when the corresponding link exists.

Explore.Blazor currently registers Interactive Server rendering only. Explore.Blazor.Client contains Razor components and service contracts, but it is not a standalone token-owning WebAssembly security boundary.

## Authentication

Explore.API validates Keycloak-issued JWTs. The source defaults are:

- valid audiences: islamu-ngo-api and islamu-ngo-blazor;
- audience matching against both aud and Keycloak azp;
- issuer, lifetime, and signing-key validation;
- five-minute clock skew.

The authentication diagnostics path records safe claim metadata, not tokens or secret material. Account lifecycle email such as verification, reset, and required-action messages remains owned by the configured identity authority. Application email is handled separately by the platform email-dispatch subsystem.

## Browser Request Protections

- Authentication cookies are server-owned and HttpOnly.
- BffAntiforgeryMessageHandler adds the antiforgery header for unsafe same-origin API calls.
- YARP forwards access tokens on the server.
- CORS is configured in the API rather than delegated to UI code.
- Rate limiting and request validation are defense-in-depth; they do not replace authorization.
- Content Security Policy and upload rules must permit only the origins and media types needed by the first-party website.

## Authorization

Authorization has four layers:

| Layer | Enforcement |
|---|---|
| Endpoint | AllowAnonymous or Authorize metadata establishes the request boundary. |
| Application | MediatR authorization behaviors evaluate an action against a resource descriptor. |
| Provider | RuntimeAuthorizationProvider selects Cerbos or the local fallback according to governed configuration. |
| Representation | HAL link policies expose only actions the caller may perform. |

Cerbos failures and invalid provider configuration fail closed. Tenant configuration may select an approved provider, but a tenant cannot bypass platform endpoint or handler checks. See [AUTHORIZATION.md](AUTHORIZATION.md).

The islamuevent_* strings in ResourceKinds and Cerbos policies are stable technical authorization identifiers, not active product branding. Current identifiers cover live website/platform resources such as tenants, organizations, groups, users, Blog content, storage objects, email dispatch, webhooks, notifications, and federation foundations.

Retired product-domain resources are not part of the current authorization surface.

Custom-property authorization remains live for shared definitions, templates, values, and governance used by current organization and group capabilities.

## Tenant And Organization Isolation

Tenant resolution is an input to authorization, persistence, provider routing, and webhook processing. Tenant-scoped requests must not trust a client-supplied identifier without reconciling it with authenticated context and the addressed resource.

Defense in depth includes:

- tenant context at controller and handler boundaries;
- EF Core tenant query filters and tenant-aware repositories;
- resource descriptors containing the tenant and ownership attributes needed by the provider;
- tenant-scoped provider credentials and settings;
- organization membership checks for organization-owned operations;
- support access sessions and tenant role grants as explicit resources.

Any row-level-security prototype remains secondary to application tenant enforcement. Examples and validation must use current tables; retired product tables are not security evidence.

## Machine Credentials And API Keys

External API keys are hashed before persistence and map to explicit machine scopes. Current scope families include organizations, groups, users, lookups, MCP read/propose, API-key management, and tenant/instance administration. No retired product-domain machine scopes remain.

Machine callers remain subject to:

- allowed-scope mapping;
- tenant binding;
- resource-level checks;
- the same provider fail-closed behavior as interactive callers.

## Webhook Trust Boundaries

Outgoing webhooks materialize immutable tenant-scoped message and delivery-plan evidence before network delivery. Local delivery signs the exact payload bytes, protects secret references, rejects unsafe endpoint targets, and records retry outcomes. Svix modes preserve the application ledger while delegating supported delivery capabilities to a configured Svix provider.

Incoming webhook routes are provider-specific. The only public provider callback currently exposed is the Svix operational intake. Its verification, tenant resolution, replay/idempotency handling, settlement receipts, and redrive records are separate from outgoing endpoint delivery.

No Coop or Osprey callback route or verifier is registered in the current API. Do not describe either provider as an active first-party report-moderation path. See [WEBHOOKS.md](WEBHOOKS.md), [INTEGRATIONS.md](INTEGRATIONS.md), and [OSPREY_INTEGRATION.md](OSPREY_INTEGRATION.md).

## Storage And Email

Storage access is authorized as islamuevent_storage_object. Upload validation, object ownership, download authorization, provider credentials, and tenant isolation are server responsibilities; a public object URL must not become an authorization bypass.

Application mail is authorized as islamuevent_email_dispatch and routed through the configured email provider. Identity-authority mail remains outside this subsystem. Logs must not contain message secrets, provider credentials, raw tokens, or unnecessary recipient content.

## Federation Boundary

ATProto entities, internal CRUD resources, and outbound PDS synchronization are present. Public ATProto OAuth, WebFinger, ActivityPub, and a hosted PDS protocol surface are not. AtprotoAuthenticationHandler deliberately returns no authenticated identity and a 501 challenge. See [FEDERATION.md](FEDERATION.md).

## Secrets And Logging

- Keep OIDC client secrets, API-key material, webhook signing secrets, storage keys, email credentials, Cerbos credentials, Coop keys, and PDS credentials in the configured secret provider.
- Store secret references where the model permits; never expose resolved values through HAL, DTOs, logs, or metrics.
- Prefer structured security events containing tenant, resource, action, outcome, and correlation identifiers.
- Never log access tokens, cookies, raw webhook secrets, or full incoming/outgoing payloads merely for diagnostics.

## Verification Boundary

Static source and policy inspection establish the behavior documented here. Deployment-specific controls such as TLS termination, external Keycloak backup, network policy, Coolify configuration, secret rotation, and provider availability require operator evidence in the target environment.
