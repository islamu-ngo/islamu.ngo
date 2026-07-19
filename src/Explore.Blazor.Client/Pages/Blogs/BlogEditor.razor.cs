// ABOUTME: Code-behind for the authenticated Blog create/edit page with HAL-gated saves and actions.
// ABOUTME: Maps local editor state to generated Blog DTOs while preserving resource concurrency and links.

using System.ComponentModel.DataAnnotations;
using Explore.Blazor.Client.Clients;
using Explore.Blazor.Client.Contracts.Services.Accessibility;
using Explore.Blazor.Client.Helpers;
using Explore.Blazor.Client.Services;
using Explore.Blazor.Client.Services.Blogs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Explore.Blazor.Client.Pages.Blogs;

#nullable enable

public sealed partial class BlogEditor : ComponentBase, IDisposable
{
    private readonly CancellationTokenSource _disposeCts = new();
    private readonly BlogEditorModel _model = new();
    private EditContext? _editContext;
    private HalCollectionResourceOfBlogSummaryDto? _collection;
    private HalResourceOfBlogDetailDto? _resource;
    private List<CategoryListDto> _categories = [];
    private List<TagListDto> _tags = [];
    private FileUploadData? _pendingThumbnail;
    private string? _thumbnailPreviewUrl;
    private string? _errorMessage;
    private string? _statusMessage;
    private bool _isLoading = true;
    private bool _isSaving;
    private bool _isMutating;

    [Parameter] public Guid Id { get; set; }
    [Inject] public required BlogService BlogService { get; set; }
    [Inject] public required ICategoryService CategoryService { get; set; }
    [Inject] public required ITagService TagService { get; set; }
    [Inject] public required IImageStorageService ImageStorageService { get; set; }
    [Inject] public required BlogMarkdownRenderer MarkdownRenderer { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required IDialogService DialogService { get; set; }
    [Inject] public required IAccessibilityAnnouncerService Announcer { get; set; }

    private bool IsNew => Id == Guid.Empty;
    private bool IsBusy => _isSaving || _isMutating;
    private bool CanCreate => IsNew && _collection?.HasLink("create") == true;
    private bool CanEdit => !IsNew && _resource?.HasLink("edit") == true;
    private bool CanUseForm => CanCreate || CanEdit;
    private bool CanPublish => !IsNew && _resource?.HasLink("publish") == true;
    private bool CanUnpublish => !IsNew && _resource?.HasLink("unpublish") == true;
    private bool CanDelete => !IsNew && _resource?.HasLink("delete") == true;
    private bool CanRemoveThumbnail => CanUseForm
        && _model.ThumbnailStorageObjectId is { } thumbnailId
        && thumbnailId != Guid.Empty
        && string.IsNullOrWhiteSpace(_thumbnailPreviewUrl);
    private string RenderedPreviewHtml => DemoteContentH1(MarkdownRenderer.Render(_model.ContentMarkdown));
    private string PageTitleText => IsNew ? "New Blog Article | ISLAMU" : "Edit Blog Article | ISLAMU";
    private string HeadingText => IsNew ? "New Blog article" : "Edit Blog article";
    private string SaveButtonText => IsNew ? "Create article" : "Save article";
    private string UnavailableMessage => IsNew
        ? "Article creation is unavailable for this account."
        : "This article is not available for editing.";

    protected override async Task OnInitializedAsync()
    {
        _editContext = new EditContext(_model);
        await LoadAsync();
    }

    public void Dispose()
    {
        _disposeCts.Cancel();
        _disposeCts.Dispose();
    }

    private async Task LoadAsync()
    {
        _isLoading = true;
        _errorMessage = null;
        _statusMessage = null;

        try
        {
            await LoadTaxonomyAsync();

            if (IsNew)
            {
                _collection = await BlogService.GetBlogsForManagementAsync(1, 1, cancellationToken: _disposeCts.Token);
            }
            else
            {
                await LoadResourceAsync(applyModel: true);
            }

            if (!CanUseForm)
            {
                await Announcer.AnnouncePoliteAsync(UnavailableMessage);
            }
        }
        catch (OperationCanceledException) when (_disposeCts.IsCancellationRequested)
        {
        }
        catch
        {
            _errorMessage = "The article editor could not be loaded.";
            await Announcer.AnnounceAssertiveAsync("The article editor could not be loaded. Please try again later.");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadTaxonomyAsync()
    {
        try
        {
            _categories = (await CategoryService.GetAllCategoriesAsync()).OrderBy(DisplayName).ToList();
            _tags = (await TagService.GetAllTagsAsync()).OrderBy(DisplayName).ToList();
        }
        catch
        {
            _categories = [];
            _tags = [];
            _statusMessage = "Categories and tags are temporarily unavailable; they are optional.";
        }
    }

    private async Task<HalResourceOfBlogDetailDto?> LoadResourceAsync(bool applyModel)
    {
        var resource = await BlogService.GetBlogByIdForManagementAsync(Id, _disposeCts.Token);
        _resource = resource;

        if (resource is null)
            return null;

        if (applyModel)
            ApplyResource(resource);

        return resource;
    }

    private void ApplyResource(HalResourceOfBlogDetailDto resource)
    {
        _model.Title = resource.Title ?? string.Empty;
        _model.Slug = resource.Slug;
        _model.ContentMarkdown = resource.ContentMarkdown ?? string.Empty;
        _model.CategoryIds = resource.CategoryIds?.OfType<Guid>().ToHashSet() ?? [];
        _model.TagIds = resource.TagIds?.OfType<Guid>().ToHashSet() ?? [];
        _model.ThumbnailStorageObjectId = resource.ThumbnailStorageObjectId;
        _model.ConcurrencyStamp = resource.ConcurrencyStamp;
        _pendingThumbnail = null;
        _thumbnailPreviewUrl = null;
        _editContext = new EditContext(_model);
    }

    private async Task SaveAsync()
    {
        if (_editContext?.Validate() != true || IsBusy)
            return;

        _isSaving = true;
        _errorMessage = null;
        _statusMessage = "Saving article...";

        try
        {
            if (!await RefreshSaveAffordanceAsync())
                return;

            var thumbnailId = await ResolveThumbnailAsync();
            if (_errorMessage is not null)
                return;

            if (!await RefreshSaveAffordanceAsync())
                return;

            if (IsNew)
            {
                await SaveNewAsync(thumbnailId);
            }
            else
            {
                await SaveExistingAsync(thumbnailId);
            }
        }
        catch (OperationCanceledException) when (_disposeCts.IsCancellationRequested)
        {
        }
        catch
        {
            _errorMessage = "Article changes could not be saved.";
            await Announcer.AnnounceAssertiveAsync("Article changes could not be saved. Please try again later.");
        }
        finally
        {
            _isSaving = false;
            if (_errorMessage is null)
                _statusMessage = null;
        }
    }

    private async Task SaveNewAsync(Guid? thumbnailId)
    {
        var response = await BlogService.CreateBlogAsync(ToCreateDto(thumbnailId), _disposeCts.Token);
        if (response?.Success == true)
        {
            await Announcer.AnnouncePoliteAsync("Article created.");
            Navigation.NavigateTo("/blog/manage");
            return;
        }

        _errorMessage = "Article could not be created.";
        await Announcer.AnnounceAssertiveAsync(_errorMessage);
    }

    private async Task SaveExistingAsync(Guid? thumbnailId)
    {
        var response = await BlogService.UpdateBlogAsync(Id, ToUpdateDto(thumbnailId), _disposeCts.Token);
        if (response?.Success == true)
        {
            await LoadResourceAsync(applyModel: true);
            await Announcer.AnnouncePoliteAsync("Article saved.");
            return;
        }

        _errorMessage = "Article could not be saved.";
        await Announcer.AnnounceAssertiveAsync(_errorMessage);
    }

    private async Task<Guid?> ResolveThumbnailAsync()
    {
        if (_pendingThumbnail is null)
            return _model.ThumbnailStorageObjectId;

        var upload = await ImageStorageService.UploadAndCreateRecordFromBytesAsync(
            _pendingThumbnail,
            ImageUploadPurposes.BlogImage);
        if (upload?.Success == true)
        {
            _model.ThumbnailStorageObjectId = upload.StorageObjectId;
            _pendingThumbnail = null;
            return upload.StorageObjectId;
        }

        _errorMessage = upload?.ErrorMessage ?? "Thumbnail could not be uploaded.";
        await Announcer.AnnounceAssertiveAsync(_errorMessage);
        return null;
    }

    private async Task<bool> RefreshSaveAffordanceAsync()
    {
        if (IsNew)
        {
            _collection = await BlogService.GetBlogsForManagementAsync(1, 1, cancellationToken: _disposeCts.Token);
            if (CanCreate)
                return true;
        }
        else
        {
            var latest = await LoadResourceAsync(applyModel: false);
            if (latest?.HasLink("edit") == true)
                return true;
        }

        _errorMessage = UnavailableMessage;
        await Announcer.AnnounceAssertiveAsync(UnavailableMessage);
        return false;
    }

    private CreateBlogDto ToCreateDto(Guid? thumbnailId) => new()
    {
        Title = _model.Title.Trim(),
        Slug = CleanOptional(_model.Slug),
        ContentMarkdown = _model.ContentMarkdown,
        ThumbnailStorageObjectId = thumbnailId,
        CategoryIds = _model.CategoryIds.ToList(),
        TagIds = _model.TagIds.ToList()
    };

    private UpdateBlogDto ToUpdateDto(Guid? thumbnailId) => new()
    {
        Title = _model.Title.Trim(),
        Slug = CleanOptional(_model.Slug),
        ContentMarkdown = _model.ContentMarkdown,
        ThumbnailStorageObjectId = thumbnailId,
        CategoryIds = _model.CategoryIds.ToList(),
        TagIds = _model.TagIds.ToList(),
        ExpectedConcurrencyStamp = _model.ConcurrencyStamp
    };

    private async Task PublishAsync() => await RunResourceCommandAsync(
        "publish",
        () => BlogService.PublishBlogAsync(Id, _disposeCts.Token),
        "Article published.",
        "Article could not be published.");

    private async Task UnpublishAsync() => await RunResourceCommandAsync(
        "unpublish",
        () => BlogService.UnpublishBlogAsync(Id, _disposeCts.Token),
        "Article unpublished.",
        "Article could not be unpublished.");

    private async Task DeleteAsync()
    {
        if (IsBusy)
            return;

        var latest = await LoadResourceAsync(applyModel: false);
        if (latest?.HasLink("delete") != true)
        {
            await Announcer.AnnounceAssertiveAsync("Article deletion is unavailable.");
            return;
        }

        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Delete article",
            $"Delete '{_model.Title}'? This cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel");
        if (confirmed != true)
            return;

        _isMutating = true;
        try
        {
            if (await BlogService.DeleteBlogAsync(Id, _disposeCts.Token))
            {
                await Announcer.AnnouncePoliteAsync("Article deleted.");
                Navigation.NavigateTo("/blog/manage");
                return;
            }

            await Announcer.AnnounceAssertiveAsync("Article could not be deleted.");
        }
        finally
        {
            _isMutating = false;
        }
    }

    private async Task RunResourceCommandAsync(
        string rel,
        Func<Task<BaseCommandResponseOfGuid?>> command,
        string successMessage,
        string failureMessage)
    {
        if (IsBusy)
            return;

        var latest = await LoadResourceAsync(applyModel: false);
        if (latest?.HasLink(rel) != true)
        {
            await Announcer.AnnounceAssertiveAsync(failureMessage);
            return;
        }

        _isMutating = true;
        try
        {
            var response = await command();
            if (response?.Success == true)
            {
                await LoadResourceAsync(applyModel: true);
                await Announcer.AnnouncePoliteAsync(successMessage);
                return;
            }

            await Announcer.AnnounceAssertiveAsync(failureMessage);
        }
        finally
        {
            _isMutating = false;
        }
    }

    private Task OnThumbnailFileSelectedAsync(FileUploadData? fileData)
    {
        _pendingThumbnail = fileData;
        if (fileData is null)
            _model.ThumbnailStorageObjectId = null;

        return Task.CompletedTask;
    }

    private void RemoveThumbnail()
    {
        _model.ThumbnailStorageObjectId = null;
        _pendingThumbnail = null;
        _thumbnailPreviewUrl = null;
    }

    private void OnContentInput(ChangeEventArgs args)
    {
        _model.ContentMarkdown = args.Value?.ToString() ?? string.Empty;
        _editContext?.NotifyFieldChanged(FieldIdentifier.Create(() => _model.ContentMarkdown));
    }

    private void ToggleCategory(Guid id, ChangeEventArgs args) => Toggle(_model.CategoryIds, id, args);

    private void ToggleTag(Guid id, ChangeEventArgs args) => Toggle(_model.TagIds, id, args);

    private static void Toggle(HashSet<Guid> values, Guid id, ChangeEventArgs args)
    {
        if (args.Value is bool isChecked && isChecked)
            values.Add(id);
        else
            values.Remove(id);
    }

    private static string? CleanOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static string DisplayName(CategoryListDto category) => DisplayName(category.FullName, category.MasterCode);

    private static string DisplayName(TagListDto tag) => DisplayName(tag.FullName, tag.MasterCode);

    private static string DisplayName(string? fullName, string? masterCode)
        => string.IsNullOrWhiteSpace(fullName) ? masterCode ?? "Unnamed" : fullName;

    private static string DemoteContentH1(string html)
        => html.Replace("<h1>", "<h2>", StringComparison.OrdinalIgnoreCase)
            .Replace("</h1>", "</h2>", StringComparison.OrdinalIgnoreCase);

    private sealed class BlogEditorModel
    {
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Slug { get; set; }

        [Required]
        [MaxLength(100_000)]
        public string ContentMarkdown { get; set; } = string.Empty;

        public HashSet<Guid> CategoryIds { get; set; } = [];
        public HashSet<Guid> TagIds { get; set; } = [];
        public Guid? ThumbnailStorageObjectId { get; set; }
        public Guid? ConcurrencyStamp { get; set; }
    }
}
