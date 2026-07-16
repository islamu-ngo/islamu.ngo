// ABOUTME: Builds the typed public-experience shell from tenant-local settings and referenced content.
// ABOUTME: Keeps OrganizationCentric behavior in Application read models without changing tenant resolution.

using System.Text.Json;
using Explore.Application.Contracts.Infrastructure;
using Explore.Application.Contracts.Persistence;
using Explore.Application.DTOs.Onboarding;
using Explore.Application.DTOs.PublicExperience;
using Explore.Application.DTOs.Tenant;
using Explore.Application.Features.PublicExperience.Requests.Queries;
using Explore.Application.Features.Tenants.Requests.Queries;
using Explore.Application.Models;
using Explore.Application.Models.PublicExperience;
using Explore.Application.Settings;
using Explore.Domain;
using Explore.Domain.Constants;
using Explore.Domain.Enums;
using MediatR;

namespace Explore.Application.Features.PublicExperience.Handlers.Queries;

public class GetPublicExperienceShellQueryHandler(
    IRequestHandler<GetPublicExperienceSettingsQuery, PublicExperienceSettingsDto> settingsHandler,
    IRequestHandler<GetTenantNavLinksQuery, List<TenantNavigationLinkDto>> navigationLinksHandler,
    ITenantContext tenantContext,
    IHierarchicalSettingsResolver hierarchicalSettingsResolver,
    IOrganizationRepository organizationRepository)
    : IRequestHandler<GetPublicExperienceShellQuery, PublicExperienceShellDto>
{
    public async Task<PublicExperienceShellDto> Handle(GetPublicExperienceShellQuery request, CancellationToken cancellationToken)
    {
        var settings = await settingsHandler.Handle(new GetPublicExperienceSettingsQuery(), cancellationToken);
        var navigationLinks = await navigationLinksHandler.Handle(new GetTenantNavLinksQuery(), cancellationToken);
        var tenantId = tenantContext.TenantId;
        var settingContext = new SettingContext(TenantId: tenantId);

        var mode = await hierarchicalSettingsResolver.ResolveAsync<string>(
            GovernanceSettingKeys.PublicExperience.Mode, settingContext, cancellationToken);
        var primaryOrganizationId = await hierarchicalSettingsResolver.ResolveAsync<string>(
            GovernanceSettingKeys.PublicExperience.PrimaryOrganizationId, settingContext, cancellationToken);
        var homeBlocks = await hierarchicalSettingsResolver.ResolveAsync<string>(
            GovernanceSettingKeys.PublicExperience.HomeBlocks, settingContext, cancellationToken);
        var ctas = await hierarchicalSettingsResolver.ResolveAsync<string>(
            GovernanceSettingKeys.PublicExperience.Ctas, settingContext, cancellationToken);

        var parsedMode = ParseMode(mode);
        var primaryOrganization = await ResolvePrimaryOrganizationAsync(primaryOrganizationId, tenantId, cancellationToken);
        var homeBlockDtos = BuildHomeBlocks(homeBlocks);
        var ctaDtos = BuildCtas(ctas);
        var shellNavigationLinks = BuildNavigationLinks(navigationLinks);
        ApplyOrganizationCentricDefaults(parsedMode, primaryOrganization, homeBlockDtos);

        return new PublicExperienceShellDto
        {
            SchemaVersion = 1,
            Revision = BuildRevision(settings, parsedMode, primaryOrganization, homeBlockDtos, ctaDtos, shellNavigationLinks),
            Mode = parsedMode,
            Home = new PublicExperienceHomeDto
            {
                PreferredHomePage = settings.PreferredHomePage,
                BrandDisplayName = settings.BrandDisplayName,
                BrandLogoUrl = settings.BrandLogoUrl,
                BrandFaviconUrl = settings.BrandFaviconUrl,
                Blocks = homeBlockDtos
            },
            Navigation = new PublicExperienceNavigationDto { Links = shellNavigationLinks },
            PrimaryOrganization = primaryOrganization,
            Ctas = ctaDtos,
            Footer = settings.FooterConfig
        };
    }

    private static PublicExperienceMode ParseMode(string? mode)
    {
        return Enum.TryParse(mode?.Trim().Trim('"'), ignoreCase: true, out PublicExperienceMode parsed)
            ? parsed
            : PublicExperienceMode.DiscoveryCentric;
    }

    private static void ApplyOrganizationCentricDefaults(
        PublicExperienceMode mode,
        PublicExperiencePrimaryOrganizationDto primaryOrganization,
        List<PublicExperienceHomeBlockDto> homeBlocks)
    {
        if (mode != PublicExperienceMode.OrganizationCentric ||
            primaryOrganization.State != PublicExperiencePrimaryOrganizationState.Available)
        {
            return;
        }

        if (homeBlocks.Count == 0)
        {
            homeBlocks.Add(new PublicExperienceHomeBlockDto
            {
                Key = "primary-organization-summary",
                Kind = PublicExperienceHomeBlockKind.OrganizationSummary,
                Title = primaryOrganization.DisplayName,
                Body = "Explore public programs and community updates from this organization.",
                ImageUrl = primaryOrganization.ProfilePictureUri,
                SortOrder = 0
            });
        }
    }

    private static T? DeserializeConfig<T>(string? rawConfig)
    {
        if (string.IsNullOrWhiteSpace(rawConfig))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(rawConfig, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static List<PublicExperienceNavigationLinkDto> BuildNavigationLinks(IReadOnlyList<TenantNavigationLinkDto> navigationLinks)
    {
        return navigationLinks
            .Where(link => !string.IsNullOrWhiteSpace(link.Label) && !string.IsNullOrWhiteSpace(link.Url))
            .OrderBy(link => link.Order)
            .ThenBy(link => link.Label, StringComparer.OrdinalIgnoreCase)
            .Select(link => new PublicExperienceNavigationLinkDto
            {
                Label = link.Label.Trim(),
                Url = link.Url.Trim(),
                SortOrder = link.Order
            })
            .ToList();
    }

    private static List<PublicExperienceHomeBlockDto> BuildHomeBlocks(string? rawConfig)
    {
        PublicExperienceHomeBlocksConfig? config = DeserializeConfig<PublicExperienceHomeBlocksConfig>(rawConfig);
        return config?.Blocks?
            .Where(block => block.IsEnabled && !string.IsNullOrWhiteSpace(block.Id) && !string.IsNullOrWhiteSpace(block.Title))
            .OrderBy(block => block.SortOrder)
            .ThenBy(block => block.Title, StringComparer.OrdinalIgnoreCase)
            .Select(block => new PublicExperienceHomeBlockDto
            {
                Key = block.Id.Trim(),
                Kind = block.Kind,
                Title = block.Title.Trim(),
                Subtitle = block.Subtitle?.Trim() ?? string.Empty,
                Body = block.Body?.Trim() ?? string.Empty,
                ImageUrl = block.ImageUrl?.Trim() ?? string.Empty,
                LinkText = block.LinkText?.Trim() ?? string.Empty,
                LinkUrl = block.LinkUrl?.Trim() ?? string.Empty,
                SortOrder = block.SortOrder
            })
            .ToList() ?? [];
    }

    private static List<PublicExperienceCtaDto> BuildCtas(string? rawConfig)
    {
        PublicExperienceCtasConfig? config = DeserializeConfig<PublicExperienceCtasConfig>(rawConfig);
        return config?.Ctas?
            .Where(cta => cta.IsEnabled && !string.IsNullOrWhiteSpace(cta.Id) && !string.IsNullOrWhiteSpace(cta.Label) && !string.IsNullOrWhiteSpace(cta.Url))
            .OrderBy(cta => cta.SortOrder)
            .ThenBy(cta => cta.Label, StringComparer.OrdinalIgnoreCase)
            .Select(cta => new PublicExperienceCtaDto
            {
                Key = cta.Id.Trim(),
                Label = cta.Label.Trim(),
                Url = cta.Url.Trim(),
                Placement = cta.Placement,
                Style = cta.Style,
                SortOrder = cta.SortOrder
            })
            .ToList() ?? [];
    }

    private async Task<PublicExperiencePrimaryOrganizationDto> ResolvePrimaryOrganizationAsync(
        string? rawOrganizationId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(rawOrganizationId?.Trim().Trim('"'), out var organizationId))
        {
            return new PublicExperiencePrimaryOrganizationDto
            {
                State = PublicExperiencePrimaryOrganizationState.NotConfigured
            };
        }

        Organization? organization = await organizationRepository.GetOrganizationWithDetails(organizationId, cancellationToken);
        if (organization is null)
        {
            return new PublicExperiencePrimaryOrganizationDto
            {
                State = PublicExperiencePrimaryOrganizationState.Missing,
                OrganizationId = organizationId
            };
        }

        if (organization.TenantId != tenantId)
        {
            return new PublicExperiencePrimaryOrganizationDto
            {
                State = PublicExperiencePrimaryOrganizationState.CrossTenantInvalid,
                OrganizationId = organizationId
            };
        }

        if (organization.IsDeleted)
        {
            return new PublicExperiencePrimaryOrganizationDto
            {
                State = PublicExperiencePrimaryOrganizationState.Deleted,
                OrganizationId = organizationId
            };
        }

        if (organization.ApprovalStatusId != (int)ApprovalStatusEnum.Approved)
        {
            return new PublicExperiencePrimaryOrganizationDto
            {
                State = PublicExperiencePrimaryOrganizationState.HiddenOrInactive,
                OrganizationId = organizationId
            };
        }

        if (organization.Actor is null || organization.Actor.IsDeleted)
        {
            return new PublicExperiencePrimaryOrganizationDto
            {
                State = PublicExperiencePrimaryOrganizationState.ActorUnavailable,
                OrganizationId = organizationId
            };
        }

        return new PublicExperiencePrimaryOrganizationDto
        {
            State = PublicExperiencePrimaryOrganizationState.Available,
            OrganizationId = organization.Id,
            ActorId = organization.ActorId,
            DisplayName = organization.Actor.DisplayName,
            Handle = organization.Actor.Handle ?? string.Empty,
            WebsiteUrl = organization.WebsiteUrl ?? string.Empty,
            ProfilePictureUri = organization.Actor.ProfilePictureUri ?? string.Empty
        };
    }

    private static string BuildRevision(
        PublicExperienceSettingsDto settings,
        PublicExperienceMode mode,
        PublicExperiencePrimaryOrganizationDto primaryOrganization,
        IReadOnlyList<PublicExperienceHomeBlockDto> homeBlocks,
        IReadOnlyList<PublicExperienceCtaDto> ctas,
        IReadOnlyList<PublicExperienceNavigationLinkDto> navigationLinks)
    {
        var primaryOrganizationToken = primaryOrganization.State == PublicExperiencePrimaryOrganizationState.Available
            ? string.Join('=',
                primaryOrganization.OrganizationId?.ToString("N"),
                primaryOrganization.ActorId?.ToString("N"),
                primaryOrganization.DisplayName,
                primaryOrganization.Handle,
                primaryOrganization.WebsiteUrl,
                primaryOrganization.ProfilePictureUri)
            : primaryOrganization.State.ToString();

        return string.Join(':',
            1,
            settings.TenantId.ToString("N"),
            mode,
            primaryOrganizationToken,
            string.Join(',', homeBlocks.Select(block => $"{block.Key}={block.Kind}:{block.Title}")),
            string.Join(',', ctas.Select(cta => $"{cta.Key}={cta.Placement}:{cta.Url}")),
            string.Join(',', navigationLinks.Select(link => $"{link.SortOrder}={link.Label}:{link.Url}")),
            settings.FooterConfig.Settings.Template,
            settings.FooterConfig.LinkGroups.Count);
    }
}
