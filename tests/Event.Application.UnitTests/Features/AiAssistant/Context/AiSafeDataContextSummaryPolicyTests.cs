// ABOUTME: Unit tests for generic AI safe-data context definition validation.
// ABOUTME: Proves duplicate projected fields cannot create ambiguous allow-list entries.

using Explore.Application.Features.AiAssistant.Context;

namespace Event.Application.UnitTests.Features.AiAssistant.Context;

public sealed class AiSafeDataContextSummaryPolicyTests
{
    [Test]
    public async Task RegistryWhenDuplicateFieldsAreConfiguredThrows()
    {
        var act = () => new AiSafeDataContextDefinition(
            "duplicate-context",
            "Projection",
            [
                new AiSafeDataContextField("summary", "Summary."),
                new AiSafeDataContextField("SUMMARY", "Duplicate summary.")
            ]);

        await Assert.That(act).Throws<ArgumentException>();
    }
}
