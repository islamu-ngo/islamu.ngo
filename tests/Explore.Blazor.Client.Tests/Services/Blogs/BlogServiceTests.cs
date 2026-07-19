// ABOUTME: Unit tests for BlogService covering all generated-client routing,
// ABOUTME: cancellation, safe fallbacks, typed HAL extraction, and mapping.

using Explore.Blazor.Client.Helpers;
using Explore.Blazor.Client.Services.Blogs;

namespace Explore.Blazor.Client.Tests.Services.Blogs;

public class BlogServiceTests
{
    private readonly IIslamuApiClient _apiClient;
    private readonly ILogger<BlogService> _logger;
    private readonly BlogService _service;

    public BlogServiceTests()
    {
        _apiClient = Substitute.For<IIslamuApiClient>();
        _logger = Substitute.For<ILogger<BlogService>>();
        _service = new BlogService(_apiClient, _logger);
    }

    // ========== GetPublishedBlogsAsync ==========

    [Test]
    public async Task GetPublishedBlogsAsync_RoutesToGeneratedMethod_WithCancellation()
    {
        var cts = new CancellationTokenSource();
        var hal = new HalCollectionResourceOfBlogSummaryDto();
        _apiClient.GetPublishedBlogsAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            categoryId: Arg.Any<Guid?>(),
            tagId: Arg.Any<Guid?>(),
            search: Arg.Any<string?>(),
            cancellationToken: cts.Token).Returns(hal);

        await _service.GetPublishedBlogsAsync(cancellationToken: cts.Token);

        await _apiClient.Received(1).GetPublishedBlogsAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            categoryId: Arg.Any<Guid?>(),
            tagId: Arg.Any<Guid?>(),
            search: Arg.Any<string?>(),
            cancellationToken: cts.Token);
    }

    [Test]
    public async Task GetPublishedBlogsAsync_ReturnsEmptyCollection_OnApiFailure()
    {
        var ex = new ApiException("Server Error", 500, null, null, null);
        _apiClient.GetPublishedBlogsAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            categoryId: Arg.Any<Guid?>(),
            tagId: Arg.Any<Guid?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).ThrowsAsync(ex);

        var result = await _service.GetPublishedBlogsAsync();

        await Assert.That(result).IsNotNull();
        await Assert.That(result._embedded?.Items).IsNull();
    }

    // ========== GetPublishedBlogBySlugAsync ==========

    [Test]
    public async Task GetPublishedBlogBySlugAsync_RoutesToGeneratedMethod_WithCancellation()
    {
        var cts = new CancellationTokenSource();
        var hal = new HalResourceOfBlogDetailDto { Id = Guid.NewGuid(), Title = "Test" };
        _apiClient.GetPublishedBlogBySlugAsync(
            slug: Arg.Any<string>(),
            cancellationToken: cts.Token).Returns(hal);

        await _service.GetPublishedBlogBySlugAsync("test-slug", cts.Token);

        await _apiClient.Received(1).GetPublishedBlogBySlugAsync(
            slug: "test-slug",
            cancellationToken: cts.Token);
    }

    [Test]
    public async Task GetPublishedBlogBySlugAsync_ReturnsMappedDto_OnSuccess()
    {
        var id = Guid.NewGuid();
        var hal = new HalResourceOfBlogDetailDto { Id = id, Title = "Mapped", ContentMarkdown = "# Hello" };
        _apiClient.GetPublishedBlogBySlugAsync(
            slug: Arg.Any<string>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(hal);

        var result = await _service.GetPublishedBlogBySlugAsync("mapped-slug");

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Id).IsEqualTo(id);
    }

    [Test]
    public async Task GetPublishedBlogBySlugAsync_ReturnsNull_On404()
    {
        var ex = new ApiException("Not Found", 404, null, null, null);
        _apiClient.GetPublishedBlogBySlugAsync(
            slug: Arg.Any<string>(),
            cancellationToken: Arg.Any<CancellationToken>()).ThrowsAsync(ex);

        var result = await _service.GetPublishedBlogBySlugAsync("missing");

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GetPublishedBlogBySlugAsync_ReturnsNull_OnApiError()
    {
        var ex = new ApiException("Server Error", 500, null, null, null);
        _apiClient.GetPublishedBlogBySlugAsync(
            slug: Arg.Any<string>(),
            cancellationToken: Arg.Any<CancellationToken>()).ThrowsAsync(ex);

        var result = await _service.GetPublishedBlogBySlugAsync("error");

        await Assert.That(result).IsNull();
    }

    // ========== GetBlogsForManagementAsync ==========

    [Test]
    public async Task GetBlogsForManagementAsync_RoutesToGeneratedMethod_WithCancellation()
    {
        var cts = new CancellationTokenSource();
        var hal = new HalCollectionResourceOfBlogSummaryDto();
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: cts.Token).Returns(hal);

        await _service.GetBlogsForManagementAsync(cancellationToken: cts.Token);

        await _apiClient.Received(1).GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: cts.Token);
    }

    [Test]
    public async Task GetBlogsForManagementAsync_ReturnsEmptyCollection_OnApiFailure()
    {
        var ex = new ApiException("Server Error", 500, null, null, null);
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).ThrowsAsync(ex);

        var result = await _service.GetBlogsForManagementAsync();

        await Assert.That(result).IsNotNull();
        await Assert.That(result._embedded?.Items).IsNull();
    }

    // ========== GetBlogByIdForManagementAsync ==========

    [Test]
    public async Task GetBlogByIdForManagementAsync_RoutesToGeneratedMethod_WithCancellation()
    {
        var cts = new CancellationTokenSource();
        var hal = new HalResourceOfBlogDetailDto { Id = Guid.NewGuid(), Title = "Mgmt" };
        _apiClient.GetBlogByIdForManagementAsync(
            id: Arg.Any<Guid>(),
            cancellationToken: cts.Token).Returns(hal);

        await _service.GetBlogByIdForManagementAsync(Guid.NewGuid(), cts.Token);

        await _apiClient.Received(1).GetBlogByIdForManagementAsync(
            id: Arg.Any<Guid>(),
            cancellationToken: cts.Token);
    }

    [Test]
    public async Task GetBlogByIdForManagementAsync_PreservesHalLinks_OnSuccess()
    {
        var id = Guid.NewGuid();
        var hal = new HalResourceOfBlogDetailDto
        {
            Id = id,
            Title = "Mgmt Detail",
            _links = new Dictionary<string, HalLink>
            {
                { "edit", new HalLink { Href = $"/api/management/blogs/{id}" } },
                { "publish", new HalLink { Href = $"/api/management/blogs/{id}/publish" } },
                { "delete", new HalLink { Href = $"/api/management/blogs/{id}" } }
            }
        };
        _apiClient.GetBlogByIdForManagementAsync(
            id: Arg.Any<Guid>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(hal);

        var result = await _service.GetBlogByIdForManagementAsync(id);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Id).IsEqualTo(id);
        await Assert.That(result.HasLink("edit")).IsTrue();
        await Assert.That(result.HasLink("publish")).IsTrue();
        await Assert.That(result.HasLink("delete")).IsTrue();
    }

    [Test]
    public async Task GetBlogByIdForManagementAsync_ReturnsNull_On404()
    {
        var ex = new ApiException("Not Found", 404, null, null, null);
        _apiClient.GetBlogByIdForManagementAsync(
            id: Arg.Any<Guid>(),
            cancellationToken: Arg.Any<CancellationToken>()).ThrowsAsync(ex);

        var result = await _service.GetBlogByIdForManagementAsync(Guid.NewGuid());

        await Assert.That(result).IsNull();
    }

    // ========== CreateBlogAsync ==========

    [Test]
    public async Task CreateBlogAsync_RoutesToGeneratedMethod_WithCancellation()
    {
        var cts = new CancellationTokenSource();
        var dto = new CreateBlogDto { Title = "New" };
        var response = ComponentDataBuilder.SuccessResponse();
        _apiClient.CreateBlogAsync(
            body: Arg.Any<CreateBlogDto>(),
            cancellationToken: cts.Token).Returns(response);

        await _service.CreateBlogAsync(dto, cts.Token);

        await _apiClient.Received(1).CreateBlogAsync(
            body: dto,
            cancellationToken: cts.Token);
    }

    [Test]
    public async Task CreateBlogAsync_ReturnsFailedResponse_OnApiError()
    {
        var dto = new CreateBlogDto { Title = "New" };
        var ex = new ApiException("Bad Request", 400, "validation error", null, null);
        _apiClient.CreateBlogAsync(
            body: Arg.Any<CreateBlogDto>(),
            cancellationToken: Arg.Any<CancellationToken>()).ThrowsAsync(ex);

        var result = await _service.CreateBlogAsync(dto);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Success).IsFalse();
    }

    // ========== UpdateBlogAsync ==========

    [Test]
    public async Task UpdateBlogAsync_RoutesToGeneratedMethod_WithCancellation()
    {
        var cts = new CancellationTokenSource();
        var id = Guid.NewGuid();
        var dto = new UpdateBlogDto { Title = "Updated" };
        var response = ComponentDataBuilder.SuccessResponse(id);
        _apiClient.UpdateBlogAsync(
            id: id,
            body: Arg.Any<UpdateBlogDto>(),
            cancellationToken: cts.Token).Returns(response);

        await _service.UpdateBlogAsync(id, dto, cts.Token);

        await _apiClient.Received(1).UpdateBlogAsync(
            id: id,
            body: dto,
            cancellationToken: cts.Token);
    }

    [Test]
    public async Task UpdateBlogAsync_ReturnsFailedResponse_OnApiError()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateBlogDto { Title = "Updated" };
        var ex = new ApiException("Bad Request", 400, "validation error", null, null);
        _apiClient.UpdateBlogAsync(
            id: Arg.Any<Guid>(),
            body: Arg.Any<UpdateBlogDto>(),
            cancellationToken: Arg.Any<CancellationToken>()).ThrowsAsync(ex);

        var result = await _service.UpdateBlogAsync(id, dto);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Success).IsFalse();
    }

    // ========== PublishBlogAsync ==========

    [Test]
    public async Task PublishBlogAsync_RoutesToGeneratedMethod_WithCancellation()
    {
        var cts = new CancellationTokenSource();
        var id = Guid.NewGuid();
        var response = ComponentDataBuilder.SuccessResponse(id);
        _apiClient.PublishBlogAsync(
            id: id,
            cancellationToken: cts.Token).Returns(response);

        await _service.PublishBlogAsync(id, cts.Token);

        await _apiClient.Received(1).PublishBlogAsync(
            id: id,
            cancellationToken: cts.Token);
    }

    [Test]
    public async Task PublishBlogAsync_ReturnsFailedResponse_OnApiError()
    {
        var id = Guid.NewGuid();
        var ex = new ApiException("Forbidden", 403, null, null, null);
        _apiClient.PublishBlogAsync(
            id: Arg.Any<Guid>(),
            cancellationToken: Arg.Any<CancellationToken>()).ThrowsAsync(ex);

        var result = await _service.PublishBlogAsync(id);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Success).IsFalse();
    }

    // ========== UnpublishBlogAsync ==========

    [Test]
    public async Task UnpublishBlogAsync_RoutesToGeneratedMethod_WithCancellation()
    {
        var cts = new CancellationTokenSource();
        var id = Guid.NewGuid();
        var response = ComponentDataBuilder.SuccessResponse(id);
        _apiClient.UnpublishBlogAsync(
            id: id,
            cancellationToken: cts.Token).Returns(response);

        await _service.UnpublishBlogAsync(id, cts.Token);

        await _apiClient.Received(1).UnpublishBlogAsync(
            id: id,
            cancellationToken: cts.Token);
    }

    [Test]
    public async Task UnpublishBlogAsync_ReturnsFailedResponse_OnApiError()
    {
        var id = Guid.NewGuid();
        var ex = new ApiException("Forbidden", 403, null, null, null);
        _apiClient.UnpublishBlogAsync(
            id: Arg.Any<Guid>(),
            cancellationToken: Arg.Any<CancellationToken>()).ThrowsAsync(ex);

        var result = await _service.UnpublishBlogAsync(id);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Success).IsFalse();
    }

    // ========== DeleteBlogAsync ==========

    [Test]
    public async Task DeleteBlogAsync_RoutesToGeneratedMethod_WithCancellation()
    {
        var cts = new CancellationTokenSource();
        var id = Guid.NewGuid();
        _apiClient.DeleteBlogAsync(
            id: id,
            cancellationToken: cts.Token).Returns(Task.CompletedTask);

        await _service.DeleteBlogAsync(id, cts.Token);

        await _apiClient.Received(1).DeleteBlogAsync(
            id: id,
            cancellationToken: cts.Token);
    }

    [Test]
    public async Task DeleteBlogAsync_ReturnsTrue_OnSuccess()
    {
        var id = Guid.NewGuid();
        _apiClient.DeleteBlogAsync(
            id: Arg.Any<Guid>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await _service.DeleteBlogAsync(id);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task DeleteBlogAsync_ReturnsFalse_OnApiError()
    {
        var id = Guid.NewGuid();
        var ex = new ApiException("Forbidden", 403, null, null, null);
        _apiClient.DeleteBlogAsync(
            id: Arg.Any<Guid>(),
            cancellationToken: Arg.Any<CancellationToken>()).ThrowsAsync(ex);

        var result = await _service.DeleteBlogAsync(id);

        await Assert.That(result).IsFalse();
    }

    // ========== HAL Extension Tests ==========

    [Test]
    public async Task HalCollection_GetItems_ReturnsTypedBlogSummaryDtos()
    {
        var item1 = new BlogSummaryDto { Id = Guid.NewGuid(), Title = "One" };
        var item2 = new BlogSummaryDto { Id = Guid.NewGuid(), Title = "Two" };
        var halItems = new List<HalResourceOfBlogSummaryDto>
        {
            ToHalResource(item1),
            ToHalResource(item2)
        };
        var collection = new HalCollectionResourceOfBlogSummaryDto
        {
            _embedded = new HalCollectionEmbeddedOfBlogSummaryDto { Items = halItems }
        };

        var result = collection.GetItems();

        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result.First().Title).IsEqualTo("One");
    }

    [Test]
    public async Task HalCollection_GetHalItems_PreservesPerItemLinks()
    {
        var item = new HalResourceOfBlogSummaryDto
        {
            Id = Guid.NewGuid(),
            Title = "Managed",
            _links = new Dictionary<string, HalLink>
            {
                { "edit", new HalLink { Href = "/edit" } },
                { "delete", new HalLink { Href = "/delete" } }
            }
        };
        var collection = new HalCollectionResourceOfBlogSummaryDto
        {
            _embedded = new HalCollectionEmbeddedOfBlogSummaryDto { Items = new List<HalResourceOfBlogSummaryDto> { item } }
        };

        var result = collection.GetHalItems();

        await Assert.That(result.Count).IsEqualTo(1);
        await Assert.That(result.First().HasLink("edit")).IsTrue();
        await Assert.That(result.First().HasLink("delete")).IsTrue();
    }

    [Test]
    public async Task HalCollection_GetItems_ReturnsEmpty_WhenItemsNull()
    {
        var collection = new HalCollectionResourceOfBlogSummaryDto();

        var result = collection.GetItems();

        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task HalDetail_ToDto_MapsCorrectly()
    {
        var id = Guid.NewGuid();
        var hal = new HalResourceOfBlogDetailDto
        {
            Id = id,
            Title = "Detail",
            ContentMarkdown = "# Markdown"
        };

        var result = hal.ToDto();

        await Assert.That(result.Id).IsEqualTo(id);
        await Assert.That(result.Title).IsEqualTo("Detail");
        await Assert.That(result.ContentMarkdown).IsEqualTo("# Markdown");
    }

    [Test]
    public async Task HalCollection_HasLink_ChecksLinks()
    {
        var collection = new HalCollectionResourceOfBlogSummaryDto();
        collection._links = new Dictionary<string, HalLink> { { "next", new HalLink { Href = "/next" } } };

        await Assert.That(collection.HasLink("next")).IsTrue();
        await Assert.That(collection.HasLink("prev")).IsFalse();
    }

    [Test]
    public async Task HalSummaryItem_HasLink_ChecksLinks()
    {
        var item = new HalResourceOfBlogSummaryDto();
        item._links = new Dictionary<string, HalLink> { { "self", new HalLink { Href = "/self" } } };

        await Assert.That(item.HasLink("self")).IsTrue();
        await Assert.That(item.HasLink("edit")).IsFalse();
    }

    [Test]
    public async Task HalDetail_HasLink_ChecksLinks()
    {
        var detail = new HalResourceOfBlogDetailDto();
        detail._links = new Dictionary<string, HalLink> { { "edit", new HalLink { Href = "/edit" } } };

        await Assert.That(detail.HasLink("edit")).IsTrue();
        await Assert.That(detail.HasLink("delete")).IsFalse();
    }

    private static HalResourceOfBlogSummaryDto ToHalResource(BlogSummaryDto item)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(item);
        return System.Text.Json.JsonSerializer.Deserialize<HalResourceOfBlogSummaryDto>(json)
               ?? new HalResourceOfBlogSummaryDto();
    }
}
