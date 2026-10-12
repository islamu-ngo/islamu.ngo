<!-- ABOUTME: Policy for keeping repository Markdown as ISLAMU's single technical documentation source. -->
<!-- ABOUTME: Prevents a duplicate hosted-docs tree until a concrete publishing requirement is approved. -->

# Public Documentation Policy

> **Audience:** Contributors | First-party operators | AI agents
> **Status:** Active
> **Owner:** Contributor Experience
> **Last Verified:** 2026-07-15
> **Source Anchors:** `docs/DOCUMENTATION_ARCHITECTURE.md`, `docs/index.md`, `README.md`

Repository Markdown is the technical documentation source of truth. The parked `docs/docs-website/` copy was removed because there is no approved hosted-docs requirement and duplicated pages drifted from current product and operational truth.

## Publishing Gate

Do not introduce a docs-site generator, mirrored page tree, or publishing pipeline until a separate approved workstream proves:

1. canonical repository docs pass metadata, source-anchor, placeholder, and link validation;
2. release documentation and first-party operator runbooks are accurate;
3. the project chooses which pages are safe for public mirroring;
4. ownership is clear for keeping mirrored public pages synchronized with source docs.

## Eligible Source Material

If publishing is later approved, select content directly from these canonical sources:

| Candidate | Source Of Truth | Public-Site Notes |
|---|---|---|
| Product overview | `README.md`, `docs/PROJECT.md` | Keep claims tied to the capability horizon and content truth inventory. |
| Contributor onboarding | `docs/GETTING_STARTED.md`, `docs/CONTRIBUTING.md` | Publish only verified local-development paths. |
| First-party operations | `docs/OPERATIONS.md`, `docs/BACKUP_RESTORE_UPGRADE.md` | Do not present internal runbooks as a supported self-hosting product. |
| API integration | `docs/API_COOKBOOK.md`, `docs/API.md` | Link generated OpenAPI reference rather than duplicating endpoint contracts. |
| Admin guide | `docs/ADMIN_GUIDE.md` | Include implemented-vs-planned labels for admin surfaces. |
| Accessibility | `docs/ACCESSIBILITY.md`, `docs/ACCESSIBILITY_ARTIFACTS.md` | Publish release-specific evidence only after validation is filled in. |

## Non-Goals For Current Phase

- No MkDocs, Docusaurus, VitePress, or other generator configuration.
- No hosted docs deployment workflow.
- No duplicate technical pages inside the marketing website.
- No duplicate copies of configuration tables or operator runbooks.

## Required Future Contract

When public docs work begins, each mirrored page needs:

- a source repository path;
- a sync owner;
- a freshness policy;
- a release-docs impact check;
- a clear warning when the public page intentionally omits contributor-only or operator-only detail.
