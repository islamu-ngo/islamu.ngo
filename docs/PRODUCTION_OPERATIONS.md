<!-- ABOUTME: First-party production deployment runbook for ISLAMU NGO operators. -->
<!-- ABOUTME: Captures external prerequisites, service order, health checks, rollback gates, and deferred evidence. -->

# Production Operations Runbook

| Field | Value |
|---|---|
| Audience | Operators/maintainers |
| Status | Draft until execution evidence exists |
| Owner | Operations |
| Last Verified | 2026-07-18 |
| Source Anchors | `compose.production.yaml`; `docker/production-validation/validate-production-environment.sh`; `docker/keycloak/provision-islamu-ngo-client.sh`; `docker/healthcheck.sh`; `src/Event.MigrationService/Dockerfile`; `src/Explore.ServiceDefaults/Extensions.cs`; `docs/OPERATIONS.md`; `docs/BACKUP_RESTORE_UPGRADE.md`; `docs/CONFIGURATION.md`; `docs/SECRETS.md`; `docs/LOCALIZATION.md`; `docs/EMAIL_NOTIFICATIONS.md`; `docs/SECURITY-MODEL.md` |

This runbook is for first-party ISLAMU NGO production operations. It is not a self-hosting guide and does not promise bundled infrastructure beyond the services declared in `compose.production.yaml`.

## Production Boundary

Production deployment assumes these external prerequisites already exist and are operated outside this repository:

- TLS and reverse-proxy/platform routing for the public BFF endpoint.
- External Keycloak over HTTPS, with the target realm already created and a dedicated admin service client available for provisioning.
- External Redis-compatible cache for `ConnectionStrings__cache` and `CACHE_CONNECTION_STRING`.
- PostgreSQL with verified backup and restore capability before any migration is allowed.
- Registry-built application images promoted by immutable tag or digest evidence.

Do not infer bundled production ownership for Keycloak, Cerbos, SMTP, telemetry storage, backup automation, or reverse proxy infrastructure. Add those only when the repository contains explicit production implementation and evidence.

### Integration Ownership

Current source retains several configurable integrations without adding them to the production
Compose topology:

- **Weblate** is available as an optional local localization resource and as a configurable TMS
  provider. A production Weblate endpoint is externally operated or separately deployed.
- **Tolgee** remains a source-backed configurable TMS adapter. The repository does not declare a
  Tolgee production service.
- **PostHog** remains an external configurable analytics provider. The selected
  `analytics.endpoint_url`, transport, consent, and credential settings belong to that provider
  connection; no PostHog container is bundled.
- **Mailpit** is local/test SMTP capture only. Production email uses the approved external SMTP
  route.
- **Listmonk** is an optional external subscriber-synchronization integration, disabled unless
  intentionally configured. It is separate from SMTP dispatch and is not a production service in
  this topology.
- **Cerbos** remains the supported external authorization provider when selected. Runtime PDP and
  Admin API configuration are external dependencies; production Compose does not bundle Cerbos.

Provider URLs, ports, image tags, and credential shapes are technical integration values. They do
not promise that ISLAMU operates, distributes, or supports third-party infrastructure as a product.

## Service Chain

`compose.production.yaml` declares this exact startup chain:

1. `production-environment-validation` runs once with `network_mode: none`. It validates required variable names and formats, rejects placeholders/local-only values, redacts values, and prints only success or variable-name failures.
2. `postgres` starts after validation succeeds and gates dependents with `pg_isready`.
3. `keycloak-ngo-provisioner` runs once after validation and healthy PostgreSQL. It connects to the external Keycloak realm and creates or updates only owned ISLAMU NGO clients and mapper.
4. `islamu-ngo-migration` runs once after PostgreSQL is healthy and Keycloak provisioning succeeds. Its image entrypoint is `dotnet Event.MigrationService.dll`.
5. `islamu-ngo-api` starts after PostgreSQL, provisioning, and migration. API hosted workers run in-process with the API container; they are not separate Compose services.
6. `islamu-ngo-ui` starts after the API is healthy and the one-shot provisioner and migration have completed successfully.

Only `islamu-ngo-ui` publishes the public application port: `${ISLAMU_BFF_PUBLIC_PORT}:8080`. The API listens internally on `8080`; it is not host-published. Both API and UI expose management port `9090` internally. `/metrics` returns Prometheus metrics only when reached on local port `9090`; the same path returns `404` on other ports.

## Required Configuration Names

List names only in deployment tickets and evidence. Never paste values, sample secrets, or generated credentials into documentation, issues, logs, or chat.

Operator-supplied Compose substitution variables:

- `POSTGRESQL_DATABASE`
- `POSTGRESQL_USERNAME`
- `POSTGRESQL_PASSWORD`
- `ISLAMU_API_PUBLIC_URL`
- `ISLAMU_BFF_PUBLIC_URL`
- `ISLAMU_BFF_PUBLIC_PORT`
- `KEYCLOAK_AUTHORITY`
- `KEYCLOAK_INTERNAL_URL`
- `KEYCLOAK_REALM`
- `KEYCLOAK_ADMIN_REALM`
- `KEYCLOAK_ADMIN_SERVICE_CLIENT_ID`
- `KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET`
- `KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET`
- `KEYCLOAK_ISLAMU_NGO_REDIRECT_URIS`
- `KEYCLOAK_ISLAMU_NGO_WEB_ORIGINS`
- `KEYCLOAK_ISLAMU_NGO_LOGOUT_REDIRECT_URIS`
- `SETUP_SECRET`
- `CACHE_CONNECTION_STRING`

Compose-controlled first-party Keycloak invariants, not operator-selected values:

- `KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID`
- `KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID`
- `KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME`
- `KEYCLOAK_ISLAMU_NGO_PROVISIONING_REQUIRED`

Compose-injected application settings, not additional operator inputs:

- `POSTGRESQL_HOST`
- `POSTGRESQL_PORT`
- `ConnectionStrings__cache`
- `Keycloak__Authority`
- `Keycloak__MetadataAddress`
- `Keycloak__Realm`
- `Keycloak__ClientId`
- `Keycloak__ClientSecret`
- `Keycloak__Audience`
- `Keycloak__ValidAudiences__0`
- `Keycloak__ValidAudiences__1`
- `PublicBaseUrl`
- `API_ENDPOINT`
- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_HTTP_PORTS`
- `SecretProvider__Provider`
- `Storage__Local__RootPath`
- `Storage__Local__CreateRootIfMissing`

## Deployment Procedure

Final deployment commands are deferred until operator approval and verified cloud backup evidence exist. The intended future command shape may be documented only as `NOT RUN` in this workstream.

```bash
# NOT RUN in this workstream: final command pending operator approval and verified cloud backup evidence.
docker compose -f compose.production.yaml up --detach
```

Before any production run:

1. Verify the external PostgreSQL backup can be restored. Do not start migration without this gate.
2. Verify image promotion evidence uses immutable tags or digests. Do not rely on `latest` after promotion.
3. Verify external Keycloak realm access over HTTPS using the dedicated admin service client.
4. Verify Redis/cache availability because distributed-cache readiness participates in service health.
5. Verify BFF public URL, API public URL, redirect URIs, web origins, and post-logout redirect URIs match the TLS/proxy boundary.
6. Run the networkless production validator first. Treat any failure as a deployment stop, not a warning.
7. Allow the Keycloak provisioner to complete before migration. It is additive and owns only `islamu-ngo-blazor`, `islamu-ngo-api`, and `islamu-ngo-api-audience`.
8. Allow migration to complete before API/UI rollout. If migration fails, stop rollout and investigate before starting application containers.
9. Point load-balancer readiness at `/health` and liveness at `/alive` on the public BFF route. Treat `Unhealthy` as non-deployable.
10. Check metrics only through the internal management network on port `9090`.

## Health And Triage

`/alive` is the liveness endpoint. `docker/healthcheck.sh` sends `GET /alive` by default and accepts only HTTP `200` in the first status line.

`/health` is the readiness endpoint. Healthy and Degraded return `200`; Unhealthy returns `503`. Readiness includes checks such as distributed cache and OIDC discovery where configured.

`/metrics` is for Prometheus scraping on management port `9090` only. A `404` on application port `8080` is expected and is not a metrics failure.

First triage order:

1. Check validator result for missing, placeholder, local-only, or malformed variable names.
2. Check PostgreSQL health and migration completion.
3. Check Keycloak realm availability, client provisioning result, and OIDC discovery readiness.
4. Check Redis/cache connection because API and BFF readiness depend on it.
5. Check BFF-to-API internal routing through `API_ENDPOINT=http://islamu-ngo-api:8080/`.
6. Check management-port metrics from the internal monitoring path, not the public route.

## Rollback And Recovery

Rollback starts with data safety, not containers:

1. Stop new traffic at the proxy/platform boundary.
2. Preserve logs and the release manifest, including image tags or digests.
3. If migration ran, assume database restore is required unless release notes explicitly prove image-only rollback is safe.
4. Restore PostgreSQL before starting services that depend on it.
5. Restore storage data and data-protection material consistently with the database snapshot. Blazor BFF session protection depends on data-protection keys.
6. Restore or verify Redis/cache state as operationally appropriate. Treat cache loss as session/temporary-data impact unless the owning feature says otherwise.
7. Roll back images by digest or immutable tag, never by mutable `latest`.
8. Re-run Keycloak provisioning only when needed. It is additive and limited to owned clients and mapper, but it still requires the existing realm and service-client credentials.
9. Restart the service chain in dependency order: validation, PostgreSQL, Keycloak provisioner, migration, API, UI.
10. Re-check `/alive`, `/health`, and internal `9090` metrics before restoring traffic.

## Secret Rotation

Rotate deployment-managed secrets in the external environment or secret provider, then redeploy the affected services. With `SecretProvider__Provider=None`, services read explicit environment/appsettings values and do not contact an external provider.

Rotation rules:

- Never log or persist temporary provider admin credentials.
- Never paste secret values into tickets, docs, chat, shell history, or evidence files.
- Keep `POSTGRESQL_PASSWORD`, `KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET`, `KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET`, `SETUP_SECRET`, and cache secrets distinct.
- After rotation, verify readiness and authentication flows through the BFF boundary.

## Security Invariants

- Browser clients receive only the BFF HttpOnly session cookie; bearer tokens stay server-side.
- Browser traffic reaches the API through the BFF `/api/*` proxy, not by direct public API exposure.
- The API remains internal on `8080`; only the BFF public port is published.
- HAL links remain the UI action source of truth; do not gate edit/delete affordances by local role or claim checks.
- Browser-controlled setup/admin headers are stripped by the BFF boundary.
- Forwarded headers are trusted only when the proxy boundary is explicitly configured.
- Keycloak owns identity lifecycle email; do not add local identity email behavior from this runbook.
- Logs and diagnostics must redact passwords, secrets, keys, tokens, credentials, connection strings, and API keys.

Forbidden actions:

- Do not run production migration before verified PostgreSQL backup/restore evidence exists.
- Do not publish `islamu-ngo-api` directly to the public internet.
- Do not expose management port `9090` publicly.
- Do not use local, placeholder, `.test`, `example`, `changeme`, `dummy`, `default`, or sample secret values.
- Do not create or mutate non-owned Keycloak resources from the ISLAMU NGO provisioner.
- Do not document private Control Plane identifiers or secret values.

## Evidence Status

Authored from static source/config reads only. Under the current workstream constraint, Compose execution, runtime startup, Docker builds, database schema execution, Keycloak smoke tests, cache smoke tests, parser/linter/docs checks, and application tests were not run.

Evidence captured:

- Production Compose topology and declared service chain were read from `compose.production.yaml`.
- Networkless, redacting validation behavior was read from `docker/production-validation/validate-production-environment.sh`.
- Keycloak owned-resource provisioning behavior was read from `docker/keycloak/provision-islamu-ngo-client.sh`.
- Healthcheck path and success criteria were read from `docker/healthcheck.sh` and `src/Explore.ServiceDefaults/Extensions.cs`.
- Migration image entrypoint was read from `src/Event.MigrationService/Dockerfile`.
- Security, configuration, secrets, backup, and operations constraints were read from the linked existing docs.

Task 3.4 live blockers remain:

- External database schema drift is not verified.
- Required cache service is not available for smoke evidence.
- External Keycloak smoke test is not available.

## Related Docs

- `docs/OPERATIONS.md`
- `docs/BACKUP_RESTORE_UPGRADE.md`
- `docs/CONFIGURATION.md`
- `docs/SECRETS.md`
- `docs/SECURITY-MODEL.md`
- `docs/DOCUMENTATION_STYLE_GUIDE.md`
- `docs/DOCUMENTATION_ARCHITECTURE.md`

Index-link coordination remains deferred to the concurrent docs workstream that owns `docs/index.md`.
