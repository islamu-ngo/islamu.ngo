---
name: docs-lint
description: Manually review documentation links, source anchors, metadata, and truth-state claims before pushing.
type: verification
priority: high
---
<!-- ABOUTME: Manual documentation preflight for links, source anchors, metadata, and truth states. -->
<!-- ABOUTME: Keeps repository Markdown reviewable without coupling architecture tests to prose files. -->

# /docs-lint - Documentation Preflight

## When To Run

- After editing `docs/**`, `.claude/**`, `.agents/**`, `AGENTS.md`, or contributor templates.
- After moving or deleting a referenced file.
- Before `/review-pr` for documentation or agent-context changes.

## Review Steps

1. Search changed files for relative Markdown links:

   ```bash
   rg -n '\[[^]]+\]\([^)]+\)' <changed-files>
   ```

2. Resolve each local target from the document's directory and inspect it directly:

   ```bash
   ls -ld <resolved-path>
   ```

3. Confirm canonical documents contain current audience, status, owner, verification date, and source anchors.
4. Confirm every source anchor exists and every behavior claim is labeled Implemented, Planned, Deferred, or Historical accurately.
5. Search for old paths or names after a move or deletion:

   ```bash
   rg -n 'OLD_PATH_OR_NAME' AGENTS.md docs .claude .agents .github dev
   ```

6. Record documentation impact and the reviewed paths in the pull request.

Architecture tests cover executable source-code and configuration contracts. They do not validate
Markdown structure, links, or `.claude/contract/intents.yaml`.

## Related

- [`/check`](check.md)
- [`/review-pr`](review-pr.md)
- [`AGENTS.md`](../../AGENTS.md)
- [Documentation architecture](../../docs/DOCUMENTATION_ARCHITECTURE.md)
