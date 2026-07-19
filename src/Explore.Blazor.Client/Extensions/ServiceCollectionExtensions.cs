// ABOUTME: Shared service registrations used by the Blazor Server BFF host and client RCL.
// ABOUTME: Keeps component-library service wiring reusable without a browser-side bootstrap.

using Explore.Blazor.Client.Contracts.Interop;
using Explore.Blazor.Client.Contracts.Providers;
using Explore.Blazor.Client.Contracts.Services;
using Explore.Blazor.Client.Contracts.Services.Accessibility;
using Explore.Blazor.Client.Contracts.Services.Ai;
using Explore.Blazor.Client.Contracts.Services.Blogs;
using Explore.Blazor.Client.Contracts.Services.ControlPlane;
using Explore.Blazor.Client.Contracts.Services.CustomProperties;
using Explore.Blazor.Client.Contracts.Services.Footer;
using Explore.Blazor.Client.Contracts.Services.Lookup;
using Explore.Blazor.Client.Contracts.Services.Notifications;
using Explore.Blazor.Client.Contracts.Services.Organizations;
using Explore.Blazor.Client.Contracts.Services.SupportAccess;
using Explore.Blazor.Client.Contracts.Services.Webhooks;
using Explore.Blazor.Client.Services;
using Explore.Blazor.Client.Services.Accessibility;
using Explore.Blazor.Client.Services.Ai;
using Explore.Blazor.Client.Services.Blogs;
using Explore.Blazor.Client.Services.ControlPlane;
using Explore.Blazor.Client.Services.Docking;
using Explore.Blazor.Client.Services.Http;
using Explore.Blazor.Client.Services.Lookup;
using Explore.Blazor.Client.Services.Webhooks;
using Microsoft.Extensions.DependencyInjection;
using ExploreControlPlaneApiAdapter = Explore.Blazor.Client.Services.ControlPlane.ControlPlaneApiAdapter;

namespace Explore.Blazor.Client.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers application services shared by the Blazor Server host and client RCL.
    /// Services that need different implementations per host (e.g., IAnalyticsInterop)
    /// must be registered separately by each host.
    /// </summary>
    public static IServiceCollection AddSharedApplicationServices(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient>? configureBffRefitClient = null,
        Action<IHttpClientBuilder>? configureBffRefitClientBuilder = null)
    {
        // Domain services (NSwag IIslamuApiClient consumers)
        services.AddScoped<IApiClientExecutor, ApiClientExecutor>();
        services.AddScoped<ExploreControlPlaneApiAdapter>();
        services.AddScoped<IControlPlaneOverviewService>(provider => provider.GetRequiredService<ExploreControlPlaneApiAdapter>());
        services.AddScoped<IControlPlaneTenantService>(provider => provider.GetRequiredService<ExploreControlPlaneApiAdapter>());
        services.AddScoped<IControlPlaneDomainService>(provider => provider.GetRequiredService<ExploreControlPlaneApiAdapter>());
        services.AddScoped<IControlPlaneOperationsService>(provider => provider.GetRequiredService<ExploreControlPlaneApiAdapter>());
        services.AddScoped<IControlPlanePlanCatalogService>(provider => provider.GetRequiredService<ExploreControlPlaneApiAdapter>());
        services.AddScoped<IControlPlaneTenantConfigurationService>(provider => provider.GetRequiredService<ExploreControlPlaneApiAdapter>());
        services.AddScoped<IExternalApiKeyService, ExternalApiKeyService>();
        services.AddScoped<IWebhookManagementService, WebhookManagementService>();
        services.AddScoped<IListmonkIntegrationSettingsService, ListmonkIntegrationSettingsService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IOrganizationMemberService, OrganizationMemberService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ICustomPropertyAdminService, CustomPropertyAdminService>();
        services.AddScoped<ICustomPropertyDefinitionService, CustomPropertyDefinitionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISupportAccessClientService, SupportAccessClientService>();
        services.AddScoped<IAiAssistantClientService, AiAssistantClientService>();
        services.AddScoped<IOrganizationReviewService, OrganizationReviewService>();
        services.AddScoped<ITenantNavigationService, TenantNavigationService>();
        services.AddScoped<IFooterAdminService, FooterAdminService>();
        services.AddBffRefitClient<IMapsApi>(configureBffRefitClient)
            .ConfigureBffRefitClient(configureBffRefitClientBuilder);
        services.AddScoped<IMapsService, MapsService>();
        services.AddScoped<IImageContentClassifier, ImageContentClassifier>();
        services.AddScoped<IImageFileReaderService, ImageFileReaderService>();
        services.AddScoped<IImagePreviewService, ImagePreviewService>();
        services.AddScoped<IStorageObjectUrlResolver, StorageObjectUrlResolver>();
        services.AddScoped<IImageUploadClient, ImageUploadClient>();
        services.AddScoped<IImageStorageRecordClient, ImageStorageRecordClient>();
        services.AddScoped<IImageStorageService, ImageStorageService>();

        // Blog rendering
        services.AddScoped<IBlogMarkdownRenderer, BlogMarkdownRenderer>();
        services.AddScoped<IBlogService, BlogService>();

        // Lookup / reference data services
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IMadhabService, MadhabService>();
        services.AddScoped<ILookupCacheService, LookupCacheService>();

        services.AddScoped<IActorService, ActorService>();

        // Notification services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IActorSubscriptionService, ActorSubscriptionService>();
        services.AddScoped<INotificationRefreshStreamClient, NotificationRefreshStreamClient>();
        services.AddScoped<IWebPushBrowserInterop, WebPushBrowserInterop>();

        // BFF / onboarding services (use named HttpClient "BffClient")
        services.AddBffRefitClient<IBffAuthApi>(configureBffRefitClient)
            .ConfigureBffRefitClient(configureBffRefitClientBuilder);
        services.AddScoped<IInstanceOnboardingService, InstanceOnboardingService>();
        services.AddScoped<ITenantOnboardingService, TenantOnboardingService>();
        services.AddScoped<IPublicExperienceService, PublicExperienceService>();
        services.AddScoped<ITenantPublicExperienceAdminService, TenantPublicExperienceAdminService>();
        services.AddScoped<ITenantBrandingSettingsAdminService, TenantBrandingSettingsAdminService>();
        services.AddScoped<ITenantStorageSettingsAdminService, TenantStorageSettingsAdminService>();
        services.AddScoped<IAppearanceThemeService, AppearanceThemeService>();
        services.AddScoped<IUserAppearancePreferencesService, UserAppearancePreferencesService>();

        // Runtime render policy and routing
        services.AddScoped<IRuntimeRenderPolicyService, RuntimeRenderPolicyService>();
        services.AddScoped<IStartupRoutingService, StartupRoutingService>();

        // Auth state
        services.AddScoped<IAuthStateService, AuthStateService>();
        services.AddScoped<IUserExternalLoginService, UserExternalLoginService>();

        // Localization
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddBffRefitClient<ILanguagePreferenceApi>(configureBffRefitClient)
            .ConfigureBffRefitClient(configureBffRefitClientBuilder);
        services.AddScoped<ILanguagePreferenceService, LanguagePreferenceService>();
        services.AddTransient<MudBlazor.MudLocalizer, MudBlazorLocalizer>();

        // UI state
        services.AddScoped<SidebarState>();
        services.AddScoped<CurrentUserState>();
        services.AddScoped<MainContentAppearanceState>();
        services.AddScoped<AiAssistantState>();
        services.AddScoped<AiAssistantConversationState>();
        services.AddScoped<TenantNavLinksState>();
        services.AddScoped<DockLayoutState>();
        services.AddScoped<IDockPanelRegistry>(provider => provider.GetRequiredService<DockLayoutState>());
        services.AddScoped<IDockLayoutPersistence, Explore.Blazor.Client.Services.Interop.LocalStorageDockLayoutPersistence>();

        // User-scoped settings (auth-branching: API for authenticated, localStorage for anonymous)
        services.AddScoped<IUserSettingsService, UserSettingsService>();

        // Accessibility services (ARIA announcements + focus management)
        services.AddScoped<IAccessibilityAnnouncerService, AccessibilityAnnouncerService>();
        services.AddScoped<IAccessibilityFocusService, AccessibilityFocusService>();
        services.AddScoped<IBrowserActionInterop, BrowserActionInterop>();

        // Feature flags (hydrated from API, no OpenFeature SDK dependency)
        services.AddScoped<FeatureStateContainer>();
        services.AddScoped<IFeatureFlagClientService, FeatureFlagClientService>();

        return services;
    }


    private static IHttpClientBuilder ConfigureBffRefitClient(
        this IHttpClientBuilder builder,
        Action<IHttpClientBuilder>? configure)
    {
        configure?.Invoke(builder);
        return builder;
    }
}
