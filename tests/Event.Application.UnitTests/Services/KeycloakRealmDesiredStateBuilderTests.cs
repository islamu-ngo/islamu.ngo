// ABOUTME: Tests composable Keycloak realm desired-state registry behavior.
// ABOUTME: Verifies the first-party ISLAMU identity contract and extension points.

using Explore.Application;
using Explore.Application.Contracts.Services;
using Explore.Application.DTOs.Onboarding;
using Explore.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Event.Application.UnitTests.Services;

public class KeycloakRealmDesiredStateBuilderTests
{
    [Test]
    public async Task Build_WithDefaultContributor_ReturnsIslamuRealmContract()
    {
        var builder = KeycloakRealmDesiredStateBuilder.CreateDefault();

        var desiredState = builder.Build(new KeycloakRealmDesiredStateBuildRequestDto
        {
            Realm = "ISLAMU",
            BlazorClientId = "islamu-ngo-blazor",
            ApiClientId = "islamu-ngo-api",
            BlazorRedirectUris = ["https://ngo.example.com/signin-oidc"],
            BlazorWebOrigins = ["https://ngo.example.com"]
        });

        await Assert.That(desiredState.DestructiveOperationsSupported).IsFalse();
        await Assert.That(desiredState.RequiredRealmRoles).IsEquivalentTo(["offline_access"]);
        await Assert.That(desiredState.ClientScopes).HasSingleItem();
        await Assert.That(desiredState.ClientScopes.Single().Name).IsEqualTo("offline_access");
        await Assert.That(desiredState.ClientScopes.Single().RealmRoleMappings).IsEquivalentTo(["offline_access"]);
        await Assert.That(desiredState.RoleComposites).HasSingleItem();
        await Assert.That(desiredState.RoleComposites.Single().RoleName).IsEqualTo("default-roles-islamu");
        await Assert.That(desiredState.RoleComposites.Single().CompositeRoleNames).IsEquivalentTo(["offline_access"]);

        await Assert.That(desiredState.Clients.Select(client => client.ClientId))
            .IsEquivalentTo(["islamu-ngo-blazor", "islamu-ngo-api"]);

        var blazorClient = desiredState.Clients.Single(client => client.ClientId == "islamu-ngo-blazor");
        await Assert.That(blazorClient.ClientKind).IsEqualTo("blazor-confidential");
        await Assert.That(blazorClient.PublicClient).IsFalse();
        await Assert.That(blazorClient.BearerOnly).IsFalse();
        await Assert.That(blazorClient.StandardFlowEnabled).IsTrue();
        await Assert.That(blazorClient.DirectAccessGrantsEnabled).IsFalse();
        await Assert.That(blazorClient.ServiceAccountsEnabled).IsFalse();
        await Assert.That(blazorClient.RedirectUris).IsEquivalentTo(["https://ngo.example.com/signin-oidc"]);
        await Assert.That(blazorClient.WebOrigins).IsEquivalentTo(["https://ngo.example.com"]);
        await Assert.That(blazorClient.OptionalClientScopes).IsEquivalentTo(["offline_access"]);

        var apiClient = desiredState.Clients.Single(client => client.ClientId == "islamu-ngo-api");
        await Assert.That(apiClient.ClientKind).IsEqualTo("api-bearer");
        await Assert.That(apiClient.BearerOnly).IsTrue();
        await Assert.That(apiClient.PublicClient).IsFalse();
        await Assert.That(apiClient.StandardFlowEnabled).IsFalse();
        await Assert.That(apiClient.DirectAccessGrantsEnabled).IsFalse();
        await Assert.That(apiClient.ServiceAccountsEnabled).IsFalse();
        await Assert.That(apiClient.ProtocolMappers).HasSingleItem();
        await Assert.That(apiClient.ProtocolMappers.Single().Name).IsEqualTo("islamu-ngo-api-audience");
        await Assert.That(apiClient.ProtocolMappers.Single().MapperType).IsEqualTo("oidc-audience-mapper");
        await Assert.That(apiClient.ProtocolMappers.Single().IncludedClientAudience).IsEqualTo("islamu-ngo-api");
        await Assert.That(apiClient.ProtocolMappers.Single().AddToAccessToken).IsTrue();
        await Assert.That(apiClient.ProtocolMappers.Single().AddToIdToken).IsFalse();

        await Assert.That(FlattenDesiredStateValues(desiredState)).DoesNotContain("islamu-event-api");
        await Assert.That(FlattenDesiredStateValues(desiredState)).DoesNotContain("islamu-event-blazor");
        await Assert.That(FlattenDesiredStateValues(desiredState)).DoesNotContain("event-blazor");
        await Assert.That(FlattenDesiredStateValues(desiredState)).DoesNotContain("control-plane");
    }

    [Test]
    public async Task Build_WithoutApiClient_OnlyCreatesBlazorClient()
    {
        var builder = KeycloakRealmDesiredStateBuilder.CreateDefault();

        var desiredState = builder.Build(new KeycloakRealmDesiredStateBuildRequestDto
        {
            Realm = "ISLAMU",
            BlazorClientId = "islamu-ngo-blazor"
        });

        await Assert.That(desiredState.Clients.Select(client => client.ClientId)).IsEquivalentTo(["islamu-ngo-blazor"]);
        await Assert.That(desiredState.Clients.SelectMany(client => client.ProtocolMappers)).IsEmpty();
    }

    [Test]
    public async Task Build_WithAdditionalContributor_ComposesFutureProjectClient()
    {
        var builder = new KeycloakRealmDesiredStateBuilder(
        [
            new IslamuKeycloakIdentityContractContributor(),
            new FutureProjectContributor()
        ]);

        var desiredState = builder.Build(new KeycloakRealmDesiredStateBuildRequestDto
        {
            Realm = "ISLAMU",
            BlazorClientId = "islamu-ngo-blazor",
            ApiClientId = "islamu-ngo-api"
        });

        await Assert.That(desiredState.Clients.Select(client => client.ClientId)).Contains("islamu-ngo-blazor");
        await Assert.That(desiredState.Clients.Select(client => client.ClientId)).Contains("islamu-ngo-api");
        await Assert.That(desiredState.Clients.Select(client => client.ClientId)).Contains("identity-service-api");
        await Assert.That(desiredState.RequiredRealmRoles).Contains("offline_access");
        await Assert.That(desiredState.RequiredRealmRoles).Contains("identity-service-reader");
        await Assert.That(desiredState.DestructiveOperationsSupported).IsFalse();
    }

    [Test]
    public async Task ConfigureApplicationServices_RegistersIslamuContributorForInjectedBuilder()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.ConfigureApplicationServices(configuration);

        await using var provider = services.BuildServiceProvider();
        var builder = provider.GetRequiredService<IKeycloakRealmDesiredStateBuilder>();

        var desiredState = builder.Build(new KeycloakRealmDesiredStateBuildRequestDto
        {
            Realm = "ISLAMU",
            BlazorClientId = "islamu-ngo-blazor",
            ApiClientId = "islamu-ngo-api"
        });

        await Assert.That(desiredState.Clients.Select(client => client.ClientId))
            .IsEquivalentTo(["islamu-ngo-blazor", "islamu-ngo-api"]);
    }

    [Test]
    public async Task OnboardingKeycloakRequestDefaults_UseIslamuApiClientId()
    {
        await Assert.That(new KeycloakRealmDoctorRequestDto().ApiClientId).IsEqualTo("islamu-ngo-api");
        await Assert.That(new KeycloakRealmSyncPreviewRequestDto().ApiClientId).IsEqualTo("islamu-ngo-api");
        await Assert.That(new KeycloakRealmSyncApplyRequestDto().ApiClientId).IsEqualTo("islamu-ngo-api");
    }

    private static IReadOnlyList<string> FlattenDesiredStateValues(KeycloakRealmDesiredStateDto desiredState) =>
    [
        desiredState.Realm,
        desiredState.BlazorClientId,
        desiredState.ApiClientId ?? string.Empty,
        .. desiredState.RequiredRealmRoles,
        .. desiredState.RoleComposites.Select(composite => composite.RoleName),
        .. desiredState.RoleComposites.SelectMany(composite => composite.CompositeRoleNames),
        .. desiredState.ClientScopes.Select(scope => scope.Name),
        .. desiredState.ClientScopes.SelectMany(scope => scope.RealmRoleMappings),
        .. desiredState.Clients.Select(client => client.ClientId),
        .. desiredState.Clients.Select(client => client.DisplayName),
        .. desiredState.Clients.Select(client => client.ClientKind),
        .. desiredState.Clients.SelectMany(client => client.RedirectUris),
        .. desiredState.Clients.SelectMany(client => client.WebOrigins),
        .. desiredState.Clients.SelectMany(client => client.OptionalClientScopes),
        .. desiredState.Clients.SelectMany(client => client.DefaultClientScopes),
        .. desiredState.Clients.SelectMany(client => client.RealmRoleMappings),
        .. desiredState.Clients.SelectMany(client => client.ProtocolMappers).Select(mapper => mapper.Name),
        .. desiredState.Clients.SelectMany(client => client.ProtocolMappers).Select(mapper => mapper.MapperType),
        .. desiredState.Clients.SelectMany(client => client.ProtocolMappers).Select(mapper => mapper.IncludedClientAudience ?? string.Empty)
    ];

    private sealed class FutureProjectContributor : IKeycloakIdentityContractContributor
    {
        public string ContractName => "identity-service";

        public void Contribute(KeycloakRealmDesiredStateDto desiredState, KeycloakRealmDesiredStateBuildRequestDto request)
        {
            desiredState.RequiredRealmRoles = desiredState.RequiredRealmRoles.Append("identity-service-reader").ToArray();
            desiredState.Clients = desiredState.Clients.Append(new KeycloakClientDesiredStateDto
            {
                ClientId = "identity-service-api",
                DisplayName = "identity-service-api",
                ClientKind = "api-bearer",
                BearerOnly = true
            }).ToArray();
        }
    }
}
