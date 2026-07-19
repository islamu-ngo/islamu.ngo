ABOUTME: Operational reference for the first-party ISLAMU website and contributor-local runtime.
ABOUTME: Defines startup, health, integration ownership, redaction, and recovery boundaries.

# Operations

> **Audience:** First-party operators | Contributors | AI agents
> **Status:** Active
> **Owner:** Platform/Ops
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.AppHost/AppHost.cs`, `src/Explore.API/Program.cs`, `src/Explore.ServiceDefaults/Extensions.cs`, `docker-compose.yml`, `compose.production.yaml`, `docs/PRODUCTION_OPERATIONS.md`, `docs/BACKUP_RESTORE_UPGRADE.md`

This document owns runtime behavior shared by local development and first-party operations. It does
not define a third-party self-hosting product. Use the production runbook for deployment order and
the focused references for configuration, secrets, and recovery.

## Runbook Map

| Task | Canonical document |
|---|---|
| Run the repository locally | [GETTING_STARTED.md](GETTING_STARTED.md) |
| Deploy or recover the first-party production topology | [PRODUCTION_OPERATIONS.md](PRODUCTION_OPERATIONS.md) |
| Back up, restore, upgrade, or roll back persisted state | [BACKUP_RESTORE_UPGRADE.md](BACKUP_RESTORE_UPGRADE.md) |
| Configure runtime settings and optional integrations | [CONFIGURATION.md](CONFIGURATION.md) |
| Manage secret ownership and rotation | [SECRETS.md](SECRETS.md) |
| Diagnose a concrete symptom | [TROUBLESHOOTING.md](TROUBLESHOOTING.md) |

## Read-Only Doctor

Run the repository doctor from the root:

```bash
dotnet run --project src/Explore.Diagnostic/Explore.Diagnostic.csproj -- --root .
```

Doctor checks SDK and Docker tooling, local Compose topology, discrete PostgreSQL bootstrap names,
documentation targets, and selected diagnostic artifacts. It does not repair configuration, create
secrets, start services, run migrations, call write endpoints, or persist setup state. Output must
redact passwords, connection strings, tokens, cookies, setup secrets, provider responses, and raw
authorization headers.

## Local Aspire Operations

The default command starts `DefaultLocal`:

```bash
aspire run --apphost src/Explore.AppHost/Explore.AppHost.csproj
```

The current AppHost modes are:

| Mode | Local resources |
|---|---|
| `DefaultLocal` | Local data/cache and the normal platform set, including Mailpit, Keycloak, Cerbos, MinIO, RabbitMQ, and Svix. |
| `FullLocal` | `DefaultLocal` plus heavy tools/providers such as Weblate, Coop, Osprey, PgAdmin, Prometheus, and Grafana. |
| `LocalDataExternalPlatform` | Local PostgreSQL, Redis, and Mailpit; external platform configuration. |
| `ExternalInfra` | External data/platform configuration; Mailpit remains local. |

Every mode starts `Event.MigrationService`, API, and Blazor. API waits for migration completion;
Blazor waits for API readiness. Use Aspire resource metadata for endpoints because isolated runs can
assign dynamic ports.

## Runtime Startup Contract

The supported startup order is data safety first:

1. Resolve configuration and secret ownership without printing values.
2. Establish PostgreSQL and cache connectivity.
3. Provision or verify only ISLAMU-owned Keycloak resources.
4. Apply application and data-protection migrations before API/UI traffic.
5. Start API-hosted workers and readiness checks.
6. Start the Blazor BFF and expose browser traffic through it.

Local Compose performs this with service dependencies; `compose.production.yaml` makes validation,
provisioning, migration, API, and UI separate ordered services. Do not run a production migration
without verified database backup/restore evidence.

## Health And Management Endpoints

| Endpoint | Meaning | Operator action |
|---|---|---|
| `/alive` | Process liveness | Restart or replace the process when it is not `200`. |
| `/health` | Readiness for required dependencies | `Healthy`/`Degraded` return `200`; `Unhealthy` returns `503`. Treat required-dependency failures as non-deployable. |
| `/metrics` | Prometheus metrics | In production, scrape only the internal management listener on port `9090`; `404` on application port `8080` is expected. |

Readiness can include PostgreSQL, distributed cache, OIDC discovery, SMTP, durable email dispatch,
storage, reconciliation, secret provider, authorization provider, and BFF-to-API reachability. A
`Degraded` optional integration is acceptable only when it was intentionally disabled and core
flows remain available.

## Retained Integration Boundaries

### Localization: Weblate And Tolgee

- Embedded/static bundles remain the offline fallback. Writable overrides live under
  `{ContentRoot}/App_Data/Localization/Bundles/{code}.json`; persist that path when operator edits
  must survive restarts.
- `FullLocal` Aspire and the Compose `localization` profile can run Weblate for development.
- Weblate is the documented first-party default integration. Tolgee remains a source-backed
  configurable adapter and is not deleted while its runtime caller exists.
- Provider outage must fall back to bundles; never log bundle content, translation values, project
  secrets, or API tokens.

See [LOCALIZATION.md](LOCALIZATION.md) for provider-specific behavior.

### Analytics: External Configurable PostHog

- Analytics defaults disabled through provider `none`.
- PostHog remains the rich analytics and feature-flag integration through
  `analytics.provider=posthog` and an operator-controlled `analytics.endpoint_url`.
- This repository does not start a local PostHog service. An endpoint value configures an external
  service or first-party proxy; it is not a hosting promise.
- Analytics failures are best-effort observational loss and must not break website routes or writes.
- Session replay, autocapture, heatmaps, and toolbar remain disabled by default. Consent and privacy
  settings must match the approved public notice before client tracking is enabled.

### Email, Mailpit, And Listmonk

- Mailpit is local/test SMTP capture only. It starts in every Aspire profile and in local Compose;
  it is never a production SMTP provider.
- Product email uses PostgreSQL `EmailDispatchOutbox` as durable authority. TickerQ or the hosted
  fallback triggers the same drain service; retries, attempts, receipts, unknown outcomes, and
  dead-letter state stay database-backed.
- Optional RabbitMQ carries pointer-only dispatch messages and never becomes the email source of
  truth. It must not contain recipients, subjects, bodies, SMTP secrets, or provider responses.
- Listmonk remains an optional external integration for an approved newsletter/waitlist capability.
  It is disabled by default, is not started by Compose/Aspire, and does not itself publish a public
  website feature.
- Keycloak owns identity-lifecycle email tokens, templates, and delivery. Product email workers must
  not replay Keycloak verification or password-reset mail.
- Logs and metrics use bounded outcome/failure categories only; never include recipients, bodies,
  secret values, provider payloads, or raw errors.

See [EMAIL_NOTIFICATIONS.md](EMAIL_NOTIFICATIONS.md) for the durable delivery model.

### Cerbos Authorization

- Local development mounts the repository policy package into Cerbos. Selecting Local RBAC does not
  require a Cerbos process.
- When Cerbos is selected, unavailable PDP or unpublished required policies fail closed; do not
  silently fall back to a more permissive provider.
- `compose.production.yaml` does not bundle Cerbos. Production Cerbos, if selected, is operated as
  an external dependency with server-held credentials.
- Browser clients receive authorization outcomes through API/HAL affordances. They never receive
  Cerbos Admin API credentials or recreate role/claim authorization locally.

## First-Party Production Boundary

`compose.production.yaml` contains exactly six services: networkless environment validation,
PostgreSQL, additive Keycloak provisioning, one-shot migration, API, and Blazor BFF. Only the BFF
publishes an application port. Keycloak, Redis-compatible cache, TLS/reverse proxy, SMTP, Cerbos,
analytics storage, and backup automation are external operational dependencies unless a future
source-backed production change adds them explicitly.

Use [PRODUCTION_OPERATIONS.md](PRODUCTION_OPERATIONS.md) for required variable names, startup order,
health checks, rollback gates, and evidence status.

## Security And Secret Rules

- Browser-controlled setup, tenant, authorization, and provider-administration headers are stripped
  at the BFF; trusted server state is validated again by the API.
- Temporary Keycloak administrator credentials are request/job scoped and never persisted.
- Deployment-managed secrets rotate in the environment or external provider. Application-managed
  secrets stay write-only and are never returned after save.
- Keep API, Keycloak, SMTP, PostHog, Listmonk, Cerbos, and TMS credentials out of logs, traces,
  health payloads, screenshots, issue templates, and support bundles.
- Preserve ASP.NET Core Data Protection keys with the application database; losing them invalidates
  BFF cookies and protected setup state.

## Recovery Order

1. Remove new traffic at the proxy/platform boundary.
2. Preserve logs and the immutable release manifest.
3. Determine whether migrations ran; restore PostgreSQL before dependent services when required.
4. Restore storage data and data-protection state from the matching snapshot.
5. Restore or verify Keycloak and cache according to the incident scope.
6. Restart in dependency order and verify `/alive`, `/health`, then internal `9090` metrics.
7. Restore traffic only after authentication, BFF-to-API routing, and required integrations pass.

Detailed backup and rollback procedures remain in
[BACKUP_RESTORE_UPGRADE.md](BACKUP_RESTORE_UPGRADE.md).
