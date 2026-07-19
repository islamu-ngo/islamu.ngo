<!-- ABOUTME: Documents the surviving Svix operational callback and its verification boundaries. -->
<!-- ABOUTME: Separates incoming provider intake from outgoing webhook delivery configuration. -->

# Integrations

> **Audience:** First-party operators | Integrators | Contributors | AI agents
> **Status:** Active
> **Owner:** Platform/Ops
> **Last Verified:** 2026-07-19
> **Source Anchors:** `src/Explore.API/Controllers/IncomingWebhooksController.cs`, `src/Explore.API/Services/IncomingWebhookIntakeService.cs`, `src/Explore.API/Services/SvixIncomingWebhookVerifier.cs`, `src/Explore.API/Services/IncomingWebhookVerifierRegistry.cs`, `docs/API.md`, `docs/SECURITY-MODEL.md`

The current incoming integration surface accepts signed Svix operational callbacks. Incoming
verification and capture are independent from the outgoing webhook provider mode.

```text
Provider callback
  -> raw body read
  -> Svix signature and provider-binding verification
  -> incoming_webhook_messages idempotency row
  -> durable incoming-webhook processing
```

Outgoing product webhooks use `Local`, `Svix`, `Composite`, `DryRun`, or `Disabled`; see [WEBHOOKS.md](WEBHOOKS.md).

## Incoming Routes

| Route | Provider | Authentication | Behavior |
|---|---|---|---|
| `POST /api/integrations/svix/operational` | Svix operational webhook | `[AllowAnonymous]` at HTTP edge plus Svix-compatible signature verification | Verifies operational callbacks with `svix-id`, `svix-timestamp`, and `svix-signature`; tenant-addressed payloads are captured in the incoming ledger. |

This route continues to work when outgoing webhooks are `Disabled`, `Local`, `Svix`, `Composite`, or `DryRun`.

## Verification Rules

- Read the raw request body before JSON parsing.
- Verify signatures before processing side effects.
- Enforce configured max body sizes.
- Use provider message IDs as replay/idempotency keys.
- Acknowledge duplicate verified callbacks without replaying side effects.
- Persist only bounded intake metadata: tenant when present, provider, provider message ID, idempotency key, event type, payload hash, redacted headers, status, and failure category.
- Route side effects through Application commands or outbox-backed services; provider callbacks must not directly mutate sensitive aggregates.

ProblemDetails, logs, metrics, traces, screenshots, and issue templates must not include raw bodies, signature headers, authorization headers, provider secrets, API keys, bearer tokens, tenant/user identifiers, provider message IDs, or raw provider exceptions.

## Svix Operational Callback

Svix operational callbacks use Svix-compatible verification. Configure:

| Key | Purpose |
|---|---|
| `Webhooks:Svix:OperationalWebhookSecretRef` | Secret binding key used by the verifier. |
| `WEBHOOKS_SVIX_OPERATIONAL_WEBHOOK_SECRET` | Local/deployment environment value used by the development seeder when configured. |

This callback does not require `Webhooks:Provider=Svix`; it is an incoming integration endpoint, not the outgoing SvixProvider.

No Coop or Osprey callback route or verifier is registered in the current API.

## Related

- [WEBHOOKS.md](WEBHOOKS.md)
- [API.md](API.md#incoming-integration-webhooks)
- [CONFIGURATION.md](CONFIGURATION.md)
- [SECURITY-MODEL.md](SECURITY-MODEL.md)
