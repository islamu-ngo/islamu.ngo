// ABOUTME: Architecture guards for local Aspire infrastructure required by integrated services.
// ABOUTME: Keeps local service endpoints and readiness sequencing explicit.

namespace Event.Architecture.Tests;

public sealed class AspireLocalInfrastructureArchitectureTests
{
    private static readonly string RepoRoot = ResolveRepoRoot();
    private const string LocalKeycloakAdminPassword = "local-keycloak-admin-fixture";
    private const string LocalNgoBffClientSecret = "islamu-ngo-blazor-local-secret";
    private const string LocalSetupSecret = "islamu-ngo-local-setup-secret";

    [Test]
    public async Task LocalLite_MustRunMigrationServiceBeforeApiAndBlazorWithoutLocalInfra()
    {
        var appHost = await File.ReadAllTextAsync(Path.Combine(RepoRoot, "src", "Explore.AppHost", "AppHost.cs"));
        var migrationRegistrationIndex = appHost.IndexOf(
            "var migrations = WithProfileSecretMode(",
            StringComparison.Ordinal);
        var localDatabaseBranchIndex = appHost.IndexOf(
            "if (database is not null)",
            StringComparison.Ordinal);

        await Assert.That(migrationRegistrationIndex).IsGreaterThanOrEqualTo(0);
        await Assert.That(localDatabaseBranchIndex).IsGreaterThan(migrationRegistrationIndex);
        await Assert.That(appHost).Contains("builder.AddProject<Projects.Event_MigrationService>");
        await Assert.That(appHost).Contains("var exploreAPI = WithProfileSecretMode(");
        await Assert.That(appHost).Contains("var exploreBlazor = WithProfileSecretMode(");
        await Assert.That(appHost).DoesNotContain("if (migrations is not null)");
        await Assert.That(appHost).Contains("exploreAPI = exploreAPI\n    .WithReference(migrations)\n    .WaitForCompletion(migrations);");
        await Assert.That(appHost).Contains("exploreBlazor = exploreBlazor\n    .WithReference(migrations)\n    .WaitForCompletion(migrations);");
        await Assert.That(appHost).Contains("if (runMode.UsesLocalData())");
        await Assert.That(appHost).Contains("if (runMode is AspireRunMode.FullLocal or AspireRunMode.DefaultLocal)");
    }

    [Test]
    public async Task OspreyCoordinator_MustUseReadyLocalKafkaBroker()
    {
        var appHost = await File.ReadAllTextAsync(Path.Combine(RepoRoot, "src", "Explore.AppHost", "AppHost.cs"));

        await Assert.That(appHost).Contains("\"osprey-kafka\"");
        await Assert.That(appHost).Contains("OSPREY_KAFKA_BOOTSTRAP_SERVERS");
        await Assert.That(appHost).Contains("osprey-kafka:29092");
        await Assert.That(appHost).Contains("\"osprey-kafka-bootstrap\"");
        await Assert.That(appHost).Contains("WaitForCompletion(ospreyKafkaBootstrap)");
    }

    [Test]
    public async Task CerbosGrpcCompatibilityEndpoint_MustUseAspireResolvedEndpoint()
    {
        var appHost = await File.ReadAllTextAsync(Path.Combine(RepoRoot, "src", "Explore.AppHost", "AppHost.cs"));

        await Assert.That(appHost).Contains(".WithEnvironment(\"CERBOS_GRPC_ENDPOINT\", cerbosGrpcEndpoint)");
    }

    [Test]
    public async Task CerbosAdminApi_MustUseMutablePostgresStore()
    {
        var cerbosConfig = await File.ReadAllTextAsync(
            Path.Combine(RepoRoot, "cerbos", "config", ".cerbos.yaml"));
        var appHost = await File.ReadAllTextAsync(Path.Combine(RepoRoot, "src", "Explore.AppHost", "AppHost.cs"));
        var compose = await File.ReadAllTextAsync(Path.Combine(RepoRoot, "docker-compose.yml"));

        await Assert.That(cerbosConfig).Contains("storage:\n  driver: \"postgres\"");
        await Assert.That(appHost).Contains("\"CERBOS_PG_URL\"");
        await Assert.That(compose).Contains("CERBOS_PG_URL:");
    }

    [Test]
    public async Task LocalFull_MustUseAdditiveNgoKeycloakProvisioner()
    {
        var appHost = await File.ReadAllTextAsync(Path.Combine(RepoRoot, "src", "Explore.AppHost", "AppHost.cs"));

        await Assert.That(appHost).Contains("keycloak-ngo-provisioner");
        await Assert.That(appHost).Contains("quay.io/keycloak/keycloak");
        await Assert.That(appHost).Contains("26.5.2");
        await Assert.That(appHost).Contains("provision-islamu-ngo-client.sh");
        await Assert.That(appHost).Contains("isReadOnly: true");
        await Assert.That(appHost).Contains("KEYCLOAK_INTERNAL_URL");
        await Assert.That(appHost).Contains("http://keycloak:8080/auth");
        await Assert.That(appHost).Contains("KEYCLOAK_ALLOW_INSECURE_HTTP");
        await Assert.That(appHost).Contains("KEYCLOAK_ADMIN_REALM");
        await Assert.That(appHost).Contains("KEYCLOAK_ISLAMU_NGO_PROVISIONING_REQUIRED");
        await Assert.That(appHost).Contains("islamu-ngo-blazor");
        await Assert.That(appHost).Contains("islamu-ngo-api");
        await Assert.That(appHost).Contains("islamu-ngo-api-audience");
        await Assert.That(appHost).Contains(LocalNgoBffClientSecret);
        await Assert.That(appHost).Contains(LocalKeycloakAdminPassword);
        await Assert.That(appHost).DoesNotContain("keycloak-init");
        await Assert.That(appHost).DoesNotContain("keycloak-init.sh");
        await Assert.That(appHost).DoesNotContain("KEYCLOAK_INIT_ALLOW_DEFAULT_LOCAL_SECRET");
        await Assert.That(appHost).DoesNotContain("islamu-event-blazor");
        await Assert.That(appHost).DoesNotContain("islamu-event-api");
        await Assert.That(appHost).DoesNotContain("\"islamu-ngo-blazor-secret\"");
        await Assert.That(appHost).DoesNotContain("configuration[\"KEYCLOAK_ADMIN_PASSWORD\"] ?? \"admin\"");
    }

    [Test]
    public async Task LocalKeycloakFixtures_MustStayAlignedBetweenAppHostAndEnvExample()
    {
        var appHost = await File.ReadAllTextAsync(Path.Combine(RepoRoot, "src", "Explore.AppHost", "AppHost.cs"));
        var envExample = await File.ReadAllTextAsync(Path.Combine(RepoRoot, ".env.example"));

        await Assert.That(appHost).Contains($"LocalKeycloakAdminPassword = \"{LocalKeycloakAdminPassword}\"");
        await Assert.That(appHost).Contains($"LocalNgoBffClientSecret = \"{LocalNgoBffClientSecret}\"");
        await Assert.That(envExample).Contains($"KEYCLOAK_ADMIN_PASSWORD={LocalKeycloakAdminPassword}");
        await Assert.That(envExample).Contains($"KEYCLOAK_BLAZOR_CLIENT_SECRET={LocalNgoBffClientSecret}");
        await Assert.That(envExample).Contains($"KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET={LocalNgoBffClientSecret}");
        await Assert.That(envExample).DoesNotContain("KEYCLOAK_ADMIN_PASSWORD=admin");
        await Assert.That(envExample).DoesNotContain("islamu-ngo-blazor-secret");
    }

    [Test]
    public async Task LocalSetupSecretFixture_MustStayAlignedAndReachApiAndBff()
    {
        var appHost = await File.ReadAllTextAsync(Path.Combine(RepoRoot, "src", "Explore.AppHost", "AppHost.cs"));
        var envExample = await File.ReadAllTextAsync(Path.Combine(RepoRoot, ".env.example"));

        await Assert.That(appHost).Contains($"LocalSetupSecret = \"{LocalSetupSecret}\"");
        await Assert.That(appHost).Contains(
            "ConfiguredValue(builder.Configuration, \"SETUP_SECRET\", LocalSetupSecret)");
        await Assert.That(
                appHost.Split(".WithEnvironment(\"SETUP_SECRET\", localSetupSecret)", StringSplitOptions.None).Length - 1)
            .IsEqualTo(2);
        await Assert.That(envExample).Contains($"SETUP_SECRET={LocalSetupSecret}");
        await Assert.That(envExample).DoesNotContain("writes it to API startup logs");
    }

    [Test]
    public async Task LocalWebhookProvider_MustStayReadyWithoutOptionalSvixCredentials()
    {
        var appHost = await File.ReadAllTextAsync(Path.Combine(RepoRoot, "src", "Explore.AppHost", "AppHost.cs"));
        var envExample = await File.ReadAllTextAsync(Path.Combine(RepoRoot, ".env.example"));

        await Assert.That(appHost).Contains("var svixAuthToken = configuration[\"WEBHOOKS_SVIX_AUTH_TOKEN\"];");
        await Assert.That(appHost).Contains(
            "configuration[\"WEBHOOKS_PROVIDER\"] ?? (string.IsNullOrWhiteSpace(svixAuthToken) ? \"Local\" : \"Svix\")");
        await Assert.That(envExample).Contains("WEBHOOKS_PROVIDER=Local");
    }

    [Test]
    public async Task LocalProjects_MustWaitForNgoProvisionerCompletion()
    {
        var appHost = await File.ReadAllTextAsync(Path.Combine(RepoRoot, "src", "Explore.AppHost", "AppHost.cs"));

        await Assert.That(appHost).Contains("KeycloakNgoProvisioner: keycloakNgoProvisioner");
        await Assert.That(appHost).Contains(".WaitForCompletion(resources.KeycloakNgoProvisioner)");
        await Assert.That(appHost).DoesNotContain("resources.KeycloakInit");
    }

    [Test]
    public async Task LocalCompose_MustUseAdditiveNgoKeycloakProvisioner()
    {
        var compose = await File.ReadAllTextAsync(Path.Combine(RepoRoot, "docker-compose.yml"));

        await Assert.That(compose).Contains("keycloak-ngo-provisioner:");
        await Assert.That(compose).Contains("context: docker/keycloak");
        await Assert.That(compose).Contains("dockerfile: Dockerfile.provisioner");
        await Assert.That(compose).Contains("quay.io/keycloak/keycloak:26.5.2");
        await Assert.That(compose).Contains("KEYCLOAK_ADMIN_PASSWORD: ${KEYCLOAK_ADMIN_PASSWORD:-local-keycloak-admin-fixture}");
        await Assert.That(compose).Contains("KEYCLOAK_ALLOW_INSECURE_HTTP: \"true\"");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_PROVISIONING_REQUIRED: \"true\"");
        await Assert.That(compose).Contains("KEYCLOAK_ADMIN_REALM: ${KEYCLOAK_ADMIN_REALM:-master}");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID: ${KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID:-islamu-ngo-blazor}");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID: ${KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID:-islamu-ngo-api}");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME: ${KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME:-islamu-ngo-api-audience}");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET: ${KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET:-islamu-ngo-blazor-local-secret}");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_REDIRECT_URIS:");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_WEB_ORIGINS:");
        await Assert.That(compose).Contains("KEYCLOAK_ISLAMU_NGO_LOGOUT_REDIRECT_URIS:");
        await Assert.That(compose).Contains("Keycloak__ClientId: ${KEYCLOAK_BLAZOR_CLIENT_ID:-islamu-ngo-blazor}");
        await Assert.That(compose).Contains("Keycloak__ClientSecret: ${KEYCLOAK_BLAZOR_CLIENT_SECRET:-islamu-ngo-blazor-local-secret}");
        await Assert.That(compose).Contains("keycloak-ngo-provisioner:\n        condition: service_completed_successfully");
        await Assert.That(compose).DoesNotContain("keycloak-init:");
        await Assert.That(compose).DoesNotContain("keycloak-init.sh");
        await Assert.That(compose).DoesNotContain("KEYCLOAK_INIT_ALLOW_DEFAULT_LOCAL_SECRET");
        await Assert.That(compose).DoesNotContain("islamu-event-blazor");
        await Assert.That(compose).DoesNotContain("islamu-event-api");
    }

    private static string ResolveRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Explore.slnx"))
                && (Directory.Exists(Path.Combine(current.FullName, "Explore.AppHost")) ||
                    Directory.Exists(Path.Combine(current.FullName, "src", "Explore.AppHost"))))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root containing Explore.slnx and Explore.AppHost.");
    }
}
