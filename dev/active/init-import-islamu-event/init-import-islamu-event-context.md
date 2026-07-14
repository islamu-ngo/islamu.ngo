<!-- ABOUTME: Operational memory for the init-import-islamu-event workstream (Event platform import into islamu.ngo). -->
<!-- ABOUTME: Holds session progress, key decisions, validation baseline, and handoff notes for resuming agents. -->

# Init Import ISLAMU Event — Context

Last Updated: 2026-07-14 Europe/Brussels

## SESSION PROGRESS (2026-07-14 Europe/Brussels)

### ✅ COMPLETED
- 2026-07-13: Investigated Event + islamu.ngo repos; produced plan Revision 1.
- 2026-07-14: **Analyzed the private Event-Control-Plane repo and its `dev/report/EVENT_REFERENCE_ALIGNMENT_REPORT.md` (3,009 lines); produced plan Revision 2 (deep update).** Key intake: config-driven Keycloak topology (3 options), dev-fixture realm vs additive production provisioner job, lean non-self-hostable production compose, InteractiveServer-only precedent, decision-category divergence governance.
- 2026-07-14: **Revision 2.1 — U7 resolved by owner:** full copyright across all three repos; copying allowed in any direction. C-BOUNDARY → **C-FLOW** (per-file deliberate-publication + provenance log + no runtime coupling). Task 4.2 now adapts CP's proven `provision-control-plane-client.sh` directly (effort L→M).

### 🟡 IN PROGRESS
- Awaiting user review of plan Rev 2.1 (decisions U1-U3 remain).

### ⏭️ NEXT
1. User answers U1 (history), U2 (domain), U3 (snapshot SHA).
2. First implementation slice: Phase 0 + Phase 1.
3. Update this file after each slice.

### ⚠️ BLOCKERS
- U1: bloated history vs fresh orphan `main` (recommendation: orphan).
- U3: Event tree dirty; pin a committed SHA (currently `b1fadaa9` on `develop`) or wait for the webhook-redesign commit.

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

Filled during Task 2.2 — per-project pass/fail for the 13 copied test projects. Commands:
```bash
dotnet build --configuration Release --verbosity quiet
dotnet test --project tests/<Project>/<Project>.csproj --configuration Release --verbosity quiet
```
Phase 4 adds: provisioner double-run idempotence; fail-closed-on-missing-realm; Event-clients-untouched realm diff; audience negative test. Phase 9 adds: `docker compose -f compose.production.yaml config`.

## Current Known Risks / Unknowns

- Open user decisions: U1 (history), U2 (domain), U3 (snapshot SHA). U7 resolved (copying allowed; C-FLOW).
- Phase 8/9 investigations: U4 (side services; CP's Svix-only resolution is the template), U5 (single-tenant vs projects-as-tenants), Task 9.1 (does Event API support management-port metrics binding?).
- Top risks: R11 (provisioner damaging shared-realm sibling resources — adaptation must not leave CP-owned resources in its reconcile set; first shared-realm run human-supervised), R10 reworked (undeliberate publication of commercial internals — publication log discipline), R2 (dirty-tree snapshot), R4 (volume collision corrupting Event dev data), R12 (divergence folklore — alignment ledger). Full register: plan §13.

## Handoff Notes

### Handoff — 2026-07-14 Europe/Brussels (Rev 2.1)
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
