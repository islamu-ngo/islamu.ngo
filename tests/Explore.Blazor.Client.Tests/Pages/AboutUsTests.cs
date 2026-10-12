// ABOUTME: Component accessibility tests for the public About Us page.
// ABOUTME: Verifies that visual typography preserves a sequential semantic heading outline.

using Explore.Blazor.Client.Pages.About;

namespace Explore.Blazor.Client.Tests.Pages;

public sealed class AboutUsTests : IDisposable
{
    private readonly BlazorTestContext _ctx = new();

    public void Dispose()
    {
        _ctx.Dispose();
    }

    [Test]
    public async Task AboutUs_UsesH2ForEverySectionBelowThePageHeading()
    {
        var cut = _ctx.RenderMudComponent<AboutUs>();

        var headingTags = cut.FindAll("h1, h2, h3, h4, h5, h6")
            .Select(element => element.TagName)
            .ToArray();
        await Assert.That(headingTags).IsEquivalentTo(["H1", "H2", "H2", "H2"]);
        await Assert.That(cut.Find(".mud-container").ClassList).Contains("mud-container-maxwidth-sm");
    }
}
