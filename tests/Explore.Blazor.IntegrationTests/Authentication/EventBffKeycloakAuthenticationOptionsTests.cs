// ABOUTME: Focused tests for reusable BFF Keycloak authentication option defaults.
// ABOUTME: Protects the public BFF client fallback while preserving explicit override precedence.

using Event.Web.BffHosting.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Explore.Blazor.IntegrationTests.Authentication;

public sealed class EventBffKeycloakAuthenticationOptionsTests
{
    [Test]
    public void FromConfiguration_PublicWebDefaultsToNgoBffClientId()
    {
        var options = EventBffKeycloakAuthenticationOptions.FromConfiguration(
            BuildConfiguration(),
            EventBffHostProfile.PublicWeb,
            new TestHostEnvironment());

        options.ClientId.Should().Be("islamu-ngo-blazor");
    }

    [Test]
    public void FromConfiguration_PublicWebExplicitClientIdWins()
    {
        var options = EventBffKeycloakAuthenticationOptions.FromConfiguration(
            BuildConfiguration(new Dictionary<string, string?>
            {
                ["Keycloak:ClientId"] = "explicit-client"
            }),
            EventBffHostProfile.PublicWeb,
            new TestHostEnvironment());

        options.ClientId.Should().Be("explicit-client");
    }

    [Test]
    public void FromConfiguration_DefaultCookieNamesPreserveHostProfileContracts()
    {
        var configuration = BuildConfiguration();

        var publicDevelopment = EventBffKeycloakAuthenticationOptions.FromConfiguration(
            configuration,
            EventBffHostProfile.PublicWeb,
            new TestHostEnvironment());
        var controlPlaneDevelopment = EventBffKeycloakAuthenticationOptions.FromConfiguration(
            configuration,
            EventBffHostProfile.ControlPlane,
            new TestHostEnvironment());
        var publicProduction = EventBffKeycloakAuthenticationOptions.FromConfiguration(
            configuration,
            EventBffHostProfile.PublicWeb,
            new TestHostEnvironment { EnvironmentName = Environments.Production });
        var controlPlaneProduction = EventBffKeycloakAuthenticationOptions.FromConfiguration(
            configuration,
            EventBffHostProfile.ControlPlane,
            new TestHostEnvironment { EnvironmentName = Environments.Production });

        publicDevelopment.CookieName.Should().Be("islamu-ngo-public-dev");
        controlPlaneDevelopment.CookieName.Should().Be("event-control-plane-bff-dev");
        publicProduction.CookieName.Should().Be("__Host-islamu-ngo-web");
        controlPlaneProduction.CookieName.Should().Be("__Host-event-control-plane-bff");
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?>? values = null) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values ?? new Dictionary<string, string?>())
            .Build();

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Explore.Blazor.IntegrationTests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
