// ABOUTME: Unit tests for Blazor-side external API key creation validation.
// ABOUTME: Verifies immediate client feedback mirrors server-side API key input hardening.

using Explore.Blazor.Client.Constants;
using Explore.Blazor.Client.Validators;

namespace Explore.Blazor.Client.Tests.Validators;

public class CreateExternalApiKeyDtoValidatorTests
{
    private readonly CreateExternalApiKeyDtoValidator _validator = new();

    [Test]
    public async Task Validate_WhenNameContainsControlCharacter_ReturnsError()
    {
        var result = _validator.Validate(new CreateExternalApiKeyDto
        {
            Name = "Ops\nBot",
            Scopes = ["users:read"]
        });

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors).Contains(error => error.ErrorMessage == "API key name must not contain control characters.");
    }

    [Test]
    public async Task Validate_WhenDescriptionExceedsLimit_ReturnsError()
    {
        var result = _validator.Validate(new CreateExternalApiKeyDto
        {
            Name = "Ops Bot",
            Description = new string('a', 1001),
            Scopes = ["users:read"]
        });

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors).Contains(error => error.ErrorMessage == "API key description cannot exceed 1000 characters.");
    }

    [Test]
    public async Task Validate_WhenScopesAreEmpty_ReturnsError()
    {
        var result = _validator.Validate(new CreateExternalApiKeyDto
        {
            Name = "Ops Bot",
            Scopes = []
        });

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors).Contains(error => error.ErrorMessage == "Select at least one scope.");
    }

    [Test]
    public async Task AvailableScopes_MatchRetainedServerCatalogAndOwnerCeilings()
    {
        var expectedByOwner = new Dictionary<int, string[]>
        {
            [1] = ["users:read", "users:write", "lookups:read", "mcp:read", "mcp:propose", "api-keys:manage"],
            [2] = ["users:read", "users:write", "lookups:read", "mcp:read", "mcp:propose", "api-keys:manage", "organizations:read", "organizations:write"],
            [3] = ["users:read", "users:write", "lookups:read", "mcp:read", "mcp:propose", "api-keys:manage", "groups:read", "groups:write"],
            [4] = ["users:read", "users:write", "lookups:read", "mcp:read", "mcp:propose", "api-keys:manage", "organizations:read", "organizations:write", "groups:read", "groups:write", "admin:tenant"],
            [5] = ["users:read", "users:write", "lookups:read", "mcp:read", "mcp:propose", "api-keys:manage", "organizations:read", "organizations:write", "groups:read", "groups:write", "admin:tenant", "admin:instance"]
        };

        await Assert.That(ExternalApiKeyConstants.AvailableScopes.Select(scope => scope.Value))
            .IsEquivalentTo(expectedByOwner[5]);

        foreach (var (ownerType, expectedScopes) in expectedByOwner)
        {
            await Assert.That(ExternalApiKeyConstants.GetAvailableScopesForOwnerType(ownerType).Select(scope => scope.Value))
                .IsEquivalentTo(expectedScopes);
        }
    }
}
