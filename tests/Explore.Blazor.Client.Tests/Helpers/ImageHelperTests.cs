// ABOUTME: Unit tests for local SVG image fallback helpers used by Blazor client pages.
// ABOUTME: Preserves organization placeholder behavior without deleted Event image helpers.

using Explore.Blazor.Client.Helpers;

namespace Explore.Blazor.Client.Tests.Helpers;

public class ImageHelperTests
{
    [Test]
    public async Task GetOrganizationPlaceholder_ReturnsLocalSvgFallback_WhenImageMissing()
    {
        var result = ImageHelper.GetOrganizationPlaceholder(null, "Community");

        await Assert.That(result).StartsWith("data:image/svg+xml;utf8,");
        await Assert.That(result).Contains(Uri.EscapeDataString("Community"));
        await Assert.That(result).DoesNotContain("placehold.co");
    }
}
