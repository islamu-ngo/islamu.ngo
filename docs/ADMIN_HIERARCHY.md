<!-- ABOUTME: Defines current administrative authority and data-scope boundaries for the first-party ISLAMU website. -->
<!-- ABOUTME: Covers interactive roles, hierarchical settings, support access, and external API-key ceilings. -->

# Administrative Hierarchy

> **Audience:** ISLAMU administrators | First-party operators | Contributors
> **Status:** Active
> **Owner:** Security/Platform
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.Infrastructure/Identity/AdminContext.cs`, `src/Explore.Infrastructure/Services/HierarchicalSettingsResolver.cs`, `src/Explore.API/Controllers/ControlPlaneController.cs`, `src/Explore.Application/Features/ExternalApiKeys/ExternalApiKeyScopeCeiling.cs`, `src/Explore.Infrastructure/SupportAccess/SupportAccessSessionService.cs`

## Scope

This hierarchy is an internal authorization and governance contract for ISLAMU. It separates
instance, tenant, organization, group, and user scopes without promising a public SaaS,
self-hosting, or community-operator product.

Authentication identifies a principal. `AdminContext`, Application authorization metadata, the
configured local/Cerbos authorization provider, tenant binding, and API/HAL policies decide what
that principal may do. Client-visible roles and route guards are not sufficient authority.

## Interactive Authority

| Scope | Authority source | Current boundary |
|---|---|---|
| Instance administrator | Platform admin role, with the documented bootstrap-owner fallback | Instance settings, multi-tenant control plane, tenant lifecycle/plans, support access, and instance-level credentials. Cross-tenant work must use an explicit bounded system/admin path. |
| Tenant administrator | Active tenant membership and tenant-scoped role grant | Tenant configuration, membership/role grants, public experience, delegated integrations, and tenant-owned credentials. No authority in another tenant. |
| Organization administrator | Organization membership/role within the resolved tenant | Organization profile, members, and organization credentials for that organization. |
| Group administrator | Group membership/role within the resolved tenant | Group profile, members, and group credentials for that group. |
| Standard user | Authenticated identity plus tenant-local participation | Own profile/preferences and only the resource actions exposed by server authorization. |

Instance authority does not disable tenant query filters globally. Any intentional cross-tenant
repository path must opt into a named bypass reason and still apply a bounded predicate. Missing
tenant context fails closed for tenant-scoped data.

## Tenant And Data Boundaries

- Multi-tenant requests resolve a tenant from the trusted BFF header, configured host/domain, or
  subdomain; unresolved requests return `404`.
- A tenant-bound API key and a conflicting tenant hint return `404`; the runtime does not fall
  back to either tenant.
- `ExploreDbContext` applies named `Tenant` and `SoftDelete` filters. Ignoring soft delete
  must not also remove tenant isolation.
- `User` is global, while `TenantUser`, `TenantUserProfile`, `TenantUserRoleGrant`, and user
  `Actor` records carry tenant-local state and authority.
- UI mutation controls must follow API HAL links. Link presence is an affordance hint; the API
  reauthorizes every request.

## Hierarchical Settings

`HierarchicalSettingsResolver` loads and resolves five tiers:

1. User
2. Group
3. Organization
4. Tenant
5. Instance

The most specific available value wins unless a higher authority has locked the setting.
Instance-level locks block lower-scope overrides in multi-tenant mode. Single-tenant mode bypasses
instance locks for the default tenant. Each scope has a five-minute cache and mutations invalidate
the affected scope.

This is a setting-resolution contract, not a general permission inheritance rule. A value resolving
at a scope does not grant write access to that scope.

## Multi-Tenant Control Plane

`ControlPlaneController` is instance-admin and multi-tenant-only. It exposes bounded operations
for tenant lifecycle, effective configuration, setting locks, plan assignment/apply/rollback, and
aggregate operational posture.

`[RequireMultiTenant]` returns `403` in single-tenant mode. Tenant lifecycle actions use HAL
links so invalid transitions are not offered as client actions, and the command handler validates
them again. Purge scheduling records lifecycle intent and audit evidence; it does not physically
delete tenant data inline.

## Support Access

Support access is tenant-context access, not user impersonation:

- the persisted session records the real actor, target tenant, mode, expiry, reason, and ticket
  reference;
- the BFF removes browser-supplied support headers and forwards only a server-owned session id;
- actor mismatch, tenant mismatch, disabled access, invalid mode, and stopped, revoked, or expired
  sessions fail closed;
- support access does not change the current user id or create `TenantUserRoleGrant` rows; and
- audit evidence keeps the real operator identity.

## Machine Principals

External API keys have explicit owner types: `User`, `Organization`, `Group`, `Tenant`, and
`InstanceAdmin`. Effective authority is the intersection of the requested scopes and the owner
type ceiling defined by `ExternalApiKeyScopeCeiling`; granting a broader scope cannot promote the
owner.

User, organization, group, and tenant keys are tenant-bound. `InstanceAdmin` credentials may have
a nullable stored tenant, but tenant-scoped API and MCP execution still requires an explicit,
non-conflicting execution tenant (or the default tenant in single-tenant mode). Tenantless
execution is limited to explicit host-administration routes.

The raw key is revealed once. Persisted keys are hashed, can be revoked, and are rate-limited.
Local and Cerbos authorization paths both consume the parsed machine-principal context; neither may
skip tenant binding or scope ceilings.

## Fail-Closed Rules

1. No tenant context means no tenant-scoped query.
2. A scope mismatch, owner-ceiling violation, revoked key, or conflicting tenant fails without
   widening authority.
3. Cerbos mode does not fall back to allow when the provider is unavailable.
4. Support access preserves the real actor and cannot manufacture tenant membership.
5. HAL absence hides the client action; server authorization remains mandatory.

## Related

- [ADMIN_GUIDE.md](ADMIN_GUIDE.md)
- [MULTI_TENANCY.md](MULTI_TENANCY.md)
- [AUTHORIZATION.md](AUTHORIZATION.md)
- [SECURITY.md](SECURITY.md)
- [API_COOKBOOK.md](API_COOKBOOK.md)
