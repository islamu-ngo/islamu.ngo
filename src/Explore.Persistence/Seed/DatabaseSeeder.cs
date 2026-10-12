// ABOUTME: Seeds retained platform lookup data and missing tenant branding documents.
// ABOUTME: Keeps system seeding tenant-safe without recreating removed product fixtures.

using Explore.Domain;
using Explore.Domain.Constants;
using Explore.Domain.Enums;
using Explore.Domain.Secrets;
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

            if (environment.IsDevelopment() && seedDevelopmentData)
            {
                await SeedDevelopmentSmtpAsync(context, configuration, cancellationToken);
                await SeedDevelopmentWebhookSecretsAsync(context, configuration, cancellationToken);
            }

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

    private static async Task SeedDevelopmentSmtpAsync(
        ExploreDbContext context,
        IConfiguration? configuration,
        CancellationToken cancellationToken)
    {
        var hostSetting = await context.Set<SystemSetting>()
            .FirstOrDefaultAsync(
                setting => setting.SettingKey == GovernanceSettingKeys.Email.SmtpHost,
                cancellationToken);

        if (hostSetting is null)
        {
            return;
        }

        var forceLocalAspireRefresh = IsAspireMode(configuration);
        var currentHost = hostSetting.Value?.Trim('"');
        if (!forceLocalAspireRefresh
            && !string.IsNullOrWhiteSpace(currentHost)
            && !currentHost.Equals("mailpit.openislamu.org", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var host = ReadEnvironmentValue(configuration, "localhost", "MAIL_SMTP_HOST", "SMTP_HOST", "Smtp:Host");
        var port = ReadEnvironmentValue(configuration, "1025", "MAIL_SMTP_PORT", "SMTP_PORT", "Smtp:Port");
        var security = NormalizeSmtpSecurity(ReadEnvironmentValue(
            configuration,
            "None",
            "MAIL_SMTP_ENCRYPTION",
            "SMTP_SECURITY",
            "Smtp:Encryption"));
        var fromAddress = ReadEnvironmentValue(
            configuration,
            "noreply@localhost",
            "MAIL_SMTP_FROM_ADDRESS",
            "SMTP_FROM_ADDRESS",
            "Smtp:FromAddress");
        var fromName = ReadEnvironmentValue(
            configuration,
            "ISLAMU Website Dev",
            "MAIL_SMTP_FROM_NAME",
            "SMTP_FROM_NAME",
            "Smtp:FromName");

        await UpdateSettingValueAsync(
            context,
            GovernanceSettingKeys.Email.SmtpHost,
            SerializeString(host),
            now,
            cancellationToken);
        await UpdateSettingValueAsync(
            context,
            GovernanceSettingKeys.Email.SmtpPort,
            port,
            now,
            cancellationToken);
        await UpdateSettingValueAsync(
            context,
            GovernanceSettingKeys.Email.SmtpSecurity,
            SerializeString(security),
            now,
            cancellationToken);
        await UpdateSettingValueAsync(
            context,
            GovernanceSettingKeys.Email.FromAddress,
            SerializeString(fromAddress),
            now,
            cancellationToken);
        await UpdateSettingValueAsync(
            context,
            GovernanceSettingKeys.Email.FromName,
            SerializeString(fromName),
            now,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static bool IsAspireMode(IConfiguration? configuration)
    {
        var value = Environment.GetEnvironmentVariable("ISLAMU_ASPIRE_MODE")
            ?? configuration?["ISLAMU_ASPIRE_MODE"];
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string ReadEnvironmentValue(
        IConfiguration? configuration,
        string defaultValue,
        params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        foreach (var key in keys)
        {
            var value = configuration?[key];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return defaultValue;
    }

    private static async Task SeedDevelopmentWebhookSecretsAsync(
        ExploreDbContext context,
        IConfiguration? configuration,
        CancellationToken cancellationToken)
    {
        await EnsureDevelopmentEnvironmentSecretBindingAsync(
            context,
            ResolveKnownSecretRef(
                configuration,
                SecretDefinitionRegistry.Keys.Webhooks.SvixAuthToken,
                "WEBHOOKS_SVIX_AUTH_TOKEN_SECRET_REF",
                "Webhooks:Svix:AuthTokenSecretRef"),
            "WEBHOOKS_SVIX_AUTH_TOKEN",
            configuration,
            cancellationToken);

        await EnsureDevelopmentEnvironmentSecretBindingAsync(
            context,
            ResolveKnownSecretRef(
                configuration,
                SecretDefinitionRegistry.Keys.Webhooks.SvixOperationalWebhookSecret,
                "WEBHOOKS_SVIX_OPERATIONAL_WEBHOOK_SECRET_REF",
                "Webhooks:Svix:OperationalWebhookSecretRef"),
            "WEBHOOKS_SVIX_OPERATIONAL_WEBHOOK_SECRET",
            configuration,
            cancellationToken);
    }

    private static string ResolveKnownSecretRef(
        IConfiguration? configuration,
        string defaultValue,
        params string[] keys)
    {
        var configured = ReadEnvironmentValue(configuration, defaultValue, keys).Trim();
        return SecretDefinitionRegistry.IsKnown(configured) ? configured : defaultValue;
    }

    private static async Task EnsureDevelopmentEnvironmentSecretBindingAsync(
        ExploreDbContext context,
        string settingKey,
        string environmentVariableName,
        IConfiguration? configuration,
        CancellationToken cancellationToken)
    {
        if (!HasConfiguredSecretValue(configuration, environmentVariableName))
        {
            return;
        }

        var existing = await context.Set<SecretBinding>()
            .FirstOrDefaultAsync(binding =>
                binding.SettingKey == settingKey
                && binding.SettingScopeId == (int)ConfigurationScopeEnum.Instance
                && binding.ScopeId == null,
                cancellationToken);

        if (existing is not null)
        {
            return;
        }

        context.Set<SecretBinding>().Add(SecretBinding.CreateEnvironmentVariable(
            settingKey,
            SecretScope.Instance,
            scopeId: null,
            environmentVariableName,
            isLocked: false));

        await context.SaveChangesAsync(cancellationToken);
    }

    private static bool HasConfiguredSecretValue(IConfiguration? configuration, string key) =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key))
        || !string.IsNullOrWhiteSpace(configuration?[key]);

    private static string NormalizeSmtpSecurity(string value) =>
        value.Trim().ToLowerInvariant() switch
        {
            "none" or "false" or "off" => "None",
            "starttls" or "start-tls" => "StartTls",
            "ssl" or "tls" or "ssl-on-connect" or "sslonconnect" => "SslOnConnect",
            "auto" => "Auto",
            _ => value
        };

    private static string SerializeString(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static async Task UpdateSettingValueAsync(
        ExploreDbContext context,
        string key,
        string value,
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        var setting = await context.Set<SystemSetting>()
            .FirstOrDefaultAsync(candidate => candidate.SettingKey == key, cancellationToken);

        if (setting is not null)
        {
            setting.Value = value;
            setting.UpdatedAt = timestamp;
        }
    }
}
