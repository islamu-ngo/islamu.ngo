// ABOUTME: Integration tests for runtime database seeding against PostgreSQL.
// ABOUTME: Verifies development infrastructure settings are seeded from local configuration and environment.

using Event.Persistence.IntegrationTests.Fixtures;
using Explore.Domain;
using Explore.Domain.Constants;
using Explore.Domain.Enums;
using Explore.Domain.Secrets;
using Explore.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using TUnit.Assertions;
using TUnit.Core;

namespace Event.Persistence.IntegrationTests.Seed;

[ClassDataSource<PostgreSqlContainerFixture>(Shared = SharedType.PerAssembly)]
[NotInParallel("PersistenceDb")]
public class DatabaseSeederTests(PostgreSqlContainerFixture fixture)
{
    private static readonly IHostEnvironment DevelopmentEnvironment = new TestHostEnvironment();

    [Test]
    public async Task SeedAsync_InDevelopment_SeedsSmtpSettingsFromConfiguration()
    {
        await fixture.ResetAsync();

        var environmentKeys = new[]
        {
            "MAIL_SMTP_HOST",
            "MAIL_SMTP_PORT",
            "MAIL_SMTP_ENCRYPTION",
            "MAIL_SMTP_FROM_ADDRESS",
            "MAIL_SMTP_FROM_NAME",
            "SMTP_HOST",
            "SMTP_PORT",
            "SMTP_SECURITY",
            "SMTP_FROM_ADDRESS",
            "SMTP_FROM_NAME"
        };
        var originalEnvironment = environmentKeys.ToDictionary(key => key, Environment.GetEnvironmentVariable);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MAIL_SMTP_HOST"] = "mailpit",
                ["MAIL_SMTP_PORT"] = "1025",
                ["MAIL_SMTP_ENCRYPTION"] = "None",
                ["MAIL_SMTP_FROM_ADDRESS"] = "noreply@localhost",
                ["MAIL_SMTP_FROM_NAME"] = "ISLAMU Website Dev"
            })
            .Build();

        try
        {
            foreach (var key in environmentKeys)
            {
                Environment.SetEnvironmentVariable(key, null);
            }

            await using (var resetContext = fixture.CreateDbContext())
            {
                var smtpSettingKeys = new[]
                {
                    GovernanceSettingKeys.Email.SmtpHost,
                    GovernanceSettingKeys.Email.SmtpPort,
                    GovernanceSettingKeys.Email.SmtpSecurity,
                    GovernanceSettingKeys.Email.FromAddress,
                    GovernanceSettingKeys.Email.FromName
                };
                var smtpSettings = await resetContext.Set<SystemSetting>()
                    .Where(setting => smtpSettingKeys.Contains(setting.SettingKey))
                    .ToListAsync();

                foreach (var setting in smtpSettings)
                {
                    setting.Value = setting.SettingKey == GovernanceSettingKeys.Email.SmtpPort
                        ? "587"
                        : "\"\"";
                }

                await resetContext.SaveChangesAsync();
            }

            await using (var context = fixture.CreateDbContext())
            {
                await DatabaseSeeder.SeedAsync(context, DevelopmentEnvironment, configuration: configuration);
            }

            await using var verifyContext = fixture.CreateDbContext();
            var settings = await verifyContext.Set<SystemSetting>()
                .Where(setting => new[]
                {
                    GovernanceSettingKeys.Email.SmtpHost,
                    GovernanceSettingKeys.Email.SmtpPort,
                    GovernanceSettingKeys.Email.SmtpSecurity,
                    GovernanceSettingKeys.Email.FromAddress,
                    GovernanceSettingKeys.Email.FromName
                }.Contains(setting.SettingKey))
                .ToDictionaryAsync(setting => setting.SettingKey, setting => setting.Value);

            await Assert.That(settings[GovernanceSettingKeys.Email.SmtpHost]).IsEqualTo("\"mailpit\"");
            await Assert.That(settings[GovernanceSettingKeys.Email.SmtpPort]).IsEqualTo("1025");
            await Assert.That(settings[GovernanceSettingKeys.Email.SmtpSecurity]).IsEqualTo("\"None\"");
            await Assert.That(settings[GovernanceSettingKeys.Email.FromAddress]).IsEqualTo("\"noreply@localhost\"");
            await Assert.That(settings[GovernanceSettingKeys.Email.FromName]).IsEqualTo("\"ISLAMU Website Dev\"");
        }
        finally
        {
            foreach (var (key, value) in originalEnvironment)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    [Test]
    public async Task SeedAsync_InDevelopment_SeedsSvixSecretBindingsFromEnvironment()
    {
        await fixture.ResetAsync();

        var environmentKeys = new[]
        {
            "WEBHOOKS_SVIX_AUTH_TOKEN",
            "WEBHOOKS_SVIX_OPERATIONAL_WEBHOOK_SECRET"
        };
        var originalEnvironment = environmentKeys.ToDictionary(key => key, Environment.GetEnvironmentVariable);

        try
        {
            Environment.SetEnvironmentVariable("WEBHOOKS_SVIX_AUTH_TOKEN", "local-test-svix-token");
            Environment.SetEnvironmentVariable(
                "WEBHOOKS_SVIX_OPERATIONAL_WEBHOOK_SECRET",
                "whsec_local_test_operational_secret");

            await using (var context = fixture.CreateDbContext())
            {
                await DatabaseSeeder.SeedAsync(context, DevelopmentEnvironment);
            }

            await using var verifyContext = fixture.CreateDbContext();
            var bindings = await verifyContext.Set<SecretBinding>()
                .Where(binding => new[]
                {
                    SecretDefinitionRegistry.Keys.Webhooks.SvixAuthToken,
                    SecretDefinitionRegistry.Keys.Webhooks.SvixOperationalWebhookSecret
                }.Contains(binding.SettingKey))
                .ToDictionaryAsync(binding => binding.SettingKey);

            await Assert.That(bindings.Count).IsEqualTo(2);
            await Assert.That(bindings[SecretDefinitionRegistry.Keys.Webhooks.SvixAuthToken].SourceType)
                .IsEqualTo(SecretSourceType.EnvironmentVariable);
            await Assert.That(bindings[SecretDefinitionRegistry.Keys.Webhooks.SvixAuthToken].EnvironmentVariableName)
                .IsEqualTo("WEBHOOKS_SVIX_AUTH_TOKEN");
            await Assert.That(bindings[SecretDefinitionRegistry.Keys.Webhooks.SvixOperationalWebhookSecret].SourceType)
                .IsEqualTo(SecretSourceType.EnvironmentVariable);
            await Assert.That(bindings[SecretDefinitionRegistry.Keys.Webhooks.SvixOperationalWebhookSecret].EnvironmentVariableName)
                .IsEqualTo("WEBHOOKS_SVIX_OPERATIONAL_WEBHOOK_SECRET");
        }
        finally
        {
            foreach (var (key, value) in originalEnvironment)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Event.Persistence.IntegrationTests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
