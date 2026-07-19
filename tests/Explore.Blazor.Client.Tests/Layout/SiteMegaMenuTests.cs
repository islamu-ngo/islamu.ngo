// ABOUTME: bUnit/TUnit coverage for the standalone SiteMegaMenu desktop disclosure and mobile drawer.
// ABOUTME: Verifies static route inventory, accessibility attributes, dismissal paths, and viewport cleanup.

#nullable enable

using Explore.Blazor.Client.Contracts.Services.Accessibility;
using Explore.Blazor.Client.Layout;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor;
using MudBlazor.Services;

namespace Explore.Blazor.Client.Tests.Layout;

public sealed class SiteMegaMenuTests : IDisposable
{
    private readonly BlazorTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Test]
    public async Task SiteMegaMenu_RendersExactPublicRouteInventoryAndPrimaryNavigation()
    {
        SetupViewport(Breakpoint.Lg, 1280);

        var cut = RenderMenu();
        await OpenDesktopSectionAsync(cut, "discover");

        var hrefs = cut.FindAll("a[href]")
            .Select(link => link.GetAttribute("href"))
            .OfType<string>()
            .Where(href => !string.IsNullOrWhiteSpace(href))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        await Assert.That(hrefs.Length).IsEqualTo(5);
        foreach (var href in new[] { "/", "/about", "/blog", "/community-guidelines", "/contact" })
        {
            await Assert.That(hrefs).Contains(href);
        }
        await Assert.That(hrefs).DoesNotContain("/privacy");
        await Assert.That(hrefs).DoesNotContain("/terms");
        await Assert.That(cut.Markup).DoesNotContain("/events/");
        await Assert.That(cut.Markup).DoesNotContain("/admin");
        await Assert.That(cut.Markup).DoesNotContain("placeholder");
        await Assert.That(cut.Find("nav[aria-label='Primary']")).IsNotNull();
        await Assert.That(cut.FindAll("button[id^='site-mega-menu-trigger-']").Count).IsEqualTo(3);
    }

    [Test]
    public async Task SiteMegaMenu_UsesDisclosureAriaAndSemanticPanelStructure()
    {
        SetupViewport(Breakpoint.Lg, 1280);

        var cut = RenderMenu();
        await OpenDesktopSectionAsync(cut, "about");

        var trigger = cut.Find("[data-testid='site-mega-menu-trigger-about']");
        await Assert.That(trigger.GetAttribute("aria-expanded")).IsEqualTo("true");
        await Assert.That(trigger.GetAttribute("aria-controls")).IsEqualTo("site-mega-menu-panel");
        foreach (var sectionId in new[] { "discover", "about", "community" })
        {
            var sectionTrigger = cut.Find($"[data-testid='site-mega-menu-trigger-{sectionId}']");
            await Assert.That(sectionTrigger.GetAttribute("aria-controls")).IsEqualTo("site-mega-menu-panel");
        }

        await Assert.That(cut.Find("#site-mega-menu-panel").GetAttribute("aria-label")).IsEqualTo("Primary navigation links");
        await Assert.That(cut.FindAll(".site-mega-menu__section").Count).IsEqualTo(3);
        await Assert.That(cut.Find("#site-mega-menu-heading-about").TextContent).IsEqualTo("About ISLAMU");
    }

    [Test]
    public async Task SiteMegaMenu_PointerFocusOpenSwitchesAdjacentSectionsWithoutReopeningPanel()
    {
        var focusService = SetupFocusService();
        SetupViewport(Breakpoint.Lg, 1280);
        var openingCount = 0;

        var cut = RenderMenu(parameters => parameters.Add(x => x.OnOpening, () => openingCount++));
        await OpenDesktopSectionAsync(cut, "discover");
        await cut.Find("[data-testid='site-mega-menu-trigger-community']")
            .TriggerEventAsync("onfocusin", new FocusEventArgs());

        await Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-discover']").GetAttribute("aria-expanded")).IsEqualTo("false");
        await Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-community']").GetAttribute("aria-expanded")).IsEqualTo("true");
        await Assert.That(openingCount).IsEqualTo(1);
        await focusService.Received(1).SaveFocusAsync();
    }

    [Test]
    public async Task SiteMegaMenu_DelayedCloseCancelsOnReentryThenClosesAfterGrace()
    {
        SetupViewport(Breakpoint.Lg, 1280);

        var cut = RenderMenu();
        await OpenDesktopSectionAsync(cut, "discover");
        await cut.Find(".site-mega-menu__desktop").TriggerEventAsync("onpointerleave", new PointerEventArgs());
        await cut.Find("#site-mega-menu-panel").TriggerEventAsync("onpointerenter", new PointerEventArgs());
        await Task.Delay(190);

        await Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-discover']").GetAttribute("aria-expanded")).IsEqualTo("true");

        await cut.Find(".site-mega-menu__desktop").TriggerEventAsync("onpointerleave", new PointerEventArgs());
        cut.WaitForAssertion(() =>
            Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-discover']").GetAttribute("aria-expanded")).IsEqualTo("false").GetAwaiter().GetResult());
        await Assert.That(cut.Find("#site-mega-menu-panel").ClassList.Contains("site-mega-menu__panel--closing")).IsTrue();
        cut.WaitForAssertion(() =>
            Assert.That(cut.FindAll("#site-mega-menu-panel").Count).IsEqualTo(0).GetAwaiter().GetResult(),
            timeout: TimeSpan.FromMilliseconds(600));
    }

    [Test]
    public async Task SiteMegaMenu_ReopenDuringExitCancelsPanelRemoval()
    {
        SetupViewport(Breakpoint.Lg, 1280);

        var cut = RenderMenu();
        await OpenDesktopSectionAsync(cut, "discover");
        cut.Find("[data-testid='site-mega-menu-desktop-close']").Click();
        await Assert.That(cut.Find("#site-mega-menu-panel").ClassList.Contains("site-mega-menu__panel--closing")).IsTrue();

        await OpenDesktopSectionAsync(cut, "about");
        await Task.Delay(220);

        await Assert.That(cut.FindAll("#site-mega-menu-panel").Count).IsEqualTo(1);
        await Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-about']").GetAttribute("aria-expanded")).IsEqualTo("true");
        await Assert.That(cut.Find("#site-mega-menu-panel").ClassList.Contains("site-mega-menu__panel--open")).IsTrue();
    }

    [Test]
    public async Task SiteMegaMenu_EscapeAfterAdjacentSwitchRestoresCurrentTriggerFocus()
    {
        var focusService = SetupFocusService();
        SetupViewport(Breakpoint.Lg, 1280);
        var openingCount = 0;

        var cut = RenderMenu(parameters => parameters.Add(x => x.OnOpening, () => openingCount++));
        await OpenDesktopSectionAsync(cut, "community");
        await cut.Find("[data-testid='site-mega-menu-trigger-about']")
            .TriggerEventAsync("onfocusin", new FocusEventArgs());
        await cut.Find("[data-testid='site-mega-menu']").TriggerEventAsync("onkeydown", new KeyboardEventArgs { Key = "Escape" });

        await Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-community']").GetAttribute("aria-expanded")).IsEqualTo("false");
        await Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-about']").GetAttribute("aria-expanded")).IsEqualTo("false");
        await Assert.That(openingCount).IsEqualTo(1);
        await focusService.Received(1).SaveFocusAsync();
        await focusService.Received(1).FocusAsync("#site-mega-menu-trigger-about");
        await focusService.DidNotReceive().RestoreFocusAsync("#site-mega-menu-trigger-community");
    }

    [Test]
    public async Task SiteMegaMenu_BackdropExplicitCloseRouteAndLinkClickDismissDesktopPanel()
    {
        SetupViewport(Breakpoint.Lg, 1280);

        var cut = RenderMenu();
        await OpenDesktopSectionAsync(cut, "discover");
        cut.Find("[data-testid='site-mega-menu-backdrop']").Click();
        await Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-discover']").GetAttribute("aria-expanded")).IsEqualTo("false");

        await OpenDesktopSectionAsync(cut, "discover");
        cut.Find("[data-testid='site-mega-menu-desktop-close']").Click();
        await Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-discover']").GetAttribute("aria-expanded")).IsEqualTo("false");

        await OpenDesktopSectionAsync(cut, "discover");
        cut.Find("[data-testid='site-mega-menu-link-blog']").Click();
        await Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-discover']").GetAttribute("aria-expanded")).IsEqualTo("false");

        await OpenDesktopSectionAsync(cut, "discover");
        _ctx.Services.GetRequiredService<NavigationManager>().NavigateTo("/contact");
        cut.WaitForAssertion(() =>
            Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-discover']").GetAttribute("aria-expanded")).IsEqualTo("false").GetAwaiter().GetResult());
    }

    [Test]
    public async Task SiteMegaMenu_MobileDrawerTogglesAccordionsLinksOverlayAndEscape()
    {
        var focusService = SetupFocusService();
        SetupViewport(Breakpoint.Sm, 390);

        var cut = RenderMenu();
        cut.Find("[data-testid='site-mega-menu-mobile-toggle']").Click();

        await Assert.That(cut.Find("[data-testid='site-mega-menu-mobile-toggle']").GetAttribute("aria-expanded")).IsEqualTo("true");
        await Assert.That(cut.FindAll("[data-testid^='site-mega-menu-mobile-section-']").Count).IsEqualTo(3);
        await Assert.That(cut.Find("[data-testid='site-mega-menu-mobile-link-community-guidelines']").GetAttribute("href")).IsEqualTo("/community-guidelines");
        await Assert.That(cut.Markup).DoesNotContain("/privacy");
        await Assert.That(cut.Markup).DoesNotContain("/terms");

        cut.Find("[data-testid='site-mega-menu-mobile-link-community-guidelines']").Click();
        await Assert.That(cut.Find("[data-testid='site-mega-menu-mobile-toggle']").GetAttribute("aria-expanded")).IsEqualTo("false");

        cut.Find("[data-testid='site-mega-menu-mobile-toggle']").Click();
        cut.Find("[data-testid='site-mega-menu-mobile-close']").Click();
        await focusService.Received(1).RestoreFocusAsync("#site-mega-menu-mobile-toggle");

        cut.Find("[data-testid='site-mega-menu-mobile-toggle']").Click();
        await cut.Find("[data-testid='site-mega-menu']").TriggerEventAsync("onkeydown", new KeyboardEventArgs { Key = "Escape" });
        await Assert.That(cut.Find("[data-testid='site-mega-menu-mobile-toggle']").GetAttribute("aria-expanded")).IsEqualTo("false");
    }

    [Test]
    public async Task SiteMegaMenu_ViewportTransitionClosesOpenSurfacesAndDisposalUnsubscribes()
    {
        IBrowserViewportObserver? observer = null;
        var viewportService = SetupViewport(Breakpoint.Lg, 1280, captured => observer = captured);

        var cut = RenderMenu();
        await OpenDesktopSectionAsync(cut, "discover");
        await observer!.NotifyBrowserViewportChangeAsync(new BrowserViewportEventArgs(
            Guid.NewGuid(),
            new BrowserWindowSize { Width = 390, Height = 844 },
            Breakpoint.Sm,
            isImmediate: false));

        await Assert.That(cut.Find("[data-testid='site-mega-menu-trigger-discover']").GetAttribute("aria-expanded")).IsEqualTo("false");

        cut.Dispose();
        await viewportService.Received(1).UnsubscribeAsync(Arg.Any<IBrowserViewportObserver>());
    }

    [Test]
    public async Task SiteMegaMenu_DisposalCancelsPendingDelayedCloseAndExit()
    {
        var viewportService = SetupViewport(Breakpoint.Lg, 1280);

        var delayedCloseCut = RenderMenu();
        await OpenDesktopSectionAsync(delayedCloseCut, "discover");
        await delayedCloseCut.Find(".site-mega-menu__desktop").TriggerEventAsync("onpointerleave", new PointerEventArgs());
        delayedCloseCut.Dispose();

        var exitCut = RenderMenu();
        await OpenDesktopSectionAsync(exitCut, "about");
        exitCut.Find("[data-testid='site-mega-menu-desktop-close']").Click();
        exitCut.Dispose();

        await Task.Delay(240);

        await viewportService.Received(2).UnsubscribeAsync(Arg.Any<IBrowserViewportObserver>());
    }

    private IRenderedComponent<SiteMegaMenu> RenderMenu(Action<ComponentParameterCollectionBuilder<SiteMegaMenu>>? parameters = null)
        => parameters is null ? _ctx.RenderMudComponent<SiteMegaMenu>() : _ctx.RenderMudComponent(parameters);

    private static async Task OpenDesktopSectionAsync(IRenderedComponent<SiteMegaMenu> cut, string sectionId)
    {
        await cut.Find($"[data-testid='site-mega-menu-trigger-{sectionId}']")
            .TriggerEventAsync("onpointerenter", new PointerEventArgs());
    }

    private IAccessibilityFocusService SetupFocusService()
    {
        var focusService = Substitute.For<IAccessibilityFocusService>();
        focusService.FocusAsync(Arg.Any<string>(), Arg.Any<bool>()).Returns(Task.CompletedTask);
        focusService.SaveFocusAsync().Returns(Task.CompletedTask);
        focusService.RestoreFocusAsync(Arg.Any<string?>()).Returns(Task.CompletedTask);
        _ctx.Services.AddScoped(_ => focusService);
        return focusService;
    }

    private IBrowserViewportService SetupViewport(
        Breakpoint breakpoint,
        int width,
        Action<IBrowserViewportObserver>? capture = null)
    {
        var viewportService = Substitute.For<IBrowserViewportService>();
        viewportService.SubscribeAsync(Arg.Any<IBrowserViewportObserver>(), Arg.Any<bool>())
            .Returns(callInfo =>
            {
                var observer = callInfo.Arg<IBrowserViewportObserver>();
                if (observer is null)
                {
                    throw new InvalidOperationException("Viewport observer is required.");
                }

                capture?.Invoke(observer);
                return observer.NotifyBrowserViewportChangeAsync(new BrowserViewportEventArgs(
                    Guid.NewGuid(),
                    new BrowserWindowSize { Width = width, Height = 844 },
                    breakpoint,
                    isImmediate: true));
            });
        viewportService.UnsubscribeAsync(Arg.Any<IBrowserViewportObserver>()).Returns(Task.CompletedTask);

        _ctx.Services.RemoveAll<IBrowserViewportService>();
        _ctx.Services.AddSingleton(viewportService);
        return viewportService;
    }
}
