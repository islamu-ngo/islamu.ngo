<!-- ABOUTME: Current test strategy and project map for the first-party ISLAMU website. -->
<!-- ABOUTME: Covers TUnit commands, runtime lanes, generated contracts, residue guardrails, and docs verification. -->

# Testing

> **Audience:** Contributors | Maintainers | AI agents
> **Status:** Active
> **Owner:** Quality
> **Last Verified:** 2026-07-19
> **Source Anchors:** `Explore.slnx`, `tests/`, `.github/workflows/_build-test.yml`, `.github/workflows/openapi-contract.yml`, `src/Explore.API/Explore.API.csproj`, `src/Explore.Blazor.Client/Explore.Blazor.Client.csproj`, `schemas/openapi_islamu.json`, `docs/API_CONTRACT_INVENTORY.md`, `tests/Explore.Infrastructure.Tests/Infrastructure/Webhooks/SvixVersionConformanceTests.cs`, `tests/Explore.Infrastructure.Tests/Infrastructure/Webhooks/SvixManagedConformanceTests.cs`, `tests/Event.Architecture.Tests/Phase10EventResidueGuardrailsTests.cs`

## Framework And Command Rules

All .NET test projects use TUnit. Run projects individually from the repository root:

```bash
dotnet test --project tests/<Project>/<Project>.csproj --configuration Release --verbosity quiet
```

Do not use solution-level `dotnet test`. Use TUnit `--treenode-filter` only when the exact filter has
already been validated for that suite; VSTest `--filter` examples are not portable here.

## Test Projects

| Project | Primary responsibility |
|---|---|
| `Event.Domain.UnitTests` | Domain entities and invariants, including Blog lifecycle |
| `Event.Application.UnitTests` | CQRS handlers, validators, authorization metadata, specifications |
| `Event.Architecture.Tests` | Layer, naming, authorization, accessibility, runtime, and residue guardrails |
| `Event.Persistence.IntegrationTests` | PostgreSQL repositories, migrations, query filters, constraints |
| `Event.API.IntegrationTests` | HTTP, HAL, auth, ProblemDetails, OpenAPI, sitemap, provider contracts |
| `Explore.Infrastructure.Tests` | External-provider, authorization, storage, email, and outbox behavior |
| `Explore.Secrets.UnitTests` | Secret-provider and secret-service behavior |
| `Explore.Blazor.Client.Tests` | bUnit components, services, routes, Blog, navigation, accessibility |
| `Explore.Blazor.IntegrationTests` | BFF auth/proxy/header/session behavior |
| `Explore.Blazor.Client.E2ETests` | Browser journeys with runtime infrastructure |
| `Explore.Diagnostic.UnitTests` | Diagnostic, doctor, and inventory behavior |

`Event.Benchmarks` is a BenchmarkDotNet project, not a test project. The `Event.*` prefix is a legacy
project identifier and is explicitly allowed by the Event-removal guardrail.

## Canonical Project Commands

```bash
dotnet test --project tests/Event.Domain.UnitTests/Event.Domain.UnitTests.csproj --configuration Release --verbosity quiet
dotnet test --project tests/Event.Application.UnitTests/Event.Application.UnitTests.csproj --configuration Release --verbosity quiet
dotnet test --project tests/Event.Architecture.Tests/Event.Architecture.Tests.csproj --configuration Release --verbosity quiet
dotnet test --project tests/Event.Persistence.IntegrationTests/Event.Persistence.IntegrationTests.csproj --configuration Release --verbosity quiet
dotnet test --project tests/Event.API.IntegrationTests/Event.API.IntegrationTests.csproj --configuration Release --verbosity quiet
dotnet test --project tests/Explore.Infrastructure.Tests/Explore.Infrastructure.Tests.csproj --configuration Release --verbosity quiet -- --treenode-filter "/*/*/*/*[Category!=Runtime]" --minimum-expected-tests 1
dotnet test --project tests/Explore.Secrets.UnitTests/Explore.Secrets.UnitTests.csproj --configuration Release --verbosity quiet
dotnet test --project tests/Explore.Blazor.Client.Tests/Explore.Blazor.Client.Tests.csproj --configuration Release --verbosity quiet
dotnet test --project tests/Explore.Blazor.IntegrationTests/Explore.Blazor.IntegrationTests.csproj --configuration Release --verbosity quiet
dotnet test --project tests/Explore.Diagnostic.UnitTests/Explore.Diagnostic.UnitTests.csproj --configuration Release --verbosity quiet
```

Browser E2E is a separate/manual or scheduled lane because it starts the application topology.

## Infrastructure Runtime Lanes

The standard Infrastructure command excludes `Runtime` tests so the required local and CI lane is
deterministic and does not silently depend on Docker, external credentials, or managed-provider
state. Runtime evidence is additive and must use an explicit, non-empty selector.

Run the pinned self-hosted Svix 1.96.1 conformance matrix with Testcontainers:

```bash
dotnet test --project tests/Explore.Infrastructure.Tests/Explore.Infrastructure.Tests.csproj --configuration Release --verbosity quiet -- --treenode-filter "/*/*/SvixVersionConformanceTests/*" --minimum-expected-tests 2
```

Managed Svix conformance is a protected manual process, not part of the standard or self-hosted
runtime lanes. It creates provider resources and preserves one expiry probe across the documented
12-hour idempotency window. Run its phases as separate invocations; running both tests together
cannot satisfy that elapsed-time boundary.

Phase 1 requires `SVIX_CONFORMANCE_MANAGED_AUTH_TOKEN_CURRENT`,
`SVIX_CONFORMANCE_MANAGED_AUTH_TOKEN_ROTATED`,
`SVIX_CONFORMANCE_MANAGED_CREDENTIAL_VERSION`, and an absolute JSON path outside source control in
`SVIX_CONFORMANCE_MANAGED_PROBE_PATH`. `SVIX_CONFORMANCE_MANAGED_BASE_URL` is optional and defaults
to the managed Svix API.

```bash
dotnet test --project tests/Explore.Infrastructure.Tests/Explore.Infrastructure.Tests.csproj --configuration Release --verbosity quiet -- --treenode-filter "/*/*/SvixManagedConformanceTests/ManagedProfile_SeedsExpiryProbeAfterImmediateMatrix" --minimum-expected-tests 1
```

After the probe's `VerifyNotBeforeUtc` has elapsed, run Phase 2 with the current token, the same
credential version, and the same protected probe path. The rotated token is not required for this
phase.

```bash
dotnet test --project tests/Explore.Infrastructure.Tests/Explore.Infrastructure.Tests.csproj --configuration Release --verbosity quiet -- --treenode-filter "/*/*/SvixManagedConformanceTests/ManagedProfile_VerifiesExpiredProbeAndCleansUp" --minimum-expected-tests 1
```

Missing managed credentials or probe state must fail loudly; do not convert those failures into
skips because that would allow a selected conformance lane to report success without evidence.

## Blog Coverage

The implemented Blog has source test coverage in every owned layer:

| Layer | Current tests |
|---|---|
| Domain | `tests/Event.Domain.UnitTests/Entities/BlogTests.cs` |
| Application | `tests/Event.Application.UnitTests/Features/Blogs/` and `Specifications/BlogQuerySpecificationTests.cs` |
| Persistence | `Repositories/BlogRepositoryTests.cs`, `Migrations/AddBlogFoundationMigrationTests.cs` |
| API/HAL/SEO | `BlogControllerTests.cs`, `Hateoas/BlogHateoasTests.cs`, `SitemapControllerTests.cs` |
| Blazor | `Pages/Blogs/`, `Services/Blogs/BlogServiceTests.cs`, `BlogMarkdownRendererTests.cs` |

The coverage shape includes lifecycle invariants, validators, CQRS handlers, repository behavior,
public/editor API boundaries, HAL affordances, sitemap inclusion, public/editor pages, generated
client service adaptation, and Markdown sanitization.

Source-file presence is not a pass result. Documentation-only changes use the proportional docs
checks below; implementation claims require current output from the owning test projects.

## Navigation Coverage

Public navigation coverage lives in:

- `tests/Explore.Blazor.Client.Tests/Layout/SiteMegaMenuTests.cs`
- `tests/Explore.Blazor.Client.Tests/Layout/NavMenuMegaMenuTests.cs`
- `tests/Explore.Blazor.Client.Tests/Layout/NavMenuAdminTests.cs`

These suites are the source-owned checks for desktop/mobile disclosure behavior, implemented route
links, focus/close behavior, and coexistence with authenticated navigation.

Manual navigation acceptance should also cover keyboard-only use, Escape dismissal, focus return,
mobile drawer behavior, RTL, 200% zoom, and reduced motion.

## Event-Removal Guardrail

`Phase10EventResidueGuardrailsTests` scans live source, test source, Cerbos, schema/config, and
OpenAPI contract sources. It forbids removed Event entities, DTOs, controllers, UI routes,
resources, and legacy schema/generated-client identities while allowing legacy project names and
generic .NET event types.

Do not weaken or delete this guardrail to make stale source compile.

## API Contract Generation Checks

The governed generated contract set is checked in:

| Artifact | Owning generation step | Current inspected state |
|---|---|---|
| `schemas/openapi_islamu.json` | Release build of `src/Explore.API/Explore.API.csproj` | Present; 309 paths and 420 operations |
| `docs/API_CONTRACT_INVENTORY.md` | `ApiContractInventory_Generate_WritesMarkdownToDocs` | Present; matches 309 paths and 420 operations |
| `src/Explore.Blazor.Client/Clients/IslamuApiClient.g.cs` | `GenerateIslamuApiClient` in `Explore.Blazor.Client.csproj` | Present canonical NSwag client |

`OpenAPI Contract Guard` regenerates the schema, runs stable OpenAPI invariants, regenerates the
inventory and NSwag client, and fails when any governed artifact drifts. Generated artifacts must
not be hand-edited.

The last owning-lane run before the current unrelated worktree edits was green for generation,
stable invariants, inventory/client regeneration, and drift at these counts. This documentation
update rechecked artifact presence, ownership, and counts only; it does not claim a fresh full
contract-gate result for the final combined worktree.

## Architecture Expectations

Architecture tests enforce executable contracts such as:

- inward-only Clean Architecture dependencies;
- repositories returning entities;
- CQRS/naming conventions;
- endpoint classification and API contract invariants;
- authorization/Cerbos parity;
- HAL affordance rules;
- accessibility structure; and
- Event-product residue prevention.

Markdown and agent-context prose are reviewed with repository searches, link/path checks, metadata
inspection, compatible schema tooling, and diff checks. Do not reintroduce architecture tests that
parse Markdown merely to enforce prose formatting.

## Test Authoring Rules

- Add the smallest test that proves the changed behavior.
- Never remove or weaken a failing test to get green.
- Keep tenant/security paths fail-closed and test unauthorized/forbidden outcomes.
- Test HAL link presence/absence, then make Blazor tests render controls from those links.
- Use real PostgreSQL fixtures for EF behavior that in-memory providers cannot prove.
- Keep raw secrets, tokens, webhook payloads, email bodies, and PII out of test output.
- Use deterministic IDs/time when the behavior depends on them.
- Every `[Skip]` needs an owner/reason and removal condition according to the existing governance
  checks.

## Proportional Verification

| Change | Minimum evidence |
|---|---|
| Domain/Application logic | Owning unit project + architecture project |
| Persistence/migration | Persistence integration project + architecture project |
| API/HAL | API integration project + architecture project |
| Blazor service/component | Blazor client project; add BFF integration when transport/auth changes |
| Blog end-to-end change | Blog-owning layer projects plus manual reader/editor journey |
| Navigation change | Blazor client project plus keyboard/mobile/RTL/reduced-motion QA |
| Docs-only Markdown | Structure, metadata, links/paths, source alignment, residual searches, `git diff --check` |

## Related

- [ARCHITECTURE.md](ARCHITECTURE.md)
- [API.md](API.md)
- [ACCESSIBILITY.md](ACCESSIBILITY.md)
- [GOVERNANCE.md](GOVERNANCE.md)
- [OPERATIONS.md](OPERATIONS.md)
