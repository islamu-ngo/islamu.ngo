// ABOUTME: Code-behind for the public Blog article reader page.
// ABOUTME: Guards draft/future articles as not found and renders Markdown through BlogMarkdownRenderer.

#nullable enable

using Explore.Blazor.Client.Clients;
using Microsoft.AspNetCore.Components;

namespace Explore.Blazor.Client.Pages.Blogs;

public partial class BlogArticle : IDisposable
{
    private readonly CancellationTokenSource _disposeCts = new();

    [Parameter] public string? Slug { get; set; }

    private BlogDetailDto? _article;
    private bool _isLoading = true;
    private string? _errorMessage;
    private string _renderedHtml = string.Empty;

    private string ArticlePageTitle => _article is null ? "Blog article | ISLAMU" : $"{_article.Title} | ISLAMU";

    private string CanonicalPath => string.IsNullOrWhiteSpace(Slug) ? "/blog" : $"/blog/{Uri.EscapeDataString(Slug)}";

    private string ArticleDescription => _article is null
        ? "Read ISLAMU editorial notes and community updates."
        : $"Read {_article.Title} on the ISLAMU Blog.";

    protected override async Task OnParametersSetAsync()
    {
        await LoadArticleAsync();
    }

    private async Task LoadArticleAsync()
    {
        _isLoading = true;
        _errorMessage = null;
        _article = null;
        _renderedHtml = string.Empty;

        if (string.IsNullOrWhiteSpace(Slug))
        {
            _isLoading = false;
            return;
        }

        try
        {
            var article = await BlogService.GetPublishedBlogBySlugAsync(Slug, _disposeCts.Token);
            if (!IsPublicArticle(article))
                return;

            _article = article;
            _renderedHtml = DemoteContentH1(MarkdownRenderer.Render(article.ContentMarkdown));
        }
        catch (OperationCanceledException) when (_disposeCts.IsCancellationRequested)
        {
            return;
        }
        catch (Exception)
        {
            _errorMessage = "This article could not be loaded. Please try again later.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static bool IsPublicArticle(BlogDetailDto? article)
        => article?.PublishedAtUtc is { } publishedAtUtc && publishedAtUtc <= DateTime.UtcNow;

    private static string FormatMachineDate(DateTimeOffset? publishedAtUtc)
        => publishedAtUtc?.ToUniversalTime().ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

    private static string FormatDisplayDate(DateTimeOffset? publishedAtUtc)
        => publishedAtUtc?.ToUniversalTime().ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

    private static string DemoteContentH1(string html)
        => html.Replace("<h1>", "<h2>", StringComparison.OrdinalIgnoreCase)
            .Replace("</h1>", "</h2>", StringComparison.OrdinalIgnoreCase);

    public void Dispose()
    {
        _disposeCts.Cancel();
        _disposeCts.Dispose();
    }
}
