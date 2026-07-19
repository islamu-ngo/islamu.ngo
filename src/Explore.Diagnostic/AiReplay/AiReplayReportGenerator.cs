// ABOUTME: Generates deterministic fake/replay AI usability reports for assistant and MCP flows.
// ABOUTME: Exercises registry, HAL catalog, plan validation, and recovery without live providers or persistence writes.

using Explore.Application.Features.AiAssistant.Tools;
using Explore.Domain.Ai;

namespace Explore.Diagnostic.AiReplay;

public sealed class AiReplayReportGenerator
{
    private const string Version = "atcr-ai-replay-v1";
    private const string SafeReferencePayloadJson = "{\"referenceId\":\"safe-reference\"}";

    private static readonly Guid TenantId = Guid.Parse("018e4e5c-7f00-7000-8000-000000000011");
    private readonly Func<DateTimeOffset> _clock;
    private readonly IAiToolContractRegistry _registry;
    private readonly AiToolCatalogService _catalogService;

    public AiReplayReportGenerator(Func<DateTimeOffset>? clock = null, IAiToolContractRegistry? registry = null)
    {
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
        _registry = registry ?? AiToolContractRegistry.CreateDefault();
        _catalogService = new AiToolCatalogService(_registry);
    }

    public AiReplayReport Generate()
    {
        var results = new[]
        {
            ReplayAssistantRailProposalPreview(),
            ReplayMcpInspectorContract(),
            ReplayMcpProposalFirstFlow(),
            ReplayMcpProjectedToolSelection(),
            ReplayMcpConfirmationRequired(),
            ReplayMissingHalBlocksPreview(),
            ReplayInvalidPayloadRecovery(),
        };

        var report = new AiReplayReport(
            _clock(),
            Version,
            UsesLiveProviderCredentials: false,
            ContainsContentBearingArtifacts: false,
            results);

        return report with
        {
            ContainsContentBearingArtifacts = AiReplayArtifactSafetyPolicy.ContainsContentBearingData(report)
        };
    }

    private AiReplayScenarioResult ReplayAssistantRailProposalPreview()
    {
        var catalog = _catalogService.GetCatalog(CreateCatalogQuery(HalSet("safe-reference")));

        if (catalog.Count == 0 && NoRegisteredActionKinds() && NoRegisteredToolDefinitions())
        {
            return AiReplayScenarioResult.Pass(
                AiReplayScenarioCodes.AssistantRailProposalPreview,
                "Assistant rail replay found no concrete proposed-action kinds to preview.",
                "Generic AI text/reference safety remains available, but the catalog exposes no mutating proposal affordance or execution authority.");
        }

        return AiReplayScenarioResult.Fail(
            AiReplayScenarioCodes.AssistantRailProposalPreview,
            AiReplayFailureClass.ProposalValidation,
            "Assistant rail replay found unexpected proposed-action surface.",
            "Review the AI tool registry and remove stale product-specific action definitions.");
    }

    private AiReplayScenarioResult ReplayMcpInspectorContract()
    {
        var mcpDefinitions = GetMcpProposalDefinitions();

        if (mcpDefinitions.Length == 0 && NoRegisteredActionKinds())
        {
            return AiReplayScenarioResult.Pass(
                AiReplayScenarioCodes.McpInspectorContract,
                "MCP Inspector replay has a bounded discovery checklist without product-specific proposal tools.",
                "Expected smoke scope: tools/list, resources/list, resources/templates/list, prompts/list, list_ai_tool_contracts, ai_conversations, ai_conversation_detail, and 0 registry-projected proposal tools; do not confirm or execute proposed actions.");
        }

        return AiReplayScenarioResult.Fail(
            AiReplayScenarioCodes.McpInspectorContract,
            AiReplayFailureClass.CatalogAuthorization,
            "MCP Inspector replay could not derive the expected registry-backed discovery checklist.",
            "Review registry MCP exposure and confirmation mode before running manual Inspector smoke checks.");
    }

    private AiReplayScenarioResult ReplayMcpProposalFirstFlow()
    {
        var mcpDefinitions = GetMcpProposalDefinitions();

        if (mcpDefinitions.Length == 0 && NoRegisteredActionKinds())
        {
            return AiReplayScenarioResult.Pass(
                AiReplayScenarioCodes.McpProposalFirst,
                "MCP replay found no proposal-first tools to expose or execute.",
                "The empty registry keeps replay side-effect-free: no executor, repository, or MediatR command can be invoked by this harness.");
        }

        return AiReplayScenarioResult.Fail(
            AiReplayScenarioCodes.McpProposalFirst,
            AiReplayFailureClass.SideEffectSafety,
            "MCP replay did not preserve proposal-first behavior.",
            "Review registry MCP exposure, confirmation mode, and proposal command wiring.");
    }

    private AiReplayScenarioResult ReplayMcpProjectedToolSelection()
    {
        var mcpDefinitions = GetMcpProposalDefinitions();

        if (mcpDefinitions.Length == 0 && NoRegisteredActionKinds())
        {
            return AiReplayScenarioResult.Pass(
                AiReplayScenarioCodes.McpProjectedToolSelection,
                "MCP replay projects no proposal tools while no product action kinds exist.",
                "Clients must not invent action names or privileged payload fields; only future registry definitions may create projected proposal tools.");
        }

        return AiReplayScenarioResult.Fail(
            AiReplayScenarioCodes.McpProjectedToolSelection,
            AiReplayFailureClass.ProposalValidation,
            "MCP replay could not prove safe projected-tool selection.",
            "Review registry allowed/forbidden fields and projected MCP tool naming before client smoke.");
    }

    private AiReplayScenarioResult ReplayMcpConfirmationRequired()
    {
        var mcpDefinitions = GetMcpProposalDefinitions();

        if (mcpDefinitions.Length == 0 && NoRegisteredActionKinds())
        {
            return AiReplayScenarioResult.Pass(
                AiReplayScenarioCodes.McpConfirmationRequired,
                "MCP replay preserves confirmation-required posture by exposing no executable proposal surface.",
                "Future mutating registry entries must remain human-confirmed before side effects; current generic text/reference AI has none.");
        }

        return AiReplayScenarioResult.Fail(
            AiReplayScenarioCodes.McpConfirmationRequired,
            AiReplayFailureClass.SideEffectSafety,
            "MCP replay could not verify confirmation-required guidance.",
            "Review registry confirmation mode, approval metadata, and MCP prompt/runbook wording.");
    }

    private AiReplayScenarioResult ReplayMissingHalBlocksPreview()
    {
        var catalog = _catalogService.GetCatalog(CreateCatalogQuery(HalSet()));

        if (catalog.Count == 0 && NoRegisteredActionKinds())
        {
            return AiReplayScenarioResult.Pass(
                AiReplayScenarioCodes.AssistantRailMissingHal,
                "Missing HAL replay exposes no proposal confirmation path.",
                "HAL remains the affordance source of truth, and the empty registry leaves no fallback mutating action to display.");
        }

        return AiReplayScenarioResult.Fail(
            AiReplayScenarioCodes.AssistantRailMissingHal,
            AiReplayFailureClass.CatalogAuthorization,
            "Missing HAL replay did not block the mutating proposal path.",
            "Review route/workflow catalog and plan-preview HAL checks.");
    }

    private AiReplayScenarioResult ReplayInvalidPayloadRecovery()
    {
        var validation = _registry.ValidatePayload((AiProposedActionKind)0, SafeReferencePayloadJson);

        if (!validation.Succeeded &&
            validation.FailureCode == "unknown_action_kind" &&
            validation.EffectiveRecovery.NextActions.Count > 0)
        {
            return AiReplayScenarioResult.Pass(
                AiReplayScenarioCodes.InvalidPayloadRecovery,
                "Unsupported payload replay returned structured recovery without echoing fixture content.",
                "Stable recovery code and next action remain available for UI/MCP retry guidance when no action kind is registered.");
        }

        return AiReplayScenarioResult.Fail(
            AiReplayScenarioCodes.InvalidPayloadRecovery,
            AiReplayFailureClass.Recovery,
            "Invalid payload replay did not produce safe recovery metadata.",
            "Review payload guard recovery metadata and plan validator recovery propagation.");
    }

    private static AiToolCatalogQuery CreateCatalogQuery(IReadOnlySet<string> halRels)
        => new(
            TenantId,
            IsAuthenticated: true,
            AiToolCatalogPrincipalKind.User,
            RoutePath: "/",
            WorkflowScope: "reference-review",
            ContextScope: "selected-references",
            halRels);

    private static HashSet<string> HalSet(params string[] rels) => new(rels, StringComparer.OrdinalIgnoreCase);

    private static bool NoRegisteredActionKinds() => Enum.GetValues<AiProposedActionKind>().Length == 0;

    private bool NoRegisteredToolDefinitions() => _registry.Definitions.Count == 0;

    private AiToolDefinition[] GetMcpProposalDefinitions()
        => _registry.Definitions
            .Where(definition => definition.ExposeToMcp)
            .OrderBy(definition => definition.Kind)
            .ToArray();

}
