<!-- ABOUTME: Engineering and documentation governance for the first-party ISLAMU website. -->
<!-- ABOUTME: Defines current architecture, API, HAL, naming, generated-contract, and review rules. -->

# Code Conventions And Governance

> **Audience:** Contributors | Maintainers | AI agents
> **Status:** Active
> **Owner:** Architecture
> **Last Verified:** 2026-07-18
> **Source Anchors:** `AGENTS.md`, `docs/PROJECT.md`, `docs/QUICK_REFERENCE.md`, `Explore.slnx`, `dev/report/EVENT_REFERENCE_ALIGNMENT.md`

## Authority

When rules conflict, use this order:

1. `AGENTS.md` critical rules
2. `docs/QUICK_REFERENCE.md`
3. this document
4. matching `.claude/rules/*.md`
5. task-specific skills and examples

The repository targets one first-party ISLAMU website. Event product source is deleted. Legacy
`Explore.*` and `Event.*` project names remain source identifiers and are not compatibility or
product requirements.

## Critical Rules

| Rule | Required behavior |
|---|---|
| Repository boundary | Repositories return entities; Application handlers map DTOs |
| Validation | Validators are manually instantiated, not injected as `IValidator<T>` |
| IDs | `Guid` for aggregates, `int` for most lookups, `long` for cursors/sizes |
| Writes | Link/junction writes go through repositories and transactions |
| Auth | Safe public reads may be anonymous; writes require authorization |
| HAL | UI mutation affordances come from `_links`, never local role/claim checks |
| Tenancy | Tenant filters/binding fail closed; do not bypass query filters casually |
| Errors | API errors use RFC 7807 ProblemDetails |
| Source files | Every file begins with two ABOUTME lines |
| Generated output | Never hand-edit generated schemas or API clients |

## Product Truth

- Blog and public navigation are implemented source-backed capabilities.
- Event entities, controllers, UI, template sync, contact sharing, and Event schema/client artifacts
  remain deleted.
- Webhooks, federation/PDS, Coop, and Osprey documentation remains because current source exists.
- Planned email engagement, catalogs, and other expansion are not public routes or retention
  justifications.
- Public/product docs distinguish source presence from executed runtime acceptance.

The living alignment authority is
[`dev/report/EVENT_REFERENCE_ALIGNMENT.md`](../dev/report/EVENT_REFERENCE_ALIGNMENT.md).

## Solution And Layer Ownership

| Layer/project | Owns | Must not own |
|---|---|---|
| `Explore.Domain` | Entities and invariants | EF Core, MediatR, ASP.NET, Blazor |
| `Explore.Application` | CQRS, DTOs, validators, specs, contracts, authorization metadata | DbContext or HTTP composition |
| `Explore.Persistence` | EF Core mappings, migrations, repository implementations | DTO mapping or presentation logic |
| `Explore.Infrastructure` | External-provider/service implementations | API/UI types |
| `Explore.API` | HTTP, HAL, OpenAPI metadata, middleware, DI composition | Business/persistence logic in controllers |
| `Explore.Blazor` | BFF auth/session/proxy/render host | Domain persistence or browser token storage |
| `Explore.Blazor.Client` | Razor UI and presentation services | Backend authority or Domain/Application references |

Dependency direction is inward. The Blazor boundary is the BFF/API contract, not project references
to backend implementation layers.

## CQRS And Persistence

- Commands describe writes; queries describe reads.
- Handlers pass `CancellationToken` through all async work.
- Multi-step writes use the existing unit-of-work/transaction pattern.
- Validation and read-only authorization/prefetch happen before retryable transaction lambdas.
- External I/O, cache invalidation, metrics, and notifications occur after commit.
- Repositories do not return `IQueryable` or DTOs to handlers.
- Specifications live in Application; EF applies them in Persistence.
- Soft delete and tenant filters remain central persistence policies.

The implemented Blog vertical is the preferred current example: `Blog` Domain entity,
`Features/Blogs` CQRS, `IBlogRepository`/`BlogRepository`, `BlogController`, HAL policies, and Blog
Razor pages.

## Naming

Use PascalCase for types/public members, camelCase for locals/parameters, `_camelCase` for private
fields, `I` for interfaces, and `Async` for asynchronous methods.

New examples and templates use current entities such as Blog and Organization. Use `Event` only
for an existing legacy project identifier, generic .NET event, or clearly historical record. See
[NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md).

## API Contract Rules

Every controller action needs:

1. an explicit verb/route template;
2. a stable named route;
3. an `EndpointClassification`;
4. explicit response types;
5. the matching anonymous/authenticated boundary; and
6. HAL policy updates for new/removed affordances.

API versioning uses media type, query string, or `X-Api-Version`; URL-segment versions are not
supported. Controllers remain thin and dispatch MediatR.

### Blog Contract

Public `/api/blog` reads are anonymous. Management and lifecycle writes are authenticated.
Blog create/edit/publish/unpublish/delete controls render only from HAL relations returned by the
API.

### Retained Integrations

Webhook APIs plus federation/PDS, Coop, and Osprey integration surfaces remain technical platform
capabilities. Changes must preserve secret/payload redaction, idempotency, tenant isolation,
retry/dead-letter behavior, and HAL authorization. They must not be rebranded as public products
without product approval.

## OpenAPI And Generated Client Governance

Current configuration targets:

- runtime development/testing document `/openapi/v1.json`;
- checked-in schema `schemas/openapi_islamu.json`; and
- NSwag output `src/Explore.Blazor.Client/Clients/IslamuApiClient.g.cs`.

The checked-in schema and generated client were absent during the 2026-07-18 inspection. Do not
claim they exist and do not create manual stand-ins. The owning code/generation lane must build the
API, generate the client, review the diff, and run contract checks.

The legacy `openapi_islamu-event.json`, `islamu-event.md`, and `EventApiClient.g.cs` identities are
forbidden by the Phase 10 residue guardrail.

## Blazor And Navigation

- Browser tokens remain behind the BFF.
- Components call presentation services, not backend controllers directly.
- Per-resource actions use HAL `_links`.
- `SiteMegaMenu` is a static typed public navigation model with implemented destinations only.
- New navigation links require an existing route, accessible name/keyboard behavior, indexing
  decision, and content owner.
- Use logical CSS properties and preserve RTL, reduced motion, focus, touch targets, and 200% zoom.

## Reliability, Security, And Privacy

- Durable side effects use transactional outboxes and idempotent consumers.
- Controllers/handlers do not perform external delivery inside database transactions.
- Secret values, tokens, raw webhook bodies, email bodies, provider responses, and PII stay out of
  logs, metrics, traces, ProblemDetails, screenshots, and documentation.
- Cerbos/local authorization parity must remain fail-closed.
- Public analytics/email behavior requires the approved consent and privacy boundary.
- Generated or public legal/privacy claims require accountable review; repository evidence alone
  does not authorize commitments.

## Documentation Governance

Canonical docs use:

```markdown
> **Audience:** ...
> **Status:** Active | Draft | Deprecated | Historical
> **Owner:** ...
> **Last Verified:** YYYY-MM-DD
> **Source Anchors:** `path`, `path::Symbol`
```

- Implemented means source/config/policy and an owned destination exist.
- Planned is not clickable or described as available.
- Historical is not current guidance.
- Before deletion, search Markdown links, source anchors, workflows, tests, active dev docs, and
  generated ownership; repair active references in the same slice.
- Documentation and intent metadata are authoring contracts reviewed with compatible schema tools,
  repository searches, link/path checks, and diff review. Executable architecture tests must not
  parse Markdown as a prose gate.

## Contribution Routing

Every change starts by classifying `.claude/contract/intents.yaml`. Load its must-read docs, rules,
skills, permitted paths, minimum tests, and forbidden actions. When no intent matches, use an
explicit fallback contract rather than silently widening scope.

Path-scoped rules remain under `.claude/rules/`; pattern guidance remains under `.agents/skills/`.

## Review Checklist

- Does the change describe the first-party website rather than the removed Event product?
- Are Blog/navigation claims tied to current source and runtime claims tied to executed evidence?
- Are layer and repository boundaries intact?
- Are writes authorized and UI actions HAL-gated?
- Are tenant, secret, privacy, retry, and idempotency boundaries preserved?
- Are generated outputs produced by the generator and actually present?
- Are docs, links, metadata, and source anchors current?
- Were only owning test projects run, with no weakened tests?

## Related

- [PROJECT.md](PROJECT.md)
- [ARCHITECTURE.md](ARCHITECTURE.md)
- [API.md](API.md)
- [QUICK_REFERENCE.md](QUICK_REFERENCE.md)
- [NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md)
- [TESTING.md](TESTING.md)
