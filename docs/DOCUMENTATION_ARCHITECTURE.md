<!-- ABOUTME: Canonical documentation architecture for the first-party ISLAMU website repository. -->
<!-- ABOUTME: Defines audiences, metadata, source anchors, truth states, and deletion safety. -->

# Documentation Architecture

> **Audience:** Contributors | Maintainers | Operators | AI agents
> **Status:** Active
> **Owner:** Architecture
> **Last Verified:** 2026-07-15
> **Source Anchors:** `AGENTS.md`, `docs/index.md`, `docs/DOCUMENTATION_STYLE_GUIDE.md`, `tests/Event.Architecture.Tests/DocumentationQualityTests.cs`

## Purpose

Documentation is part of the repository contract. It must describe the first-party ISLAMU
website accurately, help contributors change it safely, and give ISLAMU operators the
information needed to run it. It must not advertise unsupported self-hosting, Event product
features, or planned destinations as available behavior.

## Audiences

| Audience | Needs | Primary Entry Points |
|---|---|---|
| Public/product readers | Purpose, provenance, contribution and security paths | `README.md`, `SECURITY-POLICY.md`, `CODE_OF_CONDUCT.md` |
| Contributors | Setup, architecture, conventions, tests, docs impact | `docs/GETTING_STARTED.md`, `docs/CONTRIBUTING.md`, `docs/ARCHITECTURE.md` |
| First-party operators | Configuration, secrets, deployment, recovery, observability | `docs/CONFIGURATION.md`, `docs/SECRETS.md`, `docs/OPERATIONS.md` |
| Product/design/content owners | Product truth, experience, accessibility, localization | `docs/PROJECT.md`, `docs/DESIGN.md`, `docs/ACCESSIBILITY.md`, `docs/LOCALIZATION.md` |
| AI agents | Authority, intent, rules, active work, verification | `AGENTS.md`, `.claude/contract/intents.yaml`, `dev/active/` |

Third-party operator and evaluator documentation is not a supported audience contract.

## Information Model

- **Tutorials:** learning-oriented setup and first-contribution paths.
- **How-to guides:** task-oriented operations and contributor procedures.
- **Reference:** configuration, API, policy, schema, and generated contracts.
- **Explanation:** product, architecture, security, design, and decision records.

One canonical document owns each claim. Other documents link to it rather than copy drift-prone
details.

## Required Metadata

Canonical and operator-facing documents use:

```markdown
> **Audience:** ...
> **Status:** Active | Draft | Deprecated | Historical
> **Owner:** ...
> **Last Verified:** YYYY-MM-DD
> **Source Anchors:** `path`, `path::Symbol`
```

Every source anchor must resolve. Generated documents identify their generator and must not be
edited by hand.

## Truth States

- **Implemented:** source/config/policy and an owned destination exist.
- **Planned:** approved work exists but behavior is not available; it is never linked as working.
- **Deferred:** not committed and not a reason to retain code or configuration.
- **Historical:** preserved only for provenance or decisions and clearly marked non-current.

## Change And Deletion Rules

Before deleting or renaming a document, inspect Markdown links, source anchors, tests, workflows,
agent context, active dev docs, and generated ownership. Canonical-doc test constants change in
the same slice as the document disposition; quality gates are updated, not weakened.

Behavior, configuration, security, API, operations, and public claims change with their owning
documentation. Source-coupled docs are finalized after the corresponding code-led workstream is
green.

## Quality Gate

The authoritative repository-root documentation/context gate is:

```bash
dotnet test --project tests/Event.Architecture.Tests/Event.Architecture.Tests.csproj --configuration Release --verbosity quiet
```

The suite owns link integrity, required metadata, source anchors, placeholder markers, command
syntax, ABOUTME coverage, and agent-context structure.
