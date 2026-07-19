// ABOUTME: PostgreSQL-backed tests for custom-property governance adoption reporting.
// ABOUTME: Verifies Organization and Group definitions use distinct in-scope entity counts.

using Event.Persistence.IntegrationTests.Fixtures;
using Explore.Application.DTOs.CustomPropertyGovernance;
using Explore.Application.Features.CustomPropertyGovernance.Handlers.Queries;
using Explore.Application.Features.CustomPropertyGovernance.Requests.Queries;
using Explore.Domain;
using Explore.Domain.Enums;
using Explore.Persistence.Repositories;

namespace Event.Persistence.IntegrationTests.Repositories;

[ClassDataSource<PostgreSqlContainerFixture>(Shared = SharedType.PerAssembly)]
[NotInParallel("PersistenceDb")]
public sealed class CustomPropertyGovernanceRepositoryTests(PostgreSqlContainerFixture fixture)
{
    [Test]
    public async Task Report_UsesDistinctOrganizationAndGroupAdoptionDenominators()
    {
        await fixture.ResetAsync();
        await using var context = fixture.CreateDbContext();
        var tenant = CreateTenant("governance");
        var otherTenant = CreateTenant("other-governance");
        context.Tenants.AddRange(tenant, otherTenant);
        await context.SaveChangesAsync();

        var organizations = new[]
        {
            CreateOrganization(tenant.Id, "Organization One"),
            CreateOrganization(tenant.Id, "Organization Two"),
            CreateOrganization(otherTenant.Id, "Other Organization")
        };
        var groups = new[]
        {
            CreateGroup(tenant.Id, "Group One"),
            CreateGroup(tenant.Id, "Group Two"),
            CreateGroup(tenant.Id, "Group Three"),
            CreateGroup(tenant.Id, "Group Four"),
            CreateGroup(otherTenant.Id, "Other Group")
        };
        context.Organizations.AddRange(organizations);
        context.Groups.AddRange(groups);
        await context.SaveChangesAsync();

        var organizationDefinition = CreateDefinition(tenant.Id, EntityTypeName.Organization, "organization-risk");
        var groupDefinition = CreateDefinition(tenant.Id, EntityTypeName.Group, "group-risk");
        var inactiveDefinition = CreateDefinition(tenant.Id, EntityTypeName.Organization, "inactive-risk");
        inactiveDefinition.IsActive = false;
        var otherTenantDefinition = CreateDefinition(otherTenant.Id, EntityTypeName.Organization, "other-risk");
        context.CustomPropertyDefinitions.AddRange(
            organizationDefinition,
            groupDefinition,
            inactiveDefinition,
            otherTenantDefinition);
        await context.SaveChangesAsync();

        var latestUsage = new DateTime(2026, 7, 18, 12, 30, 0, DateTimeKind.Utc);
        context.CustomPropertyValues.AddRange(
            CreateValue(tenant.Id, organizationDefinition.Id, organizations[0].Id, 0, latestUsage.AddHours(-1)),
            CreateValue(tenant.Id, organizationDefinition.Id, organizations[0].Id, 1, latestUsage),
            CreateValue(tenant.Id, groupDefinition.Id, groups[0].Id, 0, latestUsage.AddHours(-2)),
            CreateValue(otherTenant.Id, otherTenantDefinition.Id, organizations[2].Id, 0, latestUsage.AddHours(1)));
        await context.SaveChangesAsync();

        var repository = new CustomPropertyGovernanceRepository(context);
        var handler = new GetCustomPropertyGovernanceReportQueryHandler(repository);

        var result = await handler.Handle(
            new GetCustomPropertyGovernanceReportQuery
            {
                TenantId = tenant.Id,
                Filter = new GovernanceReportFilterDto()
            },
            CancellationToken.None);

        await Assert.That(result.TotalCount).IsEqualTo(2);
        var organizationRow = result.Items.Single(row => row.EntityScope == nameof(EntityTypeName.Organization));
        await Assert.That(organizationRow.ActiveInstanceCount).IsEqualTo(1);
        await Assert.That(organizationRow.LastUsedAt).IsEqualTo(latestUsage);
        await Assert.That(organizationRow.Recommendation).IsEqualTo(PromotionRecommendation.ConsiderLayer1Promotion);

        var groupRow = result.Items.Single(row => row.EntityScope == nameof(EntityTypeName.Group));
        await Assert.That(groupRow.ActiveInstanceCount).IsEqualTo(1);
        await Assert.That(groupRow.Recommendation).IsEqualTo(PromotionRecommendation.ConsiderLayer2Promotion);
    }

    private static Tenant CreateTenant(string slugPrefix)
    {
        return new Tenant
        {
            FullName = $"Governance {slugPrefix}",
            Slug = $"{slugPrefix}-{Guid.NewGuid():N}",
            TenantStatusId = 2,
            TenantStatus = null!
        };
    }

    private static Organization CreateOrganization(Guid tenantId, string fullName)
    {
        return new Organization
        {
            Pii = new OrganizationPii { FullName = fullName },
            ApprovalStatusId = 1,
            ApprovalStatus = null!,
            TenantId = tenantId,
            Tenant = null!,
            ConcurrencyStamp = Guid.CreateVersion7()
        };
    }

    private static Group CreateGroup(Guid tenantId, string fullName)
    {
        return new Group
        {
            FullName = fullName,
            ApprovalStatusId = 1,
            ApprovalStatus = null!,
            TenantId = tenantId,
            Tenant = null!,
            ConcurrencyStamp = Guid.CreateVersion7()
        };
    }

    private static CustomPropertyDefinition CreateDefinition(
        Guid tenantId,
        EntityTypeName entityTypeName,
        string key)
    {
        return new CustomPropertyDefinition
        {
            TenantId = tenantId,
            EntityTypeName = entityTypeName,
            Namespace = "tenant.custom",
            Key = key,
            DisplayName = key,
            PropertyType = PropertyType.Text,
            ExposureLevel = ExposureLevel.Public,
            IsActive = true,
            IsMulti = true,
            IsSearchable = true,
            IsModerationRelevant = true
        };
    }

    private static CustomPropertyValue CreateValue(
        Guid tenantId,
        Guid definitionId,
        Guid entityId,
        int ordinal,
        DateTime updatedAt)
    {
        return new CustomPropertyValue
        {
            TenantId = tenantId,
            CustomPropertyDefinitionId = definitionId,
            EntityId = entityId,
            Ordinal = ordinal,
            TextValue = $"value-{ordinal}",
            CreatedAt = updatedAt.AddDays(-1),
            UpdatedAt = updatedAt,
            ConcurrencyStamp = Guid.CreateVersion7()
        };
    }
}
