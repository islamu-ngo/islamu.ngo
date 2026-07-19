// ABOUTME: Architecture guardrails preventing deleted Event product residue after Phase 10 teardown.
// ABOUTME: Scans live source, tests, Cerbos, schema, generated-client, and config artifacts with narrow exclusions.

namespace Event.Architecture.Tests;

using System.Text.RegularExpressions;

public sealed class Phase10EventResidueGuardrailsTests
{
    private static readonly string RepoRoot = ResolveRepoRoot();

    private static readonly string[] ProductSourceRoots =
    [
        Path.Combine(RepoRoot, "src"),
    ];

    private static readonly string[] TestSourceRoots =
    [
        Path.Combine(RepoRoot, "tests"),
    ];

    private static readonly string[] CerbosRoots =
    [
        Path.Combine(RepoRoot, "cerbos", "policies"),
        Path.Combine(RepoRoot, "cerbos", "policies", "_schemas"),
        Path.Combine(RepoRoot, "cerbos", "tests"),
    ];

    private static readonly string[] ClientGeneratedConfigRoots =
    [
        Path.Combine(RepoRoot, "schemas"),
        Path.Combine(RepoRoot, "src", "Explore.API"),
        Path.Combine(RepoRoot, "src", "Explore.Blazor.Client", "Clients"),
        Path.Combine(RepoRoot, "src", "Explore.Blazor.Client", "Configuration"),
    ];

    private static readonly string[] OpenApiContractSourceFiles =
    [
        Path.Combine(RepoRoot, ".github", "workflows", "openapi-contract.yml"),
        Path.Combine(RepoRoot, ".ci", "scripts", "validate-release-impact-pr.cs"),
    ];

    private static readonly string[] SourceFileExtensions = [".cs", ".razor"];
    private static readonly string[] ArtifactFileExtensions = [".cs", ".json", ".yaml", ".yml", ".xml", ".config", ".props", ".targets", ".csproj"];

    private static readonly string[] ForbiddenSourcePatterns =
    [
        @"\bEventDto\b",
        @"\bEventListDto\b",
        @"\bCreateEventDraftRequestDto\b",
        @"\bCreateEventRequest\b",
        @"\bUpdateEventRequest\b",
        @"\bDeleteEventRequest\b",
        @"\bEventRegistrationListDto\b",
        @"\bEventSessionListDto\b",
        @"\bEventDayListDto\b",
        @"\bEventAgendaItemListDto\b",
        @"\bEventRegistrationPolicyListDto\b",
        @"\bHalResourceOfEventListDto\b",
        @"\bHalCollectionResourceOfEventListDto\b",
        @"\bHalCollectionEmbeddedOfEventListDto\b",

        @"\bEvent\b",
        @"\bEventType\b",
        @"\bEventFormat\b",
        @"\bEventStatus\b",
        @"\bEventRegistration\b",
        @"\bEventSession\b",
        @"\bEventDay\b",
        @"\bEventAgendaItem\b",
        @"\bEventRegistrationPolicy\b",
        @"\bEventCustomProperty\b",
        @"\bEventCustomPropertyProjection\b",
        @"\bEventLocation\b",
        @"\bEventRoom\b",
        @"\bEventSpeaker\b",
        @"\bEventContactShareConsent\b",
        @"\bEventAspect\b",
        @"\bEventModerationRecord\b",
        @"\bEventReport\b",
        @"\bEventSeries\b",
        @"\bEventSessionGroup\b",
        @"\bEventSessionLanguage\b",

        @"\bIEventService\b",
        @"\bIEventDayService\b",
        @"\bIEventAgendaItemService\b",
        @"\bIEventRegistrationService\b",
        @"\bIEventSessionService\b",
        @"\bIEventRegistrationPolicyService\b",
        @"\bIEventFormatService\b",
        @"\bIEventStatusService\b",
        @"\bIEventTypeService\b",
        @"\bIEventSeriesService\b",
        @"\bIEventReportService\b",
        @"\bIEventModerationService\b",
        @"\bEventController\b",
        @"\bEventDayController\b",
        @"\bEventSessionController\b",
        @"\bEventRegistrationController\b",
        @"\bEventAgendaItemController\b",
        @"\bEventAspectController\b",
        @"\bEventCustomPropertyController\b",
        @"\bEventReportsController\b",
        @"\bEventSeriesController\b",
        @"\bEventList\.razor\b",
        @"\bEventDetail\.razor\b",
        @"\bEventEdit\.razor\b",
        @"\bEventCreate\.razor\b",
        @"\bCreateEvent\.razor\b",
        @"\bMyEvents\.razor\b",
        @"\bGetEventList\b",
        @"\bGetEventDetail\b",
        @"\bCreateEventCommand\b",
        @"\bCreateEventDraft\b",
        @"\bDeleteEvent\b",
        @"\bPublishEvent\b",
        @"\bUpdateEventDraft\b",

        @"\bEventApiClient\.g\.cs\b",
        @"\bopenapi_islamu-event\.json\b",
        @"\bschemas/openapi_islamu-event\.json\b",
        @"\bschemas/islamu-event\.md\b",
        @"\bislamu-event\.md\b",
        @"Explore\.Domain\.Event\b",
    ];

    private static readonly string[] ForbiddenCerbosResources =
    [
        "islamuevent_event",
        "islamuevent_event_day",
        "islamuevent_event_session",
        "islamuevent_event_registration",
        "islamuevent_event_agenda_item",
        "islamuevent_event_aspect",
        "islamuevent_event_contact_share_consent",
        "islamuevent_event_session_group",
        "islamuevent_event_session_agenda_item",
        "islamuevent_custom_property_projection",
    ];

    private static readonly string[] AllowedLegacyProjectFragments =
    [
        "Event.API",
        "Event.Application",
        "Event.Architecture.Tests",
        "Event.Benchmarks",
        "Event.Domain",
        "Event.Infrastructure",
        "Event.MigrationService",
        "Event.Persistence",
        "Event.Web.BffHosting",
    ];

    [Test]
    [DisplayName("Product source must not reference deleted Event product artifacts")]
    public async Task ProductSource_MustNotReference_DeletedEventProductArtifacts()
    {
        var violations = await ScanRootsAsync(ProductSourceRoots, SourceFileExtensions, includeGuardrailFile: true);

        await AssertNoViolationsAsync(
            violations,
            "Phase 10 deleted the Event product; live product source must not reference deleted Event types, routes, resources, generated clients, or schema artifacts");
    }

    [Test]
    [DisplayName("Test source must not reference deleted Event product artifacts")]
    public async Task TestSource_MustNotReference_DeletedEventProductArtifacts()
    {
        var violations = await ScanRootsAsync(TestSourceRoots, SourceFileExtensions, includeGuardrailFile: false);

        await AssertNoViolationsAsync(
            violations,
            "Phase 10 deleted the Event product; tests may keep legacy project names and framework identifiers, but not deleted Event product symbols or artifacts");
    }

    [Test]
    [DisplayName("Cerbos roots must not contain deleted Event product resources")]
    public async Task CerbosRoots_MustNotContain_DeletedEventProductResources()
    {
        var violations = new List<string>();

        foreach (var file in EnumerateFiles(CerbosRoots, ArtifactFileExtensions))
        {
            var relativePath = Relative(file);
            var content = await File.ReadAllTextAsync(file);

            foreach (var resource in ForbiddenCerbosResources)
            {
                if (content.Contains(resource, StringComparison.Ordinal)
                    || Path.GetFileName(file).Contains(resource, StringComparison.Ordinal))
                {
                    violations.Add($"{relativePath}: contains deleted Cerbos resource '{resource}'");
                }
            }
        }

        await AssertNoViolationsAsync(
            violations,
            "Phase 10 deleted Event product Cerbos resources; policy, schema, and test roots must not reference them");
    }

    [Test]
    [DisplayName("Client, generated, config, and schema artifacts must use the ISLAMU contract identity")]
    public async Task ClientGeneratedConfigArtifacts_MustNotUse_DeletedEventContractIdentity()
    {
        var violations = await ScanRootsAsync(ClientGeneratedConfigRoots, ArtifactFileExtensions, includeGuardrailFile: true);

        await AssertNoViolationsAsync(
            violations,
            "schemas/openapi_islamu.json is the canonical schema identity; old islamu-event schema/config names and deleted generated client artifacts are forbidden");
    }

    [Test]
    [DisplayName("OpenAPI contract sources must not reference deleted Event artifacts")]
    public async Task OpenApiContractSources_MustNotReference_DeletedEventArtifacts()
    {
        var violations = await ScanExactFilesAsync(OpenApiContractSourceFiles, DeletedContractArtifactMarkers());

        await AssertNoViolationsAsync(
            violations,
            "OpenAPI CI, release-impact classification, and inventory metadata must use the canonical Islamu schema and generated client names");
    }

    private static async Task<List<string>> ScanRootsAsync(
        IEnumerable<string> roots,
        IReadOnlyCollection<string> extensions,
        bool includeGuardrailFile)
    {
        var violations = new List<string>();

        foreach (var file in EnumerateFiles(roots, extensions))
        {
            var relativePath = Relative(file);
            if (IsExcludedPath(relativePath, includeGuardrailFile))
                continue;

            var lines = await File.ReadAllLinesAsync(file);
            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];

                foreach (var pattern in ForbiddenSourcePatterns)
                {
                    if (!Regex.IsMatch(line, pattern, RegexOptions.CultureInvariant))
                        continue;

                    if (IsAllowedLine(relativePath, line, pattern))
                        continue;

                    violations.Add($"{relativePath}:{lineIndex + 1}: matches '{pattern}' => {line.Trim()}");
                }

                foreach (var resource in ForbiddenCerbosResources)
                {
                    if (line.Contains(resource, StringComparison.Ordinal))
                    {
                        violations.Add($"{relativePath}:{lineIndex + 1}: contains deleted Cerbos resource '{resource}' => {line.Trim()}");
                    }
                }
            }
        }

        return violations.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
    }

    private static async Task<List<string>> ScanExactFilesAsync(IEnumerable<string> files, IReadOnlyCollection<string> markers)
    {
        var violations = new List<string>();

        foreach (var file in files.Where(File.Exists).Distinct(StringComparer.Ordinal))
        {
            var relativePath = Relative(file);
            var lines = await File.ReadAllLinesAsync(file);

            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];

                foreach (var marker in markers)
                {
                    if (!line.Contains(marker, StringComparison.Ordinal))
                        continue;

                    if (IsAllowedPriorBranchSchemaFallback(relativePath, line, marker))
                        continue;

                    violations.Add($"{relativePath}:{lineIndex + 1}: contains deleted OpenAPI artifact '{marker}' => {line.Trim()}");
                }
            }
        }

        return violations.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
    }

    private static string[] DeletedContractArtifactMarkers()
    {
        var deletedSchema = "schemas/openapi_islamu" + "-event.json";
        var deletedClient = "Event" + "ApiClient";

        return
        [
            deletedSchema,
            "Clients/" + deletedClient + ".g.cs",
            deletedClient + ".g.cs",
            deletedClient,
        ];
    }

    private static bool IsAllowedPriorBranchSchemaFallback(string relativePath, string line, string marker)
    {
        return relativePath.Equals(".github/workflows/openapi-contract.yml", StringComparison.Ordinal)
            && marker.Equals("schemas/openapi_islamu" + "-event.json", StringComparison.Ordinal)
            && line.Contains("prior-branch legacy comparison fallback only", StringComparison.Ordinal);
    }

    private static IEnumerable<string> EnumerateFiles(IEnumerable<string> roots, IReadOnlyCollection<string> extensions)
    {
        foreach (var root in roots.Where(Directory.Exists).Distinct(StringComparer.Ordinal))
        {
            foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                if (extensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                    yield return file;
            }
        }
    }

    private static bool IsExcludedPath(string relativePath, bool includeGuardrailFile)
    {
        if (!includeGuardrailFile
            && relativePath.Equals("tests/Event.Architecture.Tests/Phase10EventResidueGuardrailsTests.cs", StringComparison.Ordinal))
            return true;

        if (relativePath.Contains("/bin/", StringComparison.Ordinal)
            || relativePath.Contains("/obj/", StringComparison.Ordinal))
            return true;

        return relativePath.Contains("/Migrations/", StringComparison.Ordinal);
    }

    private static bool IsAllowedLine(string relativePath, string line, string pattern)
    {
        var trimmed = line.Trim();

        if (IsAboutmeOrCommentMention(trimmed))
            return true;

        if (IsLegacyProjectNameLine(trimmed))
            return true;

        if (IsBffFrameworkIdentifierLine(trimmed))
            return true;

        if (IsTask34AppHostIdentifier(relativePath, trimmed))
            return true;

        if (IsGenericTechnicalEventConcept(relativePath, trimmed, pattern))
            return true;

        return false;
    }

    private static bool IsAboutmeOrCommentMention(string trimmed)
    {
        return trimmed.StartsWith("// ABOUT" + "ME:", StringComparison.Ordinal)
            || trimmed.StartsWith("///", StringComparison.Ordinal);
    }

    private static bool IsLegacyProjectNameLine(string trimmed)
    {
        if (!trimmed.StartsWith("namespace ", StringComparison.Ordinal)
            && !trimmed.StartsWith("using ", StringComparison.Ordinal)
            && !trimmed.Contains("<ProjectReference", StringComparison.Ordinal)
            && !trimmed.Contains("AssemblyName", StringComparison.Ordinal))
        {
            return false;
        }

        return AllowedLegacyProjectFragments.Any(fragment => trimmed.Contains(fragment, StringComparison.Ordinal));
    }

    private static bool IsBffFrameworkIdentifierLine(string trimmed)
    {
        return trimmed.Contains("EventBff", StringComparison.Ordinal)
            || trimmed.Contains("EventApiBaseAddressResolver", StringComparison.Ordinal)
            || trimmed.Contains("EventApiProxyExtensions", StringComparison.Ordinal);
    }

    private static bool IsTask34AppHostIdentifier(string relativePath, string trimmed)
    {
        if (!relativePath.Equals("src/Explore.AppHost/AppHost.cs", StringComparison.Ordinal)
            && !relativePath.Equals("tests/Event.Architecture.Tests/AspireLocalInfrastructureArchitectureTests.cs", StringComparison.Ordinal)
            && !relativePath.Equals("tests/Explore.Blazor.Client.E2ETests/Fixtures/AppHostFixture.cs", StringComparison.Ordinal))
        {
            return false;
        }

        return trimmed.Contains("Event.MigrationService", StringComparison.Ordinal)
            || trimmed.Contains("islamu-event-blazor", StringComparison.Ordinal)
            || trimmed.Contains("islamu-event-api", StringComparison.Ordinal)
            || trimmed.Contains("islamu-event-blazor-secret", StringComparison.Ordinal);
    }

    private static bool IsGenericTechnicalEventConcept(string relativePath, string trimmed, string pattern)
    {
        if (pattern is @"\bEventFormat\b" or @"\bEventStatus\b")
            return false;

        if (trimmed.Contains("WebhookEventType", StringComparison.Ordinal))
            return IsWebhookContext(relativePath, trimmed);

        if (pattern is @"\bEvent\b" or @"\bEventType\b")
        {
            return IsWebhookContext(relativePath, trimmed)
                || IsOutboxContext(relativePath, trimmed)
                || IsDomainOrIntegrationEventContext(relativePath, trimmed)
                || IsAuditOrSupportLifecycleContext(relativePath, trimmed)
                || IsDotNetEventHandlerContext(relativePath, trimmed);
        }

        return false;
    }

    private static bool IsWebhookContext(string relativePath, string trimmed)
    {
        return relativePath.Contains("/Webhooks/", StringComparison.Ordinal)
            || relativePath.Contains("Webhook", StringComparison.Ordinal)
            || trimmed.Contains("Webhook", StringComparison.Ordinal)
            || trimmed.Contains("Svix", StringComparison.Ordinal);
    }

    private static bool IsOutboxContext(string relativePath, string trimmed)
    {
        return relativePath.Contains("Outbox", StringComparison.Ordinal)
            || trimmed.Contains("Outbox", StringComparison.Ordinal);
    }

    private static bool IsDomainOrIntegrationEventContext(string relativePath, string trimmed)
    {
        return relativePath.Contains("Integration", StringComparison.Ordinal)
            || trimmed.Contains("IntegrationEvent", StringComparison.Ordinal)
            || trimmed.Contains("DomainEvent", StringComparison.Ordinal)
            || trimmed.Contains("IEventHandler", StringComparison.Ordinal);
    }

    private static bool IsAuditOrSupportLifecycleContext(string relativePath, string trimmed)
    {
        return relativePath.Contains("Audit", StringComparison.Ordinal)
            || relativePath.Contains("Support", StringComparison.Ordinal)
            || trimmed.Contains("AuditEvent", StringComparison.Ordinal)
            || trimmed.Contains("SupportAccessAuditEvent", StringComparison.Ordinal)
            || trimmed.Contains("IsLifecycleEvent", StringComparison.Ordinal)
            || trimmed.Contains("Lifecycle", StringComparison.Ordinal);
    }

    private static bool IsDotNetEventHandlerContext(string relativePath, string trimmed)
    {
        return relativePath.Contains("Components/", StringComparison.Ordinal)
            && (trimmed.Contains("EventCallback", StringComparison.Ordinal)
                || trimmed.Contains("EventArgs", StringComparison.Ordinal));
    }

    private static async Task AssertNoViolationsAsync(IReadOnlyCollection<string> violations, string because)
    {
        if (violations.Count > 0)
        {
            Console.WriteLine($"Phase 10 Event residue violations ({violations.Count}):");
            foreach (var violation in violations)
            {
                Console.WriteLine($"  - {violation}");
            }
        }

        await Assert.That(violations).IsEmpty().Because(because);
    }

    private static string Relative(string file)
    {
        return Path.GetRelativePath(RepoRoot, file).Replace(Path.DirectorySeparatorChar, '/');
    }

    private static string ResolveRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not resolve repository root");
    }
}
