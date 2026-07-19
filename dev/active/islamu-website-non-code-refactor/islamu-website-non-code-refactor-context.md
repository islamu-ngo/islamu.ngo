<!-- ABOUTME: Stores the resumable state and canonical artifact-disposition ledger for the ISLAMU website non-code refactor. -->
<!-- ABOUTME: Records ownership boundaries, verified evidence, risks, validation, and the next implementation action. -->

# ISLAMU Website Non-Code Refactor — Context

Last Updated: 2026-07-19 Europe/Brussels

## SESSION PROGRESS (2026-07-19 Europe/Brussels)

### COMPLETED

- Planning workstream created with a source-grounded current-state report, fallback contract, phased implementation tasks, and verification gates.
- Canonical agent/product/documentation authorities and active init-import workstream read.
- Release baseline run: 0 errors, 89 warnings in 10.14s.
- Architecture/documentation validation run after creating the planning files: 268 passed, 7 failed, 1 skipped. No failure names any file in this workstream; the failures expose pre-existing/concurrent webhook intent/HAL, deleted performance source-anchor, CQRS namespace/public-handler, and DTO naming issues.
- Repository-wide non-code families inventoried; Event/self-host terminology and retained infrastructure evidence sampled across docs, agent context, GitHub metadata, configuration, policies, schemas, and assets.
- Weblate, PostHog, and Mailpit retention confirmed as binding. PostHog code/config/tests exist, but no PostHog Compose service exists.
- Ownership boundary established: this workstream owns non-code disposition/content; init-import owns product source/runtime/migration/generation and Phases 10-12.
- Parallel audits completed and independently checked. Their only material addition is the optional `local-full` pgAdmin configuration: `src/Explore.AppHost/Config/pgadmin/pgpass` contains fixed development database passwords and is mounted by `AppHost.cs`; its retention or removal follows the init runtime profile decision.
- Senior CTO review completed. The workstream is user-reviewed with a bounded ISLAMU digital-front-door north star, capability horizons, and content/route truth rules.
- Senior CTO rewrite revalidated: Release build passed with 0 errors and the same 89 warnings in 10.59s; architecture/docs tests reproduced the same 268 passed, 7 failed, 1 skipped baseline with no failure in these workstream files.
- **Task 0.1 refresh correction:** Context7 was available and official `/microsoft/aspire.dev` guidance was consulted; Tavily MCP alone was unavailable. Init Phases 10 and 11 remain unstarted. Phase 12 implementation is unstarted, but Task 12.0 research is complete. Task 3.4 targeted checks passed while the canonical sweep retained 7 architecture and 6 API failures unrelated to Task 3.4. The earlier delegated refresh artifact remains rejected evidence and must not be used as authority.
- Direct implementation began after the user explicitly requested implementation without further tests, builds, project startup, or Aspire runs. This overrides the original Phase 0 sequencing but not deletion safety or the init ownership boundary.
- Public/community entrypoints rewritten: `README.md`, `CODE_OF_CONDUCT.md`, `SECURITY-POLICY.md`, CLA v2.0, contribution governance, PR template, and CLA workflow. `LICENSE` remains unchanged and imported SHA provenance remains in README.
- Canonical product and experience docs rewritten or reframed: `docs/PROJECT.md`, `docs/DESIGN.md`, `docs/ARCHITECTURE.md`, `docs/CODEBASE_STRUCTURE.md`, `docs/DOCUMENTATION_ARCHITECTURE.md`, `docs/index.md`, `docs/QUICK_REFERENCE.md`, `docs/GOVERNANCE.md`, localization, accessibility, render policy, and SEO.
- Added `docs/PRIVACY_DATA_INVENTORY.md` as engineering evidence rather than inventing a legal privacy policy. Reframed durable email/Mailpit/Listmonk boundaries in `docs/EMAIL_NOTIFICATIONS.md`.
- Removed the duplicate `docs/docs-website/` subtree and three unreferenced imported PNG assets; retained ISLAMU logo assets.
- Re-baselined canonical `.claude` planning/task/hook context and selected project-specific agent skills. Deleted two now-unreferenced self-hostable/Event CTO review resources.
- Reworked the active Senior CTO rubric, severity model, rewrite guidance, and output template around first-party product truth, data scope, privacy, accessibility, and operational recovery instead of universal self-hosting assumptions.
- Re-baselined all GitHub issue templates, CODEOWNERS, PR/CLA surfaces, and repository-settings wording for public visitors, content editors, first-party operators, and contributors.
- Removed `docs/DEPLOYMENT_MODES.md` and `docs/DEPLOYMENT_TIERS.md` after clearing active documentation references. `docs/SELF_HOSTING.md` remains temporarily because runtime doctor code, architecture tests, and concurrently edited docs still hard-reference it.
- Reframed non-concurrent troubleshooting, storage, configuration, Cerbos, Blazor, backup/restore, and ADR links away from unsupported self-host/deployment-tier documentation.
- Completed the source-independent agent/tool audit: canonical commands, hooks, Copilot instructions, active CTO/consultation skills, `.codex` configuration, `.design-sync`, `.repomix`, `.opencode`, and `.ci` documentation are either first-party accurate or deliberately retained generic/vendor tooling. Code hooks and CI scripts remain with their code owners.
- Completed the exact path/glob disposition ledger and init Phase 9-12 handoff matrix. Every uncertain source-coupled family now has a named owner and completion signal rather than a guessed disposition.
- Preserved durable history without presenting it as current product truth: journal records remain historical, semantic release history is excluded from the current docs journey, owner-controlled active/paused work is untouched, and unreferenced Event-only I-VSD analyses were removed.
- Re-baselined plan/task execution state: 12 of 36 tasks are complete, 16 are partial, and eight source-coupled/final-convergence tasks have not started.
- Completed Task 0.4 by deleting five documentation/agent-context governance files containing 38 tests, removing one Markdown-backed API convention assertion, deleting the obsolete `agent-context` workflow, and removing dead Markdown parsers. The surviving architecture suite now inspects executable source/configuration artifacts only; documentation and intent schemas use explicit author/reviewer checks.
- Task 0.4 residual inspection passes: no surviving architecture test reads Markdown or `.claude/contract/intents.yaml`, and no removed parser/helper symbol has a caller.
- Debug and Release architecture runs were attempted on 2026-07-18 but stopped before test execution because concurrent Phase 10 Domain changes leave five missing constant references. The full Release build was also attempted and stopped during restore because two WebAssembly packages required by `Explore.Blazor.Client.Tests` were unavailable from the configured sources.
- Source-coupled docs and metadata advanced after Phase 9-12 implementation landed: active product/security/integration/platform/operations/history docs now describe current first-party website, Blog, navigation, production, Weblate/Tolgee/PostHog/Mailpit/Listmonk, Cerbos, and webhook/federation/Coop/Osprey boundaries without reviving Event product promises.
- Deleted `docs/SELF_HOSTING.md`, `docs/MODULAR_EVENTS.md`, `docs/TEMPLATE_SYNC.md`, and `docs/CONTACT_SHARING.md` after active references were repaired. Historical/workstream mentions remain only as implementation history.
- Removed public `/privacy` and `/terms` pages, routes, navigation, sitemap, and seeded footer links because the repository has an engineering privacy inventory but no approved public legal policy. Tests were adjusted to assert legal-route absence rather than publishing unsupported claims.
- Reconciled agent/CI/generated-contract metadata: `.claude/context-state.json`, intents, benchmarks, CI/OpenAPI/Cerbos/security workflows, Dependabot paths, release-impact labels, hook mirrors, and CTO outputs now target first-party website contracts. Canonical generated outputs `schemas/openapi_islamu.json` and `src/Explore.Blazor.Client/Clients/IslamuApiClient.g.cs` were produced by configured project owners.
- Compile/test convergence progressed: Domain constants and retained Listmonk/Coop/Osprey/custom-property contracts were restored without Event aliases; `Event.Application.UnitTests` direct execution is green with 1,441 passed, 0 failed, 0 skipped. `Explore.API`, `Explore.Infrastructure.Tests`, `Explore.Blazor.Client`, and `Event.Architecture.Tests` compile individually. The full solution remains ungreen.

### IN PROGRESS

- Final convergence is in progress. API integration and Blazor downstream test lanes failed/time-boxed with stale Event/generated-client/legal-route references still to adapt. Phase 10 residue guardrails currently report three failing categories that require classification rather than blind replacement.

### NEXT

1. Continue API integration and Blazor downstream convergence from current worker failures; do not restore deleted Event contracts or public legal routes.
2. Classify Phase 10 residue guardrail failures into forbidden product residue, allowed legacy project/framework identifiers, and generic .NET/domain-event terms.
3. Collect pending independent verifier outputs when system reminders arrive, then update task completion status again.
4. Record owner/legal decisions for Code of Conduct/security contact wording and CLA v2.0 enforcement.
5. Run final residual, `/docs-lint`, YAML/Compose/Cerbos/OpenAPI, Debug/Release architecture, targeted tests, and Release build gates once convergence completes.

### BLOCKERS

- Public privacy/cookie text, CLA v2.0, and legal/contact commitments require owner/legal approval before publication or enforcement.
- Owner/legal approval for security/contact wording and CLA v2.0 enforcement remains unresolved.
- Full verification is blocked by remaining source/test convergence: stale API integration tests, Blazor downstream test projects, Phase 10 residue guardrails, and downstream generated-contract consumers are not yet green.
- Public privacy/cookie publication remains approval-gated. The current safe state is no public `/privacy` or `/terms` route until approved policy text exists.

## Quick Resume

1. Read `islamu-website-non-code-refactor-plan.md`.
2. Read `islamu-website-non-code-refactor-tasks.md`.
3. Inspect current git status and current source/test state; the prior init-import docs were deleted by the concurrent Phase 9-12 implementation.
4. Run `git status --short`; do not overwrite concurrent changes.
5. Reconcile completed and partial implementation below against the exact path/glob ledger and handoff matrix.
6. Update the disposition ledger before any newly discovered source-coupled doc, policy, schema, generated output, or configuration change.
7. Keep all three dev docs synchronized after each meaningful slice.

## Workstream Ownership Boundary

| Concern | Owner | Completion signal |
|---|---|---|
| Non-code artifact inventory, disposition, content, links, and residual terms | This workstream | Ledger complete; non-code gates green |
| Event source/routes/contracts removal | Completed source work + current convergence lanes | Phase 10 guardrails classify/clear remaining residue |
| Database migration/data backup | Init-import Phase 10 | Verified backup/rehearsal and migrations green |
| Generated API/client/database outputs | Code owner/generator targets | `schemas/openapi_islamu.json`, `docs/API_CONTRACT_INVENTORY.md`, and `IslamuApiClient.g.cs` generated without drift |
| Blog source/API/UI/SEO behavior | Current source + convergence lanes | Blog/API/Blazor tests and route/content docs green |
| Navbar/mega navigation behavior/assets | Current source + convergence lanes | Responsive/accessibility/navigation tests green |
| Canonical docs describing those behaviors | This workstream after handoff | Source anchors and behavior review green |
| Documentation/agent-context governance | This workstream and reviewers | Manual schema/link/source-anchor review recorded; executable architecture tests remain prose-independent |
| Legal/privacy commitments | Repository owner/legal reviewer | Explicit approval recorded |

## Key Files And Responsibilities

| Path | Existing/New | Layer | Purpose | Notes |
|---|---|---|---|---|
| `README.md` | Existing | Public docs | Repository/product entrypoint | Rewritten; source SHA and AGPL provenance preserved |
| `AGENTS.md` | Existing | Agent context | Canonical contribution contract | First-party framing complete; final Phase 10 rule pass pending |
| `docs/PROJECT.md` | Existing | Product docs | Product scope/source of truth | Rewritten as the first-party digital-front-door contract |
| `docs/DESIGN.md` | Existing | Product/design docs | Public ISLAMU atmosphere and experience principles | Replace Event admin command-center framing; retain useful tokens and motion restraint |
| `docs/DESIGN_SYSTEM.md` | Existing | Design-system docs | CSS layers, tokens, wrappers, isolation | Retain technical system; rewrite stale examples only |
| `docs/ACCESSIBILITY.md` | Existing | Accessibility docs | WCAG 2.2 AA and interaction gates | Retain RTL, keyboard, zoom, targets, focus, and reduced-motion constraints |
| `docs/SEO.md`, `docs/RENDER_POLICIES.md` | Existing | Public delivery docs | Discoverability and render-policy truth | Finalize routes/structured data only after source exists |
| `docs/ARCHITECTURE.md` | Existing | Architecture docs | Surviving system design | Finalize after Phase 10-11 |
| `docs/index.md` | Existing | Docs navigation | Audience entrypoint | Rebuild after dispositions |
| `docs/DOCUMENTATION_ARCHITECTURE.md` | Existing | Docs governance | Audiences/metadata/source anchors | Remove third-party operator/self-host architecture |
| `docs/GETTING_STARTED.md` | Existing, concurrently modified | Contributor docs | Canonical dev setup | Re-read after init Task 3.4 |
| `docs/OPERATIONS.md` | Existing, concurrently modified | Operations | Runtime and verification runbooks | Re-read after init Task 3.4 and Phase 9 |
| `docs/CONFIGURATION.md` | Existing | Operations | Config keys/ownership | Trim only after code ledger |
| `docs/LOCALIZATION.md` | Existing | Infrastructure docs | Weblate/Tolgee/offline behavior | Weblate retained; Tolgee needs code decision |
| `docs/EMAIL_NOTIFICATIONS.md` | Existing | Infrastructure docs | Mailpit/email outbox behavior | Remove Event fanout; retain durable email chassis |
| `docs/SELF_HOSTING.md` | Deleted | Unsupported product docs | Third-party self-host guide | Unique local/ops content moved to current docs before deletion |
| `docs/DEPLOYMENT_MODES.md` | Deleted | Unsupported product docs | Removed product-mode promise | Deleted after active non-concurrent references were cleared |
| `docs/DEPLOYMENT_TIERS.md` | Deleted | Unsupported product docs | Removed self-host scaling tiers | Deleted after active non-concurrent references were cleared |
| `tests/Event.Architecture.Tests/DocumentationQualityTests.cs` and `AgentContext*Tests.cs` | Deleted | Test governance | Removed prose/schema coupling from executable architecture tests | Replaced by `/docs-lint`, schema-tool validation, and reviewer ownership |
| `.env.example` | Existing | Config | Safe local/production examples | 400+ lines; trim after ownership evidence |
| `docker-compose.yml` | Existing | DevOps | Local/full infrastructure | Mailpit and Weblate retained; no PostHog service currently |
| `cerbos/` | Existing | Policy | Authorization package/tests | Source-coupled to Phase 10/11 |
| `schemas/` | Existing | Contracts | Database/OpenAPI/provider schemas | Event outputs regenerate/delete; Weblate/Listmonk retain |
| `.github/` | Existing | DevOps/community | CI, security, templates, deployment | Retain safeguards; adapt product identity/paths |
| `.claude/`, `.agents/`, `.codex/` | Existing | Agent context | Intents/rules/skills/agents/mirrors | Delete only with inbound-link evidence |
| `assets/` | Existing | Brand/docs | Logos/screenshots/roadmap images | Retain logos; delete unreferenced Event imagery |

## Key Decisions

1. The repository represents one first-party ISLAMU website/webapp, not a supported self-hostable or white-label product.
2. This workstream owns non-code artifact disposition/content; the init-import workstream owns product source, runtime, migrations, generated outputs, Blog, and navigation.
3. The context file is the canonical disposition ledger; no fourth planning/report artifact is created unless ledger volume proves it necessary.
4. Disposition and rationale are separate fields: action categories are Retain unchanged, Rewrite, Merge then delete, Delete, Regenerate, Archive, and Needs code decision; rationale uses init alignment categories.
5. `README.md` is rewritten early but must distinguish Implemented from Planned and preserve source SHA/license provenance.
6. `DEPLOYMENT_MODES.md`, `DEPLOYMENT_TIERS.md`, and `SELF_HOSTING.md` are deleted after active references were repaired and valid local/operations content was moved.
7. Weblate is retained as the first-party localization default. Tolgee is not deleted until code ownership resolves.
8. PostHog integration/config/tests are retained; no local PostHog Compose deployment is added.
9. Mailpit remains local/test infrastructure only. Reusable email outbox/Listmonk material remains for future waitlist/newsletter/transactional flows, subject to caller evidence.
10. Generated OpenAPI/database contracts are never hand-edited; `schemas/openapi_islamu.json` and `IslamuApiClient.g.cs` now exist because configured project targets generated them.
11. Legal/privacy claims are approval-gated; broken public links are removed rather than filled with generic text.
12. Residual Event/self-host terms require a reviewed allowlist; global textual replacement is forbidden.
13. The website is ISLAMU's first-party digital front door. Marketing is the entry layer into trustworthy content and real participation, not a brochure or speculative marketplace.
14. Capability horizons are Foundation, Publish, Engage, and Expand. Engage and Expand require separate approved workstreams before public routes or CTAs.
15. A content/route truth inventory governs every claim, route, navigation item, CTA, locale, analytics event, and email capture. Planned/Deferred destinations are not clickable.
16. The experience contract is values-led, editorial, multilingual/RTL, WCAG 2.2 AA, calm in motion, privacy-respecting, and fast/resilient; visual novelty alone is insufficient.

## Initial Artifact Disposition Ledger

Execution Task 0.2 expands this family ledger to exact tracked paths. A row marked `Needs code decision` is blocked from content deletion, not implicitly retained forever.

| Artifact family/path | Initial disposition | Alignment reason | Owner/dependency | Destination or retained purpose | Evidence/notes |
|---|---|---|---|---|---|
| `LICENSE` | Retain unchanged | Mandatory parity | This workstream | AGPL-3.0-or-later license | Byte-identical provenance already established |
| `README.md` | Rewrite | islamu.ngo adaptation | This workstream | Public website/repository entrypoint | Preserve imported SHA/provenance |
| `CODE_OF_CONDUCT.md` | Rewrite | Reusable pattern | This workstream + owner | Community behavior policy | Preserve Contributor Covenant attribution |
| `SECURITY-POLICY.md` | Rewrite | Reusable pattern | This workstream + security owner | First-party disclosure policy | Current response/contact wording needs approval |
| `legal/CLA.md`, contribution governance, CLA workflow/template | Rewrite or versioned replacement | Reusable pattern | Legal owner + this workstream | Contribution licensing | Preserve/invalidate signatures only deliberately |
| `PRIVACY-POLICY.md` | Create after approval | islamu.ngo adaptation | Owner/legal + Task 1.4 | Public data/consent notice | Current README target is missing |
| `docs/PROJECT.md` | Rewrite | islamu.ngo adaptation | This workstream | Website product definition | Current file is Event/self-host product |
| `docs/ARCHITECTURE.md` | Rewrite | Reusable pattern | This workstream after init 10/11 | Surviving website architecture | Keep proven chassis, replace Event flows |
| `docs/index.md` | Rewrite | islamu.ngo adaptation | This workstream | Public/contributor/operator navigation | Current index routes to Event/self-host docs |
| `docs/DOCUMENTATION_ARCHITECTURE.md` | Rewrite | Reusable pattern | This workstream | First-party docs audiences/governance | Keep Diátaxis/metadata/source anchors |
| `docs/DOCUMENTATION_STYLE_GUIDE.md` | Retain/rewrite narrowly | Reusable pattern | This workstream | Style and drift rules | Product examples may need replacement |
| `docs/QUICK_REFERENCE.md`, `docs/GOVERNANCE.md` | Rewrite | Reusable pattern | This workstream after init 10 | Surviving invariants | Must stay synchronized with tests/code |
| `docs/GETTING_STARTED.md`, `CONTRIBUTING.md`, `FIRST_CONTRIBUTION.md`, `TESTING.md`, `TROUBLESHOOTING.md` | Rewrite/merge | Reusable pattern | This workstream after init 3.4/10 | Contributor workflow | Correct root-relative test paths and TUnit syntax |
| `docs/SELF_HOSTING.md` | Merge then delete | Not applicable | This workstream + code guardrail handoff | Local setup -> GETTING_STARTED; ops -> OPERATIONS | Unsupported product promise |
| `docs/DEPLOYMENT_MODES.md` | Delete | Not applicable | This workstream | None | Deleted; unsupported product-mode promise removed |
| `docs/DEPLOYMENT_TIERS.md` | Delete | Not applicable | This workstream | None | Deleted; unsupported self-host tier narrative removed |
| `docs/MODULAR_EVENTS.md`, `TEMPLATE_SYNC.md`, `CONTACT_SHARING.md` | Delete candidate | Not applicable | Init Phase 10 ledger | None unless unique generic content exists | Event product features |
| Event API/federation/moderation/Coop/Osprey docs and ADRs | Needs code decision, likely delete | Not applicable | Init Phase 10 | None or merged generic chassis content | Exact provider/caller ledger required |
| Accessibility/design/localization/SEO/render-policy docs | Rewrite/retain against experience contract | Reusable pattern | This workstream after Task 2.6 and init Phase 12 | ISLAMU public experience and quality gates | Preserve RTL/a11y/design-system truths; publish routes/SEO only when implemented |
| Security/auth/BFF/authorization docs | Rewrite/retain | Reusable pattern | This workstream after Phase 10 | Surviving first-party security architecture | Remove tenant/Event claims only with code evidence |
| `docs/LOCALIZATION.md` and Weblate config/schemas | Rewrite/retain | Mandatory owner decision | This workstream + init provider ledger | First-party localization | Weblate explicitly retained |
| Tolgee docs/config/schemas | Needs code decision | Needs investigation | Init provider ledger | Optional provider only if code survives | User selected Weblate, did not explicitly order Tolgee deletion |
| Analytics/PostHog docs/config | Rewrite/retain | Mandatory owner decision | This workstream + privacy approval | First-party analytics/feature flags | Code/config/tests exist; no Compose service |
| Mailpit/SMTP/email docs/config/tests references | Rewrite/retain | Mandatory owner decision | This workstream + init email ledger | Local capture and reusable durable email | Remove Event fanout only |
| Listmonk docs/config/schema | Rewrite/retain pending evidence | Reusable pattern | Init Phase 10 + this workstream | Future waitlist/newsletter sync | Existing durable integration slice |
| `docs/docs-website/` | Delete candidate | Not applicable | This workstream | None | Parked and mostly empty; no hosted-doc requirement |
| Semantic-version/release history | Archive/delete by value | Historical | This workstream | Provenance only | Must not appear as current website roadmap |
| `AGENTS.md` | Rewrite | islamu.ngo adaptation | This workstream after canonical docs | Canonical agent contract | Current title says Event platform |
| `.claude/contract/intents.yaml`, rules/commands | Rewrite/retain/delete by path | Reusable pattern | This workstream after init 10 | Website task routing | Add recurring non-code intent if justified |
| `.agents/skills/**`, `.claude/agents/**`, `.codex/**` | Retain/rewrite selective | Reusable pattern | This workstream | Reusable engineering context | Do not edit pedagogical examples mechanically |
| `.opencode/**` generic BMAD wrappers | Retain unchanged unless broken | Reusable pattern | This workstream audit | Tooling | Avoid vendored churn |
| `.design-sync/**`, `.repomix/**`, `.ci/**` | Audit then retain/rewrite | Reusable pattern | This workstream | Tool/CI configuration | Update only actual identity/path defects |
| `.github` templates/CODEOWNERS/settings | Rewrite | islamu.ngo adaptation | This workstream | Website contribution routing | Current CLA/Event/operator wording stale |
| `.github` security/build/context workflows | Retain/rewrite | Mandatory parity | This workstream + code owner | Supply-chain and quality gates | Never weaken permissions/test execution |
| OpenAPI/contract workflows | Needs code decision | Needs investigation | Init Phase 10 | Surviving website contracts | Delete/regenerate with source owner |
| Coolify/deploy workflows | Needs code decision | islamu.ngo adaptation | Init Phase 9 | One first-party deployment | Do not present stale workflows as supported |
| `.env.example` | Rewrite/trim | islamu.ngo adaptation | This workstream after code ledger | Safe first-party/local config | Remove keys only after owner deletion |
| `docker-compose.yml` | Rewrite/retain by service | Reusable pattern | Init provider ledger + this workstream docs | Local developer infrastructure | Mailpit/Weblate retain; no PostHog service |
| `src/Explore.AppHost/Config/**` non-code configuration | Needs code decision | Needs investigation | Init Task 3.4/Phase 4 + this workstream | Retained local profile support only | Includes Prometheus, Coop, and pgAdmin files; `pgadmin/pgpass` has fixed dev-only passwords and is currently mounted by `AppHost.cs` |
| `cerbos/**` | Needs code decision | Needs investigation | Init Phase 10/11 | Surviving authorization resources | Behavior-coupled policy artifacts |
| `schemas/islamu-event.md`, `openapi_islamu-event.json` | Regenerate/rename or delete | Not applicable | Init generator owner | Current website schema/contracts | Never hand-edit |
| Weblate/Listmonk provider schemas | Retain | Mandatory/reusable | This workstream + code owner | Retained provider generation | Verify surviving adapters |
| Tolgee provider schemas | Needs code decision | Needs investigation | Init provider ledger | Optional provider only if retained | Do not delete while generated client is used |
| ISLAMU logos | Retain | Mandatory parity | Brand owner | Website branding | Verify active variants |
| Event screenshots/roadmap images | Delete candidate | Not applicable | This workstream after reference check | None | Delete only with zero inbound references |
| Active init-import dev docs | Retain unchanged by this workstream | Mandatory coordination | Init owner | Code workstream memory | Re-read; do not overwrite concurrent edits |
| Other active/pause dev docs | Needs individual reference decision | Historical/active | Owning workstream | Operational memory | Dirty deletions belong to another agent/user |

## Exact Path And Glob Disposition Ledger

This table completes Task 0.2 without pretending that source-coupled decisions have already been made. An unambiguous glob covers every artifact beneath that path; exceptions are named explicitly. Product source and test implementation are excluded from this non-code workstream except as read-only evidence or a code-owner handoff.

| Exact path or unambiguous glob | Disposition | Owner/dependency | Destination or rule | Evidence/status |
|---|---|---|---|---|
| `README.md`, `CODE_OF_CONDUCT.md`, `SECURITY-POLICY.md` | Rewrite | This workstream; owner approval for policies | First-party public/community entrypoints | Implemented; policy owner confirmation remains |
| `LICENSE` | Retain unchanged | Repository owner | AGPL license continuity | Must remain byte-identical |
| `legal/CLA.md`, `docs/legal/**`, `.github/workflows/cla.yml`, `.github/PULL_REQUEST_TEMPLATE.md` | Versioned rewrite | Legal owner + this workstream | CLA v2.0 and contribution governance | Implemented; legal approval remains |
| `AGENTS.md`, `CLAUDE.md` | Rewrite/retain | This workstream | Canonical agent entrypoints | `AGENTS.md` re-baselined; `CLAUDE.md` remains a routing shim |
| `docs/PROJECT.md`, `docs/DESIGN.md`, `docs/DESIGN_SYSTEM.md`, `docs/ACCESSIBILITY.md`, `docs/LOCALIZATION.md`, `docs/PRIVACY_DATA_INVENTORY.md` | Rewrite/retain | This workstream | Canonical product and experience contract | Implemented or retained with first-party framing |
| `docs/DOCUMENTATION_ARCHITECTURE.md`, `docs/DOCUMENTATION_STYLE_GUIDE.md`, `docs/DOCUMENTATION_SYNTHESIS.md`, `docs/PUBLIC_DOCS_ROADMAP.md`, `docs/index.md` | Rewrite/retain | This workstream | Canonical docs governance and navigation | Core rewrite implemented; final residual pass remains |
| `docs/ARCHITECTURE.md`, `docs/CODEBASE_STRUCTURE.md`, `docs/QUICK_REFERENCE.md`, `docs/GOVERNANCE.md` | Transitional rewrite, then finalize | This workstream after init Phase 10/11 | Surviving source-grounded architecture | Transitional framing implemented; no invented Blog/Event-free flow |
| `docs/SEO.md`, `docs/RENDER_POLICIES.md` | Transitional rewrite, then finalize | This workstream after init Phase 11/12 | Implemented public route/index/render truth | Current imported behavior separated from planned website behavior |
| `docs/GETTING_STARTED.md`, `docs/OPERATIONS.md`, `docs/SECRETS.md` | Rewrite after concurrent handoff | Init Task 3.4/Phase 9, then this workstream | Contributor and first-party operator truth | Concurrently modified; do not overwrite |
| `docs/CONFIGURATION.md`, `docs/TROUBLESHOOTING.md`, `docs/STORAGE.md`, `docs/BLAZOR.md`, `docs/BACKUP_RESTORE_UPGRADE.md`, `docs/CERBOS_COOLIFY.md` | Rewrite/retain | This workstream after source decisions | First-party configuration and operations | Non-concurrent self-host links removed; source-specific final pass remains |
| `docs/EMAIL_NOTIFICATIONS.md` | Rewrite/retain | This workstream + init Phase 10 | Durable email, Mailpit, and evidence-based Listmonk boundary | Public Engage flows remain separate future work |
| `docs/SELF_HOSTING.md` | Merge then delete | Init/code owner + Task 0.4 | Unique local/ops facts move to contributor/operator docs | Blocked by runtime doctor, architecture-test, and concurrent-doc references |
| `docs/DEPLOYMENT_MODES.md`, `docs/DEPLOYMENT_TIERS.md` | Delete | This workstream | None | Deleted after active non-concurrent references were removed |
| `docs/docs-website/**` | Delete | This workstream | None | Deleted; repository Markdown remains the single docs source |
| `docs/MODULAR_EVENTS.md`, `docs/TEMPLATE_SYNC.md`, `docs/CONTACT_SHARING.md`, `docs/TEMPLATE_GLOSSARY.md` | Needs code decision, likely delete | Init Phase 10 | Merge only proven reusable content | Event feature contracts; no early deletion |
| `docs/FEDERATION.md`, `docs/LEXICONS.md`, `docs/COOP_INTEGRATION.md`, `docs/OSPREY_INTEGRATION.md`, `docs/WEBHOOKS.md` | Needs code decision, likely delete | Init Phase 10 | None unless surviving caller evidence exists | Provider/federation/Webhook source-coupled |
| `docs/API.md`, `docs/API_COOKBOOK.md`, `docs/API_CHANGELOG.md`, `docs/API_CONTRACT_INVENTORY.md`, `docs/API_CONTRACT_TEST_DEBT.md` | Regenerate/rewrite/delete by surviving API | Init Phase 10/11 + generator/code owner | Website/Blog API truth only | Never hand-edit generated inventory as final truth |
| `docs/MULTI_TENANCY.md`, `docs/ADMIN_GUIDE.md`, `docs/ADMIN_HIERARCHY.md`, `docs/AUTHORIZATION.md`, `docs/AUTHORIZATION_PATTERNS.md`, `docs/SECURITY-MODEL.md` | Needs code decision | Init Phase 10 | Surviving first-party auth/data-scope docs or deletion | Tenant/admin claims leave only with source/policy/tests |
| `docs/DOMAIN.md`, `docs/INTEGRATIONS.md`, `docs/EXTENSIBILITY.md`, `docs/NOTIFICATIONS.md`, `docs/OUTBOX_PATTERN.md`, `docs/CUSTOM_PROPERTIES.md`, `docs/FOOTER_MANAGEMENT.md` | Rewrite/retain/delete by caller evidence | Init Phase 10/11 | Reusable chassis patterns only | Do not preserve speculative flexibility |
| `docs/ACCESSIBILITY_ARTIFACTS.md`, `docs/UI_GOVERNANCE.md`, `docs/BLAZOR_DEV_WORKFLOW.md`, `docs/DOCK_LAYOUT.md` | Rewrite/retain | This workstream after init Phase 12 | Website UI quality and workflow | Remove Event examples only; preserve verified accessibility rules |
| `docs/TESTING.md`, `docs/TEST_RELIABILITY.md`, `docs/BENCHMARKS.md`, `docs/CI_CD_GOVERNANCE.md`, `docs/CI_CD_RUNBOOKS.md`, `docs/RELEASE_CHECKLIST.md` | Rewrite/retain | This workstream + code/CI owners | Current contribution and release gates | Do not weaken existing checks |
| `docs/AI_*.md`, `docs/MCP_DEBUGGING.md`, `docs/adr/ADR-011-*.md`, `docs/adr/ADR-012-*.md` | Retain/rewrite only if active | This workstream + owning workstream | Agent/MCP governance | Product identity changes only where operationally relevant |
| `docs/adr/ADR-001-*.md` through `docs/adr/ADR-010-*.md` | Retain historical or supersede; never rewrite history silently | Init Phase 10 + this workstream | Decision history with explicit supersession | Event-only decisions may be archived after source removal |
| `docs/semantic_versioning/**`, `docs/DUAL_VERSIONING.md` | Archive/delete from current navigation | This workstream | Historical provenance only | Must not imply a current public product roadmap |
| Remaining `docs/*.md` not named above | Audit then retain/rewrite | This workstream | Contributor/design/operations evidence | No deletion without a new explicit row |
| `.github/ISSUE_TEMPLATE/**`, `.github/CODEOWNERS`, `.github/dependabot.yml`, `.github/copilot-instructions.md` | Rewrite/retain | This workstream | Contribution and dependency governance | Templates/CODEOWNERS re-baselined; safeguards retained |
| `.github/workflows/_build-test.yml`, `_container-build.yml`, `cerbos-policy-check.yml`, `codeql.yml`, `coverage.yml`, `dependency-review.yml`, `e2e.yml`, `performance-smoke.yml`, `release-impact.yml`, `scorecard.yml`, `secret-scanning.yml`, `security-tests.yml`, `test.yml`, `workflow-security.yml` | Retain; rewrite paths only with owner evidence | CI/code owner | Supply-chain and executable quality gates | Never weaken permissions or checks |
| `.github/workflows/agent-context.yml` | Delete | This workstream | None | Deleted with the Markdown/intent governance tests it exclusively executed |
| `.github/workflows/deploy-coolify.yml`, `.github/workflows/deploy-coolify-develop.yml` | Needs code decision | Init Phase 9 | One first-party deployment topology | Do not advertise as third-party deployment support |
| `.github/workflows/openapi-contract.yml` | Needs code decision/regenerate | Init Phase 10/11 | Surviving API contract | Source/generator owned |
| `.claude/contract/**`, `.claude/commands/**`, `.claude/rules/**`, `.claude/agents/**`, `.claude/hooks/**`, `.claude/benchmarks/**`, `.claude/context-state.json`, `.claude/settings.json` | Selective rewrite/retain | This workstream; code mirrors remain code-owner | First-party agent routing and guardrails | Core planning identity updated; no mechanical pedagogical churn |
| `.agents/skills/**` | Selective rewrite/retain/delete with inbound-link check | This workstream | Reusable first-party skills | CTO guidance updated; generic I-VSD self-host examples remain framework examples, not product claims |
| `.codex/agents/**`, `.codex/rules/**`, `.codex/hooks.json`, `.codex/hooks/**` | Retain/rewrite mirrors | This workstream; `.cs` hook code excluded | Tool mirror parity | Config identity updated; code files are outside scope |
| `.opencode/command/**`, `.opencode/skill/**`, `.opencode/package.json`, `.opencode/package-lock.json`, `.opencode/tui.json`, `.opencode/.gitignore` | Retain unless broken | Tool owner | Installed tooling | `.opencode/node_modules/**` is vendored/generated and excluded |
| `.ci/README.md`, `.ci/actions/**`, `.ci/spectral.yaml` | Retain/rewrite narrowly | CI owner | CI documentation/configuration | Preserve active gates |
| `.ci/scripts/**` | Code-owner handoff | CI owner | Active validation scripts | Script code excluded from direct non-code edits |
| `.design-sync/**`, `.repomix/**`, `repomix.config.json`, `jean.json`, `skills-lock.json` | Retain unless path/identity defect is proven | Tool owners | Development tooling | No speculative churn |
| `.editorconfig`, `.gitignore`, `.dockerignore`, `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `dotnet-tools.json`, `packages.lock.json`, `Explore.slnx` | Retain; source/build-owner changes only | Build/code owner | Build and dependency contracts | Outside content rewrite scope |
| `.env.example`, `docker-compose.yml` | Rewrite/retain by surviving key/service | Init provider ledger + this workstream | Safe local and first-party environment | Identity/default DB/display names updated; key removal waits for caller deletion |
| `docker/keycloak/**`, `docker/minio/**` | Needs code decision | Init Phase 4/10 | Surviving first-party identity/storage setup | Realm client IDs and generated fixtures are source-coupled |
| `src/Explore.AppHost/Config/**` | Needs code decision | Init Task 3.4/Phase 4 | Surviving local profile config | Includes mounted dev-only pgAdmin password file; remove with caller |
| `cerbos/config/**`, `cerbos/init/**`, `cerbos/policies/**`, `cerbos/tests/**` | Needs code decision/regenerate | Init Phase 10/11 | Surviving authorization package | Policy behavior is source-coupled |
| `schemas/islamu-event.md`, `schemas/openapi_islamu-event.json` | Delete/regenerate/rename | Init Phase 10 generator owner | Post-Event schema/API truth | Never hand-edit |
| `schemas/openapi-weblate-provider.yaml` | Retain | Localization owner | Weblate generated client contract | Explicit retained provider |
| `schemas/openapi-listmonk.yaml` | Retain pending caller evidence | Email/integration owner | Future approved Engage workflow support | Adapter existence is not a public feature |
| `schemas/openapi-tolgee-provider.yaml` | Needs code decision | Init provider ledger | Retain only if adapter survives | Weblate selection alone is not caller evidence |
| `assets/images/adopters/islamu-text-only.png`, `assets/images/adopters/islamu-logo-text-only.svg` | Retain | Brand owner | ISLAMU branding | Active logo assets retained |
| `assets/fff.png`, `assets/event-list-image.png`, `assets/Roadmap Kanban View.png` | Delete | This workstream | None | Deleted after zero inbound references |
| `dev/active/islamu-website-non-code-refactor/**` | Retain/update | This workstream | Durable execution memory | Canonical plan/context/tasks |
| `dev/active/init-import-islamu-event/**`, `dev/active/braindump.md` | Retain unchanged here | Init owner | Code migration memory/inspiration | Concurrent/owner-controlled |
| `dev/active/**`, `dev/pause/**` excluding rows above | Owning-workstream decision | Respective owner | Active/paused memory | Never delete because a link is stale without ownership check |
| `dev/_journal/**` | Retain historical; update index framing only | Repository owner | Durable findings | Historical Event terms are allowed evidence |
| `.omo/**` | Retain as orchestration state | Active work owners | Execution ledger/artifacts | Not product documentation; do not publish as product truth |
| `islamic-value-sensitive-design/**` | Delete obsolete project-specific artifacts | This workstream | None | Three unreferenced Event-only analyses deleted; directory is now empty/absent when filesystem permits |

## Init Phase 10-12 Handoff Matrix

This matrix completes Task 0.3. The init workstream owns source behavior; this workstream owns the final non-code description only after the named completion signal.

| Shared artifact | Source owner | Completion signal required | Non-code action after handoff |
|---|---|---|---|
| Event feature/API/federation/Coop/Osprey/Webhook docs | Init Phase 10 | Caller ledger complete and matching source/routes/tests removed or retained deliberately | Delete Event-only docs or rewrite only surviving contracts |
| `docs/SELF_HOSTING.md` and doctor/document references | Init Phase 10 + this workstream | Runtime doctor and remaining document references removed/reframed | Merge unique facts, delete file, repair links/source anchors |
| Cerbos Event/admin/tenant resources | Init Phase 10/11 | Policy callers and tests identify exact surviving resources | Delete/regenerate policies and document only surviving authorization |
| `schemas/islamu-event.md`, `schemas/openapi_islamu-event.json`, API inventories | Init Phase 10/11 generator owner | Event endpoints/models absent and generators run successfully | Publish regenerated website/Blog contracts; remove stale generated names |
| Event-specific ADRs and semantic release history | Init Phase 10 | Superseding source decision is recorded | Archive with supersession note or remove from current navigation |
| Email Event fanout and Listmonk integration docs | Init Phase 10 | Caller ledger distinguishes durable chassis from Event fanout | Remove Event fanout; retain only evidence-based email/integration behavior |
| Blog docs, SEO, sitemap, structured data, and public routes | Init Phase 11 | Blog Domain/API/UI/SEO acceptance complete | Mark Blog routes Implemented and add verified source anchors |
| Navigation model, route inventory, responsive/RTL/accessibility behavior | Init Phase 12 | Navigation acceptance and route inventory complete | Finalize design/render/navigation docs; expose only implemented destinations |
| Deployment/Coolify/operator topology | Init Phase 9 | One first-party production topology and recovery path approved | Rewrite deployment workflows/runbooks without self-host product promises |

## Documentation Governance Disposition

Task 0.4 deliberately removed `DocumentationQualityTests.cs`, the four `AgentContext*Tests.cs` files,
the Markdown-backed assertion in `ApiConventionTests.cs`, and `.github/workflows/agent-context.yml`.
`ContextSystemHelpers.cs` now resolves executable source paths only, and the Phase 10 residue guardrail
no longer scans Markdown. Documentation owners use `/docs-lint`, compatible schema tooling, exact
reference searches, and pull-request review for metadata, links, source anchors, and truth states.
Source-code architecture, authorization, accessibility, production-runtime, and Phase 10 residue
tests remain executable gates.

## Constraints And Rules To Remember

- No matching intent exists; use the fallback contract in plan Section 0.
- Every modified/new file must satisfy repository ABOUTME requirements.
- No global Event/OpenISLAMU replacement.
- No deletion without the exact-path ledger and reference/source-anchor/test/workflow checks.
- No source/test implementation in this workstream; coordinate guardrail/generator changes with the init/code owner.
- No compatibility aliases without a proven external consumer.
- No generated output hand-edit.
- No legal/privacy commitments without approval.
- No secret values or private environment details in documentation.
- Do not revert or modify unrelated worktree changes.

## Validation Baseline

Planning baseline on 2026-07-15:

```text
dotnet build --configuration Release --verbosity quiet
Result: PASS, 0 errors, 89 warnings, 10.14s
```

Pre-existing warning classes recorded outside this workstream:

- AutoMapper 14.0.0 high-severity advisory `GHSA-rvv3-g6hj-g44x`.
- Microsoft.OpenApi high-severity advisory `GHSA-v5pm-xwqc-g5wc`.
- Roslyn Workspaces `NU1608` version mismatch.
- Deprecated `Microsoft.Extensions.ApiDescription.Client` package.

Required architecture/docs command from repository root:

```bash
dotnet test --project tests/Event.Architecture.Tests/Event.Architecture.Tests.csproj --configuration Release --verbosity quiet
```

Planning validation result on 2026-07-15:

```text
Result: FAIL, 268 passed, 7 failed, 1 skipped, 276 total
```

The seven failures are outside the three new planning files and must not be repaired by this workstream without ownership coordination:

- `EveryMustReadDocExists`: the concurrently deleted `dev/active/webhook-delivery-redesign/` files remain referenced by the webhook intent.
- `HATEOAS link policies use explicit AuthorizationActions permission metadata`: two existing `WebhookConsumerLinkPolicy.cs` calls fail.
- `Commands_ShouldResideIn_CommandsNamespace`, `CommandHandlers_ShouldResideIn_CommandsNamespace`, and `Handlers_ShouldBePublic`: existing CQRS architecture violations.
- `CanonicalDocs_SourceAnchorsResolve`: `docs/BENCHMARKS.md` references the concurrently deleted `dev/pause/performance-optimization/performance-optimization-context.md`.
- `DTOs_ShouldEndWith_Dto`: seven existing management DTO naming violations.

Some copied docs/commands currently omit the `tests/` prefix. Correct those examples during contributor-doc refactoring after verifying invocation from repository root.

Additional proportional gates:

```bash
docker compose config
rg -n -i 'ISLAMU Event|islamu-event|islamuevent|event\.openislamu|OpenISLAMU' --glob '!src/**' --glob '!tests/**'
rg -n -i 'self-host|white-label|multi-tenant|third-party operator' --glob '!src/**' --glob '!tests/**'
```

Run targeted provider/config/policy/schema/workflow tests named in the plan when those artifact families change.

Implementation verification attempted on 2026-07-18:

```text
dotnet test --project tests/Event.Architecture.Tests/Event.Architecture.Tests.csproj --configuration Debug --verbosity quiet
Result: BLOCKED BEFORE TEST EXECUTION, exit 1

dotnet test --project tests/Event.Architecture.Tests/Event.Architecture.Tests.csproj --configuration Release --verbosity quiet
Result: BLOCKED BEFORE TEST EXECUTION, exit 1

Both architecture builds report the same five CS0117 errors:
- CustomPropertyNamespaces.IsSector
- GovernanceSettingKeys.Integrations.Listmonk.SyncOnRegistration
- GovernanceSettingKeys.TenantDelegation.LockReportingProviders
- GovernanceSettingKeys.TenantDelegation.LockTenantOspreyProvider
- GovernanceSettingKeys.TenantDelegation.LockTenantCoopProvider

dotnet build --configuration Release --verbosity quiet
Result: BLOCKED DURING RESTORE, exit 1, 26 warnings, 2 errors
- NU1101 Microsoft.AspNetCore.Components.WebAssembly
- NU1101 Microsoft.AspNetCore.Components.WebAssembly.Authentication
```

These failures occur in concurrent source/package ownership, not in the governance-test removal. Scoped `git diff --check` passed for the governance-test/docs change set. Rerun all three commands after the owning slices restore a buildable worktree.

## Current Known Risks / Unknowns

- Highest risk: docs/runtime drift during concurrent init Phases 10-12.
- Highest deletion risk: load-bearing source anchors and document references that require explicit reviewer searches.
- Exact Phase 10 chassis survival is not yet known.
- Tolgee and several side-service dispositions remain code-ledger decisions.
- Final tenancy semantics and production topology remain unresolved in the init plan.
- Privacy, contact, CLA continuity, and public commitments require owner/legal approval.
- Existing worktree changes in init docs, AppHost, operations docs, and architecture tests belong to another active slice and must not be touched.
- Do not classify fixed optional-`local-full` development credentials as production secrets, but do not retain `pgadmin/pgpass` after its pgAdmin/database callers are removed; decide it with the runtime owner.
- Mission wording, campaigns, impact proof, testimonials, imagery rights, editorial approvers, and participation destinations are not proven by repository source and require accountable owner evidence.
- “Much more” can cause scope creep and dead public surfaces; capability horizons and implemented-only destinations are mandatory controls.

## Handoff Notes

### Handoff — 2026-07-19 Europe/Brussels

#### Current State

- **Completed:** 17/36 execution tasks. New completions since the last handoff: privacy publication resolved conservatively by removing unapproved legal routes, Event/self-host document classification/removal, intent/path re-baseline, and `SELF_HOSTING.md` deletion.
- **Partial:** 14/36 tasks have implementation progress but still require final verifier output, residual audits, owner/legal approval, or build/test gates.
- **Blocked:** Owner/legal approval remains for security/contact commitments and CLA v2.0 enforcement. Technical convergence remains blocked by API integration, Blazor downstream, Phase 10 residue, and broader solution test/build failures.
- **Verification progress:** `Event.Application.UnitTests` direct executable is green: 1,441 passed, 0 failed, 0 skipped. `Explore.API`, `Explore.Infrastructure.Tests`, `Explore.Blazor.Client`, and `Event.Architecture.Tests` compile individually. Full solution is not green.
- **Not started:** Five final/source-coupled convergence tasks remain: generated inventory finalization, final residual audit, final verification gates, audience walk-through, and final handoff.

#### Next Action

1. Continue API integration and Blazor downstream test convergence from the current failed worker outputs.
2. Classify and fix Phase 10 residue guardrail failures without global-replacing allowed legacy project/framework or generic event terms.
3. Collect pending independent verifier outputs when notified and update task status if they confirm the worker claims.
4. Run final verification gates once source/test convergence completes, then refresh these docs again.

#### Blockers

- Owner/legal approval for security/contact and CLA v2.0 enforcement.
- No public privacy/cookie/terms route should be restored until owner/legal-approved text exists.
- API integration tests still reference deleted Event permissions/entities/webhook callbacks/email ticker jobs.
- Blazor downstream test projects still need generated-client/legal-route/navigation expectation convergence.
- Phase 10 residue guardrails still report three categories requiring classification.

#### Modified Files

- Root/community/legal: `README.md`, `CODE_OF_CONDUCT.md`, `SECURITY-POLICY.md`, `legal/CLA.md`, `docs/legal/CONTRIBUTION_GOVERNANCE.md`; public `/privacy` and `/terms` Blazor routes are deleted pending approval.
- Product/docs: `docs/PROJECT.md`, `docs/DESIGN.md`, `docs/ARCHITECTURE.md`, `docs/CODEBASE_STRUCTURE.md`, `docs/DOCUMENTATION_ARCHITECTURE.md`, `docs/index.md`, `docs/PRIVACY_DATA_INVENTORY.md`, live security/integration/platform/operations docs, and the exact paths recorded in the disposition ledger.
- Agent/GitHub metadata: `AGENTS.md`, selected `.claude/**`, `.agents/**`, `.codex/**`, `.github/ISSUE_TEMPLATE/**`, `.github/CODEOWNERS`, `.github/PULL_REQUEST_TEMPLATE.md`, selected workflows, Dependabot, and CI scripts.
- Generated contracts: `schemas/openapi_islamu.json` and `src/Explore.Blazor.Client/Clients/IslamuApiClient.g.cs` were generated by configured owners; `docs/API_CONTRACT_INVENTORY.md` finalization remains pending.
- Deleted unsupported/duplicate artifacts: `docs/DEPLOYMENT_MODES.md`, `docs/DEPLOYMENT_TIERS.md`, `docs/SELF_HOSTING.md`, `docs/MODULAR_EVENTS.md`, `docs/TEMPLATE_SYNC.md`, `docs/CONTACT_SHARING.md`, `docs/docs-website/**`, unreferenced imported PNG assets, and unreferenced Event-only I-VSD analyses.
- Workstream memory: the three files under `dev/active/islamu-website-non-code-refactor/`.

#### Validation

- Task 0.4 residual inspection passed and scoped `git diff --check` was clean.
- Domain constants and direct WebAssembly package references were repaired by later convergence workers; `Event.Application.UnitTests` direct executable is green: 1,441 passed, 0 failed, 0 skipped.
- `Explore.API`, `Explore.Infrastructure.Tests`, `Explore.Blazor.Client`, and `Event.Architecture.Tests` compile individually. `Explore.API` generation produced `schemas/openapi_islamu.json`; Blazor client generation produced `IslamuApiClient.g.cs`.
- Full solution remains ungreen: the latest worker stopped after reaching downstream API integration, Blazor downstream, Phase 10 residue, and additional generated-contract/test consumers. Final exact error count is pending a fresh writable build after current lanes settle.
- No project startup or Aspire run was performed.

#### Documentation Impact

- Public/product/design/privacy/contribution/release/agent entrypoints now describe the first-party website or explicitly label imported behavior as transitional.
- Source-coupled Event, policy, schema, deployment, Blog/SEO, and navigation documentation has been substantially updated; final publication waits on residual audit and build/test convergence.

#### Risks

- Phase 10 residue scans can overmatch allowed `Event.*` project names, analytics `EventType`, generic .NET events, or historical records; classify before editing.
- Generated OpenAPI/client outputs must remain generator-owned; do not manually patch them.
- Approval-gated legal/security/CLA text must not be treated as final policy merely because draft artifacts exist.

#### Notes For Next Contributor Or Agent

- Read this context, the plan/tasks, current git status, and current worker outputs before editing.
- Do not restore deleted deployment-mode/tier product promises.
- Do not restore `/privacy` or `/terms` routes without approved legal text.
- Do not overwrite concurrent source/test/docs changes; this worktree has a large active implementation diff.
