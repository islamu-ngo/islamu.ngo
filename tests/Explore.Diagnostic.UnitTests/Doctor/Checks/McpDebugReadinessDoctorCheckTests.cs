// ABOUTME: Unit tests for review-first MCP debug readiness doctor checks.
// ABOUTME: Ensures the doctor reports redacted docs/tests readiness without invoking MCP clients.

using Explore.Diagnostic.Doctor;
using Explore.Diagnostic.Doctor.Checks;
using FluentAssertions;

namespace Explore.Diagnostic.UnitTests.Doctor.Checks;

public sealed class McpDebugReadinessDoctorCheckTests
{
    private const string Root = "/repo";

    [Test]
    public async Task RunAsyncWhenDebugArtifactsExistReturnsPass()
    {
        var fileSystem = CreateFileSystemWithRequiredArtifacts();
        var check = new McpDebugReadinessDoctorCheck(fileSystem, Root);

        var result = await check.RunAsync(CancellationToken.None);

        result.Status.Should().Be(DoctorCheckStatus.Pass);
    }

    [Test]
    public async Task RunAsyncWhenAuthorizationHarnessIsMissingReturnsWarn()
    {
        var fileSystem = CreateFileSystemWithRequiredArtifacts(includeAuthorizationHarness: false);
        var check = new McpDebugReadinessDoctorCheck(fileSystem, Root);

        var result = await check.RunAsync(CancellationToken.None);

        result.Status.Should().Be(DoctorCheckStatus.Warn);
        result.RedactedEvidence.Should().Contain("missing:tests/Event.API.IntegrationTests/Features/McpAuthorizationTests.cs");
    }

    [Test]
    public async Task RunAsyncWhenDocsContainLikelySecretReturnsWarnWithoutEchoingSecret()
    {
        var fileSystem = CreateFileSystemWithRequiredArtifacts();
        fileSystem.AddFile(
            Path.Combine(Root, "docs/MCP_DEBUGGING.md"),
            "McpAuthorizationTests McpArchitectureTests resources/templates/list list_ai_tool_contracts ai_conversations redacted Bearer eyJsecret");
        var check = new McpDebugReadinessDoctorCheck(fileSystem, Root);

        var result = await check.RunAsync(CancellationToken.None);

        result.Status.Should().Be(DoctorCheckStatus.Warn);
        result.RedactedEvidence.Should().Contain("docs:possible-secret");
        result.RedactedEvidence.Should().NotContain("eyJsecret");
    }

    private static FakeDoctorFileSystem CreateFileSystemWithRequiredArtifacts(bool includeAuthorizationHarness = true)
    {
        var fileSystem = new FakeDoctorFileSystem();
        fileSystem.AddFile(
            Path.Combine(Root, "docs/MCP_DEBUGGING.md"),
            "McpAuthorizationTests McpArchitectureTests resources/templates/list list_ai_tool_contracts ai_conversations redacted");
        fileSystem.AddFile(Path.Combine(Root, "docs/adr/ADR-011-local-mcp-stdio-diagnostic-host.md"), "stdio decision");
        fileSystem.AddFile(Path.Combine(Root, ".gitignore"), ".mcp.json\n.vscode/*");
        fileSystem.AddFile(Path.Combine(Root, "tests/Event.API.IntegrationTests/Features/McpRuntimeStateServiceTests.cs"), "tests");
        fileSystem.AddFile(Path.Combine(Root, "tests/Event.API.IntegrationTests/Features/McpAdapterSettingsValidatorTests.cs"), "tests");
        fileSystem.AddFile(Path.Combine(Root, "tests/Event.API.IntegrationTests/Features/McpAdapterHealthCheckTests.cs"), "tests");
        fileSystem.AddFile(Path.Combine(Root, "tests/Event.Architecture.Tests/McpArchitectureTests.cs"), "tests");
        fileSystem.AddFile(Path.Combine(Root, "src/Explore.Diagnostic/AiReplay/AiReplayScenarioCodes.cs"), "codes");
        fileSystem.AddFile(Path.Combine(Root, "src/Explore.Diagnostic/AiEvaluation/AiEvaluationReportGenerator.cs"), "eval");
        fileSystem.AddFile(Path.Combine(Root, "tests/Explore.Diagnostic.UnitTests/AiReplay/AiReplayReportGeneratorTests.cs"), "tests");
        fileSystem.AddFile(Path.Combine(Root, "tests/Explore.Diagnostic.UnitTests/AiEvaluation/AiEvaluationReportGeneratorTests.cs"), "tests");

        if (includeAuthorizationHarness)
        {
            fileSystem.AddFile(Path.Combine(Root, "tests/Event.API.IntegrationTests/Features/McpAuthorizationTests.cs"), "tests");
        }

        return fileSystem;
    }
}
