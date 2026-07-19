// ABOUTME: Component tests for tenant render-policy preset normalization and server-only render-mode copy.
// ABOUTME: Proves tenant presets control prerender strategy without exposing legacy Auto or WebAssembly choices.

using Explore.Blazor.Client.Pages.Admin.Tenant.Components;
using MudBlazor;

namespace Explore.Blazor.Client.Tests.Pages.Admin;

public sealed class TenantRenderPolicySectionTests : IDisposable
{
    private readonly BlazorTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Test]
    public async Task TenantRenderPolicySection_LegacyAutoAndWebAssemblyInput_NormalizesToInteractiveServer()
    {
        TenantPolicySettingsDto model = CreateEditableModel();
        model.RenderPolicyPreset = "AllInteractiveAutoNoPrerender";
        model.PublicSeoRenderMode = "InteractiveAuto";
        model.OperationalRenderMode = "InteractiveWebAssembly";
        model.AdminRenderMode = "InteractiveAuto";

        _ctx.Render<TenantRenderPolicySection>(parameters => parameters
            .Add(component => component.Model, model));

        await Assert.That(model.RenderPolicyPreset).IsEqualTo("AllInteractiveServer");
        await Assert.That(model.PublicSeoRenderMode).IsEqualTo("InteractiveServer");
        await Assert.That(model.OperationalRenderMode).IsEqualTo("InteractiveServer");
        await Assert.That(model.AdminRenderMode).IsEqualTo("InteractiveServer");
    }

    [Test]
    public async Task TenantRenderPolicySection_RendersExactlyFourPrerenderPresets()
    {
        TenantPolicySettingsDto model = CreateEditableModel();

        IRenderedComponent<TenantRenderPolicySection> cut = _ctx.Render<TenantRenderPolicySection>(parameters => parameters
            .Add(component => component.Model, model));

        IRenderedComponent<MudSelect<string>> select = cut.FindComponent<MudSelect<string>>();
        cut.Find(".mud-select > .mud-input-control").MouseDown();
        await Assert.That(select.FindComponents<MudSelectItem<string>>().Count).IsEqualTo(4);
        await Assert.That(cut.Markup).DoesNotContain("AllInteractiveAutoNoPrerender", StringComparison.OrdinalIgnoreCase);
        await Assert.That(cut.Markup).Contains("Render modes are fixed to Interactive Server", StringComparison.OrdinalIgnoreCase);
    }

    [Test]
    public async Task TenantRenderPolicySection_ServerOnlyUi_DoesNotRenderAutoOrWebAssemblyChoices()
    {
        TenantPolicySettingsDto model = CreateEditableModel();

        IRenderedComponent<TenantRenderPolicySection> cut = _ctx.Render<TenantRenderPolicySection>(parameters => parameters
            .Add(component => component.Model, model));

        await Assert.That(cut.Markup).DoesNotContain("Interactive Auto", StringComparison.OrdinalIgnoreCase);
        await Assert.That(cut.Markup).DoesNotContain("Interactive WebAssembly", StringComparison.OrdinalIgnoreCase);
        await Assert.That(cut.Markup).DoesNotContain("WebAssembly", StringComparison.OrdinalIgnoreCase);
        await Assert.That(cut.Markup).Contains("Fixed render mode", StringComparison.OrdinalIgnoreCase);
    }

    private static TenantPolicySettingsDto CreateEditableModel() => new()
    {
        CanOverrideRenderPolicy = true,
        CanOverridePublicSeoRenderPolicy = true,
        CanOverrideOperationalRenderPolicy = true,
        CanOverrideAdminRenderPolicy = true,
        RenderPolicyPreset = "SeoBalanced"
    };
}
