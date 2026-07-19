ABOUTME: Secret ownership and redaction rules for local and first-party ISLAMU operations.
ABOUTME: Documents provider boundaries, bootstrap credentials, rotation, and safe key names.

# Secrets Management

> **Audience:** First-party operators | Maintainers | AI agents
> **Status:** Active
> **Owner:** Platform/Ops
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.Secrets`, `src/Explore.Domain/Secrets/SecretDefinitionRegistry.cs`, `src/Explore.API/Extensions/ConfigurationExtensions.cs`, `src/Explore.AppHost/AppHost.cs`, `compose.production.yaml`, `.env.example`

Document secret names and ownership, never secret values. Local defaults are disposable development
fixtures; they are not production recommendations or provider commitments.

## Provider Boundary

`Explore.Secrets` implements a provider-neutral runtime abstraction:

| Provider enum | Runtime status | Meaning |
|---|---|---|
| `None` | Implemented | Read environment variables, user secrets, and appsettings; contact no external secret service. |
| `Infisical` | Implemented | Resolve configured Infisical paths with Universal Auth. |
| `Vault` | Not implemented | Enum/config placeholder only. |
| `AzureKeyVault` | Not implemented | Enum/config placeholder only. |
| `AwsSecretsManager` | Not implemented | Enum/config placeholder only. |

An enum value is a technical extension point, not a promise that ISLAMU operates or supports that
provider in production.

Minimal static shape:

```json
{
  "SecretProvider": {
    "Provider": "None",
    "FailFast": true,
    "Infisical": {
      "Url": "",
      "ProjectId": "",
      "ClientId": "",
      "ClientSecret": "",
      "Environment": "dev",
      "Paths": ["/"]
    }
  }
}
```

## Ownership Model

| Ownership | Authority | UI/runtime behavior |
|---|---|---|
| Application-managed | Encrypted application setting | Write-only after save; a deployment value may bootstrap only until an operator saves a replacement. |
| Deployment-managed | Environment, appsettings, or external secret provider | Read-only ownership metadata in UI; rotate at the deployment source and refresh/restart. |
| Deployment bootstrap | Deployment value with no saved application value | May prefill setup; becomes application-managed only after an authorized save. |

Do not merge sources after ownership is selected. A `SecretBinding` resolves from one source; it does
not fall through to another source after selection.

## Local Development

The technical .NET User Secrets ID remains `event-shared-secrets` in current project files. The
contributor-default `DefaultLocal` and `FullLocal` Aspire modes force
`SecretProvider__Provider=None` and clear Infisical bootstrap identifiers for child projects.

Maintainer-only `local-core` and `local-lite` runs can supply equivalent external values through
user secrets, environment variables, appsettings, or Infisical. Do not commit the user-secrets file
or copy its contents into tickets.

Repository `.env` rules:

- `.env.example` documents names and disposable local fixtures.
- `.env` is ignored and is used by Compose/AppHost for local interpolation.
- Compose passes explicit variables to each service; do not broadly import every secret into every container.
- Keep `SECRET_PROVIDER=None` and `INFISICAL_*` blank for local-only authority.

## First-Party Production Secrets

`compose.production.yaml` validates the names below without printing their values:

- `POSTGRESQL_PASSWORD`
- `KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET`
- `KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET`
- `SETUP_SECRET`
- `CACHE_CONNECTION_STRING`

Other required production values include usernames, database/realm names, public/internal URLs, and
redirect/origin lists; treat private URLs and environment topology as operationally sensitive even
when they are not credentials. See [PRODUCTION_OPERATIONS.md](PRODUCTION_OPERATIONS.md) for the full
name-only list.

Keep PostgreSQL bootstrap values discrete:

- `POSTGRESQL_HOST`
- `POSTGRESQL_PORT`
- `POSTGRESQL_DATABASE`
- `POSTGRESQL_USERNAME`
- `POSTGRESQL_PASSWORD`

Do not replace them with a documentation-only connection-string alias; `BootstrapSecretLoader`
owns composition.

## Integration Secret Families

| Integration | Secret-bearing names or settings | Boundary |
|---|---|---|
| Keycloak runtime | `Keycloak:ClientSecret`, `KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET` | BFF/server only; never returned in onboarding reads. |
| Keycloak provisioning | `KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET` or request-scoped admin credential | Provisioner/request only; never persist the temporary administrator credential. |
| SMTP | `MAIL_SMTP_USERNAME`, `MAIL_SMTP_PASSWORD`, `email.smtp_username`, `email.smtp_password` | Server-side dispatch only. Mailpit local defaults require no real credentials. |
| Listmonk | `LISTMONK_API_USERNAME`, `LISTMONK_API_KEY`, `integrations.listmonk.api_username`, `integrations.listmonk.api_key` | Optional external integration; browser reads expose configured flags only. |
| Analytics/PostHog | `analytics.api_key`, `analytics.personal_api_key` | `personal_api_key` is sensitive; client exposure is limited to the public key required by the selected transport. |
| Localization | `localization.tms_api_key`, Weblate/Tolgee provider credentials | Server-side provider binding only; never include translation content or tokens in diagnostics. |
| Cerbos | `Cerbos:AdminApi:AdminUsername`, `Cerbos:AdminApi:AdminPassword`, deployment equivalents | Policy publishing only; browser sees status, not credentials. |
| Optional S3-compatible storage | `S3Settings:AccessKeyId`, `S3Settings:SecretAccessKey`, `STORAGE_S3_ACCESS_KEY_ID`, `STORAGE_S3_SECRET_ACCESS_KEY` | Server/provider only. Local filesystem mode needs neither. |

PostHog, Listmonk, Tolgee, and external Weblate values configure source-backed adapters. They do not
mean the repository bundles those services for production or promises third-party hosting support.

## Setup And Provider Credentials

The `/setup` route is a pre-authentication operator gateway. The BFF strips browser-supplied setup,
tenant, authorization, and provider-administration headers and forwards only trusted server-owned
state. The API validates authority again.

Never persist or return:

- access, refresh, provider, or administrator tokens;
- the setup secret or a recoverable derivative;
- temporary provider administrator usernames/passwords or service-account credentials;
- raw provider request/response bodies.

External Keycloak doctor, preview, apply, and rotation operations use temporary credentials only for
the active request. Additive repair owns only ISLAMU clients/scopes/mappers; destructive realm,
user, group, or unrelated-client changes require separate explicit approval and verified backup.

## Refresh, Health, And Audit

`SecretRefresh` controls polling, exponential backoff, jitter, and unhealthy thresholds. The
`secret_provider` readiness check reports provider type, refresh support, bounded failure count, and
last successful refresh time; it must never report keys or values.

`Explore.Secrets` metrics report refresh attempts, failures, duration, consecutive failures, and
last successful refresh timestamp. The audit decorator redacts key patterns containing password,
secret, key, token, credential, connection string, or API key.

## Rotation Rules

1. Rotate deployment-managed values in the owning environment/secret provider.
2. Rotate application-managed values through the authorized write-only operation.
3. Keep old and new credentials distinct and overlap only when the provider requires it.
4. Refresh or restart affected processes according to provider behavior.
5. Verify configured/readiness metadata and authentication or provider flow; never verify by reading the secret back.
6. Revoke temporary provider administrator credentials immediately after provisioning or repair.

Never paste values into documentation, shell transcripts, logs, traces, screenshots, issue bodies,
chat, or support bundles.

## Related

- [CONFIGURATION.md](CONFIGURATION.md) — non-secret settings and precedence.
- [OPERATIONS.md](OPERATIONS.md) — runtime health and recovery boundaries.
- [PRODUCTION_OPERATIONS.md](PRODUCTION_OPERATIONS.md) — production-required names and rotation posture.
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md) — safe recovery without secret disclosure.
