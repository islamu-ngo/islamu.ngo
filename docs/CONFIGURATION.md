ABOUTME: Configuration authority and runtime profiles for first-party ISLAMU operations.
ABOUTME: Separates static deployment values, secrets, governance settings, and optional integrations.

# Configuration

> **Audience:** Contributors | First-party operators | Maintainers | AI agents
> **Status:** Active
> **Owner:** Platform/Ops
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.AppHost/AppHost.cs`, `src/Explore.API/Extensions/ConfigurationExtensions.cs`, `src/Explore.Blazor/Extensions/ConfigurationExtension.cs`, `src/Explore.Blazor/Extensions/YarpProxyExtensions.cs`, `src/Explore.Infrastructure/Services/HierarchicalSettingsResolver.cs`, `src/Explore.Domain/Constants/GovernanceSettingKeys.cs`, `src/Explore.Domain/Secrets/SecretDefinitionRegistry.cs`, `compose.production.yaml`, `.env.example`

This document describes the configuration surfaces used to run the first-party ISLAMU website. A
provider name, endpoint, port, container image, or local fixture is a technical integration value;
it is not a public product tier or a promise that ISLAMU will operate third-party software for
other deployments.

## Authority And Precedence

Keep the three configuration authorities distinct:

1. **Static deployment configuration** comes from appsettings, environment variables, and Aspire
   or Compose wiring. It selects infrastructure, public URLs, BFF/API routing, and bootstrap mode.
2. **Secrets** come from the selected `SecretProvider` or explicit local development sources. See
   [SECRETS.md](SECRETS.md); never place secret values in this document.
3. **Governance settings** are application-managed database values resolved by
   `HierarchicalSettingsResolver`. Their documented defaults and ownership rules are not
   replacements for deployment configuration.

Use standard .NET mapping for environment overrides: `Section:Child` becomes
`Section__Child`. Compatibility aliases in `ConfigurationExtensions` exist for current deployment
inputs; do not create new aliases in documentation.

When a field is explicitly deployment-managed, the deployment source remains authoritative and
the UI must present ownership metadata rather than silently overriding it. Application-managed
secret fields remain write-only after save.

## Runtime Profiles

`src/Explore.AppHost/AppHost.cs` is authoritative for Aspire composition.

| Launch profile | `ISLAMU_ASPIRE_MODE` | Intended use | Local resources |
|---|---|---|---|
| `local-default` | `DefaultLocal` | Normal contributor workflow and the default when the variable is absent | Local data plus the standard local platform set. |
| `local-full` | `FullLocal` | Integration work needing the complete local stack | Standard resources plus heavy optional services such as Weblate and observability tools. |
| `https` | `FullLocal` | Compatibility alias for the full local profile | Same mode as `local-full`. |
| `local-core` | `ExternalInfra` | Maintainer workflow against externally supplied infrastructure | Application projects and always-on local developer resources only. |
| `local-lite` | `LocalDataExternalPlatform` | Local data with externally supplied platform services | Local data plus application projects. |

Mailpit, migration orchestration, and the application projects are part of the current AppHost
composition. Aspire may allocate dynamic ports, especially with isolation; use Aspire discovery
instead of treating local port numbers as stable contracts.

The contributor start path is documented in [GETTING_STARTED.md](GETTING_STARTED.md). Maintainers
using external modes must provide every required endpoint and credential explicitly.

## Core Deployment Configuration

### PostgreSQL

Bootstrap configuration uses discrete values, not a documentation-only database URL:

- `POSTGRESQL_HOST`
- `POSTGRESQL_PORT`
- `POSTGRESQL_DATABASE`
- `POSTGRESQL_USERNAME`
- `POSTGRESQL_PASSWORD`

`BootstrapSecretLoader` composes the application connection string. Keep migration ownership with
the migration service; application containers must not improvise schema changes at startup.

### Cache

Production uses the externally supplied Redis-compatible `CACHE_CONNECTION_STRING`. Local Aspire
profiles wire their local cache resource where the selected mode owns local infrastructure.

### Identity And BFF Routing

Keycloak settings cover the authority/realm, server-side clients, service credentials, and public
redirect/origin values. The browser authenticates through the BFF; do not expose confidential
client credentials or forward browser-supplied authorization/setup administration headers.

Public URLs and proxy trust must match the deployed HTTPS origins. Production validation checks
the required first-party API, UI, Keycloak, redirect, CORS, and trusted-proxy names before the
stateful service chain starts. Use the exact list in
[PRODUCTION_OPERATIONS.md](PRODUCTION_OPERATIONS.md).

### Authorization

The runtime authorization selector supports the repository's Local and Cerbos implementations:

- Blank intent leaves provider choice to onboarding.
- Explicit `local` selects the local implementation and performs no Cerbos call.
- Explicit `cerbos` selects the external PDP path and remains fail-closed until endpoint
  verification and policy synchronization succeed.

`Cerbos:GrpcEndpoint` and `Cerbos:AdminApi:*` are separate concerns: the first serves runtime policy
decisions; the second supports controlled package status/synchronization. Admin API credentials
are secrets and must be redacted. Local Aspire/Compose resources use the repository policy files;
the production Compose topology does not bundle Cerbos.

HAL links remain the UI authorization authority. Configuration must not cause clients to infer
edit/delete affordances from local roles or claims.

## Integration Matrix

These integrations remain supported by current source. Their deployment values describe adapters
and local development fixtures, not an offer to host the provider.

| Integration | Current boundary | Configuration notes |
|---|---|---|
| Weblate | Local `FullLocal`/Compose localization resource and configurable TMS provider | `localization.tms_provider=2`; configure the API URL, project/component, and server-side credential for the selected instance. Weblate is not in the production Compose topology. |
| Tolgee | Source-backed configurable TMS adapter | `localization.tms_provider=1`; configure the API URL/project and server-side credential. No Tolgee service is bundled by AppHost or production Compose. |
| PostHog | External configurable analytics provider | Select `analytics.provider=posthog`, opt in with `analytics.enabled`, and configure `analytics.endpoint_url` plus required keys. No PostHog service is bundled locally or in production. |
| Mailpit | Local/test SMTP capture | Local defaults use `MAIL_SMTP_*`; production must use an approved external SMTP route. Mailpit is not a production mail system. |
| Listmonk | Optional external subscriber synchronization | Disabled unless `LISTMONK_ENABLED` and the corresponding governance settings are intentionally configured. No Listmonk service is bundled. |
| Cerbos | Local development resource or external production PDP/Admin API | Select explicitly, configure endpoint/admin credentials by ownership, and preserve fail-closed behavior. Cerbos is not in production Compose. |

### Localization And TMS

Canonical governance settings include:

- `localization.default_language`
- `localization.enabled_languages`
- `localization.fallback_language`
- `localization.client_picker_enabled`
- `localization.force_offline_mode`
- `localization.tms_provider` (`0=None`, `1=Tolgee`, `2=Weblate`)
- `localization.tms_api_url`
- `localization.tms_project_id`
- `localization.tms_component` (Weblate)
- `localization.tms_api_key` (secret binding)

Offline translation bundles remain the fallback when no TMS is selected or offline mode is
forced. Never send TMS credentials or translated content in diagnostics. See
[LOCALIZATION.md](LOCALIZATION.md).

### Analytics And PostHog

The analytics abstraction is disabled by default. Important keys are:

| Key | Safe default or rule |
|---|---|
| `analytics.provider` | `none`; `posthog` is one explicit provider choice. |
| `analytics.enabled` | `false`; enabling is an operator decision. |
| `analytics.endpoint_url` | Canonical provider base URL; do not invent `analytics.endpoint`. |
| `analytics.transport_mode` | `direct`, `proxy`, or `relay`; relay keeps the write key server-side. |
| `analytics.api_key` | Provider public/write key according to transport; secret binding rules still apply. |
| `analytics.personal_api_key` | Sensitive server-side provider key. |
| `analytics.consent_mode` | Preserve the privacy mode selected by governance. |
| `analytics.global_disable_client_tracking` | Browser-side emergency kill switch. |
| `analytics.posthog_session_replay` | `false` unless explicitly approved with consent handling. |
| `analytics.posthog_autocapture` | `false` unless explicitly approved with consent handling. |
| `analytics.posthog_heatmaps` | `false` unless explicitly approved with consent handling. |

PostHog endpoints may point to a hosted or separately operated instance. That endpoint choice is a
technical provider setting, not a first-party hosting commitment.

### Email, Mailpit, And Listmonk

Product email dispatch resolves the `email.*` governance/secret keys. Local development seeds
those values from `MAIL_SMTP_*`, then supported compatibility aliases, and finally the active
Mailpit endpoint when the SMTP host is empty. Aspire isolation can change Mailpit ports; follow
service discovery.

Listmonk deployment bootstrap names are:

- `LISTMONK_ENABLED`
- `LISTMONK_INSTANCE_URL`
- `LISTMONK_DEFAULT_LIST_ID`
- `LISTMONK_PRECONFIRM_SUBSCRIPTIONS`
- `LISTMONK_SYNC_ON_REGISTRATION`
- `LISTMONK_API_USERNAME`
- `LISTMONK_API_KEY`

The username/API key are server-side secret bindings. Browser/API reads expose configured status,
not credentials. Listmonk subscriber synchronization is distinct from SMTP dispatch, and Keycloak
realm email remains owned by Keycloak. See [EMAIL_NOTIFICATIONS.md](EMAIL_NOTIFICATIONS.md).

## Secret Provider

`Explore.Secrets` binds `SecretProvider:*`:

- `SecretProvider:Provider` (`None` or implemented `Infisical`)
- `SecretProvider:FailFast`
- `SecretProvider:Infisical:Url`
- `SecretProvider:Infisical:ProjectId`
- `SecretProvider:Infisical:ClientId`
- `SecretProvider:Infisical:ClientSecret`
- `SecretProvider:Infisical:Environment`
- `SecretProvider:Infisical:Paths`

Other enum values are placeholders until source implements them. Their presence is not a support
or operations promise. Local contributor profiles use `None`; external modes may use an approved
provider. See [SECRETS.md](SECRETS.md) for ownership, redaction, and rotation.

## Production Topology Boundary

`compose.production.yaml` is intentionally narrow: validation, PostgreSQL, Keycloak NGO
provisioning, migrations, API, and UI/BFF. Cache, identity, SMTP, Cerbos, analytics/TMS providers,
object storage, telemetry backends, reverse proxy/TLS, and backup automation remain externally
owned unless a future source-backed production artifact explicitly adds them.

Do not copy local AppHost or `docker-compose.yml` fixtures into production merely because they have
working technical defaults. Follow [PRODUCTION_OPERATIONS.md](PRODUCTION_OPERATIONS.md).

## Safe Change Procedure

1. Identify whether the setting is static deployment configuration, a secret, or governance data.
2. Confirm the current source binding and owner before changing a name.
3. Change only the authoritative source; do not create a second fallback authority.
4. Redact secret values from logs, screenshots, diffs, and support artifacts.
5. Use the appropriate local or production runbook to verify readiness and behavior.
6. Roll back the setting at its owning source if readiness or authentication regresses.

## Related

- [GETTING_STARTED.md](GETTING_STARTED.md) — contributor local setup.
- [OPERATIONS.md](OPERATIONS.md) — shared runtime operation and recovery order.
- [SECRETS.md](SECRETS.md) — secret providers, ownership, and rotation.
- [PRODUCTION_OPERATIONS.md](PRODUCTION_OPERATIONS.md) — exact production topology and required names.
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md) — symptom-driven diagnosis.
