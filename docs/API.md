<!-- ABOUTME: Current API contract and authoring rules for the first-party ISLAMU website. -->
<!-- ABOUTME: Documents Blog endpoints, HAL authority, retained integrations, and generation status without stale Event claims. -->

# API Architecture

> **Audience:** Contributors | Integrators | AI agents
> **Status:** Active
> **Owner:** API
> **Last Verified:** 2026-07-19
> **Source Anchors:** `src/Explore.API/Program.cs`, `src/Explore.API/Controllers/BlogController.cs`, `src/Explore.API/Controllers/SitemapController.cs`, `src/Explore.API/Hateoas/Policies/BlogLinkPolicy.cs`, `src/Explore.Application/Features/Blogs/`, `src/Explore.Blazor.Client/nswag.json`

## Scope

`Explore.API` is the HTTP boundary for the first-party ISLAMU website. Controllers remain thin:
they classify/authenticate the endpoint, dispatch MediatR requests, map command failures to
ProblemDetails, and assemble HAL responses.

The Event product API is deleted. Legacy `Event.*` test/project names are not API capabilities.

## Development Contract Surfaces

In Development, Testing, and OpenAPI-generation hosts:

- native OpenAPI: `/openapi/v1.json`
- Swagger UI: `/swagger`
- Scalar reference: mapped by `MapScalarApiReference()`

Production does not map those development contract surfaces. Operational `/alive`, `/health`, and
management-port metrics are not controller operations.

## Request Pipeline

The exact order lives in `Explore.API/Program.cs`. At a high level it applies:

1. exception handling, forwarded headers, security headers, correlation, logging, and compression;
2. routing, pre-auth tenant resolution, request timeouts, auth conflict checks, authentication, and
   post-auth tenant binding;
3. localization, idempotency, rate limiting, authorization, support-access audit, output cache, and
   ETags; and
4. controller, health, MCP, and other mapped endpoints.

Do not reorder middleware from documentation alone. Read `Program.cs` and the relevant middleware
before changing the pipeline.

## Versioning

API version `0.1` is assumed when omitted and can be supplied through:

- media type, for example `Accept: application/hal+json;v=0.1`;
- `?api-version=0.1`; or
- `X-Api-Version: 0.1`.

URL-segment versions such as `/api/v0.1/...` are not supported. Named routes in `RouteNames` remain
the basis for stable HAL URLs.

## Blog API

`BlogController` is the implemented public/editorial content API at `/api/blog`.

### Public Reads

| Method | Route | Behavior |
|---|---|---|
| `GET` | `/api/blog` | Paginated published posts; optional `categoryId`, `tagId`, and `search` filters |
| `GET` | `/api/blog/{slug}` | One currently published post by slug |

Both routes are anonymous, public-classified, cached, tenant-scoped, and returned as HAL resources.

### Editorial Management

| Method | Route | Behavior |
|---|---|---|
| `GET` | `/api/blog/manage` | Authenticated management collection with optional title search |
| `GET` | `/api/blog/manage/{id}` | Authenticated management detail by `Guid` |
| `POST` | `/api/blog` | Create a draft |
| `PATCH` | `/api/blog/{id}` | Update an existing post |
| `POST` | `/api/blog/{id}/publish` | Publish a draft |
| `POST` | `/api/blog/{id}/unpublish` | Return a published post to draft |
| `DELETE` | `/api/blog/{id}` | Soft-delete a post |

Management and mutation routes require authentication. Application authorization metadata and the
Blog HAL policies decide which create/edit/publish/unpublish/delete relations are emitted.

## Blog Data And Response Rules

- `BlogSummaryDto` is used for collections and `BlogDetailDto` for detail/editor data.
- Create/update handlers validate title, slug/content, taxonomy IDs, and thumbnail references.
- Repositories return `Blog` entities; Application maps them to DTOs.
- Public queries return only currently published content.
- Category and tag relationships are explicit `BlogCategory` and `BlogTag` entities.
- Pagination uses `PaginatedResult<T>` and HAL collection links.
- Command failures use structured validation ProblemDetails; missing reads return `404`.

## HAL Is The UI Authority

Blog detail/item policies can emit these relations when allowed:

- `self`
- `collection`
- `management`
- `edit`
- `delete`
- `publish`
- `unpublish`

The management collection can emit `create`. Blazor renders editorial controls only when the
server returned the matching relation. Roles or claims must not recreate per-resource decisions.

The same rule applies to all surviving HAL resources: action affordances flow through `_links`.

## Sitemap API

`GET /sitemap.xml` is anonymous XML output. It contains the implemented static public route catalog
plus one `/blog/{slug}` entry for each current published Blog post. It uses `IPublicUrlBuilder` for
the request-aware base URL and output caching via `SitemapData`.

## Retained Integration APIs

Event-product removal did not remove every platform integration. Current source still contains:

- webhook consumer, endpoint, message, delivery-attempt, provider-binding, test, retry, and incoming
  redrive APIs;
- the incoming Svix operational callback;
- Osprey coordinator protobuf/client and local orchestration configuration without a public callback route;
- ATProto record and PDS synchronization APIs/infrastructure; and
- Organizations, Groups, notifications, storage, email, support-access, and provider administration.

Their dedicated documentation remains authoritative only while its named source exists:

- [WEBHOOKS.md](WEBHOOKS.md)
- [FEDERATION.md](FEDERATION.md)
- [OSPREY_INTEGRATION.md](OSPREY_INTEGRATION.md)

Do not describe these technical surfaces as public ISLAMU products without a separate approved
product workstream.

## Authentication And Authorization

- Public safe reads use `[AllowAnonymous]` and public endpoint classification.
- Writes and privileged reads use `[Authorize]` with authenticated/admin classification.
- JWT/OIDC audiences use current ISLAMU identifiers (`islamu-ngo-api`, `islamu-ngo-blazor`).
- API keys remain available for approved non-interactive callers.
- MediatR authorization evaluates resource/action metadata through the selected local or Cerbos
  provider and fails closed.
- User ID extraction falls back `sub` → `nameidentifier` → `sid`.
- Tenant binding and EF query filters preserve tenant isolation for surviving resources.

## Reliability And Errors

- `Idempotency-Key` supports safe retries for eligible writes.
- RFC 7807 ProblemDetails is the error contract.
- Output caching, HybridCache, and weak ETags remain separate layers.
- Transactional outboxes own durable asynchronous side effects; handlers/controllers do not send
  SMTP or publish external messages inside database transactions.
- Logs, metrics, traces, and ProblemDetails must not expose tokens, secrets, raw webhook payloads,
  email bodies, or tenant/user PII.

## OpenAPI And Client Generation Status

The generation pipeline is configured but its outputs are not currently present:

| Artifact | Configured owner | Inspection state on 2026-07-18 |
|---|---|---|
| `schemas/openapi_islamu.json` | Release build of `src/Explore.API/Explore.API.csproj` | Absent |
| `src/Explore.Blazor.Client/Clients/IslamuApiClient.g.cs` | NSwag target in `Explore.Blazor.Client.csproj` | Absent |

`src/Explore.Blazor.Client/nswag.json` exists and targets `IIslamuApiClient`/`IslamuApiClient`, but a
configuration file is not a generated contract. Do not hand-create or hand-edit either output.
Build the API first, then run the client generation target in the owning code/generation lane.

The legacy `openapi_islamu-event.json`, `islamu-event.md`, and `EventApiClient.g.cs` identities are
forbidden by `Phase10EventResidueGuardrailsTests`.

## Controller Authoring Rules

Every new action must have:

1. an explicit verb/route template;
2. a stable named route;
3. an `EndpointClassification`;
4. explicit success and error response types;
5. the correct anonymous/authenticated boundary; and
6. HAL policy updates when the resource gains or loses an affordance.

Generated contract changes require regeneration from source. Never patch generated schemas or
client output by hand.

## Related

- [ARCHITECTURE.md](ARCHITECTURE.md)
- [CODEBASE_STRUCTURE.md](CODEBASE_STRUCTURE.md)
- [SEO.md](SEO.md)
- [OUTBOX_PATTERN.md](OUTBOX_PATTERN.md)
- [AUTHORIZATION.md](AUTHORIZATION.md)
