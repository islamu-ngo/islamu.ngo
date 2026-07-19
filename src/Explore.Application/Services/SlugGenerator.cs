// ABOUTME: Generates deterministic Unicode-safe slugs from titles for routed content records.
// ABOUTME: Preserves Arabic and other letters/digits while folding whitespace and punctuation to hyphens.

using System.Globalization;
using System.Text;

namespace Explore.Application.Services;

public static class SlugGenerator
{
    public static string FromTitle(string title)
    {
        ArgumentNullException.ThrowIfNull(title);

        var slug = new StringBuilder(title.Length);
        bool needsSeparator = false;

        foreach (char character in title.Trim().Normalize(NormalizationForm.FormD))
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                if (needsSeparator && slug.Length > 0)
                {
                    slug.Append('-');
                }

                slug.Append(char.ToLowerInvariant(character));
                needsSeparator = false;
                continue;
            }

            if (char.IsWhiteSpace(character) || IsSeparator(category))
            {
                needsSeparator = slug.Length > 0;
            }
        }

        return slug.ToString().Normalize(NormalizationForm.FormC);
    }

    private static bool IsSeparator(UnicodeCategory category)
    {
        return category is UnicodeCategory.ConnectorPunctuation
            or UnicodeCategory.DashPunctuation
            or UnicodeCategory.OpenPunctuation
            or UnicodeCategory.ClosePunctuation
            or UnicodeCategory.InitialQuotePunctuation
            or UnicodeCategory.FinalQuotePunctuation
            or UnicodeCategory.OtherPunctuation
            or UnicodeCategory.MathSymbol
            or UnicodeCategory.CurrencySymbol
            or UnicodeCategory.ModifierSymbol
            or UnicodeCategory.OtherSymbol;
    }
}
