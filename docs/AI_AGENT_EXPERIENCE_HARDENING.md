<!-- ABOUTME: Documents current fail-closed AI assistant, plan-preview, replay, and MCP guardrails. -->
<!-- ABOUTME: Distinguishes surviving generic safety infrastructure from deleted Event action contracts. -->

# AI Agent Experience Hardening

> **Audience:** Contributors | Security reviewers | AI agents
> **Status:** Active
> **Owner:** AI Platform/Security
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.Application/Features/AiAssistant/`, `src/Explore.Domain/Ai/AiProposedActionKind.cs`, `src/Explore.API/Controllers/AiAssistantController.cs`, `src/Explore.API/Mcp/`, `src/Explore.Diagnostic/AiReplay/AiReplayReportGenerator.cs`

## Current Truth

The generic AI assistant, tenant-scoped conversations, bounded actor/organization reference search,
context disclosure/redaction services, plan validator, replay diagnostics, and API-hosted MCP
adapter remain in source.

The default AI tool registry is deliberately empty:

- `AiProposedActionKind` has no values;
- `AiToolContractRegistry.CreateDefault()` has no definitions;
- the assistant catalog exposes no mutating proposal affordance;
- MCP projects no `propose_*` tools; and
- a generic proposal request fails as an unsupported action kind.

Deleted Event action kinds, Event context summaries, Event HAL rels, Event MCP tools/resources,
prompts, and confirmation executors are not current contracts. The remaining infrastructure is an
internal platform boundary for future source-backed tools, not a promise that mutations are
available now.

## Allowed Control Flow

1. Tenant/user-authorized queries load conversation or reference data.
2. `SearchAiReferencesQueryHandler` returns a bounded list of tenant-filtered Actor and
   Organization DTOs; it does not expose EF entities or arbitrary repository access.
3. `AiReferencePromptPacker` caps reference count, per-reference/total characters and tokens, and
   XML-escapes selected Actor/Organization reference fields before prompt inclusion.
4. Provider output is parsed only against the current registry.
5. A plan preview or tool catalog never grants execution authority.
6. A future registered mutation must still require current HAL affordances, payload validation,
   tenant/user authorization, idempotency, and an explicit confirmation path before CQRS/MediatR
   dispatch.

Direct Blazor-to-repository calls, arbitrary EF/SQL/LINQ access, reflection execution, remote MCP
command import, and model-granted authorization remain forbidden.

## Plan Preview Guardrail

`AiProposedPlanValidator` enforces tenant/conversation context, a ten-step limit, unique proposed
steps, context freshness, registered tool metadata, required HAL relations, and schema validation.
Every result sets `ExecutionAuthorityGranted = false`.

With the current empty registry, every non-empty tool step is unsupported and cannot become
confirmation-ready. This fail-closed state is intentional; adding a tool requires a real
`AiProposedActionKind`, registry definition, authorization/HAL contract, handler/executor,
confirmation behavior, and focused evidence in the same change.

The remaining `AiSafeDataContextDefinition` and `AiSafeDataContextField` types are schema
building blocks only. There is no current `AiSafeDataContextRegistry` or
`event-reference-summary` default context.

## Disclosure And Artifact Safety

`AiContextGateway` remains the registered allow-list/trust boundary for PII entity disclosure, and
the hygiene service redacts affected conversation data when consent is revoked. The bounded
Actor/Organization reference DTO path is separate and uses the prompt packer's size/token limits
and escaping. Do not give provider or MCP paths direct access to PII entities, repositories, or
raw persistence models.

Replay/manual evidence must omit:

- prompts and assistant/provider response text;
- selected-reference content and raw proposed-action payloads;
- tenant/user identifiers and private resource ids;
- endpoint URLs, bearer tokens, API keys, and model secrets;
- screenshots or protocol exports containing user content; and
- raw exceptions or provider response bodies.

Keep only scenario code, pass/fail, bounded failure category, and redacted execution posture.

## Deterministic Replay

`AiReplayReportGenerator` is source-grounded to the empty registry. Its expected results are:

- no concrete assistant proposal preview;
- MCP discovery limited to `list_ai_tool_contracts`, authenticated conversation resources, and
  zero projected proposal tools;
- no executor, repository, or command invocation from the replay harness;
- no confirmation-ready mutation; and
- stable `unknown_action_kind` recovery for unsupported payloads.

The replay command remains:

```bash
dotnet run --project src/Explore.Diagnostic/Explore.Diagnostic.csproj \
  --configuration Release --no-restore -- \
  ai-replay-report --output /tmp/explore-ai-replay-check
```

This command is documented for maintainers; it was not run during this documentation-only audit.

## MCP Manual Smoke

Use [MCP_DEBUGGING.md](MCP_DEBUGGING.md). Current smoke must verify:

1. anonymous discovery lists only `list_ai_tool_contracts`;
2. the returned registry list is empty;
3. authenticated MCP read authority can discover `ai_conversations` and conversation detail;
4. no Event tool/resource/prompt or registry-projected proposal tool appears; and
5. no proposal or confirmation endpoint is called as a success scenario.

## Review Gate For A Future Tool

A future action is incomplete unless the same change provides:

- a typed action kind and one registry definition;
- an allow-listed JSON schema with forbidden fields;
- server authorization metadata and a current HAL affordance;
- proposal-only persistence separated from confirmed execution;
- idempotent confirmation and tenant-scoped lookup;
- redacted telemetry and recovery errors; and
- focused Application/API/MCP checks.

Until then, the shortest safe contract is the empty registry.

## Related

- [AI_CONTEXT_SECURITY.md](AI_CONTEXT_SECURITY.md)
- [MCP_DEBUGGING.md](MCP_DEBUGGING.md)
- [AUTHORIZATION.md](AUTHORIZATION.md)
