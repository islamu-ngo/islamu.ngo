// ABOUTME: Business-readable scenario seed for tenant contexts in integration tests.
// ABOUTME: Creates a complete tenant + user + actor graph for test scenarios.

using Event.Api.IntegrationTests.Builders;
using Explore.Domain;
using Explore.Domain.Constants;
using Explore.Domain.Enums;
using Explore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Event.Api.IntegrationTests.Seeds;

/// <summary>
/// Seeds complete tenant contexts (tenant + user + actor) for integration test scenarios.
/// Uses the two-phase save pattern required by the User → Actor circular dependency.
/// </summary>
public static class TenantScenarioSeed
{
    /// <summary>Carries the IDs of all entities created by a tenant scenario seed.</summary>
    public sealed record TenantScenarioResult(Guid TenantId, Guid UserId, Guid ActorId);

    /// <summary>Carries the IDs for a tenant user that can publish events for an organization.</summary>
    public sealed record TenantOrganizationScenarioResult(
        Guid TenantId,
        Guid UserId,
        Guid ActorId,
        Guid OrganizationId,
        Guid OrganizationActorId);

    /// <summary>
    /// Seeds an active tenant with a user and user-type actor.
    /// Uses <see cref="PlatformDefaults.DefaultTenantId"/> for single-tenant API compatibility.
    /// </summary>
    public static async Task<TenantScenarioResult> SeedActiveTenantWithUserAsync(ExploreDbContext context)
    {
        var tenant = await context.Tenants.FindAsync(PlatformDefaults.DefaultTenantId);
        if (tenant is null)
        {
            tenant = new TenantBuilder()
                .WithId(PlatformDefaults.DefaultTenantId)
                .WithFullName("Default Test Tenant")
                .WithSlug("default-test")
                .Build();
            context.Tenants.Add(tenant);
        }

        var user = new UserBuilder().Build();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var actor = new ActorBuilder()
            .WithTenantId(tenant.Id)
            .WithUserId(user.Id)
            .Build();
        context.Actors.Add(actor);
        await context.SaveChangesAsync();

        return new TenantScenarioResult(tenant.Id, user.Id, actor.Id);
    }

    /// <summary>
    /// Seeds an active tenant user plus an approved organization administered by that user.
    /// </summary>
    public static async Task<TenantOrganizationScenarioResult> SeedActiveTenantWithOrganizationPublisherAsync(ExploreDbContext context)
    {
        TenantScenarioResult seeded = await SeedActiveTenantWithUserAsync(context);

        var organizationId = Guid.CreateVersion7();
        var organization = new Organization
        {
            Id = organizationId,
            TenantId = seeded.TenantId,
            Tenant = null!,
            ApprovalStatusId = (int)ApprovalStatusEnum.Approved,
            ApprovalStatus = null!,
            Pii = new OrganizationPii
            {
                OrganizationId = organizationId,
                FullName = "AI Test Publisher Organization"
            },
            CreatedAt = DateTime.UtcNow,
            ConcurrencyStamp = Guid.CreateVersion7()
        };
        context.Organizations.Add(organization);
        await context.SaveChangesAsync();

        var organizationActor = new ActorBuilder()
            .WithTenantId(seeded.TenantId)
            .WithActorType(ActorTypeEnum.Organization)
            .WithDisplayName("AI Test Publisher Organization")
            .Build();
        organizationActor.OrganizationId = organizationId;
        context.Actors.Add(organizationActor);
        await context.SaveChangesAsync();

        organization.ActorId = organizationActor.Id;
        context.OrganizationMembers.Add(new OrganizationMember
        {
            Id = Guid.CreateVersion7(),
            TenantId = seeded.TenantId,
            Tenant = null!,
            OrganizationId = organizationId,
            Organization = null!,
            UserId = seeded.UserId,
            User = null!,
            RoleId = (int)RoleEnum.OrgAdmin,
            Role = null!,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        return new TenantOrganizationScenarioResult(
            seeded.TenantId,
            seeded.UserId,
            seeded.ActorId,
            organizationId,
            organizationActor.Id);
    }

    /// <summary>
    /// Seeds a secondary tenant with its own user and actor.
    /// Uses a random tenant ID for multi-tenant isolation testing.
    /// </summary>
    public static async Task<TenantScenarioResult> SeedSecondaryTenantWithUserAsync(
        ExploreDbContext context,
        string name = "Secondary Tenant")
    {
        var tenant = new TenantBuilder()
            .WithFullName(name)
            .Build();
        context.Tenants.Add(tenant);

        var user = new UserBuilder().Build();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var actor = new ActorBuilder()
            .WithTenantId(tenant.Id)
            .WithUserId(user.Id)
            .WithDisplayName($"{name} User")
            .Build();
        context.Actors.Add(actor);
        await context.SaveChangesAsync();

        return new TenantScenarioResult(tenant.Id, user.Id, actor.Id);
    }
}
