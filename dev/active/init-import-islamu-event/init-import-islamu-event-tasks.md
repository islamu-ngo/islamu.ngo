<!-- ABOUTME: Execution checklist for retaining the imported chassis while removing the Event product from islamu.ngo. -->
<!-- ABOUTME: Tracks local-lite development, Event deletion, Blog delivery, ISLAMU navigation, and verification. -->

# Init Import ISLAMU Event — Task Checklist

Last Updated: 2026-07-15 Europe/Brussels

## Status Summary
- **Overall status:** IN PROGRESS — Revision 3.1 re-baselined and ready.
- **Completed:** Phases 0–2 and tasks 3.1–3.3.
- **Current priority:** Task 3.4 (canonical `local-lite`).
- **Next product slices:** Phase 10 Event removal, Phase 11 Blog, then Phase 12 ISLAMU navbar/mega navigation.

## Phase 0: Target Repo Re-Baseline ✅ DONE
- [x] **0.1 History decision (U1)** — user chose fresh orphan `main`; old history on `pre-import-backup` + `origin/main` until force-push.
- [x] **0.2 Execute re-baseline** — orphan root commit `691f1ba` (LICENSE, README, .gitignore, dev docs); stale OpenISLAMU solution + committed binaries dropped from the new root.

## Implementation Maintenance Rules
- [ ] Before starting work, read Revision-3 plan / context / tasks.
- [ ] After each completed task, update this checklist immediately.
- [ ] If scope or architecture changes, update the plan before continuing.
- [ ] If discoveries affect future work, update the context file.
- [ ] Final summaries: Implemented / Verified / Remaining / Next / Docs updated.
- [ ] Never modify the Event or Event-Control-Plane repos. Copying from them is allowed (C-FLOW), but every CP-origin copy: strip CP identifiers/secrets, record provenance, add to the publication log; no dependency on sibling repos.
- [ ] Every pruning/divergence verdict carries a D8 decision category + recorded why.

## Phase 1: Snapshot And Copy ✅ DONE
- [x] **1.1 Pin source ref + export snapshot** — pinned `5cfa996b2344969856946955270dd333e06fc61d` (Event `develop` HEAD, 2026-07-14); `git archive` export, 7,122 tracked files.
- [x] **1.2 Copy with exclusions/merges** — excluded: Event `dev/{active,backlog,next,pause,report}/`, `dev/_journal` content (journal.md, MAJOR_DECISIONS.md, dated findings — process templates kept, fresh stubs seeded), `.claude/settings.local.json`, Event LICENSE (byte-identical to target's) and README. Copy-fidelity diff: exact match (7,012 expected vs actual; only `.gitignore` arrived as modify-of-tracked).
- [x] **1.3 Import commit with provenance** — commit `24e11eb` on islamu.ngo `main`; README gained Provenance section (source repo, full SHA, AGPL continuity, divergence pointer).

## Phase 2: Build And Test Green (Unmodified) ✅ DONE
- [x] **2.1 Restore + Release build** — `dotnet build --configuration Release --verbosity quiet` exit 0 (26 projects, 0 errors; warning volume identical in kind to source: analyzers + known AutoMapper/Microsoft.OpenApi advisories).
- [x] **2.2 Full per-project test sweep** — baseline table in context. Verdict: islamu.ngo == Event baseline exactly (15 pre-existing failures shared with source: 5 architecture + 8 API integration + 2 infrastructure; 2 shared flakes; zero environment failures; zero copy defects after restoring 8 load-bearing `dev/` files the intents/tests/docs referenced).

## Phase 3: Identity, Deployment Split, And Canonical `local-lite` 🟡
- [x] **3.1 Identifier inventory** — rename table in context (§Phase 3.1 Identifier Inventory). Keycloak client ids explicitly deferred to Phase 4; product identifiers (MCP URI scheme `islamu-event://`, OpenAPI doc name, `islamu-event-control-plane` provider key, `islamu-event` Keycloak contract name, nswag schema path) deferred to Phase 6/8.
- [x] **3.2 Apply deployment renames** — 15 volumes → `islamu-ngo-*-data`; DB → `islamu-ngo-db`/`islamu_ngo`; compose/services/DataProtection/cookie/ports renamed; compose config, Release build, and affected suites verified. The former dual-stack gate is superseded by Task 3.4 `local-lite`.
- [x] **3.3 Patch identity-touched docs** — OPERATIONS/SELF_HOSTING service+volume+DB names; port shift applied across all `docs/*.md` `localhost:` references (container-internal refs like `mailpit:1025`, `keycloak:8080` untouched); docs-link tests green (Architecture suite).
- [ ] **3.4 Port updated Event `local-lite` behavior** — preserve islamu.ngo names/ports; always add MigrationService under `ExternalInfra`; forward Infisical bootstrap to MigrationService/API/Blazor; gate API/Blazor readiness; assert no local data/platform/heavy resources; update config/ops/troubleshooting docs.
  - Acceptance: graph/diagnostic tests + Release build; real `local-lite` completes cloud migration, API/Blazor healthy, Keycloak login succeeds, no local infra containers.
  - 2026-07-15 evidence: implementation, focused architecture regression, E2E fixture build, and Release build are complete. Bounded `local-lite` QA artifact: `.omo/start-work/artifacts/task-3.4/local-lite-qa-summary.md`.
  - 2026-07-15 blocker: live cloud-backed health remains unverified because external DB migration hit duplicate column `webhook_endpoints.configuration_version` and Blazor requires external `ConnectionStrings:cache`; Keycloak login smoke was not run.
  - 2026-07-15 independent verification: Release build passed; Application 2243 passed; Domain 336 passed; Secrets 204 passed; Persistence 333 passed; Blazor integration 241 passed; Blazor client 1638 passed with 1 skipped; Architecture 268 passed/7 unrelated existing failures/1 skipped; API integration 1760 passed/6 existing failures/3 skipped; no Task 3.4 regression.

## Phase 4: Keycloak Topology Integration (D5 v2) ⏳ NOT STARTED
- [ ] **4.1 Optional `local-full` dev fixture** — extend the copied realm export with islamu.ngo clients; `local-lite`, staging, and production never import it. NO Event-repo edits.
  - Acceptance: optional fresh local Keycloak has islamu.ngo clients; fixture warning documented; `local-lite` contains no Keycloak/import resource. Effort: M.
- [ ] **4.2 Additive provisioner job** — NEW `docker/keycloak/provision-islamu-ngo-client.sh` + `Dockerfile.provisioner`, **copied and adapted from CP's `provision-control-plane-client.sh`** (C-FLOW; deliberate publication, logged). Adapt: env surface → `KEYCLOAK_ISLAMU_NGO_*`; owned resources → `islamu-ngo-blazor`/`islamu-ngo-api`/audience mapper; strip CP-only resources (partner-automation client, `partner_account_id`, `control-plane-operator`); keep hardening verbatim (fail-closed if realm missing, owns-only reconciliation, idempotent, `set -euo pipefail`, `umask 077`); env in `.env.example`.
  - Acceptance: double-run → zero changes; missing realm → hard fail; Event AND CP clients byte-identical before/after; shellcheck clean; no CP-specific identifiers/secrets remain; provenance logged for the 8.5 ledger. Effort: M. Depends: 4.1.
- [ ] **4.3 App config + audience separation** — authority as full realm URL (3 topologies env-only); default `islamu-ngo-*` clients; negative test: `islamu-event-api`-audience token rejected.
  - Acceptance: login round-trip green; `Explore.Blazor.IntegrationTests` + `Event.API.IntegrationTests` green. Effort: M. Depends: 4.1.
- [ ] **4.4 Topology + shared-realm governance doc** — SECURITY-MODEL.md: 3 topologies, chosen default, realm-settings-as-shared-state rule, provisioner ownership boundary, forbidden ops. Effort: S. Depends: 4.2, 4.3.

## Phase 5: Fully InteractiveServer ⏳ NOT STARTED
- [ ] **5.1 Map render-policy system** — `IRuntimeRenderPolicyService` impl + governance-setting source + all `@rendermode` uses + WASM persisted-auth-state paths; ALSO check whether architecture tests already enforce component discipline (no `HttpContext` in reusable components — CP report §9.4 pattern); design note in context. Effort: M.
- [ ] **5.2 Enforce server-only rendering** — policy default InteractiveServer; remove WASM registrations (`Program.cs:75-76,202-203`); map WASM/auto settings to server; add policy-default regression test; add component-discipline test if 5.1 found a gap. Effort: M. Depends: 5.1.
- [ ] **5.3 Render regression sweep** — three Blazor suites; manual smoke; no `_framework/*.wasm` fetch; circuit active; HAL affordances render. Effort: M. Depends: 5.2.

## Phase 6: Branding And Product Identity ⏳ NOT STARTED
- [ ] **6.1 Branding inventory + apply** — user-facing "ISLAMU Event" sweep; theme/footer/SMTP names; README rewrite (incl. provenance section); resolve U2. Effort: M.

## Phase 7: CI/CD Bootstrap ⏳ NOT STARTED
- [ ] **7.1 Workflow audit + adaptation** — keep/adapt/disable table; missing-secret workflows disabled with pointer comment; no deploys.
  - Acceptance: build+test workflow green on test PR. Effort: M.

## Phase 8: Scope And Posture Decisions (investigate/decide only; D8 categories mandatory) ⏳ NOT STARTED
- [ ] **8.1 Chassis + side-service scoping** — Event product verdict is already **Not applicable — delete**. Classify adjacent modules by verified non-Event callers; do not reopen Event removal. Effort: L.
- [ ] **8.2 islamu.ngo feature roadmap** — braindump themes → ranked future workstreams. Effort: M.
- [ ] **8.3 Solution identity rename decision** — go/no-go (CP's `Event.ControlPlane.*` as precedent); if go, dedicated workstream. Effort: S.
- [ ] **8.4 Add `bootstrap-sibling-app` intent** — in copied intents.yaml; manifest tests green. Effort: S.
- [ ] **8.5 Create living alignment doc** — NEW `dev/report/EVENT_REFERENCE_ALIGNMENT.md` (CP report §2/§15 structure; may reuse CP text under C-FLOW): decision-category ledger seeded with server-only rendering, YARP-BFF retention (Intentional exception), non-self-hostable posture, Keycloak topology, all 8.1 verdicts, **plus the CP-origin publication log** (first entry: Task 4.2 provisioner); referenced from islamu.ngo AGENTS.md. Effort: M.

## Phase 9: Production Deployment Posture ⏳ AFTER PHASE 12
- [ ] **9.1 Production topology investigation** — verify Event supports: external-authority-only Keycloak config; management-port metrics binding (unverified!); MigrationService as standalone gate; worker readiness gating. Output: gap list with file evidence. Effort: M.
- [ ] **9.2 Author production compose + ops doc** — NEW `compose.production.yaml` (validator → Postgres → provisioner job → migration gate → API+workers → InteractiveServer BFF; external Keycloak; management-port metrics; no bundled Keycloak/telemetry/backup/proxy) + NEW `docs/PRODUCTION_OPERATIONS.md`. Close S-effort gaps from 9.1; larger gaps → follow-up tasks.
  - Acceptance: `docker compose -f compose.production.yaml config` valid; ordering dry-run against dev Keycloak in "external" mode. Effort: L. Depends: 9.1, Phase 4.

## Phase 10: Delete Event Product Surface ⏳

### 10.1 Boundary + backup
- [ ] Inventory Event artifacts across Domain/Application/Persistence/API/Blazor/policies/contracts/tests/docs/schema.
- [ ] Create technical-event allowlist with caller evidence.
- [ ] Add failing absence checks for canonical Event routes/resources.
- [ ] Capture Event table row counts without tenant data.
- [ ] Create and verify restorable PostgreSQL backup.

### 10.2 Presentation + contracts
- [ ] Delete Event Blazor/admin/navigation/services/tests.
- [ ] Delete Event controllers/routes/HAL/API tests.
- [ ] Delete Event MCP/AI/SEO/calendar/notification integrations.
- [ ] Delete event-only Cerbos actions/resources/policies/modules.
- [ ] Regenerate OpenAPI/NSwag; remove Event operations/models/aliases.
- [ ] Verify no Event HTTP/HAL/navigation/MCP/Cerbos surface.

### 10.3 Lower layers + schema
- [ ] Delete Event/EventSession/dependent Domain types/enums.
- [ ] Delete Event CQRS/validators/contracts/services/specifications/mappings.
- [ ] Delete Event repositories/configs/projections/DbSets/filters/seeds/views/DI.
- [ ] Preserve technical event/outbox/webhook/audit only with evidence.
- [ ] Scaffold `RemoveEventProductSurface`; old migrations untouched.
- [ ] Review/test Up/Down on isolated PostgreSQL before cloud.
- [ ] Verify retained filters/FKs and no pending model.

### 10.4 Green chassis
- [ ] Classify/delete Event tests; retain/replace generic coverage.
- [ ] Update product/architecture/config/ops/schema/README/dev docs.
- [ ] Residual search with explicit allowlist.
- [ ] Release build and remaining project tests pass.
- [ ] `local-lite` shows no Event route/navigation/behavior.
- [ ] Record new baseline.

## Phase 11: Blog Vertical Slice ⏳

### 11.1 Domain + persistence
- [ ] Add tenant-scoped Blog UUIDv7 Guid, audit/soft-delete/concurrency, title/slug/Markdown/thumbnail/`PublishedAtUtc`.
- [ ] Add draft/publish/unpublish/update/delete behavior.
- [ ] Add Blog/category and Blog/tag joins using existing taxonomy.
- [ ] Add entity repository/specification and EF configs/filters/unique tenant+slug/DbSets/DI.
- [ ] Add independent `AddBlogFoundation` migration.
- [ ] Add lifecycle/filter/uniqueness/FK/model tests.

### 11.2 CQRS/API/Cerbos/HAL/contracts
- [ ] Anonymous published list/detail.
- [ ] Authorized create/update/publish/unpublish/delete.
- [ ] Manual validators/cancellation and same-tenant taxonomy/thumbnail checks.
- [ ] Blog authorization actions + Cerbos policy.
- [ ] Named routes/controller/RFC7807 + state-aware HAL.
- [ ] Handler/auth/tenant/draft/slug/HAL/API tests.
- [ ] Regenerate OpenAPI/NSwag after contract stabilization.

### 11.3 Blazor
- [ ] `/blog`, `/blog/{slug}`, authenticated management list/editor.
- [ ] Reuse upload/taxonomy selectors.
- [ ] Disable raw HTML in Markdown; add XSS regression.
- [ ] Gate writes only by HAL.
- [ ] bUnit accessibility/state/validation/affordance tests.

### 11.4 SEO/docs/manual acceptance
- [ ] Published Blog sitemap/metadata; drafts excluded.
- [ ] Update Domain/API/BLAZOR/auth/config/testing/ops docs.
- [ ] `local-lite`: create draft → edit → taxonomy/thumbnail → publish → anonymous read → unpublish/delete.
- [ ] Confirm likes/comments/views/scheduling/revisions/recommendations remain deferred.
- [ ] Refresh dev docs/final baseline.

## Phase 12: ISLAMU Navbar And Mega Navigation ⏳

### 12.0 Plane behavior reference ✅ DONE
- [x] Inspect Plane desktop navigation with Chrome DevTools MCP: disclosure triggers, full-width panel, four-column body, announcement strip, backdrop, in-place switching, close delay, and transform/opacity timing.
- [x] Inspect 390×844 touch/mobile behavior: explicit menu toggle and vertically ordered navigation; record evidence without copying Plane code/assets/copy.

### 12.1 Information architecture + design contract
- [ ] Inventory surviving public/authenticated routes after Event removal and Blog delivery; do not ship placeholder/dead links.
- [ ] Define the ISLAMU desktop groups, item descriptions/icons, mobile order, and optional featured Blog/announcement content.
- [ ] Keep the first content model static/typed in the shell; defer database/CQRS/admin-configurable navigation.
- [ ] Document `SiteMegaMenu` anatomy, tokens, z-index, states, breakpoints, motion, RTL, and focus behavior in `docs/DESIGN.md`; align `docs/BLAZOR.md` and `docs/ACCESSIBILITY.md`.

### 12.2 Desktop disclosure navigation
- [ ] Remove Event search/create/catalog assumptions from `NavMenu` and add the smallest maintainable `SiteMegaMenu.razor(.cs/.css)` component.
- [ ] Use real disclosure buttons with `aria-expanded`/`aria-controls`; panel content remains semantic headings/lists/links, not ARIA application-menu semantics.
- [ ] Open on hover/focus; switch adjacent sections in place; allow pointer/focus movement across the trigger+panel composite.
- [ ] Close with a cancellable 120–180ms grace when pointer/focus leaves the composite; cancel a pending close on re-entry.
- [ ] Close on Escape (restore trigger focus), backdrop/outside activation, route change, breakpoint change, and component disposal.
- [ ] Build a viewport-wide absolute panel below the fixed header: centered four-column desktop body, optional lower strip, safe max-height/scroll, dim backdrop, logical CSS/BEM/tokens, ~220ms enter and 150–220ms exit, reduced-motion fallback.
- [ ] Coordinate z-index/state with user dropdown and other shell overlays; opening a conflicting overlay closes the mega navigation.

### 12.3 Responsive/touch navigation
- [ ] At the design-system breakpoint, replace desktop hover disclosures with an explicit labelled toggle and temporary MudBlazor v9 drawer/overlay.
- [ ] Reuse the same content in touch-sized expandable sections and normal reading order; never require hover.
- [ ] Close on link selection, explicit close, outside activation, Escape/back, and responsive transition; prevent stale overlay/body-scroll state.

### 12.4 Tests + visual/accessibility evidence
- [ ] bUnit: open/switch/close, close-grace cancellation, internal-vs-external focus movement, Escape/focus restoration, route/disposal cleanup, ARIA relationships, and mobile toggle/sections.
- [ ] Browser matrix: 375/390, 768, 1280, and 1760 widths; mouse, keyboard-only, touch emulation; light/dark; English/Arabic RTL; 200% zoom; reduced motion; overflow.
- [ ] Accessibility scan + manual Chrome DevTools comparison: no dead links, console errors, layout shift, hidden focused item, hover-only path, or per-hover API request; focus loss reliably closes and adjacent panels switch without flicker.

## Verification Checklist
- [ ] LSP diagnostics clean for modified files.
- [ ] `dotnet build --configuration Release --verbosity quiet` passes.
- [ ] All 13 test projects pass individually.
- [ ] Architecture/context tests pass after docs/rules/intents changes.
- [ ] `local-lite` graph + real cloud-backed migration/login smoke verified.
- [ ] Login with existing ISLAMU-realm account over InteractiveServer circuit.
- [ ] Provisioner: idempotent double-run; fail-closed on missing realm; Event AND CP clients untouched in shared realm.
- [ ] R10 sweep: every CP-origin file is in the publication log; no leftover CP identifiers/secrets (grep `control-plane-operator`, `partner_account_id`, CP client ids).
- [ ] Docs updated where behavior/config/operations changed.
- [ ] Dev docs (plan/context/tasks) refreshed with final state.
- [ ] Event route/HAL/OpenAPI/client/schema absence checks + residual allowlist pass.
- [ ] Event removal and Blog creation are separate tested migrations.
- [ ] Blog wrong-tenant/draft/slug/HAL/auth/XSS tests and manual Blazor → BFF → API → PostgreSQL/Cerbos flow pass.
- [ ] Mega navigation bUnit + browser matrix pass: pointer/focus/close grace/Escape/focus restore/mobile/RTL/zoom/reduced motion/accessibility.

## Remaining / Deferred Work
- Solution/namespace rename — D3 deferral → Task 8.3.
- All braindump features — Phase 8.2 roadmap outputs.
- Actual production deployment (hosting, DNS, TLS, Coolify wiring) — after Phase 9's spec; future workstream.
- ~~Realm-export.json back-port to Event repo~~ — **dropped in Rev 2** (D5 v2 removes the need).
- CP-inspired future hardening (authenticated-browser evidence layer, operational drills) — candidates for the alignment doc's improvement backlog, not this workstream.
- Blog likes/comments/views/recommendations/scheduling/revisions/syndication/search — deferred until a named requirement.
- Backend-configurable mega-menu CMS, personalization, analytics, and speculative destinations — deferred until a named requirement; Phase 12 ships a typed static ISLAMU route map.
