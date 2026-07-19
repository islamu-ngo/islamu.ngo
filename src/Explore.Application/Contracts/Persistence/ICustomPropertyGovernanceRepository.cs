// ABOUTME: Repository contract for tenant-scoped custom-property governance report reads.
// ABOUTME: Returns definition entities and scope totals so handlers own report DTO mapping.

using Explore.Domain;
using Explore.Domain.Enums;

namespace Explore.Application.Contracts.Persistence;

public interface ICustomPropertyGovernanceRepository
{
    Task<(List<CustomPropertyDefinition> Items, int TotalCount)> GetGovernanceDefinitionsAsync(
        Guid tenantId,
        EntityTypeName? entityTypeName,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> GetTotalEntityCountForTenantAsync(
        Guid tenantId,
        EntityTypeName entityTypeName,
        CancellationToken cancellationToken);
}
