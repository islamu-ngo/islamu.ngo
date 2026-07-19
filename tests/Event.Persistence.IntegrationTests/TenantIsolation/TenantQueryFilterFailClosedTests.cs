// ABOUTME: PostgreSQL-backed tests proving tenant query filters fail closed without ambient tenant context.
// ABOUTME: Certifies explicit tenant context and explicit bypass paths after removing null-context broad reads.

using Event.Persistence.IntegrationTests.Fixtures;
using Explore.Application.Contracts.Infrastructure;
using Explore.Domain;
using Explore.Persistence;
using Explore.Persistence.QueryFilters;
using Microsoft.EntityFrameworkCore;
using TUnit.Core;

namespace Event.Persistence.IntegrationTests.TenantIsolation;

[ClassDataSource<PostgreSqlContainerFixture>(Shared = SharedType.PerAssembly)]
[NotInParallel("PersistenceDb")]
public class TenantQueryFilterFailClosedTests(PostgreSqlContainerFixture fixture)
{
    [Test]
    public async Task TenantScopedRows_WhenTenantContextIsMissing_AreHiddenByDefault()
    {
        await fixture.ResetAsync();
        using var seedContext = fixture.CreateDbContext();
        var tenantA = await SeedOrganizationAsync(seedContext, "fail-closed-a");
        var tenantB = await SeedOrganizationAsync(seedContext, "fail-closed-b");
        var organizationIds = new[] { tenantA.OrganizationId, tenantB.OrganizationId };

        using var missingTenantContext = fixture.CreateTenantFilteredDbContext();
        var missingTenantResults = await missingTenantContext.Organizations
            .AsNoTracking()
            .Where(organization => organizationIds.Contains(organization.Id))
            .Select(organization => organization.Id)
            .ToListAsync();

        using var tenantAContext = fixture.CreateTenantFilteredDbContext(new TestTenantContext(tenantA.TenantId));
        var tenantAResults = await tenantAContext.Organizations
            .AsNoTracking()
            .Where(organization => organizationIds.Contains(organization.Id))
            .Select(organization => organization.Id)
            .ToListAsync();

        using var bypassContext = fixture.CreateTenantFilteredDbContext();
        bypassContext.EnableTenantFilterBypass("Test verifies explicit system tenant-filter bypass behavior.");
        var bypassResults = await bypassContext.Organizations
            .AsNoTracking()
            .Where(organization => organizationIds.Contains(organization.Id))
            .Select(organization => organization.Id)
            .ToListAsync();

        await Assert.That(missingTenantResults).IsEmpty();
        await Assert.That(tenantAResults).IsEquivalentTo([tenantA.OrganizationId]);
        await Assert.That(bypassResults).IsEquivalentTo(organizationIds);
    }

    [Test]
    public async Task IgnoreTenantFilter_WhenReasonIsMissing_Throws()
    {
        using var context = fixture.CreateDbContext();

        var exception = Assert.Throws<ArgumentException>(() =>
            context.Organizations.IgnoreTenantFilter(string.Empty));

        await Assert.That(exception.ParamName).IsEqualTo("reason");
    }

    private static async Task<TenantOrganizationScope> SeedOrganizationAsync(
        ExploreDbContext context,
        string slugPrefix)
    {
        var tenant = new Tenant
        {
            FullName = $"Tenant Filter {slugPrefix}",
            Slug = $"{slugPrefix}-{Guid.NewGuid().ToString("N")[..8]}",
            TenantStatusId = 2,
            TenantStatus = null!,
        };
        context.Tenants.Add(tenant);

        var organization = new Organization
        {
            Id = Guid.CreateVersion7(),
            Pii = new OrganizationPii { FullName = $"Tenant Filter Organization {slugPrefix}" },
            ApprovalStatusId = 1,
            ApprovalStatus = null!,
            TenantId = tenant.Id,
            Tenant = null!,
            ConcurrencyStamp = Guid.CreateVersion7(),
        };
        context.Organizations.Add(organization);
        await context.SaveChangesAsync();

        return new TenantOrganizationScope(tenant.Id, organization.Id);
    }

    private sealed record TestTenantContext(Guid TenantId) : ITenantContext;

    private sealed record TenantOrganizationScope(Guid TenantId, Guid OrganizationId);
}
