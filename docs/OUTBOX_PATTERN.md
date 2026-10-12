<!-- ABOUTME: Documents the current generic and specialized durable-work boundaries. -->
<!-- ABOUTME: Covers managed provisioning dispatch, retries, email, webhooks, federation, and integrations. -->

# Outbox Pattern

> **Audience:** Contributors | Operators | AI agents
> **Status:** Active
> **Owner:** Platform/Architecture
> **Last Verified:** 2026-07-19
> **Source Anchors:** `src/Explore.Domain/OutboxMessage.cs`, `src/Explore.Persistence/Repositories/OutboxRepository.cs`, `src/Explore.Infrastructure/Messaging/CompositeOutboxMessageDispatcher.cs`, `src/Explore.API/BackgroundServices/OutboxProcessor.cs`, `src/Explore.Application/Features/Management/Handlers/ManagedTenantProvisioningHandlers.cs`, `src/Explore.Domain/EmailDispatchOutbox.cs`, `src/Explore.Infrastructure/EmailDispatchDrainService.cs`, `src/Explore.Domain/Federation/PdsSyncOutbox.cs`, `src/Explore.API/BackgroundServices/PdsSyncWorker.cs`, `src/Explore.Infrastructure/IntegrationSyncDrainService.cs`, `src/Explore.API/Controllers/WebhooksController.cs`

## Current Boundary

The repository uses durable PostgreSQL state for work that must survive process failure. There is
one generic `OutboxMessage` processor and several specialized state machines. They share the
at-least-once principle, but they are not interchangeable and do not share one dispatcher.

The current generic `OutboxMessage` producer and dispatcher route are limited to managed tenant
provisioning. No content publication, moderation, reporting, or subscriber-fanout message is
registered in `CompositeOutboxMessageDispatcher`.

## Generic Outbox

`OutboxMessage` stores:

- UUIDv7 identity;
- source aggregate type and Guid;
- a generic `EventType` discriminator;
- optional JSONB payload;
- `Pending`, `Processing`, `Completed`, `Failed`, or `DeadLettered` status;
- creation/completion timestamps; and
- retry, lease, bounded error, and dead-letter state.

`EventType` is a technical dispatch discriminator. It is not a product entity type.

### Processing Flow

~~~text
business transaction creates durable OutboxMessage
  -> OutboxProcessor polls due rows
  -> OutboxRepository atomically claims a processing lease
  -> CompositeOutboxMessageDispatcher invokes the registered consumer
  -> repository records completion or schedules bounded retry
  -> exhausted rows are dead-lettered and reconciled by the owning consumer
~~~

The repository can reclaim expired processing leases, preventing a crashed worker from leaving a
row permanently stuck. Completion and failure transitions include the lease timestamp in their
predicate so a stale worker cannot overwrite a replacement claim.

Delivery is at least once. A consumer may complete its side effect before the completion update is
persisted, so every dispatcher must be idempotent.

### Current Route

`ManagedTenantProvisioningOutboxEvents.ProcessRequested` is the only registered generic route.
The provisioning request persists an `OutboxMessage` for its
`ManagedTenantProvisioningOperation`; the dispatcher sends
`ProcessManagedTenantProvisioningOperationCommand` through MediatR.

Dead-letter reconciliation sends `ReconcileManagedTenantProvisioningDeadLetterCommand`. Unknown
discriminators fail closed so they retry and dead-letter instead of being silently acknowledged.

### Retry And Operations

`OutboxProcessorSettings` controls enablement, polling interval, batch size, retry-delay base/cap,
and verbose logging. The worker uses exponential backoff. The row's `MaxRetries` is authoritative
for exhaustion; managed provisioning currently writes ten attempts.

`OutboxRepository.GetFailedEntries` exposes `Failed` and `DeadLettered` rows for inspection, while
completed rows can be deleted by retention cleanup. Dead-letter state must remain visible until its
owner has reconciled the affected business operation.

## Specialized Durable State

Specialized tables exist when a workflow needs fields or transitions that do not fit the generic
message shape:

| State machine | Current responsibility |
|---|---|
| `EmailDispatchOutbox` | Tenant-scoped email intent, attempts, receipts, parking/dead-letter state, and optional RabbitMQ pointer metadata |
| `WebPushDispatchOutbox` | Browser-push attempt and expiry state for registered subscriptions |
| `PdsSyncOutbox` | DID, collection, record key, payload, host, and retry state for outbound PDS synchronization |
| `IntegrationSyncOutbox` | Provider/resource payload snapshots and retry state for native integration drains |
| `PolicyChangeOutbox` | Persisted governance-policy propagation state |

The existence of a table, repository, or drain worker does not prove that every product workflow
currently enqueues it. Callers must identify a checked-in producer before documenting an active
end-to-end flow.

### Email

`EmailDispatchOutbox` is the durable source of truth for direct SMTP delivery. The shared drain
service claims due rows, restores tenant context, sends through the approved email abstraction,
and records bounded attempts and receipts. TickerQ is the default trigger; a hosted processor can
trigger the same drain, and optional RabbitMQ mode publishes pointer-only messages after the
PostgreSQL row exists.

Broker acknowledgement must follow the PostgreSQL state transition. RabbitMQ is transport, not
the durable email ledger. See [Email notifications](EMAIL_NOTIFICATIONS.md).

### Federation And Native Integrations

`PdsSyncWorker` drains `PdsSyncOutbox` through `PdsService`. `IntegrationSyncDrainService` drains
`IntegrationSyncOutbox` through the selected native integration service. These state machines keep
their own claim, retry, and terminal fields and do not pass through
`CompositeOutboxMessageDispatcher`.

### Webhooks

Webhook delivery uses its own catalog and durable message, plan-snapshot, target-snapshot,
attempt, publication, incoming-message, effect-receipt, and redrive tables. Those records preserve
delivery provenance and retry state; they are not generic `OutboxMessage` rows. See
[Webhooks](WEBHOOKS.md).

## Non-Negotiable Rules

- Persist durable intent before making the external side effect.
- Keep the intent in the same owning transaction as its business state when atomicity is required.
- Make consumers idempotent and treat delivery as at least once.
- Claim work atomically and prevent stale workers from overwriting newer claims.
- Bound retries, errors, logs, metric dimensions, and payload snapshots.
- Preserve tenant context without accepting tenant identity from an untrusted payload.
- Do not perform SMTP, broker publication, webhook delivery, or remote PDS calls inside a domain
  mutation handler unless that handler owns an explicit after-commit boundary.
- Do not route specialized state through the generic dispatcher merely because both are called
  outboxes.

## Related

- [Architecture](ARCHITECTURE.md)
- [Domain model](DOMAIN.md)
- [Email notifications](EMAIL_NOTIFICATIONS.md)
- [Federation](FEDERATION.md)
- [Webhooks](WEBHOOKS.md)
- [ADR-002: Outbox Pattern](adr/ADR-002-outbox-pattern.md)
- [Operations](OPERATIONS.md)
