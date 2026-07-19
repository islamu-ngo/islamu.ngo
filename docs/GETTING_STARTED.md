ABOUTME: Short runnable onboarding path for contributors to the first-party ISLAMU website.
ABOUTME: Separates local Aspire and Compose workflows from first-party production operations.

# Getting Started

> **Audience:** Contributors | Maintainers | AI agents
> **Status:** Active
> **Owner:** Contributor Experience
> **Last Verified:** 2026-07-18
> **Source Anchors:** `global.json`, `src/Explore.AppHost/AppHost.cs`, `src/Explore.AppHost/Properties/launchSettings.json`, `docker-compose.yml`, `docs/TESTING.md`

Use this page to run the repository locally. Production deployment belongs to
[PRODUCTION_OPERATIONS.md](PRODUCTION_OPERATIONS.md); local provider containers are development
infrastructure, not a supported self-hosting product.

## Prerequisites

| Tool | Version or posture | Purpose |
|---|---|---|
| .NET SDK | `10.0.300` or the compatible version selected by `global.json` | Build and run the solution. |
| Docker with Compose v2 | Current | Run local infrastructure. |
| .NET Aspire CLI | Current compatible CLI | Preferred local orchestration loop. |

Install Aspire when it is not already available:

```bash
curl -sSL https://aspire.dev/install.sh | bash
```

## Clone And Build

```bash
git clone https://github.com/islamu-ngo/islamu.ngo.git
cd islamu.ngo
dotnet build --configuration Release --verbosity quiet
```

Copy `.env.example` to `.env` only when you need to override local defaults. Never commit `.env`.

## Recommended Local Run

The shortest contributor path starts the `DefaultLocal` Aspire graph:

```bash
aspire run --apphost src/Explore.AppHost/Explore.AppHost.csproj
```

The AppHost always starts Mailpit, the migration service, API, and Blazor. It waits for migrations
before application startup and prints the Aspire dashboard URL. Press `Ctrl+C` to stop the run.
Use the dashboard or `aspire describe`; do not assume Aspire endpoints match Compose ports.

### Aspire Profiles

| Launch profile | `ISLAMU_ASPIRE_MODE` | Local ownership |
|---|---|---|
| `local-default` | `DefaultLocal` | PostgreSQL, Redis, RabbitMQ, Mailpit, Keycloak, Cerbos, MinIO, and Svix, plus migrations/API/Blazor. This is the CLI default when the mode is unset. |
| `local-full` | `FullLocal` | `local-default` plus heavy development tools and providers, including Weblate, Coop, Osprey, PgAdmin, Prometheus, and Grafana. |
| `https` | `FullLocal` | IDE compatibility alias for `local-full`. |
| `local-core` | `LocalDataExternalPlatform` | Local PostgreSQL, Redis, and Mailpit; other platform services come from maintainer configuration. |
| `local-lite` | `ExternalInfra` | Mailpit remains local; database, cache, identity, authorization, and other platform services come from maintainer configuration. |

Run an explicit profile through `dotnet run` when an IDE or investigation requires it:

```bash
dotnet run --project src/Explore.AppHost/Explore.AppHost.csproj --launch-profile local-default
dotnet run --project src/Explore.AppHost/Explore.AppHost.csproj --launch-profile local-full
dotnet run --project src/Explore.AppHost/Explore.AppHost.csproj --launch-profile local-core
dotnet run --project src/Explore.AppHost/Explore.AppHost.csproj --launch-profile local-lite
```

Maintainer modes require complete external configuration. The AppHost still runs
`Event.MigrationService`; `local-lite` is not an application-only mode.

## Local Email Capture

Mailpit is local/test infrastructure and starts in every Aspire mode. Non-isolated development uses:

| Endpoint | Address |
|---|---|
| SMTP | `localhost:11025` |
| Capture UI | `http://localhost:18025` |

Isolated Aspire runs can assign dynamic ports. Read them from Aspire resource metadata. Mailpit must
never be documented or configured as the production SMTP provider.

## Local Compose Alternative

Use repository Compose when you need to inspect the container topology without Aspire:

```bash
cp .env.example .env
docker compose config
docker compose up -d postgres redis mailpit keycloak-db keycloak keycloak-ngo-provisioner islamu-ngo-api islamu-ngo-ui
```

Local default endpoints are:

| Service | URL |
|---|---|
| Blazor BFF/UI | `http://localhost:17002` |
| API | `http://localhost:17039` |
| Mailpit | `http://localhost:18025` |

Optional Compose profiles exist for development and provider verification:

```bash
docker compose --profile storage up -d
docker compose --profile authz up -d
docker compose --profile webhooks up -d
docker compose --profile localization up -d
docker compose --profile moderation up -d
docker compose --profile osprey up -d
```

The `localization` profile runs Weblate. Tolgee remains a configurable translation-provider adapter,
but this repository does not start a Tolgee service. PostHog and Listmonk are external configurable
integrations; the local Compose graph does not start either service.

## Configuration And Secrets

- `SecretProvider__Provider=None` is the contributor-local posture; no external secret provider is contacted.
- The shared technical .NET User Secrets ID remains `event-shared-secrets` while project names are being migrated.
- `local-core` and `local-lite` may use environment variables, user secrets, appsettings, or Infisical-backed maintainer configuration.
- `ConnectionStrings:DefaultConnection` wins when Aspire supplies a database reference. Without it, the bootstrap loader resolves discrete `POSTGRESQL_*` values; do not invent a URL-only alias.
- Keep provider credentials and real endpoints out of docs, logs, screenshots, and committed files.

See [CONFIGURATION.md](CONFIGURATION.md) and [SECRETS.md](SECRETS.md) for the exact ownership rules.

## Validation

Run affected test projects individually from the repository root; never run solution-level
`dotnet test`.

```bash
dotnet test --project tests/Event.Architecture.Tests/Event.Architecture.Tests.csproj --configuration Release --verbosity quiet
```

Use [TESTING.md](TESTING.md) for the full project matrix and TUnit filters. For runtime problems,
start with [TROUBLESHOOTING.md](TROUBLESHOOTING.md).

## Related

- [OPERATIONS.md](OPERATIONS.md) — local and runtime operating behavior.
- [PRODUCTION_OPERATIONS.md](PRODUCTION_OPERATIONS.md) — first-party production service chain.
- [CONFIGURATION.md](CONFIGURATION.md) — supported settings and integration boundaries.
- [SECRETS.md](SECRETS.md) — secret ownership, redaction, and rotation.
- [CONTRIBUTING.md](CONTRIBUTING.md) — contribution workflow.
- [TESTING.md](TESTING.md) — verification commands.
