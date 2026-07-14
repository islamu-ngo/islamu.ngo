<!-- ABOUTME: Task checklist for the init-import-islamu-event workstream (Event platform import into islamu.ngo). -->
<!-- ABOUTME: Tactical tracking file; check items immediately as completed and add discovered tasks. -->

# Init Import ISLAMU Event — Task Checklist

Last Updated: 2026-07-14 Europe/Brussels (Rev 2.1)

## Status Summary
- **Overall status:** Draft, Revision 2.1 (awaiting user review; U7 resolved — copying between repos allowed, C-FLOW)
- **Completed:** 0/30
- **Current priority:** User decisions U1 (history), U2 (domain), U3 (snapshot SHA)
- **Next recommended slice:** Phase 0 (re-baseline) + Phase 1 (snapshot & copy) in one session

## Implementation Maintenance Rules
- [ ] Before starting work, read plan (Rev 2) / context / tasks.
- [ ] After each completed task, update this checklist immediately.
- [ ] If scope or architecture changes, update the plan before continuing.
- [ ] If discoveries affect future work, update the context file.
- [ ] Final summaries: Implemented / Verified / Remaining / Next / Docs updated.
- [ ] Never modify the Event or Event-Control-Plane repos. Copying from them is allowed (C-FLOW), but every CP-origin copy: strip CP identifiers/secrets, record provenance, add to the publication log; no dependency on sibling repos.
- [ ] Every pruning/divergence verdict carries a D8 decision category + recorded why.

## Phase 1: Snapshot And Copy ⏳ NOT STARTED
- [ ] **1.1 Pin source ref + export snapshot** — committed SHA only (git archive semantics); record SHA in context. Effort: S.
- [ ] **1.2 Copy with exclusions/merges** — exclude Event `dev/active/*` workstreams, journal, tool-local state; merge LICENSE/README/dev. Effort: M. Depends: 1.1.
- [ ] **1.3 Import commit with provenance** — commit message + README provenance/notice section (source repo, SHA, AGPL statement — C-PROVENANCE). Effort: S. Depends: 1.2.

## Phase 2: Build And Test Green (Unmodified) ⏳ NOT STARTED
- [ ] **2.1 Restore + Release build** — exits 0 with lock files intact. Effort: S.
- [ ] **2.2 Full per-project test sweep** — 13 projects individually; baseline table in context; triage environment / copy defect / pre-existing. Effort: M. Depends: 2.1.

## Phase 3: Identity And Deployment Split ⏳ NOT STARTED
- [ ] **3.1 Identifier inventory** — grep `islamu-event`, "ISLAMU Event", port literals; rename table (old → new → file:line) in context. Effort: M.
- [ ] **3.2 Apply deployment renames** — services, volumes (incl. `AppHost.cs:290`), compose project, ports, DB `islamu_ngo`. NO C# namespace changes.
  - Acceptance: `docker compose config` valid; dual-stack boot, zero collisions. Effort: L. Depends: 3.1.
- [ ] **3.3 Patch identity-touched docs** — CONFIGURATION/SELF_HOSTING/OPERATIONS sections; docs-link tests green. Effort: M. Depends: 3.2.

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
