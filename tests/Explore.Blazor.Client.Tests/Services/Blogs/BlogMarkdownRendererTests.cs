// ABOUTME: Unit tests for BlogMarkdownRenderer covering Markdown-to-HTML conversion, HTML sanitization,
// ABOUTME: and href scrubbing. Ensures only minimal semantic tags and absolute http/https anchors survive.

using Explore.Blazor.Client.Services.Blogs;

namespace Explore.Blazor.Client.Tests.Services.Blogs;

public class BlogMarkdownRendererTests
{
    private readonly BlogMarkdownRenderer _renderer = new();

    [Test]
    public async Task Render_Headings_Emphasis_Lists_Code()
    {
        var markdown = """
# Heading 1
## Heading 2
**bold** _italic_ ~~strikethrough~~
- item one
- item two
1. ordered one
2. ordered two
> a quote
```csharp
var x = 1;
```
""";

        var result = _renderer.Render(markdown);

        await Assert.That(result).Contains("<h1>");
        await Assert.That(result).Contains("<h2>");
        await Assert.That(result).Contains("<strong>");
        await Assert.That(result).Contains("<em>");
        await Assert.That(result).Contains("<del>");
        await Assert.That(result).Contains("<ul>");
        await Assert.That(result).Contains("<ol>");
        await Assert.That(result).Contains("<li>");
        await Assert.That(result).Contains("<blockquote>");
        await Assert.That(result).Contains("<pre>");
        await Assert.That(result).Contains("<code>");
    }

    [Test]
    public async Task Render_RawHtml_Disabled()
    {
        var markdown = "<div>hello</div>";

        var result = _renderer.Render(markdown);

        await Assert.That(result).DoesNotContain("<div>");
    }

    [Test]
    public async Task Render_ScriptTag_Removed()
    {
        var markdown = "<script>alert('x')</script>";

        var result = _renderer.Render(markdown);

        await Assert.That(result).DoesNotContain("<script>");
        await Assert.That(result).DoesNotContain("alert");
    }

    [Test]
    public async Task Render_Onerror_Attribute_Removed()
    {
        var markdown = "<img src=x onerror=alert(1)>";

        var result = _renderer.Render(markdown);

        await Assert.That(result).DoesNotContain("onerror");
        await Assert.That(result).DoesNotContain("<img");
    }

    [Test]
    public async Task Render_Style_Attribute_Removed()
    {
        var markdown = "<p style='color:red'>text</p>";

        var result = _renderer.Render(markdown);

        await Assert.That(result).DoesNotContain("style");
        await Assert.That(result).DoesNotContain("color");
    }

    [Test]
    public async Task Render_Img_Tag_Removed()
    {
        var markdown = "![alt](http://example.com/img.png)";

        var result = _renderer.Render(markdown);

        await Assert.That(result).DoesNotContain("<img");
    }

    [Test]
    public async Task Render_JavaScript_Href_Removed()
    {
        var markdown = "[click](javascript:alert(1))";

        var result = _renderer.Render(markdown);

        await Assert.That(result).DoesNotContain("javascript");
        await Assert.That(result).DoesNotContain("href");
    }

    [Test]
    public async Task Render_Data_Href_Removed()
    {
        var markdown = "[click](data:text/html,<script>alert(1)</script>)";

        var result = _renderer.Render(markdown);

        await Assert.That(result).DoesNotContain("href");
    }

    [Test]
    public async Task Render_File_Href_Removed()
    {
        var markdown = "[click](file:///etc/passwd)";

        var result = _renderer.Render(markdown);

        await Assert.That(result).DoesNotContain("href");
    }

    [Test]
    public async Task Render_Relative_Href_Removed()
    {
        var markdown = "[click](/relative/path)";

        var result = _renderer.Render(markdown);

        await Assert.That(result).DoesNotContain("href");
    }

    [Test]
    public async Task Render_ProtocolRelative_Href_Removed()
    {
        var markdown = "[click](//evil.com)";

        var result = _renderer.Render(markdown);

        await Assert.That(result).DoesNotContain("href");
    }

    [Test]
    public async Task Render_AbsoluteHttp_Href_Preserved()
    {
        var markdown = "[click](http://example.com)";

        var result = _renderer.Render(markdown);

        await Assert.That(result).Contains("href");
        await Assert.That(result).Contains("http://example.com");
    }

    [Test]
    public async Task Render_AbsoluteHttps_Href_Preserved()
    {
        var markdown = "[click](https://example.com)";

        var result = _renderer.Render(markdown);

        await Assert.That(result).Contains("href");
        await Assert.That(result).Contains("https://example.com");
    }

    [Test]
    public async Task Render_Null_ReturnsEmpty()
    {
        var result = _renderer.Render(null);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Render_Whitespace_ReturnsEmpty()
    {
        var result = _renderer.Render("   \n\t  ");

        await Assert.That(result).IsEqualTo(string.Empty);
    }
}
