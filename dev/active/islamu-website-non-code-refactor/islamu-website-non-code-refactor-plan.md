<!-- ABOUTME: Plans the repository-wide non-code re-baseline and experience contract for the first-party ISLAMU website. -->
<!-- ABOUTME: Defines truthful product positioning, artifact disposition, sequencing, implementation state, and verification. -->

# ISLAMU Website Non-Code Refactor — Implementation Plan

Last Updated: 2026-07-19 Europe/Brussels

## 0. Planning Metadata

- **Request:** Refactor every non-code artifact copied from ISLAMU Event so it either describes the first-party ISLAMU marketing website/webapp, remains for a verified website need, or is deleted. Make the plan implementation-ready and establish a distinctive, extensible ISLAMU experience without turning “much more” into unsupported feature promises. Rewrite `README.md`; do not preserve a self-hostable-product promise; retain Weblate, PostHog, and Mailpit.
- **Task directory:** `dev/active/islamu-website-non-code-refactor/`
- **Planning status:** In implementation. Seventeen of 36 execution tasks are complete, 14 are partially implemented or pending verification, and five final/source-coupled tasks remain not started. Final completion remains gated by build/test convergence and owner/legal approvals.
- **Intent classification:** No entry in `.claude/contract/intents.yaml` matches a whole-repository product/documentation/configuration re-baseline.
- **Fallback Contract:** `AGENTS.md`, `docs/QUICK_REFERENCE.md`, `docs/GOVERNANCE.md`, `docs/DOCUMENTATION_STYLE_GUIDE.md`, and `docs/DOCUMENTATION_ARCHITECTURE.md` are authoritative. Scope is non-code documentation, legal/community material, agent context, configuration/orchestration metadata, policies, schemas, workflows, and assets. Minimum verification is a Release build plus `tests/Event.Architecture.Tests`; changed configuration formats receive targeted validation. Destructive deletion additionally requires reference, source-anchor, workflow, test, and active-work checks.
- **Relevant skills loaded during planning:** `agentic-research` for local-first evidence selection and `senior-cto-feedback` for implementation-readiness review. Execution agents must load the path-relevant project skill before changing each artifact family, including `clean-architecture-rules`, `auth-patterns`, `outbox-pattern`, `error-tracking`, `blazor-ui-conventions`, `design-system`, or `aspire` when the surviving artifact documents that concern.
- **Path-scoped rules:** None apply to the three new planning files. Execution agents must reload `.claude/rules/*.md` for every source, test, workflow, or configuration path they touch.
- **Primary layers touched:** Docs, DevOps, agent context, policy, configuration, legal/community, generated contracts, and assets. Product source and test code are coordination dependencies, not implementation scope.
- **Estimated complexity:** XL. The imported repository contains roughly 90 `docs/` artifacts plus repository-root, agent, CI, policy, schema, orchestration, and asset surfaces. The work is deletion-heavy and dependency-coupled to an active code teardown; prose governance uses explicit owner review while executable architecture remains test-enforced.

### Re-baseline — 2026-07-18 Europe/Brussels

- **Reason:** Source-independent implementation advanced beyond the original phase order, while source-coupled and approval-gated work remained unavailable.
- **What changed:** Public/product/community docs, first-party experience guidance, selected operations/release docs, agent context, GitHub metadata, duplicate docs, unsupported deployment-mode/tier docs, and unreferenced assets were implemented. Exact disposition and init handoff matrices are complete.
- **Plan impact:** Completed tasks are marked consistently below. Partial tasks retain unchecked status with their exact remaining gate. Task dependencies now describe the work actually required instead of depending on unfinished final passes.
- **Remaining work:** Obtain owner/legal approvals, then complete Event/source/policy/schema/Blog/navigation/deployment convergence after init Phases 9-12. Task 0.4 removed obsolete prose-governance tests and their workflow.
- **Current verification state:** Task 0.4 residual inspection passes: no surviving architecture test opens Markdown or `.claude/contract/intents.yaml`. Debug and Release architecture runs were attempted on 2026-07-18 but could not execute tests because concurrent Phase 10 Domain edits leave five missing constant references. The full Release build also stops during restore because the configured feeds do not provide two WebAssembly packages required by `Explore.Blazor.Client.Tests`. These are shared-worktree blockers, not deleted-test regressions.

### Re-baseline — 2026-07-19 Europe/Brussels

- **Reason:** Phase 9-12 source/concurrent work landed while this workstream continued; several source-coupled non-code tasks became executable and were implemented before the final build/test gates are green.
- **What changed:** `SELF_HOSTING.md`, `MODULAR_EVENTS.md`, `TEMPLATE_SYNC.md`, and `CONTACT_SHARING.md` are deleted after reference repair. Active product, operations, security/integration, platform, historical, CI, agent, and generated-contract docs were reconciled with current Blog/navigation/production source. Public `/privacy` and `/terms` pages/routes/links were removed instead of publishing unapproved legal claims. Canonical `schemas/openapi_islamu.json` and `src/Explore.Blazor.Client/Clients/IslamuApiClient.g.cs` are now generated by their configured project owners.
- **Plan impact:** Tasks 1.4, 3.1, 3.2, 3.3, and 5.2 are promoted to complete. Several prior partial tasks are now implemented but remain unchecked until independent verifier output, final residual audit, or build/test gates settle.
- **Verification progress:** `Event.Application.UnitTests` is green via direct TUnit execution: 1,441 passed, 0 failed, 0 skipped. `Explore.API`, `Explore.Infrastructure.Tests`, `Explore.Blazor.Client`, and `Event.Architecture.Tests` compile individually after generated contract creation, but the full solution is still not green. Latest convergence workers report API integration-test stale references, Blazor downstream test failures, Phase 10 residue failures, and broader downstream test references that still require source-backed adaptation. A read-only Release build in a sandbox failed before evaluation because that sandbox was read-only; writable build workers produced the meaningful project-level evidence.
- **Remaining work:** Finish API integration and Blazor downstream test convergence, classify Phase 10 residue failures, rerun final gates, collect pending independent verifiers, and then synchronize final counts again. Owner/legal approvals for contact/security and CLA v2.0 remain unresolved.

## 1. Executive Summary

This workstream will make the repository's non-code surface truthfully describe one public, first-party ISLAMU website/webapp. It will replace Event/self-host/operator product positioning with website-specific public, contributor, and internal-operator guidance; delete Event-only artifacts; and retain only infrastructure and platform documentation backed by surviving behavior or a named website need.

The website is ISLAMU's first-party digital front door, not a brochure and not a generic feature marketplace. Marketing is the entry layer: the experience must explain ISLAMU's purpose, publish trustworthy content, help people understand how to participate, and create a governed path for future digital services. The first committed product capabilities remain the Event-free public shell, Blog, and ISLAMU navigation owned by the init workstream. Waitlists, newsletters, and other engagement services are capability horizons that require separate approved feature workstreams before they become public routes or promises.

The target is not a white-label distribution and not a supported self-hostable product. The public repository remains AGPL-3.0-or-later, and the README must preserve the imported source SHA and license provenance. Weblate remains the localization platform, PostHog remains the rich analytics integration, and Mailpit remains local email-capture infrastructure. Listmonk/email material is retained and reframed because waitlists, newsletters, and transactional email are named future needs. Retention means preserving each load-bearing end-to-end slice, not merely leaving a Compose service or a paragraph behind.

This plan owns **artifact disposition and non-code content**. `dev/active/init-import-islamu-event/` continues to own product source deletion, runtime behavior, data/schema migrations, generated-client regeneration, Blog implementation, and navigation implementation. Non-code content that describes those areas is finalized only after the corresponding Phase 10, 11, or 12 behavior is verifiable.

Out of scope:

- Product source refactoring, Event domain deletion, Blog implementation, and navigation implementation.
- Renaming `Explore.*` or `Event.*` projects solely for branding.
- Adding a local PostHog deployment; no current PostHog Compose service exists and the existing integration supports an external endpoint.
- Inventing privacy, cookie, moderation, security-response, or CLA legal claims without owner/legal review.
- Rewriting generated OpenAPI/schema output by hand.
- Churning vendored/generic examples because they happen to mention Event, tenancy, or self-hosting in a pedagogical context.

## 2. Source-Grounded Current State Report

### 2.1 Evidence Log

| Claim | Evidence | Confidence | Notes |
|---|---|---:|---|
| The repository still declares itself the ISLAMU Event platform | `AGENTS.md`; `docs/PROJECT.md`; `.claude/commands/dev-docs.md` | High | The canonical entrypoints actively teach stale product assumptions. |
| The target is one first-party deployment, not a self-hostable product | `dev/active/init-import-islamu-event/init-import-islamu-event-plan.md` Revision 3.1, decisions D7/D8 | High | Binding owner decision; public AGPL licensing remains. |
| Event deletion and new Blog/navigation code are already planned elsewhere | Init workstream Phases 10, 11, and 12 | High | This workstream must not duplicate those code tasks. |
| `README.md` is incomplete and stale | `README.md` | High | Uses OpenISLAMU wording, has minimal onboarding, broken/TODO privacy link, and a provenance section that must survive. |
| Core product documentation is Event/self-host oriented | `docs/PROJECT.md`, `docs/index.md`, `docs/DOCUMENTATION_ARCHITECTURE.md`, `docs/SELF_HOSTING.md`, `docs/DEPLOYMENT_MODES.md`, `docs/DEPLOYMENT_TIERS.md` | High | These files define the wrong product and audience model. |
| The stale terminology is systemic | Search found 181 Event/OpenISLAMU identity matches across 38 top-level docs and 206 self-host/white-label/multi-tenant/provider matches across 47 docs | High | A global replacement would preserve false semantics. |
| Documentation files were test-coupled | Deleted `DocumentationQualityTests.cs` and `AgentContext*Tests.cs` | High | Task 0.4 removed 38 prose/schema tests plus one Markdown-backed API assertion; review ownership replaces executable prose parsing. |
| Agent context is load-bearing | `.claude/contract/intents.yaml`, `.claude/rules/`, `.agents/skills/`, `tests/Event.Architecture.Tests/` | High | The init import previously restored eight omitted `dev/` files after architecture tests exposed broken references. |
| Mailpit is implemented local infrastructure | `docker-compose.yml`; `docs/EMAIL_NOTIFICATIONS.md`; Mailpit runtime tests under `tests/Explore.Infrastructure.Tests/Infrastructure/` | High | Compose, Aspire, SMTP configuration, health checks, outbox drain, and runtime evidence depend on it. |
| Weblate is implemented local/provider infrastructure | `docker-compose.yml`; `docs/LOCALIZATION.md`; `schemas/openapi-weblate-provider.yaml`; localization resilience tests | High | Weblate has Compose services and a generated provider client path. |
| PostHog is implemented but not a Compose service | `src/Explore.Infrastructure/Analytics/PostHogAnalyticsProvider.cs`; analytics settings/tests; `docs/CONFIGURATION.md`; no `posthog` service in `docker-compose.yml` | High | Preserve the integration and first-party configuration/privacy contract; do not invent local hosting. |
| Listmonk and durable email are implemented chassis capabilities | `docs/CONFIGURATION.md`, `docs/OUTBOX_PATTERN.md`, `docs/EMAIL_NOTIFICATIONS.md`, provider schema and tests | High | Reframe for future waitlist/newsletter/transactional website flows after Event fanout is removed. |
| CI/GitHub metadata still contains Event assumptions | `.github/PULL_REQUEST_TEMPLATE.md`, `.github/workflows/openapi-contract.yml`, deploy workflows, CLA workflow, issue templates | High | Security and supply-chain workflows are load-bearing and require adapt/retain decisions, not bulk deletion. |
| Policy/schema/assets include Event-only and generic material | `cerbos/`, `schemas/`, `assets/` | High | Their lifecycle is coupled to Phase 10 source deletion and regeneration. |
| Baseline build is green with pre-existing warnings | `dotnet build --configuration Release --verbosity quiet` on 2026-07-15: 0 errors, 89 warnings, 10.14s | High | Advisory/package warnings are outside this workstream. |

### 2.2 Existing Implementation

**Public and contributor surface:** `README.md`, `CODE_OF_CONDUCT.md`, `SECURITY-POLICY.md`, `LICENSE`, `legal/CLA.md`, `.github/PULL_REQUEST_TEMPLATE.md`, issue templates, and contribution docs form the repository's public contract. `LICENSE` is valid and must remain unchanged. The other files mix OpenISLAMU, ISLAMU Explore, and ISLAMU Event identities and contain stale links or product promises.

**Canonical documentation:** `docs/PROJECT.md`, `docs/ARCHITECTURE.md`, `docs/index.md`, and `docs/DOCUMENTATION_ARCHITECTURE.md` are the highest-impact stale files. `docs/ARCHITECTURE.md` still contains useful Clean Architecture, CQRS/MediatR, YARP BFF, PostgreSQL, HAL, authorization, and outbox explanations, but its Event examples and data flow are not the target product. `docs/index.md` currently routes readers to self-hosting, Event APIs, federation, tenants, and administration.

**Operations/configuration:** `docs/GETTING_STARTED.md`, `docs/CONFIGURATION.md`, `docs/OPERATIONS.md`, `docs/TESTING.md`, `docs/TROUBLESHOOTING.md`, `.env.example`, `docker-compose.yml`, Aspire launch metadata, and deployment workflows mix contributor-local, first-party production, and third-party operator guidance. The active init workstream is concurrently changing `local-lite` behavior and these docs, so their final rewrite must follow its current state rather than overwrite it.

**Retained infrastructure:** Mailpit is used for local SMTP capture and real runtime tests; Weblate is represented by Compose services plus localization provider schemas/clients; PostHog has a provider implementation, runtime selection, privacy/governance settings, and tests but no local service; Listmonk has provider configuration and durable integration-sync semantics. These slices are reusable website infrastructure even when Event-specific callers disappear.

**Agent/tool context:** `AGENTS.md`, `.claude/`, `.agents/`, `.codex/`, `.opencode/`, `.design-sync/`, and `.repomix/` contain both generic engineering guidance and Event-specific assumptions. Generic third-party command wrappers and examples are not product documentation and should remain unless they create an actual path, identity, or behavior defect.

**Policies/contracts/assets:** Cerbos resources and tests include Event-specific and generic resources. `schemas/islamu-event.md` and `schemas/openapi_islamu-event.json` are generated snapshots coupled to the code surface. Provider schemas for Weblate, Tolgee, and Listmonk are separate integration contracts. `assets/` contains ISLAMU logos plus Event screenshots and roadmap imagery.

### 2.3 Existing Tests And Verification Coverage

- Task 0.4 deleted five documentation/agent-context governance files containing 38 tests and removed one Markdown-backed API convention test.
- Surviving `Event.Architecture.Tests` enforce executable ABOUTME, Clean Architecture, accessibility, authorization, production-runtime, policy, and Phase 10 residue contracts without opening Markdown or `.claude/contract/intents.yaml`.
- Mailpit behavior has real SMTP/outbox/RabbitMQ tests under `tests/Explore.Infrastructure.Tests/Infrastructure/` and browser/API coverage referenced by `docs/TESTING.md` and `docs/EMAIL_NOTIFICATIONS.md`.
- PostHog configuration, capability, runtime-provider, consent, and edge-case behavior is covered across `tests/Event.Domain.UnitTests`, `tests/Event.Application.UnitTests`, `tests/Explore.Infrastructure.Tests`, and `tests/Explore.Blazor.Client.Tests`.
- Localization resilience and provider-generation behavior is covered by architecture/infrastructure tests and schema generation configuration.
- There is no test that proves every non-code artifact has a disposition. This plan's ledger and residual-search gate provide that missing review mechanism.

### 2.4 Existing Documentation And Contracts

The repository currently has approximately 90 top-level `docs/` Markdown/JSON artifacts, plus:

- Root public/legal/community files.
- `.claude`, `.agents`, `.codex`, `.github`, `.ci`, `.opencode`, `.design-sync`, and `.repomix` metadata.
- `.env.example`, Docker Compose files, launch metadata, package/tool configuration, and deployment workflows.
- `cerbos/**/*.yaml|json` policies, schemas, derived roles, and native tests.
- `schemas/` database/OpenAPI/provider contracts.
- `assets/` images and logos.
- `dev/` active, paused, journal, report, and process memory.

Generated OpenAPI/database snapshots are non-code artifacts but are source-generated contracts. Their disposition must be executed with the code owner, not edited manually.

### 2.5 Current Pain Points / Improvement Areas

1. **False product contract:** Canonical docs advertise a self-hostable Event platform that the repository will not support.
2. **Audience confusion:** Public users, contributors, first-party operators, third-party operators, and instance administrators are mixed together.
3. **Unsafe deletion coupling:** Canonical doc filenames are embedded in tests, links, source anchors, commands, workflows, and active workstream memory.
4. **Identity fragmentation:** OpenISLAMU, ISLAMU Explore, ISLAMU Event, `islamu-event-*`, and `islamu-ngo-*` coexist for different reasons; only some are stale.
5. **Configuration overload:** `.env.example` and operations docs expose Event, self-host, optional-provider, and first-party settings in one undifferentiated surface.
6. **Missing public privacy artifact:** `README.md` points to a nonexistent `PRIVACY-POLICY.md`; PostHog behavior requires factual consent/data-handling documentation before public enablement.
7. **Generated/manual drift:** Event OpenAPI/schema snapshots will become false after Phase 10 and cannot be safely rewritten before source deletion.
8. **Parallel-work collision:** Init Task 3.4 (canonical `local-lite` profile) is in progress and has modified `docs/GETTING_STARTED.md`, `docs/OPERATIONS.md`, `docs/SECRETS.md`, `src/Explore.AppHost/AppHost.cs`, `tests/Event.Architecture.Tests/AspireLocalInfrastructureArchitectureTests.cs`, and `tests/Explore.Blazor.Client.E2ETests/Fixtures/AppHostFixture.cs`. The init workstream also deleted `dev/active/webhook-delivery-redesign/*` and `dev/pause/performance-optimization/performance-optimization-context.md`, which currently cause pre-existing architecture-test failures (`EveryMustReadDocExists` and `CanonicalDocs_SourceAnchorsResolve`). Init Phases 10–12 are not started.
9. **Legal wording risk:** CLA, security response, privacy, and moderation commitments contain organization/status/contact assertions that require owner review.
10. **Over-retention risk:** Generic chassis documentation can preserve dead product capability if it has no surviving caller, owner, or named future use.
11. **Missing experience contract:** Existing design guidance is an Event administration design system. It does not yet define the public ISLAMU story, content hierarchy, emotional tone, participation journey, or boundary between committed and future capabilities.
12. **Unbounded “much more” risk:** Without capability horizons, aspirational language can create dead navigation, false public promises, and speculative infrastructure retention.

### 2.6 Unknowns After Investigation

| Unknown | Evidence/search | Resolution task |
|---|---|---|
| Exact chassis surviving Phase 10 | Init workstream has not created the caller/ownership ledger yet | 0.3, 3.1, 7.1 |
| Final first-party production topology | Init Phase 9 is deferred until after Phases 10-12 | 4.4, 6.3 |
| Tolgee disposition after Weblate is selected | Both providers and schemas exist; user explicitly retained Weblate only | 4.1; keep Tolgee as `Needs code decision` until the code ledger resolves it |
| Whether Listmonk remains after Event registration deletion | Listmonk integration exists; waitlists/newsletters are named future needs | 4.3; retain the reusable integration unless Phase 10 proves it inseparable from Event registration |
| Final tenancy semantics | Init decision U5 remains narrowed but unresolved | 2.4, 5.2; do not publish multi-tenant product claims |
| Approved public privacy/cookie text and contact details | No privacy file exists; current contact/legal claims are unverified | 1.4; factual technical inventory first, owner/legal approval before publication |
| Which deploy workflows survive | Two Coolify workflows and production-posture work remain | 6.3; retain disabled or adapt only after target topology is known |
| Final public content, proof points, and calls to action | Repository evidence establishes technical capability, not approved organizational copy, impact claims, imagery rights, or campaign priorities | 2.6; create a content/route truth inventory, use owner-supplied evidence, and omit unsupported claims |

## 3. Proposed Future State

### Product And Experience North Star

The target public experience is **purposeful, trustworthy, culturally aware, and useful**. It should feel recognizably ISLAMU without relying on decorative Islamic motifs, generic charity templates, or high-motion marketing effects. The experience contract is:

1. **Purpose before promotion:** explain why ISLAMU exists and what a visitor can truthfully do now before presenting calls to action.
2. **Editorial depth:** use the Blog as the first durable publishing surface for useful, source-owned content rather than filling the site with static marketing claims.
3. **Participation over passive browsing:** every public journey should end in a real next step, such as reading, contacting, subscribing, or joining a future waitlist, but only when that route and consent flow exist.
4. **Multilingual parity:** English, French, and Arabic/RTL content and interaction are first-class. Weblate remains the translation source; no locale ships as a visibly incomplete afterthought.
5. **Calm, accessible interactivity:** rich interaction supports orientation and comprehension. It uses semantic HTML, keyboard/touch parity, reduced motion, strong focus, and the Phase 12 disclosure-navigation model; no hover-only, autoplay, gratuitous parallax, or animation-only meaning.
6. **Privacy-respecting trust:** analytics and email engagement are consent-aware, non-essential features fail safely, and public claims match verified data flows.
7. **Fast and resilient by default:** content remains understandable during provider failure, missing translation, disabled analytics, or constrained devices. SEO, performance, and accessibility are launch criteria, not later polish.

### Capability Horizons

| Horizon | Status | Public treatment | Owner |
|---|---|---|---|
| Foundation: Event-free website shell, factual public identity, contributor/operator truth | Committed by this and init workstreams | May be described as implemented only after its acceptance gates pass | Init Phase 10 plus this plan |
| Publish: Blog, public discovery/SEO, accessible ISLAMU navigation | Committed | Label Planned until init Phases 11-12 pass; then promote with verified routes and content owners | Init Phases 11-12 plus this plan |
| Engage: waitlists, newsletters, contact and transactional email journeys | Directionally approved, not yet implemented | Preserve infrastructure and describe as future capability; no dead CTA or collection form | Separate approved feature workstreams |
| Expand: additional ISLAMU programs, community services, donations, tools, or personalized experiences | Discovery only | Do not name as a product commitment, add routes, or retain speculative dependencies | Future PRD/workstream per capability |

Every public claim, route, navigation item, CTA, locale, analytics event, and email capture point must appear in a content/route truth inventory with an owner and one state: Implemented, Planned, or Deferred. `Planned` and `Deferred` items do not become clickable placeholders.

### Repository Information Architecture

The repository will expose one coherent non-code information architecture:

```text
Public visitor / contributor
  -> README.md
     -> ISLAMU website purpose and current implemented capabilities
     -> contributor setup and contribution/security/legal links
     -> factual provenance and AGPL continuity

Contributor
  -> docs/index.md
     -> GETTING_STARTED / CONTRIBUTING / TESTING
     -> ARCHITECTURE / BLAZOR / API for surviving website behavior
     -> design, accessibility, localization, email, analytics

First-party operator
  -> OPERATIONS / PRODUCTION_OPERATIONS / CONFIGURATION / SECRETS
     -> one ISLAMU deployment
     -> Weblate, external PostHog, Mailpit-local-only, email/Listmonk
     -> no third-party self-hosting promise

Agent
  -> AGENTS.md -> intents/rules/skills -> source-grounded docs
     -> no Event-product assumptions unless explicitly historical/provenance
```

Every in-scope artifact will have one disposition in the context ledger:

- **Retain unchanged:** already accurate and load-bearing.
- **Rewrite:** path remains but content/audience/identity changes.
- **Merge then delete:** unique useful content moves to a named destination before deletion.
- **Delete:** Event-only, duplicate, parked, or unreferenced material.
- **Regenerate:** generated artifact rebuilt from surviving source/contracts.
- **Archive:** historically valuable but non-canonical record retained with explicit status.
- **Needs code decision:** no content action until the init workstream proves surviving ownership.

Each row also records an alignment reason from the init workstream methodology: Mandatory parity, Reusable pattern, islamu.ngo adaptation, Intentional exception, or Not applicable. Disposition answers **what to do**; alignment answers **why**.

## 4. Non-Negotiable Constraints

- This repository is one first-party ISLAMU website/webapp deployment, not a supported self-hostable or white-label product.
- Public AGPL-3.0-or-later licensing remains; `LICENSE` is retained unchanged.
- `README.md` retains source repository SHA `5cfa996b2344969856946955270dd333e06fc61d` and factual AGPL provenance.
- Weblate, PostHog, and Mailpit are retained. Mailpit is local/test infrastructure, not a production email provider. PostHog retention does not imply a new local Compose service.
- Event-only non-code artifacts are deleted, not hidden behind compatibility aliases.
- Product source/test implementation remains owned by `dev/active/init-import-islamu-event/`; this workstream must not race it.
- No generated OpenAPI/database artifact is manually edited.
- No file is deleted until tracked references, Markdown links, source anchors, workflow references, test constants, and active dev docs have been checked.
- Canonical docs separate Implemented, Planned, and Historical behavior and include required metadata/source anchors.
- Root/community/agent files use two leading `ABOUTME:` comments when modified.
- Security, privacy, CLA, governance, and response-time claims require factual source evidence and owner/legal approval where they create commitments.
- Secret names may be documented; secret values, credentials, private endpoints, tenant data, and PII must not be copied into docs.
- HAL, authorization, tenant isolation, outbox, retry/idempotency, and observability descriptions may be retained only when the corresponding implementation survives.
- Current unrelated dirty-worktree changes must not be reverted or overwritten.
- No public route, navigation item, CTA, campaign, impact statistic, testimonial, or capability claim is added without an implemented destination and an accountable content owner.
- “Much more” is governed through capability horizons and separate feature workstreams; it is not permission for speculative abstractions, providers, pages, or configuration.

## 5. Architecture And Design Decisions

### Decision 1: Use One Canonical Disposition Ledger

- **Decision:** Maintain the ledger in `islamu-website-non-code-refactor-context.md`, first by artifact family and then by exact path before edits.
- **Why:** The three required dev docs remain the only new planning artifacts, and future agents have one resumable source of truth.
- **Alternatives considered:** A separate generated report. Rejected until volume proves the context ledger unmanageable.
- **Consequences:** Every execution slice updates context before and after changes; no unledgered deletion is allowed.
- **Files/layers affected:** This workstream context plus all in-scope non-code families.

### Decision 2: Separate Disposition Ownership From Code Ownership

- **Decision:** This plan owns non-code truth and deletion. The init plan owns code, runtime, migrations, and generation.
- **Why:** It prevents duplicate Phase 10-12 work and avoids documentation describing behavior that is still moving.
- **Alternatives considered:** Fold all work into the init plan. Rejected because the non-code surface is repository-wide and needs an independent, auditable ledger.
- **Consequences:** Shared boundaries use explicit handoff gates; source-coupled artifacts remain `Needs code decision` until the init ledger is current.
- **Files/layers affected:** `dev/active/init-import-islamu-event/`, generated schemas, Cerbos, configuration, canonical docs.

### Decision 3: Rewrite Semantics, Never Global-Replace Branding

- **Decision:** Rewrite each retained artifact for its target audience and current behavior.
- **Why:** Event/self-host terms often encode real architecture and cannot be safely renamed without changing meaning.
- **Alternatives considered:** Repository-wide textual replacement. Rejected as unsafe and likely to preserve false contracts.
- **Consequences:** Residual searches use an allowlist for provenance, historical records, technical project names, and still-live source identifiers.

### Decision 4: Delete The Self-Hosting Product Contract

- **Decision:** Merge valid contributor-local setup into `GETTING_STARTED.md`; merge first-party operating procedures into `OPERATIONS.md` and the init plan's future `PRODUCTION_OPERATIONS.md`; then delete `SELF_HOSTING.md`. Delete or merge `DEPLOYMENT_MODES.md` and `DEPLOYMENT_TIERS.md` unless surviving runtime behavior needs an internal technical reference.
- **Why:** These files advertise unsupported distribution/product promises.
- **Alternatives considered:** Add a “not supported” banner and keep them. Rejected because it leaves dead navigation and maintenance burden.
- **Consequences:** Documentation quality test constants and all links must be updated in the paired code-owner slice before deletion lands.

### Decision 5: Retain Infrastructure By End-To-End Slice

- **Decision:** Retain Weblate, PostHog, Mailpit, and reusable email/Listmonk assets only when runtime/config/docs/tests/policy/schema ownership is coherent.
- **Why:** A leftover Compose service or provider document alone is not useful infrastructure.
- **Alternatives considered:** Keep all imported providers “for later.” Rejected by YAGNI and the first-party posture.
- **Consequences:** Tolgee and other providers remain undecided until caller evidence exists; Weblate is the documented first-party localization default.

### Decision 6: Do Not Add Local PostHog

- **Decision:** Preserve the existing external/configurable PostHog integration and document first-party endpoint, consent, CSP, relay/proxy, and failure behavior; do not add a PostHog Compose stack.
- **Why:** PostHog code and tests exist, but no current Compose service does. Adding infrastructure is not required to retain the capability.
- **Alternatives considered:** Add a full local PostHog deployment. Rejected as unrequested operational scope.

### Decision 7: Privacy Publication Is Approval-Gated

- **Decision:** Remove the broken README privacy link until a factual `PRIVACY-POLICY.md` is approved; create the file only from a verified data-flow inventory and owner/legal-approved commitments.
- **Why:** A fabricated privacy policy is worse than an explicit missing-publication state.
- **Alternatives considered:** Publish a generic template. Rejected because PostHog, authentication, email, localization, and hosting data flows are repository-specific.

### Decision 8: Marketing Is The Entry Layer, Not The Product Boundary

- **Decision:** Define the first-party ISLAMU digital-front-door experience and capability horizons in `docs/PROJECT.md`, with the public atmosphere and interaction principles in `docs/DESIGN.md`. Marketing explains purpose and creates truthful paths into content and participation; it is not a brochure, an open-ended roadmap, or a speculative feature marketplace.
- **Why:** The website needs a recognizable ISLAMU experience while remaining extensible beyond marketing. A governed content/route inventory prevents “much more” from becoming dead links, unsupported claims, or infrastructure retained for imagined features.
- **Alternatives considered:** Treat the website as a static marketing brochure, or publish placeholders for future services. Both were rejected because the first limits the webapp direction and the second damages trust.
- **Consequences:** Every public claim, route, navigation item, CTA, locale, analytics event, and email capture has an owner and an `Implemented`, `Planned`, or `Deferred` state. Only implemented destinations are clickable. Planned capabilities require a separate approved workstream before publication.
- **Files/layers affected:** `docs/PROJECT.md`, `docs/DESIGN.md`, `docs/DESIGN_SYSTEM.md`, `docs/ACCESSIBILITY.md`, `docs/LOCALIZATION.md`, `docs/SEO.md`, `docs/RENDER_POLICIES.md`, and the init Phase 12 navigation handoff.

## 6. Implementation Phases

### Phase 0: Approval, Baseline, And Disposition Control — COMPLETE

- **Goal:** Re-read moving dependencies and establish an exact path-level ledger before any destructive edit.
- **Depends on:** Planning gate completed on 2026-07-15; execution starts with Task 0.1 and does not repeat approval.
- **Relevant files:** This workstream; init workstream; repository non-code inventory.
- **Acceptance criteria:** Every tracked non-code family is represented; shared ownership is explicit; baseline commands and dirty files are recorded.
- **Verification:** `git status --short`; artifact counts/searches recorded in context; requested build/test gates attempted with current shared-worktree blockers recorded rather than bypassed.
- **Rollback / failure handling:** Planning/ledger edits are reversible; no production artifact changes occur in this phase.

#### Task 0.1: Refresh Contracts And Concurrent State ✅ COMPLETE

- **Type:** investigate
- **Layer:** Docs/DevOps
- **Files:** `AGENTS.md`, `.claude/contract/intents.yaml`, canonical docs, both active workstreams
- **Description:** Re-read current files because Task 3.4 and later init phases may have changed since planning. Record new dirty files and do not overwrite concurrent work. **Corrected refresh evidence:** Init Task 3.4 targeted checks passed, while the canonical sweep retained 7 architecture and 6 API failures unrelated to Task 3.4. Cloud-backed health remained unverified because of the duplicate `webhook_endpoints.configuration_version` column and missing external `ConnectionStrings:cache`; Keycloak login smoke was not run. Init Phases 10 and 11 were unstarted; Phase 12 implementation was unstarted but Task 12.0 research was complete. Context7 official `/microsoft/aspire.dev` guidance was successfully consulted; Tavily MCP alone was unavailable. The rejected delegated refresh artifact is not authoritative; the current context and init tasks/ledger govern.
- **Acceptance Criteria:** Current init priority/status and shared-file owners are recorded; no stale phase assumption remains.
- **Dependencies:** None; planning gate completed on 2026-07-15.
- **Effort:** S
- **Required Skills/Rules:** `agentic-research`; path-scoped rules discovered during refresh.
- **Validation:** Context handoff contains current SHA/status and active init task. Artifact: `.omo/start-work/artifacts/task-0.1/refresh-summary.md`.

#### Task 0.2: Expand The Canonical Disposition Ledger ✅ COMPLETE

- **Type:** investigate/docs
- **Layer:** Docs/DevOps
- **Files:** All tracked non-code paths, grouped under root, `docs/`, agent/tool context, `.github/.ci`, orchestration/config, `cerbos/`, `schemas/`, `assets/`, and `dev/`
- **Description:** Expanded the initial family ledger in context to exact paths or unambiguous globs. Each row assigns disposition, owner/dependency, destination/rule, and evidence/status; generated/vendor/source-code exclusions are explicit.
- **Acceptance Criteria:** No tracked in-scope artifact lacks a disposition; generated/vendor exclusions are explicit; uncertain rows are `Needs code decision`, not guessed.
- **Dependencies:** 0.1
- **Effort:** L
- **Validation:** Compare ledger coverage against `git ls-files` grouped by extension/path; review zero unmatched in-scope files.

#### Task 0.3: Establish The Init-Plan Handoff Matrix ✅ COMPLETE

- **Type:** docs/investigate
- **Layer:** Docs/DevOps
- **Files:** Both workstream docs; source-coupled rows in the disposition ledger
- **Description:** Mapped each source-coupled artifact to init Phase 9, 10, 11, or 12 and named the evidence required before final content/deletion. The matrix covers Cerbos, OpenAPI/database schemas, API inventories, Event ADRs, email fanout, routes, navigation assets, `SELF_HOSTING.md`, and first-party deployment.
- **Acceptance Criteria:** Each shared row has one owner and one completion signal; this plan does not duplicate code tasks.
- **Dependencies:** 0.1, 0.2
- **Effort:** M
- **Validation:** No task in this plan edits product source; no shared artifact has two simultaneous owners.

#### Task 0.4: Remove Prose Governance From Executable Architecture Tests ✅ COMPLETE

- **Type:** delete/modify tests/docs/workflow
- **Layer:** Test/Docs
- **Files:** five deleted governance test files, `ContextSystemHelpers.cs`, `ApiConventionTests.cs`, `Phase10EventResidueGuardrailsTests.cs`, `.github/workflows/agent-context.yml`, and governance docs
- **Description:** Removed architecture tests that parsed prose and intent manifests, their exclusive workflow, one Markdown-backed API assertion, and dead parser helpers. Preserved code/configuration architecture and Phase 10 residue gates.
- **Acceptance Criteria:** No architecture test opens Markdown or `.claude/contract/intents.yaml`; documentation metadata/link/source-anchor/schema rules remain explicit author/reviewer contracts.
- **Dependencies:** 0.2, 0.3
- **Effort:** S
- **Validation:** Residual source inspection passed. Debug/Release architecture runs and the Release build were attempted on 2026-07-18; execution is externally blocked by five concurrent Domain constant-reference errors and two unavailable WebAssembly restore packages, as recorded in context.

### Phase 1: Public, Legal, And Community Entry Points

- **Goal:** Make the repository's public face truthful without waiting for all code teardown.
- **Depends on:** Tasks 0.1-0.3. Task 0.4 gates canonical-doc deletion, not source-independent public-entrypoint rewrites.
- **Relevant files:** `README.md`, `CODE_OF_CONDUCT.md`, `SECURITY-POLICY.md`, `legal/CLA.md`, `docs/legal/CONTRIBUTION_GOVERNANCE.md`, `.github` templates/workflow.
- **Acceptance criteria:** No broken public links; first-party website positioning; provenance/license continuity; legal claims approval-gated.
- **Verification:** Markdown link checks, architecture tests, manual rendered review.
- **Rollback / failure handling:** Preserve prior legal text in git history; do not publish unapproved commitments.

#### Task 1.1: Rewrite `README.md` ✅ COMPLETE

- **Type:** modify/docs
- **Layer:** Docs
- **Files:** `README.md` (existing)
- **Description:** Replace OpenISLAMU/Event/self-host language with a concise ISLAMU website overview; clearly separate implemented website capabilities from planned Blog/navigation/features; add correct contributor setup/docs/security/license links; preserve Palestine support content only if still owner-approved; retain imported source SHA and AGPL provenance.
- **Acceptance Criteria:** No broken privacy or MudBlazor links; no self-host promise; no false “implemented” feature claims; provenance is intact.
- **Dependencies:** 0.2
- **Effort:** M
- **Validation:** Rendered Markdown review; link graph test; residual identity search with allowlist.

#### Task 1.2: Re-baseline Community And Security Policies — IMPLEMENTED, OWNER CONFIRMATION PENDING

- **Type:** modify/docs
- **Layer:** Docs/Security
- **Files:** `CODE_OF_CONDUCT.md`, `SECURITY-POLICY.md`
- **Description:** Add ABOUTME headers; normalize ISLAMU website/project identity; verify reporting contact and remove informal/undefined response commitments; retain upstream attribution and responsible-disclosure safeguards.
- **Acceptance Criteria:** Contact/status claims are owner-approved; Code of Conduct attribution/license remains; security scope matches the first-party service and public repository.
- **Dependencies:** 0.2
- **Effort:** M
- **Validation:** Manual policy review; links resolve; no placeholder or undefined commitment remains.

#### Task 1.3: Re-baseline CLA And Contribution Governance — IMPLEMENTED, LEGAL/OWNER APPROVAL PENDING

- **Type:** modify/docs/workflow
- **Layer:** Docs/DevOps/Legal
- **Files:** `legal/CLA.md`, `docs/legal/CONTRIBUTION_GOVERNANCE.md`, `.github/workflows/cla.yml`, `.github/PULL_REQUEST_TEMPLATE.md`
- **Description:** Replace Event product identity with the ISLAMU website/repository legal subject only after owner/legal confirmation. Preserve version/signature continuity or deliberately issue a new CLA version and signature path; never silently invalidate existing signatures.
- **Acceptance Criteria:** CLA text, signing comment, workflow path, PR template, branch, and repository URL agree; AGPL relationship remains accurate; legal review status is explicit.
- **Dependencies:** 0.2; owner/legal decision.
- **Effort:** L
- **Validation:** Workflow YAML parse/security review; exact signing text search; architecture link checks.

#### Task 1.4: Resolve Privacy And Cookie Publication — INVENTORY COMPLETE, PUBLICATION APPROVAL PENDING

- **Type:** investigate/create/docs
- **Layer:** Docs/Security/Privacy
- **Files:** `PRIVACY-POLICY.md` (new only after approval), `README.md`, analytics/email/auth/localization configuration docs
- **Description:** Inventory data handled by Keycloak/BFF, PostHog, email/Listmonk, Weblate, logs/metrics, hosting, and cookies. Draft factual public text and obtain owner/legal approval. Until approved, omit the broken README link rather than publish generic claims.
- **Acceptance Criteria:** Published text names actual controller/contact/retention/consent/withdrawal behavior and external processors only when verified; PostHog replay/autocapture/heatmap defaults remain off unless separately approved.
- **Dependencies:** 1.1, 4.2
- **Effort:** L
- **Validation:** Evidence-to-claim review; public link check; analytics configuration matches the approved notice.

### Phase 2: Canonical Product And Documentation Architecture

- **Goal:** Replace the Event/self-host information architecture with a first-party website documentation system.
- **Depends on:** Tasks 0.1-0.3; can proceed with explicit Implemented/Planned boundaries before source teardown completes. Task 0.4 gates final canonical deletions.
- **Relevant files:** `docs/PROJECT.md`, `docs/DESIGN.md`, `docs/DESIGN_SYSTEM.md`, `docs/ACCESSIBILITY.md`, `docs/LOCALIZATION.md`, `docs/SEO.md`, `docs/RENDER_POLICIES.md`, `docs/ARCHITECTURE.md`, `docs/index.md`, `docs/DOCUMENTATION_ARCHITECTURE.md`, style/governance/contributor docs.
- **Acceptance criteria:** Canonical docs identify audiences, current behavior, source anchors, capability horizons, public experience principles, and future-state boundaries accurately.
- **Verification:** Documentation quality tests; link graph; source-anchor resolution; manual audience walk-through.
- **Rollback / failure handling:** Keep uncertain behavior labeled Planned/Needs Investigation rather than guessing.

#### Task 2.1: Rewrite Product Definition ✅ COMPLETE

- **Type:** modify/docs
- **Layer:** Docs/Product
- **Files:** `docs/PROJECT.md`
- **Description:** Define the ISLAMU digital front door, where marketing is the entry layer into trustworthy content and real participation. Record first-party deployment, public/contributor/operator audiences, the Foundation/Publish/Engage/Expand capability horizons, the current implemented chassis, Blog when implemented, and explicit non-goals including Event, white-label, third-party self-hosting, and a speculative feature marketplace.
- **Acceptance Criteria:** No Event lifecycle/federation/white-label product promise; current, planned, and deferred capabilities are separated; the website is not framed as a static brochure or an ownerless roadmap; source anchors resolve.
- **Dependencies:** 0.3
- **Effort:** M
- **Validation:** Metadata/source-anchor tests; cross-check against init Phase 10-12 state.

#### Task 2.2: Rewrite Surviving Architecture — TRANSITION FRAMING COMPLETE, FINAL INIT HANDOFF PENDING

- **Type:** modify/docs
- **Layer:** Docs/Architecture
- **Files:** `docs/ARCHITECTURE.md`, `docs/CODEBASE_STRUCTURE.md`, relevant diagrams
- **Description:** Preserve only surviving Clean Architecture, CQRS/MediatR, InteractiveServer/YARP BFF, HAL, PostgreSQL, authorization, outbox, localization, analytics, email, observability, and deployment flows. Replace Event examples with verified website/Blog examples after Phase 10-11.
- **Acceptance Criteria:** Every named class/path exists; no removed Event use case is presented as current; planned Blog flow is not marked implemented before Phase 11.
- **Dependencies:** 0.3; final pass depends on init Phases 10-11.
- **Effort:** L
- **Validation:** Source-anchor test plus explicit path search; architecture review against code ledger.

#### Task 2.3: Rebuild Documentation Index And Audience Model — CORE REWRITE COMPLETE, FINAL DISPOSITION PASS PENDING

- **Type:** modify/docs
- **Layer:** Docs
- **Files:** `docs/index.md`, `docs/DOCUMENTATION_ARCHITECTURE.md`, `docs/DOCUMENTATION_STYLE_GUIDE.md`
- **Description:** Route public/product readers, contributors, and first-party operators to distinct entrypoints. Remove third-party evaluator/operator and self-hosting routes. Retain Diátaxis, metadata, source-anchor, impact, and drift policies.
- **Acceptance Criteria:** Every indexed file exists and has the intended status; no deleted doc is linked; ownership categories cover surviving docs.
- **Dependencies:** 2.1; final pass after Phases 3-7.
- **Effort:** L
- **Validation:** Architecture/link tests and manual navigation from README to each audience destination.

#### Task 2.4: Re-baseline Governance And Quick Reference — FIRST-PARTY FRAMING COMPLETE, PHASE 10 FINAL PASS PENDING

- **Type:** modify/docs
- **Layer:** Docs/Agent Context
- **Files:** `docs/GOVERNANCE.md`, `docs/QUICK_REFERENCE.md`, `AGENTS.md`
- **Description:** Replace Event product assumptions while preserving surviving engineering invariants. Mark multi-tenancy/federation/Event rules applicable only if code still exists. Keep ABOUTME, verification, HAL, authorization, and Clean Architecture constraints where valid.
- **Acceptance Criteria:** Agent entrypoint says ISLAMU website/webapp; no rule commands future agents to preserve deleted Event behavior; invariants still match architecture tests.
- **Dependencies:** 0.3; final pass after init Phase 10.
- **Effort:** L
- **Validation:** Source-code architecture tests plus `/docs-lint` and manual intent/rule/skill schema/link review.

#### Task 2.5: Re-baseline Contributor Documentation — PARTIAL, CONCURRENT DOC HANDOFF PENDING

- **Type:** modify/merge/delete docs
- **Layer:** Docs
- **Files:** `docs/GETTING_STARTED.md`, `docs/CONTRIBUTING.md`, `docs/FIRST_CONTRIBUTION.md`, `docs/TESTING.md`, `docs/TROUBLESHOOTING.md`
- **Description:** Describe current `local-lite` and any retained local-full profile, correct all test project paths, remove Event feature journeys, and keep provider-specific runtime tests only when the provider survives.
- **Acceptance Criteria:** A fresh contributor can build/test/run the website without following self-host instructions; commands are repository-root correct and use TUnit syntax.
- **Dependencies:** Init Task 3.4; 3.1; 4.1-4.3.
- **Effort:** L
- **Validation:** Execute documented build/architecture commands and one canonical dev startup smoke; no unsupported `dotnet test --filter` examples.

#### Task 2.6: Establish The ISLAMU Experience And Content Contract — DOCUMENT CONTRACT COMPLETE, ROUTE HANDOFF PENDING

- **Type:** modify/docs/product
- **Layer:** Docs/Product/Design
- **Files:** `docs/PROJECT.md`, `docs/DESIGN.md`, `docs/DESIGN_SYSTEM.md`, `docs/ACCESSIBILITY.md`, `docs/LOCALIZATION.md`, `docs/SEO.md`, `docs/RENDER_POLICIES.md`
- **Description:** Translate the digital-front-door north star into an executable public experience contract. Define the public journey from purpose to content to real participation, then maintain a content/route inventory with route or CTA, audience need, owner, state (`Implemented`, `Planned`, or `Deferred`), locale readiness, analytics/consent impact, source evidence, and owning implementation workstream. Preserve the existing token/layer/wrapper foundations while replacing the Event admin “command center” atmosphere with a calm, editorial, values-led ISLAMU web experience. Actual pages, components, routes, and features remain owned by init Phases 11-12 or future approved workstreams.
- **Acceptance Criteria:** The contract is recognizably ISLAMU rather than Event administration or generic marketing; only implemented destinations may be linked; no ownerless claim, impact statistic, testimonial, campaign, image-rights assertion, or placeholder CTA is published; English/French/Arabic and RTL are first-class; WCAG 2.2 AA, keyboard/touch, 200% zoom, reduced motion, fast/resilient delivery, truthful SEO, privacy/consent, and editorial ownership are launch gates.
- **Dependencies:** 2.1; init Phase 12 route inventory for final route states.
- **Effort:** L
- **Validation:** Review the content/route inventory against implemented routes and init handoffs; manually walk the public journey in all three locales; run documentation metadata/link/source-anchor checks. UI/browser validation remains in the owning implementation workstream.

### Phase 3: Event And Self-Hosting Documentation Removal

- **Goal:** Merge useful chassis knowledge and delete Event-only/self-host product documents after source ownership is proven.
- **Depends on:** Init Phase 10 ledger and relevant deletions; Phase 0 deletion gates.
- **Relevant files:** Event feature docs, self-host/deployment docs, Event ADRs, API inventories, release notes, parked docs site.
- **Acceptance criteria:** No canonical Event product docs remain; historical/provenance exceptions are explicit; no links/source anchors break.
- **Verification:** Architecture tests, residual searches, generated-contract checks.
- **Rollback / failure handling:** Restore the deleted file from git if a missed load-bearing reference appears; classify and fix the dependency before retrying.

#### Task 3.1: Classify Every Event-Oriented Doc Against The Code Ledger

- **Type:** investigate/docs
- **Layer:** Docs
- **Files:** `docs/*.md`, `docs/adr/**`, `docs/semantic_versioning/**`, generated inventories
- **Description:** Assign exact disposition after the init Phase 10 caller/ownership ledger. Strong delete candidates include `MODULAR_EVENTS.md`, `TEMPLATE_SYNC.md`, `CONTACT_SHARING.md`, Event-only Coop/Osprey/moderation/federation content, Event release notes, and Event-only ADRs.
- **Acceptance Criteria:** Each candidate cites surviving owner or deletion evidence; no generic event/outbox technical term is removed solely by name.
- **Dependencies:** Init 10.1; 0.2.
- **Effort:** L
- **Validation:** Ledger review against code references and docs links.

#### Task 3.2: Merge And Delete Self-Hosting/Deployment Product Docs — MODES/TIERS DELETED, SELF-HOST GUIDE CODE-COUPLED

- **Type:** merge/delete docs
- **Layer:** Docs/DevOps
- **Files:** `docs/SELF_HOSTING.md`, `docs/DEPLOYMENT_MODES.md`, `docs/DEPLOYMENT_TIERS.md`, destinations `GETTING_STARTED.md`, `OPERATIONS.md`, future `PRODUCTION_OPERATIONS.md`
- **Description:** Move only verified contributor-local and first-party operational content, then delete unsupported product docs. If deployment mode remains a runtime implementation detail, document it narrowly inside configuration/architecture without a product tier narrative.
- **Acceptance Criteria:** No self-host product entrypoint remains; all unique valid commands/settings have a named destination; paired architecture-test path list is updated by code owner.
- **Dependencies:** 0.4, 2.5, init Phase 9/10 evidence.
- **Effort:** L
- **Validation:** Link/source-anchor search and architecture tests.

#### Task 3.3: Delete Event-Only Docs And Parked Docs Site — PARKED SITE DELETED, EVENT BATCHES PENDING

- **Type:** delete/archive docs
- **Layer:** Docs
- **Files:** Rows classified Delete under `docs/`; `docs/docs-website/` unless a hosted-docs requirement is approved
- **Description:** Delete Event-only and duplicate material in reviewable batches. Archive only durable decision/provenance records that explain current architecture; archived files must not appear as current guidance.
- **Acceptance Criteria:** Every deletion has a ledger row and reference check; the parked empty docs site is removed unless explicitly needed.
- **Dependencies:** 3.1, 0.4.
- **Effort:** L
- **Validation:** Architecture tests after each batch; zero missing Markdown targets.

#### Task 3.4: Regenerate Or Remove Event Contracts And Inventories

- **Type:** coordinate/regenerate/delete
- **Layer:** Docs/API/Schema
- **Files:** `docs/API_CONTRACT_INVENTORY.md`, `docs/AI_AGENT_CONTRACT_INVENTORY.md`, Event schema/inventory outputs
- **Description:** Use the owning source/test generators after init Phase 10. Delete outputs whose generator/use case is removed; regenerate surviving inventories from the post-Event source.
- **Acceptance Criteria:** No hand-edited generated output; generation command and source anchor are recorded; Event routes/types are absent except allowlisted history.
- **Dependencies:** Init 10.2-10.4.
- **Effort:** M
- **Validation:** Generator tests, OpenAPI snapshot checks, residual Event route/type search.

### Phase 4: Retained Infrastructure, Configuration, And Operations

- **Goal:** Preserve useful website infrastructure while removing self-host/operator choice framing and Event-specific callers.
- **Depends on:** Init code ledger and Task 3.4 `local-lite` completion.
- **Relevant files:** localization, analytics, email/Listmonk, config/secrets/operations, Compose/Aspire metadata, provider schemas.
- **Acceptance criteria:** Retained slices have coherent runtime/config/docs/test ownership; first-party defaults and privacy boundaries are explicit.
- **Verification:** Format validation, architecture tests, targeted provider tests, documented startup/health checks.
- **Rollback / failure handling:** Keep unresolved providers disabled and `Needs code decision`; do not delete a working adapter before its replacement/default is proven.

#### Task 4.1: Make Weblate The Documented Localization Default — DOCUMENT FRAMING COMPLETE, PROVIDER/CONFIG HANDOFF PENDING

- **Type:** modify/merge/docs/config
- **Layer:** Infrastructure/Docs/DevOps
- **Files:** `docs/LOCALIZATION.md`, `docs/CONFIGURATION.md`, `docs/OPERATIONS.md`, `.env.example`, `docker-compose.yml`, Weblate provider schemas/config
- **Description:** Retain Weblate Compose/provider/runtime/test artifacts and rewrite self-hoster tiers as first-party local/production operating modes. Resolve Tolgee through caller evidence; do not delete its schema/client while code still supports it.
- **Acceptance Criteria:** Weblate local startup, credentials ownership, fallback/offline bundles, health/failure behavior, and secret redaction are documented; Tolgee has an explicit ledger status.
- **Dependencies:** 0.3; init Phase 10 provider ledger.
- **Effort:** L
- **Validation:** `docker compose config`; localization tests/generation checks; source anchors resolve.

#### Task 4.2: Reframe PostHog For First-Party Analytics — PRIVACY INVENTORY COMPLETE, CONFIG/OPS HANDOFF PENDING

- **Type:** modify/docs/config
- **Layer:** Infrastructure/Blazor/Privacy/Docs
- **Files:** analytics sections in `CONFIGURATION.md`, `OPERATIONS.md`, `BLAZOR.md`, security/privacy docs, `.env.example`
- **Description:** Preserve PostHog provider/feature-flag/relay/proxy configuration and failure isolation. Remove self-hoster provider-marketplace language. Select the actual first-party endpoint/transport through configuration, default non-essential capture features off, and document consent/withdrawal/CSP/data minimization.
- **Acceptance Criteria:** No local PostHog service is added; normal product flows remain independent of analytics; privacy publication matches active settings.
- **Dependencies:** Init Phase 10 analytics retention; 1.4.
- **Effort:** L
- **Validation:** Existing analytics unit/infrastructure/client tests; config key search; manual consent/disable behavior review.

#### Task 4.3: Reframe Mailpit, Email, And Listmonk — DOCUMENT BOUNDARY COMPLETE, CALLER HANDOFF PENDING

- **Type:** modify/merge/docs/config
- **Layer:** Infrastructure/Docs/DevOps
- **Files:** `docs/EMAIL_NOTIFICATIONS.md`, `docs/OUTBOX_PATTERN.md`, `docs/CONFIGURATION.md`, `docs/OPERATIONS.md`, `docs/TESTING.md`, `.env.example`, Compose/Aspire metadata, Listmonk schema
- **Description:** Keep Mailpit as local/test SMTP capture and preserve durable outbox/retry/idempotency/error-handling guidance that survives Event deletion. Remove Event notification fanout examples. Retain Listmonk as reusable waitlist/newsletter synchronization unless code evidence proves it inseparable from Event registration.
- **Acceptance Criteria:** Mailpit is never documented as production SMTP; email bodies/PII/secrets remain out of logs; unsubscribe/consent semantics are explicit for future marketing email; Event reminder/publication jobs are removed after code deletion.
- **Dependencies:** Init Phase 10 email ledger.
- **Effort:** L
- **Validation:** Email/Mailpit target tests where affected; Compose/Aspire resource validation; source-anchor checks.

#### Task 4.4: Trim Configuration And Operations To First-Party Needs — PARTIAL, CONCURRENT AND INIT HANDOFFS PENDING

- **Type:** modify/merge docs/config
- **Layer:** DevOps/Docs
- **Files:** `.env.example`, `docs/CONFIGURATION.md`, `docs/SECRETS.md`, `docs/OPERATIONS.md`, `docs/TROUBLESHOOTING.md`, future `docs/PRODUCTION_OPERATIONS.md`
- **Description:** Remove settings only after their code/service owner is removed; regroup remaining settings by contributor-local, first-party deployment, and optional integration; replace self-host instructions with internal operational ownership.
- **Acceptance Criteria:** Every documented key resolves to current code/config; every required runtime key is documented; example values are safe; production posture does not bundle unapproved infrastructure.
- **Dependencies:** 3.1, 4.1-4.3, init Phase 9/10.
- **Effort:** XL
- **Validation:** Config-binding tests, `docker compose config`, startup validation, exact key searches.

### Phase 5: Agent Context And Tooling

- **Goal:** Make agent instructions describe the website without deleting reusable engineering knowledge.
- **Depends on:** Canonical product/architecture decisions and stable artifact paths.
- **Relevant files:** `AGENTS.md`, `.claude/`, `.agents/`, `.codex/`, `.opencode/`, `.design-sync/`, `.repomix/`.
- **Acceptance criteria:** Agents route website work correctly; obsolete Event/self-host assumptions are gone; generic/vendored examples are not churned.
- **Verification:** Source-code architecture suite plus `/docs-lint`, schema-tool review, and residual searches with allowlist.
- **Rollback / failure handling:** Restore broken link/schema files first; do not recreate Markdown-reading architecture tests to hide weak review.

#### Task 5.1: Rewrite Canonical Agent Entrypoints And Commands ✅ COMPLETE

- **Type:** modify/docs/config
- **Layer:** Agent Context
- **Files:** `AGENTS.md`, `.claude/commands/dev-docs.md`, `dev-docs-update.md`, `tasks.md`, hook config, `.github/copilot-instructions.md`
- **Description:** Replace Event product orientation with website context while preserving the contribution contract, verification policy, and current code invariants.
- **Acceptance Criteria:** New planning no longer assumes white-label/self-host/Event constraints; commands still scaffold the required dev-doc structure.
- **Dependencies:** 2.1 and the first-party framing established by Tasks 2.2-2.4; their source-coupled final passes are independent.
- **Effort:** L
- **Validation:** `/docs-lint`, compatible schema-tool validation, and manual duplication review.

#### Task 5.2: Re-baseline Intent Registry And Rules — SELF-HOST CONTRACT REMOVED, PHASE 10 PATH CLEANUP PENDING

- **Type:** modify/create docs/config
- **Layer:** Agent Context
- **Files:** `.claude/contract/intents.yaml`, `.claude/rules/*.md`, benchmarks/indexes
- **Description:** Remove or rewrite Event-only triggers/paths only after source deletion; add a recurring non-code/product-positioning intent if evidence shows this workflow will recur. Preserve generic layer rules while narrowing stale product assumptions.
- **Acceptance Criteria:** Every intent path/doc/skill resolves; no intent targets deleted Event-only files; new intent has minimum tests, docs impact, acceptance, and forbidden actions.
- **Dependencies:** Init Phase 10; 5.1.
- **Effort:** L
- **Validation:** Compatible schema-tool validation plus manual path, test-project, and duplication review.

#### Task 5.3: Audit Skills, Agents, And Mirrored Context ✅ COMPLETE

- **Type:** retain/modify/delete docs
- **Layer:** Agent Context
- **Files:** `.agents/**`, `.claude/agents/**`, `.codex/**`
- **Description:** Retain generic skills and domain-independent examples. Rewrite Event-specific CTO/Plane/project resources where they remain useful; delete only skills/resources whose purpose is removed and whose inbound links are zero. Keep mirrors synchronized according to repository tooling.
- **Acceptance Criteria:** No broken skill/agent links; no duplicate authority; pedagogical mentions are allowlisted rather than mechanically edited.
- **Dependencies:** 5.1. Phase 10 intent/path cleanup remains in Task 5.2.
- **Effort:** L
- **Validation:** `/docs-lint` and inbound reference search for every deletion.

#### Task 5.4: Audit Auxiliary Tool Configuration ✅ COMPLETE

- **Type:** retain/modify/delete config
- **Layer:** DevOps/Agent Context
- **Files:** `.opencode/**`, `.design-sync/**`, `.repomix/**`, `.ci/**`
- **Description:** Update repository identity, paths, and generated indexes only where they affect this repo. Do not modify installed/generic BMAD wrappers solely for vocabulary.
- **Acceptance Criteria:** Tool configs point to existing paths and correct project identity; vendored material remains intact unless demonstrably broken.
- **Dependencies:** 5.1, 5.3. Source-coupled intent cleanup remains in Task 5.2.
- **Effort:** M
- **Validation:** Tool-specific schema/check commands where available; `/docs-lint` for repository links.

### Phase 6: GitHub, CI, Release, And Deployment Metadata

- **Goal:** Preserve security/build/release safeguards while removing Event and self-host product assumptions.
- **Depends on:** Stable project/docs paths and init production decision.
- **Relevant files:** `.github/**`, `.ci/**`, release/deploy docs.
- **Acceptance criteria:** Required workflows remain secure and point to surviving projects; deploy workflows match one first-party deployment or are explicitly disabled.
- **Verification:** YAML parsing, workflow-security tests, dry-run/config checks, link tests.
- **Rollback / failure handling:** Disable an obsolete deploy workflow before deleting it when production ownership is unresolved; never weaken supply-chain permissions.

#### Task 6.1: Re-baseline Templates, CODEOWNERS, And Repository Settings ✅ COMPLETE

- **Type:** modify/docs/config
- **Layer:** DevOps
- **Files:** `.github/ISSUE_TEMPLATE/**`, `.github/PULL_REQUEST_TEMPLATE.md`, `.github/CODEOWNERS`, `.github/workflows/repository-settings.yml`
- **Description:** Replace Event issue prompts, release-impact choices, CLA URLs, and self-host/operator wording; preserve documentation-impact and validation requirements.
- **Acceptance Criteria:** Templates route website/product/docs work correctly; CODEOWNERS paths exist; no stale Event repository URL remains except provenance.
- **Dependencies:** 1.1 plus the implemented metadata portions of Tasks 1.3 and 2.3; their approval/final-disposition gates are independent.
- **Effort:** M
- **Validation:** YAML parse and link checks.

#### Task 6.2: Audit CI, Security, Contract, And Release Workflows — CLA WORKFLOW UPDATED, SOURCE-COUPLED WORKFLOWS PENDING

- **Type:** retain/modify/delete workflow
- **Layer:** DevOps/Security
- **Files:** `.github/workflows/{test,coverage,codeql,security-tests,secret-scanning,scorecard,dependency-review,workflow-security,agent-context,cerbos-policy-check,openapi-contract,release-impact,_build-test,_container-build}.yml`
- **Description:** Retain security/build/context gates; update project/schema/image names only when their source owner changes; delete Event-only contract jobs after their generator is removed.
- **Acceptance Criteria:** Required gates invoke existing projects and paths; permissions/pins remain least-privilege; no workflow silently succeeds by skipping all tests.
- **Dependencies:** Init Phase 10; 3.4; 5.2.
- **Effort:** L
- **Validation:** Workflow security tests, YAML parsing, local command parity review.

#### Task 6.3: Resolve First-Party Deployment Workflows

- **Type:** investigate/modify/disable/delete workflow
- **Layer:** DevOps
- **Files:** `.github/workflows/deploy-coolify*.yml`, `e2e.yml`, production docs/compose
- **Description:** Align deploy workflows with the init Phase 9 first-party topology. Until that topology is approved, prevent stale self-host/Event deployment instructions from being presented as supported; retain secure reusable workflow pieces.
- **Acceptance Criteria:** Exactly one documented production ownership path exists; secrets/environments/branches/images are correct; obsolete workflows are disabled or deleted with rationale.
- **Dependencies:** Init Phase 9 and Phase 10.
- **Effort:** L
- **Validation:** Workflow parse/security review and deployment dry-run/config validation without production mutation.

### Phase 7: Policies, Schemas, Generated Artifacts, Assets, And History

- **Goal:** Remove Event-specific non-code residue only after source/data ownership is resolved.
- **Depends on:** Init Phase 10 and relevant Blog/navigation implementation.
- **Relevant files:** `cerbos/`, `schemas/`, `assets/`, generated docs, semantic history, `dev/` records.
- **Acceptance criteria:** Surviving policies/contracts/assets have callers; generated outputs match source; historical records are clearly non-canonical.
- **Verification:** Cerbos tests, schema generation/snapshot checks, reference searches, asset link checks.
- **Rollback / failure handling:** Pair policy/schema changes with their code/migration owner; restore from git if a hidden runtime package dependency appears.

#### Task 7.1: Reconcile Cerbos Policies And Tests

- **Type:** coordinate/delete/modify policy
- **Layer:** Authorization/DevOps
- **Files:** `cerbos/policies/**`, `cerbos/schemas/**`, `cerbos/tests/**`
- **Description:** Delete Event resource policies with their code resources; retain generic/auth/storage/tenant/blog-needed policies only with caller evidence. Rename resources only through the code owner's authorization-parity slice.
- **Acceptance Criteria:** Every resource kind has code and policy ownership; Event resources are absent after Phase 10; no local fallback/Cerbos parity gap is introduced.
- **Dependencies:** Init Phase 10/11.
- **Effort:** L
- **Validation:** Cerbos compile/test workflow and authorization-parity architecture tests.

#### Task 7.2: Reconcile Schemas And Generated Contracts

- **Type:** regenerate/delete/retain schema
- **Layer:** API/Persistence/Infrastructure
- **Files:** `schemas/islamu-event.md`, `schemas/openapi_islamu-event.json`, provider schemas, NSwag config/output references
- **Description:** Regenerate surviving website database/OpenAPI artifacts under an accurate name after source/migrations settle; delete Event snapshots and generators that no longer exist. Retain Weblate and Listmonk provider schemas; keep Tolgee pending code decision.
- **Acceptance Criteria:** No Event endpoint/table appears in current generated output; provider schemas correspond to surviving adapters; generation is reproducible.
- **Dependencies:** Init Phase 10/11; 4.1, 4.3.
- **Effort:** L
- **Validation:** OpenAPI/schema generator and snapshot tests; no hand-edited generated diff.

#### Task 7.3: Remove Unreferenced Event Assets ✅ COMPLETE

- **Type:** delete/retain asset
- **Layer:** Docs/Blazor
- **Files:** `assets/**`, documentation screenshots, static diagrams
- **Description:** Retain ISLAMU logos and referenced website assets. Delete Event list/catalog screenshots and stale roadmap imagery after checking README/docs/UI/workflow references and license/provenance needs.
- **Acceptance Criteria:** Every retained asset has an owner/reference or explicit brand purpose; every deletion has zero inbound references.
- **Dependencies:** 0.2 and 1.1. Only zero-reference imported assets were deleted; future route-owned assets belong to init Phase 12.
- **Effort:** M
- **Validation:** Repository reference search and rendered docs/UI smoke.

#### Task 7.4: Preserve Only Useful Historical And Dev Records ✅ COMPLETE

- **Type:** archive/delete/modify docs
- **Layer:** Dev Process
- **Files:** `dev/**`, `docs/semantic_versioning/**`, old release reports/ADRs
- **Description:** Preserve active work, durable decisions, provenance, and historical records needed to explain current architecture. Mark imported Event history Historical or archive/delete it when it has no continuing value. Never delete another active agent's dirty files.
- **Acceptance Criteria:** Active/handoff dependencies remain; historical content cannot be mistaken for current product guidance; deleted work has no inbound contract/source-anchor references.
- **Dependencies:** 0.2. Source-coupled Event history classification remains in Task 3.1.
- **Effort:** M
- **Validation:** Context/link tests and git-status review before each batch.

### Phase 8: Repository-Wide Convergence And Handoff

- **Goal:** Prove all non-code surfaces converge on the first-party website and leave synchronized operational memory.
- **Depends on:** Phases 1-7 and init Phase 10-12 behavior used by final docs.
- **Relevant files:** Entire in-scope repository plus three workstream docs.
- **Acceptance criteria:** Ledger complete; quality gates green; residual exceptions reviewed; no implementation/doc drift.
- **Verification:** Full commands in Section 14.
- **Rollback / failure handling:** Bisect by artifact batch; restore only this workstream's failing changes, never unrelated dirty work.

#### Task 8.1: Run Residual Identity And Product-Claim Audit

- **Type:** investigate/fix docs/config
- **Layer:** All non-code
- **Files:** All tracked non-code artifacts
- **Description:** Search Event/OpenISLAMU/self-host/white-label/multi-tenant/provider terminology and classify every remaining match as stale or allowlisted provenance/history/technical identifier.
- **Acceptance Criteria:** Zero unexplained residuals; allowlist records path, term, and reason; no public/canonical self-host product claim remains.
- **Dependencies:** Phases 1-7.
- **Effort:** M
- **Validation:** Repeatable `rg` commands captured in context.

#### Task 8.2: Run Reference, Format, Build, And Test Gates

- **Type:** test
- **Layer:** Docs/DevOps
- **Files:** All modified artifacts
- **Description:** Run `/docs-lint`, Release build, source-code architecture project, Compose/YAML/policy/schema validation, and targeted provider tests for changed behavior-coupled artifacts.
- **Acceptance Criteria:** New failures are zero; any accepted pre-existing warning/failure is documented with unchanged evidence.
- **Dependencies:** 8.1.
- **Effort:** L
- **Validation:** Commands and exit codes recorded in context/tasks.

#### Task 8.3: Final Public And Operator Walk-Through

- **Type:** manual QA/docs
- **Layer:** Docs/Product/DevOps
- **Files:** README, docs index, contributor setup, operations, privacy/security/legal entrypoints
- **Description:** Follow links as a public visitor, new contributor, first-party operator, and implementation agent. Confirm each audience reaches accurate implemented guidance without Event/self-host detours.
- **Acceptance Criteria:** All four journeys complete; no dead link, false feature, secret, private endpoint, or unsupported deployment instruction appears.
- **Dependencies:** 8.2.
- **Effort:** M
- **Validation:** QA notes recorded in context.

## 7. Testing Strategy

This workstream uses tests-after for executable source/configuration contracts and explicit manual review for prose. Architecture tests are not a parser or schema gate for Markdown or `.claude/contract/intents.yaml`.

| Requirement | Automated evidence | Manual evidence |
|---|---|---|
| Canonical metadata/source anchors/links | `/docs-lint`, schema tools, repository searches | Render docs and follow audience paths |
| Agent intent/rule/skill integrity | Compatible schema tools and diff checks | Review paths, duplication, and stale product assumptions |
| README/community/legal links | `/docs-lint` | GitHub-rendered Markdown review |
| Compose/config validity | `docker compose config`; existing config-binding tests | Compare first-party deployment inputs |
| Cerbos policy validity | Cerbos workflow/tests; authorization parity tests | Resource-to-caller ledger review |
| OpenAPI/schema validity | Existing generation/snapshot tests | Inspect removed Event endpoints/types |
| Weblate retention | Localization/provider tests and generation checks | Start/health/fallback runbook review |
| PostHog retention | Existing domain/application/infrastructure/client analytics tests | Consent/disable/CSP/relay review |
| Mailpit/email retention | Existing Email/Mailpit runtime categories when touched | Local capture/health/runbook review |
| Residual product identity | Repeatable searches plus reviewed allowlist | Public/operator journey audit |
| ISLAMU experience/content contract | Documentation metadata/link/source-anchor checks; route-state reconciliation | Purpose-to-content-to-participation journey in English/French/Arabic, including RTL, keyboard, reduced motion, consent, and only implemented destinations |

Do not run solution-level `dotnet test`. Test projects run individually with `--project` and TUnit `--treenode-filter` only where already validated.

## 8. Documentation, Configuration, And Operations Impact

Every non-code family is in scope for disposition, but not every file must change. Retain-unchanged is a valid reviewed result. Highest-impact modifications are:

- Root: `README.md`, community/security/legal files, `.env.example`, Docker/solution/tool metadata where identity or paths are stale.
- Canonical docs: project, architecture, index, documentation architecture, governance, quick reference, contributor setup/testing, configuration, operations, security/privacy, localization, analytics, email, accessibility/design/SEO/storage.
- Product experience: digital-front-door principles, capability horizons, content/route truth inventory, editorial ownership, localization/RTL, accessibility, performance/resilience, SEO, consent, and implemented-only CTA rules.
- Delete/merge candidates: Event product docs, self-hosting/deployment product docs, obsolete provider/integration docs, parked docs website, generated Event inventories, Event-only ADRs/releases.
- Agent context: `AGENTS.md`, intents, rules, commands, skills/resources, mirrors, benchmarks/indexes.
- DevOps: GitHub templates/workflows, `.ci`, deploy metadata, CODEOWNERS, repository settings.
- Policy/contracts/assets: Cerbos, schemas, generated OpenAPI/database snapshots, Event screenshots.

The disposition ledger in context is authoritative during implementation.

## 9. Security, Authorization, Privacy, And Abuse Considerations

- Preserve security disclosure and workflow hardening; reword scope/contact/commitments only with owner approval.
- Do not expose Keycloak admin tokens, SMTP credentials, PostHog personal keys, Weblate/Listmonk secrets, connection strings, private hosts, or tenant data.
- Retain HAL and server-authoritative authorization documentation only for surviving operations; no UI claim-based affordance guidance may replace HAL links.
- Cerbos resource deletion must remain synchronized with local fallback cases and architecture parity tests.
- PostHog failures remain non-blocking. Session replay, autocapture, and heatmaps default off; identified tracking and consent behavior require an approved public notice.
- Email docs preserve durable outbox/idempotency/retry/error-state behavior and PII-safe logging. Marketing/waitlist/newsletter consent and unsubscribe semantics must be explicit before activation.
- Mailpit is local-only; production docs must point to the approved external SMTP provider without embedding credentials.
- Removing self-host/admin surfaces reduces attack surface, but source deletion and endpoint exposure are owned by the init plan.

## 10. Multi-Tenancy, Federation, Localization, Accessibility, And Product Considerations

| Concern | Status | Planning consequence |
|---|---|---|
| Multi-tenancy | Needs Investigation | Init U5 decides whether tenant machinery represents multiple ISLAMU projects. Do not market multi-tenancy or self-hosting. Retain technical docs only for surviving behavior. |
| Federation | Needs code decision / likely Not Applicable | Delete Event/ATProto/federation docs if Phase 10 removes callers; retain only named future ownership. |
| Localization | Applicable | Weblate retained end-to-end; first-party audience and fallback/offline behavior documented. |
| Accessibility | Applicable | Retain accessibility policy, artifacts, RTL/logical CSS, reduced-motion, zoom, keyboard, and future Phase 12 navigation evidence. |
| White-label/self-host | Not Applicable | Delete product promise and third-party operator entrypoints. |
| Rich interactivity | Applicable | Docs describe actual InteractiveServer/BFF behavior after init Phase 5/12, not speculative features. |
| Blog | Planned, then Applicable | Do not document as implemented until init Phase 11 passes. |
| Waitlists/newsletters | Planned | Preserve reusable email/Listmonk infrastructure; no feature implementation in this workstream. |
| Additional programs/services | Discovery-only | Donations, community services, tools, personalization, and other “much more” ideas are not public commitments. Each needs a separate approved workstream and implemented destination before appearing in navigation or CTAs. |

## 11. Observability And Operations

- Retain OpenTelemetry/health/logging/metrics docs only for surviving resources and first-party operations.
- Preserve low-cardinality and redaction guidance for analytics, email, authorization, identity, and provider failures.
- Mailpit, Weblate, PostHog, Listmonk, SMTP, Keycloak, Cerbos, database, and migration runbooks must distinguish local, external, and first-party production ownership.
- Remove Event-specific dashboard/job/metric examples after source deletion; replace with Blog/website examples only when they exist.
- `OPERATIONS.md` remains current operational detail; future `PRODUCTION_OPERATIONS.md` becomes the first-party deployment entrypoint after init Phase 9.

## 12. Migration And Compatibility Plan

No compatibility aliases are added for deleted documentation paths. Link updates land in the same slice as moves/deletions. Publicly linked docs may use a short redirect only if an actual external consumer is proven; none is assumed.

Generated schema/OpenAPI and Cerbos changes follow the init code/migration sequence:

1. Prove data/caller ownership and backup through init Phase 10.1.
2. Remove Event source/contracts/policies through init 10.2-10.3.
3. Regenerate current website contracts and inventories.
4. Rewrite/finalize docs against the generated result.
5. Run architecture, integration, policy, and residual gates.

Historical provenance remains in README/git/dev decision records; it is not a compatibility surface.

## 13. Risk Register

| Risk | Likelihood | Impact | Mitigation | Detection Signal | Owner/Task |
|---|---:|---:|---|---|---|
| Docs finalize while Phase 10-12 behavior is still moving | High | High | Ownership/handoff matrix; final-pass dependencies | Source anchors or behavior claims drift | 0.3, 2.2, 8.2 |
| Deleting a load-bearing doc breaks agent/context tests | Medium | High | Reference/source-anchor/test gate before deletion | Architecture test missing path/link | 0.4, 3.3 |
| Mechanical Event-term removal deletes technical domain-event/outbox concepts | Medium | High | Caller evidence and residual allowlist | Build/runtime docs lose generic flow | 3.1, 8.1 |
| Retained service becomes an orphaned Compose/config fragment | Medium | High | End-to-end slice ledger | Service has no caller/docs/tests or vice versa | 4.1-4.3 |
| PostHog retention is mistaken for local hosting | Medium | Medium | Explicit external-integration decision | New unplanned Compose stack | 4.2 |
| Privacy/legal text over-promises or misstates data handling | Medium | High | Technical inventory plus owner/legal approval | Policy disagrees with settings/hosting | 1.2-1.4 |
| CLA rewrite invalidates signature continuity | Low-Medium | High | Version/path decision and workflow parity | Existing signatures no longer match | 1.3 |
| Concurrent init agent overwrites shared docs | High | Medium | Dirty-file check and phase ownership | Same shared file modified by both workstreams | 0.1, 0.3 |
| Generated Event contracts are hand-edited and drift | Medium | High | Regenerate-only rule | Generator reproduces a different file | 3.4, 7.2 |
| Generic/vendored agent material is churned needlessly | Medium | Low | Actual-defect gate and allowlist | Large unrelated diffs | 5.3, 5.4 |
| Self-host wording survives in public/canonical docs | Medium | High | Residual audit and audience walkthrough | Search/public journey finds product promise | 8.1, 8.3 |
| Pre-existing dependency warnings are mistaken for this work's regressions | Medium | Low | Record green baseline with 89 warnings | Warning count/advisories unchanged | 0.1, 8.2 |
| “Unique” becomes decorative novelty without a coherent ISLAMU journey | Medium | High | Experience contract ties atmosphere to purpose, content, accessibility, localization, privacy, and performance | Design language changes but visitor journey and ownership remain generic | 2.6, init Phase 12 |
| “Much more” creates speculative routes, dead CTAs, or retained infrastructure | High | High | Capability horizons, separate-workstream gate, and implemented-only links | Planned/deferred item appears clickable or lacks owner/evidence | 2.1, 2.6, 8.3 |

## 14. Success Metrics And Definition Of Done

Functional success:

- All tracked non-code artifacts have a reviewed disposition.
- README and canonical docs describe the first-party ISLAMU website/webapp and distinguish implemented from planned behavior.
- `docs/PROJECT.md` and `docs/DESIGN.md` establish the digital-front-door north star, capability horizons, and a distinctive values-led editorial experience without turning aspiration into public claims.
- Every public route, navigation item, CTA, locale, analytics event, and email capture is represented in the content/route truth inventory; Planned and Deferred destinations are not clickable.
- Launch guidance treats English/French/Arabic and RTL, WCAG 2.2 AA, keyboard/touch/zoom, reduced motion, performance/resilience, truthful SEO, privacy/consent, and content ownership as gates rather than optional polish.
- No public/canonical self-hostable, white-label, or Event-product promise remains.
- Event-only artifacts are deleted; useful content is merged before deletion.
- Weblate, PostHog, Mailpit, and approved email/Listmonk slices remain coherent end-to-end.
- Provenance and AGPL licensing remain accurate.
- Agent context, CI, policies, schemas, generated contracts, and assets point to surviving behavior.
- Privacy/security/legal publication is factually approved, or clearly withheld without a broken link.

Required final gates from repository root:

```bash
dotnet build --configuration Release --verbosity quiet
dotnet test --project tests/Event.Architecture.Tests/Event.Architecture.Tests.csproj --configuration Release --verbosity quiet
docker compose config
```

Also run the exact targeted test/generator/policy/workflow validation required by changed source-coupled artifacts. Residual searches must cover at least:

```bash
rg -n -i 'ISLAMU Event|islamu-event|islamuevent|event\.openislamu|OpenISLAMU' --glob '!src/**' --glob '!tests/**'
rg -n -i 'self-host|white-label|multi-tenant|third-party operator' --glob '!src/**' --glob '!tests/**'
```

Every remaining match must appear in the reviewed allowlist with a reason. Technical project names, provenance, and historical records are not failures when explicitly classified.

The planning baseline passed the Release build with 0 errors and 89 warnings. Existing AutoMapper/Microsoft.OpenApi advisories, Roslyn version mismatch, and deprecated API-description package are dependency debt outside this workstream; new errors or warnings caused by implementation are not accepted.

Current implementation verification on 2026-07-18 did not reproduce that green baseline. Debug and Release architecture commands both stopped before test execution on the same five missing Domain constants introduced by concurrent Phase 10 edits. The full Release build stopped during restore on missing `Microsoft.AspNetCore.Components.WebAssembly` and `Microsoft.AspNetCore.Components.WebAssembly.Authentication` packages. Do not delete executable tests or weaken package/source contracts to hide these failures; rerun the gates after their owning slices restore a buildable worktree.

## 15. Implementation Agent Contract — KEEP DEV DOCS CURRENT

Future agents implementing this plan MUST follow this contract:

1. Before starting any implementation slice, read this plan, `islamu-website-non-code-refactor-context.md`, `islamu-website-non-code-refactor-tasks.md`, and the current init-import workstream docs.
2. Start from the highest-priority incomplete task unless user instruction overrides it.
3. Check `git status --short` before editing. Never revert, overwrite, or “clean up” concurrent changes.
4. Update the context disposition ledger to exact paths before modifying or deleting an artifact.
5. For every deletion, check tracked references, Markdown links, source anchors, workflow/test constants, active dev docs, and generated ownership.
6. After each meaningful task or discovery, update:
   - this plan if architecture/scope/phases/risks changed;
   - the context with decisions, dispositions, files changed, blockers, validation, and next step;
   - the tasks checklist by checking completed items and adding discovered work.
7. Do not report “done” unless all three docs match the repository state.
8. Do not edit generated contracts manually; invoke or coordinate with their generator owner.
9. Do not invent legal/privacy/organizational commitments. Record the approval blocker and continue independent work.
10. Every implementation summary must teach what changed, including artifact ownership, moved/deleted content, surviving infrastructure/configuration, source coupling, verification, and remaining work.
11. If validation fails, record the failure, root cause if known, and recovery action before pausing.
12. Before handoff, context reset, PR, or final completion, refresh all three dev docs and add a dated handoff.

## 16. Progress Reporting Contract

- **Implemented:** Explain the disposition/content changes, named files, ownership decisions, preserved infrastructure, and any source-coupled handoff. Do not say only “docs updated.”
- **Verified:** List exact commands, exit results, link/source-anchor checks, residual allowlist review, and manual audience journey performed.
- **Remaining:** Name unfinished phases, approval gates, `Needs code decision` rows, and concurrent init dependencies.
- **Next:** Identify one executable next task and its prerequisites.
- **Docs updated:** State whether plan/context/tasks were synchronized and why.

## 17. Potential Risks & Unknowns

The sharpest risk is temporal coupling: the init workstream is actively changing `local-lite` and will later delete Event source, implement Blog, and replace navigation. The experience contract should be established early so those implementations have a north star, but route, CTA, Blog, SEO, and navigation claims must remain Planned until the matching init phase is green. Deleting source-anchored docs too early can also break agent and architecture gates.

The second risk is confusing a word with a capability. “Event” appears both as the removed product and as generic domain-event/outbox/webhook terminology. “Self-host” appears in obsolete product promises and in potentially useful security/provider examples. Only path-level caller and audience evidence can distinguish them; global replacement or deletion is forbidden.

Legal/privacy publication remains the only owner approval gate that repository evidence cannot resolve. The implementation agent may build a factual technical inventory and remove broken links, but must not publish controller, retention, processor, response-time, or CLA continuity claims without approval.

Public content truth is a second owner dependency: source code cannot prove mission wording, campaign priorities, impact statistics, testimonials, imagery rights, editorial approvers, or participation destinations. Task 2.6 must record those fields as unapproved or absent until an accountable owner supplies evidence; implementation readiness does not authorize invention.
