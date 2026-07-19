// ABOUTME: bUnit coverage for the authenticated Blog editor create/edit surface.
// ABOUTME: Verifies HAL-gated forms/actions, DTO mapping, sanitized preview, and accessibility states.

using Explore.Blazor.Client.Contracts.Services.Accessibility;
using Explore.Blazor.Client.Contracts.Services.Blogs;
using Explore.Blazor.Client.Pages.Blogs;
using Explore.Blazor.Client.Services.Blogs;
using Explore.Blazor.Client.Shared;
using MudBlazor;

namespace Explore.Blazor.Client.Tests.Pages.Blogs;

public sealed class BlogEditorTests : IDisposable
{
    private static readonly Guid InitialConcurrencyStamp = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid UpdatedConcurrencyStamp = Guid.Parse("10000000-0000-0000-0000-000000000002");

    private readonly BlazorTestContext _ctx = new();
    private readonly IIslamuApiClient _apiClient = Substitute.For<IIslamuApiClient>();
    private readonly ICategoryService _categoryService = Substitute.For<ICategoryService>();
    private readonly ITagService _tagService = Substitute.For<ITagService>();
    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();
    private readonly IDialogService _dialogService = Substitute.For<IDialogService>();
    private readonly IAccessibilityAnnouncerService _announcer = Substitute.For<IAccessibilityAnnouncerService>();

    public BlogEditorTests()
    {
        _ctx.SetAuthenticatedUser(Guid.NewGuid(), "Blog Editor");
        _categoryService.GetAllCategoriesAsync().Returns(new List<CategoryListDto>());
        _tagService.GetAllTagsAsync().Returns(new List<TagListDto>());
        _ctx.Services.AddSingleton(_categoryService);
        _ctx.Services.AddSingleton(_tagService);
        _ctx.Services.AddSingleton(_imageStorageService);
        _ctx.Services.AddSingleton(_dialogService);
        _ctx.Services.AddSingleton(_announcer);
        _ctx.Services.AddSingleton<IBlogMarkdownRenderer>(new BlogMarkdownRenderer());
        _ctx.Services.AddSingleton<IBlogService>(new BlogService(_apiClient, Substitute.For<ILogger<BlogService>>()));
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }

    [Test]
    public async Task BlogEditor_SourceIncludesRoutesAuthorizeRenderModeNoIndexAndNoLocalClaimsAuthority()
    {
        var source = await ReadClientSourceAsync("Pages/Blogs/BlogEditor.razor");

        await Assert.That(source).Contains("@page \"/blog/manage/new\"");
        await Assert.That(source).Contains("@page \"/blog/manage/{Id:guid}\"");
        await Assert.That(source).Contains("@attribute [Authorize]");
        await Assert.That(source).Contains("@rendermode InteractiveServer");
        await Assert.That(source).Contains("name=\"robots\"");
        await Assert.That(source).DoesNotContain("User.IsInRole");
        await Assert.That(source).DoesNotContain("ClaimsPrincipal");
    }

    [Test]
    public async Task BlogEditor_SourceKeepsBlogValidatorsAlignedWithApiContracts()
    {
        var source = await ReadClientSourceAsync("Pages/Blogs/BlogEditor.razor.cs");

        await Assert.That(source).Contains("[MaxLength(300)]");
        await Assert.That(source).Contains("[MaxLength(200)]");
        await Assert.That(source).Contains("ExpectedConcurrencyStamp = _model.ConcurrencyStamp");
        await Assert.That(source).Contains("_model.ThumbnailStorageObjectId = upload.StorageObjectId;");
        await Assert.That(source).Contains("_pendingThumbnail = null;");
    }

    [Test]
    public async Task BlogEditor_RendersExactlyOneH1()
    {
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(BlogCollection(Link("create")));

        var cut = _ctx.RenderMudComponent<BlogEditor>();
        cut.WaitForState(() => cut.Markup.Contains("Create article", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        await Assert.That(cut.FindAll("h1").Count).IsEqualTo(1);
    }

    [Test]
    public async Task BlogEditor_NewRoute_ShowsNoFormWithoutCreateHalAndShowsFormWithCreateHal()
    {
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(BlogCollection());

        var denied = _ctx.RenderMudComponent<BlogEditor>();
        denied.WaitForState(() => denied.Markup.Contains("Article creation is unavailable", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        await Assert.That(denied.FindAll("form.blog-editor__form").Count).IsEqualTo(0);

        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(BlogCollection(Link("create")));

        var permitted = _ctx.RenderMudComponent<BlogEditor>();
        permitted.WaitForState(() => permitted.Markup.Contains("Create article", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        await Assert.That(permitted.FindAll("form.blog-editor__form").Count).IsEqualTo(1);
    }

    [Test]
    public async Task BlogEditor_EditRoute_ShowsNoFormWithoutEditHal()
    {
        var id = Guid.NewGuid();
        _apiClient.GetBlogByIdForManagementAsync(id, cancellationToken: Arg.Any<CancellationToken>()).Returns(Detail(id, "Locked Article"));

        var cut = _ctx.RenderMudComponent<BlogEditor>(parameters => parameters.Add(p => p.Id, id));
        cut.WaitForState(() => cut.Markup.Contains("not available for editing", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        await Assert.That(cut.FindAll("form.blog-editor__form").Count).IsEqualTo(0);
    }

    [Test]
    public async Task BlogEditor_EditRoute_MissingConcurrencyStampFailsClosed()
    {
        var id = Guid.NewGuid();
        var resource = Detail(id, "Missing Stamp", links: Link("edit"));
        resource.ConcurrencyStamp = null;
        _apiClient.GetBlogByIdForManagementAsync(id, cancellationToken: Arg.Any<CancellationToken>()).Returns(resource);

        var cut = _ctx.RenderMudComponent<BlogEditor>(parameters => parameters.Add(p => p.Id, id));
        cut.WaitForState(() => cut.Markup.Contains("not available for editing", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        await Assert.That(cut.FindAll("form.blog-editor__form").Count).IsEqualTo(0);
        await _apiClient.DidNotReceive().UpdateBlogAsync(id, Arg.Any<UpdateBlogDto>(), cancellationToken: Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task BlogEditor_EditRoute_InitializesFormFromResourceTaxonomyAndThumbnail()
    {
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var thumbnailId = Guid.NewGuid();
        _categoryService.GetAllCategoriesAsync().Returns(new List<CategoryListDto>
        {
            new() { Id = categoryId, FullName = "Community", MasterCode = "COMMUNITY" }
        });
        _tagService.GetAllTagsAsync().Returns(new List<TagListDto>
        {
            new() { Id = tagId, FullName = "Ramadan", MasterCode = "RAMADAN" }
        });
        _apiClient.GetBlogByIdForManagementAsync(id, cancellationToken: Arg.Any<CancellationToken>()).Returns(Detail(
            id,
            "Existing Article",
            slug: "existing-article",
            content: "# Existing",
            categoryIds: [categoryId],
            tagIds: [tagId],
            thumbnailStorageObjectId: thumbnailId,
            concurrencyStamp: InitialConcurrencyStamp,
            links: Link("edit")));

        var cut = _ctx.RenderMudComponent<BlogEditor>(parameters => parameters.Add(p => p.Id, id));
        cut.WaitForState(() => cut.Markup.Contains("Existing Article", StringComparison.Ordinal), TimeSpan.FromSeconds(2));

        await Assert.That(cut.Find("#blog-editor-title").GetAttribute("value")).IsEqualTo("Existing Article");
        await Assert.That(cut.Find("#blog-editor-slug").GetAttribute("value")).IsEqualTo("existing-article");
        await Assert.That(cut.Markup).Contains("Community");
        await Assert.That(cut.Markup).Contains("Ramadan");
        await Assert.That(cut.Markup).Contains("Existing thumbnail selected.");
    }

    [Test]
    public async Task BlogEditor_Create_MapsDtoUploadsBlogImagePurposeAndNavigatesToManagement()
    {
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var thumbnailId = Guid.NewGuid();
        var selectedFile = new FileUploadData { Content = [1, 2, 3], FileName = "hero.png", ContentType = "image/png" };
        CreateBlogDto? captured = null;
        string? capturedPurpose = null;
        _categoryService.GetAllCategoriesAsync().Returns(new List<CategoryListDto>
        {
            new() { Id = categoryId, FullName = "Community", MasterCode = "COMMUNITY" }
        });
        _tagService.GetAllTagsAsync().Returns(new List<TagListDto>
        {
            new() { Id = tagId, FullName = "Ramadan", MasterCode = "RAMADAN" }
        });
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(BlogCollection(Link("create")));
        _imageStorageService.UploadAndCreateRecordFromBytesAsync(selectedFile, Arg.Any<string>())
            .Returns(call =>
            {
                capturedPurpose = call.ArgAt<string>(1);
                return new ImageUploadResult { Success = true, StorageObjectId = thumbnailId };
            });
        _apiClient.CreateBlogAsync(Arg.Do<CreateBlogDto>(dto => captured = dto), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Success(Guid.NewGuid()));

        var cut = _ctx.RenderMudComponent<BlogEditor>();
        cut.WaitForState(() => cut.Markup.Contains("Create article", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        cut.Find("#blog-editor-title").Change("  New Article  ");
        cut.Find("#blog-editor-slug").Change(" new-article ");
        cut.Find("#blog-editor-content").Input("# Body");
        cut.Find($"#blog-editor-category-{categoryId}").Change(true);
        cut.Find($"#blog-editor-tag-{tagId}").Change(true);
        await cut.InvokeAsync(() => cut.FindComponent<ImageUpload>().Instance.FileDataSelected.InvokeAsync(selectedFile));
        cut.Find("form.blog-editor__form").Submit();

        cut.WaitForAssertion(() => _apiClient.Received(1).CreateBlogAsync(Arg.Any<CreateBlogDto>(), cancellationToken: Arg.Any<CancellationToken>()), TimeSpan.FromSeconds(2));
        await Assert.That(captured).IsNotNull();
        await Assert.That(captured!.Title).IsEqualTo("New Article");
        await Assert.That(captured.Slug).IsEqualTo("new-article");
        await Assert.That(captured.ContentMarkdown).IsEqualTo("# Body");
        await Assert.That(captured.CategoryIds).Contains(categoryId);
        await Assert.That(captured.TagIds).Contains(tagId);
        await Assert.That(captured.ThumbnailStorageObjectId).IsEqualTo(thumbnailId);
        await Assert.That(capturedPurpose).IsEqualTo(ImageUploadPurposes.BlogImage);
        await _announcer.Received(1).AnnouncePoliteAsync("Article created.");
        await Assert.That(_ctx.Services.GetRequiredService<NavigationManager>().Uri).EndsWith("/blog/manage");
    }

    [Test]
    public async Task BlogEditor_Update_MapsDtoWithExpectedConcurrencyStampAndReloadsResource()
    {
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        UpdateBlogDto? captured = null;
        _categoryService.GetAllCategoriesAsync().Returns(new List<CategoryListDto>
        {
            new() { Id = categoryId, FullName = "Community", MasterCode = "COMMUNITY" }
        });
        _apiClient.GetBlogByIdForManagementAsync(id, cancellationToken: Arg.Any<CancellationToken>()).Returns(
            Detail(id, "Before", content: "Before body", concurrencyStamp: InitialConcurrencyStamp, links: Link("edit")),
            Detail(id, "Before", content: "Before body", concurrencyStamp: InitialConcurrencyStamp, links: Link("edit")),
            Detail(id, "After", content: "After body", concurrencyStamp: UpdatedConcurrencyStamp, links: Link("edit")));
        _apiClient.UpdateBlogAsync(id, Arg.Do<UpdateBlogDto>(dto => captured = dto), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Success(id));

        var cut = _ctx.RenderMudComponent<BlogEditor>(parameters => parameters.Add(p => p.Id, id));
        cut.WaitForState(() => cut.Markup.Contains("Before", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        cut.Find("#blog-editor-title").Change("After");
        cut.Find("#blog-editor-content").Input("After body");
        cut.Find($"#blog-editor-category-{categoryId}").Change(true);
        cut.Find("form.blog-editor__form").Submit();

        cut.WaitForAssertion(() => _apiClient.Received(1).UpdateBlogAsync(id, Arg.Any<UpdateBlogDto>(), cancellationToken: Arg.Any<CancellationToken>()), TimeSpan.FromSeconds(2));
        await Assert.That(captured).IsNotNull();
        await Assert.That(captured!.Title).IsEqualTo("After");
        await Assert.That(captured.ContentMarkdown).IsEqualTo("After body");
        await Assert.That(captured.CategoryIds).Contains(categoryId);
        await Assert.That(captured.ExpectedConcurrencyStamp).IsEqualTo(InitialConcurrencyStamp);
        await _announcer.Received(1).AnnouncePoliteAsync("Article saved.");
    }

    [Test]
    public async Task BlogEditor_Save_RechecksCreateHalBeforeUploadingThumbnail()
    {
        var selectedFile = new FileUploadData { Content = [1, 2, 3], FileName = "hero.png", ContentType = "image/png" };
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(BlogCollection(Link("create")), BlogCollection());

        var cut = _ctx.RenderMudComponent<BlogEditor>();
        cut.WaitForState(() => cut.Markup.Contains("Create article", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        cut.Find("#blog-editor-title").Change("New Article");
        cut.Find("#blog-editor-content").Input("# Body");
        await cut.InvokeAsync(() => cut.FindComponent<ImageUpload>().Instance.FileDataSelected.InvokeAsync(selectedFile));
        cut.Find("form.blog-editor__form").Submit();

        cut.WaitForState(() => cut.Markup.Contains("Article creation is unavailable", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        _ = _imageStorageService.DidNotReceive().UploadAndCreateRecordFromBytesAsync(Arg.Any<FileUploadData>(), Arg.Any<string>());
        await _apiClient.DidNotReceive().CreateBlogAsync(Arg.Any<CreateBlogDto>(), cancellationToken: Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task BlogEditor_PreviewUsesSanitizedMarkdownRenderer()
    {
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(BlogCollection(Link("create")));

        var cut = _ctx.RenderMudComponent<BlogEditor>();
        cut.WaitForState(() => cut.Markup.Contains("Create article", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        cut.Find("#blog-editor-content").Input("[bad](javascript:alert(1))\n\n<script>alert(1)</script>\n\n<img src=x>");

        await Assert.That(cut.FindAll(".blog-editor__preview-body script").Count).IsEqualTo(0);
        await Assert.That(cut.FindAll(".blog-editor__preview-body img").Count).IsEqualTo(0);
        await Assert.That(cut.FindAll(".blog-editor__preview-body a[href]").Count).IsEqualTo(0);
        await Assert.That(cut.Find(".blog-editor__preview-body").TextContent).Contains("<script>alert(1)</script>");
    }

    [Test]
    public async Task BlogEditor_PreviewDemotesMarkdownH1AndUpdatesOnInput()
    {
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(BlogCollection(Link("create")));

        var cut = _ctx.RenderMudComponent<BlogEditor>();
        cut.WaitForState(() => cut.Markup.Contains("Create article", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        cut.Find("#blog-editor-content").Input("# Preview Heading");

        await Assert.That(cut.FindAll("h1").Count).IsEqualTo(1);
        await Assert.That(cut.Markup).Contains("<h2>Preview Heading</h2>");
    }

    [Test]
    public async Task BlogEditor_RemoveExistingThumbnailClearsMappedUpdateDto()
    {
        var id = Guid.NewGuid();
        var thumbnailId = Guid.Parse("30000000-0000-0000-0000-000000000001");
        UpdateBlogDto? captured = null;
        _apiClient.GetBlogByIdForManagementAsync(id, cancellationToken: Arg.Any<CancellationToken>()).Returns(
            Detail(id, "Before", thumbnailStorageObjectId: thumbnailId, concurrencyStamp: InitialConcurrencyStamp, links: Link("edit")),
            Detail(id, "Before", thumbnailStorageObjectId: thumbnailId, concurrencyStamp: InitialConcurrencyStamp, links: Link("edit")),
            Detail(id, "Before", thumbnailStorageObjectId: thumbnailId, concurrencyStamp: InitialConcurrencyStamp, links: Link("edit")),
            Detail(id, "After", concurrencyStamp: UpdatedConcurrencyStamp, links: Link("edit")));
        _apiClient.UpdateBlogAsync(id, Arg.Do<UpdateBlogDto>(dto => captured = dto), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Success(id));

        var cut = _ctx.RenderMudComponent<BlogEditor>(parameters => parameters.Add(p => p.Id, id));
        cut.WaitForState(() => cut.Markup.Contains("Existing thumbnail selected.", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        cut.FindAll("button").Single(button => button.TextContent.Contains("Remove thumbnail", StringComparison.Ordinal)).Click();
        cut.Find("form.blog-editor__form").Submit();

        cut.WaitForAssertion(() => _apiClient.Received(1).UpdateBlogAsync(id, Arg.Any<UpdateBlogDto>(), cancellationToken: Arg.Any<CancellationToken>()), TimeSpan.FromSeconds(2));
        await Assert.That(captured).IsNotNull();
        await Assert.That(captured!.ThumbnailStorageObjectId).IsNull();
    }

    [Test]
    public async Task BlogEditor_PublishUnpublishAndDelete_FollowHalOnlyAndConfirmDelete()
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

        _apiClient.GetBlogByIdForManagementAsync(publishId, cancellationToken: Arg.Any<CancellationToken>()).Returns(
            Detail(publishId, "Publish Me", links: Link("publish")),
            Detail(publishId, "Publish Me", links: Link("publish")),
            Detail(publishId, "Publish Me"));
        var publishCut = _ctx.RenderMudComponent<BlogEditor>(parameters => parameters.Add(p => p.Id, publishId));
        publishCut.WaitForState(() => publishCut.Markup.Contains("Publish", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        await Assert.That(publishCut.Markup).DoesNotContain("Unpublish");
        publishCut.FindAll("button").Single(button => button.TextContent.Contains("Publish", StringComparison.Ordinal)).Click();
        publishCut.WaitForAssertion(() => _apiClient.Received(1).PublishBlogAsync(publishId, cancellationToken: Arg.Any<CancellationToken>()), TimeSpan.FromSeconds(2));

        _apiClient.GetBlogByIdForManagementAsync(unpublishId, cancellationToken: Arg.Any<CancellationToken>()).Returns(
            Detail(unpublishId, "Unpublish Me", publishedAtUtc: DateTime.UtcNow, links: Link("unpublish")),
            Detail(unpublishId, "Unpublish Me", publishedAtUtc: DateTime.UtcNow, links: Link("unpublish")),
            Detail(unpublishId, "Unpublish Me"));
        var unpublishCut = _ctx.RenderMudComponent<BlogEditor>(parameters => parameters.Add(p => p.Id, unpublishId));
        unpublishCut.WaitForState(() => unpublishCut.Markup.Contains("Unpublish", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        await Assert.That(unpublishCut.Markup).DoesNotContain("Publish</span>");
        unpublishCut.FindAll("button").Single(button => button.TextContent.Contains("Unpublish", StringComparison.Ordinal)).Click();
        unpublishCut.WaitForAssertion(() => _apiClient.Received(1).UnpublishBlogAsync(unpublishId, cancellationToken: Arg.Any<CancellationToken>()), TimeSpan.FromSeconds(2));

        _apiClient.GetBlogByIdForManagementAsync(deleteId, cancellationToken: Arg.Any<CancellationToken>()).Returns(
            Detail(deleteId, "Delete Me", links: Link("delete")),
            Detail(deleteId, "Delete Me", links: Link("delete")));
        var deleteCut = _ctx.RenderMudComponent<BlogEditor>(parameters => parameters.Add(p => p.Id, deleteId));
        deleteCut.WaitForState(() => deleteCut.Markup.Contains("Delete", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        deleteCut.FindAll("button").Single(button => button.TextContent.Contains("Delete", StringComparison.Ordinal)).Click();
        deleteCut.WaitForAssertion(() => _dialogService.Received(1).ShowMessageBoxAsync(
            "Delete article",
            "Delete 'Delete Me'? This cannot be undone.",
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<DialogOptions>()), TimeSpan.FromSeconds(2));
        deleteCut.WaitForAssertion(() => _apiClient.Received(1).DeleteBlogAsync(deleteId, cancellationToken: Arg.Any<CancellationToken>()), TimeSpan.FromSeconds(2));
    }

    [Test]
    public async Task BlogEditor_LoadErrorsAndTaxonomyFailuresUseAccessibleStatus()
    {
        _categoryService.GetAllCategoriesAsync().ThrowsAsync(new InvalidOperationException("taxonomy down"));
        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).Returns(BlogCollection(Link("create")));

        var taxonomyFailure = _ctx.RenderMudComponent<BlogEditor>();
        taxonomyFailure.WaitForState(() => taxonomyFailure.Markup.Contains("temporarily unavailable", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        await Assert.That(taxonomyFailure.Markup).Contains("role=\"status\"");

        _apiClient.GetBlogsForManagementAsync(
            pageNumber: Arg.Any<int?>(),
            pageSize: Arg.Any<int?>(),
            search: Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("boom"));

        var loadFailure = _ctx.RenderMudComponent<BlogEditor>();
        loadFailure.WaitForState(() => loadFailure.Markup.Contains("role=\"alert\"", StringComparison.Ordinal), TimeSpan.FromSeconds(2));
        await _announcer.Received().AnnounceAssertiveAsync("The article editor could not be loaded. Please try again later.");
    }

    private static HalCollectionResourceOfBlogSummaryDto BlogCollection(params KeyValuePair<string, HalLink>[] links)
        => new()
        {
            PageNumber = 1,
            PageSize = 1,
            TotalCount = 0,
            _links = links.ToDictionary(pair => pair.Key, pair => pair.Value),
            _embedded = new HalCollectionEmbeddedOfBlogSummaryDto { Items = [] }
        };

    private static HalResourceOfBlogDetailDto Detail(
        Guid id,
        string title,
        string? slug = null,
        string? content = null,
        DateTime? publishedAtUtc = null,
        IReadOnlyList<Guid>? categoryIds = null,
        IReadOnlyList<Guid>? tagIds = null,
        Guid? thumbnailStorageObjectId = null,
        Guid? concurrencyStamp = null,
        params KeyValuePair<string, HalLink>[] links)
        => new()
        {
            Id = id,
            Title = title,
            Slug = slug ?? title.ToLowerInvariant().Replace(' ', '-'),
            ContentMarkdown = content ?? "# Body",
            PublishedAtUtc = publishedAtUtc,
            CategoryIds = categoryIds?.ToList() ?? [],
            TagIds = tagIds?.ToList() ?? [],
            ThumbnailStorageObjectId = thumbnailStorageObjectId,
            ConcurrencyStamp = concurrencyStamp ?? InitialConcurrencyStamp,
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
