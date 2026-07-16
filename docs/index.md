<!-- ABOUTME: Curated documentation entrypoint for the first-party ISLAMU website repository. -->
<!-- ABOUTME: Routes readers to current product, contributor, operator, design, and governance truth. -->

# ISLAMU Documentation

> **Audience:** Public readers | Contributors | Operators | Product owners | AI agents
> **Status:** Active
> **Owner:** Architecture
> **Last Verified:** 2026-07-15
> **Source Anchors:** `README.md`, `docs/PROJECT.md`, `docs/DOCUMENTATION_ARCHITECTURE.md`, `dev/active/islamu-website-non-code-refactor/`

This index describes the first-party ISLAMU website. The repository is still removing imported
Event-platform artifacts; documents not listed here are not automatically current or supported.

## Start Here

- [Product definition](PROJECT.md)
- [Architecture](ARCHITECTURE.md)
- [Getting started](GETTING_STARTED.md)
- [Contributing](CONTRIBUTING.md)
- [Testing](TESTING.md)
- [Troubleshooting](TROUBLESHOOTING.md)

## Product And Experience

- [Experience direction](DESIGN.md)
- [Technical design system](DESIGN_SYSTEM.md)
- [Accessibility](ACCESSIBILITY.md)
- [Localization](LOCALIZATION.md)
- [SEO](SEO.md)
- [Render policies](RENDER_POLICIES.md)

## Security And Trust

- [Security overview](SECURITY.md)
- [Security model](SECURITY-MODEL.md)
- [Privacy data inventory](PRIVACY_DATA_INVENTORY.md) - engineering facts and publication gates, not a public legal policy.
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

## Platform Patterns Retained By Evidence

- [Blazor architecture](BLAZOR.md)
- [API](API.md)
- [Outbox pattern](OUTBOX_PATTERN.md)
- [Email notifications](EMAIL_NOTIFICATIONS.md)
- [Notifications](NOTIFICATIONS.md)

Feature-specific reference remains valid only while its source, policy, configuration, tests,
and operational owner survive the import refactor.

## Documentation Governance

- [Documentation architecture](DOCUMENTATION_ARCHITECTURE.md)
- [Documentation style guide](DOCUMENTATION_STYLE_GUIDE.md)
- [Governance](GOVERNANCE.md)
- [Quick reference](QUICK_REFERENCE.md)
- [Repository agent contract](../AGENTS.md)

## Active Work

- [ISLAMU website non-code refactor](../dev/active/islamu-website-non-code-refactor/)
- [Imported platform refactor](../dev/active/init-import-islamu-event/)

Planned routes and capabilities are tracked in active workstreams, not published here as working
product documentation.
