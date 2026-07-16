<!-- ABOUTME: Canonical product definition for the first-party ISLAMU website and web application. -->
<!-- ABOUTME: Separates committed capabilities from future discovery and unsupported product promises. -->

# ISLAMU Website Product Definition

> **Audience:** Product contributors | Maintainers | Designers | Operators | AI agents
> **Status:** Active
> **Owner:** Product | Platform/Ops
> **Last Verified:** 2026-07-15
> **Source Anchors:** `README.md`, `dev/active/init-import-islamu-event/`, `dev/active/islamu-website-non-code-refactor/`, `src/Explore.Blazor/`, `src/Explore.Blazor.Client/`

## Product

ISLAMU is a first-party website and web application operated for the ISLAMU organization.
It is the public digital front door for explaining ISLAMU's purpose, publishing trustworthy
content, helping people participate, and adding future services through explicit, owned
workstreams.

The repository is not a supported self-hostable, white-label, multi-tenant product. Imported
platform capabilities survive only when the application still calls them and ISLAMU owns an
operational use for them.

## Experience Contract

1. **Purpose before promotion:** every page helps a visitor understand, learn, or take a real next step.
2. **Truth before breadth:** implemented behavior is stated as implemented; planned behavior is labelled and is not exposed as a working destination.
3. **Editorial depth:** the Blog is the first new product vertical and the foundation for durable publishing.
4. **Multilingual parity:** English, French, and Arabic are first-class; Arabic uses end-to-end right-to-left layout and content review.
5. **Accessible interaction:** WCAG 2.2 AA, keyboard/touch parity, visible focus, 200% zoom, reduced motion, and semantic structure are launch constraints.
6. **Privacy-respecting engagement:** analytics, forms, email capture, and personalization require explicit consent, retention, ownership, and deletion decisions.
7. **Fast and resilient:** public content remains useful when optional analytics, email, or third-party integrations fail.

## Capability Horizons

| Horizon | Commitment | Public Contract |
|---|---|---|
| Foundation | Committed | Event-free ISLAMU identity, truthful public shell, accessibility, localization, security, and operations. |
| Publish | Committed, implementation in progress | Blog, purposeful navigation, metadata, sitemap/robots, and content governance appear only after their implementation gates pass. |
| Engage | Direction approved, separate workstreams required | Waitlists, newsletters, contact, and transactional email require complete consent and operational flows before publication. |
| Expand | Discovery only | Programs, community services, donations, tools, and personalization are not product commitments or dependency-retention reasons. |

## Public Truth Model

Every proposed route, navigation item, call to action, locale, analytics event, form, and email
capture must record:

- owner;
- state: `Implemented`, `Planned`, or `Deferred`;
- implemented destination or source evidence;
- content owner and review date;
- localization readiness;
- accessibility acceptance;
- privacy/consent impact; and
- operational dependency.

Only `Implemented` entries may be clickable or described as available. `Planned` and `Deferred`
entries remain internal planning data.

## Retained Platform Capabilities

- Clean Architecture boundaries and CQRS/MediatR application flow where they remain useful.
- ASP.NET Core, Blazor, MudBlazor, YARP BFF, PostgreSQL, Keycloak, and authorization boundaries used by surviving features.
- Weblate-backed localization and checked-in fallback resources.
- External/configurable PostHog analytics with consent and non-blocking failure behavior.
- Mailpit for local email testing only.
- Durable email/outbox and Listmonk integration only while verified callers or approved engagement workstreams exist.

Retention is end-to-end: source, configuration, policy, tests, operations, and ownership must agree.

## Explicit Non-Goals

- Supporting third-party self-hosting, white-label deployments, or deployment tiers.
- Preserving Event product APIs, data, routes, policies, or compatibility aliases.
- Publishing placeholder routes, dead calls to action, invented impact metrics, unverified testimonials, or unapproved legal/privacy claims.
- Retaining speculative providers or abstractions for possible future features.

## Delivery Ownership

The code-led import workstream at `dev/active/init-import-islamu-event/` owns runtime changes,
Event deletion, Blog implementation, navigation implementation, migrations, and generated
contracts. The non-code workstream at `dev/active/islamu-website-non-code-refactor/` owns public
positioning, content truth, artifact disposition, contributor contracts, and final documentation.
