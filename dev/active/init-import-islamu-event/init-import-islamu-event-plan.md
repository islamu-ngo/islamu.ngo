<!-- ABOUTME: Implementation plan for importing the full ISLAMU Event platform into islamu.ngo as the central ISLAMU webapp. -->
<!-- ABOUTME: Source of truth for the copy strategy, Keycloak topology, InteractiveServer conversion, non-self-hostable posture, and phased execution. -->

# Init Import ISLAMU Event — Implementation Plan

Last Updated: 2026-07-14 Europe/Brussels

> **Revision 2 (2026-07-14):** Deep update after analyzing the private **Event-Control-Plane** repository and its `dev/report/EVENT_REFERENCE_ALIGNMENT_REPORT.md`. Major changes: (a) islamu.ngo is **not a self-hostable product** — it is a single first-party deployment, which reshapes deployment, docs, and pruning decisions; (b) Keycloak integration redesigned around the Control Plane pattern — config-driven OIDC authority supporting three topologies, dev-fixture realm import, and a per-repo **additive provisioner job** (no shared realm-export editing, no cross-repo backport); (c) the Control Plane's InteractiveServer-only BFF is family precedent for the render-mode conversion; (d) the alignment report's decision categories (Mandatory parity / Reusable pattern / Adaptation / Intentional exception / Not applicable) are adopted as the governance method for Phase 8 scoping and long-term divergence from Event; (e) new production-posture phase modeled on `compose.production.yaml`.
>
> **Revision 2.1 (2026-07-14):** U7 resolved by the owner: the user holds full copyright over all three codebases (sole author plus friends who assigned copyright fully) and authorizes copying code in any direction, including Control Plane → islamu.ngo. The former hard C-BOUNDARY prohibition is replaced by C-FLOW (a provenance + deliberate-publication rule): copying is legally unrestricted; the only remaining check is the *business* one — content copied from the private Control Plane repo into public islamu.ngo becomes public under AGPL, so each such copy is a deliberate per-file publication choice, recorded in the alignment ledger. Practical consequence: Task 4.2 now **adapts the Control Plane's finished `provision-control-plane-client.sh` directly** instead of re-authoring the pattern from Event's `keycloak-init.sh`, cutting effort and risk.

## 0. Planning Metadata

- **Request:** Copy ALL of the ISLAMU Event platform (API + Blazor Server + Blazor Client) into the `islamu.ngo` repository to become the central ISLAMU webapp under the islamu.ngo domain — fully InteractiveServer render mode, connecting to the *same* Keycloak instance and realm as a new set of client applications (with topology kept configurable, per the Control Plane pattern), then modify what must be modified. islamu.ngo is **not** self-hostable by third parties. The braindump (`dev/active/braindump.md`) is inspiration only, **not** final direction.
- **Task directory:** `dev/active/init-import-islamu-event/`
- **Planning status:** Draft, Revision 2 (awaiting user review)
- **Reference repositories:**
  - **Source:** `/home/amir/ISLAMU/Github/Event` (public, AGPL) — the platform being copied.
  - **Sibling precedent:** `/home/amir/ISLAMU/Github/Event-Control-Plane` (private, commercial) — a *different* copy approach: fresh `Event.ControlPlane.*` solution aligned to Event invariants by decision category rather than wholesale transplant. Its Keycloak, production-compose, and InteractiveServer patterns are adopted here, and selected assets (notably the Keycloak provisioner script) may be copied and adapted directly — the owner holds full copyright across all three repos (C-FLOW).
- **Matched intents:** None in Event's `.claude/contract/intents.yaml` — **Fallback Contract** (§0.1). Closest analogs: `external-infrastructure-bootstrap` (Keycloak constraints adopted) and the Control Plane report's §3 "Non-Negotiable Boundaries" (repository-direction and provenance rules adopted).
- **Relevant skills (Event repo):** `clean-architecture-rules`, `auth-patterns`, `blazor-bff-patterns`, `blazor-ui-conventions`, `aspire`, `dotnet-efcore-guidelines`
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
4. **Separate everything except identity** — own PostgreSQL, Cerbos, MinIO, Redis, ports, volumes; side-by-side dev with the Event stack.

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

### 2.2 Existing Implementation

**Target repo (`islamu.ngo`)** — greenfield with liabilities: AGPL LICENSE + README shell; polluted `main` history (Evidence #2); no code.

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
| Event ships self-host product surfaces islamu.ngo doesn't want long-term (instance onboarding, deployment modes, white-label config) | #19, #21; user statement "it is not a self hostable webapp" | New decision D7 + Phase 8 pruning with decision categories; do NOT prune during import |
| Three-repo family now needs divergence governance | #21-23 | D8: alignment-report method adopted; Task 8.5 creates a living alignment doc |

### 2.6 Unknowns After Investigation

| # | Unknown | Status | Resolution path |
|---|---|---|---|
| U1 | Git history: keep vs fresh orphan root | Open | Phase 0 user decision (recommend orphan) |
| U2 | Canonical domain/brand: `islamu.ngo` vs `openislamu.org` | Open | Phase 6; assume islamu.ngo per request |
| U3 | Which Event commit to snapshot | Open | Latest committed `develop` (`b1fadaa9` as of 2026-07-13) or post-webhook-redesign; user picks at execution |
| U4 | Side services fit (Svix, Coop, Osprey, Weblate) | Open | Phase 8.1 with decision categories; note CP resolved Svix-only for webhooks and deferred product-domain publication until a named consumer exists (#23) — a useful template for islamu.ngo's own webhook scoping |
| U5 | Tenancy semantics | **Narrowed** | Non-self-hostable posture (D7) implies a single first-party install; whether the platform's multi-tenant machinery hosts multiple ISLAMU *projects* as tenants (one deployment, many project spaces) vs single-tenant is the remaining Phase 8.1 question |
| U6 | Production hosting | **Largely resolved** | Target posture = Control Plane pattern (#19): lean compose, external shared Keycloak, provisioner job + migration gate before app readiness, management-port metrics, Coolify proxy/TLS. Phase 9 specifies; implementation still a later workstream |
| **U7** | May Control Plane *assets* be copied into public islamu.ngo? | **Resolved 2026-07-14: YES.** Owner holds full copyright over all three repos (sole author + full assignments); copying in any direction is authorized | Replaced by C-FLOW: copying allowed; each CP→islamu.ngo copy is a deliberate publication (private→public AGPL) recorded in the alignment ledger |

## 3. Proposed Future State

islamu.ngo = byte-faithful Event copy with these divergences:

1. **Identity/deployment split:** deployment identifiers re-prefixed `islamu-event-*` → `islamu-ngo-*`; C# untouched.
2. **Auth (Control Plane topology pattern):**
   - OIDC authority is configuration: `https://<keycloak-host>/realms/<realm>` — the same binary supports (a) own Keycloak, (b) shared instance + own realm, (c) shared instance + shared realm. **Default and chosen topology: (c), realm `ISLAMU`**, so existing users sign in unchanged.
   - Clients owned by islamu.ngo: `islamu-ngo-blazor` (confidential BFF), `islamu-ngo-api` (resource server), `islamu-ngo-api-audience` mapper — provisioned by `docker/keycloak/provision-islamu-ngo-client.sh`, an additive idempotent kcadm job **copied and adapted from the Control Plane's `provision-control-plane-client.sh`** (C-FLOW; Rev 2.1): owns only its resources, never creates/deletes a realm, bootstrap credentials ephemeral.
   - Local dev: own Keycloak container importing a **dev-fixture** realm (copied realm-export.json + islamu-ngo dev clients baked in, localhost callbacks, dev credentials) — never used in production; no cross-repo file sharing.
3. **Rendering:** fully InteractiveServer (policy default + WASM registration removed); Event's YARP BFF retained (intentional divergence from CP's opaque-ticket BFF — swapping BFFs is out of scope).
4. **Posture:** not self-hostable. Production is one first-party deployment following the CP lean-compose model; self-host product surfaces are pruned/parked in Phase 8 via decision categories, each with a recorded "why" to prevent accidental scope regrowth (report §2 "Not applicable" discipline).
5. **Branding:** ISLAMU / islamu.ngo.
6. **Governance:** copied Event contract system + a living `EVENT_REFERENCE_ALIGNMENT.md` for islamu.ngo (Task 8.5) tracking divergence from Event the same way CP does.

End-state gate (unchanged plus one): build green; 13 suites green; dual-stack dev boot; login with an existing ISLAMU-realm account over a pure InteractiveServer circuit; **and the provisioner job runs idempotently against a realm it did not create**.

## 4. Non-Negotiable Constraints

All Rev-1 constraints stand (Event invariants, additive-only Keycloak, per-project verification, no compatibility shims, no committed binaries). Added in Rev 2:

- **C-FLOW (replaces the Rev-2 C-BOUNDARY; per owner statement 2026-07-14):** The owner holds full copyright over Event, Event-Control-Plane, and islamu.ngo; code may be copied in any direction without legal restriction. Two practical rules remain: (1) **deliberate publication** — copying private CP content into public islamu.ngo publishes it under AGPL, so each CP-origin file is a conscious per-file choice, logged with provenance in the alignment ledger (Task 8.5), never a reflexive bulk copy of commercial surfaces; (2) **no runtime coupling** — nothing in islamu.ngo may take a build/runtime dependency on the private repo (path references, project references, packages); copies must be self-contained. Public Event still never references either sibling.
- **C-PROVISIONER (from Evidence #17-18):** production identity provisioning is an additive, idempotent, ownership-scoped job; it never creates or deletes a realm, never assigns roles to users, and keeps bootstrap credentials in ephemeral process state. The dev-fixture realm import is dev-only and must be documented as such.
- **C-POSTURE (from user + Evidence #19):** islamu.ngo is not self-hostable. No work item may add third-party-operator product surface; pruning of existing surfaces goes through Phase 8 decisions, never ad-hoc deletion.
- **C-PROVENANCE:** the import commit and a short `NOTICE.md`-style section in islamu.ngo README record source repo + SHA (AGPL→AGPL, license-clean; modeled on CP's factual NOTICE.md, Evidence #22).

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

### D6 — Separate database, Cerbos, storage; side-by-side runnable *(unchanged)*

### D7 — Not self-hostable: single first-party deployment *(new)*
- **Decision:** islamu.ngo does not support third-party operators. Consequences, phased:
  - **Import (Phases 1-2):** no effect — everything copies (D2).
  - **Phase 8 pruning targets (decide, don't assume):** instance-onboarding flows for external operators; deployment-mode/tier selection surfaces; white-label/branding configurability (hard-set to ISLAMU); self-host provider-selection UI (e.g., webhook Local-vs-Svix operator choice — CP's Svix-only resolution is the template, Evidence #23); `SELF_HOSTING.md`/`DEPLOYMENT_MODES.md`/`DEPLOYMENT_TIERS.md` reframed as internal ops or marked Not applicable with recorded why.
  - **Phase 9 production posture:** lean CP-style compose — validator → Postgres → provisioner job → migration gate → API+workers → InteractiveServer BFF; external Keycloak; management-port metrics; Coolify (or equivalent) owns proxy/TLS; never bundle Keycloak/telemetry/backup/reverse-proxy into app compose without explicit requirement (Evidence #19).
- **Why:** user statement ("it is not a self hostable webapp"), and the CP precedent shows exactly how a non-self-hostable sibling of Event is shaped.
- **Nuance:** the *repo* stays public AGPL; "not self-hostable" is a product-support posture (no operator surfaces, no support matrix), not a license change. Anyone may read the code; ISLAMU runs the only supported deployment.

### D8 — Adopt alignment-report decision categories for all divergence governance *(new)*
- **Decision:** Every Phase 8 scoping verdict and every future intentional divergence from Event uses exactly one category — **Mandatory parity / Reusable pattern / islamu.ngo adaptation / Intentional exception / Not applicable** — with rationale and evidence, accumulated in a living `dev/report/EVENT_REFERENCE_ALIGNMENT.md` for islamu.ngo (created by Task 8.5, modeled on CP's report structure §2/§15; may borrow CP report text where useful under C-FLOW). The ledger also records every CP-origin file copied into islamu.ngo (publication log).
- **Why:** three sibling repos now diverge from one reference; CP demonstrated that undisciplined divergence claims collapse under audit (report §5's three waves of corrections). Cheap to adopt now, expensive to retrofit.
- **Consequences:** slightly more writing per pruning decision; drastically cheaper future audits.

## 6. Implementation Phases

Phases 0-3 unchanged from Rev 1 (re-baseline; snapshot & copy; build/test green; identity split). Phase 4 redesigned; Phase 5 gains one check; Phase 8 extended; Phase 9 new.

### Phase 0: Target Repo Re-Baseline *(unchanged)*
Tasks 0.1 (history decision U1), 0.2 (execute; backup branch; Event-grade `.gitignore`). Acceptance/verification as Rev 1.

### Phase 1: Snapshot And Copy *(one addition)*
Tasks 1.1 (pin SHA, export tracked files), 1.2 (copy with exclusions/merges), 1.3 (import commit with provenance). **Added to 1.3:** README gains a short provenance/notice section (C-PROVENANCE): source repo, SHA, AGPL statement — factual, modeled on CP `NOTICE.md` tone, written fresh.

### Phase 2: Build And Test Green (Unmodified) *(unchanged)*
Tasks 2.1 (restore+build), 2.2 (13-project sweep, baseline table, triage: environment / copy defect / pre-existing).

### Phase 3: Identity And Deployment Split *(unchanged)*
Tasks 3.1 (identifier inventory table), 3.2 (apply renames: services, volumes incl. `AppHost.cs:290`, ports, DB `islamu_ngo`), 3.3 (patch identity-touched docs). Dual-stack boot is the acceptance gate.

### Phase 4: Keycloak Topology Integration *(redesigned per D5 v2)*
- **Goal:** users log into islamu.ngo with existing ISLAMU-realm accounts; identity provisioning follows the dev-fixture + additive-provisioner model; topology is pure configuration.
- **Depends on:** Phase 3.
- **Relevant files:** islamu.ngo's copied `docker/keycloak/realm-export.json` + `keycloak-init.sh` (basis for the new provisioner), `src/Explore.AppHost/AppHost.cs:261-316,653-661`, BFF auth config (`Explore.Blazor`, `Event.Web.BffHosting/Authentication/`), API JWT config, `docs/SECURITY-MODEL.md`, `.env.example`.
- **Acceptance criteria:** login round-trip green with `islamu-ngo-blazor`/`islamu-ngo-api`; provisioner job idempotent against a realm it did not create; Event clients untouched; no bootstrap credentials persisted; topology switchable by env alone (verified at least for: local own-Keycloak fixture, and simulated shared-realm run of the provisioner against the dev Keycloak).
- **Verification:** manual login smoke; provisioner double-run diff-clean; `Explore.Blazor.IntegrationTests`, `Event.API.IntegrationTests`; negative audience test.
- **Rollback:** provisioned clients are removable; revert branch.

#### Task 4.1: Dev-fixture realm for islamu.ngo
- **Type:** modify · **Layer:** Infrastructure · **Effort:** M
- **Description:** Extend islamu.ngo's copied `realm-export.json` with `islamu-ngo-blazor` (confidential; localhost + dev redirect URIs), `islamu-ngo-api`, `islamu-ngo-api-audience` mapper, dev users as needed. Mark the file (header comment + `docs/CONFIGURATION.md` note) as a development fixture that production must never import (mirrors CP `SECURITY.md:45` discipline). AppHost keeps importing it for local dev.
- **Acceptance:** [ ] fresh local Keycloak boots with both event-dev and ngo-dev clients; [ ] fixture-only warning documented. No Event-repo file is touched.

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

#### Task 8.1: Module + side-service scoping *(extended)*
- **Effort:** L — Now covers: (a) domain modules (events, sessions, registrations, prayer scheduling, spatial, notifications, AI, federation) — keep/repurpose/disable; (b) **self-host product surfaces** (instance onboarding for external operators, deployment-mode/tier selection, white-label configurability) — expected verdict Not applicable per D7, but each needs the recorded-why; (c) side services (Svix, Coop, Osprey, Weblate) — CP's Svix-only + deferred-until-named-consumer resolution (Evidence #23) is the analytical template for islamu.ngo's webhook stance; (d) tenancy semantics (U5): single-tenant vs ISLAMU-projects-as-tenants.
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

## 7. Testing Strategy

Rev-1 table stands, with additions:

| Concern | Instrument | Phase |
|---|---|---|
| Copy fidelity | file-list diff vs source `git ls-files` | 1 |
| Platform integrity | Release build + 13 suites per-project | 2 |
| Deployment split | dual-stack boot, compose config, AppHost run | 3 |
| **Provisioner correctness** | **double-run idempotence; fail-closed-on-missing-realm; Event-clients-untouched diff; shellcheck** | **4** |
| Auth + audience separation | login smoke; negative cross-audience test; two integration suites | 4 |
| Server-only rendering | three Blazor suites; no-WASM network assertion; **component-discipline architecture test (if gap found)** | 5 |
| Docs/context integrity | `Event.Architecture.Tests` docs/link/schema tests | 3,4,6,8 |
| CI | green Actions run | 7 |
| **Production shape** | **compose config validation; ordering dry-run** | **9** |

## 8. Documentation, Configuration, And Operations Impact

- Patched: `CONFIGURATION.md` (identifiers, topology envs, fixture warning), `SECURITY-MODEL.md` (topology + ownership governance), `OPERATIONS.md`, `BLAZOR.md` (render divergence).
- Reframed per D7/Phase 8: `SELF_HOSTING.md`, `DEPLOYMENT_MODES.md`, `DEPLOYMENT_TIERS.md` → internal-ops reference or Not-applicable with recorded why; superseded operationally by new `PRODUCTION_OPERATIONS.md` (Task 9.2).
- New: `docker/keycloak/provision-islamu-ngo-client.sh` + `Dockerfile.provisioner`; `compose.production.yaml`; `dev/report/EVENT_REFERENCE_ALIGNMENT.md`; README provenance section; `bootstrap-sibling-app` intent.
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
| Accessibility | Applicable, no action | Inherited; CP report §12.4's authenticated-browser-evidence idea noted as a future islamu.ngo improvement (alignment doc) |
| White-label/self-host | **Not applicable (changed from Rev 1)** | D7: islamu.ngo is the single supported deployment; white-label machinery is a Phase 8 pruning candidate with recorded why |

## 11. Observability And Operations

Inherited (OTel, health checks, Aspire dashboard). Added: Phase 9 adopts the CP posture — metrics on a management listener not the public port (9.1 verifies Event's support), migration job as readiness gate, low-cardinality discipline already Event policy. Both stacks' telemetry distinguishable via `islamu-ngo-*` service names (Phase 3).

## 12. Migration And Compatibility Plan

Unchanged: fresh `islamu_ngo` DB; full copied migration chain via `Event.MigrationService` (which Phase 9 elevates to the production one-shot gate, matching CP); no data migration; no shims.

## 13. Risk Register

| # | Risk | Likelihood | Impact | Mitigation | Detection signal | Owner task |
|---|---|---|---|---|---|---|
| R1 | Polluted target history kept | Medium (if U1=keep) | Medium | Recommend orphan re-root; backup branch | clone size | 0.1/0.2 |
| R2 | Snapshot from dirty Event tree | Medium | High | Pin committed SHA; `git archive` | file-list diff | 1.1 |
| R3 | Hidden dependency on excluded files | Medium | Medium | Phase 2 green-before-modification gate | failures classed "copy defect" | 2.1/2.2 |
| R4 | Name/volume collision corrupts Event dev data | Medium | High | 3.1 rename table review; dual-stack boot test | Event stack misbehaves | 3.1/3.2 |
| R5 *(reworked)* | Realm drift between repos | **Low (was High)** | Medium | D5 v2: per-repo dev fixtures + additive provisioner; no shared file | dev login failures after realm edits | 4.1/4.2/4.4 |
| R6 | Server-only rendering breaks WASM-assuming path | Medium | Medium | 5.1 investigation first; CP precedent narrows unknowns | Blazor suite/E2E failures | 5.1-5.3 |
| R7 | Audience misconfiguration cross-authorizes the two apps | Low | High | negative test in 4.3 | cross-app token accepted | 4.3 |
| R8 | Integration-test env prerequisites undocumented post-rename | Medium | Low | record in 2.2; rerun after Phase 3 | green in P2, red in P3 | 2.2/3.2 |
| R9 | Scope creep from braindump | Medium | Medium | §1 out-of-scope; Phase 8 investigate-only | untracked tasks appear | all |
| **R10** *(reworked in Rev 2.1)* | **Undeliberate publication of commercial CP internals into public islamu.ngo** — copying is now allowed, so the risk shifts from "legal breach" to "bulk-copying product differentiators without a decision" | Low-Medium | Medium (one-way publication of commercial surface) | C-FLOW per-file deliberation + publication log (Task 8.5); Task 4.2's approved copy sets the template; strip CP-specific identifiers/secrets on every copy | publication log missing entries for CP-origin diffs; CP secrets/URLs/claims in public tree | 4.2, 8.5, all |
| **R11** | **Provisioner run against the shared production realm damages sibling resources** | Low | High | fail-closed on missing realm; owns-only reconciliation; Event-clients-untouched acceptance test; dry-run against dev Keycloak first | realm diff shows non-owned changes | 4.2 |
| **R12** | **Three-repo divergence becomes untracked folklore** (future agents re-import pruned surfaces or "fix" intentional exceptions) | High over time | Medium | D8 categories + Task 8.5 living alignment doc + recorded-why on every verdict | PRs reversing Phase 8 verdicts without citing the doc | 8.1/8.5 |

## 14. Success Metrics And Definition Of Done

Rev-1 items 1-9 stand, amended/added:

5. *(amended)* Login with an existing ISLAMU-realm account via `islamu-ngo-blazor`; API accepts only `islamu-ngo-api` audience; **provisioner job proven idempotent and ownership-scoped against a realm it did not create**.
10. `compose.production.yaml` + `PRODUCTION_OPERATIONS.md` committed and config-validated (Phase 9).
11. `EVENT_REFERENCE_ALIGNMENT.md` exists with seeded divergence ledger **and CP-origin publication log** (Task 8.5).
12. Every CP-origin file in the repo appears in the publication log, carries adapted (non-CP) identifiers, and contains no CP secrets/URLs (R10 sweep).

## 15. Implementation Agent Contract — KEEP DEV DOCS CURRENT

Rev-1 items 1-7 stand. Item 8 replaced and item 9 added:

8. **Cross-repo guard (v3):** this workstream edits only **islamu.ngo**. The Event and Event-Control-Plane repos are read-only sources. Copying from either is permitted (C-FLOW), but every CP-origin copy is a deliberate publication: strip CP-specific identifiers/secrets, note provenance, and add it to the publication log. Never create a build/runtime dependency on either sibling repo.
9. Every pruning/divergence verdict must carry a D8 decision category and land in the alignment doc once Task 8.5 creates it.

## 16. Progress Reporting Contract

Unchanged from Rev 1 (Implemented / Verified / Remaining / Next / Docs updated — teaching-level detail).

## 17. Potential Risks & Unknowns (Candid Critique)

Rev 2 removes the worst Rev-1 seam (shared realm-export drift); Rev 2.1 removes the second one (the provisioner no longer needs to be re-authored — the proven CP script is the basis). What remains: First, **the provisioner is still the most safety-critical artifact** — a bash job holding realm-admin authority pointed at the same realm that serves the commercial product's users and the Event platform's users. Starting from CP's battle-tested script shrinks the implementation risk, but the *adaptation* is where new bugs enter: a missed rename could make islamu.ngo's job reconcile (and "correct") CP-owned clients in the shared realm. The Event-clients-untouched diff test must therefore also assert CP-owned clients untouched, and the first run against any shared (non-dev) Keycloak should be human-supervised. Second, **C-FLOW's remaining failure mode is drift-by-convenience**: with copying legally free in all directions, the cheap path for every future gap is "grab it from CP," which can silently publish commercial surfaces and blur which repo owns which behavior — the publication log only works if agents actually write to it, so PR review should ask "any CP-origin content? logged?" as a standing question. Third, the **non-self-hostable pruning (Phase 8) is where scope discipline will actually be tested** — Event's self-host surfaces are woven through onboarding, settings, docs, and tests, and CP's report shows (three waves of corrections) how often "obviously removable" surfaces turn out to be load-bearing; the decision-category ledger keeps pruning honest, and "just delete the onboarding wizard" is the predictable failure. Finally, Phase 9's assumption that Event's API can bind metrics to a management port the way CP does is **unverified** (Task 9.1) — if Event lacks the split, that's a small change in islamu.ngo, but don't let it silently become "metrics on the public port in production."
