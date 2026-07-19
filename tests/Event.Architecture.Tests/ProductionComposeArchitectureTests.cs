// ABOUTME: Source architecture guards for the production Compose topology.
// ABOUTME: Ensures production orchestration stays external-provider-only and port-safe.

namespace Event.Architecture.Tests;

using System.Text.RegularExpressions;

public sealed class ProductionComposeArchitectureTests
{
    private static readonly string[] ExpectedServices =
    [
        "production-environment-validation",
        "postgres",
        "keycloak-ngo-provisioner",
        "islamu-ngo-migration",
        "islamu-ngo-api",
        "islamu-ngo-ui"
    ];

    private static readonly string[] HardenedServices =
    [
        "production-environment-validation",
        "keycloak-ngo-provisioner",
        "islamu-ngo-migration",
        "islamu-ngo-api",
        "islamu-ngo-ui"
    ];

    [Test]
    public async Task ProductionCompose_MustUseExactProductionServiceSetAndExternalProviders()
    {
        var compose = await ReadProductionComposeAsync();
        var serviceNames = ExtractServiceNames(compose);

        await Assert.That(serviceNames.SequenceEqual(ExpectedServices)).IsTrue();
        await Assert.That(compose).Contains("dockerfile: Dockerfile.provisioner");
        await Assert.That(compose).Contains("KEYCLOAK_ADMIN_SERVICE_CLIENT_ID: ${KEYCLOAK_ADMIN_SERVICE_CLIENT_ID:?");
        await Assert.That(compose).Contains("KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET: ${KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET:?");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_PROVISIONING_REQUIRED: \"true\"");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID: islamu-ngo-blazor");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID: islamu-ngo-api");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME: islamu-ngo-api-audience");
        await Assert.That(compose).Contains("KEYCLOAK_INTERNAL_URL: ${KEYCLOAK_INTERNAL_URL:?");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_REDIRECT_URIS: ${KEYCLOAK_ISLAMU_NGO_REDIRECT_URIS:?");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_WEB_ORIGINS: ${KEYCLOAK_ISLAMU_NGO_WEB_ORIGINS:?");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_LOGOUT_REDIRECT_URIS: ${KEYCLOAK_ISLAMU_NGO_LOGOUT_REDIRECT_URIS:?");

        foreach (var bannedService in new[] { "keycloak", "redis", "cache", "cerbos", "mailpit", "smtp", "telemetry", "backup", "reverse-proxy", "worker" })
        {
            await Assert.That(compose).DoesNotContain($"\n  {bannedService}:\n");
        }

        await Assert.That(compose).DoesNotContain("KEYCLOAK_ADMIN_PASSWORD");
        await Assert.That(compose).DoesNotContain("KEYCLOAK_ALLOW_INSECURE_HTTP");
    }

    [Test]
    public async Task ProductionCompose_MustSequenceValidationDatabaseProvisioningMigrationApiAndUi()
    {
        var compose = await ReadProductionComposeAsync();

        await Assert.That(Block(compose, "production-environment-validation")).Contains("network_mode: none");
        await Assert.That(Block(compose, "production-environment-validation")).Contains("POSTGRESQL_PASSWORD: ${POSTGRESQL_PASSWORD:?");
        await Assert.That(Block(compose, "production-environment-validation")).Contains("CACHE_CONNECTION_STRING: ${CACHE_CONNECTION_STRING:?");
        await Assert.That(Block(compose, "production-environment-validation")).Contains("KEYCLOAK_AUTHORITY: ${KEYCLOAK_AUTHORITY:?");
        await Assert.That(Block(compose, "production-environment-validation")).Contains("KEYCLOAK_INTERNAL_URL: ${KEYCLOAK_INTERNAL_URL:?");

        await Assert.That(Block(compose, "postgres")).Contains("image: postgres:18-alpine");
        await Assert.That(Block(compose, "postgres")).Contains("production-environment-validation:\n        condition: service_completed_successfully");
        await Assert.That(Block(compose, "postgres")).Contains("postgres_data:/var/lib/postgresql/data");
        await Assert.That(Block(compose, "postgres")).Contains("pg_isready -U $$POSTGRES_USER -d $$POSTGRES_DB");
        await Assert.That(Block(compose, "postgres")).DoesNotContain("ports:");

        await Assert.That(Block(compose, "keycloak-ngo-provisioner")).Contains("production-environment-validation:\n        condition: service_completed_successfully");
        await Assert.That(Block(compose, "keycloak-ngo-provisioner")).Contains("postgres:\n        condition: service_healthy");
        await Assert.That(Block(compose, "islamu-ngo-migration")).Contains("postgres:\n        condition: service_healthy");
        await Assert.That(Block(compose, "islamu-ngo-migration")).Contains("keycloak-ngo-provisioner:\n        condition: service_completed_successfully");
        await Assert.That(Block(compose, "islamu-ngo-api")).Contains("islamu-ngo-migration:\n        condition: service_completed_successfully");
        await Assert.That(Block(compose, "islamu-ngo-ui")).Contains("islamu-ngo-api:\n        condition: service_healthy");
    }

    [Test]
    public async Task ProductionCompose_MustKeepRuntimePortsAndNetworksConstrained()
    {
        var compose = await ReadProductionComposeAsync();
        var api = Block(compose, "islamu-ngo-api");
        var ui = Block(compose, "islamu-ngo-ui");

        await Assert.That(api).Contains("ASPNETCORE_HTTP_PORTS: \"8080;9090\"");
        await Assert.That(ui).Contains("ASPNETCORE_HTTP_PORTS: \"8080;9090\"");
        await Assert.That(api).Contains("expose:\n      - \"8080\"\n      - \"9090\"");
        await Assert.That(api).DoesNotContain("ports:");
        await Assert.That(ui).Contains("ports:\n      - \"${ISLAMU_BFF_PUBLIC_PORT:?ISLAMU_BFF_PUBLIC_PORT is required}:8080\"");
        await Assert.That(compose).DoesNotContain(":9090\"");
        await Assert.That(compose).DoesNotContain("9090:9090");
        await Assert.That(api).Contains("ConnectionStrings__cache: ${CACHE_CONNECTION_STRING:?");
        await Assert.That(ui).Contains("ConnectionStrings__cache: ${CACHE_CONNECTION_STRING:?");
        await Assert.That(api).Contains("Keycloak__Authority: ${KEYCLOAK_AUTHORITY:?");
        await Assert.That(api).Contains("Keycloak__MetadataAddress: ${KEYCLOAK_INTERNAL_URL:?");
        await Assert.That(ui).Contains("Keycloak__Authority: ${KEYCLOAK_AUTHORITY:?");
        await Assert.That(ui).Contains("Keycloak__MetadataAddress: ${KEYCLOAK_INTERNAL_URL:?");
        await Assert.That(api).Contains("SETUP_SECRET: ${SETUP_SECRET:?");
        await Assert.That(ui).Contains("SETUP_SECRET: ${SETUP_SECRET:?");
        await Assert.That(api).Contains("monitoring-network");
        await Assert.That(ui).Contains("monitoring-network");
        await Assert.That(compose).Contains("monitoring-network:\n    internal: true");
        await Assert.That(Block(compose, "postgres")).DoesNotContain("monitoring-network");
        await Assert.That(Block(compose, "keycloak-ngo-provisioner")).DoesNotContain("monitoring-network");
        await Assert.That(Block(compose, "islamu-ngo-migration")).DoesNotContain("monitoring-network");
    }

    [Test]
    public async Task ProductionCompose_MustUseDiscreteDatabaseVariablesAndInProcessApiWorkers()
    {
        var compose = await ReadProductionComposeAsync();
        var migration = Block(compose, "islamu-ngo-migration");
        var api = Block(compose, "islamu-ngo-api");
        var program = await File.ReadAllTextAsync(ContextSystemHelpers.RepoPath("Explore.API", "Program.cs"));

        foreach (var block in new[] { migration, api })
        {
            await Assert.That(block).Contains("POSTGRESQL_HOST: postgres");
            await Assert.That(block).Contains("POSTGRESQL_PORT: \"5432\"");
            await Assert.That(block).Contains("POSTGRESQL_DATABASE: ${POSTGRESQL_DATABASE:?");
            await Assert.That(block).Contains("POSTGRESQL_USERNAME: ${POSTGRESQL_USERNAME:?");
            await Assert.That(block).Contains("POSTGRESQL_PASSWORD: ${POSTGRESQL_PASSWORD:?");
            await Assert.That(block).Contains("SecretProvider__Provider: None");
        }

        await Assert.That(program).Contains("builder.Services.AddHostedService<OutboxProcessor>();");
        await Assert.That(program).Contains("builder.Services.AddHostedService<EmailDispatchProcessor>();");
        await Assert.That(program).Contains("builder.Services.AddHostedService<StorageReconciliationProcessor>();");
        await Assert.That(program).Contains("builder.Services.AddHostedService<IntegrationSyncProcessor>();");
        await Assert.That(compose).DoesNotContain("islamu-ngo-worker");
    }

    [Test]
    public async Task ProductionCompose_MustApplyRuntimeContainerHardening()
    {
        var compose = await ReadProductionComposeAsync();

        foreach (var serviceName in HardenedServices)
        {
            var block = Block(compose, serviceName);
            await Assert.That(block).Contains("read_only: true");
            await Assert.That(block).Contains("tmpfs:\n      - /tmp");
            await Assert.That(block).Contains("cap_drop:\n      - ALL");
            await Assert.That(block).Contains("security_opt:\n      - no-new-privileges:true");
        }
    }

    private static async Task<string> ReadProductionComposeAsync() =>
        (await File.ReadAllTextAsync(ContextSystemHelpers.RepoPath("compose.production.yaml"))).ReplaceLineEndings("\n");

    private static string[] ExtractServiceNames(string compose)
    {
        var services = Section(compose, "services", "networks");
        return Regex.Matches(services, "^  ([a-z0-9][a-z0-9-]*):$", RegexOptions.Multiline)
            .Select(match => match.Groups[1].Value)
            .ToArray();
    }

    private static string Block(string compose, string serviceName)
    {
        var serviceMarker = $"\n  {serviceName}:\n";
        var startIndex = compose.IndexOf(serviceMarker, StringComparison.Ordinal);
        if (startIndex < 0 && compose.StartsWith($"  {serviceName}:\n", StringComparison.Ordinal))
        {
            startIndex = -1;
        }

        if (startIndex < 0)
        {
            throw new InvalidOperationException($"Service block not found: {serviceName}");
        }

        var blockStart = startIndex + 1;
        var nextService = Regex.Match(compose[(blockStart + serviceMarker.Length - 1)..], "\\n  [a-z0-9][a-z0-9-]*:\\n");
        var blockEnd = nextService.Success
            ? blockStart + serviceMarker.Length - 1 + nextService.Index
            : compose.IndexOf("\nnetworks:\n", blockStart, StringComparison.Ordinal);

        return compose[blockStart..blockEnd];
    }

    private static string Section(string content, string start, string end)
    {
        var startMarker = $"{start}:\n";
        var endMarker = $"\n{end}:\n";
        var startIndex = content.IndexOf(startMarker, StringComparison.Ordinal);
        var endIndex = content.IndexOf(endMarker, StringComparison.Ordinal);

        if (startIndex < 0 || endIndex <= startIndex)
        {
            throw new InvalidOperationException($"Section not found: {start}");
        }

        return content[startIndex..endIndex];
    }
}
