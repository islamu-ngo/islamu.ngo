// ABOUTME: Converts raw Markdown blog content into sanitized HTML using Markdig then HtmlSanitizer.
// ABOUTME: Produces a minimal, safe subset of semantic HTML with strict href and tag allowlists.

using AngleSharp.Dom;
using Ganss.Xss;
using Markdig;
using Markdig.Extensions.EmphasisExtras;

namespace Explore.Blazor.Client.Services.Blogs;

public sealed class BlogMarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseEmphasisExtras()
        .DisableHtml()
        .Build();

    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedTags.Clear();
        foreach (var tag in new[]
        {
            "p", "br", "strong", "em", "del", "blockquote", "pre", "code",
            "ul", "ol", "li", "h1", "h2", "h3", "h4", "h5", "h6", "a",
        })
        {
            sanitizer.AllowedTags.Add(tag);
        }

        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.Add("href");
        sanitizer.AllowedAttributes.Add("title");

        sanitizer.AllowedCssProperties.Clear();
        sanitizer.AllowedAtRules.Clear();

        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("https");

        sanitizer.PostProcessNode += (sender, e) =>
        {
            if (e.Node is IElement element &&
                element.TagName.Equals("a", StringComparison.OrdinalIgnoreCase))
            {
                var href = element.GetAttribute("href");
                if (!string.IsNullOrEmpty(href) && !IsAbsoluteHttpOrHttps(href))
                {
                    element.RemoveAttribute("href");
                }
            }
        };

        return sanitizer;
    }

    private static bool IsAbsoluteHttpOrHttps(string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }

        return false;
    }

    public string Render(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var html = Markdown.ToHtml(markdown, Pipeline);
        return Sanitizer.Sanitize(html);
    }
}
