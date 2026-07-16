// ABOUTME: HATEOAS link policies for custom-property governance report endpoints.
// ABOUTME: Provides discovery links for the custom-property governance report.

using System.Security.Claims;
using Explore.Application.Contracts.Hateoas;
using Explore.Application.DTOs.CustomPropertyGovernance;
using Explore.Application.Hateoas;

namespace Explore.API.Hateoas.Policies;

/// <summary>
/// Link policy for CustomPropertyGovernanceRowDto collection items.
/// Rows are embedded in the governance report collection.
/// </summary>
public sealed class CustomPropertyGovernanceCollectionLinkPolicy : ICollectionLinkPolicy<CustomPropertyGovernanceRowDto>
{
    public IEnumerable<LinkDefinition> GetItemLinks(CustomPropertyGovernanceRowDto dto, ClaimsPrincipal? user) => [];

    public IEnumerable<LinkDefinition> GetCollectionLinks(ClaimsPrincipal? user)
    {
        yield return LinkDefinition.Self(
            RouteNames.GetCustomPropertyGovernanceReport);
    }
}
