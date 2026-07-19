// ABOUTME: bUnit coverage for the anonymous Blog index and article reader pages.
// ABOUTME: Verifies public-state filtering, Markdown sanitation, thumbnails, metadata, and service calls.

using Explore.Blazor.Client.Helpers;
using Explore.Blazor.Client.Pages.Blogs;
using Explore.Blazor.Client.Services.Blogs;

namespace Explore.Blazor.Client.Tests.Pages.Blogs;

public sealed class BlogPublicPagesTests : IDisposable
{
    private readonly BlazorTestContext _ctx = new();
    private readonly IIslamuApiClient _apiClient = Substitute.For<IIslamuApiClient>();
    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();

    public BlogPublicPagesTests()
    {
        _ctx.SetAnonymousUser();
        _ctx.Services.AddSingleton(_imageStorageService);
        _ctx.Services.AddSingleton(new BlogService(_apiClient, Substitute.For<ILogger<BlogService>>()));
        _ctx.Services.AddSingleton(new BlogMarkdownRenderer());
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }

    [Test]
    public async Task BlogIndex_ShowsLoadingState_Initially()
    {
        var pending = new TaskCompletionSource<HalCollectionResourceOfBlogSummaryDto>();
        _apiClient.GetPublishedBlogsAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            categoryId: Arg.Any<Guid?>(),
            tagId: Arg.Any<Guid?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(_ => pending.Task);

        var cut = _ctx.RenderMudComponent<BlogIndex>();

        await Assert.That(cut.Markup).Contains("role=\"status\"");
        await Assert.That(cut.Markup).Contains("Loading published articles");
    }

    [Test]
    public async Task BlogIndex_RendersPublishedArticlesAndOmitsDraftFutureAndThumbnaillessPosts()
    {
        var thumbnailId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        _imageStorageService.GetPresignedUrlByIdAsync(thumbnailId).Returns("https://cdn.example/blog.jpg");
        _apiClient.GetPublishedBlogsAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            categoryId: Arg.Any<Guid?>(),
            tagId: Arg.Any<Guid?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(BlogCollection(
            Summary("Published & Ready", "published-ready", DateTime.UtcNow.AddDays(-2), thumbnailId),
            Summary("Draft Article", "draft-article", null),
            Summary("Future Article", "future-article", DateTime.UtcNow.AddDays(2))));

        var cut = _ctx.RenderMudComponent<BlogIndex>();
        cut.WaitForState(() => cut.Markup.Contains("Published &amp; Ready", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        await Assert.That(cut.FindAll("h1").Count).IsEqualTo(1);
        await Assert.That(cut.Markup).Contains("href=\"/blog/published-ready\"");
        await Assert.That(cut.Markup).Contains("alt=\"Thumbnail for Published &amp; Ready\"");
        await Assert.That(cut.Markup).DoesNotContain("Draft Article");
        await Assert.That(cut.Markup).DoesNotContain("Future Article");
    }

    [Test]
    public async Task BlogIndex_RendersEmptyState_WhenNoPublicArticlesExist()
    {
        _apiClient.GetPublishedBlogsAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            categoryId: Arg.Any<Guid?>(),
            tagId: Arg.Any<Guid?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(BlogCollection());

        var cut = _ctx.RenderMudComponent<BlogIndex>();
        cut.WaitForState(() => cut.Markup.Contains("No published articles", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        await Assert.That(cut.FindAll("h1").Count).IsEqualTo(1);
        await Assert.That(cut.Markup).Contains("No published articles are available yet.");
    }

    [Test]
    public async Task BlogIndex_SearchAndPagination_CallPublishedServiceWithExpectedArguments()
    {
        _apiClient.GetPublishedBlogsAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            categoryId: Arg.Any<Guid?>(),
            tagId: Arg.Any<Guid?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(
            BlogCollection(1, 10, 20, Summary("First", "first", DateTime.UtcNow.AddDays(-2))),
            BlogCollection(1, 10, 20, Summary("Search Hit", "search-hit", DateTime.UtcNow.AddDays(-2))),
            BlogCollection(2, 10, 20, Summary("Second", "second", DateTime.UtcNow.AddDays(-1))));

        var cut = _ctx.RenderMudComponent<BlogIndex>();
        cut.WaitForState(() => cut.Markup.Contains("First", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        cut.Find("#blog-search").Input("community");
        cut.Find("form.blog-index__search").Submit();
        cut.WaitForState(() => cut.Markup.Contains("Search Hit", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        cut.FindAll("button").Single(button => button.TextContent.Contains("Next", StringComparison.Ordinal)).Click();
        cut.WaitForState(() => cut.Markup.Contains("Second", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        await _apiClient.Received(1).GetPublishedBlogsAsync(
            pageNumber: 1,
            pageSize: 10,
            categoryId: null,
            tagId: null,
            search: "community",
            cancellationToken: Arg.Any<CancellationToken>());
        await _apiClient.Received(1).GetPublishedBlogsAsync(
            pageNumber: 2,
            pageSize: 10,
            categoryId: null,
            tagId: null,
            search: "community",
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task BlogArticle_RendersPublishedArticleThroughSanitizedMarkdown()
    {
        var thumbnailId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        _imageStorageService.GetPresignedUrlByIdAsync(thumbnailId).Returns("https://cdn.example/article.jpg");
        _apiClient.GetPublishedBlogBySlugAsync(
            slug: "safe-article",
            cancellationToken: Arg.Any<CancellationToken>()).Returns(Detail(
            "Safe Article",
            "safe-article",
            DateTime.UtcNow.AddDays(-1),
            "# Heading\n[good](https://example.com) [bad](javascript:alert(1)) <script>alert(1)</script>",
            thumbnailId));

        var cut = _ctx.RenderMudComponent<BlogArticle>(parameters => parameters.Add(x => x.Slug, "safe-article"));
        cut.WaitForState(() => cut.Markup.Contains("Safe Article", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        await Assert.That(cut.FindAll("h1").Count).IsEqualTo(1);
        await Assert.That(cut.Markup).Contains("href=\"https://example.com\"");
        await Assert.That(cut.Markup).DoesNotContain("javascript:");
        await Assert.That(cut.Markup).DoesNotContain("<script");
        await Assert.That(cut.Markup).Contains("alt=\"Featured image for Safe Article\"");
        await _apiClient.Received(1).GetPublishedBlogBySlugAsync(
            slug: "safe-article",
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task BlogArticle_RendersNotFound_ForMissingDraftOrFutureArticle()
    {
        _apiClient.GetPublishedBlogBySlugAsync(
            slug: "missing",
            cancellationToken: Arg.Any<CancellationToken>()).Returns((HalResourceOfBlogDetailDto?)null);
        _apiClient.GetPublishedBlogBySlugAsync(
            slug: "draft",
            cancellationToken: Arg.Any<CancellationToken>()).Returns(Detail("Draft", "draft", null, "# Draft"));
        _apiClient.GetPublishedBlogBySlugAsync(
            slug: "future",
            cancellationToken: Arg.Any<CancellationToken>()).Returns(Detail("Future", "future", DateTime.UtcNow.AddDays(2), "# Future"));

        var missing = _ctx.RenderMudComponent<BlogArticle>(parameters => parameters.Add(x => x.Slug, "missing"));
        missing.WaitForState(() => missing.Markup.Contains("Article not found", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        var draft = _ctx.RenderMudComponent<BlogArticle>(parameters => parameters.Add(x => x.Slug, "draft"));
        draft.WaitForState(() => draft.Markup.Contains("Article not found", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        var future = _ctx.RenderMudComponent<BlogArticle>(parameters => parameters.Add(x => x.Slug, "future"));
        future.WaitForState(() => future.Markup.Contains("Article not found", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        await Assert.That(missing.FindAll("h1").Count).IsEqualTo(1);
        await Assert.That(draft.Markup).DoesNotContain("# Draft");
        await Assert.That(future.Markup).DoesNotContain("# Future");
    }

    [Test]
    public async Task BlogPages_SourceIncludesCanonicalAndMetaDescription()
    {
        var index = await ReadClientSourceAsync("Pages/Blogs/BlogIndex.razor");
        var article = await ReadClientSourceAsync("Pages/Blogs/BlogArticle.razor");

        await Assert.That(index).Contains("rel=\"canonical\"");
        await Assert.That(index).Contains("CanonicalUrlHelper.Build");
        await Assert.That(index).Contains("name=\"description\"");
        await Assert.That(article).Contains("rel=\"canonical\"");
        await Assert.That(article).Contains("CanonicalUrlHelper.Build");
        await Assert.That(article).Contains("name=\"description\"");
    }

    private static HalCollectionResourceOfBlogSummaryDto BlogCollection(params HalResourceOfBlogSummaryDto[] items)
        => BlogCollection(1, 10, items.Length, items);

    private static HalCollectionResourceOfBlogSummaryDto BlogCollection(
        int pageNumber,
        int pageSize,
        int totalCount,
        params HalResourceOfBlogSummaryDto[] items)
        => new()
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            _embedded = new HalCollectionEmbeddedOfBlogSummaryDto { Items = items.ToList() }
        };

    private static HalResourceOfBlogSummaryDto Summary(string title, string slug, DateTime? publishedAtUtc, Guid? thumbnailId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Slug = slug,
            PublishedAtUtc = publishedAtUtc,
            ThumbnailStorageObjectId = thumbnailId
        };

    private static HalResourceOfBlogDetailDto Detail(
        string title,
        string slug,
        DateTime? publishedAtUtc,
        string contentMarkdown,
        Guid? thumbnailId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Slug = slug,
            PublishedAtUtc = publishedAtUtc,
            ContentMarkdown = contentMarkdown,
            ThumbnailStorageObjectId = thumbnailId
        };

    private static async Task<string> ReadClientSourceAsync(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Explore.Blazor.Client", relativePath);
            if (File.Exists(candidate))
                return await File.ReadAllTextAsync(candidate);

            directory = directory.Parent;
        }

        throw new InvalidOperationException($"Could not locate src/Explore.Blazor.Client/{relativePath} from test base directory.");
    }
}
