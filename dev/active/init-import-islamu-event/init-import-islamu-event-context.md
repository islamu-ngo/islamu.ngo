<!-- ABOUTME: Resume context for retaining the imported chassis while removing the Event product from islamu.ngo. -->
<!-- ABOUTME: Tracks local-lite development, Event removal, Blog delivery, and the ISLAMU navigation-shell decisions. -->

# Init Import ISLAMU Event — Context

Last Updated: 2026-07-15 Europe/Brussels

## SESSION PROGRESS (2026-07-15 Europe/Brussels)

### ✅ COMPLETED
- 2026-07-13: Investigated Event + islamu.ngo repos; produced plan Revision 1.
- 2026-07-14: **Analyzed the private Event-Control-Plane repo and its `dev/report/EVENT_REFERENCE_ALIGNMENT_REPORT.md` (3,009 lines); produced plan Revision 2 (deep update).** Key intake: config-driven Keycloak topology (3 options), dev-fixture realm vs additive production provisioner job, lean non-self-hostable production compose, InteractiveServer-only precedent, decision-category divergence governance.
- 2026-07-14: **Revision 2.1 — U7 resolved by owner:** full copyright across all three repos; copying allowed in any direction. C-BOUNDARY → **C-FLOW** (per-file deliberate-publication + provenance log + no runtime coupling). Task 4.2 now adapts CP's proven `provision-control-plane-client.sh` directly (effort L→M).

- 2026-07-14 (implementation session): **U1/U2/U3 resolved by user** — U1 = fresh orphan `main`; U2 = islamu.ngo branding; U3 = pin `5cfa996b2344969856946955270dd333e06fc61d` (Event `develop` HEAD; the in-flight webhook migration rework stays uncommitted in Event and is NOT in the snapshot).
- 2026-07-14: **Phase 0 complete** — islamu.ngo `main` re-rooted as orphan (`691f1ba`); old history reachable via `pre-import-backup` and `origin/main` (force-push to origin still pending, user action).
- 2026-07-14: **Phase 1 complete** — snapshot exported via `git archive`, copied with exclusions (Event dev workstream state, journal content, `.claude/settings.local.json`; LICENSE byte-identical so target's kept; target README kept + Provenance section added). Copy fidelity verified by file-list diff (exact). Fresh `dev/_journal/journal.md` + `MAJOR_DECISIONS.md` stubs seeded. Import commit `24e11eb` (7,020 tracked files).

- 2026-07-14: **Phase 2 complete** — Release build exit 0; full per-project sweep done; baseline == source repo exactly (see Validation Baseline table). Copy-defect fix: 8 load-bearing `dev/` files restored.
- 2026-07-14: **Phase 3 applied (3.1–3.3)** — deployment identity fully split (see §Phase 3.1 Identifier Inventory for the scheme: `islamu-ngo-*`, DB `islamu_ngo`, all dev ports +10000). Compose config valid; build green; affected suites green; docs patched. The former dual-stack acceptance was superseded by Revision-3 Task 3.4.
- 2026-07-15: **Revision 3 re-baselined** — owner removed the dual-full-stack gate, made `local-lite` canonical, decided the Event product cluster is deleted, and selected Blog as the first islamu.ngo vertical.
- 2026-07-15: Compared AppHosts. Event always creates MigrationService and forwards Infisical bootstrap under `ExternalInfra`; islamu.ngo currently creates migrations only with a local DB. Task 3.4 records the selective port.
- 2026-07-15: Read iLoveIbadah Blog Domain/repository/CQRS/controller. Reuse its behavior vocabulary only; reject int IDs, Domain DataAnnotations, mutable entities, role-string auth, and incomplete lifecycle.
- 2026-07-15: **Revision 3.1 navigation research complete.** Chrome DevTools MCP inspection of Plane recorded the desktop full-width panel/backdrop, disclosure trigger attributes, four-column/footer-strip anatomy, ~220ms enter/150–220ms exit motion, in-place panel switching, and explicit mobile-menu behavior. Decision: reproduce the interaction in MudBlazor v9 with ISLAMU content and an accessible disclosure pattern, not Plane code/assets/ARIA-menu semantics.

### 🟡 IN PROGRESS
- Revision-3.1 planning complete; no Revision-3 implementation code changes started.

### ⏭️ NEXT
1. Task 3.4: port and prove Event's updated `local-lite` resource graph without overwriting islamu.ngo identifiers.
2. Phase 10: create the Event deletion/caller ledger and verified PostgreSQL backup, then remove presentation/contracts, lower layers/schema, and residue.
3. Phase 11: deliver Blog Domain/persistence, CQRS/API/Cerbos/HAL, Blazor, SEO/docs, and manual acceptance.
4. Phase 12: replace the Event navbar with the ISLAMU mega navigation after Blog routes stabilize.

### ⚠️ BLOCKERS
- None for planning. Applying the destructive Event-removal migration requires a verified cloud PostgreSQL backup.

## Quick Resume
1. Read `init-import-islamu-event-plan.md` (**Revision 3**).
2. Read `init-import-islamu-event-tasks.md`.
3. Work happens in **/home/amir/ISLAMU/Github/islamu.ngo** only. **/home/amir/ISLAMU/Github/Event** and **/home/amir/ISLAMU/Github/Event-Control-Plane** = read-only sources; copying from either is allowed (C-FLOW, owner holds full copyright), but every CP-origin copy is a deliberate publication: strip CP identifiers/secrets, note provenance, add to the publication log (Task 8.5 ledger). No build/runtime dependency on either sibling.
4. After Phase 1, the copied `AGENTS.md` inside islamu.ngo becomes the binding contract there.
5. Every pruning/divergence verdict needs a D8 decision category (Mandatory parity / Reusable pattern / islamu.ngo adaptation / Intentional exception / Not applicable) + recorded why.
6. Canonical command: `dotnet run --project Explore.AppHost/Explore.AppHost.csproj --launch-profile local-lite`. Never run Event and islamu.ngo as two `local-full` stacks.
7. Blog references are read-only: `iLoveIbadah.Application/Features/Blogs`, `BlogsController.cs`, and `iLoveIbadah.Domain/Blog*.cs`.
8. Plane is a behavior/layout reference only. Phase 12 uses ISLAMU copy/assets/routes/tokens and MudBlazor v9; it does not copy Plane code or depend on hover for touch/keyboard access.

## Key Files And Responsibilities

| Path | Existing/New | Repo | Purpose / Notes |
|---|---|---|---|
| `dev/active/braindump.md` | Existing | islamu.ngo | Inspiration only; feeds Task 8.2 |
| `README.md`, `LICENSE` (AGPL v3) | Existing | islamu.ngo | Preserved; README rewritten Phase 6 + provenance section (Task 1.3) |
| `Explore.slnx` (16 src + 13 test projects) | To copy | Event → islamu.ngo | Copied unchanged (D3: no renames) |
| `src/Explore.AppHost/AppHost.cs` | Existing, modify | islamu.ngo | Task 3.4: always-present MigrationService and Infisical-backed `ExternalInfra`; preserve target names/ports |
| `docker/keycloak/realm-export.json` | To copy+modify | Event → islamu.ngo | Becomes islamu.ngo's **dev fixture** (Task 4.1): + `islamu-ngo-*` dev clients; production must never import it |
| `docker/keycloak/keycloak-init.sh` | To copy | Event → islamu.ngo | Copied with the platform; dev-side reference |
| `docker/keycloak/provision-islamu-ngo-client.sh` + `Dockerfile.provisioner` | **New (adapted from CP)** | islamu.ngo | Additive idempotent kcadm job; owns only islamu.ngo clients/scopes/mappers; never creates/deletes realm; fail-closed. Basis: CP's `provision-control-plane-client.sh` (C-FLOW); strip CP-only resources (partner-automation client, `partner_account_id`, `control-plane-operator`); log in publication ledger |
| `src/Explore.Blazor/Components/App.razor` :107-194 | To copy+modify | Event → islamu.ngo | Runtime render policy; fallback already `InteractiveServer(prerender: true)` |
| `src/Explore.Blazor/Program.cs` :75-76, :202-203 | To copy+modify | Event → islamu.ngo | WASM registrations removed in Phase 5 |
| `compose.production.yaml` + `docs/PRODUCTION_OPERATIONS.md` | **New** | islamu.ngo | Phase 9: lean CP-style production posture (external Keycloak, provisioner job, migration gate, management-port metrics) |
| `dev/report/EVENT_REFERENCE_ALIGNMENT.md` | **New** | islamu.ngo | Task 8.5: living divergence ledger (D8) |
| `Event-Control-Plane/docker/keycloak/provision-control-plane-client.sh` + `Dockerfile.provisioner` | **Copy basis** | CP (private) | Proven provisioner; Task 4.2 adapts it (C-FLOW; deliberate publication, logged) |
| `Event-Control-Plane/docs/SECURITY.md` :43-47, :67 | Reference | CP (private) | Keycloak ownership model + production compose ordering |
| `Event-Control-Plane/dev/report/EVENT_REFERENCE_ALIGNMENT_REPORT.md` | Reference | CP (private) | §2 decision categories; §9.4/§10.6/§15.3 InteractiveServer; §15 resolved decisions; text may be reused under C-FLOW |
| `Event-Control-Plane/.env.example` :9-27 | Reference | CP (private) | Topology env surface model (`CONTROL_PLANE_OIDC_AUTHORITY=…/realms/ISLAMU`, admin realm, provisioner client) |
| `src/Explore.Domain/Event*.cs`, Event-family Application/Persistence/API/Blazor surfaces | Existing, delete | islamu.ngo | Phase 10 Event product teardown |
| iLoveIbadah `Features/Blogs`, `BlogsController.cs`, `Domain/Blog*.cs` | Reference | iLoveIbadah | Shape reference only |
| `Blog`, `BlogCategory`, `BlogTag` + Blog CQRS/API/HAL/Blazor | New | islamu.ngo | Phase 11 first product vertical |
| `src/Explore.Blazor.Client/Layout/NavMenu.razor(.cs/.css)` | Existing, redesign | islamu.ngo | Phase 10 removes Event search/create/catalog assumptions; Phase 12 owns trigger strip, overlay integration, route close, and shell state |
| `src/Explore.Blazor.Client/Components/Shell/SiteMegaMenu.razor(.cs/.css)` | New (provisional path) | islamu.ngo | Small MudBlazor v9 disclosure component: one open section, desktop panel/backdrop, focus/pointer lifecycle, responsive mobile projection |
| `src/Explore.Blazor.Client/Layout/MainLayout.razor(.cs/.css)` | Existing, inspect/modify if needed | islamu.ngo | Fixed-header and z-index/overlay coordination; do not push page content when the panel opens |
| `docs/DESIGN.md`, `docs/BLAZOR.md`, `docs/ACCESSIBILITY.md` | Existing, update | islamu.ngo | SiteMegaMenu design-system primitive, shell composition, accessibility/focus/mobile contract |

## Key Decisions (plan §5 — Revision 3)

- **D1:** Snapshot copy at pinned SHA, fresh history. CP's fresh-build alternative considered and rejected (islamu.ngo wants the whole surface).
- **D2:** The snapshot copied everything; Phase 8 classifies divergence and Phase 10 executes the approved Event-product deletion.
- **D3:** No C# renames now; CP's `Event.ControlPlane.*` is the naming precedent for Task 8.3.
- **D4:** Fully InteractiveServer via existing policy system; **keep Event's YARP BFF** (Intentional exception vs CP's opaque-ticket BFF); CP precedent de-risks (report §9.4/§10.6/§15.3).
- **D5 v2 (redesigned; provisioner basis updated in Rev 2.1):** Keycloak = config-driven authority (3 topologies: own / shared-instance-own-realm / same-realm; default = same realm `ISLAMU`) + per-repo dev-fixture realm + additive production provisioner job **adapted from CP's proven script**. **No cross-repo realm-export back-port** (Rev-1 idea dead).
- **D6:** Everything else separate (DB `islamu_ngo`, Cerbos, MinIO, Redis, ports, volumes); cloud-backed `local-lite` is canonical and dual-full-stack is forbidden as a gate.
- **D7:** islamu.ngo is **not self-hostable** — single first-party deployment; self-host product surfaces are classified in Phase 8 and approved removals execute in Phase 10; repo stays public AGPL (posture, not license).
- **D8 (new):** All divergence/pruning verdicts use CP-report decision categories; ledger = Task 8.5 alignment doc.
- **D9 (Revision 3):** Event product verdict is final: Not applicable — delete end to end. Technical event/outbox/webhook/audit names require caller evidence.
- **D10 (Revision 3):** Blog is implemented fresh with UUIDv7 Guid, tenant/audit/soft-delete/concurrency, per-tenant slug, Markdown, optional StorageObject thumbnail, Category/Tag joins, draft/publish, Cerbos, HAL, and Blazor. Likes/comments/views/recommendations deferred.
- **D11 (Revision 3.1):** Build a focused `SiteMegaMenu` disclosure component, not a general menu framework and not a stretched `MudMenu`. Use MudBlazor content primitives plus isolated CSS for the viewport-wide panel; desktop hover/focus and mobile explicit drawer share one static typed ISLAMU content model. Backend-configurable navigation is YAGNI.

## Constraints And Rules To Remember

- **C-FLOW (replaces C-BOUNDARY; owner statement 2026-07-14):** copying between all three repos is legally unrestricted (owner holds full copyright). Remaining discipline: (1) each CP→islamu.ngo copy = deliberate publication (private commercial → public AGPL), logged with provenance in the alignment ledger; (2) strip CP-specific identifiers/secrets on copy (grep: `control-plane-operator`, `partner_account_id`, `control-plane.operator`, CP client ids); (3) no build/runtime dependency on sibling repos.
- **C-PROVISIONER:** additive, idempotent, ownership-scoped; never create/delete realm; never assign roles to users; bootstrap credentials ephemeral; fail closed on missing realm.
- **C-POSTURE:** no third-party-operator surface may be added; pruning only via Phase 8 decisions.
- **C-PROVENANCE:** import commit + README section record source repo/SHA (AGPL→AGPL clean).
- Event Critical Rules apply post-copy (entities-not-DTOs; manual validators; ABOUTME headers; HAL-gated UI; Blazor isolation via `IEventApiClient`).
- Verification: Release build + per-project tests; never solution-level.
- Memory note: `Explore.Blazor.Client` cannot ProjectReference `Explore.Application` (NU1605/WASM); isolation stays under server-only rendering.
- **C-LOCAL-LITE:** no dual-`local-full` acceptance; never expose Infisical/cloud secret values.
- **C-EVENT-DELETE:** no Event compatibility routes/DTOs/HAL/UI/schema aliases; applied migrations remain immutable.
- **C-AMBIGUOUS-EVENT:** do not delete technical event/outbox/webhook/audit infrastructure without caller/ownership evidence.
- **C-BLOG:** API/Cerbos enforces; Blazor gates by HAL; every Blog/taxonomy/thumbnail operation is tenant-safe.
- **C-NAVIGATION:** focus leaving the trigger+panel composite closes it; Escape closes and restores trigger focus; touch has explicit controls; public links use normal navigation semantics; logical CSS/RTL, focus-visible, 200% zoom, and reduced motion are required; no dead routes or API request per hover.

## Revision 3 Product Direction

### Event removal

Delete the Event/EventSession/day/program/agenda/registration/series/template/sync graph, Event taxonomy/custom-property bindings, roles/team, reporting/moderation, repositories/configurations/DbSets/seeds, controllers/routes/HAL/MCP/AI/SEO integrations, Blazor surfaces, Cerbos/OpenAPI/client contracts, tests, schemas, and product docs. Preserve generic domain events, webhook event types, audit events, Category/Tag/StorageObject, and cross-cutting chassis code only with verified non-Event ownership.

### Blog V1

Tenant-scoped UUIDv7 `Blog` with title, unique slug, Markdown, optional StorageObject thumbnail, Category/Tag joins, nullable `PublishedAtUtc`, anonymous published list/detail, authorized create/update/publish/unpublish/delete, Cerbos actions, HAL-gated UI, and safe Markdown rendering. iLoveIbadah is vocabulary/use-case evidence, not copy-ready code.

### ISLAMU navbar / mega navigation

Plane reference capture at a representative 1760px desktop viewport: Product/Solutions/Resources are disclosure buttons with `aria-expanded`; an absolute full-viewport layer begins below the fixed navbar; a white, centered four-column panel (~463px in the observed capture) sits above a translucent page backdrop; a separate lower strip carries an announcement/CTA; adjacent trigger hover swaps content in place. Computed motion was approximately 220ms for enter and 150–220ms for exit using opacity/transform. At 390×844 touch emulation, Plane removes the hover contract and exposes an explicit “Open menu” control with vertically ordered content.

islamu.ngo adaptation: `SiteMegaMenu` uses a disclosure-navigation pattern—trigger buttons plus labelled regions containing headings, lists, and normal links—because this is website navigation, not an application command menu. One open-section state controls all desktop panels. Pointer/focus may move freely between trigger and panel; a cancellable 120–180ms close grace prevents gap flicker. Close on composite focus/pointer exit, backdrop/outside activation, route change, responsive transition, and Escape (with focus restoration). Desktop uses four columns, intermediate widths two, and mobile an explicit MudBlazor drawer/expandable list. ISLAMU tokens, logical CSS, safe viewport scrolling, reduced motion, and existing routes only are non-negotiable.

### Migration order

1. Verify cloud DB and create a restorable backup.
2. Test/apply independent `RemoveEventProductSurface`.
3. Reach a green Event-free chassis.
4. Test/apply independent `AddBlogFoundation`.

## Validation Baseline

Filled 2026-07-14 during Task 2.2 — unmodified copy at `5cfa996b`. Commands:
```bash
dotnet build --configuration Release --verbosity quiet   # exit 0, 26 projects, 0 errors
dotnet test --project tests/<Project>/<Project>.csproj --configuration Release --verbosity quiet
```

| Project | Total | Failed | Verdict |
|---|---|---|---|
| Event.Application.UnitTests | 2243 | 0 | ✅ green |
| Event.Domain.UnitTests | 336 | 0 | ✅ green |
| Event.Architecture.Tests | 275 | 5 | **Pre-existing** — identical 5 fail in Event repo: `Commands_ShouldResideIn_CommandsNamespace`, `CommandHandlers_ShouldResideIn_CommandsNamespace`, `Handlers_ShouldBePublic`, `DTOs_ShouldEndWith_Dto`, `AllLinkPoliciesHaveExplicitPermissionActions` (WebhookConsumerLinkPolicy) — in-flight webhook-redesign fallout |
| Explore.Secrets.UnitTests | 204 | 0 | ✅ green |
| Explore.Blazor.Client.Tests | 1639 | 0 | ✅ green (1 flake on first run, green on rerun) |
| Explore.Diagnostic.UnitTests | 35 | 0 | ✅ green |
| Event.Persistence.IntegrationTests | 333 | 0 | ✅ green (Testcontainers, Docker 29.6.1) |
| Event.API.IntegrationTests | 1769 | 8 | **Pre-existing** — same 8 fail in Event repo (`Public_Get_Endpoints_ReturnOk`, `InstanceAdmin_GetControlPlaneTenantListAndDetail…`, `MissingCerbosEndpoint…`, `AllowAllStub…`, 2× session-registration capacity, `RegularUser…(islamuevent_instance_setting)`, `Governance_InstanceSettings_ResolverConfig…`); plus 1 shared flake (`MissingKeycloakAuthority…`) |
| Explore.Blazor.IntegrationTests | 241 | 0 | ✅ green |
| Explore.Infrastructure.Tests | 794 | 2 | **Pre-existing** — same 2 `ManagedProfile_*` expiry-probe tests fail in Event repo (Event totals 812: its dirty tree has extra uncommitted webhook tests) |
| Explore.Blazor.Client.E2ETests | — | — | Skipped by policy (requires running Aspire AppHost) |
| Event.Benchmarks | — | — | Not a test suite |

**Triage summary:** zero environment failures, zero copy defects (after the 8-file dev restoration below), 15 pre-existing failures identical to source. islamu.ngo == Event baseline exactly.

**Copy-defect fix (2026-07-14):** first architecture run had 4 extra failures because excluded `dev/` files were load-bearing (intents.yaml `must_read_docs`, `ApiConventionTests` reading `authorization-policy-matrix.md`, skill link to `dev/active/README.md`, `docs/BENCHMARKS.md` source anchor). Restored exactly 8 files from the snapshot: `dev/active/README.md`, `dev/active/ai-context-disclosure-policy/{field-classification-matrix,ai-context-disclosure-policy-plan}.md`, `dev/active/webhook-delivery-redesign/*-{context,plan,tasks}.md`, `dev/active/backend-api-health-refactor/authorization-policy-matrix.md`, `dev/pause/performance-optimization/performance-optimization-context.md`. Rationale: the platform arrived mid-webhook-redesign, so those docs are truthful context; restoring beats editing the copied contract files.
Phase 4 adds: provisioner double-run idempotence; fail-closed-on-missing-realm; Event-clients-untouched realm diff; audience negative test. Phase 9 adds: `docker compose -f compose.production.yaml config`.
Revision 3 adds: AppHost graph + real `local-lite` smoke; Event route/HAL/OpenAPI/client/schema absence; Blog tenant/draft/slug/auth/HAL/XSS/persistence/browser coverage; post-removal baseline. Revision 3.1 adds: navbar state/focus/ARIA bUnit coverage plus desktop/mobile/RTL/zoom/reduced-motion browser evidence and accessibility scan.

## Phase 3.1 Identifier Inventory (2026-07-14)

**Scheme:** deployment identifiers `islamu-event-*` → `islamu-ngo-*`; DB → `islamu_ngo`; dev host ports → source port + 10000 (disjoint from Event's range, deterministic). Keycloak client ids (`islamu-event-blazor`, `islamu-event-api`, `islamu-event-public-dev`, secrets, audiences, realm-export, keycloak-init.sh, Onboarding DTO defaults, EventKeycloakIdentityContractContributor, MCP resources) are **Phase 4 — not touched in Phase 3**. User-facing "ISLAMU Event" strings are Phase 6.

| Old | New | Where (file:line) |
|---|---|---|
| 15 volumes `islamu-event-*-data` | `islamu-ngo-*-data` | `AppHost.cs:58,62,69,254,291,324,349,364,409,425,443,524,578,584,614` |
| `islamu-event-db` / `islamu_event_db` | `islamu-ngo-db` / `islamu_ngo` | `AppHost.cs:59`; `docker-compose.yml:33,40`; `Config/pgadmin/servers.json:8` |
| compose project `islamu-event-platform` | `islamu-ngo-platform` | `docker-compose.yml:4` |
| services `islamu-event-api` / `islamu-event-ui` | `islamu-ngo-api` / `islamu-ngo-ui` | `docker-compose.yml:298,353,361,375`; `.env.example:141`; `DockerComposeTopologyDoctorCheck.cs:15,16,56,58` |
| DataProtection app `islamu-event` (+ keyring `islamu-event:data-protection-keys`) | `islamu-ngo` (+ `islamu-ngo:data-protection-keys`) | `DataProtectionServiceCollectionExtensions.cs:34`; `BffDataProtectionExtensions.cs:11,12` |
| cookie `__Host-islamu-event-web` | `__Host-islamu-ngo-web` | `EventBffKeycloakAuthenticationOptions.cs:146` |
| AppHost fixed dev ports (keycloak 8080/9000, crdb 26257/8081, cerbos 3592/3593, minio 9005/9006, svix 8071, weblate 8083, coop 8082/3001, pgadmin 5050, osprey 19950/19951, prometheus 9090, grafana 3000, mailpit 1025/8025) | +10000 each | `AppHost.cs` endpoint literals + `WEBLATE_SITE_DOMAIN`/`COOP_UI_URL`/`CONTROL_PLANE_PUBLIC_ORIGIN` defaults `AppHost.cs:113,194,415,478` |
| compose port defaults (same numbers) | +10000 each | `docker-compose.yml:213,214,255,321,370,396,397,452,453,512,563,597,639,671,672` |
| launchSettings (AppHost 17071-74/15182-85; Blazor 5144/7177; API 5035/7039; UI compose default 7002) | +10000 each | `*/Properties/launchSettings.json`; `docker-compose.yml:370`; `AppHost.cs:113,194` |

## Current Known Risks / Unknowns

- Resolved owner decisions: U1 = orphan history, U2 = islamu.ngo, U3 = snapshot `5cfa996b2344969856946955270dd333e06fc61d`, U7 = cross-repo copying allowed under C-FLOW.
- Phase 8/9 investigations: U4 (side services; CP's Svix-only resolution is the template), U5 (single-tenant vs projects-as-tenants), Task 9.1 (does Event API support management-port metrics binding?).
- Top risks: R11 (provisioner damaging shared-realm sibling resources), R10 (undeliberate publication), R2 (dirty snapshot), R4 (cloud migration ordering), R12 (divergence folklore), and R17 (hover/focus/touch navigation regressions). Full register: plan §13.

## Handoff Notes

### Handoff — 2026-07-15 Europe/Brussels (Revision 3.1)
- **Current state:** plan/context/tasks re-baselined through the Plane-inspired navigation phase; no Revision-3 implementation code.
- **Next action:** Task 3.4, then Phase 10 boundary/backup.
- **Manual QA target:** MigrationService completes and API/Blazor become healthy under `local-lite` with no local infrastructure containers.
- **Sharpest risk:** destructive Event table removal against cloud PostgreSQL.
- **Do not:** wholesale-copy AppHost, run two full stacks, edit applied migrations, copy iLoveIbadah code verbatim, or implement the desktop mega navigation as hover-only/`MudMenu`-only behavior.

### Handoff — 2026-07-14 Europe/Brussels (Phases 0–2 delivered)
- **Current state:** Phases 0, 1, 2 complete on islamu.ngo `main` (orphan root `691f1ba`; import commit `24e11eb` at source SHA `5cfa996b2344969856946955270dd333e06fc61d`; Phase 2 fix/docs commit follows it). Build green; test baseline == source exactly (15 shared pre-existing failures, all webhook-redesign/in-flight fallout — do NOT fix them here; they resolve upstream in Event).
- **Next action:** Phase 3 (identity/deployment split). Start with Task 3.1 identifier inventory: grep `islamu-event`, "ISLAMU Event", port literals; build rename table old → new → file:line in this file.
- **Blockers:** none. Pending user actions: force-push rewritten `main` to origin (U1 consequence); commit the in-flight webhook redesign in Event (islamu.ngo re-syncs later if wanted).
- **Validation:** see Validation Baseline table (filled).
- **Risks:** R4 (volume collision) is the sharpest edge of Phase 3 — Event dev stack and islamu.ngo dev stack must never share volumes/ports; `AppHost.cs:290` volume name and docker-compose project name are the first renames to verify.

### Handoff — 2026-07-14 Europe/Brussels (Rev 2.1) *(superseded)*
- **Current state:** Plan Revision 2.1 complete (CP analysis + owner's copyright clarification integrated); no implementation started; islamu.ngo tree untouched except these dev docs.
- **Next action:** user answers U1/U2/U3 → execute Phase 0.
- **Blockers:** U1, U3.
- **Modified files:** `dev/active/init-import-islamu-event/*` (plan Rev 2 rewritten then amended to 2.1; context + tasks refreshed).
- **Validation:** none yet.
- **Documentation impact:** none yet.
- **Risks:** R11 is the sharpest edge (provisioner adaptation vs shared realm); read plan §17 before touching Phase 4.
- **Notes for next contributor/agent:** (1) Copying from Event-Control-Plane is allowed (C-FLOW) — but strip CP identifiers/secrets, log provenance in the publication ledger, and never add a dependency on the sibling repos. (2) Do not start the copy before Phase 0's re-baseline. (3) The Rev-1 idea of editing/back-porting a shared realm-export.json is dead — do not resurrect it.

### Handoff — 2026-07-13 Europe/Brussels *(superseded)*
- Rev-1 planning complete; superseded by Rev 2 above.
