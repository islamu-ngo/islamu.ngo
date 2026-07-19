// ABOUTME: bUnit coverage for the authenticated Blog management page.
// ABOUTME: Verifies HAL-only affordances, management states, service routing, and accessibility announcements.

using Explore.Blazor.Client.Contracts.Services.Accessibility;
using Explore.Blazor.Client.Pages.Blogs;
using Explore.Blazor.Client.Services.Blogs;
using MudBlazor;

namespace Explore.Blazor.Client.Tests.Pages.Blogs;

public sealed class BlogManagementTests : IDisposable
{
    private readonly BlazorTestContext _ctx = new();
    private readonly IIslamuApiClient _apiClient = Substitute.For<IIslamuApiClient>();
    private readonly IDialogService _dialogService = Substitute.For<IDialogService>();
    private readonly IAccessibilityAnnouncerService _announcer = Substitute.For<IAccessibilityAnnouncerService>();

    public BlogManagementTests()
    {
        _ctx.SetAuthenticatedUser(Guid.NewGuid(), "Blog Editor");
        _ctx.Services.AddSingleton(_dialogService);
        _ctx.Services.AddSingleton(_announcer);
        _ctx.Services.AddSingleton(new BlogService(_apiClient, Substitute.For<ILogger<BlogService>>()));
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }

    [Test]
    public async Task BlogManagement_SourceIncludesRouteAuthorizeRenderModeAndNoIndexMetadata()
    {
        var source = await ReadClientSourceAsync("Pages/Blogs/BlogManagement.razor");

        await Assert.That(source).Contains("@page \"/blog/manage\"");
        await Assert.That(source).Contains("@attribute [Authorize]");
        await Assert.That(source).Contains("@rendermode InteractiveServer");
        await Assert.That(source).Contains("<PageTitle>Manage Blog | ISLAMU</PageTitle>");
        await Assert.That(source).Contains("name=\"robots\"");
        await Assert.That(source).DoesNotContain("User.IsInRole");
        await Assert.That(source).DoesNotContain("ClaimsPrincipal");
    }

    [Test]
    public async Task BlogManagement_RendersExactlyOneH1()
    {
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(BlogCollection(Link("create"), Summary("Draft", null, Link("edit"))));

        var cut = _ctx.RenderMudComponent<BlogManagement>();
        cut.WaitForState(() => cut.Markup.Contains("Draft", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        await Assert.That(cut.FindAll("h1").Count).IsEqualTo(1);
    }

    [Test]
    public async Task BlogManagement_CreateAction_FollowsCollectionCreateLinkOnly()
    {
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(
            BlogCollection(Link("create"), Summary("Editable", null, Link("edit"))),
            BlogCollection(Summary("Editable", null, Link("edit"))));

        var withCreate = _ctx.RenderMudComponent<BlogManagement>();
        withCreate.WaitForState(() => withCreate.Markup.Contains("New article", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        await Assert.That(withCreate.Markup).Contains("New article");

        var withoutCreate = _ctx.RenderMudComponent<BlogManagement>();
        withoutCreate.WaitForState(() => withoutCreate.Markup.Contains("Editable", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        await Assert.That(withoutCreate.Markup).DoesNotContain("New article");
    }

    [Test]
    public async Task BlogManagement_ItemActions_FollowItemLinksRatherThanPublicationState()
    {
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(BlogCollection(
            Summary("Published With Draft Action", DateTime.UtcNow.AddDays(-1), Link("publish")),
            Summary("Draft With Unpublish Action", null, Link("unpublish")),
            Summary("Linked Admin Article", DateTime.UtcNow.AddDays(-2), Link("edit"), Link("delete"), Link("self", "/blog/linked-admin-article")),
            Summary("Unlinked Draft", null)));

        var cut = _ctx.RenderMudComponent<BlogManagement>();
        cut.WaitForState(() => cut.Markup.Contains("Linked Admin Article", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        await Assert.That(cut.Markup).Contains("Published With Draft Action");
        await Assert.That(cut.Markup).Contains("Publish");
        await Assert.That(cut.Markup).Contains("Draft With Unpublish Action");
        await Assert.That(cut.Markup).Contains("Unpublish");
        await Assert.That(cut.Markup).Contains("Edit");
        await Assert.That(cut.Markup).Contains("Delete");
        await Assert.That(cut.Markup).Contains("href=\"/blog/linked-admin-article\"");
        await Assert.That(cut.Markup).DoesNotContain("href=\"/blog/unlinked-draft\"");
    }

    [Test]
    public async Task BlogManagement_ShowsLoadingEmptyAndErrorStates()
    {
        var pending = new TaskCompletionSource<HalCollectionResourceOfBlogSummaryDto>();
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(_ => pending.Task);

        var loading = _ctx.RenderMudComponent<BlogManagement>();
        await Assert.That(loading.Markup).Contains("role=\"status\"");
        await Assert.That(loading.Markup).Contains("Loading managed articles");

        pending.SetResult(BlogCollection());
        loading.WaitForState(() => loading.Markup.Contains("No managed articles", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        await Assert.That(loading.Markup).Contains("No managed articles match this view.");

        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("boom"));

        var error = _ctx.RenderMudComponent<BlogManagement>();
        error.WaitForState(() => error.Markup.Contains("role=\"alert\"", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        await Assert.That(error.Markup).Contains("Managed articles could not be loaded.");
        await _announcer.Received().AnnounceAssertiveAsync("Managed articles could not be loaded. Please try again later.");
    }

    [Test]
    public async Task BlogManagement_SearchAndPagination_CallManagementServiceWithExpectedArguments()
    {
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(
            BlogCollection(1, 10, 20, Summary("First", null)),
            BlogCollection(1, 10, 20, Summary("Search Hit", null)),
            BlogCollection(2, 10, 20, Summary("Second", null)));

        var cut = _ctx.RenderMudComponent<BlogManagement>();
        cut.WaitForState(() => cut.Markup.Contains("First", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        cut.Find("#blog-management-search").Input("governance");
        cut.Find("form.blog-management__search").Submit();
        cut.WaitForState(() => cut.Markup.Contains("Search Hit", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        cut.FindAll("button").Single(button => button.TextContent.Contains("Next", StringComparison.Ordinal)).Click();
        cut.WaitForState(() => cut.Markup.Contains("Second", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        await _apiClient.Received(1).GetBlogsForManagementAsync(
            pageNumber: 1,
            pageSize: 10,
            search: "governance",
            cancellationToken: Arg.Any<CancellationToken>());
        await _apiClient.Received(1).GetBlogsForManagementAsync(
            pageNumber: 2,
            pageSize: 10,
            search: "governance",
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task BlogManagement_PublishUnpublishAndDelete_RecheckLinksRouteServicesAndAnnounce()
    {
        var publishId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var unpublishId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var deleteId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        _apiClient.PublishBlogAsync(publishId, cancellationToken: Arg.Any<CancellationToken>()).Returns(Success(publishId));
        _apiClient.UnpublishBlogAsync(unpublishId, cancellationToken: Arg.Any<CancellationToken>()).Returns(Success(unpublishId));
        _apiClient.DeleteBlogAsync(deleteId, cancellationToken: Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _dialogService.ShowMessageBoxAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<DialogOptions>()).Returns(true);

        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(
            BlogCollection(Summary("Publish Me", null, publishId, Link("publish"))),
            BlogCollection());
        var cut = _ctx.RenderMudComponent<BlogManagement>();
        cut.WaitForState(() => cut.Markup.Contains("Publish Me", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        cut.FindAll("button").Single(button => button.TextContent.Contains("Publish", StringComparison.Ordinal)).Click();
        cut.WaitForAssertion(() => _apiClient.Received(1).PublishBlogAsync(publishId, cancellationToken: Arg.Any<CancellationToken>()), TimeSpan.FromSeconds(2));
        cut.WaitForAssertion(() => _announcer.Received(1).AnnouncePoliteAsync("Article published."), TimeSpan.FromSeconds(2));

        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(
            BlogCollection(Summary("Unpublish Me", DateTime.UtcNow.AddDays(-1), unpublishId, Link("unpublish"))),
            BlogCollection());
        cut = _ctx.RenderMudComponent<BlogManagement>();
        cut.WaitForState(() => cut.Markup.Contains("Unpublish Me", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        cut.FindAll("button").Single(button => button.TextContent.Contains("Unpublish", StringComparison.Ordinal)).Click();
        cut.WaitForAssertion(() => _apiClient.Received(1).UnpublishBlogAsync(unpublishId, cancellationToken: Arg.Any<CancellationToken>()), TimeSpan.FromSeconds(2));
        cut.WaitForAssertion(() => _announcer.Received(1).AnnouncePoliteAsync("Article unpublished."), TimeSpan.FromSeconds(2));

        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(
            BlogCollection(Summary("Delete Me", null, deleteId, Link("delete"))),
            BlogCollection());
        cut = _ctx.RenderMudComponent<BlogManagement>();
        cut.WaitForState(() => cut.Markup.Contains("Delete Me", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        cut.FindAll("button").Single(button => button.TextContent.Contains("Delete", StringComparison.Ordinal)).Click();
        cut.WaitForAssertion(() => _dialogService.Received(1).ShowMessageBoxAsync(
            "Delete article",
            "Delete 'Delete Me'? This cannot be undone.",
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<DialogOptions>()), TimeSpan.FromSeconds(2));
        cut.WaitForAssertion(() => _apiClient.Received(1).DeleteBlogAsync(deleteId, cancellationToken: Arg.Any<CancellationToken>()), TimeSpan.FromSeconds(2));
        await _announcer.Received(1).AnnouncePoliteAsync("Article deleted.");
    }

    private static HalCollectionResourceOfBlogSummaryDto BlogCollection(params object[] entries)
        => BlogCollection(1, 10, entries.OfType<HalResourceOfBlogSummaryDto>().Count(), entries);

    private static HalCollectionResourceOfBlogSummaryDto BlogCollection(
        int pageNumber,
        int pageSize,
        int totalCount,
        params object[] entries)
    {
        var links = entries.OfType<KeyValuePair<string, HalLink>>().ToDictionary(pair => pair.Key, pair => pair.Value);
        var items = entries.OfType<HalResourceOfBlogSummaryDto>().ToList();

        return new()
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            _links = links,
            _embedded = new HalCollectionEmbeddedOfBlogSummaryDto { Items = items }
        };
    }

    private static HalResourceOfBlogSummaryDto Summary(string title, DateTime? publishedAtUtc, params KeyValuePair<string, HalLink>[] links)
        => Summary(title, publishedAtUtc, Guid.NewGuid(), links);

    private static HalResourceOfBlogSummaryDto Summary(string title, DateTime? publishedAtUtc, Guid id, params KeyValuePair<string, HalLink>[] links)
        => new()
        {
            Id = id,
            Title = title,
            Slug = title.ToLowerInvariant().Replace(' ', '-'),
            PublishedAtUtc = publishedAtUtc,
            _links = links.ToDictionary(pair => pair.Key, pair => pair.Value)
        };

    private static KeyValuePair<string, HalLink> Link(string rel, string? href = null)
        => new(rel, new HalLink { Href = href ?? $"/api/blog/{rel}" });

    private static BaseCommandResponseOfGuid Success(Guid id)
        => new() { Success = true, Id = id, Message = "Success" };

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
