<!-- ABOUTME: Canonical schema every SKILL.md in this repo must satisfy. -->
<!-- ABOUTME: Defines authoring and manual review requirements for project skills. -->

# Skill Schema (Authoritative)

> Every `.agents/skills/*/SKILL.md` MUST conform to this schema.
> Authors and reviewers validate structure, links, and length before merge.

## 1. File Location

```
.agents/skills/<kebab-case-name>/SKILL.md
```

- Folder name MUST match the skill `name` in YAML frontmatter.
- Resources (longer references) live in `.agents/skills/<name>/resources/*.md` — never inline.

## 2. Required YAML Frontmatter

```yaml
---
name: <kebab-case>           # Must match folder name
description: <one sentence>  # When this skill applies; triggers
type: guardrail | pattern | reference | workflow
enforcement: block | suggest | inform
priority: critical | high | medium | low
---
```

All five fields are **required**. Review `type`, `enforcement`, and `priority` against the documented enums.

## 3. Required Sections (in order)

### `## Purpose` (≤3 sentences)

What this skill protects, enforces, or accelerates. No stack context, no tech overview — point to AGENTS.md for that.

### `## When to Load`

Bulleted list. Triggers are explicit: keywords, file paths, intent IDs. If a task matches any line, the skill loads.

### `## When NOT to Load`

Bulleted list. Explicit negatives to prevent context bloat. Example: "Not for pure domain-model questions — use `domain` rule instead."

### `## Must-Read Docs`

Links only. No prose. Relative paths from repo root (`docs/ARCHITECTURE.md`, `docs/QUICK_REFERENCE.md`, etc.). Every link MUST resolve during review.

### `## Top 5 Invariants`

Numbered list, exactly 5 items. Each invariant is a single sentence describing a non-inferable rule. Anchor terminology to `docs/DOCUMENTATION_STYLE_GUIDE.md` baseline (Instance, Tenant, Organization, BFF, Client, API).

### `## Top 5 Anti-Patterns`

Numbered list, exactly 5 items. Each item: the anti-pattern name + one-sentence consequence. No long justifications — link to docs for context.

### `## Minimal Examples`

Optional but recommended. Use fenced code blocks. Keep to **≤40 lines per example**. Show the shortest code that illustrates correct usage. Avoid full-file examples.

### `## Verification Hooks`

Bulleted list of exact commands or test-project names that catch violations. Example: `dotnet test --project Event.Architecture.Tests/Event.Architecture.Tests.csproj`. Commands must be copy-pasteable.

### `## Related Skills`

Bulleted cross-reference to sibling skills. Minimum 1 entry. Use relative paths.

## 4. File Length

- **Target**: 60–180 lines total.
- **Hard max**: 250 lines. If longer, split into `resources/*.md` referenced from Must-Read Docs.

## 5. Forbidden Content

- Stack/tech-overview paragraphs (reference `AGENTS.md`).
- Duplicated invariants from `docs/QUICK_REFERENCE.md` (link, don't copy).
- ASCII diagrams (per `docs/DOCUMENTATION_STYLE_GUIDE.md`).
- "Why clean architecture matters" prose or similar meta-content.

## 6. Review Checklist

Authors and reviewers check:
- YAML frontmatter present with all 5 required fields.
- All 8 required sections present in the documented order.
- `Top 5 Invariants` and `Top 5 Anti-Patterns` each contain exactly 5 numbered items.
- Must-Read Docs links resolve.
- Total line count ≤ 250.

## 7. Migration Debt (v1)

Existing skills that predate this schema are migration debt until reviewed. New skills must conform;
if a skill needs more depth than the line cap allows, keep `SKILL.md` compact and move detail into
`resources/*.md`. Resolve every resource link before merge.

## Related

- Agent schema → [`.claude/agents/_AGENT_SCHEMA.md`](../../.claude/agents/_AGENT_SCHEMA.md)
- Intent registry → [`.claude/contract/intents.yaml`](../../.claude/contract/intents.yaml)
- Documentation style → [`docs/DOCUMENTATION_STYLE_GUIDE.md`](../../docs/DOCUMENTATION_STYLE_GUIDE.md)
