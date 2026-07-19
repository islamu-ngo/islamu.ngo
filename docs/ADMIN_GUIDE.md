<!-- ABOUTME: Task-focused administration guide for the first-party ISLAMU website. -->
<!-- ABOUTME: Maps current admin surfaces to authority, tenant boundaries, and recovery-sensitive operations. -->

# Admin Guide

> **Audience:** ISLAMU administrators | First-party operators | Contributors
> **Status:** Active
> **Owner:** Platform Administration
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.Blazor.Client/Routes.razor`, `src/Explore.Blazor.Client/Pages/Admin/`, `src/Explore.Blazor/Components/ControlPlane/EmbeddedControlPlanePage.razor`, `src/Explore.API/Controllers/ControlPlaneController.cs`, `src/Explore.API/Controllers/InstanceSettingsController.cs`, `src/Explore.API/Controllers/TenantStorageSettingsController.cs`

## Scope

This is an internal operating guide for the first-party ISLAMU website. It documents source-backed
administrative surfaces; it is not a SaaS tier, third-party self-hosting, or community-operator
product promise.

Admin routes require authentication, but a route guard or visible component is not write
authorization. API authorization remains authoritative. Where an API response exposes HAL links,
the UI must render mutation controls from link presence and still allow the server to enforce the
operation.

## Authority And Entry Points

| Scope | Entry points | Current responsibility |
|---|---|---|
| Instance | `/admin/instance/settings`; multi-tenant console at `/admin/instance`, `/admin/instance/tenants`, `/admin/instance/domains`, `/admin/instance/operations`, and `/admin/instance/plans` | Instance defaults, provider configuration, delegation locks, tenant lifecycle, plans, support access, and instance API keys. |
| Tenant | `/admin/tenant/settings`, `/admin/tenant/footer`, `/admin/tenant/navigation`, `/admin/tenant/custom-properties`, and `/admin/tenant/custom-property-definitions` | Tenant policy and branding, public experience, lookup data, navigation/footer, delegated provider overrides, support evidence, API keys, and Organization/Group custom-property definitions. |
| Organization | `/admin/organization/{OrganizationId}/settings` | Organization profile, members, verification view, and organization API keys. |
| Group | `/admin/group/{GroupId}/settings` | Group profile, members, and group API keys. |

The tenant settings route redirects to instance settings in single-tenant mode. The multi-tenant
Instance Console is guarded in the client and its API actions use `[RequireMultiTenant]`, which
returns `403` when the runtime is single-tenant.

## Instance Administration

Instance settings currently compose governance, domain/branding, authentication providers,
modules, local or optional S3-compatible storage, SMTP, analytics/privacy, localization, AI, MCP,
support access, and API-key controls. Some sections vary by deployment mode.

`Bff:AdminHosts` classifies configured hosts for the embedded control-plane shell. Host
classification does not grant instance-admin authority. Optional
`Bff:AdminHostAllowedIpRanges` restrictions also fail closed before the shell is treated as an
admin host.

The multi-tenant console supports tenant listing and detail, effective configuration, setting
locks, plan assignment/apply/rollback, and lifecycle transitions. Lifecycle buttons are HAL
affordances. Scheduling purge requires an archived tenant, an operator reason, and exact slug
confirmation; it records the `Purged` lifecycle state without physically deleting tenant data in
the request path.

### High-Risk Instance Changes

| Change | Risk control |
|---|---|
| Schedule tenant purge | Confirm backup/export posture first. The request records audited lifecycle intent only; plan physical deletion and recovery separately. |
| Revoke an instance API key | Issue and distribute a replacement before revoking the old credential when continuity matters. Raw secrets are shown only at creation. |
| Change authentication settings | Preserve a known-good secret/configuration snapshot and an operator recovery path to avoid lockout. |
| Select Cerbos authorization | Cerbos mode is fail-closed when its authorization provider is unavailable; restore the known-good local/provider configuration through the approved operator path. |
| Change tenant delegation locks | Review affected tenant overrides before locking. A locked tenant surface becomes read-only; it does not silently broaden tenant authority. |

## Tenant Administration

Tenant settings expose policies, render policy, domain and branding, announcement/public
experience, themes, organizations, lookup tables, navigation, footer, API keys, community
guidelines, and support-access evidence. SMTP, storage, analytics, and MCP override sections render
only when instance delegation permits them.

Tenant provider overrides stay within instance policy:

- storage provider and upload limits must satisfy allowed-provider and ceiling validation;
- S3-compatible secrets are redacted on reads and preserved unless explicitly replaced;
- `governance.lock_tenant_storage` makes storage settings read-only;
- MCP enablement and legacy-SSE intent respect `governance.lock_tenant_mcp` and
  `governance.lock_tenant_mcp_legacy_sse`; legacy SSE is still unavailable at runtime; and
- tenant identity always comes from the resolved request context, never a client-supplied body id.

There is no current Event-template, Event-session-template, or Event-template-sync admin route.
Those deleted product surfaces are not an operator contract.

### High-Risk Tenant Changes

| Change | Risk control |
|---|---|
| Delete lookup or custom-property definitions | Confirm current consumers and export required metadata first. Server validation and dependency checks remain authoritative. |
| Change render/public-experience policy | Verify the affected public routes after saving; planned destinations are not created by a setting. |
| Revoke a tenant API key | Rotate first where possible, then revoke. Tenant-bound credentials cannot cross a conflicting tenant context. |
| Change delegated provider settings | Test within the admin surface, keep secrets in the configured secret provider, and retain the previous values for recovery. |

## Organization And Group Administration

Organization administration covers profile, membership, verification display, and organization
API keys. Group administration covers profile/branding, membership, and group API keys. The
authenticated page is only an entry point: server-side organization/group membership and
authorization checks decide each operation.

Custom-property definitions apply to `Organization` and `Group` only in the current source.
They are tenant-scoped, audited, soft-deletable, and typed; deleted Event-specific definition,
template, value, and projection families are not part of this guide.

## Support Access

Support access changes tenant context; it does not impersonate a tenant user or create tenant role
grants. The BFF strips browser-supplied support headers and forwards only its server-owned support
session identifier. The API validates actor, target tenant, mode, expiry, and session status before
authorization can use the context. Tenant administrators can inspect tenant-scoped support evidence
only when the API/HAL affordance permits it.

## Storage And Recovery

Storage is local-first with an optional S3-compatible provider. Browser uploads use BFF-issued
upload sessions and proxy streaming. Instance and delegated tenant storage controls use redacted
settings, provider tests, usage counters, quotas, and HAL/read-only state.

Back up database metadata and the selected backing store as one recovery set. Restore backing
objects before reopening traffic, then verify representative `StorageObject` records resolve.
See [STORAGE.md](STORAGE.md) and
[BACKUP_RESTORE_UPGRADE.md](BACKUP_RESTORE_UPGRADE.md).

## Related

- [ADMIN_HIERARCHY.md](ADMIN_HIERARCHY.md)
- [AUTHORIZATION.md](AUTHORIZATION.md)
- [AUTHORIZATION_PATTERNS.md](AUTHORIZATION_PATTERNS.md)
- [MULTI_TENANCY.md](MULTI_TENANCY.md)
- [MCP_DEBUGGING.md](MCP_DEBUGGING.md)
- [SECRETS.md](SECRETS.md)
