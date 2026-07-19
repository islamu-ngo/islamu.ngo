// ABOUTME: Architecture guards for management-port-only Prometheus metrics exposure.
// ABOUTME: Keeps public app ports separate from operator scraping ports in source and images.

namespace Event.Architecture.Tests;

public sealed class ManagementPortMetricsArchitectureTests
{
    [Test]
    public async Task MetricsEndpoint_MustBeGatedToManagementPort()
    {
        var serviceDefaults = await File.ReadAllTextAsync(
            ContextSystemHelpers.RepoPath("Explore.ServiceDefaults", "Extensions.cs"));

        await Assert.That(serviceDefaults).Contains("public const int ManagementPort = 9090;");
        await Assert.That(serviceDefaults).Contains("app.MapHealthChecks(HealthEndpointPath, healthCheckOptions);");
        await Assert.That(serviceDefaults).Contains("app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions");
        await Assert.That(serviceDefaults).Contains("app.MapPrometheusScrapingEndpoint(\"/metrics\")");
        await Assert.That(serviceDefaults).Contains(".AddEndpointFilter(async (context, next) =>");
        await Assert.That(serviceDefaults).Contains("context.HttpContext.Connection.LocalPort == ManagementPort");
        await Assert.That(serviceDefaults).Contains("Results.NotFound()");
        await Assert.That(serviceDefaults).Contains(".AllowAnonymous()");
        await Assert.That(serviceDefaults).Contains(".ExcludeFromDescription()");
        await Assert.That(serviceDefaults).DoesNotContain("Request.Host");
        await Assert.That(serviceDefaults).DoesNotContain("Headers.Host");
        await Assert.That(serviceDefaults).DoesNotContain("HostString");
    }

    [Test]
    public async Task RuntimeDockerfiles_MustExposePublicAndManagementPorts()
    {
        await AssertDockerfileExposesManagementPort("Explore.API");
        await AssertDockerfileExposesManagementPort("Explore.Blazor");
    }

    private static async Task AssertDockerfileExposesManagementPort(string projectName)
    {
        var dockerfile = await File.ReadAllTextAsync(ContextSystemHelpers.RepoPath(projectName, "Dockerfile"));

        await Assert.That(dockerfile).Contains("EXPOSE 8080");
        await Assert.That(dockerfile).Contains("EXPOSE 9090");
        await Assert.That(dockerfile).DoesNotContain("EXPOSE 8081");
    }
}
