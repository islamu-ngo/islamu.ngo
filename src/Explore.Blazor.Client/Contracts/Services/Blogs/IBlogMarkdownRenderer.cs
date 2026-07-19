// ABOUTME: Client contract for converting Blog Markdown into safe HTML for display and preview.
// ABOUTME: Lets Blog components consume the renderer through the interface-only DI boundary.

namespace Explore.Blazor.Client.Contracts.Services.Blogs;

public interface IBlogMarkdownRenderer
{
    string Render(string? markdown);
}
