<!-- ABOUTME: Records non-obvious mechanics verified in the current first-party ISLAMU source. -->
<!-- ABOUTME: Covers tenant isolation, persistence, Blog, HAL, BFF, delivery, and federation boundaries. -->

# Codebase Insights

> **Audience:** Maintainers | Contributors | AI agents
> **Status:** Active
> **Owner:** Platform/Architecture
> **Last Verified:** 2026-07-19
> **Source Anchors:** `src/Explore.API/Program.cs`, `src/Explore.Persistence/ExploreDbContext.cs`, `src/Explore.Persistence/ExploreDbContext.DbSets.cs`, `src/Explore.Persistence/ExploreDbContext.QueryFilters.cs`, `src/Explore.Persistence/ExploreDbContext.SaveChanges.cs`, `src/Explore.API/Hateoas/ResourceAssemblerBase.cs`, `src/Explore.Blazor/Program.cs`, `src/Explore.Blazor.Client/Routes.razor`, `src/Explore.Application/Specifications/BlogQuerySpecification.cs`, `src/Explore.Domain/References/ReferenceTypeRegistry.cs`, `src/Explore.Application/Notifications/NotificationRoutingOptions.cs`, `src/Explore.Infrastructure/Messaging/CompositeOutboxMessageDispatcher.cs`, `src/Explore.API/Controllers/WebhooksController.cs`, `schemas/openapi_islamu.json`

This document records current mechanics that are easy to misread from one file. Historical product
behavior belongs in clearly labeled changelogs and ADR context, not in this current-source guide.

## Tenant Resolution And Query Isolation

The API resolves tenant context before tenant-scoped data access. Normal multi-tenant resolution
uses a trusted BFF tenant slug, approved custom-domain lookup, or subdomain lookup. An unresolved
tenant fails closed; a request-body identifier does not create tenant authority.

`ExploreDbContext` receives tenant and current-user services after creation because the context is
pooled. Global named filters then apply tenant and soft-delete predicates independently.

Cross-tenant system work must call an explicit bypass API with a reason and retain a bounded
predicate. `IgnoreQueryFilters()` without a reviewed system boundary removes both tenant and
soft-delete protection and is not a normal repository technique.

## DbContext Decomposition And Save Pipeline

Persistence is split across focused partials:

- `ExploreDbContext.cs` owns construction and model composition;
- `ExploreDbContext.DbSets.cs` is the current entity inventory;
- `ExploreDbContext.QueryFilters.cs` registers tenant and soft-delete filters; and
- `ExploreDbContext.SaveChanges.cs` applies audit, soft-delete, and concurrency behavior.

Deleting an `ISoftDeletable` entity is converted to a state update during save. Audit timestamps
and actor identifiers are server-owned. Entities that do not implement the marker can be
physically deleted only through their explicit repository/lifecycle rules.

## Clean Architecture And CQRS

Dependencies point inward:

~~~text
Domain <- Application <- Persistence / Infrastructure <- API
~~~

- Domain owns entities and invariants.
- Application owns CQRS requests, handlers, specifications, DTO mapping, authorization metadata,
  and repository/service contracts.
- Persistence returns entities and applies EF mappings.
- Infrastructure owns external provider adapters and background drains.
- API controllers dispatch MediatR requests and assemble HTTP/HAL responses.

Validators are manually instantiated in handlers. Repository or service dependencies belong in
the handler, which passes only the context a validator explicitly requires. Do not register all
validators for automatic DI pipeline execution.

## HAL Capability Planning

HAL `_links` is the UI source of truth for resource actions. The assembler pipeline:

1. gathers candidate link definitions;
2. normalizes and deduplicates authorization checks;
3. batch-evaluates checks; and
4. materializes only authorized links.

Collection assembly flattens item checks into a batch to avoid per-row authorization calls. A
Blazor component must check the relevant relation before rendering Edit, Delete, Publish, or other
mutation controls; local roles and claims are not substitutes.

## Blazor BFF And Routing

`Explore.Blazor` is the server render host and BFF. OIDC access tokens remain server-side. Browser
requests use same-origin cookies and the BFF proxy; trusted tenant/auth headers are stripped or
replaced at the server boundary.

`Explore.Blazor.Client` owns Razor UI and the typed route table in `Routes.razor`. Public Blog
reader routes, authenticated Blog management routes, account/admin routes, and setup routes are
registered there. A component file without an `@page` directive may still be routable through
that table, so route audits must inspect both surfaces.

Direct browser upload to an approved presigned object-storage URL is a deliberate BFF exception.
The API authorizes and creates upload state first; provider credentials never enter the browser.

## Blog Query And Publication Model

Blog reads use immutable `BlogQuerySpecification` composition. Tenant, publication time,
category, tag, title search, and ordering rules are assembled in Application, then applied by
`BlogRepository` to both count and page queries.

Public handlers add tenant and `PublishedAt(DateTime.UtcNow)` constraints. Management handlers use
authorized management specifications. The domain aggregate owns publish, unpublish, deletion,
slug, thumbnail, category, and tag state; callers do not mutate publication timestamps directly.

The reader pages render Markdown with raw HTML disabled and sanitized HTTP(S)-only links. Blog
editor actions remain HAL-gated.

## Normalized Lookups

Stable lookup data uses integer IDs, immutable master codes, display names, and optional
descriptions. Persisted rows store FK IDs; DTOs expose `*Id`, `*Code`, and `*Name` where clients
need stable identity plus display metadata.

Current normalized families include role/permission scope, setting scope/value type, secret
source/validation status, external API-key ownership/status/credit period, notification state,
actor-subscription state, and webhook state.

Use `int` for lookups, UUIDv7 `Guid` for aggregate/state identities, and `long` for cursors. Do not
recreate a lookup as a public enum merely because a bounded internal switch uses an enum wrapper.

## Polymorphic References And Shared Extensions

`ReferenceTypeRegistry` governs polymorphic target kinds that cannot use one direct database
foreign key. It records identifier shape, tenant rule, cleanup behavior, and validation wording.

Current shared custom-property targets are Organization and Group. Current notification reference
targets are Organization, Group, and User. Application validation must resolve the registry before
persistence; a string discriminator from a request is never trusted by itself.

See [Custom properties](CUSTOM_PROPERTIES.md) for the exact EAV and lifecycle boundary.

## Notification Ownership

In-app `Notification` rows, notification-routing intents, and transport queues are different
models. `NotificationRoutingOptions` assigns identity-lifecycle email to the account authority,
provider-internal requests to an external workflow provider, and keeps unsupported product,
operations, and marketing owners disabled.

The account-authority lifecycle service records safe local intent/delegation metadata while the
identity provider owns credential-token email. Actor subscriptions and browser-push enrollment do
not imply an implemented notification producer.

See [Notifications](NOTIFICATIONS.md) and [Email notifications](EMAIL_NOTIFICATIONS.md).

## Reliable Work And External Delivery

The generic `OutboxMessage` dispatcher currently routes only managed tenant provisioning. Its
processor uses atomic leases, at-least-once dispatch, bounded retry, and dead-letter reconciliation.

Email, Web Push, PDS sync, policy propagation, and native integration work retain specialized
tables because their state machines need dedicated fields. Webhooks use durable message,
subscription, snapshot, attempt, publication, incoming receipt, and redrive records rather than
the generic dispatcher.

The durable PostgreSQL row is authoritative. TickerQ, hosted workers, optional RabbitMQ pointers,
and remote provider calls are execution mechanisms around that state, not replacements for it.

See [Outbox pattern](OUTBOX_PATTERN.md) and [Webhooks](WEBHOOKS.md).

## Federation Boundary

Actor, key-store, indexed-DID, ATProto-record, and PDS-sync foundations exist. The PDS worker can
call configured `putRecord` and `deleteRecord` endpoints. The repository is not itself a public
PDS, AppView, ATProto authentication implementation, or owned lexicon registry.

Collection identifiers on persisted records are opaque provider-facing values until a checked-in
schema and compatibility contract say otherwise. See [Federation](FEDERATION.md) and
[collection/lexicon boundary](LEXICONS.md).

## Generated API Contract

The API Release build owns `schemas/openapi_islamu.json`. The Blazor NSwag target owns
`src/Explore.Blazor.Client/Clients/IslamuApiClient.g.cs`. The generated inventory is
`docs/API_CONTRACT_INVENTORY.md`.

The current checked-in schema and inventory contain 309 paths and 420 operations. Generated files
must be regenerated through their owning build/test commands, never hand-edited.

## Middleware Ordering

The API pipeline order in `Program.cs` is security-sensitive. In broad phases it applies:

1. exception handling, forwarded headers, security/correlation/logging, and compression;
2. HTTPS, HAL preference handling, and routing;
3. tenant pre-resolution and request timeouts;
4. authentication conflict checks, authentication, and tenant post-authentication binding;
5. localization, idempotency, rate limiting, and authorization; and
6. output caching and ETags.

Forwarded headers must precede host-based tenancy. API-key tenant binding spans pre- and post-auth
stages. Output caching remains after authorization, and ETags require the final response body.

## Registration Locations

| Concern | Current composition root |
|---|---|
| Application services and MediatR | `src/Explore.Application/ApplicationServicesRegistration.cs` |
| Repositories and pooled DbContext | `src/Explore.Persistence/PersistenceServicesRegistration.cs` |
| External providers and drains | `src/Explore.Infrastructure/InfrastructureServicesRegistration.cs` |
| API middleware and hosted workers | `src/Explore.API/Program.cs` |
| Blazor BFF/auth/proxy | `src/Explore.Blazor/Program.cs` |
| Client services and route table | `src/Explore.Blazor.Client/Extensions/ServiceCollectionExtensions.cs`, `src/Explore.Blazor.Client/Routes.razor` |

Add a dependency at the layer that owns its interface/implementation. Do not make Domain depend on
EF, HTTP, provider SDKs, or UI projects.

## Related

- [Architecture](ARCHITECTURE.md)
- [Domain model](DOMAIN.md)
- [API conventions](API.md)
- [Multi-tenancy](MULTI_TENANCY.md)
- [Testing](TESTING.md)
