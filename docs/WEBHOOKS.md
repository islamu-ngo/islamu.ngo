<!-- ABOUTME: Operator and integrator guide for first-party ISLAMU platform webhooks. -->
<!-- ABOUTME: Covers the canonical ledger, provider modes, signatures, tenant safety, and current routes. -->

# Webhooks

> **Audience:** Operators | Integrators | Contributors | AI agents
> **Status:** Active
> **Owner:** Platform/Ops
> **Last Verified:** 2026-07-19
> **Source Anchors:** `src/Explore.Application/Webhooks/`, `src/Explore.Infrastructure/Webhooks/`, `src/Explore.Persistence/Services/WebhookDeliveryPlanMaterializer.cs`, `src/Explore.API/Controllers/WebhooksController.cs`, `src/Explore.API/Controllers/WebhookEndpointOperationsController.cs`, `src/Explore.API/Controllers/IncomingWebhooksController.cs`, `src/Explore.Blazor.Client/Components/Webhooks/WebhookManagementPanel.razor`, `src/Explore.AppHost/AppHost.cs`

The webhook subsystem sends first-party ISLAMU website/platform notifications to configured consumers. Incoming provider callbacks are a separate ingestion boundary.

## Outgoing Delivery Model

~~~text
application publication
  -> canonical WebhookMessage ledger row
  -> immutable delivery-plan and target snapshots
  -> provider publication or local delivery work
  -> auditable attempt/outcome rows
~~~

The application owns the event catalog, tenant-scoped message identity, payload provenance, and durable evidence even when a Svix provider performs endpoint fanout.

DefaultWebhookEventPublisher materializes the canonical message; it does not perform the network call inline. Background processors drain durable local-delivery and provider-publication work.

## Current Event Catalog

WebhookEventTypeRegistry currently defines exactly:

| Event name | Purpose |
|---|---|
| organization.verified | Catalog descriptor for organization verification payloads. |
| webhook.test | LocalProvider test delivery scheduled for one endpoint. |

The test-endpoint command publishes webhook.test. organization.verified is a registered public descriptor, but this document does not claim a live organization-verification publisher where no caller is present.

Retired product lifecycle, registration, report, and moderation names are not part of the current catalog.

## Provider Modes

| Mode | Current meaning |
|---|---|
| Disabled | No outgoing delivery work for the consumer. |
| Local | Application-managed endpoint subscriptions, signed HTTP delivery, retries, and audit. |
| Svix | Provider publication through a configured Svix application/binding. |
| Composite | Governed combination of canonical application evidence and supported Svix/local behavior. |
| DryRun | Plans and records work without external delivery. |

Capabilities are resolved from the provider mode, provider binding, conformance profile, and governed configuration. Callers must not assume every mode supports local endpoint management, manual retry, App Portal, or reconciliation. The API and UI use HAL links and capability state as the authority.

The repository can provision a self-hosted Svix service for technical local/integration profiles. That describes the provider deployment, not a separately self-hostable ISLAMU product. Managed Svix compatibility must be proven against the configured provider; it is not inferred from the self-hosted profile.

## Management API

The authenticated /api/webhooks surface currently includes:

- the anonymous event-types catalog;
- tenant-scoped consumer list/read/create and provider-mode update;
- provider-binding repair;
- endpoint list/read/create/update/archive, secret rotation, test, and resume;
- Svix App Portal access when eligible;
- message and delivery-attempt audit reads;
- manual local-delivery retry;
- incoming-message redrive.

Exact routes are declared by WebhooksController and WebhookEndpointOperationsController. The UI must follow returned HAL links rather than infer actions from roles or provider mode.

Messages deliberately omit raw payload JSON from management responses. Secret references and resolved secrets must never be returned.

## Local Delivery Security

Local delivery:

- signs the exact serialized payload bytes;
- resolves a tenant-scoped endpoint secret by reference;
- applies destination safety policy before network access;
- records attempt outcome and retry schedule;
- suspends or archives endpoints through explicit state transitions;
- retains tenant-scoped audit evidence.

Endpoint validation must prevent server-side request forgery. Production configuration should require HTTPS and reject loopback, link-local, private, metadata-service, and otherwise disallowed destinations unless an explicitly governed environment policy permits a narrow exception.

Consumers must verify the signature against the raw request body before parsing and should reject replays outside their accepted timestamp/idempotency window.

## Svix Provider Boundary

SvixApplicationMapper maps a platform consumer/tenant to provider application identity. Provider credentials remain server-side. Binding verification state, capability evidence, publication attempts, and reconciliation outcomes are persisted rather than inferred.

Opening App Portal is an authorized server command. The browser receives only the short-lived portal result permitted by the API; it never receives the Svix management token.

Operational callbacks from Svix do not depend on the outgoing provider being enabled. They enter through:

- POST /api/integrations/svix/operational

The route is anonymous at the HTTP-auth layer and instead uses provider signature verification, request-size limits, rate limiting, tenant/provider identity resolution, durable intake, deduplication, settlement evidence, and processing/redrive state.

## Tenant Isolation And Idempotency

Every consumer, binding, endpoint, message, plan, attempt, and incoming record is tenant-scoped. A provider identifier or request body alone must not select another tenant.

Outgoing publication uses stable message identity and persisted work so retries do not create a new logical event. Incoming processing retains provider message identity, payload evidence, processing generations, effect receipts, and settlement/redrive records so repeated delivery can be handled idempotently.

## Configuration And Operations

Required configuration depends on mode:

- Local: endpoint safety policy, secret resolver, timeout, retry, and worker settings.
- Svix or Composite: provider base URL, authentication material, conformance profile, binding/application configuration, and operational webhook secret.
- DryRun or Disabled: explicit governed selection; no provider credential should be required merely to represent the mode.

Health checks report current provider/runtime reachability; they are not proof of end-to-end delivery. Operators should use a tenant-scoped webhook.test action, inspect the canonical message and attempt/publication evidence, and verify the receiving system independently.

No Coop or Osprey callback route or verifier is registered in the current API. The only public provider callback is the Svix operational intake documented in [INTEGRATIONS.md](INTEGRATIONS.md).

## Change Checklist

- Add or change event types in WebhookEventTypeRegistry and schema/catalog synchronization.
- Preserve exact-byte signing and payload provenance.
- Materialize durable state before network delivery.
- Keep tenant identity in every lookup and provider mapping.
- Classify failures before retrying; do not retry permanent safety/auth/configuration failures indefinitely.
- Expose management actions only through authorized HAL links.
- Never log payloads, signing secrets, provider tokens, or portal credentials.
