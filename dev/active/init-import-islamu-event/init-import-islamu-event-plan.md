<!-- ABOUTME: Re-baselined plan for retaining the imported platform chassis while removing the Event product from islamu.ngo. -->
<!-- ABOUTME: Defines canonical local-lite development, Blog delivery, and the Plane-inspired ISLAMU navigation shell. -->

# Init Import ISLAMU Event — Implementation Plan

Last Updated: 2026-07-15 Europe/Brussels

> **Revision 2 (2026-07-14):** Deep update after analyzing the private **Event-Control-Plane** repository and its `dev/report/EVENT_REFERENCE_ALIGNMENT_REPORT.md`. Major changes: (a) islamu.ngo is **not a self-hostable product** — it is a single first-party deployment, which reshapes deployment, docs, and pruning decisions; (b) Keycloak integration redesigned around the Control Plane pattern — config-driven OIDC authority supporting three topologies, dev-fixture realm import, and a per-repo **additive provisioner job** (no shared realm-export editing, no cross-repo backport); (c) the Control Plane's InteractiveServer-only BFF is family precedent for the render-mode conversion; (d) the alignment report's decision categories (Mandatory parity / Reusable pattern / Adaptation / Intentional exception / Not applicable) are adopted as the governance method for Phase 8 scoping and long-term divergence from Event; (e) new production-posture phase modeled on `compose.production.yaml`.
>
> **Revision 2.1 (2026-07-14):** U7 resolved by the owner: the user holds full copyright over all three codebases (sole author plus friends who assigned copyright fully) and authorizes copying code in any direction, including Control Plane → islamu.ngo. The former hard C-BOUNDARY prohibition is replaced by C-FLOW (a provenance + deliberate-publication rule): copying is legally unrestricted; the only remaining check is the *business* one — content copied from the private Control Plane repo into public islamu.ngo becomes public under AGPL, so each such copy is a deliberate per-file publication choice, recorded in the alignment ledger. Practical consequence: Task 4.2 now **adapts the Control Plane's finished `provision-control-plane-client.sh` directly** instead of re-authoring the pattern from Event's `keycloak-init.sh`, cutting effort and risk.
>
> **Revision 3 (2026-07-15; owner directives after Phases 0–3 delivery):** Three changes.
> (a) **Dual-stack full boot is DEAD as an acceptance gate** — the dev machine cannot run two `local-full` Aspire stacks (hard owner constraint: "my pc will crash"). The canonical dev/verification profile for islamu.ngo is **`local-lite`** (`dotnet run --project Explore.AppHost/Explore.AppHost.csproj --launch-profile local-lite`, `ISLAMU_ASPIRE_MODE=ExternalInfra`): Keycloak, Cerbos, and PostgreSQL come from the owner's **cloud instances via Infisical** secret resolution, not local containers. Phase 3's acceptance is re-verified through a `local-lite` boot; the +10000 port split stays as collision hygiene, not as a tested dual-boot guarantee.
> (b) **Event domain surface will be DELETED** (owner decision): islamu.ngo is the ISLAMU website/webapp, not an events platform. All event-related product code (Events, EventSessions, registrations, agenda, series, templates, and the rest of the event cluster) is removed after the platform chassis is stabilized. This **pre-resolves the largest part of Task 8.1(a)**: the event module cluster's D8 category is *Not applicable — delete* (owner statement 2026-07-15), while chassis modules (identity, tenants, storage, notifications, HAL/governance, localization, SEO, webhooks, …) get keep/repurpose verdicts in the Phase 10 inventory. New **Phase 10** stages this dismantling.
> (c) **Blog module is the first islamu.ngo product feature** — new **Phase 11**. Scope model: the owner's earlier `iLoveIbadah_API` clean-architecture Blog implementation (`iLoveIbadah.Application/Features/Blogs`, `BlogsController`, `Blog`/`BlogLike`/`BlogCategoryMapping`/`BlogTagMapping` entities). It is a **shape reference only** — the islamu.ngo implementation follows this repo's conventions (CQRS/MediatR feature folders, manual validators, entities-not-DTOs in handlers, HAL affordance gating, Cerbos authorization actions, TUnit tests, architecture-test compliance), not iLoveIbadah's (int PKs, DataAnnotations, role-string auth, DTO-in-command).
>
> **Revision 3.1 (2026-07-15; navigation directive):** Add **Phase 12** for the ISLAMU site navbar. Plane's public site was inspected with Chrome DevTools MCP at desktop and mobile sizes. The desired result is its interaction model—a full-width, footer-like mega panel below the fixed navbar, multi-column grouped links, fast panel switching, dimmed page backdrop, and automatic close after pointer/focus leaves—not its copy, assets, or code. The islamu.ngo implementation uses a small MudBlazor v9 disclosure-navigation component, CSS isolation/BEM and existing design tokens; `MudMenu` is intentionally not the full-width panel primitive. Touch/mobile uses an explicit menu/drawer with expandable sections, never hover-only behavior.
> Backward compatibility is a non-goal throughout (development mode).

## 0. Planning Metadata

- **Request:** Keep the imported Event platform as the islamu.ngo application chassis; make the updated Event AppHost `local-lite` profile canonical; delete the Event product; add Blog; then replace the Event-oriented navbar with a Plane-inspired, footer-like ISLAMU mega navigation implemented in MudBlazor v9.
- **Task directory:** `dev/active/init-import-islamu-event/`
- **Planning status:** **Re-baselined, Revision 3; implementation in progress**
- **Reference repositories:**
  - **Source:** `/home/amir/ISLAMU/Github/Event` (public, AGPL) — the platform being copied.
  - **Sibling precedent:** `/home/amir/ISLAMU/Github/Event-Control-Plane` (private, commercial) — a *different* copy approach: fresh `Event.ControlPlane.*` solution aligned to Event invariants by decision category rather than wholesale transplant. Its Keycloak, production-compose, and InteractiveServer patterns are adopted here, and selected assets (notably the Keycloak provisioner script) may be copied and adapted directly — the owner holds full copyright across all three repos (C-FLOW).
- **Matched intents:** None in Event's `.claude/contract/intents.yaml` — **Fallback Contract** (§0.1). Closest analogs: `external-infrastructure-bootstrap` (Keycloak constraints adopted) and the Control Plane report's §3 "Non-Negotiable Boundaries" (repository-direction and provenance rules adopted).
- **Relevant skills (Event repo):** `clean-architecture-rules`, `auth-patterns`, `blazor-bff-patterns`, `blazor-ui-conventions`, `blazor-css-isolation`, `design-system`, `aspire`, `dotnet-efcore-guidelines`
- **Relevant rules (Event repo):** `.claude/rules/blazor-server.md`, `.claude/rules/blazor-client.md`, `.claude/rules/api-controllers.md` — these travel with the copy and become authoritative in islamu.ngo after import.
- **Primary layers touched:** All (Domain / Application / Persistence / Infrastructure / API / Blazor / Docs / DevOps) — whole-platform import.
- **Estimated complexity:** **XL.** Source: 16 src + 13 test projects, 4,505 C# + 275 Razor files, ~20-service local stack. The copy is mechanical; risk lives in identity/topology separation, render-mode conversion, non-self-hostable pruning decisions, and re-baselining a polluted target git history.

### 0.1 Fallback Contract

| Contract element | Value |
|---|---|
| Authority docs | Event repo `AGENTS.md`, `docs/QUICK_REFERENCE.md`, `docs/GOVERNANCE.md`, `docs/OPERATIONS.md` (copied in Phase 1, then authoritative in islamu.ngo). The Control Plane's `EVENT_REFERENCE_ALIGNMENT_REPORT.md` §2, §3, §15 are adopted as **methodology** references (not copied) |
| Minimum tests | Full per-project test list from `docs/OPERATIONS.md` (13 projects), never solution-level `dotnet test` |
| Docs to update | islamu.ngo `README.md`; copied `CONFIGURATION.md`, `SECURITY-MODEL.md`, `OPERATIONS.md`; `SELF_HOSTING.md`/`DEPLOYMENT_MODES.md` reframed per D7 |
| Forbidden without approval | Realm create/delete/reimport; persisting bootstrap credentials; breaking Clean Architecture layering; committing build artifacts. Copying from the private Control Plane repo is *allowed* (C-FLOW) but each copied file is a deliberate publication decision recorded in the alignment ledger |
| Follow-up | Task 8.4 adds a `bootstrap-sibling-app` intent to the copied `intents.yaml` |

## 1. Executive Summary

`islamu.ngo` becomes the **central ISLAMU webapp**: the official website of the ISLAMU non-profit, built as a full application that will eventually serve all ISLAMU projects — product catalog, service offerings, fundraising, integrations, embeddings. We import the entire mature ISLAMU Event platform and adapt it.

There are now **three sibling repositories** in this family, each with a distinct copy philosophy:

| Repo | Visibility | Approach | Deployment posture |
|---|---|---|---|
| Event | Public AGPL | The reference platform | Self-hostable, white-label, multi-tenant |
| Event-Control-Plane | Private commercial | Fresh `Event.ControlPlane.*` solution; aligns to Event *invariants* per decision category; minimal-API endpoints; Svix-only; own `DESIGN.md` identity | **Not self-hostable**; lean production compose; external Keycloak |
| islamu.ngo (this plan) | Public AGPL | **Wholesale snapshot copy** of Event, then adapt | **Not self-hostable**; single first-party deployment; shares the ISLAMU Keycloak realm |

islamu.ngo deliberately combines both approaches: Event's code (license-clean, AGPL→AGPL) with the Control Plane's *deployment and identity discipline*. Four deliberate deltas from the source platform:

1. **Fully InteractiveServer render mode** — no WASM interactivity. The Control Plane already runs an InteractiveServer-only Blazor BFF in production topology, and its alignment report (§9.4, §15.3) explicitly endorses server-only as the simpler correct choice for a first-party deployment. Event's runtime render-policy system (`IRuntimeRenderPolicyService`, fallback already `InteractiveServer(prerender: true)`) makes this mostly a policy/default change. We keep Event's YARP BFF as-is (intentional divergence from the Control Plane's opaque-ticket BFF — see D4).
2. **Keycloak via the Control Plane topology pattern** — the OIDC authority is pure configuration (`https://<host>/realms/<realm>`), so the app supports three topologies without code changes: own Keycloak, shared instance + own realm, or **same instance + same realm** (the chosen default, so users keep their ISLAMU accounts). Local dev imports a dev-fixture realm; production runs an **additive, idempotent provisioner job** that owns only islamu.ngo's clients/scopes/mappers and never creates or deletes realm resources.
3. **Not self-hostable** — islamu.ngo drops Event's third-party-operator product surfaces over time (instance onboarding for strangers, deployment-mode selection, white-label configurability). Import first, prune later (Phase 8) using the alignment report's decision categories.
4. **Separate deployment identity** — own PostgreSQL/schema, Cerbos policy surface, storage, cache, service names, ports, and volumes; the canonical developer path consumes external instances through `local-lite`.

**Out of scope:** all braindump features (waitlists, release radar, demo center, email flows — future workstreams); renaming `Explore.*`/`Event.*` projects (deferred, D3); implementing production hosting (Phase 9 defines the target posture only).

## 2. Source-Grounded Current State Report

### 2.1 Evidence Log

| # | Claim | Evidence | Confidence | Notes |
|---|---|---|---|---|
| 1 | islamu.ngo working tree is nearly empty: `LICENSE` (AGPL v3), `README.md`, `.gitignore`, `.dockerignore`, `dev/`, empty `docs/`, empty `schemas/` | Verified by `find`, 2026-07-13 | High | |
| 2 | islamu.ngo HEAD (branch `main`) contains a stale "OpenISLAMU" VS solution **with committed `bin/`, `obj/`, `.vs/` binaries**; 1,562 working-tree deletions uncommitted | Verified: `git status` (1562 `D`, 2 `??`, 1 `M`) | High | Re-baseline required (Phase 0) |
| 3 | islamu.ngo README brands "OpenISLAMU.org"; AGPL v3 | Verified: `README.md` | High | Domain decision U2 open |
| 4 | braindump.md = research report (portal UX, email/waitlist architecture); explicitly not direction | Verified: read in full | High | Future-feature inspiration only |
| 5 | Event solution: 16 src + 13 test projects; 4,505 `.cs` + 275 `.razor` under `src/` | Verified: `Explore.slnx`, `find | wc -l` | High | |
| 6 | .NET SDK 10.0.300, Microsoft.Testing.Platform, central package management | Verified: `global.json`, root listing | High | |
| 7 | Event Keycloak (dev): `phasetwo-keycloak:26`, realm `ISLAMU` imported from `docker/keycloak/realm-export.json`, additive `keycloak-init.sh`; clients `islamu-event-blazor` (BFF) + `islamu-event-api` (resource server) + `islamu-event-api-audience` mapper | Verified: `src/Explore.AppHost/AppHost.cs:261-316,653-661`; realm export | High | |
| 8 | Event render mode is runtime-resolved per path: `App.razor` injects `IRuntimeRenderPolicyService`; fallback `InteractiveServer(prerender: true)`; WASM registered in `Program.cs:75-76,202-203` | Verified: `src/Explore.Blazor/Components/App.razor:9-11,107-194`; `Program.cs` | High | |
| 9 | Blazor isolated from backend layers; backend contact only via generated `IEventApiClient` | Verified: `docs/QUICK_REFERENCE.md` rule 23 | High | Architecture-test enforced |
| 10 | Event docker-compose: ~23 services (postgres, redis, mailpit, keycloak+init, api, ui, minio, cerbos, svix, weblate, coop, osprey); volumes `islamu-event-*` | Verified: `docker-compose.yml`; `AppHost.cs:290` | High | |
| 11 | Event agent-context system enforced by `Event.Architecture.Tests` | Verified: `AGENTS.md` §10; `intents.yaml` | High | |
| 12 | Event working tree dirty (webhook-delivery-redesign in flight); last clean commit `b1fadaa9` on `develop` | Verified: `git status`, 2026-07-13 | High | Snapshot must pin a committed SHA (R2) |
| 13 | `Explore.Blazor.Client` cannot ProjectReference `Explore.Application` (NU1605/WASM) | Project memory + Rule 23 design | Medium | Isolation kept even server-only |
| 14 | Event verification: Release build + per-project tests only | Verified: `AGENTS.md` §8 | High | |
| **15** | **Control Plane is a fresh solution, not a copy:** 10 src + 6 test projects named `Event.ControlPlane.*` (API, AppHost, Application, Blazor, Client, Domain, Infrastructure, MigrationService, Persistence, ServiceDefaults); minimal-API endpoint extensions, not MVC controllers | Verified: `Event-Control-Plane/src/`, `tests/` listing; report §3.4, §6.2 | High | Precedent for Task 8.3 (rename decision), not for this import |
| **16** | **Control Plane Keycloak topology is configuration-driven:** `CONTROL_PLANE_OIDC_AUTHORITY=https://identity.example/realms/ISLAMU`; env-selected realm (`KEYCLOAK_REALM=ISLAMU`), admin realm (`master`), and a dedicated provisioner admin client (`control-plane-realm-provisioner`) | Verified: `Event-Control-Plane/.env.example:9-27` | High | Same app binary serves own-Keycloak / shared-instance-own-realm / same-realm by config |
| **17** | **Dev realm import is an isolated fixture; production never imports it.** "The AppHost realm import is an isolated development fixture and includes test clients, test users, localhost callbacks, and development-only credentials. Production deployments must not import it." | Verified: `Event-Control-Plane/docs/SECURITY.md:45` | High | Kills the Rev-1 idea of a cross-repo shared realm-export.json |
| **18** | **Production Keycloak provisioning is an additive, idempotent, ownership-scoped job:** `docker/keycloak/provision-control-plane-client.sh` (22.9K, kcadm-based, `set -euo pipefail`, `umask 077`) "applies a private additive extension after the core realm exists… owns [its clients, scopes, role, mappers]… never creates a realm, deletes resources, or assigns the operator role to users"; bootstrap credentials stay in ephemeral CLI state | Verified: `Event-Control-Plane/docs/SECURITY.md:47`; script header + env block; `docker/keycloak/Dockerfile.provisioner` | High | islamu.ngo copies and adapts this script directly (C-FLOW; Rev 2.1) |
| **19** | **Control Plane production compose is lean and non-self-hostable:** networkless input validator → PostgreSQL → additive Keycloak provisioner job → one-shot migration service → API + workers → **Interactive Server Blazor BFF**; metrics on management port 9090 (404 on public 8080); Coolify owns proxy/TLS; the existing Keycloak deployment owns the core realm; explicitly forbidden to "bundle Keycloak, Event, telemetry, backup, or reverse-proxy infrastructure into production Compose without an explicit deployment requirement" | Verified: `Event-Control-Plane/docs/SECURITY.md:67`; report §11.11 | High | Target posture for islamu.ngo production (Phase 9) |
| **20** | **Control Plane runs InteractiveServer-only Blazor** and the alignment report resolves to keep it: "Keep Interactive Server… Do not import Event YARP, InteractiveAuto, WASM, or public-site BFF composition without a measured need" (§15.3); Event's own render-mode separation is documented as "dedicated control-plane admin routes use Interactive Server mode" (§9.4) | Verified: report §9.4, §10.6, §15.3 | High | Family precedent for D4 |
| **21** | **Alignment report methodology:** every finding classified as Mandatory parity / Reusable pattern / Control Plane adaptation / Intentional exception / Not applicable, each with required action; "Not applicable" exists specifically for "public/self-hosted requirements that Control Plane does not have… Record why to prevent future accidental scope growth" | Verified: report §2 | High | Adopted for Phase 8 (D8) |
| **22** | **Repository direction + provenance rules:** public Event must never reference private Control Plane; private may adapt public code with preserved AGPL provenance (`NOTICE.md` records this factually) | Verified: report §3.1, §15.11; `Event-Control-Plane/NOTICE.md` | High | CP's internal rules; for islamu.ngo superseded by C-FLOW (owner holds full copyright, Rev 2.1) — only the no-runtime-coupling and provenance-logging disciplines carry over |
| **23** | Control Plane report was compared against Event on 2026-07-14 and is "decision-complete"; its roadmap phases: Restore Contract Integrity → API Governance → HAL/UI Contracts → Browser Evidence → Operational Drills → Maintainability; product-domain Svix deferred until a named consumer exists | Verified: report header, §12 | High | Its HAL-verb defect finding (§5.3.1) is CP-internal, not relevant to islamu.ngo |
| **24** | Target `local-lite` maps to `ExternalInfra`, but target AppHost creates MigrationService only when a local database resource exists | Verified: islamu.ngo `src/Explore.AppHost/Properties/launchSettings.json:69-81`; `AppHost.cs:89-103` | High | Task 3.4 ports the source fix |
| **25** | Updated Event AppHost always adds MigrationService, forwards Infisical bootstrap configuration for non-`FullLocal` modes, and makes API/Blazor wait for migrations | Verified: Event `src/Explore.AppHost/AppHost.cs:96-111,143-151,199-207,841-884` | High | Canonical `local-lite` behavior |
| **26** | Event surface is cross-layer and large: filename discovery found 102 Domain, 776 Application, 142 Persistence, 74 API, 225 Blazor Client, and 293 test files named for Event/session/registration/agenda/series/ticket/RSVP concepts | Verified by target `find`/`rg`, 2026-07-15 | Medium | Counts show scale only; deletion uses ownership/caller evidence |
| **27** | iLoveIbadah Blog provides list/detail/create plus Blog/category, Blog/tag, and Blog/like model vocabulary, but update/delete/publish are incomplete and the implementation uses int IDs, Domain DataAnnotations, mutable entities, and role-string auth | Verified: `iLoveIbadah.Application/Features/Blogs`, `BlogsController.cs`, `iLoveIbadah.Domain/Blog*.cs` | High | Shape reference only |
| **28** | Plane desktop navigation uses disclosure buttons for Product/Solutions/Resources (`aria-haspopup`, `aria-expanded`, `aria-controls`) and mounts one full-viewport absolute layer directly below the fixed navbar. The white panel is full-width with centered multi-column content; the remainder is a translucent backdrop. Hovering an adjacent trigger swaps panel content in place; leaving starts a short exit before unmount. | Chrome DevTools MCP inspection of `https://plane.so/`, desktop 1760px viewport, 2026-07-15 | High | Interaction/layout evidence only; do not copy Plane assets, copy, classes, or DOM verbatim |
| **29** | Representative Plane timings are about 220ms enter and 150–220ms exit using opacity/transform; its inspected desktop panel was ~463px tall at 1760px and used a four-column body plus a separate announcement strip. Item rows combine icon, title, and description. | Chrome DevTools computed styles + DOM inspection, 2026-07-15 | High | Use ISLAMU tokens and content; dimensions are reference, not fixed requirements |
| **30** | Plane mobile at 390×844 removes hover navigation and exposes an explicit “Open menu” button; the resulting navigation is a long touch-friendly overlay/list. The existing islamu.ngo `NavMenu` is Event-oriented, while its fixed header and isolated BEM/token CSS provide the correct integration point for a viewport-wide panel. | Chrome DevTools mobile emulation; islamu.ngo `NavMenu.razor(.cs/.css)` + `MainLayout.razor.css`, 2026-07-15 | High | Phase 12 must implement independent pointer and touch behaviors |

### 2.2 Existing Implementation

**Target repo (`islamu.ngo`)** — the import is complete through Phase 2 and deployment tasks 3.1–3.3. The repository now contains the full Event chassis and product surface. Revision 3 keeps the chassis and removes the product.

**Source repo (`Event`)** — full platform per §2.1. Layer map (unchanged from Rev 1):

| Layer | Projects | Notes for import |
|---|---|---|
| Core | `Explore.Domain`, `Explore.Application` | Entities, CQRS features, authorization actions, HAL link relations |
| Infrastructure | `Explore.Infrastructure`, `Explore.Persistence`, `Explore.Secrets` | EF Core + PostGIS, repositories, Keycloak/machine identity, secrets control plane |
| Presentation | `Explore.API`, `Event.MigrationService` | HAL API; MigrationService applies EF migrations (Control Plane runs the same pattern as a one-shot production gate — Evidence #19) |
| UI | `Explore.Blazor` (server/BFF), `Explore.Blazor.Client`, `Event.Web.BffHosting` | YARP token-forwarding BFF, cookie auth, MudBlazor |
| Orchestration | `Explore.AppHost`, `Explore.ServiceDefaults`, `docker-compose.yml`, `docker/` | Dev stack incl. Keycloak dev-fixture realm import |
| Governance | `AGENTS.md`, `.claude/`, `.agents/`, `docs/`, `cerbos/`, `schemas/`, `dev/` | Self-enforcing via architecture tests |

**Sibling repo (`Event-Control-Plane`)** — analyzed as reference; selected assets may be copied and adapted (C-FLOW). islamu.ngo takes from it: the Keycloak topology model (Evidence #16-18) **including the provisioner script itself as the adaptation basis**, the lean production posture (Evidence #19), the InteractiveServer precedent (Evidence #20), the decision-category governance method (Evidence #21), and the provenance discipline (Evidence #22). Each CP-origin file copied into public islamu.ngo is a deliberate publication decision logged in the alignment ledger.

### 2.3 Existing Tests And Verification Coverage

Unchanged from Rev 1: islamu.ngo inherits Event's 13 test projects; they are the primary "did the copy work" instrument. (The Control Plane's 6 test projects stay in its repo; its report's testing philosophy — generated inventories for breadth, risk-weighted deep tests for authority/ownership/secrets — informs future islamu.ngo test work but adds no tasks here.)

### 2.4 Existing Documentation And Contracts

- Event docs (100+ files) copy over; identity-touched ones patched (Phases 3-4); **self-host-oriented docs get a posture reframe** (Phase 8/9): `SELF_HOSTING.md`, `DEPLOYMENT_MODES.md`, `DEPLOYMENT_TIERS.md` describe a product capability islamu.ngo does not offer — they become internal-ops references or are marked not-applicable, never silently deleted during import.
- API contract: `schemas/openapi.json` + generated `EventApiClient.g.cs`.
- Cerbos policies + tests.
- Control Plane docs (`SECURITY.md`, `PRODUCTION_OPERATIONS.md`, `DESIGN.md`, alignment report) are references; CP assets may additionally serve as copy/adaptation bases where they save effort (C-FLOW).

### 2.5 Current Pain Points / Improvement Areas

| Issue | Evidence | Consequence |
|---|---|---|
| Target git history contains committed binaries | #2 | Phase 0 re-baseline; user decision U1 |
| Source tree dirty (webhook redesign in flight) | #12 | Pin committed SHA (R2) |
| Rev-1 Keycloak design (edit realm-export.json + cross-repo backport) created shared mutable state across repos | Rev-1 plan §5-D5; contradicted by #17-18 | **Replaced** in Rev 2 by per-repo dev fixture + additive provisioner (D5v2); R5 downgraded |
| Two stacks share one Keycloak but nothing else | #7, #10 | Phase 3 identity split unchanged |
| WASM interactivity registered unconditionally | #8 | Phase 5 removes it; CP precedent de-risks (#20) |
| Event ships self-host product surfaces islamu.ngo doesn't want long-term (instance onboarding, deployment modes, white-label config) | #19, #21; user statement "it is not a self hostable webapp" | Decision D7 + Phase 8 classification followed by Phase 10 deletion with decision categories; do NOT prune without the boundary ledger and backup |
| Three-repo family now needs divergence governance | #21-23 | D8: alignment-report method adopted; Task 8.5 creates a living alignment doc |
| Existing navbar is Event-oriented and its compact link row cannot express the ISLAMU site's broader information architecture | #30 | Phase 10 removes Event affordances; Phase 12 introduces an ISLAMU content inventory and responsive mega navigation after Blog routes stabilize |

### 2.6 Unknowns After Investigation

| # | Unknown | Status | Resolution path |
|---|---|---|---|
| U1 | Git history: keep vs fresh orphan root | **Resolved** | Fresh orphan `main` completed in Phase 0 |
| U2 | Canonical domain/brand | **Resolved** | islamu.ngo |
| U3 | Event snapshot | **Resolved** | `5cfa996b2344969856946955270dd333e06fc61d` imported |
| U4 | Side services fit (Svix, Coop, Osprey, Weblate) | Open | Phase 8.1 with decision categories; note CP resolved Svix-only for webhooks and deferred product-domain publication until a named consumer exists (#23) — a useful template for islamu.ngo's own webhook scoping |
| U5 | Tenancy semantics | **Narrowed** | Non-self-hostable posture (D7) implies a single first-party install; whether the platform's multi-tenant machinery hosts multiple ISLAMU *projects* as tenants (one deployment, many project spaces) vs single-tenant is the remaining Phase 8.1 question |
| U6 | Production hosting | **Largely resolved** | Target posture = Control Plane pattern (#19): lean compose, external shared Keycloak, provisioner job + migration gate before app readiness, management-port metrics, Coolify proxy/TLS. Phase 9 specifies; implementation still a later workstream |
| **U7** | May Control Plane *assets* be copied into public islamu.ngo? | **Resolved 2026-07-14: YES.** Owner holds full copyright over all three repos (sole author + full assignments); copying in any direction is authorized | Replaced by C-FLOW: copying allowed; each CP→islamu.ngo copy is a deliberate publication (private→public AGPL) recorded in the alignment ledger |

## 3. Proposed Future State

islamu.ngo = imported platform chassis with these divergences:

1. **Identity/deployment split:** deployment identifiers re-prefixed `islamu-event-*` → `islamu-ngo-*`; C# untouched.
2. **Auth (Control Plane topology pattern):**
   - OIDC authority is configuration: `https://<keycloak-host>/realms/<realm>` — the same binary supports (a) own Keycloak, (b) shared instance + own realm, (c) shared instance + shared realm. **Default and chosen topology: (c), realm `ISLAMU`**, so existing users sign in unchanged.
   - Clients owned by islamu.ngo: `islamu-ngo-blazor` (confidential BFF), `islamu-ngo-api` (resource server), `islamu-ngo-api-audience` mapper — provisioned by `docker/keycloak/provision-islamu-ngo-client.sh`, an additive idempotent kcadm job **copied and adapted from the Control Plane's `provision-control-plane-client.sh`** (C-FLOW; Rev 2.1): owns only its resources, never creates/deletes a realm, bootstrap credentials ephemeral.
   - Optional `local-full` only: own Keycloak container may import a **dev-fixture** realm. Canonical `local-lite` never imports a realm and uses the existing cloud realm through Infisical-backed configuration.
3. **Rendering:** fully InteractiveServer (policy default + WASM registration removed); Event's YARP BFF retained (intentional divergence from CP's opaque-ticket BFF — swapping BFFs is out of scope).
4. **Posture:** not self-hostable. Production is one first-party deployment following the CP lean-compose model; self-host product surfaces are pruned/parked in Phase 8 via decision categories, each with a recorded "why" to prevent accidental scope regrowth (report §2 "Not applicable" discipline).
5. **Branding:** ISLAMU / islamu.ngo.
6. **Canonical development:** `dotnet run --project Explore.AppHost/Explore.AppHost.csproj --launch-profile local-lite`; MigrationService, API, and Blazor use Infisical-backed external infrastructure. No dual-`local-full` gate.
7. **Product boundary:** the entire Event product cluster is deleted end to end. Generic domain/outbox events, webhook event types, audit events, and transitional project names are not deleted merely because they contain “Event”.
8. **First feature:** Blog is implemented fresh with UUIDv7 `Guid`, tenant/audit/soft-delete/concurrency rules, per-tenant slug, Markdown content, optional StorageObject thumbnail, category/tag joins, draft/publish state, public reads, authorized writes, Cerbos, HAL, and Blazor UI. Likes/comments/views/recommendations are deferred.
9. **Site navigation:** fixed ISLAMU navbar with desktop hover/focus disclosures opening one full-width, footer-like mega panel; pointer/focus leaving the composite closes it, Escape closes and restores trigger focus, and mobile uses an explicit drawer/accordion model. Only existing routes are linked; Blog is integrated after its contract stabilizes.
10. **Governance:** copied Event contract system + a living `EVENT_REFERENCE_ALIGNMENT.md` for islamu.ngo.

End-state gate: build and remaining per-project suites green; `local-lite` observed with MigrationService completing before healthy API/Blazor; login works; no Event product route/UI/schema/contract remains; the Blog draft-to-publish-to-anonymous-read workflow passes through Blazor → BFF → API → PostgreSQL/Cerbos; and the new navigation passes desktop pointer, keyboard, touch/mobile, RTL, reduced-motion, and focus-loss acceptance.

## 4. Non-Negotiable Constraints

All Rev-1 constraints stand (Event invariants, additive-only Keycloak, per-project verification, no compatibility shims, no committed binaries). Added in Rev 2:

- **C-FLOW (replaces the Rev-2 C-BOUNDARY; per owner statement 2026-07-14):** The owner holds full copyright over Event, Event-Control-Plane, and islamu.ngo; code may be copied in any direction without legal restriction. Two practical rules remain: (1) **deliberate publication** — copying private CP content into public islamu.ngo publishes it under AGPL, so each CP-origin file is a conscious per-file choice, logged with provenance in the alignment ledger (Task 8.5), never a reflexive bulk copy of commercial surfaces; (2) **no runtime coupling** — nothing in islamu.ngo may take a build/runtime dependency on the private repo (path references, project references, packages); copies must be self-contained. Public Event still never references either sibling.
- **C-PROVISIONER (from Evidence #17-18):** production identity provisioning is an additive, idempotent, ownership-scoped job; it never creates or deletes a realm, never assigns roles to users, and keeps bootstrap credentials in ephemeral process state. The dev-fixture realm import is dev-only and must be documented as such.
- **C-POSTURE (from user + Evidence #19):** islamu.ngo is not self-hostable. No work item may add third-party-operator product surface; pruning of existing surfaces goes through Phase 8 decisions, never ad-hoc deletion.
- **C-PROVENANCE:** the import commit and a short `NOTICE.md`-style section in islamu.ngo README record source repo + SHA (AGPL→AGPL, license-clean; modeled on CP's factual NOTICE.md, Evidence #22).
- **C-LOCAL-LITE:** Do not run Event + islamu.ngo as two `local-full` stacks. Port only the proven Event `ExternalInfra` resource-graph behavior and preserve islamu.ngo names, ports, volumes, and intentional divergences.
- **C-SECRETS:** Infisical and cloud credentials, connection strings, endpoints, tokens, and tenant data must never enter tracked docs, tests, logs, commits, screenshots, or chat.
- **C-EVENT-DELETE:** Event removal is final. Do not retain disabled/deprecated routes, DTO aliases, HAL relations, hidden UI, schema aliases, or compatibility shims.
- **C-AMBIGUOUS-EVENT:** Generic domain events, webhook event types, audit events, and framework event handlers survive only when caller/ownership evidence proves a platform responsibility.
- **C-MIGRATIONS:** Applied migrations remain immutable. Add a backed-up/tested `RemoveEventProductSurface` migration, reach a green chassis, then add Blog in a separate `AddBlogFoundation` migration.
- **C-BLOG:** Adapt the iLoveIbadah behavior vocabulary, not its code conventions. Blog is tenant-safe; API/Cerbos enforce writes; UI uses HAL links only.
- **C-NAVIGATION:** Plane is a behavioral reference only. Use ISLAMU content, routes, assets, and tokens. Public navigation uses an accessible disclosure-navigation pattern (buttons controlling a labelled link region), not ARIA `menu/menuitem` unless the complete application-menu keyboard contract is implemented. Hover is enhancement only; focus, keyboard, touch, reduced motion, RTL, and outside-close behavior are required.

## 5. Architecture And Design Decisions

### D1 — Snapshot copy with fresh history, not fork/subtree *(unchanged)*
Pinned committed SHA, tracked files only, provenance in commit + context file. The Control Plane's fresh-build approach was considered as an alternative model (Evidence #15) and rejected for islamu.ngo: the user's explicit direction is wholesale copy, and islamu.ngo wants the whole product surface (site + API + UI), not a narrow operator console — a fresh build would re-implement thousands of files CP never needed.

### D2 — Copy ALL, prune later *(unchanged, sharpened)*
Import everything tracked. Pruning (now including self-host surfaces per D7) happens only in Phase 8, and every prune/park/keep verdict must use the report's decision categories with a recorded rationale (D8). This is exactly the report's guard against "future accidental scope growth" — in our case, accidental scope *retention*.

### D3 — No project/namespace renames now *(unchanged, precedent noted)*
The Control Plane proves clean sibling naming (`Event.ControlPlane.*`) is viable and pleasant — but it had it from day one of a fresh build. Renaming 4,505 copied files mid-import remains the top failure mode. Task 8.3 decides the future rename with CP as the naming precedent.

### D4 — Fully InteractiveServer via the existing render-policy system *(strengthened)*
- **Decision:** unchanged mechanics (policy default → InteractiveServer; remove WASM registrations in `Program.cs:75-76,202-203`; keep `Explore.Blazor.Client` as component library).
- **New support:** the Control Plane runs InteractiveServer-only in its production compose (Evidence #19) and its report resolves "Keep Interactive Server… Do not import Event YARP, InteractiveAuto, WASM… without a measured need" (§15.3). Event itself already forces InteractiveServer on control-plane admin routes (§9.4). Server-only is the family norm for first-party deployments; islamu.ngo joining it is de-risked.
- **Explicit divergence from CP:** islamu.ngo keeps Event's YARP token-forwarding BFF (it arrives working, with its tests). CP's opaque-ticket/refresh-lease BFF is architecturally attractive but adopting it means replacing a working auth stack — **Intentional exception**, recorded for Task 8.5's alignment doc. Revisit only with a measured need.
- **Adopt from report §9.4:** the component-discipline test idea — reusable Razor components must not touch `HttpContext` or assume WASM/server specifics. Event's architecture tests may already cover part of this; Task 5.1 verifies and Task 5.2 adds the gap if any.

### D5 (v2) — Keycloak: config-driven authority + dev fixture + additive provisioner job *(redesigned)*
- **Decision:** Replace the Rev-1 design (extend the shared `realm-export.json`, back-port to Event) with the Control Plane pattern:
  1. **Topology by configuration.** The OIDC authority, realm name, and client ids are environment configuration (Event's code already reads `KEYCLOAK_REALM`, client ids, etc. from configuration — `AppHost.cs:653-661`). Three supported topologies; chosen default: same instance, same realm `ISLAMU`.
  2. **Dev fixture.** islamu.ngo's own `docker/keycloak/realm-export.json` — copied from Event, extended with `islamu-ngo-blazor`/`islamu-ngo-api` dev clients, localhost redirect URIs, dev users/credentials. Documented as a development fixture that production must never import (mirrors `SECURITY.md:45`).
  3. **Additive provisioner.** New `docker/keycloak/provision-islamu-ngo-client.sh` + `Dockerfile.provisioner`, **copied and adapted from CP's proven `provision-control-plane-client.sh` + `Dockerfile.provisioner`** (C-FLOW; Rev 2.1 — this replaces the Rev-2 plan to re-author from Event's `keycloak-init.sh`). Adaptation: rename env surface and owned resources to `islamu-ngo-*`; drop CP-only resources (partner-automation client, `partner_account_id` claim, operator role — unless 8.1 wants an islamu.ngo staff role); keep the hardening verbatim (`set -euo pipefail`, `umask 077`, kcadm against `KEYCLOAK_ADMIN_REALM` default `master`, admin user or dedicated `islamu-ngo-realm-provisioner` client, owns-only reconciliation, never creates/deletes a realm, idempotent, credentials ephemeral).
- **Why better than Rev 1:** no shared mutable file between repos → the Rev-1 realm-drift risk (old R5) collapses; each sibling owns its identity extension; the same job serves dev-against-shared-realm, staging, and production; and (post-Rev 2.1) islamu.ngo starts from the *proven script* already operating for the commercial sibling rather than a fresh implementation.
- **Consequences:** no Event-repo back-port needed (the cross-repo task disappears). Realm-level *settings* (password policy, SMTP, themes) remain shared state governed by the realm owner — documented in the Phase 4 governance note.
- **Alternatives:** Rev-1 shared realm-export (rejected: drift, cross-repo coupling); Aspire-managed Keycloak resource in production (rejected: production Keycloak is external/shared, per Evidence #19).

### D6 — Separate database, Cerbos, storage; `local-lite` is canonical *(revised)*

The +10000 ports and `islamu-ngo-*` names remain collision hygiene. They no longer imply a dual-full-stack acceptance test. The owner runs cloud-backed `local-lite`; `local-full` is optional only.

### D7 — Not self-hostable: single first-party deployment *(new)*
- **Decision:** islamu.ngo does not support third-party operators. Consequences, phased:
  - **Import (Phases 1-2):** no effect — everything copies (D2).
  - **Phase 8 classification targets (decide, don't assume):** instance-onboarding flows for external operators; deployment-mode/tier selection surfaces; white-label/branding configurability (hard-set to ISLAMU); self-host provider-selection UI (e.g., webhook Local-vs-Svix operator choice — CP's Svix-only resolution is the template, Evidence #23); `SELF_HOSTING.md`/`DEPLOYMENT_MODES.md`/`DEPLOYMENT_TIERS.md` reframed as internal ops or marked Not applicable with recorded why. Approved deletions execute in Phase 10.
  - **Phase 9 production posture:** lean CP-style compose — validator → Postgres → provisioner job → migration gate → API+workers → InteractiveServer BFF; external Keycloak; management-port metrics; Coolify (or equivalent) owns proxy/TLS; never bundle Keycloak/telemetry/backup/reverse-proxy into app compose without explicit requirement (Evidence #19).
- **Why:** user statement ("it is not a self hostable webapp"), and the CP precedent shows exactly how a non-self-hostable sibling of Event is shaped.
- **Nuance:** the *repo* stays public AGPL; "not self-hostable" is a product-support posture (no operator surfaces, no support matrix), not a license change. Anyone may read the code; ISLAMU runs the only supported deployment.

### D8 — Adopt alignment-report decision categories for all divergence governance *(new)*
- **Decision:** Every Phase 8 scoping verdict and every future intentional divergence from Event uses exactly one category — **Mandatory parity / Reusable pattern / islamu.ngo adaptation / Intentional exception / Not applicable** — with rationale and evidence, accumulated in a living `dev/report/EVENT_REFERENCE_ALIGNMENT.md` for islamu.ngo (created by Task 8.5, modeled on CP's report structure §2/§15; may borrow CP report text where useful under C-FLOW). The ledger also records every CP-origin file copied into islamu.ngo (publication log).
- **Why:** three sibling repos now diverge from one reference; CP demonstrated that undisciplined divergence claims collapse under audit (report §5's three waves of corrections). Cheap to adopt now, expensive to retrofit.
- **Consequences:** slightly more writing per pruning decision; drastically cheaper future audits.

### D9 — Event product is Not applicable: delete end to end *(Revision 3)*
- **Decision:** remove Event aggregates, use cases, routes/contracts, generated client surface, UI, policies, integrations, tests, docs, and schema in Phase 10 without compatibility aliases. Retain technical domain-event/outbox/webhook/audit infrastructure only when the boundary ledger proves independent chassis ownership.

### D10 — Blog is the first fresh product vertical *(Revision 3)*
- **Decision:** use iLoveIbadah only for Blog vocabulary and use-case discovery. Implement the vertical in this repository's Clean Architecture, tenant, authorization, HAL, persistence, testing, and Blazor conventions, with the complete draft/publish lifecycle defined in Phase 11.

### D11 — Plane-inspired mega navigation as a focused MudBlazor shell component *(Revision 3.1)*
- **Decision:** Build one reusable `SiteMegaMenu` Razor component inside the Blazor shell. Desktop triggers are real buttons (or text-variant `MudButton`s) with `aria-expanded` and `aria-controls`; panel content uses normal headings/lists/links plus `MudIcon` and existing MudBlazor primitives. The panel itself is CSS-isolated and viewport-wide beneath `NavMenu`; it is not forced into `MudMenu`, whose anchored/content-sized popover model is the wrong primitive for this layout.
- **State model:** one `string?`/enum identifies the open section. `mouseenter`/focus opens and adjacent triggers switch content without tearing down the overlay. Pointer leave and `focusout` close only after the destination is outside the combined trigger+panel region; a cancellable 120–180ms grace period prevents the physical gap from causing flicker. Escape closes immediately and restores focus to the active trigger; route navigation, backdrop/outside activation, and responsive breakpoint changes also close stale state. Add minimal JS interop only if Blazor event data cannot reliably determine containment—do not introduce a general menu framework.
- **Layout/motion:** full-width absolute layer directly below the fixed header; centered four-column content on desktop, two columns at intermediate widths, auto height with a viewport-safe maximum and internal vertical scrolling, optional announcement strip, and a backdrop over page content. Animate only opacity/transform (~220ms enter, 150–220ms exit), disable motion under `prefers-reduced-motion`, and use logical properties/tokens for RTL.
- **Mobile/touch:** no hover contract. A labelled hamburger toggles a temporary top/end `MudDrawer` or equivalent shell overlay; sections expand explicitly with `MudCollapse`/`MudExpansionPanel` behavior and remain in normal reading order.
- **Content boundary:** first release uses a small static, typed ISLAMU navigation model co-located with the shell. No database/CQRS/admin-configurable mega-menu system. Inventory actual routes first and do not create dead links; a featured Blog card/announcement may query already-cached published Blog data only after Phase 11, never once per hover.
- **Accessibility rationale:** Plane exposes ARIA menu semantics, but islamu.ngo is site navigation rather than an application command menu. The WAI-ARIA disclosure-navigation model preserves expected Tab/link behavior while still providing expanded-state announcements, focus restoration, Escape, focus-visible styling, and automatic close when focus leaves.

## 6. Implementation Phases

Phase numbers preserve history. Revision-3.1 execution order is **3.4 → 10 → 11 → 12**, then remaining relevant 4–8 work; Phase 9 runs after the new Blog/navigation surface so production describes the post-Event product.

### Phase 0: Target Repo Re-Baseline *(unchanged)*
Tasks 0.1 (history decision U1), 0.2 (execute; backup branch; Event-grade `.gitignore`). Acceptance/verification as Rev 1.

### Phase 1: Snapshot And Copy *(one addition)*
Tasks 1.1 (pin SHA, export tracked files), 1.2 (copy with exclusions/merges), 1.3 (import commit with provenance). **Added to 1.3:** README gains a short provenance/notice section (C-PROVENANCE): source repo, SHA, AGPL statement — factual, modeled on CP `NOTICE.md` tone, written fresh.

### Phase 2: Build And Test Green (Unmodified) *(unchanged)*
Tasks 2.1 (restore+build), 2.2 (13-project sweep, baseline table, triage: environment / copy defect / pre-existing).

### Phase 3: Identity, Deployment Split, And Canonical `local-lite` *(revised)*
Tasks 3.1–3.3 are complete. **Task 3.4** selectively ports the updated Event AppHost behavior: MigrationService exists in `ExternalInfra`, receives the same Infisical bootstrap configuration as API/Blazor, and gates their readiness without starting local data/platform/heavy resources. Acceptance is a real `local-lite` boot, not dual-full-stack.

### Phase 4: Keycloak Topology Integration *(redesigned per D5 v2)*
- **Goal:** users log into islamu.ngo with existing ISLAMU-realm accounts; identity provisioning follows the dev-fixture + additive-provisioner model; topology is pure configuration.
- **Depends on:** Phase 3.
- **Relevant files:** islamu.ngo's copied `docker/keycloak/realm-export.json` + `keycloak-init.sh` (basis for the new provisioner), `src/Explore.AppHost/AppHost.cs:261-316,653-661`, BFF auth config (`Explore.Blazor`, `Event.Web.BffHosting/Authentication/`), API JWT config, `docs/SECURITY-MODEL.md`, `.env.example`.
- **Acceptance criteria:** login round-trip green with `islamu-ngo-blazor`/`islamu-ngo-api`; provisioner job idempotent against a realm it did not create; Event clients untouched; no bootstrap credentials persisted; topology switchable by env alone (verified at least for: local own-Keycloak fixture, and simulated shared-realm run of the provisioner against the dev Keycloak).
- **Verification:** manual login smoke; provisioner double-run diff-clean; `Explore.Blazor.IntegrationTests`, `Event.API.IntegrationTests`; negative audience test.
- **Rollback:** provisioned clients are removable; revert branch.

#### Task 4.1: Dev-fixture realm for islamu.ngo
- **Type:** modify · **Layer:** Infrastructure · **Effort:** M
- **Description:** For the optional `local-full` profile only, extend islamu.ngo's copied `realm-export.json` with `islamu-ngo-blazor`, `islamu-ngo-api`, mapper, and dev users. Mark it as a fixture that `local-lite`, staging, and production must never import.
- **Acceptance:** [ ] optional fresh local Keycloak boots with the islamu.ngo clients; [ ] fixture-only warning documented; [ ] `local-lite` resource graph contains no Keycloak/import resources. No Event-repo file is touched.

#### Task 4.2: Additive provisioner job (adapted from CP's proven script)
- **Type:** create · **Layer:** Infrastructure/DevOps · **Effort:** M (was L; reduced by C-FLOW) · **Dependencies:** 4.1
- **Files:** new `docker/keycloak/provision-islamu-ngo-client.sh`, new `docker/keycloak/Dockerfile.provisioner` — **copied from `Event-Control-Plane/docker/keycloak/` and adapted** (C-FLOW; publication of the adapted script into the public repo is a deliberate, logged decision — it is infrastructure glue, not commercial differentiator; confirm no CP-specific secrets/URLs/claims remain).
- **Description:** Adaptation checklist: rename env surface to `KEYCLOAK_ISLAMU_NGO_*`; owned resources become `islamu-ngo-blazor`, `islamu-ngo-api`, `islamu-ngo-api-audience`; **strip CP-only resources** (partner-automation client, `partner_account_id` claim, `control-plane-operator` role and scopes) unless Phase 8.1 defines islamu.ngo equivalents; keep hardening and control flow verbatim (fail-closed on missing realm — never create; owns-only additive reconciliation; idempotent rerun; `set -euo pipefail`; `umask 077`; ephemeral credentials, never logged; `…PROVISIONING_REQUIRED` gate). Wire env into `.env.example`.
- **Acceptance:** [ ] double-run produces zero changes on second run; [ ] missing realm → hard fail, no create; [ ] Event clients byte-identical before/after; [ ] shellcheck clean; [ ] no CP-specific identifiers/secrets remain (`control-plane-operator`, `partner_account_id`, CP client ids); [ ] copy logged with provenance (source path + adaptation summary) for the Task 8.5 ledger.

#### Task 4.3: App configuration + audience separation
- **Type:** modify · **Layer:** API/Blazor/Infrastructure · **Effort:** M · **Dependencies:** 4.1
- **Description:** Default client ids/authority in AppHost/appsettings/env to the `islamu-ngo-*` set; authority expressed as full realm URL so topology (a)/(b)/(c) is env-only. Verify `MachinePrincipalAccessor`/Cerbos principal derivation for realm users; negative test: token with only `islamu-event-api` audience is rejected by islamu.ngo API (and document the inverse guarantee for Event).
- **Acceptance:** [ ] login smoke green; [ ] audience negative test green; [ ] both integration suites green.

#### Task 4.4: Shared-realm + topology governance note
- **Type:** docs · **Layer:** Docs · **Effort:** S · **Dependencies:** 4.2, 4.3
- **Description:** `docs/SECURITY-MODEL.md` section: the three topologies and how env selects them; chosen default (shared realm `ISLAMU`); realm-level settings are shared state owned by the realm owner (cross-app coordination rule); provisioner ownership boundary (what islamu.ngo owns in the realm and what it must never touch); forbidden operations (create/delete/reimport realm).
- **Acceptance:** [ ] section names both siblings' client sets and the ownership rule.

### Phase 5: Fully InteractiveServer Render Mode *(one addition)*
Tasks 5.1 (map render-policy system — **now also**: check whether Event's architecture tests already enforce report-§9.4-style component discipline: no `HttpContext`/server-only assumptions in reusable components; note gap), 5.2 (enforce server-only; add policy-default regression test; **add component-discipline test if 5.1 found a gap**), 5.3 (regression sweep: three Blazor suites + manual smoke + no-`_framework/*.wasm` assertion). CP precedent (Evidence #20) recorded in the eventual alignment doc as supporting evidence.

### Phase 6: Branding And Product Identity *(unchanged)*
Task 6.1 (inventory + apply; README rewrite includes the provenance section from 1.3; resolve U2).

### Phase 7: CI/CD Bootstrap *(unchanged)*
Task 7.1 (workflow audit table: keep/adapt/disable; CI green; no deploys).

### Phase 8: Scope And Posture Decisions *(extended; still investigate/decide only)*
All verdicts use D8 decision categories with recorded rationale.

#### Task 8.1: Chassis + side-service scoping *(revised)*
- **Effort:** L — The Event product cluster verdict is already final: **Not applicable — delete**. Inventory adjacent modules (organizations/groups/locations, generic custom properties, AI, federation, moderation, notifications, side services) as keep/repurpose/delete using verified non-Event callers. Do not reopen the Event decision.
#### Task 8.2: islamu.ngo feature roadmap *(unchanged)* — braindump themes → ranked future workstreams.
#### Task 8.3: Solution identity rename decision *(unchanged, CP precedent)* — `Event.ControlPlane.*` shows the target shape; decide go/no-go for e.g. `Islamu.Ngo.*`.
#### Task 8.4: Add `bootstrap-sibling-app` intent *(unchanged)*.
#### Task 8.5: Create living alignment doc *(new)*
- **Type:** docs · **Effort:** M — Author `dev/report/EVENT_REFERENCE_ALIGNMENT.md` (structure per CP report §2/§15; may reuse CP report text under C-FLOW): decision-category ledger seeded with this workstream's intentional divergences (server-only rendering; YARP-BFF retention vs CP's ticket BFF; non-self-hostable posture; Keycloak topology; every Phase 8 verdict) **plus the CP-origin publication log** (starting with the Task 4.2 provisioner copy). Becomes the standing record future agents consult before "fixing" a deliberate divergence.
- **Acceptance:** [ ] doc exists with ≥ the seeded entries; [ ] referenced from islamu.ngo AGENTS.md routing table.

### Phase 9: Production Deployment Posture *(new; specification + skeleton, no hosting)*
- **Goal:** define and commit islamu.ngo's production topology per the CP model, so Phases 3-4 outputs compose into a deployable shape — without performing any actual deployment.
- **Depends on:** Phases 3-5; informs but does not block Phase 8.
- **Relevant references:** Evidence #19 (`compose.production.yaml` ordering + constraints), Event's `docker-compose.yml` (what NOT to bundle in production), `Event.MigrationService` (already the one-shot migration gate pattern), Phase 4 provisioner.
- **Acceptance criteria:** a `compose.production.yaml` (or equivalent) exists in islamu.ngo expressing: config validator (if cheap) → Postgres → **additive Keycloak provisioner job (external Keycloak, never bundled)** → one-shot MigrationService gate → API + workers → InteractiveServer Blazor BFF; metrics bound to a management listener, not the public port (verify Event's API supports this split — investigate subtask); no bundled Keycloak/telemetry/backup/reverse-proxy; TLS/proxy delegated to the platform (Coolify or equivalent); documented in an ops doc replacing the self-host framing (ties into 8.1's SELF_HOSTING.md verdict).
- **Verification:** `docker compose -f compose.production.yaml config` valid; local dry-run boot against the dev Keycloak simulating "external" mode; startup-ordering gates observed (API refuses start before migration job success).
- **Rollback:** file-level; nothing deployed.

#### Task 9.1: Production topology investigation
- **Type:** investigate · **Effort:** M — Verify Event API/Blazor support the needed shapes: external-authority-only Keycloak config (no AppHost dependency), management-port metrics binding (Event may expose metrics differently — find out), MigrationService as standalone gate, worker readiness gating. Output: gap list with file evidence.
#### Task 9.2: Author production compose + ops doc
- **Type:** create/docs · **Effort:** L · **Dependencies:** 9.1, Phase 4 — Write `compose.production.yaml` + `docs/PRODUCTION_OPERATIONS.md` (islamu.ngo-specific, fresh text). Close small gaps from 9.1 (e.g., a metrics-binding config) if S-effort; larger gaps become follow-up tasks.

### Phase 10: Delete The Event Product Surface *(new; owner decision)*

#### Task 10.1: Boundary ledger + backup gate
- Inventory Event-owned Domain/Application/Persistence/API/Blazor/policy/contract/test/doc/schema artifacts.
- Classify technical-event names by caller evidence rather than string matching.
- Add failing absence checks for canonical Event routes/resources.
- Capture Event table row counts without tenant data and verify a restorable PostgreSQL backup.

#### Task 10.2: Presentation + public-contract removal
- Delete Event Blazor/admin/navigation/services, Event controllers/routes/HAL/MCP/AI/SEO/calendar integrations, event-only Cerbos/actions/modules/notifications, and their tests.
- Regenerate OpenAPI and `EventApiClient.g.cs`; delete obsolete operations/models rather than aliasing them.
- Exit: no Event HTTP, HAL, UI, MCP, generated-client, or Cerbos surface remains.

#### Task 10.3: Domain/Application/Persistence/schema removal
- Delete the Event/EventSession graph and dependent CQRS, services, specifications, repositories, configurations, projections, DbSets, filters, seeds, views, and DI registrations.
- Preserve generic taxonomy/storage/outbox/domain-event/webhook/audit infrastructure only with independent-use evidence.
- Scaffold `RemoveEventProductSurface`; never edit old migrations. Test isolated PostgreSQL first, cloud PostgreSQL only after backup verification.
- Exit: no pending model changes; retained tenant filters/FKs pass; migration schema up/down is verified.

#### Task 10.4: Residue + green chassis
- Classify/delete Event-only tests while retaining or rewriting generic architecture/security coverage.
- Update API/Domain/Architecture/BLAZOR/config/ops/schema/README/dev docs.
- Run an allowlisted residual search. Remaining `Event` names need explicit non-product rationale.
- Exit: build and remaining project tests green; no Event product behavior visible in `local-lite`.

### Phase 11: Blog Vertical Slice *(new)*

#### Task 11.1: Domain + persistence
- Add tenant-scoped `Blog`, `BlogCategory`, and `BlogTag` with UUIDv7 `Guid`, audit/soft-delete/concurrency, normalized per-tenant slug, `ContentMarkdown`, optional StorageObject thumbnail, and nullable `PublishedAtUtc`.
- Add entity-returning repository/specification, EF configuration/filters/index/joins/DbSets/DI, and separate `AddBlogFoundation` migration.
- Test lifecycle, filters, slug uniqueness, taxonomy/thumbnail FKs, and no-pending-model.

#### Task 11.2: CQRS + API + Cerbos + HAL
- Add anonymous published list/detail-by-slug; authorized create/update/publish/unpublish/delete with manual validators and cancellation propagation.
- Validate same-tenant category/tag/thumbnail ownership.
- Add Blog actions/policy, named controller routes, RFC7807 metadata, collection/detail HAL policies, handler/auth/tenant/draft/slug/HAL/API tests.
- Regenerate OpenAPI/NSwag only after the contract stabilizes.

#### Task 11.3: Blazor
- Add `/blog`, `/blog/{slug}`, and authenticated management list/editor.
- Reuse storage upload and taxonomy selectors; gate writes only by HAL.
- Render Markdown with raw HTML disabled; add XSS, accessibility, state, validation, and affordance tests.

#### Task 11.4: SEO + docs + manual acceptance
- Add only published Blog pages to sitemap/metadata.
- Update relevant Domain/API/BLAZOR/auth/config/testing/ops docs.
- Manual `local-lite`: create draft → edit → taxonomy/thumbnail → publish → anonymous read → unpublish/delete.
- Likes/comments/views/scheduling/revisions/recommendations remain deferred.

### Phase 12: ISLAMU Navbar And Mega Navigation *(Revision 3.1)*

#### Task 12.0: Plane reference inspection *(complete — planning evidence)*
- Capture desktop trigger semantics, panel/backdrop geometry, multi-column anatomy, switch/exit behavior, animation timings, and mobile explicit-menu behavior with Chrome DevTools MCP.
- Record the findings as evidence #28–30 and in the context file. No Plane source/assets/copy enter the repository.

#### Task 12.1: Information architecture + design-system contract
- Remove remaining Event search/create/catalog assumptions from `NavMenu` as part of the Phase-10 boundary; inventory every surviving public/authenticated ISLAMU route after Blog lands.
- Define desktop groups, item labels/descriptions/icons, optional featured Blog/announcement content, and the mobile order. Ship only valid routes; mark future destinations deferred rather than linking placeholders.
- Add the `SiteMegaMenu` anatomy, states, responsive behavior, semantic tokens, z-index contract, motion values, focus behavior, RTL rules, and examples to `docs/DESIGN.md`; update `docs/BLAZOR.md` and `docs/ACCESSIBILITY.md` where their shell/navigation guidance changes.

#### Task 12.2: Desktop disclosure navigation
- Add the smallest maintainable `SiteMegaMenu.razor`/code-behind/isolated-CSS set and integrate it into `NavMenu`; retain one source of open-section state.
- Implement hover/focus open, adjacent-trigger switching, composite-region pointer/focus close with cancellable grace, backdrop/outside close, route-change close, Escape + focus restoration, and cleanup of pending close work on disposal.
- Render a centered four-column desktop body and optional lower announcement strip with ISLAMU tokens, BEM classes, logical CSS, safe max height/scroll, fixed-header/z-index coordination, and transform/opacity motion with reduced-motion fallback.

#### Task 12.3: Responsive/touch navigation
- At the selected design-system breakpoint, replace desktop hover disclosures with an explicit accessible menu toggle and temporary MudBlazor v9 drawer/overlay.
- Present the same content in touch-sized expandable sections and normal link reading order; close on route selection, explicit close, outside activation, Escape/back, and breakpoint transition.
- Verify the user/account dropdown and any shell/dock overlays remain mutually coherent; opening one shell overlay closes conflicting navigation state.

#### Task 12.4: Tests + browser evidence
- Add bUnit tests for open/switch/close, cancelled close grace, focus moving within vs outside, Escape/focus restoration, route cleanup, ARIA relationships, and mobile toggling.
- Add browser coverage at 375/390, 768, 1280, and 1760 widths for mouse, keyboard-only, and touch emulation; cover light/dark, English/Arabic RTL, 200% zoom, reduced motion, content overflow, and no layout shift.
- Run accessibility scans and manual Chrome DevTools comparison against the behavioral reference. Acceptance: no dead links, no per-hover API call, no console error, no obscured focused item, panel switches without flicker, focus loss closes, and mobile never depends on hover.

## 7. Testing Strategy

Rev-1 table stands, with additions:

| Concern | Instrument | Phase |
|---|---|---|
| Copy fidelity | file-list diff vs source `git ls-files` | 1 |
| Platform integrity | Release build + 13 suites per-project | 2 |
| Deployment split | AppHost graph tests + real cloud-backed `local-lite` migration/login smoke | 3 |
| **Provisioner correctness** | **double-run idempotence; fail-closed-on-missing-realm; Event-clients-untouched diff; shellcheck** | **4** |
| Auth + audience separation | login smoke; negative cross-audience test; two integration suites | 4 |
| Server-only rendering | three Blazor suites; no-WASM network assertion; **component-discipline architecture test (if gap found)** | 5 |
| Docs/context integrity | `Event.Architecture.Tests` docs/link/schema tests | 3,4,6,8 |
| CI | green Actions run | 7 |
| **Production shape** | **compose config validation; ordering dry-run** | **9** |
| **Event removal** | **route/HAL/OpenAPI/client/schema absence; residual allowlist; migration test** | **10** |
| **Blog** | **domain/persistence/tenant/draft/auth/HAL/XSS/bUnit + manual author-to-reader flow** | **11** |
| **Navbar/mega navigation** | **bUnit state/focus/ARIA coverage + desktop/mobile browser evidence, RTL, zoom, reduced motion, accessibility scan** | **12** |

## 8. Documentation, Configuration, And Operations Impact

- Patched: `CONFIGURATION.md` (identifiers, topology envs, fixture warning), `SECURITY-MODEL.md` (topology + ownership governance), `OPERATIONS.md`, `BLAZOR.md` (render divergence).
- Reframed per D7/Phase 8: `SELF_HOSTING.md`, `DEPLOYMENT_MODES.md`, `DEPLOYMENT_TIERS.md` → internal-ops reference or Not-applicable with recorded why; superseded operationally by new `PRODUCTION_OPERATIONS.md` (Task 9.2).
- New: `docker/keycloak/provision-islamu-ngo-client.sh` + `Dockerfile.provisioner`; `compose.production.yaml`; `dev/report/EVENT_REFERENCE_ALIGNMENT.md`; README provenance section; `bootstrap-sibling-app` intent.
- Navigation docs: `docs/DESIGN.md` gains the `SiteMegaMenu` primitive/tokens/states; `docs/BLAZOR.md` records the shell composition; `docs/ACCESSIBILITY.md` records the disclosure, focus-loss, Escape, mobile, zoom, and reduced-motion contract.
- **Removed from Rev 1:** the cross-repo realm-export back-port to Event — no longer needed under D5 v2.

## 9. Security, Authorization, Privacy, And Abuse Considerations

Rev-1 items stand (audience separation, secret handling via `Explore.Secrets`, Cerbos separation, BFF invariants). Added:

- **Provisioner threat model (C-PROVISIONER):** bootstrap admin authority exists only inside the job's process env; prefer a dedicated `islamu-ngo-realm-provisioner` admin client over the master admin user in shared environments; job output must never echo secrets; `umask 077` for any temp state. Fail closed when the realm is absent.
- **Shared-realm blast radius (refined):** islamu.ngo's realm footprint is exactly the resources its provisioner owns; everything else in realm `ISLAMU` is read-only from islamu.ngo's perspective. Realm-owner coordination documented (Task 4.4).
- **Non-self-hostable hardening dividend (D7):** operator-onboarding and deployment-mode surfaces, once pruned (Phase 8), reduce public attack surface on a first-party site — record this as rationale in the pruning verdicts.
- **Deliberate-publication control (C-FLOW):** copying CP code is legally clear (owner statement 2026-07-14), but publication of commercial-product internals into a public repo is one-way; the publication log (Task 8.5) plus per-file deliberation keeps this a product-strategy decision, not an accident.

## 10. Multi-Tenancy, Federation, Localization, Accessibility, Product Considerations

| Concern | Status | Rationale |
|---|---|---|
| Multi-tenancy | Needs decision (8.1) — narrowed by D7 | Single first-party deployment; open question is single-tenant vs ISLAMU-projects-as-tenants |
| Federation | Not applicable this workstream | Dormant foundations copy over |
| Localization | Applicable, no action | System copies over |
| Accessibility | **Applicable; Phase 12 action** | Disclosure navigation, predictable Tab order, focus-loss/Escape close, focus restoration, touch parity, 200% zoom, reduced motion, and browser evidence are explicit acceptance gates |
| White-label/self-host | **Not applicable (changed from Rev 1)** | D7: islamu.ngo is the single supported deployment; classify white-label machinery in Phase 8, then execute approved deletion in Phase 10 |

## 11. Observability And Operations

Inherited (OTel, health checks, Aspire dashboard). Added: Phase 9 adopts the CP posture — metrics on a management listener not the public port (9.1 verifies Event's support), migration job as readiness gate, low-cardinality discipline already Event policy. Both stacks' telemetry distinguishable via `islamu-ngo-*` service names (Phase 3).

## 12. Migration And Compatibility Plan

- Applied Event migrations remain immutable.
- Before `RemoveEventProductSurface`: verify the target cloud database, capture Event-table row counts, and create/test a restorable PostgreSQL backup.
- Test the destructive migration on an isolated database first. Its `Down` restores schema; discarded Event data requires backup restore.
- Reach a green Event-free chassis before creating independent `AddBlogFoundation`.
- No Event-to-Blog data conversion and no compatibility shims.

## 13. Risk Register

| # | Risk | Likelihood | Impact | Mitigation | Detection signal | Owner task |
|---|---|---|---|---|---|---|
| R1 | Polluted target history kept | Medium (if U1=keep) | Medium | Recommend orphan re-root; backup branch | clone size | 0.1/0.2 |
| R2 | Snapshot from dirty Event tree | Medium | High | Pin committed SHA; `git archive` | file-list diff | 1.1 |
| R3 | Hidden dependency on excluded files | Medium | Medium | Phase 2 green-before-modification gate | failures classed "copy defect" | 2.1/2.2 |
| R4 | Target `local-lite` starts API/Blazor without applying external migrations | Medium | High | Port always-present MigrationService + graph tests + real smoke | stale-schema startup | 3.4 |
| R5 *(reworked)* | Realm drift between repos | **Low (was High)** | Medium | D5 v2: per-repo dev fixtures + additive provisioner; no shared file | dev login failures after realm edits | 4.1/4.2/4.4 |
| R6 | Server-only rendering breaks WASM-assuming path | Medium | Medium | 5.1 investigation first; CP precedent narrows unknowns | Blazor suite/E2E failures | 5.1-5.3 |
| R7 | Audience misconfiguration cross-authorizes the two apps | Low | High | negative test in 4.3 | cross-app token accepted | 4.3 |
| R8 | Integration-test env prerequisites undocumented post-rename | Medium | Low | record in 2.2; rerun after Phase 3 | green in P2, red in P3 | 2.2/3.2 |
| R9 | Scope creep from braindump | Medium | Medium | §1 out-of-scope; Phase 8 investigate-only | untracked tasks appear | all |
| **R10** *(reworked in Rev 2.1)* | **Undeliberate publication of commercial CP internals into public islamu.ngo** — copying is now allowed, so the risk shifts from "legal breach" to "bulk-copying product differentiators without a decision" | Low-Medium | Medium (one-way publication of commercial surface) | C-FLOW per-file deliberation + publication log (Task 8.5); Task 4.2's approved copy sets the template; strip CP-specific identifiers/secrets on every copy | publication log missing entries for CP-origin diffs; CP secrets/URLs/claims in public tree | 4.2, 8.5, all |
| **R11** | **Provisioner run against the shared production realm damages sibling resources** | Low | High | fail-closed on missing realm; owns-only reconciliation; Event-clients-untouched acceptance test; dry-run against dev Keycloak first | realm diff shows non-owned changes | 4.2 |
| **R12** | **Three-repo divergence becomes untracked folklore** (future agents re-import pruned surfaces or "fix" intentional exceptions) | High over time | Medium | D8 categories + Task 8.5 living alignment doc + recorded-why on every verdict | PRs reversing Phase 8 verdicts without citing the doc | 8.1/8.5 |
| **R13** | Blind Event-name deletion removes technical event/outbox/webhook/audit infrastructure | Medium | High | caller-evidence ledger + residual allowlist | cross-cutting build/runtime failures | 10.1-10.4 |
| **R14** | Event-table deletion loses needed cloud data | Low-Medium | High | row review + verified backup + isolated migration rehearsal | missing retained data | 10.1/10.3 |
| **R15** | Blog draft or cross-tenant content leaks publicly | Low | High | repository-level published/tenant filters + wrong-tenant tests | anonymous draft read | 11.1/11.2 |
| **R16** | Markdown enables XSS | Medium | High | raw HTML disabled + malicious-content test | scriptable rendered output | 11.3 |
| **R17** | Hover mega menu flickers, traps focus, or becomes unusable on touch/zoom | Medium | Medium-High | composite-region close grace; disclosure semantics; explicit mobile drawer; viewport-safe scroll; bUnit + browser matrix | panel closes while entering it, remains open after focus leaves, or hides focused links | 12.2-12.4 |

## 14. Success Metrics And Definition Of Done

Rev-1 items 1-9 stand, amended/added:

5. *(amended)* Login with an existing ISLAMU-realm account via `islamu-ngo-blazor`; API accepts only `islamu-ngo-api` audience; **provisioner job proven idempotent and ownership-scoped against a realm it did not create**.
10. `compose.production.yaml` + `PRODUCTION_OPERATIONS.md` committed and config-validated (Phase 9).
11. `EVENT_REFERENCE_ALIGNMENT.md` exists with seeded divergence ledger **and CP-origin publication log** (Task 8.5).
12. Every CP-origin file in the repo appears in the publication log, carries adapted (non-CP) identifiers, and contains no CP secrets/URLs (R10 sweep).
13. `local-lite` is the documented canonical profile and is observed with MigrationService completing before healthy API/Blazor against Infisical-backed external infrastructure.
14. No Event product route, HAL relation, controller, CQRS feature, aggregate, repository, EF model entry, UI page, generated operation, MCP resource, Cerbos policy, seed, fixture, schema entry, or product doc remains.
15. Event removal and Blog creation use separate tested migrations with backup/rollback notes.
16. Blog supports tenant-safe draft/edit/taxonomy/thumbnail/publish/unpublish/delete, anonymous list/detail, HAL-gated management, safe Markdown, and an observed `local-lite` workflow.
17. The Event-oriented navbar is gone; desktop ISLAMU navigation opens on hover/focus, switches panels without flicker, closes when pointer/focus leaves or Escape/outside/route change occurs, restores focus on Escape, and provides an explicit touch/mobile experience with RTL, zoom, reduced-motion, and accessibility evidence.

## 15. Implementation Agent Contract — KEEP DEV DOCS CURRENT

Rev-1 items 1-7 stand. Item 8 replaced and item 9 added:

8. **Cross-repo guard (v3):** this workstream edits only **islamu.ngo**. The Event and Event-Control-Plane repos are read-only sources. Copying from either is permitted (C-FLOW), but every CP-origin copy is a deliberate publication: strip CP-specific identifiers/secrets, note provenance, and add it to the publication log. Never create a build/runtime dependency on either sibling repo.
9. Every pruning/divergence verdict must carry a D8 decision category and land in the alignment doc once Task 8.5 creates it.

## 16. Progress Reporting Contract

Unchanged from Rev 1 (Implemented / Verified / Remaining / Next / Docs updated — teaching-level detail).

## 17. Potential Risks & Unknowns (Candid Critique)

Rev 2 removes the worst Rev-1 seam (shared realm-export drift); Rev 2.1 removes the second one (the provisioner no longer needs to be re-authored — the proven CP script is the basis). What remains: First, **the provisioner is still the most safety-critical artifact** — a bash job holding realm-admin authority pointed at the same realm that serves the commercial product's users and the Event platform's users. Starting from CP's battle-tested script shrinks the implementation risk, but the *adaptation* is where new bugs enter: a missed rename could make islamu.ngo's job reconcile (and "correct") CP-owned clients in the shared realm. The Event-clients-untouched diff test must therefore also assert CP-owned clients untouched, and the first run against any shared (non-dev) Keycloak should be human-supervised. Second, **C-FLOW's remaining failure mode is drift-by-convenience**: with copying legally free in all directions, the cheap path for every future gap is "grab it from CP," which can silently publish commercial surfaces and blur which repo owns which behavior — the publication log only works if agents actually write to it, so PR review should ask "any CP-origin content? logged?" as a standing question. Third, **Phase 10 Event removal is where scope discipline will actually be tested** — Event's product and self-host surfaces are woven through onboarding, settings, docs, policies, generated contracts, and tests. The caller ledger, technical-event allowlist, backup gate, and staged removal migration prevent "delete by filename" from taking out shared chassis behavior. Finally, Phase 9's assumption that Event's API can bind metrics to a management port the way CP does is **unverified** (Task 9.1) — if Event lacks the split, that's a small change in islamu.ngo, but don't let it silently become "metrics on the public port in production."
