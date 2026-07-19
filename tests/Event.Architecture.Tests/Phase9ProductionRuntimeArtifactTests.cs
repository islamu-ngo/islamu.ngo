// ABOUTME: Static architecture guards for Phase 9 production runtime artifacts.
// ABOUTME: Verifies Dockerfile and shell-validator contracts without executing runtime tooling.

namespace Event.Architecture.Tests;

public sealed class Phase9ProductionRuntimeArtifactTests
{
    private static readonly string[] RuntimeDockerProjects = ["Explore.API", "Explore.Blazor"];

    private static readonly string[] MigrationRestoreProjects =
    [
        "Event.MigrationService",
        "Explore.Persistence",
        "Explore.Application",
        "Explore.Domain",
        "Explore.Secrets",
        "Explore.ServiceDefaults"
    ];

    private static readonly string[] RequiredProductionSecrets =
    [
        "POSTGRESQL_PASSWORD",
        "KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET",
        "KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET",
        "SETUP_SECRET",
        "CACHE_CONNECTION_STRING"
    ];

    private static readonly string[] RequiredProductionUrls =
    [
        "ISLAMU_API_PUBLIC_URL",
        "ISLAMU_BFF_PUBLIC_URL",
        "KEYCLOAK_AUTHORITY",
        "KEYCLOAK_INTERNAL_URL"
    ];

    private static readonly string[] RequiredProductionVariables =
    [
        "POSTGRESQL_PASSWORD",
        "KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET",
        "KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET",
        "SETUP_SECRET",
        "CACHE_CONNECTION_STRING",
        "ISLAMU_API_PUBLIC_URL",
        "ISLAMU_BFF_PUBLIC_URL",
        "KEYCLOAK_AUTHORITY",
        "KEYCLOAK_INTERNAL_URL",
        "KEYCLOAK_REALM",
        "KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID",
        "KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID",
        "KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME"
    ];

    private static readonly string[] ForbiddenPrivateIdentifiers =
    [
        "control-plane",
        "partner_account_id",
        "partner-automation",
        "partner automation",
        "control-plane-operator",
        "ensure_realm_role",
        "ensure_client_scope",
        "ensure_partner",
        "detach_all_client_scopes",
        "scope-mapping"
    ];

    [Test]
    public async Task SharedHealthcheckScriptMustUseBashTcpAndAcceptOnlyHttp200()
    {
        var source = await File.ReadAllTextAsync(ContextSystemHelpers.RepoPath("docker", "healthcheck.sh"));

        await AssertStartsWithAboutMeAsync(
            source,
            "# ABOUTME: Provides the shared ISLAMU container liveness probe without curl or wget.",
            "# ABOUTME: Uses Bash /dev/tcp and accepts only an HTTP 200 status from the first response line.");
        await Assert.That(source).Contains("set -euo pipefail");
        await Assert.That(source).Contains("/dev/tcp/$host/$port");
        await Assert.That(source).Contains("GET %s HTTP/1.1");
        await Assert.That(source).Contains("Connection: close");
        await Assert.That(source).Contains("ISLAMU_HEALTHCHECK_TIMEOUT_SECONDS:-5");
        await Assert.That(source).Contains("HTTP/*' 200 '*");
        var body = BodyAfterAboutMe(source);
        await Assert.That(body).DoesNotContain("curl");
        await Assert.That(body).DoesNotContain("wget");
        await Assert.That(source).DoesNotContain("nc ");
    }

    [Test]
    public async Task RuntimeDockerfilesMustInstallSharedHealthcheckWithoutChangingPorts()
    {
        foreach (var projectName in RuntimeDockerProjects)
        {
            var source = await File.ReadAllTextAsync(ContextSystemHelpers.RepoPath(projectName, "Dockerfile"));

            await Assert.That(source).Contains("EXPOSE 8080");
            await Assert.That(source).Contains("EXPOSE 9090");
            await Assert.That(source).DoesNotContain("EXPOSE 8081");
            await Assert.That(source).Contains("COPY --from=build --chmod=0555 /src/docker/healthcheck.sh /opt/islamu/healthcheck.sh");
            await Assert.That(source).Contains("ISLAMU_HEALTHCHECK_HOST=127.0.0.1");
            await Assert.That(source).Contains("ISLAMU_HEALTHCHECK_PORT=8080");
            await Assert.That(source).Contains("ISLAMU_HEALTHCHECK_PATH=/alive");
            await Assert.That(source).Contains("ISLAMU_HEALTHCHECK_TIMEOUT_SECONDS=5");
            await Assert.That(source).Contains("HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3");
            await Assert.That(source).Contains("CMD [\"bash\", \"/opt/islamu/healthcheck.sh\"]");
            await Assert.That(source).DoesNotContain("curl");
            await Assert.That(source).DoesNotContain("wget");
        }
    }

    [Test]
    public async Task MigrationServiceDockerfileMustBeLockedOneShotWorkerImage()
    {
        var source = await File.ReadAllTextAsync(ContextSystemHelpers.RepoPath("Event.MigrationService", "Dockerfile"));

        await AssertStartsWithAboutMeAsync(
            source,
            "# ABOUTME: Builds the one-shot Event.MigrationService worker image with pinned .NET 10 bases.",
            "# ABOUTME: Restores the exact migration transitive graph in locked mode before publishing only the worker.");
        await Assert.That(source).Contains("FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:");
        await Assert.That(source).Contains("FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:");
        await Assert.That(source).Contains("USER $APP_UID");
        await Assert.That(source).Contains("dotnet restore \"./src/Event.MigrationService/Event.MigrationService.csproj\" --locked-mode");
        await Assert.That(source).Contains("dotnet publish \"./Event.MigrationService.csproj\"");
        await Assert.That(source).Contains("/p:UseAppHost=false");
        await Assert.That(source).Contains("--no-restore");
        await Assert.That(source).Contains("ENTRYPOINT [\"dotnet\", \"Event.MigrationService.dll\"]");
        await Assert.That(source).DoesNotContain("EXPOSE ");
        await Assert.That(source).DoesNotContain("HEALTHCHECK");

        foreach (var projectName in MigrationRestoreProjects)
        {
            await Assert.That(source).Contains($"src/{projectName}/{projectName}.csproj");
            await Assert.That(source).Contains($"src/{projectName}/packages.lock.json");
        }
    }

    [Test]
    public async Task ProductionValidationImageMustBeBusyBoxWithSingleValidatorEntrypoint()
    {
        var dockerfile = await File.ReadAllTextAsync(
            ContextSystemHelpers.RepoPath("docker", "production-validation", "Dockerfile"));

        await AssertStartsWithAboutMeAsync(
            dockerfile,
            "# ABOUTME: Builds the networkless-compatible production environment validation image.",
            "# ABOUTME: Uses BusyBox and copies only the bounded validator entrypoint.");
        await Assert.That(dockerfile).Contains("FROM busybox:1.37.0");
        await Assert.That(dockerfile).Contains("COPY --chmod=0555 validate-production-environment.sh /opt/islamu/validate-production-environment.sh");
        await Assert.That(dockerfile).Contains("ENTRYPOINT [\"/bin/sh\", \"/opt/islamu/validate-production-environment.sh\"]");
        await Assert.That(dockerfile).DoesNotContain("curl");
        await Assert.That(dockerfile).DoesNotContain("wget");
        await Assert.That(dockerfile).DoesNotContain("dotnet");
    }

    [Test]
    public async Task ProductionValidationScriptMustRejectUnsafeConfigurationWithoutPrintingValues()
    {
        var source = await File.ReadAllTextAsync(
            ContextSystemHelpers.RepoPath("docker", "production-validation", "validate-production-environment.sh"));

        await AssertStartsWithAboutMeAsync(
            source,
            "# ABOUTME: Validates production environment variables without printing secret or URL values.",
            "# ABOUTME: Rejects sample, local, and malformed runtime configuration before deployment.");
        await Assert.That(CountAboutMeLines(source)).IsEqualTo(2);
        await Assert.That(source).Contains("set -eu");
        await Assert.That(source).DoesNotContain("pipefail");
        await Assert.That(source).DoesNotContain("[[");
        await Assert.That(source).Contains("production validation error: $1");
        await Assert.That(source).Contains("production validation ok");
        await Assert.That(source).DoesNotContain("eval");
        await Assert.That(source).Contains("case \"$1\" in");
        await Assert.That(source).Contains("*) fail \"$1\" ;;");
        await Assert.That(source).Contains("*sample*|*sentinel*|*fixture*|*local*|*localhost*|*127.0.0.1*|*.test*|*example*|*changeme*|*password*|*default*|*dummy*|*placeholder*");
        await Assert.That(source).Contains("[ \"${#value}\" -ge 16 ] || fail \"$name\"");
        await Assert.That(source).Contains("https://*) ;;");
        await Assert.That(source).Contains("[ -n \"$without_scheme\" ] || fail \"$name\"");
        await Assert.That(source).Contains("/*|*@*|*\\?*|*#*|*//*) fail \"$name\" ;;");
        await Assert.That(source).Contains("authority=${without_scheme%%/*}");
        await Assert.That(source).Contains("[ -n \"$authority\" ] || fail \"$name\"");
        await Assert.That(source).Contains("case \"$authority\" in");
        await Assert.That(source).Contains(":*) fail \"$name\" ;;");
        await Assert.That(source).Contains("*/realms/\"$realm\") ;;");
        await Assert.That(source).Contains("validate_exact KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID islamu-ngo-blazor");
        await Assert.That(source).Contains("validate_exact KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID islamu-ngo-api");
        await Assert.That(source).Contains("validate_exact KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME islamu-ngo-api-audience");
        await Assert.That(source).DoesNotContain("echo \"$value");
        await Assert.That(source).DoesNotContain("echo \"$lower");

        foreach (var secretName in RequiredProductionSecrets)
        {
            await Assert.That(source).Contains($"validate_secret {secretName}");
        }

        foreach (var urlName in RequiredProductionUrls)
        {
            await Assert.That(source).Contains($"validate_https_url {urlName}");
        }

        foreach (var variableName in RequiredProductionVariables)
        {
            await Assert.That(source).Contains(VariableReadCaseFor(variableName));
        }

        foreach (var forbiddenIdentifier in ForbiddenPrivateIdentifiers)
        {
            await Assert.That(source).DoesNotContain(forbiddenIdentifier);
        }
    }

    private static async Task AssertStartsWithAboutMeAsync(
        string source,
        string expectedFirstLine,
        string expectedSecondLine)
    {
        var lines = source.Replace("\r\n", "\n").Split('\n');

        await Assert.That(lines[0]).IsEqualTo(expectedFirstLine);
        await Assert.That(lines[1]).IsEqualTo(expectedSecondLine);
    }

    private static string BodyAfterAboutMe(string source) =>
        string.Join('\n', source.Replace("\r\n", "\n").Split('\n').Skip(2));

    private static int CountAboutMeLines(string source) =>
        source.Replace("\r\n", "\n").Split('\n').Count(line => line.StartsWith("# ABOUTME:", StringComparison.Ordinal));

    private static string VariableReadCaseFor(string variableName) =>
        variableName + ") printf '%s' \"${" + variableName + ":-}\" ;;";
}
