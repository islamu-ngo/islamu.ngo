// ABOUTME: Handles tenant-scoped custom-property governance reports and promotion recommendations.
// ABOUTME: Measures adoption against each definition's Organization or Group scope.

using Explore.Application.Contracts.Persistence;
using Explore.Application.DTOs.CustomPropertyGovernance;
using Explore.Application.Features.CustomPropertyGovernance.Requests.Queries;
using Explore.Application.Responses;
using Explore.Domain;
using Explore.Domain.Enums;
using MediatR;

namespace Explore.Application.Features.CustomPropertyGovernance.Handlers.Queries;

public class GetCustomPropertyGovernanceReportQueryHandler
    : IRequestHandler<GetCustomPropertyGovernanceReportQuery, PaginatedResult<CustomPropertyGovernanceRowDto>>
{
    private readonly ICustomPropertyGovernanceRepository _governanceRepository;

    public GetCustomPropertyGovernanceReportQueryHandler(ICustomPropertyGovernanceRepository governanceRepository)
    {
        _governanceRepository = governanceRepository;
    }

    public async Task<PaginatedResult<CustomPropertyGovernanceRowDto>> Handle(
        GetCustomPropertyGovernanceReportQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PaginatedResult<CustomPropertyGovernanceRowDto>
            .NormalizeParameters(request.Filter.PageNumber, request.Filter.PageSize);

        var (definitions, totalCount) = await _governanceRepository.GetGovernanceDefinitionsAsync(
            request.TenantId,
            ParseEntityScope(request.Filter.EntityScope),
            pageNumber,
            pageSize,
            cancellationToken);

        var entityCounts = new Dictionary<EntityTypeName, int>();
        foreach (var entityTypeName in definitions.Select(definition => definition.EntityTypeName).Distinct())
        {
            entityCounts[entityTypeName] = await _governanceRepository.GetTotalEntityCountForTenantAsync(
                request.TenantId,
                entityTypeName,
                cancellationToken);
        }

        var dtos = new List<CustomPropertyGovernanceRowDto>(definitions.Count);

        foreach (var definition in definitions)
        {
            var activeValues = definition.Values.Where(value => !value.IsDeleted).ToList();
            var activeInstanceCount = activeValues
                .Select(value => value.EntityId)
                .Distinct()
                .Count();
            var recommendation = ComputeRecommendation(
                definition,
                activeInstanceCount,
                entityCounts[definition.EntityTypeName]);

            if (request.Filter.Recommendation.HasValue && recommendation != request.Filter.Recommendation.Value)
                continue;

            dtos.Add(new CustomPropertyGovernanceRowDto
            {
                TenantId = definition.TenantId,
                Namespace = definition.Namespace,
                Key = definition.Key,
                DisplayName = definition.DisplayName,
                EntityScope = definition.EntityTypeName.ToString(),
                PropertyType = definition.PropertyType.ToString(),
                ExposureLevel = definition.ExposureLevel,
                IsSearchable = definition.IsSearchable,
                IsFilterable = definition.IsFilterable,
                IsExportable = definition.IsExportable,
                IsModerationRelevant = definition.IsModerationRelevant,
                IsAnalyticsRelevant = definition.IsAnalyticsRelevant,
                IsSystemOwned = definition.IsSystemOwned,
                ActiveInstanceCount = activeInstanceCount,
                LastUsedAt = activeValues
                    .Select(value => value.UpdatedAt ?? value.CreatedAt)
                    .Cast<DateTime?>()
                    .Max(),
                Recommendation = recommendation
            });
        }

        return PaginatedResult<CustomPropertyGovernanceRowDto>.Create(
            dtos, totalCount, pageNumber, pageSize);
    }

    private static EntityTypeName? ParseEntityScope(string? entityScope)
    {
        return Enum.TryParse<EntityTypeName>(entityScope, true, out var parsedScope)
            && Enum.IsDefined(parsedScope)
                ? parsedScope
                : null;
    }

    /// <summary>
    /// Computes the promotion recommendation using the Atlassian 4-question matrix:
    /// 1. Is it used for search/filter? → ConsiderProjectionFirst
    /// 2. Is it used for moderation/analytics (automation/AI/cross-tenant reporting)? → ConsiderLayer2Promotion
    /// 3. Is it used for moderation AND search AND widely adopted (at least 30% of in-scope entities)? → ConsiderLayer1Promotion
    /// </summary>
    public static PromotionRecommendation ComputeRecommendation(
        CustomPropertyDefinition definition,
        int activeInstanceCount,
        int totalEntityCount)
    {
        var hasSearchFilter = definition.IsSearchable || definition.IsFilterable;
        var hasModerationOrAnalytics = definition.IsModerationRelevant || definition.IsAnalyticsRelevant;
        var adoptionThresholdPct = 30;
        var isWidelyAdopted = totalEntityCount > 0
            && ((long)activeInstanceCount * 100 / totalEntityCount) >= adoptionThresholdPct;

        if (definition.IsModerationRelevant && hasSearchFilter && isWidelyAdopted)
            return PromotionRecommendation.ConsiderLayer1Promotion;

        if (hasModerationOrAnalytics)
            return PromotionRecommendation.ConsiderLayer2Promotion;

        if (hasSearchFilter)
            return PromotionRecommendation.ConsiderProjectionFirst;

        return PromotionRecommendation.None;
    }
}
