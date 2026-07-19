<!-- ABOUTME: Curated documentation entrypoint for the first-party ISLAMU website repository. -->
<!-- ABOUTME: Routes readers to current product, contributor, operator, integration, and governance truth. -->

# ISLAMU Documentation

> **Audience:** Public readers | Contributors | Operators | Product owners | AI agents
> **Status:** Active
> **Owner:** Architecture
> **Last Verified:** 2026-07-18
> **Source Anchors:** `README.md`, `docs/PROJECT.md`, `Explore.slnx`, `dev/report/EVENT_REFERENCE_ALIGNMENT.md`

This index describes the first-party ISLAMU website. Event-product source and its Event-only
documentation have been removed. Legacy `Explore.*` and `Event.*` project names are source
identifiers, not a supported Event product.

## Start Here

- [Product definition](PROJECT.md) — implemented Foundation, Blog, and public navigation
- [Architecture](ARCHITECTURE.md)
- [Codebase structure](CODEBASE_STRUCTURE.md)
- [Getting started](GETTING_STARTED.md)
- [Contributing](CONTRIBUTING.md)
- [Testing](TESTING.md)
- [Troubleshooting](TROUBLESHOOTING.md)

## Product And Experience

- [Experience direction](DESIGN.md)
- [Technical design system](DESIGN_SYSTEM.md)
- [Accessibility](ACCESSIBILITY.md)
- [Localization](LOCALIZATION.md)
- [SEO and Blog discoverability](SEO.md)
- [Render policies](RENDER_POLICIES.md)
- [API and HAL contract](API.md)

Blog and the `SiteMegaMenu` navigation shell are implemented in source. Capabilities described as
planned elsewhere are not public routes or current product promises.

## Security And Trust

- [Security overview](SECURITY.md)
- [Security model](SECURITY-MODEL.md)
- [Privacy data inventory](PRIVACY_DATA_INVENTORY.md) — engineering facts and publication gates,
  not a public legal policy
- [Authorization](AUTHORIZATION.md)
- [Secrets](SECRETS.md)
- [Public vulnerability reporting](../SECURITY-POLICY.md)
- [Code of Conduct](../CODE_OF_CONDUCT.md)
- [Contributor License Agreement](../legal/CLA.md)

## First-Party Operations

- [Configuration](CONFIGURATION.md)
- [Operations](OPERATIONS.md)
- [Storage](STORAGE.md)
- [Backup, restore, and upgrade](BACKUP_RESTORE_UPGRADE.md)
- [CI/CD governance](CI_CD_GOVERNANCE.md)
- [Release checklist](RELEASE_CHECKLIST.md)

These documents are for ISLAMU maintainers and operators. They do not create a third-party
self-hosting or deployment-tier promise.

## Source-Backed Platform And Integration References

- [Blazor architecture](BLAZOR.md)
- [Outbox pattern](OUTBOX_PATTERN.md)
- [Webhooks](WEBHOOKS.md)
- [Federation and PDS integration](FEDERATION.md)
- [Coop integration](COOP_INTEGRATION.md)
- [Osprey integration](OSPREY_INTEGRATION.md)
- [Email notifications](EMAIL_NOTIFICATIONS.md)
- [Notifications](NOTIFICATIONS.md)

These references survive because their source or infrastructure boundary still exists. Their
presence does not imply a public navigation destination or a completed end-user capability.

## Contributor Reference

- [Governance](GOVERNANCE.md)
- [Naming conventions](NAMING_CONVENTIONS.md)
- [Template glossary](TEMPLATE_GLOSSARY.md)
- [Documentation architecture](DOCUMENTATION_ARCHITECTURE.md)
- [Documentation style guide](DOCUMENTATION_STYLE_GUIDE.md)
- [Quick reference](QUICK_REFERENCE.md)
- [Repository agent contract](../AGENTS.md)

## Active Work

- [ISLAMU website non-code refactor](../dev/active/islamu-website-non-code-refactor/)

Active work may describe future slices. A capability becomes current documentation only when its
owning source, policy, route, and acceptance evidence exist.
