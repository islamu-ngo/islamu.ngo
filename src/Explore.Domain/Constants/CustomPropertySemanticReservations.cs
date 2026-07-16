// ABOUTME: Layer-2 semantic reservation guard for custom property governance.
// ABOUTME: Provides IsReservedLayer2Semantic helper used by CustomPropertyGovernancePolicy.

using System.Collections.Immutable;

namespace Explore.Domain.Constants;

public static class CustomPropertySemanticReservations
{
    private static readonly ImmutableArray<string> Layer2GenericSemantics = ImmutableArray<string>.Empty;

    public static bool IsReservedLayer2Semantic(string ns, string key)
    {
        if (!CustomPropertyNamespaces.IsSector(ns))
        {
            return false;
        }

        if (key.Contains('.'))
        {
            var parts = key.Split('.');
            var parentKey = parts[0];
            return IsReservedLayer2Semantic(ns, parentKey);
        }

        return Layer2GenericSemantics.Contains(key);
    }
}
