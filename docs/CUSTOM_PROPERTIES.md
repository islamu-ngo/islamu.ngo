<!-- ABOUTME: Documents the current Organization and Group shared custom-property model. -->
<!-- ABOUTME: Defines its EAV, governance, lifecycle, API, tenant, and non-contract boundaries. -->

# Custom Properties

> **Audience:** Contributors | Maintainers | AI agents
> **Status:** Active
> **Owner:** Architecture
> **Last Verified:** 2026-07-19
> **Source Anchors:** `src/Explore.Domain/CustomPropertyDefinition.cs`, `src/Explore.Domain/CustomPropertyOption.cs`, `src/Explore.Domain/CustomPropertyValue.cs`, `src/Explore.Domain/References/ReferenceTypeRegistry.cs`, `src/Explore.Application/Features/CustomPropertyDefinitions/`, `src/Explore.Persistence/Repositories/CustomPropertyDefinitionRepository.cs`, `src/Explore.Persistence/Configurations/Entities/CustomPropertyDefinitionConfiguration.cs`, `src/Explore.API/Controllers/CustomPropertyDefinitionController.cs`, `src/Explore.API/Controllers/CustomPropertyGovernanceController.cs`

## Current Boundary

The implemented model is tenant-scoped shared EAV metadata for `Organization` and `Group` only.
`ReferenceTypeRegistry` is the source of truth for those target kinds, their Guid identifier shape,
and same-tenant ownership validation.

| Capability | Current state |
|---|---|
| Definition and option persistence | Implemented |
| Organization/Group target registration | Implemented |
| Authenticated definition CRUD with HAL responses | Implemented |
| Quota and machine-identity governance | Implemented |
| Normal retirement and audited dependency-free purge | Implemented |
| Governance report | Implemented |
| Dedicated value-writing API or public rendering contract | Not implemented |
| Templates, generated resource types, projections, or runtime schema engine | Not implemented |

The presence of value rows in the EF model is not evidence of a complete value-authoring product.
No current controller owns a separate custom-property value CRUD surface.

## Persistence Model

### Definition

`CustomPropertyDefinition` owns:

- tenant and `EntityTypeName` scope;
- normalized `Namespace + Key` machine identity;
- display metadata, type, requiredness, multiplicity, activation, and ordering;
- exposure, search, filter, export, moderation, analytics, and system-owned flags;
- typed defaults and simple validation metadata;
- options, audit fields, soft deletion, and optimistic concurrency.

EF restricts `EntityTypeName` to the shared targets registered by `ReferenceTypeRegistry` and
enforces unique `(TenantId, EntityTypeName, Namespace, Key)` identity.

### Options

`CustomPropertyOption` belongs to one definition. It has its own namespaced key, stable value,
display metadata, activation/default state, sort order, optional parent option, audit state,
soft deletion, and concurrency stamp.

Definition create/update commands persist the option set transactionally. Updates retain matching
machine keys, create new options, and deactivate options omitted from the replacement payload.

### Values

`CustomPropertyValue` stores one typed value per row for the target `EntityId`. The definition's
target kind discriminates whether that Guid identifies an Organization or Group. The value row has
typed text, number, boolean, date/time, or option columns and an `Ordinal` for deterministic
multi-value ordering.

Because the target is polymorphic, the database cannot express a direct Organization-or-Group
foreign key. Application validation and `ReferenceTypeRegistry` must prove the target kind and
same-tenant ownership before any value-writing workflow persists rows.

## Governance And Validation

`CustomPropertyGovernancePolicy` normalizes namespace/key identity and rejects unsupported or
unprivileged reserved namespace roots. Create and update handlers also enforce:

- a unique machine key within tenant and target scope;
- the configured maximum definitions per tenant/entity scope;
- the configured maximum options per definition; and
- DTO validation for type-specific defaults and constraints.

Supported property types are `Text`, `Number`, `Option`, `Boolean`, `DateTime`, and `Url`.
Validation metadata is intentionally bounded to input shape such as length, range, pattern,
allowed URL schemes, requiredness, option membership, and multiplicity.

Custom properties must not become authorization, policy, lifecycle, ranking, workflow, or tenant
isolation logic. A value that drives those concerns belongs on a typed aggregate with explicit
application rules.

Exposure and purpose flags are metadata ceilings, not automatic publication. A consumer must own
its authorization, tenant checks, DTO projection, and tests before it exposes a property outside
the authenticated administration boundary.

## Lifecycle

Normal `DELETE` retires rather than physically removes a definition:

- the definition is deactivated and soft-deleted;
- its default option is cleared;
- options are deactivated and soft-deleted; and
- existing values are soft-deleted.

This keeps the machine key and historical dependency state available for audit.

Permanent purge is a distinct admin operation. It requires a non-blank reason and succeeds only
when the repository finds no options, values, or audit references. The handler rechecks inside a
transaction and records the purge decision in the audit log. Purge is irreversible and must not be
used as ordinary cleanup.

## API Surface

`CustomPropertyDefinitionController` exposes authenticated definition operations at
`/api/custompropertydefinition`:

- paginated list filtered by Organization or Group scope;
- detail with options;
- create;
- replace/update;
- normal retirement; and
- admin-only dependency-free purge.

Definition reads use the platform's HAL conventions, and failures use ProblemDetails. Clients must
use emitted HAL relations for available actions instead of inferring permission from local roles.

`CustomPropertyGovernanceController` exposes the authenticated governance report at
`/api/admin/custom-property-definitions/governance-report`.

## Deliberate Non-Contracts

- No user-defined entity types or arbitrary relationships.
- No formula, computed-field, migration, or workflow engine.
- No template or projection synchronization lifecycle.
- No public search, export, analytics, or moderation payload merely because a flag exists.
- No cross-tenant target reference.
- No compatibility promise for product-specific schemas removed from current source.

## Related

- [Domain model](DOMAIN.md)
- [Extensibility](EXTENSIBILITY.md)
- [Multi-tenancy](MULTI_TENANCY.md)
- [ADR-006: Custom Properties Runtime Boundary](adr/ADR-006-custom-properties-runtime-boundary.md)
