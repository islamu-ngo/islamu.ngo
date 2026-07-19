<!-- ABOUTME: Current architecture of the first-party ISLAMU website after Event-product removal. -->
<!-- ABOUTME: Maps the Blog, navigation, BFF, API, persistence, and retained integration boundaries. -->

# Technical Architecture

> **Audience:** Contributors | Maintainers | AI agents
> **Status:** Active
> **Owner:** Architecture
> **Last Verified:** 2026-07-18
> **Source Anchors:** `Explore.slnx`, `src/Explore.Domain/Blog.cs`, `src/Explore.Application/Features/Blogs/`, `src/Explore.Persistence/Repositories/BlogRepository.cs`, `src/Explore.API/Controllers/BlogController.cs`, `src/Explore.Blazor.Client/Pages/Blogs/`, `src/Explore.Blazor.Client/Layout/SiteMegaMenu.razor.cs`

## System Profile

- .NET 10 with ASP.NET Core, Blazor, PostgreSQL/EF Core, MediatR, MudBlazor, and YARP.
- `Explore.API` is the API composition root.
- `Explore.Blazor` is the server-side BFF and render host.
- `Explore.Blazor.Client` owns Razor UI and presentation services.
- `Explore.AppHost` is the local Aspire orchestration project.
- `compose.production.yaml` describes the lean first-party production topology.

`Explore.*` and `Event.*` remain in project names for now. They are repository identifiers, not a
promise that the removed Event product still exists.

## Dependency Direction

```text
Explore.Domain
  <- Explore.Application
       <- Explore.Persistence / Explore.Infrastructure
            <- Explore.API

Event.Web.BffHosting
  <- Explore.Blazor
       -> Explore.Blazor.Client
```

- Domain owns entities and invariants without framework dependencies.
- Application owns CQRS requests/handlers, DTOs, specifications, authorization metadata, and
  repository/service contracts.
- Persistence implements repositories and EF Core mappings.
- Infrastructure implements external providers and cross-cutting services.
- API owns HTTP routing, HAL assembly, OpenAPI metadata, middleware, and dependency composition.
- Blazor consumes API contracts through the BFF boundary. It must not reference Domain,
  Application, Persistence, or Infrastructure directly.

## HTTP And CQRS Flow

1. ASP.NET Core middleware resolves forwarded headers, correlation, tenancy, authentication,
   localization, idempotency, rate limits, authorization, caching, and ETags.
2. A thin controller dispatches a MediatR request.
3. Application authorization and performance behaviors run.
4. The handler manually instantiates request validators, calls repository contracts, and maps
   entities to DTOs.
5. Persistence returns entities, never DTOs.
6. The API assembler creates HAL resources and permission-filtered `_links`.
7. Blazor renders mutation controls only when the corresponding HAL relation exists.

## Implemented Blog Vertical

```text
Blog reader/editor
  -> BlogService
  -> BFF/API client boundary
  -> BlogController
  -> MediatR Blog request/handler
  -> IBlogRepository / BlogRepository
  -> PostgreSQL Blog + BlogCategory + BlogTag
```

The Blog implementation includes:

- `Blog`, `BlogCategory`, and `BlogTag` domain entities;
- CQRS queries and commands under `Explore.Application/Features/Blogs/`;
- tenant-scoped repository/specification and EF configurations;
- anonymous published reads plus authenticated management/write API routes;
- HAL links for self, collection, management, create, edit, publish, unpublish, and delete;
- public `/blog` and `/blog/{slug}` pages;
- authenticated editorial pages under `/blog/manage`; and
- Markdown rendering with raw HTML disabled and sanitized HTTP(S)-only links.

Publishing is server-authoritative. Public queries return only published content; the UI also
defensively rejects missing or future publication timestamps.

## Navigation Architecture

`NavMenu` hosts `SiteMegaMenu` as the public primary navigation. `SiteMegaMenu` owns a static,
typed list of implemented routes:

- Discover: Home, Blog
- About ISLAMU: About, Contact
- Community: Community Guidelines
- Policies: Privacy, Terms

Desktop navigation uses button disclosures and a dismissible panel. Mobile navigation uses a
temporary MudBlazor drawer with native `details` sections. The component observes viewport changes,
closes on route changes and Escape, restores focus, and has no API or dynamic product-catalog
dependency.

Authenticated account, organization, group, tenant, and instance links remain in `NavMenu` and are
separate from the public mega-menu model.

## BFF And Security Boundary

- OIDC tokens and secure session state remain server-side in `Explore.Blazor`.
- Browser API calls pass through YARP/BFF forwarding; browser-supplied trusted headers are stripped
  or replaced by BFF-owned values.
- The API remains the authorization boundary.
- HAL `_links` is the sole authority for per-resource mutation affordances.
- EF Core query filters and tenant context retain isolation for surviving tenant-scoped data.
- API failures use RFC 7807 ProblemDetails.

## SEO And Render Boundary

- `SitemapController` publishes static routes plus currently published Blog article slugs.
- Blog reader pages emit a page title, description, and canonical URL.
- No Blog JSON-LD or alternate-language metadata is implemented.
- `RuntimeRenderPolicyService` currently returns `InteractiveServer` for every route and uses route
  groups only to choose prerender behavior.
- `/blog` routes currently classify as `Operational`, not `PublicSeo`; the Blog pages explicitly use
  `InteractiveServer`.

See [SEO.md](SEO.md) and [RENDER_POLICIES.md](RENDER_POLICIES.md).

## Retained Infrastructure

Source-backed infrastructure still includes Organizations, Groups, notifications, storage,
transactional outboxes, account-authority email, Listmonk integration, webhooks, support access,
ATProto/PDS federation hooks, and Coop/Osprey provider integration. These are technical platform
capabilities. They do not restore Event product routes or create public product promises.

Webhooks keep their catalog, consumer, endpoint, incoming-callback, delivery, retry, and provider
binding layers. Federation keeps current ATProto/PDS callers. Preserve their dedicated docs while
their source remains.

## Generated Contract Boundary

Generation configuration exists:

- API Release build target: `schemas/openapi_islamu.json`
- Blazor NSwag config: `src/Explore.Blazor.Client/nswag.json`
- configured client output: `src/Explore.Blazor.Client/Clients/IslamuApiClient.g.cs`

The schema and generated client files were absent from the inspected worktree on 2026-07-18.
Configuration is not generated evidence; do not describe either output as available until it is
created and verified by the owning lane.

## Event Removal Boundary

The Event product entity/API/UI/policy/schema surface is deleted. The surviving architecture test
`Phase10EventResidueGuardrailsTests` forbids Event entities, DTOs, controllers, routes, generated
client names, and Event schema identities in live product source. Legacy `Event.*` project names and
generic .NET events remain allowed source identifiers.

## Related

- [API.md](API.md)
- [CODEBASE_STRUCTURE.md](CODEBASE_STRUCTURE.md)
- [BLAZOR.md](BLAZOR.md)
- [OUTBOX_PATTERN.md](OUTBOX_PATTERN.md)
- [FEDERATION.md](FEDERATION.md)
- [COOP_INTEGRATION.md](COOP_INTEGRATION.md)
- [OSPREY_INTEGRATION.md](OSPREY_INTEGRATION.md)
- [WEBHOOKS.md](WEBHOOKS.md)
