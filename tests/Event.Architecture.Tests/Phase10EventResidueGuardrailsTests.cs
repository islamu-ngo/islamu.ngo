// ABOUTME: Architecture guardrails preventing Event product residue in CURRENT source after Phase 10 teardown.
// ABOUTME: Scans src/, cerbos policies, and generated artifacts for deleted Event product types/resources.
// ABOUTME: Does NOT scan tests (self), migrations, docs, or generic technical event concepts.

namespace Event.Architecture.Tests;

using System.Text.RegularExpressions;

/// <summary>
/// Phase 10 guardrails: after Event product source deletion, no current source file may contain
/// deleted Event-specific types, services, controllers, pages, or Cerbos resources.
/// </summary>
/// <remarks>
/// Exclusions by design:
/// - test sources (including this file) — guardrail must never self-match
/// - historical EF migrations (immutable, timestamped)
/// - dev docs, provenance, journal entries
/// - legacy Event.* project/assembly namespace names (Event.API, Event.Application, etc.)
/// - Event.Web.BffHosting, Event.MigrationService project names
/// - generic technical terms: WebhookEventType, AuditEvent, OutboxEvent, AnalyticsEvent
/// - shared lookup types: EventType, EventFormat, EventStatus
/// </remarks>
public sealed class Phase10EventResidueGuardrailsTests
{
    private static readonly string RepoRoot = ResolveRepoRoot();

    #region Scan Roots

    private static readonly string[] SourceScanRoots =
    [
        Path.Combine(RepoRoot, "src"),
    ];

    private static readonly string[] CerbosPolicyRoots =
    [
        Path.Combine(RepoRoot, "src", "Explore.API", "Cerbos", "Policies"),
        Path.Combine(RepoRoot, "tests", "Event.API.IntegrationTests", "TestAssets", "cerbos", "policies"),
    ];

    private static readonly string[] GeneratedArtifactRoots =
    [
        Path.Combine(RepoRoot, "src", "Explore.Blazor.Client", "Clients"),
    ];

    #endregion

    #region Forbidden Patterns (Deleted Event Product Artifacts)

    /// <summary>
    /// Precise regex patterns for deleted Event PRODUCT artifacts only.
    /// Each pattern targets a specific deleted type, service, controller, page, or resource.
    /// </summary>
    private static readonly string[] ForbiddenSourcePatterns =
    [
        // Deleted DTOs
        @"\bEventListDto\b",
        @"\bEventDto\b",
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

        // Deleted entities (word-bounded to avoid matching "EventType" etc.)
        @"\bEventRegistration\b",
        @"\bEventSession\b",
        @"\bEventDay\b",
        @"\bEventAgendaItem\b",
        @"\bEventRegistrationPolicy\b",
        @"\bEventCustomProperty\b",
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

        // Deleted services
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

        // Deleted controllers
        @"\bEventController\b",
        @"\bEventDayController\b",
        @"\bEventSessionController\b",
        @"\bEventRegistrationController\b",
        @"\bEventAgendaItemController\b",
        @"\bEventAspectController\b",
        @"\bEventCustomPropertyController\b",
        @"\bEventReportsController\b",
        @"\bEventSeriesController\b",

        // Deleted Blazor pages
        @"\bEventList\.razor\b",
        @"\bEventDetail\.razor\b",
        @"\bEventEdit\.razor\b",
        @"\bEventCreate\.razor\b",
        @"\bCreateEvent\.razor\b",
        @"\bMyEvents\.razor\b",

        // Deleted query/command/handler names
        @"\bGetEventList\b",
        @"\bGetEventDetail\b",
        @"\bCreateEventCommand\b",
        @"\bCreateEventDraft\b",
        @"\bDeleteEvent\b",
        @"\bPublishEvent\b",
        @"\bUpdateEventDraft\b",

        // Deleted API client
        @"\bEventApiClient\.g\.cs\b",
        @"\bIEventApiClient\b",

        // Deleted domain entity (Explore.Domain.Event with word boundary, NOT EventType/EventFormat)
        @"Explore\.Domain\.Event\b",
    ];

    /// <summary>
    /// Cerbos policy filenames and resource identifiers that were deleted.
    /// </summary>
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
    ];

    #endregion

    #region Exclusion Predicates

    private static bool IsExcludedPath(string relativePath)
    {
        // Exclude test sources (including this guardrail file)
        if (relativePath.StartsWith("tests/", StringComparison.Ordinal))
            return true;

        // Exclude historical EF migrations
        if (relativePath.Contains("/Migrations/", StringComparison.Ordinal))
            return true;

        // Exclude dev docs and provenance
        if (relativePath.StartsWith("docs/", StringComparison.Ordinal)
            || relativePath.StartsWith("dev/", StringComparison.Ordinal)
            || relativePath.StartsWith(".claude/", StringComparison.Ordinal))
            return true;

        // Exclude generated build artifacts
        if (relativePath.Contains("/bin/", StringComparison.Ordinal)
            || relativePath.Contains("/obj/", StringComparison.Ordinal))
            return true;

        return false;
    }

    private static bool IsExcludedLine(string line)
    {
        var trimmed = line.Trim();

        // Namespace declarations using legacy project names are allowed
        if (trimmed.StartsWith("namespace ", StringComparison.Ordinal))
            return true;

        // Using statements for legacy project assemblies are allowed
        if (trimmed.StartsWith("using ", StringComparison.Ordinal)
            && (trimmed.Contains("Event.API", StringComparison.Ordinal)
                || trimmed.Contains("Event.Application", StringComparison.Ordinal)
                || trimmed.Contains("Event.Architecture", StringComparison.Ordinal)
                || trimmed.Contains("Event.Domain", StringComparison.Ordinal)
                || trimmed.Contains("Event.Persistence", StringComparison.Ordinal)
                || trimmed.Contains("Event.Infrastructure", StringComparison.Ordinal)
                || trimmed.Contains("Event.Benchmarks", StringComparison.Ordinal)
                || trimmed.Contains("Event.MigrationService", StringComparison.Ordinal)
                || trimmed.Contains("Event.Web.BffHosting", StringComparison.Ordinal)))
            return true;

        // XML doc references to legacy projects are allowed
        if (trimmed.StartsWith("///", StringComparison.Ordinal)
            && (trimmed.Contains("Event.API", StringComparison.Ordinal)
                || trimmed.Contains("Event.Application", StringComparison.Ordinal)
                || trimmed.Contains("Event.MigrationService", StringComparison.Ordinal)))
            return true;

        // Generic technical event terms (not product-specific)
        if (trimmed.Contains("WebhookEventType", StringComparison.Ordinal)
            || trimmed.Contains("AuditEvent", StringComparison.Ordinal)
            || trimmed.Contains("OutboxEvent", StringComparison.Ordinal)
            || trimmed.Contains("AnalyticsEvent", StringComparison.Ordinal)
            || trimmed.Contains("IntegrationEvent", StringComparison.Ordinal)
            || trimmed.Contains("DomainEvent", StringComparison.Ordinal))
            return true;

        // Shared lookup types (still exist)
        if (trimmed.Contains("EventType", StringComparison.Ordinal)
            || trimmed.Contains("EventFormat", StringComparison.Ordinal)
            || trimmed.Contains("EventStatus", StringComparison.Ordinal))
            return true;

        return false;
    }

    #endregion

    [Test]
    [DisplayName("Source must not reference deleted Event product types or services")]
    public async Task Source_MustNotReference_DeletedEventProductArtifacts()
    {
        var violations = new List<string>();

        foreach (var root in SourceScanRoots.Where(Directory.Exists))
        {
            var csFiles = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);

            foreach (var file in csFiles)
            {
                var relativePath = Path.GetRelativePath(RepoRoot, file);

                if (IsExcludedPath(relativePath))
                    continue;

                var content = await File.ReadAllTextAsync(file);

                foreach (var pattern in ForbiddenSourcePatterns)
                {
                    if (Regex.IsMatch(content, pattern, RegexOptions.None))
                    {
                        var lines = content.Split('\n');
                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (Regex.IsMatch(lines[i], pattern, RegexOptions.None))
                            {
                                if (IsExcludedLine(lines[i]))
                                    continue;

                                violations.Add($"{relativePath}:{i + 1}: matches '{pattern}' => {lines[i].Trim()}");
                            }
                        }
                    }
                }
            }
        }

        var uniqueViolations = violations.Distinct().ToList();

        if (uniqueViolations.Count > 0)
        {
            Console.WriteLine($"Phase 10 Source Residue Violations ({uniqueViolations.Count}):");
            foreach (var v in uniqueViolations)
            {
                Console.WriteLine($"  - {v}");
            }
        }

        await Assert.That(uniqueViolations).IsEmpty()
            .Because("Phase 10 deleted the Event product; no source file may reference deleted Event types, services, controllers, pages, or API clients");
    }

    [Test]
    [DisplayName("Cerbos policies must not contain deleted Event product resources")]
    public async Task CerbosPolicies_MustNotContain_DeletedEventProductResources()
    {
        var violations = new List<string>();

        foreach (var root in CerbosPolicyRoots.Where(Directory.Exists))
        {
            var yamlFiles = Directory.GetFiles(root, "*.yaml", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(root, "*.yml", SearchOption.AllDirectories));

            foreach (var file in yamlFiles)
            {
                var relativePath = Path.GetRelativePath(RepoRoot, file);
                var content = await File.ReadAllTextAsync(file);

                foreach (var resource in ForbiddenCerbosResources)
                {
                    if (content.Contains(resource, StringComparison.Ordinal))
                    {
                        violations.Add($"{relativePath}: contains deleted Cerbos resource '{resource}'");
                    }
                }
            }
        }

        var uniqueViolations = violations.Distinct().ToList();

        if (uniqueViolations.Count > 0)
        {
            Console.WriteLine($"Phase 10 Cerbos Residue Violations ({uniqueViolations.Count}):");
            foreach (var v in uniqueViolations)
            {
                Console.WriteLine($"  - {v}");
            }
        }

        await Assert.That(uniqueViolations).IsEmpty()
            .Because("Phase 10 deleted Event product Cerbos resources; no policy file may reference them");
    }

    [Test]
    [DisplayName("Generated API client artifacts must not contain deleted Event product types")]
    public async Task GeneratedArtifacts_MustNotContain_DeletedEventProductTypes()
    {
        var violations = new List<string>();

        foreach (var root in GeneratedArtifactRoots.Where(Directory.Exists))
        {
            var files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(root, "*.json", SearchOption.AllDirectories))
                .Concat(Directory.GetFiles(root, "*.yaml", SearchOption.AllDirectories))
                .Concat(Directory.GetFiles(root, "*.yml", SearchOption.AllDirectories));

            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(RepoRoot, file);
                var content = await File.ReadAllTextAsync(file);

                foreach (var pattern in ForbiddenSourcePatterns)
                {
                    if (Regex.IsMatch(content, pattern, RegexOptions.None))
                    {
                        violations.Add($"{relativePath}: matches '{pattern}'");
                    }
                }
            }
        }

        var uniqueViolations = violations.Distinct().ToList();

        if (uniqueViolations.Count > 0)
        {
            Console.WriteLine($"Phase 10 Generated Artifact Residue Violations ({uniqueViolations.Count}):");
            foreach (var v in uniqueViolations)
            {
                Console.WriteLine($"  - {v}");
            }
        }

        await Assert.That(uniqueViolations).IsEmpty()
            .Because("Phase 10 deleted Event product types; no generated artifact may contain them");
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
