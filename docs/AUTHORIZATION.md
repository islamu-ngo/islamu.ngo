<!-- ABOUTME: Documents authorization for the first-party ISLAMU website and platform resources. -->
<!-- ABOUTME: Covers endpoint, CQRS, provider, tenant, machine-scope, and HAL enforcement boundaries. -->

# Authorization

> **Audience:** Contributors | Security | AI agents
> **Status:** Active
> **Owner:** Security
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.Application/Authorization/ResourceKinds.cs`, `src/Explore.Application/Authorization/AuthorizationActions.cs`, `src/Explore.Application/Authorization/MachineScopeMapping.cs`, `src/Explore.Domain/Constants/ExternalApiKeyScopes.cs`, `src/Explore.Infrastructure/Services/RuntimeAuthorizationProvider.cs`, `src/Explore.Infrastructure/Services/FallbackAuthorizationService.cs`, `cerbos/policies/`

## Model

Authentication establishes the caller. Authorization decides whether that caller may perform an action on a concrete resource. The platform deliberately enforces this decision at several layers:

1. Controller metadata establishes anonymous versus authenticated routes.
2. MediatR authorization behaviors evaluate resource actions before the handler mutates state.
3. RuntimeAuthorizationProvider routes supported decisions to Cerbos or the local fallback provider.
4. Repositories and EF Core query filters retain tenant isolation at the data boundary.
5. HAL link policies advertise only actions authorized for the current representation.

Endpoint authorization is not a substitute for handler authorization. A role claim is not sufficient evidence that a caller owns, belongs to, or may mutate a particular tenant-scoped resource.

## Resource And Action Vocabulary

ResourceKinds and AuthorizationActions are the source of truth. Current resources include:

- tenant, tenant setting, tenant user role grant, and support access session;
- organization, organization member, organization review, group, and group member;
- user, location, location room, category, and tag;
- Blog, notification, storage object, email dispatch, and AI conversation;
- webhook and platform namespace;
- actor, actor subscription, ATProto record, and indexed DID;
- instance setting;
- custom-property definition, template, value, and governance.

The islamuevent_* values are retained technical identifiers used by source, policy files, tests, and stored authorization contracts. They are not product branding and must not be renamed casually.

Retired product-domain resources are not current authorization vocabulary. Documentation and policies must not recreate their former routes or decision tables.

## Endpoint Boundary

Project convention is:

- GET endpoints are anonymous unless a sensitive controller explicitly establishes an authenticated resource boundary.
- write endpoints require authentication.
- resource-level access remains a separate Application-layer decision.

Controllers such as the webhook management surface establish an authenticated class boundary and may selectively expose an anonymous catalog endpoint. Incoming provider callbacks may be anonymous at HTTP authentication level only because provider signature verification and intake controls establish a different trust boundary.

## CQRS Enforcement

Commands and queries that need a resource decision declare the action and resource kind used by the authorization behavior. Handlers then work with an already authorized request and still enforce domain invariants and tenant-safe persistence.

A typical decision includes:

- caller subject and normalized roles;
- tenant identifier;
- resource kind and stable resource identifier;
- action;
- ownership, membership, status, or governance attributes required by policy.

Pre-create checks use a stable prospective resource descriptor rather than a fake persisted entity. Existing-resource checks must load only enough tenant-safe state to form the descriptor and then authorize before mutation.

## Runtime Providers

### Cerbos

Cerbos provides policy decisions for the stable islamuevent_* resource vocabulary. Repository policy files are source-controlled under cerbos/policies, with native policy tests under cerbos/tests. A deployment that selects an external Cerbos PDP must use the compatible policy bundle and fail closed when the PDP is unavailable or returns an invalid result.

### Local fallback

FallbackAuthorizationService provides the application-owned local decision path. It evaluates the same resource/action contract, including machine callers. It is not an allow-all emergency mode.

### Provider routing

RuntimeAuthorizationProvider resolves the governed provider selection and tenant context. Tenant-specific provider configuration may route a tenant to an approved Cerbos instance, while the first-party control plane retains governance of allowed modes and required secrets. Missing, invalid, or unhealthy provider configuration must not silently grant access.

## Roles, Claims, And Machine Scopes

Role claims support provider decisions but do not authorize UI affordances by themselves. Administrative capabilities are still bounded by tenant, resource, action, and governance state.

Machine API keys use explicit scopes from ExternalApiKeyScopes and MachineScopeMapping. Current scope families are:

- organizations:read and organizations:write;
- groups:read and groups:write;
- users:read and users:write;
- lookups:read;
- mcp:read and mcp:propose;
- api-keys:manage;
- admin:tenant and admin:instance.

Retired product-domain machine scopes are absent. Machine access remains tenant-bound and resource-authorized.

## Current Capability Boundaries

| Capability | Authorization boundary |
|---|---|
| Tenant and organization administration | Tenant/organization resource, membership or grant, governed action |
| Groups | Group and group-member resources, organization/tenant context |
| Blog | islamuevent_blog actions and HAL link policy |
| Webhooks | islamuevent_webhook actions for consumers, endpoints, provider state, retries, portal access, and redrive |
| Storage | islamuevent_storage_object ownership and tenant context |
| Application email | islamuevent_email_dispatch action and tenant/provider configuration |
| Support access | Explicit session resource; never an implicit global bypass |
| Federation foundation | Actor/ATProto/indexed-DID resources; protocol availability remains separate |
| Custom properties | Current definition/template/value/governance resources only |

Coop and Osprey do not currently expose moderation-report resources or provider callback routes. Their retained configuration and infrastructure do not create an authorization surface by themselves.

## HAL Is The UI Authority

The API is the authorization authority. A Blazor component must gate Edit, Delete, Retry, Rotate, Open Portal, or similar resource actions by the corresponding HAL link in the returned representation.

The UI must not reconstruct resource authorization from local role, claim, route, tenant, or provider checks. Claims may select broad navigation or explain identity state, but they cannot replace HAL affordances for resource actions.

## Review Checklist

- Is the endpoint authentication boundary explicit?
- Does the request use a current ResourceKinds and AuthorizationActions value?
- Is tenant context included and reconciled with the addressed resource?
- Does the Application layer authorize before mutation or external side effects?
- Does provider failure deny rather than allow?
- Does the response advertise only authorized HAL actions?
- Does the UI consume those links instead of inspecting roles?
- Are machine scopes explicit and narrower than the resource decision?
- Are retired product-domain resources absent?

## Related Documentation

- [SECURITY-MODEL.md](SECURITY-MODEL.md)
- [API.md](API.md)
- [BLAZOR.md](BLAZOR.md)
- [CERBOS_COOLIFY.md](CERBOS_COOLIFY.md)
- [WEBHOOKS.md](WEBHOOKS.md)
