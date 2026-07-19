// ABOUTME: Unit tests for bounded AI reference prompt packing.
// ABOUTME: Verifies selected references are quoted safely and constrained by per-item and total budgets.

using Explore.Application.DTOs.Ai;
using Explore.Application.Features.AiAssistant.Prompting;

namespace Event.Application.UnitTests.Features.AiAssistant.Prompting;

public sealed class AiReferencePromptPackerTests
{
    [Test]
    public async Task Pack_WhenReferencesAreSelected_WrapsThemInSafeBoundaries()
    {
        var references = new[]
        {
            new AiSelectedReferenceDto("Organization", Guid.Parse("018f0000-0000-7000-8000-000000000001"), "Community <Center>", "Programs & services.")
        };

        string packed = new AiReferencePromptPacker().Pack(references);

        await Assert.That(packed).Contains("<selected_references>");
        await Assert.That(packed).Contains("<reference kind=\"Organization\" id=\"018f0000-0000-7000-8000-000000000001\">");
        await Assert.That(packed).Contains("Community &lt;Center&gt;");
        await Assert.That(packed).Contains("Programs &amp; services.");
        await Assert.That(packed).DoesNotContain("Community <Center>");
    }

    [Test]
    public async Task Pack_RespectsReferenceCountAndTotalBudgets()
    {
        var references = Enumerable.Range(1, 4)
            .Select(index => new AiSelectedReferenceDto("Organization", Guid.CreateVersion7(), $"Organization {index}", new string('x', 60)))
            .ToList();

        string packed = new AiReferencePromptPacker().Pack(
            references,
            maxReferences: 3,
            maxCharactersPerReference: 160,
            maxTotalCharacters: 340);

        await Assert.That(packed).Contains("Organization 1");
        await Assert.That(packed).Contains("Organization 2");
        await Assert.That(packed).DoesNotContain("Organization 3");
        await Assert.That(packed).DoesNotContain("Organization 4");
    }

    [Test]
    public async Task Pack_RespectsTotalTokenBudgetWhenProvided()
    {
        var references = Enumerable.Range(1, 3)
            .Select(index => new AiSelectedReferenceDto("Organization", Guid.CreateVersion7(), $"Organization {index}", "Summary"))
            .ToList();

        string packed = new AiReferencePromptPacker(new FixedAiTokenEstimator(1)).Pack(
            references,
            maxReferences: 3,
            maxTotalTokens: 2);

        await Assert.That(packed).Contains("Organization 1");
        await Assert.That(packed).Contains("Organization 2");
        await Assert.That(packed).DoesNotContain("Organization 3");
    }

    [Test]
    public async Task Pack_WhenTokenBudgetIsZero_ReturnsEmptyString()
    {
        var references = new[]
        {
            new AiSelectedReferenceDto("Organization", Guid.CreateVersion7(), "Organization", "Summary")
        };

        string packed = new AiReferencePromptPacker(new FixedAiTokenEstimator(1)).Pack(
            references,
            maxTotalTokens: 0);

        await Assert.That(packed).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Pack_WhenInputsAreEmptyOrInvalid_ReturnsEmptyString()
    {
        var packer = new AiReferencePromptPacker();

        await Assert.That(packer.Pack([])).IsEqualTo(string.Empty);
        await Assert.That(packer.Pack([new AiSelectedReferenceDto("Organization", Guid.CreateVersion7(), "Title", null)], maxReferences: 0)).IsEqualTo(string.Empty);
        await Assert.That(packer.Pack([new AiSelectedReferenceDto("", Guid.CreateVersion7(), "Title", null)])).IsEqualTo(string.Empty);
    }

    private sealed class FixedAiTokenEstimator(int tokensPerNonEmptyInput) : IAiTokenEstimator
    {
        public bool IsTokenizerBacked => true;

        public int CountTokens(string? content)
            => string.IsNullOrWhiteSpace(content) ? 0 : tokensPerNonEmptyInput;
    }
}
