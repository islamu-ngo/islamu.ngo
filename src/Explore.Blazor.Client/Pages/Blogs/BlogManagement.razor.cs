// ABOUTME: Code-behind for the authenticated Blog management page.
// ABOUTME: Preserves Blog HAL item wrappers so API links remain the only action authority.

#nullable enable

using Explore.Blazor.Client.Clients;
using Explore.Blazor.Client.Contracts.Services.Accessibility;
using Explore.Blazor.Client.Contracts.Services.Blogs;
using Explore.Blazor.Client.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Explore.Blazor.Client.Pages.Blogs;

public partial class BlogManagement : ComponentBase, IDisposable
{
    private const int DefaultPageSize = 10;

    private readonly CancellationTokenSource _disposeCts = new();
    private readonly List<HalResourceOfBlogSummaryDto> _items = [];

    private HalCollectionResourceOfBlogSummaryDto? _collection;
    private bool _isLoading = true;
    private bool _isMutating;
    private string? _errorMessage;
    private string? _searchText;
    private string? _appliedSearch;
    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;
    private int _totalCount;

    [Inject]
    private IBlogService BlogService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private IAccessibilityAnnouncerService AnnouncerService { get; set; } = default!;

    private bool CanCreate => _collection?.HasLink("create") == true;

    private bool CanGoPrevious => _pageNumber > 1;

    private bool CanGoNext => _pageNumber * _pageSize < _totalCount;

    protected override async Task OnInitializedAsync()
    {
        await LoadPageAsync(announce: true);
    }

    private async Task LoadPageAsync(bool announce = false)
    {
        _isLoading = true;
        _errorMessage = null;
        _items.Clear();

        try
        {
            _collection = await BlogService.GetBlogsForManagementAsync(
                pageNumber: _pageNumber,
                pageSize: _pageSize,
                search: string.IsNullOrWhiteSpace(_appliedSearch) ? null : _appliedSearch,
                cancellationToken: _disposeCts.Token);

            _pageNumber = _collection.PageNumber ?? _pageNumber;
            _pageSize = _collection.PageSize ?? _pageSize;
            _totalCount = _collection.TotalCount ?? 0;
            _items.AddRange(_collection.GetHalItems());

            if (announce)
                await AnnouncerService.AnnouncePoliteAsync($"{_items.Count} managed Blog articles loaded.");
        }
        catch (OperationCanceledException) when (_disposeCts.IsCancellationRequested)
        {
            return;
        }
        catch (Exception)
        {
            _collection = null;
            _errorMessage = "Managed articles could not be loaded. Please try again later.";
            await AnnouncerService.AnnounceAssertiveAsync(_errorMessage);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private Task CreateBlogAsync()
    {
        if (!CanCreate)
            return Task.CompletedTask;

        Navigation.NavigateTo("/blog/manage/new");
        return Task.CompletedTask;
    }

    private Task EditBlogAsync(HalResourceOfBlogSummaryDto item)
    {
        if (item.HasLink("edit") && item.Id is { } id)
            Navigation.NavigateTo($"/blog/manage/{id}");

        return Task.CompletedTask;
    }

    private async Task PublishBlogAsync(HalResourceOfBlogSummaryDto item)
    {
        if (!item.HasLink("publish") || item.Id is not { } id)
            return;

        await RunCommandAsync(
            () => BlogService.PublishBlogAsync(id, _disposeCts.Token),
            "Article published.",
            "Article could not be published.");
    }

    private async Task UnpublishBlogAsync(HalResourceOfBlogSummaryDto item)
    {
        if (!item.HasLink("unpublish") || item.Id is not { } id)
            return;

        await RunCommandAsync(
            () => BlogService.UnpublishBlogAsync(id, _disposeCts.Token),
            "Article unpublished.",
            "Article could not be unpublished.");
    }

    private async Task DeleteBlogAsync(HalResourceOfBlogSummaryDto item)
    {
        if (!item.HasLink("delete") || item.Id is not { } id)
            return;

        bool? confirmed = await DialogService.ShowMessageBoxAsync(
            "Delete article",
            $"Delete '{item.Title}'? This cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
            return;

        await RunDeleteAsync(id);
    }

    private async Task RunCommandAsync(
        Func<Task<BaseCommandResponseOfGuid?>> command,
        string successMessage,
        string fallbackErrorMessage)
    {
        _isMutating = true;
        _errorMessage = null;

        try
        {
            var response = await command();
            if (response?.Success == true)
            {
                await AnnouncerService.AnnouncePoliteAsync(successMessage);
                await LoadPageAsync();
                return;
            }

            _errorMessage = string.IsNullOrWhiteSpace(response?.Message) ? fallbackErrorMessage : response.Message;
            await AnnouncerService.AnnounceAssertiveAsync(_errorMessage);
        }
        finally
        {
            _isMutating = false;
        }
    }

    private async Task RunDeleteAsync(Guid id)
    {
        _isMutating = true;
        _errorMessage = null;

        try
        {
            if (await BlogService.DeleteBlogAsync(id, _disposeCts.Token))
            {
                await AnnouncerService.AnnouncePoliteAsync("Article deleted.");
                await LoadPageAsync();
                return;
            }

            _errorMessage = "Article could not be deleted.";
            await AnnouncerService.AnnounceAssertiveAsync(_errorMessage);
        }
        finally
        {
            _isMutating = false;
        }
    }

    private async Task ApplySearchAsync()
    {
        _appliedSearch = _searchText?.Trim();
        _pageNumber = 1;
        await LoadPageAsync(announce: true);
    }

    private void OnSearchInput(ChangeEventArgs args)
    {
        _searchText = args.Value?.ToString();
    }

    private async Task PreviousPageAsync()
    {
        if (!CanGoPrevious)
            return;

        _pageNumber--;
        await LoadPageAsync(announce: true);
    }

    private async Task NextPageAsync()
    {
        if (!CanGoNext)
            return;

        _pageNumber++;
        await LoadPageAsync(announce: true);
    }

    private static bool TryGetSelfHref(HalResourceOfBlogSummaryDto item, out string href)
    {
        if (item._links?.TryGetValue("self", out var link) == true && !string.IsNullOrWhiteSpace(link.Href))
        {
            href = link.Href;
            return true;
        }

        href = string.Empty;
        return false;
    }

    private static string FormatPublicationState(HalResourceOfBlogSummaryDto post)
        => post.PublishedAtUtc.HasValue ? "Published" : "Draft";

    private static string FormatDisplayDate(DateTimeOffset? publishedAtUtc)
        => publishedAtUtc?.ToUniversalTime().ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture) ?? "Not published";

    public void Dispose()
    {
        _disposeCts.Cancel();
        _disposeCts.Dispose();
    }
}
