// ABOUTME: Component tests for NavMenu integration with the static SiteMegaMenu primary navigation.
// ABOUTME: Verifies tenant top links are ignored and shell overlays dismiss each other deterministically.

using Explore.Blazor.Client.Components.Shell;
using Explore.Blazor.Client.Services;
using Explore.Blazor.Client.Services.Docking;
using Explore.Blazor.Client.Tests.Common.Authentication;

namespace Explore.Blazor.Client.Tests.Layout;

public sealed class NavMenuMegaMenuTests : IDisposable
{
    private readonly BlazorTestContext _ctx;

    public NavMenuMegaMenuTests()
    {
        _ctx = new BlazorTestContext();
        _ctx.AddShellStateMocks();
    }

    public void Dispose() => _ctx.Dispose();

    [Test]
    public async Task NavMenu_RendersStaticSiteMegaMenuAndIgnoresTenantTopLinks()
    {
        _ctx.SetAnonymousUser();
        SetupNavMenuServices();
        var tenantNavigationService = _ctx.Services.GetRequiredService<ITenantNavigationService>();
        tenantNavigationService.GetNavigationLinksAsync().Returns(new List<TenantNavigationLinkDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Tenant CMS",
                Url = "/tenant-cms",
                Order = 1
            }
        });

        var cut = RenderNavMenu();
        await OpenMegaMenuSectionAsync(cut, "discover");

        await Assert.That(cut.FindAll("[data-testid='site-mega-menu']").Count).IsEqualTo(1);
        await Assert.That(cut.Find("[data-testid='site-mega-menu-link-blog']").GetAttribute("href")).IsEqualTo("/blog");
        await Assert.That(cut.Markup).Contains("href=\"/about\"");
        await Assert.That(cut.Markup).Contains("href=\"/contact\"");
        await Assert.That(cut.Markup).Contains("href=\"/community-guidelines\"");
        await Assert.That(cut.Markup).DoesNotContain("href=\"/privacy\"");
        await Assert.That(cut.Markup).DoesNotContain("href=\"/terms\"");
        await Assert.That(cut.Markup).DoesNotContain("Tenant CMS");
        await Assert.That(cut.Markup).DoesNotContain("/tenant-cms");
    }

    [Test]
    public async Task NavMenu_SiteMegaMenuOpeningClosesProfileDropdownAndAiDockPanel()
    {
        _ctx.SetAuthenticatedUser(AuthenticationTestConstants.DefaultUserId, "Regular User");
        var settings = new PublicExperienceSettingsBuilder()
            .WithAiAssistant()
            .Build();
        SetupNavMenuServices(publicExperienceSettings: settings);
        var dockLayoutState = RegisterAiDockPanel();

        var cut = RenderNavMenu();
        cut.Find("[data-testid='shell-ai-toggle']").Click();
        cut.Find(".navbar__user-btn").Click();

        await Assert.That(cut.FindAll(".navbar__dropdown-menu--open").Count).IsEqualTo(1);
        await Assert.That(_ctx.Services.GetRequiredService<AiAssistantState>().IsOpen).IsTrue();
        await Assert.That(dockLayoutState.GetPanel(ShellDockPanels.AiAssistantId)?.State.IsOpen).IsTrue();

        await OpenMegaMenuSectionAsync(cut, "about");

        await Assert.That(cut.FindAll(".navbar__dropdown-menu--open").Count).IsEqualTo(0);
        await Assert.That(_ctx.Services.GetRequiredService<AiAssistantState>().IsOpen).IsFalse();
        await Assert.That(dockLayoutState.GetPanel(ShellDockPanels.AiAssistantId)?.State.IsOpen).IsFalse();
    }

    [Test]
    public async Task NavMenu_UserDropdownAndAiToggleDismissOpenSiteMegaMenu()
    {
        _ctx.SetAuthenticatedUser(AuthenticationTestConstants.DefaultUserId, "Regular User");
        var settings = new PublicExperienceSettingsBuilder()
            .WithAiAssistant()
            .Build();
        SetupNavMenuServices(publicExperienceSettings: settings);
        RegisterAiDockPanel();

        var cut = RenderNavMenu();
        await OpenMegaMenuSectionAsync(cut, "discover");
        cut.Find(".navbar__user-btn").Click();

        await Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-discover']").GetAttribute("aria-expanded")).IsEqualTo("false");
        await Assert.That(cut.FindAll(".navbar__dropdown-menu--open").Count).IsEqualTo(1);

        await OpenMegaMenuSectionAsync(cut, "community");
        cut.Find("[data-testid='shell-ai-toggle']").Click();

        await Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-community']").GetAttribute("aria-expanded")).IsEqualTo("false");
        await Assert.That(_ctx.Services.GetRequiredService<AiAssistantState>().IsOpen).IsTrue();
        await Assert.That(cut.FindAll(".navbar__dropdown-menu--open").Count).IsEqualTo(0);
    }

    private IRenderedComponent<DynamicComponent> RenderNavMenu()
    {
        var componentType = typeof(AiAssistantState).Assembly.GetType("Explore.Blazor.Client.Layout.NavMenu")
                            ?? throw new InvalidOperationException("NavMenu component type not found");

        return _ctx.RenderMudComponent<DynamicComponent>(p => p.Add(x => x.Type, componentType));
    }

    private static async Task OpenMegaMenuSectionAsync(IRenderedComponent<DynamicComponent> cut, string sectionId)
    {
        await cut.Find($"[data-testid='site-mega-menu-trigger-{sectionId}']")
            .TriggerEventAsync("onpointerenter", new PointerEventArgs());
    }

    private DockLayoutState RegisterAiDockPanel()
    {
        var dockLayoutState = _ctx.Services.GetRequiredService<DockLayoutState>();
        dockLayoutState.Register(ShellDockPanels.AiAssistant, builder => builder.AddContent(0, "AI Assistant"));
        return dockLayoutState;
    }

    private void SetupNavMenuServices(PublicExperienceSettingsDto? publicExperienceSettings = null)
    {
        NavMenuTestServices.Register(_ctx, publicExperienceSettings: publicExperienceSettings);
    }
}
