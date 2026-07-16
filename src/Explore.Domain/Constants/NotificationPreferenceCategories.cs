// ABOUTME: String constants for notification preference category codes used by unsubscribe services.
// ABOUTME: Bridges generic DB lookup rows with runtime normalization/validation; no product-specific categories.

using Explore.Domain.Enums;

namespace Explore.Domain.Constants;

public static class NotificationPreferenceCategories
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        // Generic categories (from NotificationPreferenceCategoryCodes) that the unsubscribe services must recognize
        NotificationPreferenceCategoryCodes.AccountSecurity,
        NotificationPreferenceCategoryCodes.BillingLegal,
        NotificationPreferenceCategoryCodes.OrganizationUpdates,
        NotificationPreferenceCategoryCodes.GroupUpdates,
        NotificationPreferenceCategoryCodes.TrustSafety,
        NotificationPreferenceCategoryCodes.ProductAnnouncements,
        NotificationPreferenceCategoryCodes.Marketing,
    };

    public static bool IsKnown(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return All.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        foreach (var known in All)
        {
            if (known.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
            {
                return known;
            }
        }

        return null;
    }
}
