<!-- ABOUTME: Manual verification matrix for skill-authoring and agent-context changes. -->
<!-- ABOUTME: Covers schema shape, links, intent references, evidence boundaries, and diff hygiene. -->

# Verification And Review

## Minimum Commands

Run the documentation preflight and a whitespace check for changed skill and contract files:

```bash
/docs-lint
git diff --check -- .claude/contract/intents.yaml .agents/skills
```

Validate `.claude/contract/intents.yaml` with `.claude/contract/schema.json` in a compatible editor
or schema tool when intent routing changes.

## Manual Checks

- `SKILL.md` has required frontmatter and sections in order.
- Top 5 lists have exactly five numbered items each.
- Must-read and resource links resolve from their containing files.
- Resource index links every resource.
- Resource files start with two `ABOUTME` comments.
- Intent ids, test projects, and path globs exist in the current repository.
- No claim exceeds the available evidence.

## When To Run More

Run executable architecture tests or the full build only when source code, configuration contracts,
test infrastructure, or project files changed. Markdown and intent manifests are not opened by the
architecture test project.
