<!-- ABOUTME: Documents the retained Coop provider infrastructure in the first-party ISLAMU platform. -->
<!-- ABOUTME: Separates provisioned containers and signature verification from absent moderation workflows. -->

# Coop Integration

> **Audience:** Operators | Integrators | Contributors | AI agents
> **Status:** Active
> **Owner:** Platform/Integrations
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.API/Services/CoopIncomingWebhookVerifier.cs`, `src/Explore.API/Services/CoopWebhookSignatureValidator.cs`, `src/Explore.API/Program.cs`, `src/Explore.AppHost/AppHost.cs`, `docker-compose.yml`, `.env.example`

## Current Truth

Coop is retained as optional provider infrastructure and incoming-signature verification foundation. It is not a live ISLAMU moderation product flow.

| Capability | Current source state |
|---|---|
| Local Coop server, migrations, client, and PostgreSQL resources | Provisioned by the FullLocal Aspire topology and the Compose moderation profile. |
| Provider endpoint, API key, and webhook-secret configuration | Present in reporting configuration and environment templates. |
| Raw-body HMAC webhook verification | Implemented by CoopIncomingWebhookVerifier and CoopWebhookSignatureValidator. |
| Public Coop callback route | Not present. IncomingWebhooksController exposes only the Svix operational route. |
| ISLAMU report/case synchronization | Not present in current application source. |
| Moderation escalation state machine or review UI | Not present in current application or Blazor source. |
| Osprey-to-Coop escalation | Not present. |

Do not infer a callable integration from dependency injection, containers, configuration keys, or retained verifier classes alone. An owned API route and an application workflow are still required.

## Provisioned Technical Topologies

The repository supports two provider arrangements at the infrastructure level:

- FullLocal/Compose moderation profile: repository orchestration starts Coop server, migrations, client, and a dedicated Coop PostgreSQL database.
- Externally operated provider: configuration may point the platform at a separately operated Coop endpoint and credentials.

These are technical provider deployment semantics. They do not make the first-party ISLAMU website a self-hosted product, and they do not prove production enablement.

The local images in docker-compose.yml and AppHost defaults are integration conveniences. Operators must pin, scan, back up, and validate provider images before production use; latest-tag defaults are not a production release policy.

## Signature Verification Foundation

CoopIncomingWebhookVerifier currently:

1. Requires a configured shared webhook secret.
2. Requires and parses the configured timestamp header.
3. Rejects timestamps outside the clamped tolerance window.
4. Computes HMAC-SHA256 over timestamp, a period separator, and the exact raw payload bytes.
5. Accepts supported hex/base64 signature encodings and compares them in fixed time.
6. Requires a tenant authority already bound by the trusted integration context.
7. Uses providerDecisionId, provider_decision_id, or the SHA-256 payload hash as provider message identity.
8. Returns the technical event name moderation.coop.decision to the shared incoming-webhook pipeline.

This verifier is not reachable through a Coop-specific controller route today. If a route is added later, it must use the shared bounded intake, provider registry, request-size/rate limits, tenant resolution, deduplication, effect receipts, and redrive model used by incoming webhooks.

## Tenant And Secret Boundaries

- A request-body tenant identifier must never establish tenant authority.
- A shared Coop cluster must bind each credential or workspace identity to one platform tenant.
- Provider decisions must use a stable provider message/decision identifier for idempotency.
- API keys, session secrets, database credentials, and webhook secrets belong in the configured secret provider.
- Logs may include correlation identifiers and verification outcomes, but not raw bodies, credentials, or signatures.
- Cross-tenant aggregation inside a provider must not be treated as application authorization.

## Activation Requirements

Before Coop can be documented as an active application integration, source must provide and verify:

- an owned provider callback or polling entrypoint;
- a current platform resource and CQRS workflow that consumes a decision;
- tenant-bound credential resolution;
- replay-safe durable intake and idempotent effects;
- resource authorization and HAL affordances for any management UI;
- integration and architecture tests for cross-tenant rejection and duplicate delivery;
- production image/version, backup, health, and rollback evidence.

Retired report, case, moderation-route, and escalation projections must not be restored to this document unless a new first-party capability is implemented and approved.
