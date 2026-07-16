// ABOUTME: Seeds retained platform lookup data and missing tenant branding documents.
// ABOUTME: Keeps system seeding tenant-safe without recreating removed product fixtures.

using Explore.Domain;
using Explore.Domain.Settings.Documents;
using Explore.Persistence.QueryFilters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Explore.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ExploreDbContext context,
        IHostEnvironment environment,
        bool seedDevelopmentData = true,
        IConfiguration? configuration = null,
        CancellationToken cancellationToken = default)
    {
        var shouldClearBypass = !context.IsTenantFilterBypassed;
        context.EnableTenantFilterBypass(TenantFilterBypassReasons.DatabaseSeeding);

        try
        {
            await LookupTableSeeder.SeedAsync(context, cancellationToken);
            await EnsureTenantBrandingDocumentsAsync(context, cancellationToken);
        }
        finally
        {
            if (shouldClearBypass)
            {
                context.ClearTenantFilterBypass();
            }
        }
    }

    private static async Task EnsureTenantBrandingDocumentsAsync(
        ExploreDbContext context,
        CancellationToken cancellationToken)
    {
        var tenants = await context.Set<Tenant>()
            .AsNoTracking()
            .Select(tenant => new { tenant.Id, tenant.FullName })
            .ToListAsync(cancellationToken);
        var tenantIds = tenants.Select(tenant => tenant.Id).ToList();
        var existing = (await context.Set<TenantSettingsDocument>()
                .Where(document => document.DocumentKey == SettingsDocumentKeys.Tenant.Branding
                    && tenantIds.Contains(document.TenantId))
                .Select(document => document.TenantId)
                .ToListAsync(cancellationToken))
            .ToHashSet();
        var missing = tenants
            .Where(tenant => !existing.Contains(tenant.Id))
            .Select(tenant => TenantBrandingSettingsDocumentDefaults.Create(tenant.Id, tenant.FullName))
            .ToList();

        if (missing.Count > 0)
        {
            context.Set<TenantSettingsDocument>().AddRange(missing);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
