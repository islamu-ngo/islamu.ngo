// ABOUTME: EF Core reads for tenant-scoped custom-property governance reports.
// ABOUTME: Loads active definition aggregates and counts their Organization or Group scope.

using Explore.Application.Contracts.Persistence;
using Explore.Domain;
using Explore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Explore.Persistence.Repositories;

public sealed class CustomPropertyGovernanceRepository(ExploreDbContext dbContext)
    : ICustomPropertyGovernanceRepository
{
    public async Task<(List<CustomPropertyDefinition> Items, int TotalCount)> GetGovernanceDefinitionsAsync(
        Guid tenantId,
        EntityTypeName? entityTypeName,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.CustomPropertyDefinitions
            .Where(definition => definition.TenantId == tenantId && definition.IsActive);

        if (entityTypeName.HasValue)
        {
            query = query.Where(definition => definition.EntityTypeName == entityTypeName.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var definitions = await query
            .AsNoTrackingWithIdentityResolution()
            .AsSplitQuery()
            .Include(definition => definition.Values)
            .OrderBy(definition => definition.EntityTypeName)
            .ThenBy(definition => definition.SortOrder)
            .ThenBy(definition => definition.DisplayName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (definitions, totalCount);
    }

    public Task<int> GetTotalEntityCountForTenantAsync(
        Guid tenantId,
        EntityTypeName entityTypeName,
        CancellationToken cancellationToken)
    {
        return entityTypeName switch
        {
            EntityTypeName.Organization => dbContext.Organizations
                .CountAsync(organization => organization.TenantId == tenantId, cancellationToken),
            EntityTypeName.Group => dbContext.Groups
                .CountAsync(group => group.TenantId == tenantId, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(entityTypeName),
                entityTypeName,
                "Custom-property governance supports Organization and Group scopes only.")
        };
    }
}
