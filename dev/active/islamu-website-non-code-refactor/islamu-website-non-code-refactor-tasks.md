<!-- ABOUTME: Tracks approval and implementation tasks for the ISLAMU website repository-wide non-code refactor. -->
<!-- ABOUTME: Keeps artifact disposition, cross-workstream dependencies, acceptance criteria, and verification resumable. -->

# ISLAMU Website Non-Code Refactor — Task Checklist

Last Updated: 2026-07-16 Europe/Brussels

## Status Summary

- **Overall status:** In implementation; source-independent slices applied, source-coupled work blocked by init Phases 10-12.
- **Completed execution tasks:** 11/36
- **Planning gates:** 1/1 complete.
- **Current priority:** Complete the Task 0.4 canonical-doc/code-owner handoff; continue only safe source-independent rewrites meanwhile.
- **Next recommended slice:** Prepare the exact replacement list for `DocumentationQualityTests.cs`, then revisit partial tasks after concurrent docs and init Phases 10-12 are complete.
## Implementation Maintenance Rules

- [ ] Before starting work, read plan/context/tasks and the current init-import workstream.
- [ ] Check `git status --short`; never revert or overwrite unrelated/concurrent changes.
- [ ] Expand/update the disposition ledger before modifying or deleting an artifact.
- [ ] Check references, links, source anchors, workflows, tests, and active dev docs before deletion.
- [ ] After each completed task, update this checklist immediately.
- [ ] If scope, architecture, ownership, or risk changes, update the plan before continuing.
- [ ] If discoveries affect future work, update the context and ledger.
- [ ] Do not hand-edit generated contracts or invent legal/privacy commitments.
- [ ] Final slice summaries use Implemented / Verified / Remaining / Next / Docs updated.

## Phase 0: Baseline And Disposition Control — READY

- [x] **0.0 User review and implementation-readiness gate**
  - **Files:** Three workstream docs.
  - **Acceptance:** Product direction is bounded; workstream is marked User-reviewed and ready; implementation has not started.
  - **Validation:** Plan/context/tasks agree on status, next task, capability horizons, and ownership boundary.
  - **Effort:** S
  - **Dependencies:** None.

- [x] **0.1 Refresh contracts, init status, and dirty worktree before first edit**
  - **Files:** `AGENTS.md`, intent/canonical docs, both workstreams, current git status.
  - **Acceptance:** Current init task/phase, shared-file ownership, and dirty paths are recorded; no stale assumption is used.
  - **Validation:** Context handoff updated with current evidence. All three dev docs synchronized. Artifact created at `.omo/start-work/artifacts/task-0.1/refresh-summary.md`.
  - **Effort:** S
  - **Dependencies:** None; planning gate 0.0 is complete.
- [x] **0.2 Expand the disposition ledger to exact tracked paths/globs**
  - **Files:** Root/docs/agent/tool/CI/config/policy/schema/asset/dev non-code families.
  - **Acceptance:** Every in-scope tracked artifact has disposition, rationale, owner, dependency, destination, and evidence; generated/vendor exclusions are explicit.
  - **Validation:** Exact paths or unambiguous globs now cover root/docs/agent/tool/CI/config/policy/schema/asset/dev families; generated/vendor/source-code exclusions and uncertain code-owner rows are explicit.
  - **Effort:** L
  - **Dependencies:** 0.1.
- [x] **0.3 Establish the init Phase 10-12 handoff matrix**
  - **Files:** Both workstreams; source-coupled ledger rows.
  - **Acceptance:** Every shared artifact has one owner/completion signal; no product source task is duplicated here.
  - **Validation:** Context matrix names the source owner, completion signal, and post-handoff non-code action for Event docs, SELF_HOSTING, Cerbos, generated schemas/API, ADR/history, email, Blog/SEO, navigation, and deployment.
  - **Effort:** M
  - **Dependencies:** 0.1, 0.2.

- [ ] **0.4 Pair canonical-doc deletions with guardrail updates**
  - **Files:** `tests/Event.Architecture.Tests/DocumentationQualityTests.cs` as code-owner handoff; docs selected for merge/delete.
  - **Acceptance:** The context now provides the exact remove/add/precondition matrix. The code owner must apply it atomically with deletions; no quality check may be weakened.
  - **Validation:** Architecture project passes with replacement docs present and deleted docs absent.
  - **Effort:** S
  - **Dependencies:** 0.2, 0.3.

## Phase 1: Public, Legal, And Community Entry Points — IN PROGRESS

- [x] **1.1 Rewrite `README.md` for the first-party ISLAMU website**
  - **Files:** `README.md`.
  - **Acceptance:** The digital-front-door purpose is accurate; implemented/planned/deferred capabilities are separated; only implemented destinations are linked; self-host/Event/OpenISLAMU product claims and broken links are gone; source SHA and AGPL provenance remain.
  - **Validation:** Rendered review, link tests, residual allowlist check.
  - **Effort:** M
  - **Dependencies:** 0.3.

- [ ] **1.2 Re-baseline Code of Conduct and security policy** — IMPLEMENTED, OWNER CONFIRMATION PENDING
  - **Files:** `CODE_OF_CONDUCT.md`, `SECURITY-POLICY.md`.
  - **Acceptance:** ABOUTME headers present; identity/contact/scope are accurate; informal or undefined commitments removed; upstream attribution preserved.
  - **Validation:** Policy/links review and owner confirmation.
  - **Effort:** M
  - **Dependencies:** 0.3.

- [ ] **1.3 Re-baseline CLA, contribution governance, workflow, and PR template** — IMPLEMENTED, LEGAL/OWNER APPROVAL PENDING
  - **Files:** `legal/CLA.md`, `docs/legal/CONTRIBUTION_GOVERNANCE.md`, `.github/workflows/cla.yml`, `.github/PULL_REQUEST_TEMPLATE.md`.
  - **Acceptance:** Legal subject/version/signing text/signature path/repository URL agree; existing signatures are not silently invalidated; owner/legal review recorded.
  - **Validation:** YAML/security review, exact-text search, architecture link tests.
  - **Effort:** L
  - **Dependencies:** 0.3 and owner/legal decision.

- [ ] **1.4 Inventory data flows and resolve privacy/cookie publication** — INVENTORY COMPLETE, PUBLICATION APPROVAL PENDING
  - **Files:** `docs/PRIVACY_DATA_INVENTORY.md`; `PRIVACY-POLICY.md` only after approval; README and privacy-relevant docs/config.
  - **Acceptance:** Broken privacy link is absent until approved; any published policy is evidence-backed and matches PostHog/auth/email/Weblate/logging/hosting behavior.
  - **Validation:** Evidence-to-claim and owner/legal review; public link check.
  - **Effort:** L
  - **Dependencies:** 1.1, 4.2.

## Phase 2: Canonical Product And Documentation Architecture — IN PROGRESS

- [x] **2.1 Rewrite `docs/PROJECT.md`**
  - **Files:** `docs/PROJECT.md`.
  - **Acceptance:** Defines one first-party ISLAMU digital front door; marketing leads into trustworthy content and real participation; Foundation/Publish/Engage/Expand horizons are explicit; Event/self-host/white-label and speculative-marketplace promises are absent.
  - **Validation:** Metadata/source anchors and init-state cross-check.
  - **Effort:** M
  - **Dependencies:** 0.4.

- [ ] **2.2 Rewrite surviving architecture and codebase structure** — TRANSITION FRAMING COMPLETE, FINAL INIT HANDOFF PENDING
  - **Files:** `docs/ARCHITECTURE.md`, `docs/CODEBASE_STRUCTURE.md`, surviving diagrams.
  - **Acceptance:** Every named path/symbol exists; Event flows removed; website/Blog flows marked implemented only after init acceptance.
  - **Validation:** Source-anchor test and path search.
  - **Effort:** L
  - **Dependencies:** 0.4; final pass after init 10/11.

- [ ] **2.3 Rebuild docs index, audience model, and docs architecture** — CORE REWRITE COMPLETE, FINAL DISPOSITION PASS PENDING
  - **Files:** `docs/index.md`, `docs/DOCUMENTATION_ARCHITECTURE.md`, `docs/DOCUMENTATION_STYLE_GUIDE.md`.
  - **Acceptance:** Public/contributor/first-party operator routes are distinct; no third-party self-host audience/entrypoint remains; metadata/source-anchor policy survives.
  - **Validation:** Architecture/link tests and manual audience walk-through.
  - **Effort:** L
  - **Dependencies:** 2.1; final pass after Phases 3-7.

- [ ] **2.4 Re-baseline `AGENTS.md`, governance, and quick reference** — FIRST-PARTY FRAMING COMPLETE, PHASE 10 FINAL PASS PENDING
  - **Files:** `AGENTS.md`, `docs/GOVERNANCE.md`, `docs/QUICK_REFERENCE.md`.
  - **Acceptance:** Agent contract names the website; surviving invariants remain accurate; removed Event behavior is not mandated.
  - **Validation:** Full architecture/context tests.
  - **Effort:** L
  - **Dependencies:** 2.1 and init Phase 10 final pass.

- [ ] **2.5 Rewrite contributor setup, testing, and troubleshooting docs**
  - **Files:** `GETTING_STARTED.md`, `CONTRIBUTING.md`, `FIRST_CONTRIBUTION.md`, `TESTING.md`, `TROUBLESHOOTING.md`.
  - **Acceptance:** Current `local-lite`/retained profiles and root-relative commands work; Event journeys removed; no unsupported TUnit filter syntax.
  - **Validation:** Execute documented build/architecture/startup commands.
  - **Effort:** L
  - **Dependencies:** Init Task 3.4; Phases 3-4 evidence.

- [ ] **2.6 Establish the ISLAMU experience and content contract** — DOCUMENT CONTRACT COMPLETE, ROUTE HANDOFF PENDING
  - **Files:** `docs/PROJECT.md`, `docs/DESIGN.md`, `docs/DESIGN_SYSTEM.md`, `docs/ACCESSIBILITY.md`, `docs/LOCALIZATION.md`, `docs/SEO.md`, `docs/RENDER_POLICIES.md`.
  - **Acceptance:** Defines the purpose-to-content-to-participation journey and content/route inventory; only implemented destinations are clickable; no ownerless claims/placeholders; English/French/Arabic+RTL, WCAG 2.2 AA, keyboard/touch/zoom, reduced motion, performance/resilience, truthful SEO, privacy/consent, and editorial ownership are launch gates.
  - **Validation:** Reconcile inventory with implemented routes and init handoffs; manual three-locale journey review; documentation metadata/link/source-anchor checks.
  - **Effort:** L
  - **Dependencies:** 2.1; init Phase 12 for final route states.

## Phase 3: Event And Self-Hosting Documentation Removal — NOT STARTED

- [ ] **3.1 Classify every Event-oriented doc against the init code ledger**
  - **Files:** `docs/*.md`, ADRs, semantic history, generated inventories.
  - **Acceptance:** Each file has caller/owner/deletion evidence; generic domain-event/outbox/webhook concepts are not removed by name alone.
  - **Validation:** Ledger-to-code/reference review.
  - **Effort:** L
  - **Dependencies:** Init 10.1; 0.3.

- [ ] **3.2 Merge and delete unsupported self-host/deployment docs** — DEPLOYMENT MODES/TIERS DELETED, SELF_HOST GUIDE CODE-COUPLED
  - **Files:** `SELF_HOSTING.md`, `DEPLOYMENT_MODES.md`, `DEPLOYMENT_TIERS.md`; destination contributor/operations docs.
  - **Acceptance:** Unique valid local/first-party content moved; unsupported product docs and links removed; paired docs-test list updated by code owner.
  - **Validation:** Architecture/link/source-anchor tests.
  - **Effort:** L
  - **Dependencies:** 0.4, 2.5, init Phase 9/10 evidence.

- [ ] **3.3 Delete Event-only docs and parked docs site in reviewed batches** — PARKED DOCS SITE DELETED, EVENT BATCHES PENDING
  - **Files:** Exact Delete rows under `docs/`; `docs/docs-website/` unless requirement changes.
  - **Acceptance:** Every deletion has zero unresolved inbound references and a ledger rationale; archives are clearly Historical.
  - **Validation:** Architecture tests after each batch.
  - **Effort:** L
  - **Dependencies:** 3.1, 0.4.

- [ ] **3.4 Regenerate or remove Event contract/inventory docs**
  - **Files:** API/agent inventories and generated schema documentation.
  - **Acceptance:** Outputs come from post-Event generators or are deleted with their generator; no hand edits.
  - **Validation:** Generator/snapshot tests and Event route/type residual search.
  - **Effort:** M
  - **Dependencies:** Init 10.2-10.4.

## Phase 4: Retained Infrastructure, Configuration, And Operations — NOT STARTED

- [ ] **4.1 Retain and reframe Weblate localization** — DOCUMENT FRAMING COMPLETE, PROVIDER/CONFIG HANDOFF PENDING
  - **Files:** Localization/config/operations docs, `.env.example`, Compose, Weblate schemas/config.
  - **Acceptance:** Weblate local/production ownership, fallback, health, credentials, and redaction are accurate; Tolgee receives explicit disposition.
  - **Validation:** `docker compose config`, localization/provider generation tests, source anchors.
  - **Effort:** L
  - **Dependencies:** Init provider ledger.

- [ ] **4.2 Retain and reframe PostHog analytics** — PRIVACY INVENTORY COMPLETE, CONCURRENT CONFIG/OPS PASS PENDING
  - **Files:** Analytics sections in config/operations/BLAZOR/security/privacy docs and `.env.example`.
  - **Acceptance:** Existing integration/feature flags/relay/proxy/failure isolation retained; self-host marketplace language removed; no local PostHog service added; privacy defaults match publication.
  - **Validation:** Existing analytics domain/application/infrastructure/client tests and manual consent review.
  - **Effort:** L
  - **Dependencies:** Init analytics retention, 1.4.

- [ ] **4.3 Retain and reframe Mailpit, durable email, and Listmonk** — DOCUMENT BOUNDARY COMPLETE, CALLER HANDOFF PENDING
  - **Files:** Email/outbox/config/operations/testing docs, `.env.example`, Compose/Aspire metadata, Listmonk schema.
  - **Acceptance:** Mailpit is local-only; Event notification callers removed; durable idempotency/retry/redaction preserved; reusable Listmonk waitlist/newsletter path has an owner.
  - **Validation:** Targeted Email/Mailpit tests, Compose/Aspire graph validation, source anchors.
  - **Effort:** L
  - **Dependencies:** Init email/integration ledger.

- [ ] **4.4 Trim configuration, secrets, operations, and troubleshooting**
  - **Files:** `.env.example`, `CONFIGURATION.md`, `SECRETS.md`, `OPERATIONS.md`, `TROUBLESHOOTING.md`, future `PRODUCTION_OPERATIONS.md`.
  - **Acceptance:** Every documented key exists; removed keys have no source owner; settings are grouped by local/first-party/optional integration; examples contain no secrets.
  - **Validation:** Config-binding/startup tests, Compose validation, exact key searches.
  - **Effort:** XL
  - **Dependencies:** 3.1, 4.1-4.3, init Phase 9/10.

## Phase 5: Agent Context And Tooling — IN PROGRESS

- [x] **5.1 Rewrite canonical agent entrypoints and planning/task commands**
  - **Files:** `AGENTS.md`, selected `.claude/commands`, hook config, Copilot instructions.
  - **Acceptance:** Agents receive website context, not Event/self-host defaults; contribution/verification contracts remain.
  - **Validation:** Canonical entrypoints, commands, hook configuration, Copilot instructions, and non-code mirrors were inspected and re-baselined; formal test execution remains deferred by explicit user instruction.
  - **Effort:** L
  - **Dependencies:** 2.1-2.4.

- [ ] **5.2 Re-baseline intents and path rules** — SELF-HOST DOC CONTRACT REMOVED, EVENT/DELETED-WORKSTREAM PATHS PENDING
  - **Files:** `.claude/contract/intents.yaml`, `.claude/rules/*.md`, benchmarks/indexes.
  - **Acceptance:** No intent targets deleted Event files; recurring non-code re-baseline intent added if justified; paths/docs/tests/acceptance/forbidden fields resolve.
  - **Validation:** Intent manifest/schema/duplication tests.
  - **Effort:** L
  - **Dependencies:** Init Phase 10, 5.1.

- [x] **5.3 Audit skills, agents, and mirrored context**
  - **Files:** `.agents/**`, `.claude/agents/**`, `.codex/**`.
  - **Acceptance:** Reusable guidance retained; removed skills have zero inbound links; mirrors synchronized; pedagogical examples are not mechanically churned.
  - **Validation:** Active project-specific CTO/consultation guidance and non-code mirrors are first-party accurate; generic I-VSD framework examples are retained deliberately; code hook mirrors remain code-owner scope.
  - **Effort:** L
  - **Dependencies:** 5.1, 5.2.

- [x] **5.4 Audit auxiliary tool configuration**
  - **Files:** `.opencode/**`, `.design-sync/**`, `.repomix/**`, `.ci/**`.
  - **Acceptance:** Repo-specific identity/paths are accurate; generic/vendored wrappers remain unless broken.
  - **Validation:** `.opencode` vendored/generated content is explicitly excluded; `.design-sync`, `.repomix`, and `.ci` documentation contain no stale product identity; `.ci/scripts/**` remains code-owner scope.
  - **Effort:** M
  - **Dependencies:** 5.1-5.3.

## Phase 6: GitHub, CI, Release, And Deployment Metadata — IN PROGRESS

- [x] **6.1 Re-baseline templates, CODEOWNERS, and repository settings**
  - **Files:** `.github/ISSUE_TEMPLATE/**`, PR template, CODEOWNERS, repository settings workflow.
  - **Acceptance:** Website work routes correctly; CLA/docs/validation fields are accurate; stale Event URLs removed except provenance.
  - **Validation:** YAML parse and link checks.
  - **Effort:** M
  - **Dependencies:** 1.3, 2.3.

- [ ] **6.2 Audit CI, security, contract, and release workflows** — CLA WORKFLOW UPDATED, REMAINING WORKFLOWS PENDING
  - **Files:** Build/test/security/context/Cerbos/OpenAPI/release reusable workflows.
  - **Acceptance:** Security and quality gates remain least-privilege and execute surviving projects; removed contract jobs have no generator.
  - **Validation:** Workflow security tests, YAML parsing, command parity review.
  - **Effort:** L
  - **Dependencies:** Init Phase 10; 3.4, 5.2.

- [ ] **6.3 Resolve first-party deploy workflows**
  - **Files:** `deploy-coolify*.yml`, `e2e.yml`, production compose/docs.
  - **Acceptance:** One approved production ownership path exists; obsolete workflows are disabled/deleted with rationale; no self-host product promise remains.
  - **Validation:** Workflow parse/security and non-mutating deployment dry-run/config check.
  - **Effort:** L
  - **Dependencies:** Init Phase 9/10.

## Phase 7: Policies, Schemas, Generated Artifacts, Assets, And History — IN PROGRESS

- [ ] **7.1 Reconcile Cerbos policies/schemas/tests with surviving resources**
  - **Files:** `cerbos/**`.
  - **Acceptance:** Every resource has code/policy ownership; Event policies removed with source; Blog policies added by init owner; parity remains.
  - **Validation:** Cerbos workflow/tests and authorization-parity architecture tests.
  - **Effort:** L
  - **Dependencies:** Init Phase 10/11.

- [ ] **7.2 Regenerate/delete Event database/OpenAPI contracts and retain provider schemas**
  - **Files:** `schemas/**`, generator configs/output references.
  - **Acceptance:** Current outputs contain no removed Event tables/endpoints; Weblate/Listmonk schemas match surviving adapters; Tolgee disposition explicit; no hand edits.
  - **Validation:** Generator/snapshot tests.
  - **Effort:** L
  - **Dependencies:** Init Phase 10/11; 4.1, 4.3.

- [x] **7.3 Delete unreferenced Event screenshots/assets**
  - **Files:** `assets/**`, docs images/diagrams.
  - **Acceptance:** ISLAMU logos/current assets retained; every deletion has zero inbound refs; stale Event visuals gone.
  - **Validation:** Reference search and rendered docs/UI smoke.
  - **Effort:** M
  - **Dependencies:** Init Phase 10/12; 1.1, 2.3.

- [x] **7.4 Preserve only useful historical and dev records**
  - **Files:** `dev/**`, semantic history, old reports/ADRs.
  - **Acceptance:** Active/durable/provenance records remain; historical content is labeled; no other agent's dirty files are changed.
  - **Validation:** Journal and semantic history are retained as history rather than current navigation; owner-controlled active/paused work remains untouched; three unreferenced Event-only I-VSD analyses were removed.
  - **Effort:** M
  - **Dependencies:** 0.2, 3.1.

## Phase 8: Repository-Wide Convergence And Handoff — NOT STARTED

- [ ] **8.1 Audit residual identity and unsupported product claims**
  - **Files:** All tracked non-code artifacts.
  - **Acceptance:** Zero unexplained Event/OpenISLAMU/self-host/white-label/multi-tenant/provider residuals; allowlist records path/term/reason.
  - **Validation:** Repeatable `rg` commands from plan Section 14.
  - **Effort:** M
  - **Dependencies:** Phases 1-7.

- [ ] **8.2 Run reference, format, build, test, policy, and generator gates**
  - **Files:** All modified artifacts.
  - **Acceptance:** New failures/errors/warnings are zero; accepted pre-existing debt is unchanged and documented.
  - **Validation:** Release build, architecture project, Compose validation, and all targeted gates from changed families.
  - **Effort:** L
  - **Dependencies:** 8.1.

- [ ] **8.3 Perform public/contributor/operator/agent audience walk-through and final handoff**
  - **Files:** README, docs index, setup/ops/privacy/security/legal entrypoints, three workstream docs.
  - **Acceptance:** Four audience journeys reach accurate content with no dead links, false features, secrets, or unsupported deployment guidance; plan/context/tasks match final state.
  - **Validation:** QA notes and dated handoff recorded in context.
  - **Effort:** M
  - **Dependencies:** 8.2.

## Verification Checklist

- [ ] Modified/new files satisfy ABOUTME requirements.
- [ ] Markdown links and source anchors resolve.
- [ ] Canonical docs have valid metadata/status/owner/date/source anchors.
- [ ] No unsupported TUnit `dotnet test --filter` examples remain.
- [ ] `dotnet build --configuration Release --verbosity quiet` passes.
- [ ] `dotnet test --project tests/Event.Architecture.Tests/Event.Architecture.Tests.csproj --configuration Release --verbosity quiet` passes.
- [ ] `docker compose config` passes when Compose/env metadata changes.
- [ ] Cerbos tests/parity pass when policy artifacts change.
- [ ] OpenAPI/schema generators and snapshots pass when generated artifacts change.
- [ ] Targeted Weblate/PostHog/Mailpit/email/Listmonk tests pass for affected slices.
- [ ] Workflow YAML/security checks pass for `.github` changes.
- [ ] Residual identity/product-claim searches have zero unexplained matches.
- [ ] Disposition ledger covers every in-scope tracked artifact.
- [ ] Content/route truth inventory covers every public claim, route, navigation item, CTA, locale, analytics event, and email capture.
- [ ] Planned/Deferred capabilities have no clickable route, CTA, or navigation placeholder.
- [ ] ISLAMU experience gates cover multilingual/RTL, accessibility, reduced motion, performance/resilience, truthful SEO, consent, and editorial ownership.
- [ ] Plan/context/tasks and final handoff are current.

## Remaining / Deferred Work

- Product source deletion, migrations, Blog, and navigation remain in `dev/active/init-import-islamu-event/`.
- Production hosting/DNS/TLS/Coolify execution is outside this workstream; only truthful metadata/docs disposition is included.
- A local PostHog stack is intentionally not planned; add only if a concrete local/offline analytics requirement appears.
- Tolgee is not deleted until the code ledger resolves its surviving caller/owner.
- Privacy/CLA/security commitment changes remain approval-gated.
