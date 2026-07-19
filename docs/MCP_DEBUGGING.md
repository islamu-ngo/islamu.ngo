<!-- ABOUTME: Redacted debugging and client-smoke runbook for the API-hosted MCP adapter. -->
<!-- ABOUTME: Documents the current empty-registry tool surface, authenticated resources, and fail-closed tenant behavior. -->

# MCP Debugging And Client Smoke

> **Audience:** Contributors | ISLAMU operators | AI agents
> **Status:** Active
> **Owner:** Platform/Security
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.API/Program.cs`, `src/Explore.API/Mcp/`, `src/Explore.API/Configuration/McpAdapterSettings.cs`, `src/Explore.API/Middleware/ApiTenantResolutionMiddleware.cs`, `src/Explore.API/Middleware/ApiTenantPostAuthenticationMiddleware.cs`, `src/Explore.Application/Features/AiAssistant/Tools/AiToolContractRegistry.cs`, `tests/Event.API.IntegrationTests/Features/McpAuthorizationTests.cs`, `tests/Event.Architecture.Tests/McpArchitectureTests.cs`

## Current Contract

The optional MCP adapter is hosted by `Explore.API` over stateless Streamable HTTP. Its startup
defaults are enabled at `/mcp`; runtime governance can still disable it. This is an internal
integration/debugging surface, not a distributed product or remote-plugin promise.

Current source exposes:

| Surface | Authorization | Current result |
|---|---|---|
| Tool `list_ai_tool_contracts` | Anonymous | Returns registry metadata. The default registry is currently empty, so the expected `tools` list is empty. |
| Resources `ai_conversations` and `ai_conversation_detail` | Authenticated MCP read policy | Return tenant/user-authorized conversation metadata; raw proposed-action payload JSON is omitted and reference summaries are redacted. |
| Tool `propose_ai_tool_action` | Authenticated MCP propose policy | Adapter entry point exists, but the empty default registry has no supported action kind to validate or persist. |
| Registry-projected `propose_*` tools | MCP propose policy | None are created because `AiToolContractRegistry.CreateDefault()` has no definitions. |
| Prompts and Event-management tools/resources | Not registered | No current MCP prompt type, public Event read, Event management context, or Event proposal tool exists in source. |

The endpoint is anonymous at the transport boundary so `list_ai_tool_contracts` can be discovered;
method/resource authorization filters still protect every scoped surface.

## Fail-Closed Security Posture

- Use exactly one credential mechanism: bearer authentication or `X-API-Key`, never both.
- API-key callers need `mcp:read` (or `mcp:propose`, which satisfies the read policy) for
  conversation resources and `mcp:propose` for proposal tools.
- Missing, blank, invalid, revoked, or inactive keys receive only the anonymous-safe registry tool.
- In multi-tenant mode, anonymous discovery still needs normal trusted tenant resolution. A valid
  tenant-bound key may supply its persisted tenant after authentication.
- A key/tenant-hint mismatch returns `404`; unresolved tenant-scoped API or MCP execution also
  fails closed.
- Runtime `mcp.enabled=false` returns `404` without advertising adapter details.
- Legacy SSE is not enabled at runtime even when startup/runtime settings request it.
- Do not retain credentials, tenant/user identifiers, prompts, raw protocol bodies, reference
  content, provider output, or exceptions in debugging artifacts.

## Safe Local Configuration

| Setting | Debug value | Behavior |
|---|---|---|
| `Mcp:Enabled` | `true` or unset | Startup ceiling; `false` leaves the route unmapped until restart. |
| `Mcp:EndpointPath` | `/mcp` or unset | Startup-only route path. |
| `Mcp:Stateless` | `true` | No session affinity or product MCP session state. |
| `Mcp:EnableLegacySse` | either value | Startup ceiling only; legacy SSE remains unavailable. |
| `mcp.enabled` | `true` or unset | Runtime instance/tenant gate. |
| `mcp.enable_legacy_sse` | either value | Records runtime intent only; it does not expose legacy SSE. |

Instance administrators control `mcp.enabled`, `mcp.enable_legacy_sse`,
`governance.lock_tenant_mcp`, and `governance.lock_tenant_mcp_legacy_sse`. Tenant overrides are
read-only while the matching lock is closed. Endpoint path and stateless mode are never
tenant-editable settings.

Example local launch with placeholders:

```bash
ASPNETCORE_ENVIRONMENT=Development \
Mcp__Enabled=true \
Mcp__EndpointPath=/mcp \
Mcp__Stateless=true \
dotnet run --project src/Explore.API/Explore.API.csproj --configuration Debug \
  --urls http://127.0.0.1:<redacted-port>
```

Rebuild and restart the API and MCP client before treating a stale discovery list as a source
defect.

## Debug Anchors

Set breakpoints in:

- `src/Explore.API/Mcp/AiToolRegistryMcpTools.cs` for anonymous registry discovery;
- `src/Explore.API/Mcp/AiAssistantMcpResources.cs` for authenticated conversation resources;
- `src/Explore.API/Mcp/AiAssistantMcpTools.cs` for the generic proposal entry point;
- `src/Explore.API/Mcp/AiMcpProjectedToolFactory.cs` for future registry-derived proposal tools;
- `src/Explore.API/Mcp/McpRuntimeState.cs` for startup/runtime governance; and
- the two tenant-resolution middleware files in the source-anchor block for tenant mismatch or
  `tenant_required` failures.

## Redacted Client Template

Keep real values in a local shell or secret manager. Root `.mcp.json` and VS Code local MCP
configuration are ignored; do not commit filled templates.

```json
{
  "servers": {
    "islamu-local-mcp": {
      "type": "http",
      "url": "http://127.0.0.1:<redacted-port>/mcp",
      "headers": {
        "X-API-Key": "${env:ISLAMU_MCP_API_KEY}",
        "X-Tenant-Slug": "${input:islamu_mcp_tenant_slug}"
      }
    }
  },
  "inputs": [
    {
      "type": "promptString",
      "id": "islamu_mcp_tenant_slug",
      "description": "Disposable tenant slug for local MCP smoke",
      "password": false
    }
  ]
}
```

Leave the API key unset for anonymous registry discovery. Use a disposable key or bearer identity
only when testing authenticated resources.

## Inspector Smoke

Run MCP Inspector manually against fake or disposable data:

```bash
npx -y @modelcontextprotocol/inspector
```

1. Connect to `http://127.0.0.1:<redacted-port>/mcp`.
2. Use no credential for anonymous discovery, or one disposable credential for authenticated
   smoke. Include the normal trusted tenant binding in multi-tenant mode.
3. List tools, resources, resource templates, and prompts.
4. Anonymous expected tools: `list_ai_tool_contracts` only.
5. With MCP read authority, expect `ai_conversations` and
   `islamu://ai/conversations/{conversationId}`; expect no MCP prompts.
6. Call `list_ai_tool_contracts` and verify its registry list is empty.
7. Do not call `propose_ai_tool_action` as a success smoke: the empty registry intentionally
   rejects every action kind. No projected proposal tool should be listed.
8. Retain only pass/fail, bounded failure category, redacted endpoint path, and redacted auth mode.

Do not save Inspector exports, browser storage, screenshots, Copilot transcripts, or copied JSON
containing user or tenant data.

## Command-Line Fallback

Use this only when Inspector is unavailable:

```bash
curl --fail-with-body \
  -H 'Accept: application/json, text/event-stream' \
  -H 'Content-Type: application/json' \
  -H 'ProtocolVersion: 2025-06-18' \
  -H 'X-Tenant-Slug: <redacted-disposable-tenant-slug>' \
  --data '{"jsonrpc":"2.0","id":1,"method":"tools/list"}' \
  'http://127.0.0.1:<redacted-port>/mcp'
```

Do not paste the raw response into issues. Record the method, status, and bounded failure category.

## Current Automated Evidence

Current source-backed checks are:

- `McpAuthorizationTests`: startup/runtime disablement, anonymous discovery, credential conflicts,
  scope policy, tenant mismatch, and rate-limit partitioning;
- `McpRuntimeStateServiceTests`, `McpAdapterSettingsValidatorTests`, and
  `McpAdapterHealthCheckTests`: configuration and bounded health posture; and
- `McpArchitectureTests`: MCP SDK types stay in the API adapter and adapter constructors do not
  inject repositories directly.

The deleted `McpProtocolContractTests` and `EventManagementMcp*Tests` are not current evidence.
The read-only `McpDebugReadinessDoctorCheck` is a file/text presence review and may warn on its
own stale required-path list; it does not prove live protocol behavior.

`/health` reports bounded adapter configuration/state such as enabled flags, endpoint path,
stateless mode, and legacy-SSE posture. It must not expose credentials, tenant/user identifiers,
prompts, payloads, provider output, or raw exceptions.

## Upgrade Gate

Before changing `ModelContextProtocol.AspNetCore` or the visible surface:

1. review the SDK/protocol release notes;
2. rerun the focused MCP integration and architecture checks;
3. repeat the redacted Inspector smoke;
4. verify the default registry and registered tool/resource/prompt types directly from source; and
5. retain the runtime `mcp.enabled=false` rollback and startup-unmap option.

Stateful sessions, legacy SSE, server-to-client requests, sampling, elicitation, roots,
subscriptions, remote tool import/execution, direct repository mutation, and product stdio hosting
remain unsupported.
