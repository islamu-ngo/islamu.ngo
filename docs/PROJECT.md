<!-- ABOUTME: Canonical product definition for the first-party ISLAMU website and web application. -->
<!-- ABOUTME: Records implemented public capabilities, deliberate platform retention, and future boundaries. -->

# ISLAMU Website Product Definition

> **Audience:** Product contributors | Maintainers | Designers | Operators | AI agents
> **Status:** Active
> **Owner:** Product | Platform/Ops
> **Last Verified:** 2026-07-18
> **Source Anchors:** `dev/report/EVENT_REFERENCE_ALIGNMENT.md`, `src/Explore.API/Controllers/BlogController.cs`, `src/Explore.API/Controllers/SitemapController.cs`, `src/Explore.Blazor.Client/Pages/Blogs/`, `src/Explore.Blazor.Client/Layout/SiteMegaMenu.razor.cs`

## Product

ISLAMU is one first-party website and web application operated for the ISLAMU organization. It is
the public digital front door for explaining ISLAMU, publishing owned content, and giving visitors
real, implemented destinations.

The repository is public under AGPL-3.0-or-later, but it is not a supported white-label,
third-party-operated, or self-hosting product. Legacy `Explore.*` and `Event.*` project names are
source identifiers, not product branding.

## Implemented Public Capabilities

### Foundation

- Public Home, About, Contact, Community Guidelines, Privacy, and Terms destinations.
- Keycloak-backed authentication through the server-side Blazor BFF.
- English, French, and Arabic/RTL infrastructure, accessibility guardrails, and a responsive shell.
- PostgreSQL persistence, HAL APIs, authorization, storage, observability, and durable outbox
  infrastructure retained where current callers still exist.

### Blog

The Blog is an implemented vertical across all application layers:

- anonymous index and article routes at `/blog` and `/blog/{slug}`;
- authenticated editorial routes at `/blog/manage`, `/blog/manage/new`, and
  `/blog/manage/{id}`;
- draft, update, publish, unpublish, and soft-delete lifecycle;
- category/tag filtering, title search, pagination, optional storage-backed thumbnails, and
  sanitized Markdown rendering through Markdig and HtmlSanitizer;
- HAL-gated editorial affordances; and
- sitemap entries for currently published articles.

Source presence proves the implementation shape. This documentation lane did not run the app,
build, migrations, or tests, so it does not claim runtime acceptance.

### Navigation

`SiteMegaMenu` is the implemented public navigation model. It exposes only real routes, grouped as
Discover, About ISLAMU, Community, and Policies. Desktop uses accessible disclosure panels; mobile
uses a temporary drawer with native `details` groups. Escape handling, route dismissal, viewport
switching, focus save/restore, and reduced-motion CSS are part of the implementation.

The menu is intentionally static and typed. Dynamic product discovery and placeholder
destinations are not implemented.

## Retained Platform Chassis

The first-party website still uses Organizations, Groups, notifications, email/outbox, storage,
support access, webhook infrastructure, and generic federation/PDS hooks. Coop and Osprey
integration source also remains. Retention means these capabilities may be documented as technical
infrastructure, not advertised as new public products.

See [`dev/report/EVENT_REFERENCE_ALIGNMENT.md`](../dev/report/EVENT_REFERENCE_ALIGNMENT.md) for the
current retain/adapt/delete decisions.

## Contract Generation Status

The API and Blazor projects contain OpenAPI/NSwag generation configuration for
`schemas/openapi_islamu.json` and `Clients/IslamuApiClient.g.cs`. Neither generated file was present
in the inspected worktree on 2026-07-18. Do not claim a checked-in schema or generated client exists
until the owning build/generation lane creates and verifies it.

## Planned, Not Publicly Available

- Newsletter, waitlist, campaign, preference-center, and other engagement journeys.
- Product/service catalog expansion, donations, community tools, or personalization.
- Public federation/PDS experiences and product-domain webhook publications without a named owner.
- Dynamic navigation or placeholder routes for future services.

Planned work requires its own approved implementation, content owner, privacy decision, and
observable destination before it appears in public navigation or calls to action.

## Product Rules

1. Only implemented destinations are linked or described as available.
2. Public claims need an accountable content owner and source evidence.
3. English, French, Arabic/RTL, WCAG 2.2 AA, keyboard/touch use, zoom, reduced motion, and safe
   failure behavior are launch gates.
4. Analytics and email engagement require explicit consent, retention, withdrawal, and operational
   ownership.
5. Event product routes, data models, APIs, policies, UI, and compatibility aliases remain deleted.

## Explicit Non-Goals

- Restoring the Event product.
- Supporting third-party self-hosting, white-label deployments, or product tiers.
- Inventing impact metrics, testimonials, legal commitments, or dead calls to action.
- Retaining speculative abstractions or providers for unapproved future features.
