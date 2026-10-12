<!-- ABOUTME: Source-grounded map of the first-party ISLAMU website repository. -->
<!-- ABOUTME: Shows current projects, Blog/navigation ownership, retained integrations, and generated contracts. -->

# Codebase Structure

> **Audience:** Contributors | Maintainers | AI agents
> **Status:** Active
> **Owner:** Architecture
> **Last Verified:** 2026-07-19
> **Source Anchors:** `Explore.slnx`, `src/`, `tests/`, `schemas/openapi_islamu.json`, `docs/API_CONTRACT_INVENTORY.md`, `.github/workflows/openapi-contract.yml`, `dev/report/EVENT_REFERENCE_ALIGNMENT.md`

## Naming Boundary

The product is the first-party ISLAMU website. `Explore.*` and `Event.*` remain in solution,
project, namespace, and test names as legacy source identifiers. Do not infer a live Event product
from those names and do not rename them piecemeal.

## Solution Projects

### Product And Runtime

| Project | Responsibility |
|---|---|
| `src/Explore.Domain` | Entities, enums, invariants, tenant/audit/soft-delete contracts |
| `src/Explore.Application` | CQRS, DTOs, validators, specifications, authorization metadata, contracts |
| `src/Explore.Persistence` | EF Core DbContext, mappings, migrations, repositories |
| `src/Explore.Infrastructure` | External providers, storage, email, federation, webhooks, authorization implementations |
| `src/Explore.API` | ASP.NET Core API, middleware, controllers, HAL, OpenAPI, background workers |
| `src/Explore.Blazor` | Server-side BFF, OIDC, YARP, render host, robots/manifest endpoints |
| `src/Explore.Blazor.Client` | Razor pages, layout, presentation services, MudBlazor UI |
| `src/Explore.Secrets` | Secret-provider abstractions and implementations |
| `src/Explore.AppHost` | Local Aspire orchestration |
| `src/Explore.ServiceDefaults` | Shared health, resilience, telemetry, and management-port defaults |
| `src/Explore.Diagnostic` | Diagnostic and doctor tooling |
| `src/Event.MigrationService` | EF Core migration runner |
| `src/Event.Web.BffHosting` | Shared BFF authentication, proxy, header, and host primitives |

### Tests And Benchmarks

The solution contains project-level TUnit suites under `tests/` for Domain, Application,
Architecture, Persistence, API, Infrastructure, Secrets, Blazor client, Blazor BFF integration,
diagnostics, and browser E2E, plus `Event.Benchmarks`. Run test projects individually; the legacy
`Event.*` prefix is allowed by the Event-removal guardrail.

## Layer Map

```text
Explore.Domain
  <- Explore.Application
       <- Explore.Persistence / Explore.Infrastructure
            <- Explore.API

Event.Web.BffHosting <- Explore.Blazor -> Explore.Blazor.Client
Explore.AppHost composes local runtime projects
```

The API side follows inward-only Clean Architecture. The Blazor projects are isolated from Domain,
Application, Persistence, and Infrastructure and communicate through the BFF/API contract.

## Blog Source Map

| Layer | Current paths |
|---|---|
| Domain | `src/Explore.Domain/Blog.cs`, `BlogCategory.cs`, `BlogTag.cs` |
| Application | `src/Explore.Application/Features/Blogs/`, `DTOs/Blog/`, `Specifications/BlogQuerySpecification.cs` |
| Persistence | `Repositories/BlogRepository.cs`, Blog EF configurations, `20260716130000_AddBlogFoundation*` migration files |
| API | `Controllers/BlogController.cs`, `Hateoas/Assemblers/BlogResourceAssembler.cs`, `Hateoas/Policies/BlogLinkPolicy.cs` |
| Public UI | `src/Explore.Blazor.Client/Pages/Blogs/BlogIndex*`, `BlogArticle*` |
| Editorial UI | `BlogManagement*`, `BlogEditor*` |
| Client services | `Services/Blogs/BlogService.cs`, `BlogMarkdownRenderer.cs` |
| SEO | `src/Explore.API/Controllers/SitemapController.cs` |
| Tests | Blog suites across Domain, Application, Persistence, API, and Blazor test projects |

## Navigation Source Map

| Concern | Current path |
|---|---|
| Main shell | `src/Explore.Blazor.Client/Layout/NavMenu.razor` and `.razor.cs` |
| Public navigation | `Layout/SiteMegaMenu.razor`, `.razor.cs`, `.razor.css` |
| Route registry | `src/Explore.Blazor.Client/Routes.razor` |
| Public menu tests | `tests/Explore.Blazor.Client.Tests/Layout/SiteMegaMenuTests.cs`, `NavMenuMegaMenuTests.cs` |

`SiteMegaMenu` is a static route model, not a dynamic catalog. Its destinations are Home, Blog,
About, Contact, Community Guidelines, Privacy, and Terms.

## API And BFF Areas

- `src/Explore.API/Controllers/`: HTTP endpoints. Blog is the current public content vertical.
- `src/Explore.API/Hateoas/`: route names, assemblers, link policies, and HAL authorization.
- `src/Explore.API/Middleware/`: tenancy, authentication conflict checks, idempotency, ETags,
  correlation, security headers, and request handling.
- `src/Explore.Blazor/Extensions/`: BFF authentication, preferences, setup, storage, support access,
  manifest, and proxy endpoints.
- `src/Explore.Blazor.Client/Services/`: browser-facing service layer over API/BFF calls.

## Retained Non-Blog Chassis

The alignment ledger retains current Organizations, Groups, taxonomy, storage, notifications,
email/outbox, support access, generic webhooks, and federation/PDS infrastructure. Useful source
entry points include:

- `src/Explore.API/Controllers/WebhooksController.cs`
- `src/Explore.API/Controllers/IncomingWebhooksController.cs`
- `src/Explore.Application/Features/Webhooks/`
- `src/Explore.Infrastructure/Webhooks/`
- `src/Explore.Infrastructure/Services/Federation/`
- `src/Explore.Infrastructure/Protos/osprey/`
- `src/Explore.AppHost/AppHost.cs`

Osprey orchestration, federation, and webhook documentation remains source-backed. Product-facing
expansion stays deferred until a named consumer owns the UX and policy.

## Event Removal

Live source no longer contains Event product entities, controllers, queries, services, pages,
template synchronization, contact-sharing, or Event OpenAPI/schema artifacts. The guardrail in
`tests/Event.Architecture.Tests/Phase10EventResidueGuardrailsTests.cs` maintains that boundary.

Generic .NET events such as secret-binding notifications and legacy project names are not Event
product residue.

## Generated Contract Status

The generated contract boundary is checked in:

- `src/Explore.API/Explore.API.csproj` generates `schemas/openapi_islamu.json` in Release;
- the schema and `docs/API_CONTRACT_INVENTORY.md` both report 309 paths and 420 operations;
- `src/Explore.Blazor.Client/nswag.json` configures `IslamuApiClient`; and
- `src/Explore.Blazor.Client/Explore.Blazor.Client.csproj` generates the checked-in
  `src/Explore.Blazor.Client/Clients/IslamuApiClient.g.cs` before compile.

The last owning-lane run before the current unrelated worktree edits was green for generation,
stable invariants, inventory/client regeneration, and drift at these counts. This documentation
update rechecked artifact presence, ownership, and counts only; it does not claim a fresh full
contract-gate result for the final combined worktree. Regenerate these files through their owners;
do not hand-edit them.

Weblate, Tolgee, and Listmonk provider schemas under `schemas/` remain separate from the website API
contract.

## Non-Code Structure

| Path | Purpose |
|---|---|
| `docs/` | Product, architecture, contributor, integration, and operator documentation |
| `dev/report/` | Living alignment evidence |
| `dev/active/` | Active workstream memory |
| `cerbos/` | Authorization policies, schemas, and policy tests |
| `schemas/` | Provider contracts and generated contract targets |
| `docker/` | Local and production-supporting container assets |
| `assets/` | ISLAMU brand/documentation assets |

## Quick Lookup

| Change | Start at |
|---|---|
| Blog domain behavior | `src/Explore.Domain/Blog.cs` |
| Blog command/query | `src/Explore.Application/Features/Blogs/` |
| Blog persistence | `src/Explore.Persistence/Repositories/BlogRepository.cs` |
| Blog API/HAL | `src/Explore.API/Controllers/BlogController.cs` and `Hateoas/` |
| Blog reader/editor | `src/Explore.Blazor.Client/Pages/Blogs/` |
| Public navigation | `src/Explore.Blazor.Client/Layout/SiteMegaMenu*` |
| BFF/auth/proxy | `src/Explore.Blazor/` and `src/Event.Web.BffHosting/` |
| Webhooks | `src/Explore.Application/Features/Webhooks/`, `src/Explore.Infrastructure/Webhooks/` |
| Federation/PDS | `src/Explore.Infrastructure/Services/Federation/` |
| Architecture guardrails | `tests/Event.Architecture.Tests/` |

## Related

- [ARCHITECTURE.md](ARCHITECTURE.md)
- [API.md](API.md)
- [TESTING.md](TESTING.md)
- [NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md)
