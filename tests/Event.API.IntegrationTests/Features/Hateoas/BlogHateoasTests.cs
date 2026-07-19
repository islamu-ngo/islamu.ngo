// ABOUTME: HATEOAS integration tests for Blog API collection and action links.
// ABOUTME: Verifies HAL shape, minimal preference handling, and permission-filtered editorial affordances.

using System.Net;
using System.Text.Json;
using Event.Api.IntegrationTests.Fixtures;
using Explore.API.Hateoas;
using Explore.API.Hateoas.Policies;
using Explore.Application.DTOs.Blog;
using Explore.Application.Hateoas;

namespace Event.Api.IntegrationTests.Features.Hateoas;

[NotInParallel("ApiTestFixture")]
[ClassDataSource<ApiTestFixture>(Shared = SharedType.PerAssembly)]
public sealed class BlogHateoasTests
{
    private const string BaseUrl = "/api/blog";
    private readonly ApiTestFixture _fixture;

    public BlogHateoasTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Test]
    public async Task BlogDetailLinkPolicy_ForDraft_ShouldHidePublicSelfAndExposeManagementSelfPath()
    {
        var blog = CreateDetail(published: false);
        var links = new BlogDetailLinkPolicy().GetLinks(blog, user: null).ToArray();

        var publicSelf = links.Single(link => link.Rel == LinkRelations.Self);
        var managementDetail = links.Single(link => link.Rel == "management-detail");
        var publish = links.Single(link => link.Rel == LinkRelations.Publish);
        var unpublish = links.Single(link => link.Rel == "unpublish");

        await Assert.That(publicSelf.RouteName).IsEqualTo(RouteNames.GetPublishedBlogBySlug);
        await Assert.That(publicSelf.Method).IsEqualTo("GET");
        await Assert.That(publicSelf.Condition is not null).IsTrue();
        await Assert.That(publicSelf.Condition!()).IsFalse();
        await Assert.That(managementDetail.RouteName).IsEqualTo(RouteNames.GetBlogByIdForManagement);
        await Assert.That(managementDetail.Method).IsEqualTo("GET");
        await Assert.That(managementDetail.RequiresAuth).IsTrue();
        await Assert.That(publish.RouteName).IsEqualTo(RouteNames.PublishBlog);
        await Assert.That(publish.Condition!()).IsTrue();
        await Assert.That(unpublish.RouteName).IsEqualTo(RouteNames.UnpublishBlog);
        await Assert.That(unpublish.Condition!()).IsFalse();
    }

    [Test]
    public async Task BlogDetailLinkPolicy_ForPublished_ShouldExposePublicSelfAndManagementDetail()
    {
        var blog = CreateDetail(published: true);
        var links = new BlogDetailLinkPolicy().GetLinks(blog, user: null).ToArray();

        var publicSelf = links.Single(link => link.Rel == LinkRelations.Self);
        var managementDetail = links.Single(link => link.Rel == "management-detail");
        var publish = links.Single(link => link.Rel == LinkRelations.Publish);
        var unpublish = links.Single(link => link.Rel == "unpublish");

        await Assert.That(publicSelf.RouteName).IsEqualTo(RouteNames.GetPublishedBlogBySlug);
        await Assert.That(publicSelf.Condition is not null).IsTrue();
        await Assert.That(publicSelf.Condition!()).IsTrue();
        await Assert.That(managementDetail.RouteName).IsEqualTo(RouteNames.GetBlogByIdForManagement);
        await Assert.That(managementDetail.RequiresAuth).IsTrue();
        await Assert.That(publish.Condition!()).IsFalse();
        await Assert.That(unpublish.Condition!()).IsTrue();
    }

    [Test]
    public async Task BlogCollectionLinkPolicy_ForDraftItem_ShouldHidePublicSelfAndExposeManagementSelfPath()
    {
        var blog = CreateSummary(published: false);
        var links = new BlogCollectionLinkPolicy(new StaticTenantContext(Guid.CreateVersion7()))
            .GetItemLinks(blog, user: null)
            .ToArray();

        var publicSelf = links.Single(link => link.Rel == LinkRelations.Self);
        var managementDetail = links.Single(link => link.Rel == "management-detail");
        var publish = links.Single(link => link.Rel == LinkRelations.Publish);
        var unpublish = links.Single(link => link.Rel == "unpublish");

        await Assert.That(publicSelf.RouteName).IsEqualTo(RouteNames.GetPublishedBlogBySlug);
        await Assert.That(publicSelf.Condition is not null).IsTrue();
        await Assert.That(publicSelf.Condition!()).IsFalse();
        await Assert.That(managementDetail.RouteName).IsEqualTo(RouteNames.GetBlogByIdForManagement);
        await Assert.That(managementDetail.Method).IsEqualTo("GET");
        await Assert.That(managementDetail.RequiresAuth).IsTrue();
        await Assert.That(publish.RouteName).IsEqualTo(RouteNames.PublishBlog);
        await Assert.That(publish.Condition!()).IsTrue();
        await Assert.That(unpublish.RouteName).IsEqualTo(RouteNames.UnpublishBlog);
        await Assert.That(unpublish.Condition!()).IsFalse();
    }

    [Test]
    public async Task BlogCollectionLinkPolicy_ForPublishedItem_ShouldExposePublicSelfAndManagementDetail()
    {
        var blog = CreateSummary(published: true);
        var links = new BlogCollectionLinkPolicy(new StaticTenantContext(Guid.CreateVersion7()))
            .GetItemLinks(blog, user: null)
            .ToArray();

        var publicSelf = links.Single(link => link.Rel == LinkRelations.Self);
        var managementDetail = links.Single(link => link.Rel == "management-detail");
        var publish = links.Single(link => link.Rel == LinkRelations.Publish);
        var unpublish = links.Single(link => link.Rel == "unpublish");

        await Assert.That(publicSelf.RouteName).IsEqualTo(RouteNames.GetPublishedBlogBySlug);
        await Assert.That(publicSelf.Condition is not null).IsTrue();
        await Assert.That(publicSelf.Condition!()).IsTrue();
        await Assert.That(managementDetail.RouteName).IsEqualTo(RouteNames.GetBlogByIdForManagement);
        await Assert.That(managementDetail.RequiresAuth).IsTrue();
        await Assert.That(publish.Condition!()).IsFalse();
        await Assert.That(unpublish.Condition!()).IsTrue();
    }

    [Test]
    public async Task GetPublishedBlogs_ShouldIncludeHalStructure()
    {
        var response = await _fixture.Client.GetAsync(BaseUrl);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        await Assert.That(json.RootElement.TryGetProperty("_links", out _)).IsTrue();
        await Assert.That(json.RootElement.TryGetProperty("_embedded", out _)).IsTrue();
        await Assert.That(json.RootElement.TryGetProperty("pageNumber", out _)).IsTrue();
        await Assert.That(json.RootElement.TryGetProperty("totalCount", out _)).IsTrue();
    }

    [Test]
    public async Task GetPublishedBlogs_WithPreferMinimal_ShouldExcludeItemLinks()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
        request.Headers.Add("Prefer", "return=minimal");

        var response = await _fixture.Client.SendAsync(request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.Headers.Contains("Preference-Applied")).IsTrue();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (TryGetFirstEmbeddedItem(json, out var firstItem))
        {
            await Assert.That(firstItem.TryGetProperty("_links", out _)).IsFalse();
        }
    }

    [Test]
    public async Task GetPublishedBlogs_ItemLinks_ShouldIncludeSelfLinkToSlug()
    {
        var response = await _fixture.Client.GetAsync(BaseUrl);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (TryGetFirstEmbeddedItem(json, out var firstItem) &&
            firstItem.TryGetProperty("_links", out var links) &&
            links.TryGetProperty("self", out var selfLink))
        {
            var href = selfLink.GetProperty("href").GetString();
            await Assert.That(href).Contains("/api/blog/");
        }
    }

    [Test]
    public async Task GetPublishedBlogs_WhenAuthorizationDenies_ShouldNotExposeCreateLink()
    {
        await using var factory = new AuthenticatedWebApplicationFactory
        {
            AuthorizationProviderOverride = new StubAuthorizationProvider { AllowAll = false }
        };
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
        request.Headers.Add(TestAuthHandler.AuthHeaderName, TestAuthHandler.CreateAuthHeaderValue(Guid.NewGuid()));

        var response = await client.SendAsync(request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var hasCreate = json.RootElement.TryGetProperty("_links", out var links) &&
            links.TryGetProperty("create", out _);
        await Assert.That(hasCreate).IsFalse();
    }

    [Test]
    public async Task GetPublishedBlogs_WhenAuthorizationAllows_ShouldExposeCreateLinkForAuthenticatedUser()
    {
        await using var factory = new AuthenticatedWebApplicationFactory
        {
            AuthorizationProviderOverride = new StubAuthorizationProvider { AllowAll = true }
        };
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
        request.Headers.Add(TestAuthHandler.AuthHeaderName, TestAuthHandler.CreateAuthHeaderValue(Guid.NewGuid()));

        var response = await client.SendAsync(request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var hasCreate = json.RootElement.TryGetProperty("_links", out var links) &&
            links.TryGetProperty("create", out _);
        await Assert.That(hasCreate).IsTrue();
    }

    [Test]
    public async Task BlogHalOpenApiCatalog_ContainsBlogDtoSchemas()
    {
        var apiAssembly = typeof(LinkRelations).Assembly;
        var catalogType = apiAssembly.GetType("Explore.API.OpenApi.HalOpenApiSchemaCatalog", throwOnError: true)!;

        var registered = (IReadOnlyList<Type>)catalogType.GetProperty("RegisteredDtoTypes")!.GetValue(null)!;
        await Assert.That(registered.Contains(typeof(BlogSummaryDto))).IsTrue();
        await Assert.That(registered.Contains(typeof(BlogDetailDto))).IsTrue();

        var detailMappings = (IReadOnlyDictionary<string, Type>)catalogType.GetProperty("DetailResourceMappings")!.GetValue(null)!;
        await Assert.That(detailMappings.ContainsKey("HalResourceOfBlogSummaryDto")).IsTrue();
        await Assert.That(detailMappings["HalResourceOfBlogSummaryDto"]).IsEqualTo(typeof(BlogSummaryDto));
        await Assert.That(detailMappings.ContainsKey("HalResourceOfBlogDetailDto")).IsTrue();
        await Assert.That(detailMappings["HalResourceOfBlogDetailDto"]).IsEqualTo(typeof(BlogDetailDto));

        var collectionMappings = (IReadOnlyDictionary<string, string>)catalogType.GetProperty("CollectionEmbeddedItemResourceMappings")!.GetValue(null)!;
        await Assert.That(collectionMappings.ContainsKey("HalCollectionEmbeddedOfBlogSummaryDto")).IsTrue();
        await Assert.That(collectionMappings["HalCollectionEmbeddedOfBlogSummaryDto"]).IsEqualTo("HalResourceOfBlogSummaryDto");
    }

    private static bool TryGetFirstEmbeddedItem(JsonDocument json, out JsonElement item)
    {
        item = default;
        if (!json.RootElement.TryGetProperty("_embedded", out var embedded) ||
            !embedded.TryGetProperty("items", out var items) ||
            items.GetArrayLength() == 0)
        {
            return false;
        }

        item = items[0];
        return true;
    }

    private static BlogDetailDto CreateDetail(bool published) => new()
    {
        Id = Guid.CreateVersion7(),
        TenantId = Guid.CreateVersion7(),
        Title = published ? "Published post" : "Draft post",
        Slug = published ? "published-post" : "draft-post",
        ContentMarkdown = "# Blog",
        PublishedAtUtc = published ? DateTime.UtcNow : null
    };

    private static BlogSummaryDto CreateSummary(bool published) => new()
    {
        Id = Guid.CreateVersion7(),
        TenantId = Guid.CreateVersion7(),
        Title = published ? "Published post" : "Draft post",
        Slug = published ? "published-post" : "draft-post",
        PublishedAtUtc = published ? DateTime.UtcNow : null
    };

    private sealed class StaticTenantContext(Guid tenantId) : Explore.Application.Contracts.Infrastructure.ITenantContext
    {
        public Guid TenantId { get; } = tenantId;
    }
}
