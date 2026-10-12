<!-- ABOUTME: Documents the current ISLAMU domain model and persistence-enforced rules. -->
<!-- ABOUTME: Focuses on tenant isolation, identity, Blog, shared extensions, and reliable platform state. -->

# Domain Model

> **Audience:** Contributors | Maintainers | AI agents
> **Status:** Active
> **Owner:** Architecture
> **Last Verified:** 2026-07-19
> **Source Anchors:** `src/Explore.Persistence/ExploreDbContext.DbSets.cs`, `src/Explore.Domain/Blog.cs`, `src/Explore.Domain/References/ReferenceTypeRegistry.cs`, `src/Explore.Persistence/Configurations/Entities/BlogConfiguration.cs`, `src/Explore.Persistence/ExploreDbContext.QueryFilters.cs`, `src/Explore.Persistence/ExploreDbContext.SaveChanges.cs`

Most entities live directly under `src/Explore.Domain/`. This document describes current source
only; removed product aggregates and historical migration shapes are intentionally excluded.

## Current Domain Areas

| Area | Current aggregate and state roots |
|---|---|
| Tenancy and plans | `Tenant`, `TenantUser`, `TenantUserRoleGrant`, `TenantSetting`, `TenantSettingsDocument`, `TenantNavigationLink`, `TenantInvitation`, `TenantLifecycleLog`, tenant plans/versions/assignments |
| Identity and actors | `User`, `Actor`, `ActorSubscription`, `Organization`, `Group`, `Role`, `Permission`, `RolePermission`, `PlatformUserRole` |
| Published content | `Blog` with `BlogCategory` and `BlogTag` joins to existing tenant taxonomy |
| Shared extensions | `CustomPropertyDefinition`, `CustomPropertyOption`, and `CustomPropertyValue` for Organization and Group targets |
| Notifications and delivery | `Notification`, preference/profile rows, `NotificationIntent`, `NotificationDelivery`, Web Push subscription/outbox state |
| Storage | `StorageObject`, `StorageUploadSession`, and `StorageUsageCounter` |
| Webhooks | consumers, provider bindings, endpoints/subscriptions, messages, delivery plans/attempts, provider publications, incoming messages/effect receipts/redrive state |
| Federation | `AtprotoRecord`, `IndexedDid`, `SyncState`, and `ActorKeyStore` |
| Settings and governance | `SystemSetting`, `AppSetting`, `ConfigurationChangeLog`, policy sets, module definitions, and tenant capabilities |
| AI assistant | conversations, messages, runs, references, proposed actions, tool executions, and consent grants |
| Operations | support-access sessions/audit, managed provisioning operations, audit logs, and specialized outboxes |

## Normalized Lookup Model

Persisted state commonly stores a stable integer foreign key plus a lookup navigation. Lookup rows
provide a stable `MasterCode`, human-readable `FullName`, and optional description. Public DTOs
expose lookup primitives such as `*Id`, `*Code`, and `*Name`; repositories query persisted IDs.

Current examples include:

- role and permission scope through `RoleScopeId`;
- setting scope and value type through `SettingScopeId` and `SettingValueTypeId`;
- secret source and validation status;
- external API-key owner, status, and credit-period lookups;
- notification scope, category, channel, intent, delivery, and ownership lookups;
- actor-subscription status and notification level; and
- webhook provider, endpoint, delivery, publication, and incoming-processing lookups.

Handlers may map lookup IDs to internal enums for bounded business-rule switches, but persistence
and public contracts remain normalized.

## Non-Inferable Modeling Patterns

### PII Split Tables

Sensitive fields are held in dedicated one-to-one extension tables while the owning aggregate keeps
non-sensitive identity and lifecycle state:

- `User` -> `UserPii` for email and personal names;
- `Organization` -> `OrganizationPii` for name, email, and address;
- `Actor` -> `ActorPii` for display/federation profile fields; and
- `Location` -> `LocationPii` for precise location fields.

Mapped convenience properties are not separate persistence columns. PII extension rows support
targeted deletion without removing the non-sensitive owner needed for audit and referential state.

### Blog Publication Aggregate

`Blog` is tenant-scoped, audited, soft-deletable, and concurrency-aware. It owns raw Markdown,
a canonical tenant-local slug, an optional tenant-safe thumbnail reference, and nullable
`PublishedAtUtc` publication state.

- Creation and update normalize required titles and enforce lowercase Unicode slug syntax.
- Publish and unpublish are explicit lifecycle methods.
- Deletion clears publication before setting soft-delete metadata.
- `ReplaceCategories` and `ReplaceTags` replace deduplicated tenant-scoped join rows.
- EF enforces unique `(TenantId, Slug)`, composite tenant foreign keys, and unique Blog/category
  and Blog/tag join tuples.

Public queries apply tenant, non-deleted, and publication-time filters. Editorial writes remain
authorized commands rather than direct status-field mutation.

### Shared Custom Properties

The current shared extension model is EAV for Organization and Group targets:

- `CustomPropertyDefinition` owns namespace/key identity, property type, validation metadata,
  exposure/governance flags, options, audit state, soft deletion, and concurrency.
- `CustomPropertyValue` stores one typed value per row with `Ordinal` for deterministic
  multi-value ordering.
- `ReferenceTypeRegistry` permits shared definitions only for Organization and Group and requires
  the target entity to belong to the definition tenant.

Only the Organization/Group shared EAV targets above are currently registered. Standard fields
belong on typed aggregates rather than being reintroduced through speculative EAV shapes.

### Polymorphic Reference Registry

References that cannot use a direct foreign key are governed by
`Explore.Domain.References.ReferenceTypeRegistry`. It centralizes target kind, Guid shape,
ownership, tenant-scope rule, cleanup behavior, and validation wording.

Current registries cover:

- approved external binding type pairs;
- notification targets for Organization, Group, and User; and
- shared custom-property targets for Organization and Group.

Repositories and application handlers validate these discriminators before saving. EF check
constraints provide defense in depth where the registered shape can be represented in the model.

### Tenant Isolation, Audit, And Soft Delete

Marker interfaces define cross-cutting persistence behavior:

- `ITenantEntity` supplies `TenantId` and participates in fail-closed tenant query filters;
- `IAuditableEntity` supplies created/updated actor and timestamp fields;
- `ISoftDeletable` supplies deletion state and is converted from delete operations by
  `ExploreDbContext.SaveChanges`; and
- `IConcurrencyAware` supplies an optimistic concurrency stamp.

Cross-tenant work must use explicit, approved query-filter bypass APIs with named reasons. Public
or tenant-scoped reads never infer tenant authority from request-body identifiers.

### Tenant-Local User Authority

`TenantUser` represents a global user's state inside one tenant. `TenantUserRoleGrant`, rather
than a direct membership role field, carries tenant authorization.

EF enforces:

- `TenantUserRoleGrant(TenantId, TenantUserId)` -> `TenantUser(TenantId, Id)`;
- `TenantUserRoleGrant(RoleId, RoleScopeId)` -> `Role(Id, RoleScopeId)`;
- `RoleScopeId = Tenant`; and
- one active grant per `(TenantId, TenantUserId, RoleId)`.

Revocation is explicit and retains the actor, timestamp, and reason for audit while active checks
ignore revoked grants.

### Actor Subscriptions And Notification Preferences

`ActorSubscription` is a durable tenant-local subscription to Organization or Group actors.
Unsubscribe changes status instead of deleting the row; resubscribe reuses the durable identity.
EF constrains target actor type, status, notification level, and one non-deleted row per subscriber
and target actor.

Notification references are limited to Organization, Group, and User. Preference categories and
channels are normalized lookup rows; `NotificationChannelPreference` and
`NotificationPreferenceProfile` store tenant-scoped choices, concurrency, and soft-delete state.
Required categories remain governed server-side rather than by client checks.

The current domain does not assign a publication-triggered fanout source to actor subscriptions.
Any future fanout producer must define its own source identity, idempotency key, authorization, and
delivery policy.

## Messaging And Reliability

### Generic Outbox

`OutboxMessage` is the generic transactional outbox row for cross-process side effects. It stores
UUIDv7 identity, source aggregate type/id, event discriminator, optional JSON payload, processing
status, retry/dead-letter state, and timestamps. `OutboxProcessor` claims rows and dispatches them
at least once through `IOutboxMessageDispatcher`.

The current generic producer is managed tenant provisioning:
`ManagedTenantProvisioningOutboxEvents.ProcessRequested` is routed by
`CompositeOutboxMessageDispatcher` to the corresponding MediatR command. Dispatchers must be
idempotent because a claimed message can be retried.

Specialized durable queues remain separate where their lifecycle requires dedicated state:

- `PdsSyncOutbox` for federation synchronization;
- `PolicyChangeOutbox` for governance policy publication;
- `EmailDispatchOutbox` with attempt, receipt, and tenant-control rows;
- `WebPushDispatchOutbox` for browser push delivery; and
- `IntegrationSyncOutbox` for native integration synchronization.

## Persistence-Enforced Rules

Representative current invariants include:

- Blog titles are required and limited to 300 characters; slugs are required, limited to 200, and
  unique per tenant.
- Blog thumbnail, category, and tag relations use composite tenant foreign keys, preventing
  cross-tenant joins.
- `Actor` has exactly one nullable owner among User, Organization, or Group.
- `ActorSubscription` permits Organization/Group targets and one non-deleted subscriber/target row.
- `TenantUserRoleGrant` is tenant-scoped, tenant-role-only, and unique while active.
- Shared custom-property definitions and values are tenant-scoped and restricted to registered
  Organization/Group targets.
- Notification preference rows enforce one valid scope/target/category/channel tuple; global mute
  profiles enforce one valid scope/target tuple.
- Sensitive setting keys are rejected from generic `AppSetting` storage.
- Tenant entities use global query filters; audit and soft-delete state is applied during
  `SaveChanges`.

## Related

- [ARCHITECTURE.md](ARCHITECTURE.md)
- [CUSTOM_PROPERTIES.md](CUSTOM_PROPERTIES.md)
- [MULTI_TENANCY.md](MULTI_TENANCY.md)
- [OUTBOX_PATTERN.md](OUTBOX_PATTERN.md)
