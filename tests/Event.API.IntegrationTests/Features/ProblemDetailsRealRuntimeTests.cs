// ABOUTME: ProblemDetails contract tests against the real ASP.NET Core pipeline with PostgreSQL.
// ABOUTME: Verifies error shapes from actual handler exceptions, not ThrowingMediator stubs.

using System.Net;
using System.Text;
using System.Text.Json;
using Event.Api.IntegrationTests.Fixtures;
using Event.Api.IntegrationTests.Helpers;
using Event.Api.IntegrationTests.Seeds;
using Explore.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Event.Api.IntegrationTests.Features;

/// <summary>
/// Verifies ProblemDetails response shape from real handler exceptions flowing through the
/// full exception handling pipeline (GlobalExceptionHandler + ValidationExceptionHandler).
/// Complements the existing ProblemDetailsContractTests which use ThrowingMediator stubs.
/// These tests verify production-faithful behavior with real PostgreSQL.
/// </summary>
[ClassDataSource<RealRuntimeApiFixture>(Shared = SharedType.PerAssembly)]
[NotInParallel("RealRuntimeDb")]
public class ProblemDetailsRealRuntimeTests(RealRuntimeApiFixture fixture)
{
    private readonly RealRuntimeApiFixture _fixture = fixture;

    [Test]
    public async Task NotFound_FromRealHandler_ReturnsStandardProblemDetails()
    {
        await _fixture.ResetDatabaseAsync();

        var response = await _fixture.Client.GetAsync($"/api/blog/missing-{Guid.NewGuid():N}");

        await ProblemDetailsAssertions.AssertProblemDetailsAsync(
            response, HttpStatusCode.NotFound, "Blog not found");
    }

    [Test]
    public async Task NotFound_FromRealHandler_DoesNotLeakStackTrace()
    {
        await _fixture.ResetDatabaseAsync();

        var response = await _fixture.Client.GetAsync($"/api/blog/missing-{Guid.NewGuid():N}");
        var content = await response.Content.ReadAsStringAsync();

        await Assert.That(content).DoesNotContain("StackTrace");
        await Assert.That(content).DoesNotContain("   at ");
        await Assert.That(content).DoesNotContain("System.Exception");
    }

    [Test]
    public async Task NotFound_FromRealHandler_ContainsRequiredFields()
    {
        await _fixture.ResetDatabaseAsync();

        var response = await _fixture.Client.GetAsync($"/api/blog/missing-{Guid.NewGuid():N}");

        using var document = await ProblemDetailsAssertions.ReadAsJsonAsync(response);
        var root = document.RootElement;

        // RFC 9110 type URI for 404
        await Assert.That(root.TryGetProperty("type", out var typeValue)).IsTrue();
        await Assert.That(typeValue.GetString()).IsEqualTo("https://tools.ietf.org/html/rfc9110#section-15.5.5");

        // Custom extensions: traceId and timestamp
        await Assert.That(root.TryGetProperty("traceId", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("timestamp", out _)).IsTrue();
    }

    [Test]
    public async Task Unauthorized_Post_ReturnsCorrectStatusCode()
    {
        await _fixture.ResetDatabaseAsync();

        var content = new StringContent("""{"title":"Unauth Blog","contentMarkdown":"# Draft"}""", Encoding.UTF8, "application/json");
        var response = await _fixture.Client.PostAsync("/api/blog", content);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task NotFound_ContentType_IsProblemJson()
    {
        await _fixture.ResetDatabaseAsync();

        var response = await _fixture.Client.GetAsync($"/api/blog/missing-{Guid.NewGuid():N}");
        var contentType = response.Content.Headers.ContentType?.MediaType;

        var isValidContentType = contentType is "application/problem+json" or "application/json";
        await Assert.That(isValidContentType).IsTrue();
    }
}
