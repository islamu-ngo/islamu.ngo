// ABOUTME: Contract tests for public API query DTO validation rules.
// ABOUTME: Verifies abusive public discovery query inputs fail before repository access.

using System.ComponentModel.DataAnnotations;
using Explore.API.Models;
using Explore.Domain.Enums;

namespace Event.Api.IntegrationTests.Features;

public sealed class PublicQueryValidationTests
{
    [Test]
    public async Task PaginationQueryRequest_WhenPaginationIsOutOfRange_IsInvalid()
    {
        var request = new PaginationQueryRequest
        {
            PageNumber = -1,
            PageSize = 101
        };

        var results = Validate(request);

        await Assert.That(results.Any(result => HasMember(result, nameof(PaginationQueryRequest.PageNumber)))).IsTrue();
        await Assert.That(results.Any(result => HasMember(result, nameof(PaginationQueryRequest.PageSize)))).IsTrue();
    }

    [Test]
    public async Task NotificationListQueryRequest_WhenLookupFilterIsNonPositive_IsInvalid()
    {
        var request = new NotificationListQueryRequest
        {
            NotificationTypeId = -1,
            NotificationScopeId = 0
        };

        var results = Validate(request);

        await Assert.That(results.Any(result => HasMember(result, nameof(NotificationListQueryRequest.NotificationTypeId)))).IsTrue();
        await Assert.That(results.Any(result => HasMember(result, nameof(NotificationListQueryRequest.NotificationScopeId)))).IsTrue();
    }

    [Test]
    public async Task ExternalApiKeyUsageReportQueryRequest_WhenDatesAreMissing_IsInvalid()
    {
        var request = new ExternalApiKeyUsageReportQueryRequest();

        var results = Validate(request);

        await Assert.That(results.Any(result => HasMember(result, nameof(ExternalApiKeyUsageReportQueryRequest.From)))).IsTrue();
        await Assert.That(results.Any(result => HasMember(result, nameof(ExternalApiKeyUsageReportQueryRequest.To)))).IsTrue();
    }

    [Test]
    public async Task ExternalApiKeyUsageReportQueryRequest_WhenDateRangeIsInverted_IsInvalid()
    {
        var request = new ExternalApiKeyUsageReportQueryRequest
        {
            From = new DateOnly(2026, 2, 1),
            To = new DateOnly(2026, 1, 31)
        };

        var results = Validate(request);

        await Assert.That(results.Any(result => HasMember(result, nameof(ExternalApiKeyUsageReportQueryRequest.From)))).IsTrue();
        await Assert.That(results.Any(result => HasMember(result, nameof(ExternalApiKeyUsageReportQueryRequest.To)))).IsTrue();
    }

    [Test]
    public async Task ExternalApiKeyUsageReportQueryRequest_WhenRangeIsTooLarge_IsInvalid()
    {
        var request = new ExternalApiKeyUsageReportQueryRequest
        {
            From = new DateOnly(2026, 1, 1),
            To = new DateOnly(2027, 1, 2)
        };

        var results = Validate(request);

        await Assert.That(results.Any(result => HasMember(result, nameof(ExternalApiKeyUsageReportQueryRequest.From)))).IsTrue();
        await Assert.That(results.Any(result => HasMember(result, nameof(ExternalApiKeyUsageReportQueryRequest.To)))).IsTrue();
    }

    [Test]
    public async Task ExternalApiKeyUsageReportQueryRequest_WhenTenantIdIsEmpty_IsInvalid()
    {
        var request = new ExternalApiKeyUsageReportQueryRequest
        {
            From = new DateOnly(2026, 1, 1),
            To = new DateOnly(2026, 1, 31),
            TenantId = Guid.Empty
        };

        var results = Validate(request);

        await Assert.That(results.Any(result => HasMember(result, nameof(ExternalApiKeyUsageReportQueryRequest.TenantId)))).IsTrue();
    }

    [Test]
    public async Task CustomPropertyGovernanceReportQueryRequest_WhenTenantScopeAndEnumAreInvalid_IsInvalid()
    {
        var request = new CustomPropertyGovernanceReportQueryRequest
        {
            TenantId = Guid.Empty,
            Scope = "Event",
            Recommendation = (PromotionRecommendation)999
        };

        var results = Validate(request);

        await Assert.That(results.Any(result => HasMember(result, nameof(CustomPropertyGovernanceReportQueryRequest.TenantId)))).IsTrue();
        await Assert.That(results.Any(result => HasMember(result, nameof(CustomPropertyGovernanceReportQueryRequest.Scope)))).IsTrue();
        await Assert.That(results.Any(result => HasMember(result, nameof(CustomPropertyGovernanceReportQueryRequest.Recommendation)))).IsTrue();
    }

    private static List<ValidationResult> Validate(object request)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(request);
        Validator.TryValidateObject(request, context, results, validateAllProperties: true);
        return results;
    }

    private static bool HasMember(ValidationResult result, string memberName)
        => result.MemberNames.Contains(memberName, StringComparer.Ordinal);
}
