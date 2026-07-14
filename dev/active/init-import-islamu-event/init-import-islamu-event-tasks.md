<!-- ABOUTME: Task checklist for the init-import-islamu-event workstream (Event platform import into islamu.ngo). -->
<!-- ABOUTME: Tactical tracking file; check items immediately as completed and add discovered tasks. -->

# Init Import ISLAMU Event — Task Checklist

Last Updated: 2026-07-14 Europe/Brussels (Rev 2.1 — implementation started)

## Status Summary
- **Overall status:** IN PROGRESS — U1/U2/U3 resolved by user 2026-07-14 (U1 = fresh orphan main; U2 = islamu.ngo; U3 = pin `5cfa996b`); Phase 0 + Phase 1 complete
- **Completed:** 5/30 (0.1, 0.2, 1.1, 1.2, 1.3)
- **Current priority:** Phase 2 (build + per-project test sweep on unmodified copy)
- **Next recommended slice:** Phase 2 → Phase 3 (identity split)

## Phase 0: Target Repo Re-Baseline ✅ DONE
- [x] **0.1 History decision (U1)** — user chose fresh orphan `main`; old history on `pre-import-backup` + `origin/main` until force-push.
- [x] **0.2 Execute re-baseline** — orphan root commit `691f1ba` (LICENSE, README, .gitignore, dev docs); stale OpenISLAMU solution + committed binaries dropped from the new root.

## Implementation Maintenance Rules
- [ ] Before starting work, read plan (Rev 2) / context / tasks.
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

## Phase 3: Identity And Deployment Split 🟡 MOSTLY DONE (dual-stack boot pending)
- [x] **3.1 Identifier inventory** — rename table in context (§Phase 3.1 Identifier Inventory). Keycloak client ids explicitly deferred to Phase 4; product identifiers (MCP URI scheme `islamu-event://`, OpenAPI doc name, `islamu-event-control-plane` provider key, `islamu-event` Keycloak contract name, nswag schema path) deferred to Phase 6/8.
- [x] **3.2 Apply deployment renames** — 15 volumes → `islamu-ngo-*-data`; DB → `islamu-ngo-db`/`islamu_ngo`; compose project `islamu-ngo-platform`; services `islamu-ngo-api`/`islamu-ngo-ui`; DataProtection app `islamu-ngo` (+keyring); BFF cookie `__Host-islamu-ngo-web`; all dev host ports +10000 (AppHost literals, launchSettings incl. OTLP/resource-service, compose defaults, `.env.example`, browser-facing `*.localhost` URLs); API-base fallbacks 17039; Cerbos default 13593. `docker compose config` valid ✅; Release build exit 0 ✅; Diagnostic 35/35 ✅ (doctor fixture + remediation assertion updated); Blazor.Integration 241/241 ✅; Infrastructure at pre-existing baseline ✅; Architecture same 5 pre-existing ✅. **Dual-stack boot NOT yet run** (attended step: boot Event + islamu.ngo Aspire stacks side by side, verify zero port/volume collisions).
- [x] **3.3 Patch identity-touched docs** — OPERATIONS/SELF_HOSTING service+volume+DB names; port shift applied across all `docs/*.md` `localhost:` references (container-internal refs like `mailpit:1025`, `keycloak:8080` untouched); docs-link tests green (Architecture suite).

## Phase 4: Keycloak Topology Integration (D5 v2) ⏳ NOT STARTED
- [ ] **4.1 Dev-fixture realm** — extend islamu.ngo's copied realm-export.json with `islamu-ngo-blazor`/`islamu-ngo-api`/audience mapper + dev users; mark as dev-only fixture (production never imports); AppHost keeps importing locally. NO Event-repo edits.
  - Acceptance: fresh local Keycloak has event-dev + ngo-dev clients; fixture warning documented. Effort: M.
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
- [ ] **8.1 Module + side-service + posture scoping** — (a) domain modules keep/repurpose/disable; (b) self-host surfaces (instance onboarding, deployment modes, white-label) — expected Not-applicable per D7, recorded why each; (c) side services (Svix/Coop/Osprey/Weblate; CP's Svix-only + defer-until-named-consumer as template); (d) tenancy: single-tenant vs ISLAMU-projects-as-tenants (U5). Effort: L.
- [ ] **8.2 islamu.ngo feature roadmap** — braindump themes → ranked future workstreams. Effort: M.
- [ ] **8.3 Solution identity rename decision** — go/no-go (CP's `Event.ControlPlane.*` as precedent); if go, dedicated workstream. Effort: S.
- [ ] **8.4 Add `bootstrap-sibling-app` intent** — in copied intents.yaml; manifest tests green. Effort: S.
- [ ] **8.5 Create living alignment doc** — NEW `dev/report/EVENT_REFERENCE_ALIGNMENT.md` (CP report §2/§15 structure; may reuse CP text under C-FLOW): decision-category ledger seeded with server-only rendering, YARP-BFF retention (Intentional exception), non-self-hostable posture, Keycloak topology, all 8.1 verdicts, **plus the CP-origin publication log** (first entry: Task 4.2 provisioner); referenced from islamu.ngo AGENTS.md. Effort: M.

## Phase 9: Production Deployment Posture (spec + skeleton, no hosting) ⏳ NOT STARTED
- [ ] **9.1 Production topology investigation** — verify Event supports: external-authority-only Keycloak config; management-port metrics binding (unverified!); MigrationService as standalone gate; worker readiness gating. Output: gap list with file evidence. Effort: M.
- [ ] **9.2 Author production compose + ops doc** — NEW `compose.production.yaml` (validator → Postgres → provisioner job → migration gate → API+workers → InteractiveServer BFF; external Keycloak; management-port metrics; no bundled Keycloak/telemetry/backup/proxy) + NEW `docs/PRODUCTION_OPERATIONS.md`. Close S-effort gaps from 9.1; larger gaps → follow-up tasks.
  - Acceptance: `docker compose -f compose.production.yaml config` valid; ordering dry-run against dev Keycloak in "external" mode. Effort: L. Depends: 9.1, Phase 4.

## Verification Checklist
- [ ] LSP diagnostics clean for modified files.
- [ ] `dotnet build --configuration Release --verbosity quiet` passes.
- [ ] All 13 test projects pass individually.
- [ ] Architecture/context tests pass after docs/rules/intents changes.
- [ ] Dual-stack boot verified (Event + islamu.ngo side by side).
- [ ] Login with existing ISLAMU-realm account over InteractiveServer circuit.
- [ ] Provisioner: idempotent double-run; fail-closed on missing realm; Event AND CP clients untouched in shared realm.
- [ ] R10 sweep: every CP-origin file is in the publication log; no leftover CP identifiers/secrets (grep `control-plane-operator`, `partner_account_id`, CP client ids).
- [ ] Docs updated where behavior/config/operations changed.
- [ ] Dev docs (plan/context/tasks) refreshed with final state.

## Remaining / Deferred Work
- Solution/namespace rename — D3 deferral → Task 8.3.
- All braindump features — Phase 8.2 roadmap outputs.
- Actual production deployment (hosting, DNS, TLS, Coolify wiring) — after Phase 9's spec; future workstream.
- ~~Realm-export.json back-port to Event repo~~ — **dropped in Rev 2** (D5 v2 removes the need).
- CP-inspired future hardening (authenticated-browser evidence layer, operational drills) — candidates for the alignment doc's improvement backlog, not this workstream.
