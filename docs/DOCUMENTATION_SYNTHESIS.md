<!-- ABOUTME: Summarizes documentation health and the current prioritized improvement stream. -->
<!-- ABOUTME: Keeps the first-party product, contributor, and operations sources of truth explicit. -->

# Documentation Synthesis

> **Audience:** Contributors | AI agents
> **Status:** Implemented
> **Owner:** Contributor Experience
> **Last Verified:** 2026-07-16
> **Source Anchors:** `README.md`, `docs/DOCUMENTATION_ARCHITECTURE.md`, `docs/DOCUMENTATION_STYLE_GUIDE.md`, `docs/index.md`

## Current State

Strengths:

- Core architecture and API docs exist.
- Security and surviving data-scope behavior are documented with implementation-level details.
- Reference docs are increasingly aligned with source code.
- Repository Markdown is the current source of truth; hosted public docs remain deferred.
- Canonical documentation metadata, source anchors, links, and commands are maintained through owner review and `/docs-lint`.

Weaknesses:

- Some docs became too long or mixed multiple intents.
- Historical docs contained diagram-heavy sections with low signal.
- Some docs duplicated each other instead of linking.
- Imported Event and external-operator docs still require source-grounded removal as their code owners complete the transition.

## High-Value Direction

1. Keep repository Markdown authoritative before investing in a public docs website.
2. Keep `README.md` as the public repository entrypoint and `docs/index.md` as the curated technical map.
3. Keep reference docs authoritative and short.
4. Keep troubleshooting symptom-first and link to runbooks instead of duplicating long procedures.
5. Keep onboarding tutorials minimal and runnable.
6. Keep architecture docs focused on implemented patterns only.
7. Treat release documentation as part of the release contract.

## Canonical Reference Set

These docs should remain the highest-priority source of truth:

- `docs/DOCUMENTATION_ARCHITECTURE.md`
- `docs/PROJECT.md`
- `docs/DESIGN.md`
- `docs/QUICK_REFERENCE.md`
- `docs/API.md`
- `docs/CONFIGURATION.md`
- `docs/SECURITY.md`
- `docs/PRIVACY_DATA_INVENTORY.md`
- `docs/ACCESSIBILITY.md`
- `docs/LOCALIZATION.md`
- `docs/RENDER_POLICIES.md`
- `docs/OPERATIONS.md`
- `docs/BACKUP_RESTORE_UPGRADE.md`
- `docs/RELEASE_CHECKLIST.md`

## Maintenance Policy

- Any behavior change should update at least one canonical reference in the same PR.
- Every PR should state docs impact: `Updated`, `Not needed`, or `Deferred` with a reason.
- Prefer links over duplicated explanations.
- Remove stale roadmap text from reference docs.
- Use source anchors for drift-prone claims about runtime, configuration, CI, tests, or deployment.
- Run `/docs-lint` and record the reviewed links, anchors, and truth-state impact before merging docs-only changes.

## Near-Term Actions

1. Remove imported Event and external-operator references only with their source, test, and workflow owners.
2. Keep `TROUBLESHOOTING.md` focused on repeat symptoms and exact first-party runbook links.
3. Keep Blog, navigation, and SEO claims Planned until their implementation workstreams are green.
4. Keep `README.md` concise and aligned with canonical product truth.
5. Keep privacy, accessibility, localization, and public-route inventories synchronized with deployed behavior.
