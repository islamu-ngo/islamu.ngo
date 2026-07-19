// ABOUTME: Query-bound request models for shared paginated API list validation.
// ABOUTME: Converts abusive page/filter query parameters into early ApiController validation failures.

using System.ComponentModel.DataAnnotations;
using Explore.Domain.Enums;

namespace Explore.API.Models;

public class PaginationQueryRequest : IValidatableObject
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        => QueryValidationRules.ValidatePagination(PageNumber, PageSize);
}

public sealed class CustomPropertyDefinitionListQueryRequest : PaginationQueryRequest
{
    public EntityTypeName EntityTypeName { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in base.Validate(validationContext))
        {
            yield return result;
        }

        if (!Enum.IsDefined(EntityTypeName))
        {
            yield return new ValidationResult(
                $"{nameof(EntityTypeName)} is not supported.",
                [nameof(EntityTypeName)]);
        }
    }
}

public sealed class NotificationListQueryRequest : PaginationQueryRequest
{
    public bool? IsRead { get; set; }

    public int? NotificationTypeId { get; set; }

    public int? NotificationScopeId { get; set; }

    public int? NotificationReasonId { get; set; }

    public bool? IsArchived { get; set; }

    public bool? IsSnoozed { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        => base.Validate(validationContext)
            .Concat(QueryValidationRules.ValidateOptionalPositiveInt(
                NotificationTypeId,
                nameof(NotificationTypeId)))
            .Concat(QueryValidationRules.ValidateOptionalPositiveInt(
                NotificationScopeId,
                nameof(NotificationScopeId)))
            .Concat(QueryValidationRules.ValidateOptionalPositiveInt(
                NotificationReasonId,
                nameof(NotificationReasonId)));
}

public sealed class ExternalApiKeyUsageReportQueryRequest : IValidatableObject
{
    public const int MaxRangeDays = 366;

    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public Guid? TenantId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        => QueryValidationRules.ValidateRequiredDateRange(
                From,
                To,
                nameof(From),
                nameof(To),
                MaxRangeDays)
            .Concat(QueryValidationRules.ValidateOptionalGuid(TenantId, nameof(TenantId)));
}

public sealed class EmailDispatchStatusQueryRequest : IValidatableObject
{
    public const int MaxLimit = 200;

    public Guid TenantId { get; set; }

    public int Limit { get; set; } = 50;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in QueryValidationRules.ValidateRequiredGuid(TenantId, nameof(TenantId)))
        {
            yield return result;
        }

        if (Limit is < 1 or > MaxLimit)
        {
            yield return new ValidationResult(
                $"{nameof(Limit)} must be between 1 and {MaxLimit}.",
                [nameof(Limit)]);
        }
    }
}

public sealed class EmailDispatchPauseTenantQueryRequest : IValidatableObject
{
    public const int MaxReasonLength = 500;

    public string? Reason { get; set; }

    public string? GetNormalizedReason() => string.IsNullOrWhiteSpace(Reason) ? null : Reason.Trim();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        => QueryValidationRules.ValidateBoundedText(Reason, nameof(Reason), MaxReasonLength);
}

public sealed class EmailDispatchParkQueryRequest : IValidatableObject
{
    public const int MaxReasonLength = 500;

    public string? Reason { get; set; }

    public string GetNormalizedReason() => Reason?.Trim() ?? string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Reason))
        {
            yield return new ValidationResult(
                $"{nameof(Reason)} is required.",
                [nameof(Reason)]);
            yield break;
        }

        foreach (var result in QueryValidationRules.ValidateBoundedText(Reason, nameof(Reason), MaxReasonLength))
        {
            yield return result;
        }
    }
}

public sealed class CustomPropertyGovernanceReportQueryRequest : PaginationQueryRequest
{
    public Guid TenantId { get; set; }

    public string? Scope { get; set; }

    public PromotionRecommendation? Recommendation { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in base.Validate(validationContext))
        {
            yield return result;
        }

        foreach (var result in QueryValidationRules.ValidateRequiredGuid(TenantId, nameof(TenantId)))
        {
            yield return result;
        }

        foreach (var result in QueryValidationRules.ValidateBoundedText(
                     Scope,
                     nameof(Scope),
                     QueryValidationRules.MaxShortTextLength))
        {
            yield return result;
        }

        if (Scope is not null
            && !string.Equals(Scope, nameof(EntityTypeName.Organization), StringComparison.OrdinalIgnoreCase)
            && !string.Equals(Scope, nameof(EntityTypeName.Group), StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                $"{nameof(Scope)} must be Organization or Group.",
                [nameof(Scope)]);
        }

        if (Recommendation.HasValue && !Enum.IsDefined(Recommendation.Value))
        {
            yield return new ValidationResult(
                $"{nameof(Recommendation)} is not supported.",
                [nameof(Recommendation)]);
        }
    }
}
