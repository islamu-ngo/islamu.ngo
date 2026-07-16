<!-- ABOUTME: Records current incoming provider callbacks and their verification boundaries during the website migration. -->
<!-- ABOUTME: Keeps imported moderation and webhook behavior explicit without promising those integrations in the target product. -->

# Integrations

> **Audience:** First-party operators | Integrators | Contributors | AI agents
> **Status:** Mixed
> **Owner:** Platform/Ops
> **Last Verified:** 2026-07-16
> **Source Anchors:** `src/Explore.API/Controllers/IncomingWebhooksController.cs`, `src/Explore.API/Controllers/ModerationIntegrationController.cs`, `src/Explore.API/Services/IncomingWebhookIntakeService.cs`, `src/Explore.API/Services/CoopWebhookSignatureValidator.cs`, `src/Explore.Infrastructure/Services/Moderation/`, `docs/API.md`, `docs/SECURITY-MODEL.md`

Incoming integration webhooks are provider callbacks received by the current imported application. They are not controlled by the outgoing webhook provider mode. Coop, Osprey, Svix, moderation, and tenant-addressed behavior below remain implementation facts only while their callers survive init Phase 10; they are not promised capabilities of the first-party website.

```text
Provider callback
  -> raw body read
  -> provider signature/API-key verification
  -> incoming_webhook_messages idempotency row
  -> Application command or outbox-backed side effect
```

Outgoing product webhooks use `Local`, `Svix`, `Composite`, `DryRun`, or `Disabled`; see [WEBHOOKS.md](WEBHOOKS.md).

## Incoming Routes

| Route | Provider | Authentication | Behavior |
|---|---|---|---|
| `POST /api/integrations/moderation/coop/callback` | Coop-compatible review queue | API key policy plus timestamped HMAC-SHA256 over raw body | Captures verified decision callbacks idempotently, records local report decisions, then dispatches local moderation execution. |
| `POST /api/integrations/moderation/osprey/callback` | Osprey-compatible signal worker | API key policy | Records bounded moderation signals on the local report; it does not directly execute moderation actions. |
| `POST /api/integrations/svix/operational` | Svix operational webhook | `[AllowAnonymous]` at HTTP edge plus Svix-compatible signature verification | Verifies operational callbacks with `svix-id`, `svix-timestamp`, and `svix-signature`; tenant-addressed payloads are captured in the incoming ledger. |

These routes continue to work when outgoing webhooks are `Disabled`, `Local`, `Svix`, `Composite`, or `DryRun`.

## Verification Rules

- Read the raw request body before JSON parsing.
- Verify signatures before processing side effects.
- Enforce configured max body sizes.
- Use provider message IDs as replay/idempotency keys.
- Acknowledge duplicate verified callbacks without replaying side effects.
- Persist only bounded intake metadata: tenant when present, provider, provider message ID, idempotency key, event type, payload hash, redacted headers, status, and failure category.
- Route side effects through Application commands or outbox-backed services; provider callbacks must not directly mutate sensitive aggregates.

ProblemDetails, logs, metrics, traces, screenshots, and issue templates must not include raw bodies, signature headers, authorization headers, provider secrets, API keys, bearer tokens, tenant/user identifiers, provider message IDs, or raw provider exceptions.

## Coop Flow

Coop integration is independent of outgoing product webhooks:

```text
User reports event
  -> local report, case, and provider-sync intent are stored
  -> optional Coop review-queue mirror runs from provider settings
  -> Coop sends signed callback
  -> ISLAMU verifies API key and HMAC signature
  -> incoming_webhook_messages records idempotency
  -> local report decision is created
  -> light moderation or heavy redaction command executes through existing local paths
```

The Coop mirror request is metadata-first. It excludes reporter text, reporter IP/User-Agent hashes, event titles, slugs, URLs, raw provider payloads, and provider secrets.

## Svix Operational Callback

Svix operational callbacks use Svix-compatible verification. Configure:

| Key | Purpose |
|---|---|
| `Webhooks:Svix:OperationalWebhookSecretRef` | Secret binding key used by the verifier. |
| `WEBHOOKS_SVIX_OPERATIONAL_WEBHOOK_SECRET` | Local/deployment environment value used by the development seeder when configured. |

This callback does not require `Webhooks:Provider=Svix`; it is an incoming integration endpoint, not the outgoing SvixProvider.

## Related

- [WEBHOOKS.md](WEBHOOKS.md)
- [API.md](API.md#incoming-integration-webhooks)
- [CONFIGURATION.md](CONFIGURATION.md)
- [SECURITY-MODEL.md](SECURITY-MODEL.md)
