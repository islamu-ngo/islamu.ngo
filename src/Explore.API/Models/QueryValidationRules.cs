// ABOUTME: Shared syntactic validation rules for public API query-binding models.
// ABOUTME: Keeps high-risk list, search, sort, date-range, and custom-property query checks consistent.

using System.ComponentModel.DataAnnotations;
using Explore.Application.Responses;

namespace Explore.API.Models;

internal static class QueryValidationRules
{
    private const string PageNumberMemberName = "PageNumber";
    private const string PageSizeMemberName = "PageSize";
    public const int MaxShortTextLength = 100;

    public static IEnumerable<ValidationResult> ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            yield return new ValidationResult(
                "PageNumber must be greater than or equal to 1.",
                [PageNumberMemberName]);
        }

        if (pageSize is < 1 or > PaginatedResult<object>.MaxPageSize)
        {
            yield return new ValidationResult(
                $"PageSize must be between 1 and {PaginatedResult<object>.MaxPageSize}.",
                [PageSizeMemberName]);
        }
    }

    public static IEnumerable<ValidationResult> ValidateBoundedText(
        string? value,
        string memberName,
        int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            yield break;
        }

        if (value.Length > maxLength)
        {
            yield return new ValidationResult(
                $"{memberName} must not exceed {maxLength} characters.",
                [memberName]);
        }

        if (ContainsControlCharacter(value))
        {
            yield return new ValidationResult(
                $"{memberName} contains unsupported control characters.",
                [memberName]);
        }
    }

    public static IEnumerable<ValidationResult> ValidateRequiredDateRange(
        DateOnly from,
        DateOnly to,
        string fromMemberName,
        string toMemberName,
        int maxRangeDays)
    {
        var hasFrom = from != default;
        var hasTo = to != default;

        if (!hasFrom)
        {
            yield return new ValidationResult(
                $"{fromMemberName} is required.",
                [fromMemberName]);
        }

        if (!hasTo)
        {
            yield return new ValidationResult(
                $"{toMemberName} is required.",
                [toMemberName]);
        }

        if (!hasFrom || !hasTo)
        {
            yield break;
        }

        if (from > to)
        {
            yield return new ValidationResult(
                $"{fromMemberName} must be earlier than or equal to {toMemberName}.",
                [fromMemberName, toMemberName]);
            yield break;
        }

        var rangeDays = to.DayNumber - from.DayNumber + 1;
        if (rangeDays > maxRangeDays)
        {
            yield return new ValidationResult(
                $"{fromMemberName} and {toMemberName} must span at most {maxRangeDays} days.",
                [fromMemberName, toMemberName]);
        }
    }

    public static IEnumerable<ValidationResult> ValidateOptionalGuid(Guid? value, string memberName)
    {
        if (value == Guid.Empty)
        {
            yield return new ValidationResult(
                $"{memberName} must not be an empty GUID.",
                [memberName]);
        }
    }

    public static IEnumerable<ValidationResult> ValidateRequiredGuid(Guid value, string memberName)
    {
        if (value == Guid.Empty)
        {
            yield return new ValidationResult(
                $"{memberName} is required.",
                [memberName]);
        }
    }

    public static IEnumerable<ValidationResult> ValidateOptionalPositiveInt(int? value, string memberName)
    {
        if (value is <= 0)
        {
            yield return new ValidationResult(
                $"{memberName} must be greater than 0.",
                [memberName]);
        }
    }

    private static bool ContainsControlCharacter(string value)
        => value.Any(char.IsControl);
}
