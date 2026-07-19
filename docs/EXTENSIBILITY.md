<!-- ABOUTME: Documents current internal extension seams: modules, Organization/Group custom properties, TMS providers, and API keys. -->
<!-- ABOUTME: Separates source-backed compile-time extension points from deleted Event and speculative plugin/product promises. -->

# Extensibility

> **Audience:** Contributors | Platform maintainers
> **Status:** Active
> **Owner:** Architecture
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.Domain/Modules/`, `src/Explore.API/Controllers/ModuleController.cs`, `src/Explore.Infrastructure/Services/ModuleService.cs`, `src/Explore.Domain/CustomPropertyDefinition.cs`, `src/Explore.Infrastructure/Localization/RuntimeTranslationProvider.cs`, `src/Explore.API/Controllers/ExternalApiKeyController.cs`

## Boundary

These are internal, source-owned extension seams for the first-party ISLAMU website. They do not
define a third-party plugin marketplace, runtime assembly loading contract, SaaS integration tier,
or compatibility promise.

Current extensibility consists of:

1. compile-time module definitions with tenant capability records;
2. tenant-scoped typed custom properties for `Organization` and `Group`;
3. a runtime-selected translation-management provider behind one interface; and
4. scoped external API keys for approved machine callers.

Deleted Event, EventSession, sector-aspect, Event-template, Event-custom-property, projection,
strategy, route, and UI types are not extension points.

## Modules

`ModuleDefinition` is the global catalog and `TenantCapability` records tenant enablement. The
current seed creates only `Mod_Core`. Source still contains optional Islamic/Tech capability
flags, but no current seed definition or deleted Event sector model makes those flags a product
feature.

`ModuleController` exposes:

- `GET /api/module/available`
- `GET /api/module/enabled`
- `GET /api/module/{moduleKey}/enabled`
- `GET /api/module/{moduleKey}/schema`
- `POST /api/module/{moduleKey}/enable`
- `POST /api/module/{moduleKey}/disable`

Reads use the resolved tenant context where applicable. Writes require authentication and dispatch
MediatR commands. Disabling a tenant capability sets `IsEnabled=false`; it does not delete the
capability record. `ModuleService` caches the global and per-tenant views for five minutes and
invalidates the affected cache on mutation.

Modules are compiled application capabilities. `WizardSchemaUrl` is catalog metadata, not proof
that a schema-driven UI exists. Adding a module requires source, authorization, persistence,
operator UI, and tests; registering a key alone is insufficient.

## Organization And Group Custom Properties

The surviving Layer 3 model is:

- `CustomPropertyDefinition`: tenant-scoped namespace/key, target entity type, value type,
  validation, exposure/governance flags, audit fields, concurrency stamp, and soft delete;
- `CustomPropertyOption`: stable namespaced options, ordering, hierarchy, audit, and soft delete;
  and
- `CustomPropertyValue`: tenant-scoped typed values for an Organization or Group id, including
  deterministic multi-value ordering.

The definition's `EntityTypeName` discriminates Organization from Group. The polymorphic
`EntityId` has no database foreign key, so Application validation and the reference-type
registry remain required. Tenant query filters and server authorization are mandatory; a custom
property cannot broaden data scope.

Use a custom property for local, non-authoritative metadata. Add a typed first-class field instead
when the value drives security, moderation, policy, ranking, lifecycle, or a stable cross-feature
contract.

## Translation Provider Abstraction

`RuntimeTranslationProvider` routes `ITranslationManagementProvider` calls to:

- `TolgeeTranslationProvider`;
- `WeblateTranslationProvider`;
- `OfflineTranslationProvider` (default and error fallback); or
- `NullTranslationProvider` where explicitly injected by internal flows/tests.

`ForceOfflineMode` bypasses the configured remote provider. Connection, language, and export
failures fall back to offline bundles; import failures are recorded and do not invent a successful
remote write.

Adding another provider is a source change: implement the interface, add the enum/config resolver
case, register the provider/HTTP client, preserve secret handling and bounded telemetry, and add
focused provider/fallback checks. This is not runtime plugin loading.

## External API Keys

Machine-to-machine access is implemented, not planned. `ExternalApiKeyController` supports
visible-key listing/detail, one-time issuance, policy update, revocation, and bounded usage reports.
Keys are hashed at rest, rate-limited, tenant-bound except for the nullable
`InstanceAdmin` credential form, and constrained by owner-type scope ceilings.

API keys extend access only to existing authorized endpoints. They do not create new routes,
override tenant resolution, or bypass local/Cerbos authorization. See
[ADMIN_HIERARCHY.md](ADMIN_HIERARCHY.md).

## Deliberate Non-Contracts

- no runtime assembly/package loading;
- no remote MCP tool import or execution;
- no Event compatibility aliases or deleted Event routes;
- no pricing-tier or quota product promise; and
- no guarantee that dormant constants, schema URLs, or UI flags have an implemented consumer.

## Related

- [CUSTOM_PROPERTIES.md](CUSTOM_PROPERTIES.md)
- [MULTI_TENANCY.md](MULTI_TENANCY.md)
- [LOCALIZATION.md](LOCALIZATION.md)
- [ADMIN_HIERARCHY.md](ADMIN_HIERARCHY.md)
