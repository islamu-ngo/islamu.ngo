// ABOUTME: Code-behind for the public Blog index reader page.
// ABOUTME: Calls BlogService, extracts HAL items, filters non-public posts defensively, and manages search pagination state.

#nullable enable

using Explore.Blazor.Client.Clients;
using Explore.Blazor.Client.Helpers;
using Microsoft.AspNetCore.Components;

namespace Explore.Blazor.Client.Pages.Blogs;

public partial class BlogIndex : IDisposable
{
    private const int DefaultPageSize = 10;

    private readonly CancellationTokenSource _disposeCts = new();
    private readonly List<BlogSummaryDto> _posts = [];

    private bool _isLoading = true;
    private string? _errorMessage;
    private string? _searchText;
    private string? _appliedSearch;
    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;
    private int _totalCount;

    private bool CanGoPrevious => _pageNumber > 1;

    private bool CanGoNext => _pageNumber * _pageSize < _totalCount;

    protected override async Task OnInitializedAsync()
    {
        await LoadPageAsync();
    }

    private async Task LoadPageAsync()
    {
        _isLoading = true;
        _errorMessage = null;
        _posts.Clear();

        try
        {
            var response = await BlogService.GetPublishedBlogsAsync(
                pageNumber: _pageNumber,
                pageSize: _pageSize,
                search: string.IsNullOrWhiteSpace(_appliedSearch) ? null : _appliedSearch,
                cancellationToken: _disposeCts.Token);

            _pageNumber = response.PageNumber ?? _pageNumber;
            _pageSize = response.PageSize ?? _pageSize;
            _totalCount = response.TotalCount ?? 0;
            _posts.AddRange(response.GetItems().Where(IsPublicPost));
        }
        catch (OperationCanceledException) when (_disposeCts.IsCancellationRequested)
        {
            return;
        }
        catch (Exception)
        {
            _errorMessage = "Published articles could not be loaded. Please try again later.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static bool IsPublicPost(BlogSummaryDto post)
        => post.PublishedAtUtc is { } publishedAtUtc && publishedAtUtc <= DateTime.UtcNow;

    private async Task ApplySearchAsync()
    {
        _appliedSearch = _searchText?.Trim();
        _pageNumber = 1;
        await LoadPageAsync();
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
        await LoadPageAsync();
    }

    private async Task NextPageAsync()
    {
        if (!CanGoNext)
            return;

        _pageNumber++;
        await LoadPageAsync();
    }

    private static string BuildArticleHref(string? slug)
        => string.IsNullOrWhiteSpace(slug) ? "/blog" : $"/blog/{Uri.EscapeDataString(slug)}";

    private static string FormatMachineDate(DateTimeOffset? publishedAtUtc)
        => publishedAtUtc?.ToUniversalTime().ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

    private static string FormatDisplayDate(DateTimeOffset? publishedAtUtc)
        => publishedAtUtc?.ToUniversalTime().ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

    public void Dispose()
    {
        _disposeCts.Cancel();
        _disposeCts.Dispose();
    }
}
