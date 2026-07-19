// ABOUTME: Focused tests for API JWT audience validation defaults.
// ABOUTME: Exercises the configured JwtBearer audience validator without starting Keycloak.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Explore.API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Event.Api.IntegrationTests.Features;

public sealed class KeycloakAudienceValidatorTests
{
    [Test]
    public async Task DefaultAudienceValidator_AcceptsNgoApiAudienceAndNgoBffAuthorizedParty()
    {
        var validator = BuildDefaultAudienceValidator();

        var ngoApiAudiencePasses = validator(
            ["islamu-ngo-api"],
            new JwtSecurityToken(),
            new());
        var ngoBffAuthorizedPartyPasses = validator(
            [],
            new JwtSecurityToken(claims: [new Claim("azp", "islamu-ngo-blazor")]),
            new());

        await Assert.That(ngoApiAudiencePasses).IsTrue();
        await Assert.That(ngoBffAuthorizedPartyPasses).IsTrue();
    }

    [Test]
    public async Task DefaultAudienceValidator_RejectsOldEventOnlyAudienceAndAuthorizedParty()
    {
        var validator = BuildDefaultAudienceValidator();

        var oldEventOnlyPasses = validator(
            ["islamu-event-api"],
            new JwtSecurityToken(claims: [new Claim("azp", "islamu-event-api")]),
            new());

        await Assert.That(oldEventOnlyPasses).IsFalse();
    }

    private static AudienceValidator BuildDefaultAudienceValidator()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:RequireHttpsMetadata"] = "false"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment());
        services.AddApiAuthentication(configuration, new TestWebHostEnvironment(), skipAuthorityWarmup: true);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        return options.TokenValidationParameters.AudienceValidator
            ?? throw new InvalidOperationException("JWT audience validator was not configured.");
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Event.Api.IntegrationTests";
        public string WebRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
