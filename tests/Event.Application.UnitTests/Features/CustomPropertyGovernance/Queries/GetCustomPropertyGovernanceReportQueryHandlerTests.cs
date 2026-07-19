// ABOUTME: Unit tests for scope-aware custom-property governance recommendations and DTO mapping.
// ABOUTME: Verifies Organization and Group adoption use their matching entity totals.

using Explore.Application.Contracts.Persistence;
using Explore.Application.DTOs.CustomPropertyGovernance;
using Explore.Application.Features.CustomPropertyGovernance.Handlers.Queries;
using Explore.Application.Features.CustomPropertyGovernance.Requests.Queries;
using Explore.Domain;
using Explore.Domain.Enums;
using NSubstitute;

namespace Event.Application.UnitTests.Features.CustomPropertyGovernance.Queries;

public class GetCustomPropertyGovernanceReportQueryHandlerTests
{
    private readonly ICustomPropertyGovernanceRepository _governanceRepo;
    private readonly GetCustomPropertyGovernanceReportQueryHandler _handler;

    public GetCustomPropertyGovernanceReportQueryHandlerTests()
    {
        _governanceRepo = Substitute.For<ICustomPropertyGovernanceRepository>();
        _handler = new GetCustomPropertyGovernanceReportQueryHandler(_governanceRepo);
    }

    // ── Promotion Recommendation Matrix Tests ──────────────────────────────

    [Test]
    public async Task ComputeRecommendation_NoneOfThe4Questions_ReturnsNone()
    {
        var row = CreateRow(isSearchable: false, isFilterable: false,
            isModerationRelevant: false, isAnalyticsRelevant: false);

        var result = GetCustomPropertyGovernanceReportQueryHandler.ComputeRecommendation(row, 0, 100);

        await Assert.That(result).IsEqualTo(PromotionRecommendation.None);
    }

    [Test]
    public async Task ComputeRecommendation_IsSearchable_ReturnsConsiderProjectionFirst()
    {
        var row = CreateRow(isSearchable: true, isFilterable: false,
            isModerationRelevant: false, isAnalyticsRelevant: false);

        var result = GetCustomPropertyGovernanceReportQueryHandler.ComputeRecommendation(row, 5, 100);

        await Assert.That(result).IsEqualTo(PromotionRecommendation.ConsiderProjectionFirst);
    }

    [Test]
    public async Task ComputeRecommendation_IsFilterable_ReturnsConsiderProjectionFirst()
    {
        var row = CreateRow(isSearchable: false, isFilterable: true,
            isModerationRelevant: false, isAnalyticsRelevant: false);

        var result = GetCustomPropertyGovernanceReportQueryHandler.ComputeRecommendation(row, 5, 100);

        await Assert.That(result).IsEqualTo(PromotionRecommendation.ConsiderProjectionFirst);
    }

    [Test]
    public async Task ComputeRecommendation_IsModerationRelevant_ReturnsConsiderLayer2()
    {
        var row = CreateRow(isSearchable: false, isFilterable: false,
            isModerationRelevant: true, isAnalyticsRelevant: false);

        var result = GetCustomPropertyGovernanceReportQueryHandler.ComputeRecommendation(row, 5, 100);

        await Assert.That(result).IsEqualTo(PromotionRecommendation.ConsiderLayer2Promotion);
    }

    [Test]
    public async Task ComputeRecommendation_IsAnalyticsRelevant_ReturnsConsiderLayer2()
    {
        var row = CreateRow(isSearchable: false, isFilterable: false,
            isModerationRelevant: false, isAnalyticsRelevant: true);

        var result = GetCustomPropertyGovernanceReportQueryHandler.ComputeRecommendation(row, 5, 100);

        await Assert.That(result).IsEqualTo(PromotionRecommendation.ConsiderLayer2Promotion);
    }

    [Test]
    public async Task ComputeRecommendation_ModerationAndSearchAndWidelyAdopted_ReturnsConsiderLayer1()
    {
        var row = CreateRow(isSearchable: true, isFilterable: false,
            isModerationRelevant: true, isAnalyticsRelevant: false);

        var result = GetCustomPropertyGovernanceReportQueryHandler.ComputeRecommendation(row, 40, 100);

        await Assert.That(result).IsEqualTo(PromotionRecommendation.ConsiderLayer1Promotion);
    }

    [Test]
    public async Task ComputeRecommendation_ModerationAndFilterAndWidelyAdopted_ReturnsConsiderLayer1()
    {
        var row = CreateRow(isSearchable: false, isFilterable: true,
            isModerationRelevant: true, isAnalyticsRelevant: false);

        var result = GetCustomPropertyGovernanceReportQueryHandler.ComputeRecommendation(row, 35, 100);

        await Assert.That(result).IsEqualTo(PromotionRecommendation.ConsiderLayer1Promotion);
    }

    [Test]
    public async Task ComputeRecommendation_ModerationAndSearchButNotWidelyAdopted_ReturnsConsiderLayer2()
    {
        var row = CreateRow(isSearchable: true, isFilterable: false,
            isModerationRelevant: true, isAnalyticsRelevant: false);

        var result = GetCustomPropertyGovernanceReportQueryHandler.ComputeRecommendation(row, 20, 100);

        await Assert.That(result).IsEqualTo(PromotionRecommendation.ConsiderLayer2Promotion);
    }

    [Test]
    public async Task ComputeRecommendation_ExactlyAtThreshold_ReturnsConsiderLayer1()
    {
        var row = CreateRow(isSearchable: true, isFilterable: false,
            isModerationRelevant: true, isAnalyticsRelevant: false);

        var result = GetCustomPropertyGovernanceReportQueryHandler.ComputeRecommendation(row, 30, 100);

        await Assert.That(result).IsEqualTo(PromotionRecommendation.ConsiderLayer1Promotion);
    }

    [Test]
    public async Task ComputeRecommendation_ZeroTotalEntities_NeverReturnsLayer1()
    {
        var row = CreateRow(isSearchable: true, isFilterable: false,
            isModerationRelevant: true, isAnalyticsRelevant: false);

        var result = GetCustomPropertyGovernanceReportQueryHandler.ComputeRecommendation(row, 10, 0);

        await Assert.That(result).IsEqualTo(PromotionRecommendation.ConsiderLayer2Promotion);
    }

    [Test]
    public async Task ComputeRecommendation_ModerationPrecedesProjection()
    {
        var row = CreateRow(isSearchable: true, isFilterable: true,
            isModerationRelevant: true, isAnalyticsRelevant: false);

        var result = GetCustomPropertyGovernanceReportQueryHandler.ComputeRecommendation(row, 5, 100);

        await Assert.That(result).IsEqualTo(PromotionRecommendation.ConsiderLayer2Promotion);
    }

    // ── Handler Integration Tests ──────────────────────────────────────────

    [Test]
    public async Task Handle_ReturnsPagedResults()
    {
        var tenantId = Guid.NewGuid();
        var rows = new List<CustomPropertyDefinition>
        {
            CreateRow(isSearchable: true),
            CreateRow(
                isSearchable: false,
                isModerationRelevant: true,
                entityTypeName: EntityTypeName.Group),
        };

        _governanceRepo
            .GetGovernanceDefinitionsAsync(tenantId, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((rows, 2));
        _governanceRepo
            .GetTotalEntityCountForTenantAsync(tenantId, Arg.Any<EntityTypeName>(), Arg.Any<CancellationToken>())
            .Returns(100);

        var query = new GetCustomPropertyGovernanceReportQuery
        {
            TenantId = tenantId,
            Filter = new GovernanceReportFilterDto()
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        await Assert.That(result.Items.Count).IsEqualTo(2);
        await Assert.That(result.Items[0].Recommendation).IsEqualTo(PromotionRecommendation.ConsiderProjectionFirst);
        await Assert.That(result.Items[1].Recommendation).IsEqualTo(PromotionRecommendation.ConsiderLayer2Promotion);
        await _governanceRepo.Received(1).GetTotalEntityCountForTenantAsync(
            tenantId,
            EntityTypeName.Organization,
            Arg.Any<CancellationToken>());
        await _governanceRepo.Received(1).GetTotalEntityCountForTenantAsync(
            tenantId,
            EntityTypeName.Group,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WithRecommendationFilter_FiltersResults()
    {
        var tenantId = Guid.NewGuid();
        var rows = new List<CustomPropertyDefinition>
        {
            CreateRow(isSearchable: true),
            CreateRow(isSearchable: false, isModerationRelevant: true),
            CreateRow(isSearchable: false),
        };

        _governanceRepo
            .GetGovernanceDefinitionsAsync(tenantId, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((rows, 3));
        _governanceRepo
            .GetTotalEntityCountForTenantAsync(tenantId, EntityTypeName.Organization, Arg.Any<CancellationToken>())
            .Returns(100);

        var query = new GetCustomPropertyGovernanceReportQuery
        {
            TenantId = tenantId,
            Filter = new GovernanceReportFilterDto { Recommendation = PromotionRecommendation.ConsiderProjectionFirst }
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        await Assert.That(result.Items.Count).IsEqualTo(1);
        await Assert.That(result.Items[0].Recommendation).IsEqualTo(PromotionRecommendation.ConsiderProjectionFirst);
    }

    [Test]
    public async Task Handle_MapsExposureAndGovernanceFlags()
    {
        var tenantId = Guid.NewGuid();
        var rows = new List<CustomPropertyDefinition>
        {
            CreateRow(
                isSearchable: true,
                isFilterable: true,
                isExportable: true,
                isModerationRelevant: true,
                isAnalyticsRelevant: true,
                exposureLevel: ExposureLevel.TenantAdminOnly)
        };

        _governanceRepo
            .GetGovernanceDefinitionsAsync(tenantId, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((rows, 1));
        _governanceRepo
            .GetTotalEntityCountForTenantAsync(tenantId, EntityTypeName.Organization, Arg.Any<CancellationToken>())
            .Returns(100);

        var result = await _handler.Handle(
            new GetCustomPropertyGovernanceReportQuery
            {
                TenantId = tenantId,
                Filter = new GovernanceReportFilterDto()
            },
            CancellationToken.None);

        var item = result.Items.Single();
        await Assert.That(item.ExposureLevel).IsEqualTo(ExposureLevel.TenantAdminOnly);
        await Assert.That(item.IsSearchable).IsTrue();
        await Assert.That(item.IsFilterable).IsTrue();
        await Assert.That(item.IsExportable).IsTrue();
        await Assert.That(item.IsModerationRelevant).IsTrue();
        await Assert.That(item.IsAnalyticsRelevant).IsTrue();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static CustomPropertyDefinition CreateRow(
        bool isSearchable = false,
        bool isFilterable = false,
        bool isExportable = false,
        bool isModerationRelevant = false,
        bool isAnalyticsRelevant = false,
        ExposureLevel exposureLevel = ExposureLevel.Public,
        EntityTypeName entityTypeName = EntityTypeName.Organization)
    {
        return new CustomPropertyDefinition
        {
            TenantId = Guid.NewGuid(),
            Namespace = "tenant.custom",
            Key = $"test-{Guid.NewGuid():N}",
            DisplayName = "Test Definition",
            EntityTypeName = entityTypeName,
            PropertyType = PropertyType.Text,
            ExposureLevel = exposureLevel,
            IsActive = true,
            IsSearchable = isSearchable,
            IsFilterable = isFilterable,
            IsExportable = isExportable,
            IsModerationRelevant = isModerationRelevant,
            IsAnalyticsRelevant = isAnalyticsRelevant,
            IsSystemOwned = false
        };
    }
}
