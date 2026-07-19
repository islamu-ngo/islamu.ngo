<!-- ABOUTME: Contributor guide for the first-party ISLAMU Blazor BFF and component boundaries. -->
<!-- ABOUTME: Grounds rendering, token handling, HAL affordances, services, and client generation in current source. -->

# Blazor Frontend Architecture

> **Audience:** Contributors | Frontend | AI agents
> **Status:** Active
> **Owner:** Frontend
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.Blazor/Program.cs`, `src/Explore.Blazor/Extensions/YarpProxyExtensions.cs`, `src/Explore.Blazor/Extensions/HttpClientExtensions.cs`, `src/Explore.Blazor.Client/Explore.Blazor.Client.csproj`, `src/Explore.Blazor.Client/Services/RuntimeRenderPolicyService.cs`, `src/Explore.Blazor.Client/Services/Http/BffAntiforgeryMessageHandler.cs`, `src/Explore.Blazor.Client/Services/`, `src/Explore.Blazor.Client/Components/`

## Scope

Explore.Blazor is the server/BFF host for the first-party ISLAMU website. Explore.Blazor.Client contains the shared Razor UI, components, client-side contracts, and typed service adapters used by that host.

| Project | Owns | Must not own |
|---|---|---|
| Explore.Blazor | OIDC cookie session, server endpoints, token forwarding, YARP /api proxy, antiforgery boundary, server rendering. | Domain logic, browser token persistence, or duplicated API authorization. |
| Explore.Blazor.Client | Razor pages/components, presentation state, generated-client adapters, HAL-aware UI logic. | Bearer tokens, controller logic, provider secrets, or independent resource authorization. |

The Event.* project names in shared BFF hosting remain technical source identifiers. They do not describe the active product.

## Rendering Truth

Program.cs registers Interactive Server components and maps InteractiveServerRenderMode. It does not register WebAssembly or Interactive Auto rendering.

RuntimeRenderPolicyService always returns InteractiveServer. Its route groups and settings currently govern prerender behavior, not a switch to WebAssembly:

- public/SEO routes may prerender;
- operational and admin routes may disable prerender where session-bound behavior requires it;
- all interactive execution remains server-side.

Do not document authentication-state serialization to a browser WebAssembly runtime; the required registration and policy are not present in current source.

See [RENDER_POLICIES.md](RENDER_POLICIES.md) for the current route-group contract.

## BFF Request Flow

~~~text
browser component
  -> same-origin Explore.Blazor endpoint or typed service
  -> BFF cookie/antiforgery boundary
  -> server-side token acquisition
  -> YARP or generated client call
  -> Explore.API
  -> authorized HAL response
~~~

The browser does not own an access token. Explore.Blazor keeps the OIDC session in an HttpOnly cookie and the shared BFF hosting path forwards the server-held access token.

BffAntiforgeryMessageHandler is the current unsafe-request browser handler. It supplies the antiforgery header required by the BFF boundary.

Interactive Server components also use server-to-server typed clients for owned BFF helpers such as storage, support access, setup secrets, and manifest operations. These endpoints must keep secrets and impersonation/session references server-side.

## Generated API Client

Explore.Blazor.Client.csproj defines the canonical generation path:

| Artifact | Configured path |
|---|---|
| OpenAPI input | schemas/openapi_islamu.json |
| Generated output | src/Explore.Blazor.Client/Clients/IslamuApiClient.g.cs |
| Client/interface | IslamuApiClient and IIslamuApiClient |

The configured schema and generated output are absent in the current worktree. Existing services reference IIslamuApiClient, so generation is a required build input rather than an optional document example.

The project target instructs contributors to build Explore.API to produce schemas/openapi_islamu.json, then generate the client. Do not hand-write DTOs or restore retired generated contracts. Generated output must not be edited directly.

When the API contract changes:

1. Build/generate the current ISLAMU OpenAPI schema.
2. Regenerate IslamuApiClient.g.cs through the project target.
3. Adapt UI behavior in a service/adapter rather than in generated code.
4. Verify the affected service/component and API contract tests.

## Service And Component Boundaries

Razor components should orchestrate presentation. Typed services should own:

- generated-client calls;
- HAL collection unwrapping;
- safe mapping to view state where necessary;
- cancellation and error translation;
- optimistic concurrency headers;
- narrow state publication.

Current adapters cover live capabilities including organizations and members, groups, tenant settings and onboarding, Blog, navigation/footer/appearance, storage, notifications, webhooks, support access, external API keys, localization, federation foundations, and AI conversations.

Retired product registration, management, moderation, report, session, agenda, and custom-property projections are not valid frontend examples.

## HAL Affordance Rule

HAL links are the single source of truth for resource actions. A component must determine whether an action is available from the returned _links relation.

Examples already present in current source include:

- webhook consumer/endpoint creation and management;
- tenant branding/storage setting updates;
- AI conversation and proposed-action transitions;
- control-plane resource actions.

Do not reimplement authorization with local role, claim, route, tenant, or provider checks. Broad claims may influence non-resource navigation, but Edit, Delete, Retry, Rotate, Confirm, or similar actions require the corresponding API link.

## State And Lifetime Guidance

- Keep presentation state scoped to the interactive circuit unless a source-grounded singleton is explicitly safe.
- Store no access tokens or provider credentials in component state or browser storage.
- Propagate cancellation from components to services.
- Unsubscribe event handlers and dispose timers/streams.
- Avoid duplicating generated API response types in UI contracts.
- Keep tenant selection reconciled with the server-resolved tenant boundary.

## Current Security-Critical Services

| Area | Current implementation |
|---|---|
| BFF token forwarding | Event.Web.BffHosting plus YarpProxyExtensions |
| Unsafe-request antiforgery | BffAntiforgeryMessageHandler |
| API client | IIslamuApiClient / IslamuApiClient generation contract |
| Render policy | RuntimeRenderPolicyService, InteractiveServer only |
| Webhook management | WebhookManagementService and HAL-aware component |
| Storage/support/setup secrets | Owned BFF endpoint extensions keep sensitive state server-side |

## Related Documentation

- [SECURITY-MODEL.md](SECURITY-MODEL.md)
- [AUTHORIZATION.md](AUTHORIZATION.md)
- [API.md](API.md)
- [DESIGN_SYSTEM.md](DESIGN_SYSTEM.md)
- [BLAZOR_DEV_WORKFLOW.md](BLAZOR_DEV_WORKFLOW.md)
