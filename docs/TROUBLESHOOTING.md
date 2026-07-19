ABOUTME: Symptom-driven troubleshooting for local and first-party ISLAMU website operations.
ABOUTME: Preserves trust boundaries while diagnosing runtime, provider, email, and localization failures.

# Troubleshooting

> **Audience:** Contributors | First-party operators | Maintainers
> **Status:** Active
> **Owner:** Platform/Ops
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.Diagnostic`, `src/Explore.AppHost/AppHost.cs`, `src/Explore.API/Program.cs`, `src/Explore.Blazor`, `src/Explore.Infrastructure`, `docker-compose.yml`, `compose.production.yaml`, `docs/OPERATIONS.md`, `docs/PRODUCTION_OPERATIONS.md`

Start with the smallest failing boundary. Do not change data, rotate credentials, bypass
authorization, or rebuild infrastructure until the failing service and configuration owner are
known.

## Runbook Map

| Situation | Canonical guide |
|---|---|
| First contributor start or local profile choice | [GETTING_STARTED.md](GETTING_STARTED.md) |
| Shared health, startup, shutdown, and recovery order | [OPERATIONS.md](OPERATIONS.md) |
| Production deployment, rollback, and six-service topology | [PRODUCTION_OPERATIONS.md](PRODUCTION_OPERATIONS.md) |
| Configuration authority and provider selectors | [CONFIGURATION.md](CONFIGURATION.md) |
| Secret ownership, redaction, and rotation | [SECRETS.md](SECRETS.md) |
| Backup, restore, and upgrade workflow | [BACKUP_RESTORE_UPGRADE.md](BACKUP_RESTORE_UPGRADE.md) |
| Authentication and authorization invariants | [SECURITY.md](SECURITY.md) |

## Quick Triage

1. Identify whether the incident is local Aspire/Compose or the first-party production deployment.
2. Run the repository doctor from the repository root; its checks are read-only:

   ```bash
   dotnet run --project src/Explore.Diagnostic/Explore.Diagnostic.csproj -- --root .
   ```

3. Check the failing service's `/alive` endpoint before `/health`.
4. Read startup logs in dependency order and redact all secret-bearing values.
5. Confirm the selected Aspire mode or production environment variables at the authoritative
   deployment source.
6. If the incident followed a restore or upgrade, stop normal changes and follow
   [BACKUP_RESTORE_UPGRADE.md](BACKUP_RESTORE_UPGRADE.md).

Local endpoints are discoverable resources. Aspire isolation can change ports; do not diagnose
against a memorized port when the dashboard or `aspire describe` reports another endpoint.

## Local Runtime Does Not Start

### Wrong Aspire profile or missing external values

Symptoms include absent local dependencies, application projects waiting indefinitely, or
validation failures for platform URLs.

Checks:

1. Confirm `ISLAMU_ASPIRE_MODE` through the selected launch profile:
   `DefaultLocal`, `FullLocal`, `ExternalInfra`, or `LocalDataExternalPlatform`.
2. Use `local-default` for the ordinary contributor path. Use `local-full` only when heavy local
   integrations are required.
3. In `local-core` or `local-lite`, provide all externally owned service endpoints and credentials;
   AppHost intentionally does not invent them.
4. Check the Aspire dashboard for the first unhealthy dependency rather than restarting every
   resource.

### Compose reports an unhealthy dependency

Check the dependency itself before the API/UI:

- PostgreSQL readiness and the configured database/user.
- Keycloak database, Keycloak health, and `keycloak-ngo-provisioner` completion.
- Redis/cache connectivity when the selected local path owns it.
- Cerbos readiness when the Cerbos profile/provider is selected.
- Mailpit SMTP readiness for local email capture.

Local Compose values are disposable fixtures. Do not apply those passwords, ports, or provider
defaults to production.

## API, BFF, Or Authentication Failure

### `/alive` succeeds but `/health` fails

The process is running but a dependency is not ready. Inspect the named readiness check and then
the corresponding service/configuration owner. Common boundaries are PostgreSQL, cache, SMTP,
secret provider, and selected authorization provider.

### Login loops, `unauthorized_client`, or redirect mismatch

1. Confirm the browser uses the UI/BFF public HTTPS origin.
2. Confirm Keycloak realm, authority, confidential client ID/secret, redirect URI, and web origin
   match the same deployment.
3. Confirm the first-party provisioner completed before expecting the NGO client to exist.
4. Behind a proxy, verify forwarded-header trust and the externally visible scheme/host.
5. Do not fix the error by exposing a confidential client secret to the browser.

### API writes return `401` or `403`

- `401`: verify the BFF session, token forwarding, issuer, audience, and expiry.
- `403`: inspect server-side authorization and the resource's HAL affordances.
- Never add client-side role/claim checks to force Edit/Delete buttons to appear. Missing HAL links
  mean the action is unavailable to that representation.

### Cerbos-selected actions disappear

Cerbos is fail-closed. Check the configured PDP endpoint first, then controlled policy-package
status/synchronization through the Admin API. Runtime PDP and Admin API credentials are separate.
Do not invent an arbitrary decision-test or resource-inventory endpoint as a recovery shortcut.

For an explicit deployment-managed `cerbos` selection, the browser cannot override deployment
intent. Repair the external endpoint/credentials and retry the existing reconciliation path. For
explicit `local`, no Cerbos call should occur.

## Production Service Chain Failure

Troubleshoot in the order encoded by `compose.production.yaml`:

1. `production-environment-validation`
2. `postgres`
3. `keycloak-ngo-provisioner`
4. `islamu-ngo-migration`
5. `islamu-ngo-api`
6. `islamu-ngo-ui`

If validation fails, correct the named deployment variable; do not bypass the validator. If
migration fails, leave API/UI stopped until the migration is understood or the approved rollback
is complete. Only the UI/BFF publishes a public port. API port `8080` and management port `9090`
remain internal to the production network.

Follow [PRODUCTION_OPERATIONS.md](PRODUCTION_OPERATIONS.md) for approved commands, backup gates,
evidence status, and rollback order.

## Secret Provider Failure

1. Confirm `SecretProvider:Provider` is an implemented value: `None` or `Infisical`.
2. Read the startup validation error before changing any key.
3. With `None`, verify the expected environment/appsettings/user-secret owner supplied the key.
4. With `Infisical`, verify URL, project, environment, Universal Auth credentials, and configured
   paths from the secret-management side without printing values.
5. Check the `secret_provider` readiness result and bounded refresh-failure metadata.
6. Do not switch to placeholder enum values such as Vault/Azure/AWS; they are not implemented.

PostgreSQL bootstrap uses discrete `POSTGRESQL_*` values. Do not substitute a URL-form connection
string unless source explicitly binds it. See [SECRETS.md](SECRETS.md).

## Localization And TMS Failure

### Offline translations still appear

That is expected when the TMS provider is `None`, offline mode is forced, or the configured adapter
cannot supply a bundle. Confirm:

- `localization.force_offline_mode`;
- `localization.tms_provider` (`1=Tolgee`, `2=Weblate`);
- provider API URL, project, and Weblate component where applicable;
- the server-side TMS credential binding;
- network access from the API, not from the browser.

Do not log translation payloads or credentials while testing.

### Local Weblate is absent

Weblate is a heavy optional development resource. Use `local-full` or the Compose localization
profile when the task requires it. Its local presence does not imply a Weblate service in
production. Tolgee remains a configurable adapter; no Tolgee container is bundled.

See [LOCALIZATION.md](LOCALIZATION.md) for provider-specific verification.

## Analytics Or PostHog Failure

1. Confirm `analytics.enabled` is intentionally true.
2. Confirm `analytics.provider=posthog` for PostHog rather than inferring the provider from an
   endpoint or key.
3. Verify `analytics.endpoint_url`; there is no canonical `analytics.endpoint` alias.
4. Confirm `analytics.transport_mode` matches the network path (`direct`, `proxy`, or `relay`).
5. Check the browser privacy kill switch, consent mode, and consent-dependent options before
   treating missing events as transport failure.
6. Keep the personal API key server-side and redact all analytics credentials.

There is no bundled local or production PostHog service. Diagnose the configured external endpoint
and its proxy/relay path separately from the website containers.

## Email, Mailpit, Or Listmonk Failure

### Local message does not appear in Mailpit

1. Open the Mailpit UI endpoint reported by Aspire, or the configured Compose endpoint.
2. Check the API `smtp` readiness result.
3. Confirm `email.*` resolved from `MAIL_SMTP_*` or the active Mailpit service-discovery values.
4. With isolated Aspire, re-check the allocated SMTP port; persistent local database settings may
   otherwise point to a previous port.
5. Check the email outbox/dispatch worker rather than resending manually and creating duplicates.

Mailpit is for local/test capture only. Production delivery must use the approved external SMTP
route.

### Production email is queued but not delivered

- Verify SMTP host/port/TLS/from identity and secret ownership without printing credentials.
- Check durable outbox state, dispatch health, bounded retry/dead-letter behavior, and correlation
  IDs.
- If RabbitMQ dispatch mode is intentionally enabled, check broker topology separately; basic
  dispatch does not require RabbitMQ.
- Keycloak realm email is a separate transport owned by Keycloak.

### Listmonk synchronization fails

Confirm the optional integration is enabled intentionally, the external instance URL is reachable,
the default list exists, and the API username/key binding is configured. Listmonk synchronization
is separate from product SMTP dispatch and no Listmonk service is bundled by this repository.

See [EMAIL_NOTIFICATIONS.md](EMAIL_NOTIFICATIONS.md).

## Rate Limit Or Timeout Responses

- `429` means a named rate-limit policy rejected the request. Respect `Retry-After`; do not retry in
  a tight loop.
- `504` means the configured request timeout expired. Correlate logs/traces and diagnose the slow
  downstream dependency before increasing a timeout.
- Preserve idempotency keys for retried writes and do not bypass outbox/idempotency safeguards.

## Restore Or Upgrade Regression

Stop state-changing operations and record:

- deployed image/tag and configuration revision;
- migration stage and last successful backup evidence;
- affected health checks and correlation IDs;
- whether PostgreSQL, Keycloak provisioning, migration, API, or UI first failed.

Use [BACKUP_RESTORE_UPGRADE.md](BACKUP_RESTORE_UPGRADE.md) for the approved restore validation and
rollback procedure. Do not delete volumes or rerun destructive provisioning as a diagnostic step.

## Escalation Evidence

Provide only sanitized evidence:

- failing service and timestamp/time zone;
- selected local profile or production deployment revision;
- health-check name/status, HTTP status, and correlation ID;
- redacted configuration names and their ownership source;
- last known-good version and backup verification status.

Never attach secret values, tokens, cookies, raw provider responses, database dumps, translation
content, or user data.
