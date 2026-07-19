// ABOUTME: Unit tests for runtime render policy service route-group classification and fallback behavior.
// ABOUTME: Verifies server-only mode selection, SeoBalanced defaults, and route-group prerender overrides.

namespace Explore.Blazor.Client.Tests.Services;

public class RuntimeRenderPolicyServiceTests
{
    private readonly IPublicExperienceService _publicExperienceService;
    private readonly ILogger<RuntimeRenderPolicyService> _logger;
    private readonly RuntimeRenderPolicyService _service;

    public RuntimeRenderPolicyServiceTests()
    {
        _publicExperienceService = Substitute.For<IPublicExperienceService>();
        _logger = Substitute.For<ILogger<RuntimeRenderPolicyService>>();
        _service = new RuntimeRenderPolicyService(_publicExperienceService, _logger);
    }

    [Test]
    public async Task ResolveForPathAsync_UsesOperationalOverride_ForOrganizationRoutes()
    {
        _publicExperienceService.GetCachedSettingsAsync().Returns(new PublicExperienceSettingsDto
        {
            EnableAdvancedRenderPolicyOverrides = true,
            GlobalRenderMode = "InteractiveServer",
            GlobalPrerenderEnabled = false,
            OperationalRenderMode = "InteractiveServer",
            OperationalPrerenderEnabled = true
        });

        var result = await _service.ResolveForPathAsync("/organizations");

        await Assert.That(result.RouteGroup).IsEqualTo(RuntimeRouteGroup.Operational);
        await Assert.That(result.RenderMode).IsEqualTo("InteractiveServer");
        await Assert.That(result.PrerenderEnabled).IsTrue();
    }

    [Test]
    public async Task ResolveForPathAsync_UsesGlobalFallback_WhenAdvancedOverridesDisabled()
    {
        _publicExperienceService.GetCachedSettingsAsync().Returns(new PublicExperienceSettingsDto
        {
            RenderPolicyPreset = "CustomAdvanced",
            EnableAdvancedRenderPolicyOverrides = false,
            GlobalRenderMode = "InteractiveServer",
            GlobalPrerenderEnabled = false,
            OperationalRenderMode = "InteractiveServer",
            OperationalPrerenderEnabled = true
        });

        var result = await _service.ResolveForPathAsync("/user/profile");

        await Assert.That(result.RouteGroup).IsEqualTo(RuntimeRouteGroup.Operational);
        await Assert.That(result.RenderMode).IsEqualTo("InteractiveServer");
        await Assert.That(result.PrerenderEnabled).IsFalse();
    }

    [Test]
    public async Task ResolveForPathAsync_AlwaysForcesInteractiveServerForOnboarding()
    {
        _publicExperienceService.GetCachedSettingsAsync().Returns(new PublicExperienceSettingsDto
        {
            EnableAdvancedRenderPolicyOverrides = true,
            OnboardingRenderMode = "InteractiveServer",
            OnboardingPrerenderEnabled = true
        });

        var result = await _service.ResolveForPathAsync("/onboarding/instance");

        await Assert.That(result.RouteGroup).IsEqualTo(RuntimeRouteGroup.Onboarding);
        await Assert.That(result.RenderMode).IsEqualTo("InteractiveServer");
        await Assert.That(result.PrerenderEnabled).IsTrue();
    }

    [Test]
    public async Task ResolveForPathAsync_SeoBalancedPreset_ForcesPublicSeoPrerenderOnFallback()
    {
        _publicExperienceService.GetCachedSettingsAsync().Returns(new PublicExperienceSettingsDto
        {
            RenderPolicyPreset = "SeoBalanced",
            EnableAdvancedRenderPolicyOverrides = false,
            GlobalRenderMode = "InteractiveServer",
            GlobalPrerenderEnabled = false
        });

        var result = await _service.ResolveForPathAsync("/");

        await Assert.That(result.RouteGroup).IsEqualTo(RuntimeRouteGroup.PublicSeo);
        await Assert.That(result.RenderMode).IsEqualTo("InteractiveServer");
        await Assert.That(result.PrerenderEnabled).IsTrue();
    }
}
