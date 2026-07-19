// ABOUTME: Cerbos gRPC SDK integration tests using the production Cerbos.Sdk client library.
// ABOUTME: Validates that the official gRPC client can connect to the containerized PDP and return correct decisions.

using Cerbos.Api.V1.Effect;
using Cerbos.Sdk;
using Cerbos.Sdk.Builder;
using Cerbos.Sdk.Response;
using Cerbos.Sdk.Utility;
using Event.Api.IntegrationTests.Fixtures;
using FluentAssertions;
using Grpc.Net.Client;

namespace Event.Api.IntegrationTests.Features;

/// <summary>
/// Production-faithful gRPC SDK tests mirroring how
/// <c>CerbosAuthorizationService</c> uses the Cerbos SDK.
/// Validates the gRPC transport path separately from HTTP API tests.
/// </summary>
[Category(TestCategories.PolicyContract)]
[NotInParallel("SecurityInfra")]
[ClassDataSource<SecurityInfrastructureFixture>(Shared = SharedType.PerAssembly)]
public class CerbosGrpcSdkTests : IDisposable
{
    private readonly ICerbosClient _client;

    public CerbosGrpcSdkTests(SecurityInfrastructureFixture infra)
    {
        var grpcEndpoint = infra.CerbosGrpcEndpoint;

        var builder = CerbosClientBuilder
            .ForTarget(grpcEndpoint)
            .WithGrpcChannelOptions(new GrpcChannelOptions
            {
                HttpHandler = new SocketsHttpHandler(),
                DisposeHttpClient = true
            });

        if (grpcEndpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            builder = builder.WithPlaintext();
        }

        _client = builder.Build();
    }

    public void Dispose()
    {
        (_client as IDisposable)?.Dispose();
    }

    #region Blog via gRPC SDK

    [Test]
    public async Task GrpcSdk_InstanceAdmin_ShouldBeAllowedAllBlogActions()
    {
        var principal = Principal
            .NewInstance("user-instance-admin", "islamuevent_authenticated_user")
            .WithAttribute("isInstanceAdmin", AttributeValue.BoolValue(true))
            .WithAttribute("tenantMemberships", AttributeValue.MapValue(new Dictionary<string, AttributeValue>()))
            .WithAttribute("orgMemberships", AttributeValue.MapValue(new Dictionary<string, AttributeValue>()));

        var resource = ResourceEntry
            .NewInstance("islamuevent_blog", "blog-grpc-1")
            .WithAttribute("tenantId", AttributeValue.StringValue("tenant-1"))
            .WithAttribute("blogId", AttributeValue.StringValue("blog-grpc-1"))
            .WithAttribute("isPublished", AttributeValue.BoolValue(false))
            .WithActions("view", "create", "update", "delete", "publish", "unpublish");

        var response = await SendCheckResourcesAsync(principal, resource);
        var result = response.Find("blog-grpc-1");

        result.Should().NotBeNull();
        AssertEffect(result!, "view", Effect.Allow);
        AssertEffect(result!, "create", Effect.Allow);
        AssertEffect(result!, "update", Effect.Allow);
        AssertEffect(result!, "delete", Effect.Allow);
        AssertEffect(result!, "publish", Effect.Allow);
        AssertEffect(result!, "unpublish", Effect.Allow);
    }

    [Test]
    public async Task GrpcSdk_TenantAdmin_ShouldManageBlogsInOwnTenant()
    {
        var principal = BuildTenantAdminPrincipal("tenant-1");
        var resource = ResourceEntry
            .NewInstance("islamuevent_blog", "blog-grpc-2")
            .WithAttribute("tenantId", AttributeValue.StringValue("tenant-1"))
            .WithAttribute("blogId", AttributeValue.StringValue("blog-grpc-2"))
            .WithActions("view", "create", "update", "delete", "publish", "unpublish");

        var response = await SendCheckResourcesAsync(principal, resource);
        var result = response.Find("blog-grpc-2")!;

        AssertEffect(result, "view", Effect.Allow);
        AssertEffect(result, "create", Effect.Allow);
        AssertEffect(result, "update", Effect.Allow);
        AssertEffect(result, "delete", Effect.Allow);
        AssertEffect(result, "publish", Effect.Allow);
        AssertEffect(result, "unpublish", Effect.Allow);
    }

    [Test]
    public async Task GrpcSdk_TenantAdmin_ShouldBeDeniedBlogsInOtherTenant()
    {
        var principal = BuildTenantAdminPrincipal("tenant-1");
        var resource = ResourceEntry
            .NewInstance("islamuevent_blog", "blog-grpc-3")
            .WithAttribute("tenantId", AttributeValue.StringValue("tenant-other"))
            .WithAttribute("blogId", AttributeValue.StringValue("blog-grpc-3"))
            .WithActions("view", "create", "update", "delete", "publish", "unpublish");

        var response = await SendCheckResourcesAsync(principal, resource);
        var result = response.Find("blog-grpc-3")!;

        AssertEffect(result, "view", Effect.Deny);
        AssertEffect(result, "create", Effect.Deny);
        AssertEffect(result, "update", Effect.Deny);
        AssertEffect(result, "delete", Effect.Deny);
        AssertEffect(result, "publish", Effect.Deny);
        AssertEffect(result, "unpublish", Effect.Deny);
    }

    [Test]
    public async Task GrpcSdk_RegularUser_ShouldBeDeniedBlogEditorialActions()
    {
        var principal = BuildRegularUserPrincipal();
        var resource = ResourceEntry
            .NewInstance("islamuevent_blog", "blog-grpc-4")
            .WithAttribute("tenantId", AttributeValue.StringValue("tenant-1"))
            .WithAttribute("blogId", AttributeValue.StringValue("blog-grpc-4"))
            .WithAttribute("isPublished", AttributeValue.BoolValue(true))
            .WithActions("view", "create", "update", "delete", "publish", "unpublish");

        var response = await SendCheckResourcesAsync(principal, resource);
        var result = response.Find("blog-grpc-4")!;

        AssertEffect(result, "view", Effect.Deny,
            "published public reads bypass Cerbos; Cerbos blog view is editorial management access");
        AssertEffect(result, "create", Effect.Deny);
        AssertEffect(result, "update", Effect.Deny);
        AssertEffect(result, "delete", Effect.Deny);
        AssertEffect(result, "publish", Effect.Deny);
        AssertEffect(result, "unpublish", Effect.Deny);
    }

    #endregion

    #region Batch Check via gRPC SDK

    [Test]
    public async Task GrpcSdk_BatchCheck_ShouldReturnResultsForMultipleResources()
    {
        var principal = BuildTenantAdminPrincipal("tenant-1");

        var blogResource = ResourceEntry
            .NewInstance("islamuevent_blog", "batch-blog-1")
            .WithAttribute("tenantId", AttributeValue.StringValue("tenant-1"))
            .WithAttribute("blogId", AttributeValue.StringValue("batch-blog-1"))
            .WithActions("view");

        var orgResource = ResourceEntry
            .NewInstance("islamuevent_organization", "batch-org-1")
            .WithAttribute("tenantId", AttributeValue.StringValue("tenant-1"))
            .WithAttribute("organizationId", AttributeValue.StringValue("org-1"))
            .WithActions("view");

        var request = CheckResourcesRequest
            .NewInstance()
            .WithRequestId(RequestId.Generate())
            .WithPrincipal(principal)
            .WithResourceEntries(blogResource, orgResource);

        var response = await _client.CheckResourcesAsync(request);

        var blogResult = response.Find("batch-blog-1")!;
        var orgResult = response.Find("batch-org-1")!;

        AssertEffect(blogResult, "view", Effect.Allow);
        AssertEffect(orgResult, "view", Effect.Allow);
    }

    #endregion

    #region Tenant Setting Lock via gRPC SDK

    [Test]
    public async Task GrpcSdk_TenantAdmin_ShouldBeDeniedUpdateWhenLockedByInstance()
    {
        var principal = BuildTenantAdminPrincipal("tenant-1");
        var resource = ResourceEntry
            .NewInstance("islamuevent_tenant_setting", "setting-grpc-1")
            .WithAttribute("tenantId", AttributeValue.StringValue("tenant-1"))
            .WithAttribute("isLockedByInstance", AttributeValue.BoolValue(true))
            .WithActions("update");

        var response = await SendCheckResourcesAsync(principal, resource);
        var result = response.Find("setting-grpc-1")!;

        AssertEffect(result, "update", Effect.Deny,
            "tenant admin must be denied update when isLockedByInstance=true");
    }


    [Test]
    public async Task GrpcSdk_TenantAdmin_ShouldBeAllowedTenantBrandingDocumentUpdateWhenLockedForHandlerValidation()
    {
        var principal = BuildTenantAdminPrincipal("tenant-1");
        var resource = ResourceEntry
            .NewInstance("islamuevent_tenant_setting", "setting-grpc-tenant-branding")
            .WithAttribute("tenantId", AttributeValue.StringValue("tenant-1"))
            .WithAttribute("documentKey", AttributeValue.StringValue("tenant.branding"))
            .WithAttribute("isLockedByInstance", AttributeValue.BoolValue(true))
            .WithActions("update");

        var response = await SendCheckResourcesAsync(principal, resource);
        var result = response.Find("setting-grpc-tenant-branding")!;

        AssertEffect(result, "update", Effect.Allow,
            "tenant.branding uses handler-level field locks after resource authorization");
    }

    #endregion

    #region Helpers

    private async Task<CheckResourcesResponse> SendCheckResourcesAsync(
        Principal principal, ResourceEntry resource)
    {
        var request = CheckResourcesRequest
            .NewInstance()
            .WithRequestId(RequestId.Generate())
            .WithPrincipal(principal)
            .WithResourceEntries(resource);

        return await _client.CheckResourcesAsync(request);
    }

    private static void AssertEffect(
        CheckResourcesResponse.Types.ResultEntry result,
        string action,
        Effect expected,
        string? because = null)
    {
        result.Actions.TryGetValue(action, out var actual).Should().BeTrue(
            $"action '{action}' must be present in the Cerbos response");
        actual.Should().Be(expected, because ?? $"action '{action}' should be {expected}");
    }

    private static Principal BuildRegularUserPrincipal() =>
        Principal
            .NewInstance("user-regular-grpc", "islamuevent_authenticated_user")
            .WithAttribute("isInstanceAdmin", AttributeValue.BoolValue(false))
            .WithAttribute("tenantMemberships", AttributeValue.MapValue(new Dictionary<string, AttributeValue>()))
            .WithAttribute("orgMemberships", AttributeValue.MapValue(new Dictionary<string, AttributeValue>()));

    private static Principal BuildTenantAdminPrincipal(string tenantId) =>
        Principal
            .NewInstance("user-tenant-admin-grpc", "islamuevent_authenticated_user")
            .WithAttribute("isInstanceAdmin", AttributeValue.BoolValue(false))
            .WithAttribute("tenantMemberships", AttributeValue.MapValue(new Dictionary<string, AttributeValue>
            {
                [tenantId] = AttributeValue.StringValue("admin")
            }))
            .WithAttribute("orgMemberships", AttributeValue.MapValue(new Dictionary<string, AttributeValue>()));

    #endregion
}
