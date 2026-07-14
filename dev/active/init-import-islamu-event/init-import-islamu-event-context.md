<!-- ABOUTME: Operational memory for the init-import-islamu-event workstream (Event platform import into islamu.ngo). -->
<!-- ABOUTME: Holds session progress, key decisions, validation baseline, and handoff notes for resuming agents. -->

# Init Import ISLAMU Event — Context

Last Updated: 2026-07-14 Europe/Brussels

## SESSION PROGRESS (2026-07-14 Europe/Brussels)

### ✅ COMPLETED
- 2026-07-13: Investigated Event + islamu.ngo repos; produced plan Revision 1.
- 2026-07-14: **Analyzed the private Event-Control-Plane repo and its `dev/report/EVENT_REFERENCE_ALIGNMENT_REPORT.md` (3,009 lines); produced plan Revision 2 (deep update).** Key intake: config-driven Keycloak topology (3 options), dev-fixture realm vs additive production provisioner job, lean non-self-hostable production compose, InteractiveServer-only precedent, decision-category divergence governance.
- 2026-07-14: **Revision 2.1 — U7 resolved by owner:** full copyright across all three repos; copying allowed in any direction. C-BOUNDARY → **C-FLOW** (per-file deliberate-publication + provenance log + no runtime coupling). Task 4.2 now adapts CP's proven `provision-control-plane-client.sh` directly (effort L→M).

- 2026-07-14 (implementation session): **U1/U2/U3 resolved by user** — U1 = fresh orphan `main`; U2 = islamu.ngo branding; U3 = pin `5cfa996b2344969856946955270dd333e06fc61d` (Event `develop` HEAD; the in-flight webhook migration rework stays uncommitted in Event and is NOT in the snapshot).
- 2026-07-14: **Phase 0 complete** — islamu.ngo `main` re-rooted as orphan (`691f1ba`); old history reachable via `pre-import-backup` and `origin/main` (force-push to origin still pending, user action).
- 2026-07-14: **Phase 1 complete** — snapshot exported via `git archive`, copied with exclusions (Event dev workstream state, journal content, `.claude/settings.local.json`; LICENSE byte-identical so target's kept; target README kept + Provenance section added). Copy fidelity verified by file-list diff (exact). Fresh `dev/_journal/journal.md` + `MAJOR_DECISIONS.md` stubs seeded. Import commit `24e11eb` (7,020 tracked files).

- 2026-07-14: **Phase 2 complete** — Release build exit 0; full per-project sweep done; baseline == source repo exactly (see Validation Baseline table). Copy-defect fix: 8 load-bearing `dev/` files restored.
- 2026-07-14: **Phase 3 applied (3.1–3.3)** — deployment identity fully split (see §Phase 3.1 Identifier Inventory for the scheme: `islamu-ngo-*`, DB `islamu_ngo`, all dev ports +10000). Compose config valid; build green; affected suites green; docs patched. **Dual-stack boot acceptance still pending** (attended).

### 🟡 IN PROGRESS
- Phase 3 acceptance: dual-stack boot (Event + islamu.ngo side by side) — attended step, not yet run.

### ⏭️ NEXT
1. Run dual-stack boot verification (user-attended): start Event AppHost + islamu.ngo AppHost; confirm zero port/volume collisions; Event dev data intact (R4).
2. Phase 4 (Keycloak topology, D5 v2) — read plan §17 and R11 before touching the provisioner; Task 4.2 copies CP's provisioner (C-FLOW: strip CP identifiers, log publication).
3. Update this file after each slice.

### ⚠️ BLOCKERS
- None. (Force-push of rewritten `main` to origin is a pending user action, not a work blocker.)

## Quick Resume
1. Read `init-import-islamu-event-plan.md` (**Revision 2** — note the revision banner).
2. Read `init-import-islamu-event-tasks.md`.
3. Work happens in **/home/amir/ISLAMU/Github/islamu.ngo** only. **/home/amir/ISLAMU/Github/Event** and **/home/amir/ISLAMU/Github/Event-Control-Plane** = read-only sources; copying from either is allowed (C-FLOW, owner holds full copyright), but every CP-origin copy is a deliberate publication: strip CP identifiers/secrets, note provenance, add to the publication log (Task 8.5 ledger). No build/runtime dependency on either sibling.
4. After Phase 1, the copied `AGENTS.md` inside islamu.ngo becomes the binding contract there.
5. Every pruning/divergence verdict needs a D8 decision category (Mandatory parity / Reusable pattern / islamu.ngo adaptation / Intentional exception / Not applicable) + recorded why.

## Key Files And Responsibilities

| Path | Existing/New | Repo | Purpose / Notes |
|---|---|---|---|
| `dev/active/braindump.md` | Existing | islamu.ngo | Inspiration only; feeds Task 8.2 |
| `README.md`, `LICENSE` (AGPL v3) | Existing | islamu.ngo | Preserved; README rewritten Phase 6 + provenance section (Task 1.3) |
| `Explore.slnx` (16 src + 13 test projects) | To copy | Event → islamu.ngo | Copied unchanged (D3: no renames) |
| `src/Explore.AppHost/AppHost.cs` | To copy+modify | Event → islamu.ngo | Keycloak dev wiring :261-316, client defaults :653-661, volume :290 |
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

## Key Decisions (plan §5 — Rev 2)

- **D1:** Snapshot copy at pinned SHA, fresh history. CP's fresh-build alternative considered and rejected (islamu.ngo wants the whole surface).
- **D2:** Copy EVERYTHING; prune only in Phase 8 via D8 categories.
- **D3:** No C# renames now; CP's `Event.ControlPlane.*` is the naming precedent for Task 8.3.
- **D4:** Fully InteractiveServer via existing policy system; **keep Event's YARP BFF** (Intentional exception vs CP's opaque-ticket BFF); CP precedent de-risks (report §9.4/§10.6/§15.3).
- **D5 v2 (redesigned; provisioner basis updated in Rev 2.1):** Keycloak = config-driven authority (3 topologies: own / shared-instance-own-realm / same-realm; default = same realm `ISLAMU`) + per-repo dev-fixture realm + additive production provisioner job **adapted from CP's proven script**. **No cross-repo realm-export back-port** (Rev-1 idea dead).
- **D6:** Everything else separate (DB `islamu_ngo`, Cerbos, MinIO, Redis, ports, volumes); dual-stack dev.
- **D7 (new):** islamu.ngo is **not self-hostable** — single first-party deployment; self-host product surfaces are Phase 8 pruning candidates with recorded why; repo stays public AGPL (posture, not license).
- **D8 (new):** All divergence/pruning verdicts use CP-report decision categories; ledger = Task 8.5 alignment doc.

## Constraints And Rules To Remember

- **C-FLOW (replaces C-BOUNDARY; owner statement 2026-07-14):** copying between all three repos is legally unrestricted (owner holds full copyright). Remaining discipline: (1) each CP→islamu.ngo copy = deliberate publication (private commercial → public AGPL), logged with provenance in the alignment ledger; (2) strip CP-specific identifiers/secrets on copy (grep: `control-plane-operator`, `partner_account_id`, `control-plane.operator`, CP client ids); (3) no build/runtime dependency on sibling repos.
- **C-PROVISIONER:** additive, idempotent, ownership-scoped; never create/delete realm; never assign roles to users; bootstrap credentials ephemeral; fail closed on missing realm.
- **C-POSTURE:** no third-party-operator surface may be added; pruning only via Phase 8 decisions.
- **C-PROVENANCE:** import commit + README section record source repo/SHA (AGPL→AGPL clean).
- Event Critical Rules apply post-copy (entities-not-DTOs; manual validators; ABOUTME headers; HAL-gated UI; Blazor isolation via `IEventApiClient`).
- Verification: Release build + per-project tests; never solution-level.
- Memory note: `Explore.Blazor.Client` cannot ProjectReference `Explore.Application` (NU1605/WASM); isolation stays under server-only rendering.

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

- Open user decisions: U1 (history), U2 (domain), U3 (snapshot SHA). U7 resolved (copying allowed; C-FLOW).
- Phase 8/9 investigations: U4 (side services; CP's Svix-only resolution is the template), U5 (single-tenant vs projects-as-tenants), Task 9.1 (does Event API support management-port metrics binding?).
- Top risks: R11 (provisioner damaging shared-realm sibling resources — adaptation must not leave CP-owned resources in its reconcile set; first shared-realm run human-supervised), R10 reworked (undeliberate publication of commercial internals — publication log discipline), R2 (dirty-tree snapshot), R4 (volume collision corrupting Event dev data), R12 (divergence folklore — alignment ledger). Full register: plan §13.

## Handoff Notes

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
