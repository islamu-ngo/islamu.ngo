<!-- ABOUTME: Living ledger for Event and Control Plane alignment decisions during the islamu.ngo bootstrap. -->
<!-- ABOUTME: Records retained, adapted, deferred, and deleted patterns with source evidence and unresolved gates. -->

# Event Reference Alignment Ledger

## Purpose

This ledger is the routing point for sibling-bootstrap and alignment decisions. Before reopening Event product removal, Control Plane pattern import, solution identity rename, BFF topology, side-service adoption, or tenants-as-projects, check this file first.

## Evidence Boundaries

- Source evidence used here is limited to Control Plane `dev/report/EVENT_REFERENCE_ALIGNMENT_REPORT.md` sections 2 and 15, archived `dev/zarchive/init-import-islamu-event/*`, and current islamu.ngo source.
- Control Plane-derived assets may be copied into islamu.ngo because the owner holds copyright, but each copy is a deliberate public AGPL publication with provenance, stripped private identifiers/secrets, and no runtime/build coupling to Control Plane.
- This ledger is static documentation evidence only. Runtime, build, test, backup, browser, and production drills remain acceptance gates until explicitly executed.

## D8 Categories For islamu.ngo

| Category | Meaning | Action |
|---|---|---|
| Mandatory NGO parity | Security, correctness, contract, accessibility, or maintainability invariant must exist in islamu.ngo. | Implement equivalent behavior, not necessarily identical code. |
| Reusable public pattern | Mature Control Plane/Event shape fits this repo after stripping private specifics. | Copy or adapt the smallest useful pattern and record it in the C-FLOW ledger. |
| islamu.ngo adaptation | The invariant remains valid but the target architecture differs. | Adapt deliberately and record the divergence. |
| Intentional NGO exception | islamu.ngo intentionally differs from the reference. | Document why, test later where runnable behavior exists. |
| Not applicable-delete | Reference feature is outside the first-party NGO posture. | Do not implement or reopen without a new approved workstream. |

## Decision Ledger

| Area | Verdict | Evidence | Decision |
|---|---|---|---|
| Generic platform chassis | Mandatory NGO parity | `Explore.slnx`; `dev/zarchive/init-import-islamu-event/init-import-islamu-event-plan.md`; CP report §15 | Retain the generic API/Application/Domain/Infrastructure/Persistence/Blazor/AppHost/ServiceDefaults chassis as current foundation. |
| Event product surface | Not applicable-delete | active plan D9/C-EVENT-DELETE; active tasks 8.1 | Final delete verdict. Do not reopen Event product routes, migrations, HAL affordances, OpenAPI/client surfaces, or UI unless a new approved feature workstream explicitly replaces this decision. |
| Tenant model / tenants-as-projects | islamu.ngo adaptation | active context U5; current solution still has tenant infrastructure | Keep the current tenant model as platform isolation scaffolding for now. Do not reinterpret tenants as projects until a named product workflow proves the need. |
| Users | Mandatory NGO parity | current chassis; CP report §15 identity/BFF decisions | Retain first-party user identity integration through Keycloak-backed auth and server-side UI/API access. |
| Organizations | Reusable public pattern | `src/Explore.API/Controllers/Organization*Controller.cs`; `src/Explore.API/Hateoas/Policies/Organization*LinkPolicy.cs`; `src/Explore.Application/Features/Organizations/`; `src/Explore.Persistence/Repositories/Organization*Repository.cs`; `src/Explore.Blazor.Client/Services/Organization*Service.cs` | Retain as a current generic platform/product ownership surface with active controllers, HAL, Application, Persistence, and Blazor callers. Do not add operator consoles, self-host organization management, or Event-specific behavior. |
| Groups | Reusable public pattern | `src/Explore.API/Controllers/Group*Controller.cs`; `src/Explore.API/Hateoas/Policies/GroupLinkPolicy.cs`; `src/Explore.Application/Features/Groups/`; `src/Explore.Persistence/Repositories/Group*Repository.cs`; `src/Explore.Blazor.Client/Services/GroupService.cs`; `src/Explore.Blazor.Client/Routing/Guards/GroupAdminRouteGuard.cs` | Retain as a current generic platform/product collaboration surface with active controllers, HAL, Application, Persistence, and Blazor callers. Do not add operator consoles, self-host group management, or Event-specific behavior. |
| Taxonomy/reference data | Reusable public pattern | current generic chassis; CP report §15 reference-contract guidance | Retain lookup/reference infrastructure where generic callers need it; avoid Event-specific taxonomies. |
| Storage | Mandatory NGO parity | `compose.production.yaml`; local storage volume evidence | Keep first-party storage infrastructure needed by current app, without importing public/self-host storage operator surfaces. |
| Auth / Keycloak | Reusable public pattern | `docker/keycloak/provision-islamu-ngo-client.sh`; `docker/keycloak/Dockerfile.provisioner`; active plan D5 | Keep additive, idempotent external Keycloak provisioning under NGO-specific names. CP identifiers/secrets are stripped. |
| BFF topology | Intentional NGO exception | active plan D4; CP report §15 | Keep islamu.ngo's retained Event-style YARP BFF/server deployment for this workstream. Do not switch to Control Plane opaque-ticket BFF without a measured dedicated need. |
| HAL action authority | Mandatory NGO parity | AGENTS critical rule #6; CP report §15 | HAL links remain the sole UI action authority; no local role/claim affordance gating. |
| Outbox / idempotency / callbacks | Mandatory NGO parity | CP report §15; current platform chassis | Retain reliability invariants for durable side effects. Do not add product-domain webhooks without a named consumer. |
| Webhooks | Reusable public pattern | `src/Explore.Persistence/Repositories/Webhook*Repository.cs`; `src/Explore.Persistence/Services/WebhookDeliveryPlanMaterializer.cs`; webhook normalization migrations | Retain the generic webhook catalog, incoming-webhook, outbox/delivery, provider binding, and provider publication chassis. Defer new product-domain publication/webhook products until a named consumer owns them. |
| Audit | Mandatory NGO parity | CP report §15 ownership/workload policies; current governance | Retain auditability as platform invariant; do not add third-party operator audit surfaces now. |
| Email | Reusable public pattern | current account-authority and dispatch/outbox/unsubscribe infrastructure; `dev/active/braindump.md` email ownership themes | Retain account-authority lifecycle email, transactional dispatch, durable outbox, and unsubscribe/suppression infrastructure. Campaigns, newsletters, waitlists, and release-radar ownership remain deferred until a named email product workstream defines consent, ownership, and data flows. Self-hosting email flows are not applicable unless the first-party non-self-hostable posture changes. |
| Notifications | Reusable public pattern | `src/Explore.Persistence/Services/NotificationPreferenceResolver.cs`; `src/Explore.Blazor.Client/Services/NotificationService.cs`; `src/Explore.Blazor.Client/Shared/ActorSubscriptionButton.razor.cs`; notification preference/fanout repositories | Retain the generic intent, preference, UI, actor-subscription, and orchestration chassis after Event-specific notification types are removed. Future campaigns, newsletters, release radar, and product waitlists remain deferred. |
| AI text | islamu.ngo adaptation | `src/Explore.Persistence/Configurations/Entities/Ai*Configuration.cs`; `src/Explore.Persistence/Repositories/Ai*Repository.cs`; consent, Actor, and Organization references in current source | Retain safe text, AI consent, Actor and Organization references, provider/settings, diagnostics, and audit infrastructure where generic. Event draft flows and AI tool/proposed-action product actions remain deleted. |
| Support access | Mandatory NGO parity | `src/Explore.Blazor/Extensions/BffSupportAccessEndpoints.cs`; `src/Explore.Blazor/Services/BffSupportAccessSessionStore.cs`; `src/Explore.Blazor/Services/SupportAccessForwardingHandler.cs`; `src/Explore.Persistence/Repositories/SupportAccessSessionRepository.cs` | Retain strict authority, session, audit, and BFF forwarding infrastructure for first-party support access. This is not a public support product, helpdesk, or operator console. |
| Federation / PDS | islamu.ngo adaptation | current source federation/PDS hooks and active first-party posture | Retain only generic protocol/infrastructure hooks where current callers require them. Product-facing federation/PDS surfaces are deliberately deferred, not deleted, until a named product workstream owns the UX, policy, and safety boundaries. |
| Observability | Reusable public pattern | `src/Explore.ServiceDefaults/Extensions.cs`; `compose.production.yaml`; CP report §15 | Retain low-cardinality OpenTelemetry, Serilog, management-port Prometheus, `/health`, and `/alive` posture. |
| Aspire/local topology | Reusable public pattern | `Explore.slnx`; active plan canonical `local-lite`; CP report §15 | Keep Aspire/local-lite as developer topology. Production remains lean first-party compose, not bundled self-host stack. |
| Production services | Reusable public pattern | `compose.production.yaml`; production validator; MigrationService Dockerfile | Keep validator -> Postgres -> provisioner -> migration -> API/UI ordering and hardening as public adaptation. |

## U4 Side-Service Verdicts

- Retained current generic platform/product surfaces: Organizations and Groups with active controller, HAL, Application, Persistence, and Blazor ownership; notification intent/preferences/UI/orchestration chassis; webhook catalog/incoming/outbox/provider chassis; account-authority email lifecycle/dispatch/outbox/unsubscribe infrastructure; AI safe-text/consent/provider/settings/diagnostic infrastructure; strict support-access authority/session/audit/BFF forwarding; and generic federation/PDS infrastructure hooks.
- Deferred until named consumer or product ownership: product-domain webhook publication, release radar, product waitlists, campaigns/newsletters, preference center, public federation/PDS UX, docs/API explorer, sandboxed demos, contributor/community portals, impact/grant/funding/reporting surfaces, tenant-as-project semantics, and solution/project rename.
- Not applicable under the current first-party, non-self-hostable posture: third-party operator consoles, public support/helpdesk product, bundled self-host operator stack, self-hosting readiness/generation/runtime/backup gates, and self-hosting email flows. These stay not applicable unless that posture changes.

## U5 Tenants-As-Projects Verdict

Keep the current tenant model. It remains a platform isolation boundary already present in the inherited chassis, while islamu.ngo is a single first-party, non-self-hostable deployment. Projects-as-tenants is not accepted for this phase because no current source-backed product workflow requires it.

## C-FLOW Publication Ledger

| Source | Destination | Action |
|---|---|---|
| Control Plane `docker/keycloak/provision-control-plane-client.sh` | `docker/keycloak/provision-islamu-ngo-client.sh` | Published as NGO-specific additive provisioner; CP identifiers/secrets stripped. |
| Control Plane `docker/keycloak/Dockerfile.provisioner` | `docker/keycloak/Dockerfile.provisioner` | Published provisioner image pattern with NGO script entrypoint. |
| Control Plane management-port metrics pattern | `src/Explore.ServiceDefaults/Extensions.cs` | Adapted management-only Prometheus scrape on port 9090. |
| Control Plane `docker/healthcheck.sh` | `docker/healthcheck.sh` | Published generic TCP HTTP healthcheck pattern. |
| Control Plane production-validation Dockerfile/script | `docker/production-validation/Dockerfile`; `docker/production-validation/validate-production-environment.sh` | Published networkless validator that checks shape without printing values. |
| Control Plane MigrationService Dockerfile pattern | `src/Event.MigrationService/Dockerfile` | Adapted migration gate image for current project names. |
| Control Plane `compose.production` ordering/hardening pattern | `compose.production.yaml` | Adapted lean first-party production order and hardening. |
| Control Plane InteractiveServer/production topology decisions | active plan and this ledger | Pattern-only adaptation; no runtime/build coupling. |

## Ranked Future Roadmap

1. Current deferred acceptance gates, not product features: generate the canonical OpenAPI schema and `IslamuApiClient`; run the Release build and per-project tests; execute local-lite, login, Keycloak double-run, browser, and production topology evidence; and obtain a verified restorable cloud backup before destructive migration application. Self-hosting readiness wizard and self-hosting email flows remain separately Not applicable unless the first-party non-self-hostable posture changes.
2. Named post-Blog product catalog or service offering plus static multi-product shell: in the post-Blog lane, define one concrete ISLAMU product/service catalog slice, static typed multi-product navigation, product cards, and status metadata without dynamic product discovery or placeholder destinations.
3. Consent/privacy-safe release radar, product waitlists, and preference center: implement only after named email ownership exists for consent text, source pages, unsubscribe/suppression, language/frequency, product interests, audit retention, and data-owner decisions.
4. Docs/API explorer and sandboxed demos: add only after catalog and security boundaries are named, with OpenAPI/docs ownership, no unsafe code execution by default, and sandbox isolation decisions recorded before any live demos.
5. Contributor, impact, grant/funding, and reporting surfaces: later product/supporting-content workstreams can add contributor onboarding, impact dashboards, grant/funding updates, and public reports once there is real data and governance.
6. Dedicated solution/project rename: keep as a separate workstream that updates project names, namespaces, tests, Docker/CI references, docs, and migration-service evidence together.

## Solution Identity Rename

Verdict: no-go for this phase.

`Explore.slnx` still contains mixed `Explore.*` and legacy `Event.*` project/test names, while the Control Plane precedent is a fresh `Event.ControlPlane.*` solution. Renaming is worthwhile only as a dedicated future workstream with project, namespace, test, Docker, CI, docs, and migration-service evidence updated together.

## Resolved Intentional Divergences

- InteractiveServer-only posture is retained; no InteractiveAuto/WASM/public-site transplant.
- islamu.ngo retains the existing YARP BFF divergence instead of Control Plane opaque-ticket BFF for this phase.
- Deployment is single first-party and non-self-hostable; no public operator stack.
- Self-hosting readiness and self-hosting email flows are Not applicable unless the first-party non-self-hostable posture changes.
- Keycloak is external and provisioned additively through NGO-specific assets.
- Deployment identity is separate from Event and Control Plane even where patterns were reused.
- Blog remains the first fresh product vertical in the existing plan; Event product remains deleted.
- `SiteMegaMenu` remains a static typed shell/navigation model, not dynamic product discovery.

## Current Acceptance Blockers

- Static ledger work is complete, but LSP diagnostics, build, tests, architecture/context tests, `local-lite`, login, provisioner double-run, backup, browser, and production drills were not executed in Phase 8.
- Phase 8.4 remains open.
- Runtime evidence must not be inferred from this report; it must be produced by the relevant later gates.
