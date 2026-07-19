// ABOUTME: Client contract for tenant-scoped governance operations on custom-property definitions.
// ABOUTME: Wraps IIslamuApiClient with HAL unwrap + error handling so admin pages bind to plain models.

using Explore.Blazor.Client.Clients;
using Explore.Blazor.Client.Models;
using Explore.Blazor.Client.Models.CustomProperties;

namespace Explore.Blazor.Client.Contracts.Services.CustomProperties;

public interface ICustomPropertyAdminService
{
    Task<PaginatedResult<CustomPropertyDefinitionListDto>> GetDefinitionsAsync(
        EntityTypeName entityTypeName,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<CustomPropertyDefinitionDto?> GetDefinitionAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<BaseCommandResponseOfGuid?> UpdateDefinitionFlagsAsync(
        DefinitionFlagUpdateModel update,
        CancellationToken cancellationToken = default);

    Task<BaseCommandResponseOfGuid> UpdateManyDefinitionFlagsAsync(
        IReadOnlyList<DefinitionFlagUpdateModel> updates,
        CancellationToken cancellationToken = default);

    Task<PaginatedResult<CustomPropertyGovernanceRowDto>> GetGovernanceReportAsync(
        Guid? tenantId = null,
        string? scope = null,
        PromotionRecommendation? recommendation = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}
