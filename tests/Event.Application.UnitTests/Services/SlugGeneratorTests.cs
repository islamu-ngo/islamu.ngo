// ABOUTME: Unit tests for deterministic Unicode-aware slug generation.
// ABOUTME: Ensures Arabic letters are preserved and blank input no longer receives random fallback text.

using Explore.Application.Services;

namespace Event.Application.UnitTests.Services;

public sealed class SlugGeneratorTests
{
    [Test]
    public async Task FromTitle_WithArabicAndLatinText_PreservesLettersAndHyphenatesSeparators()
    {
        string slug = SlugGenerator.FromTitle("  Islam 101:فقه الصيام  ");

        await Assert.That(slug).IsEqualTo("islam-101-فقه-الصيام");
    }

    [Test]
    public async Task FromTitle_WithDiacritics_RemovesCombiningMarksDeterministically()
    {
        string first = SlugGenerator.FromTitle("Café déjà vu");
        string second = SlugGenerator.FromTitle("Café déjà vu");

        await Assert.That(first).IsEqualTo("cafe-deja-vu");
        await Assert.That(second).IsEqualTo(first);
    }

    [Test]
    public async Task FromTitle_WithBlankTitle_ReturnsEmptySlugWithoutRandomFallback()
    {
        string slug = SlugGenerator.FromTitle("   ");

        await Assert.That(slug).IsEqualTo(string.Empty);
    }
}
