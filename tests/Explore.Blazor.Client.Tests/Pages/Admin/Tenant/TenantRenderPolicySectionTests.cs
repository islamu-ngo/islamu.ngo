// ABOUTME: Component tests for tenant render-policy preset normalization and server-only render-mode copy.
// ABOUTME: Proves tenant presets control prerender strategy without exposing legacy Auto or WebAssembly choices.

using Explore.Blazor.Client.Pages.Admin.Tenant.Components;

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

        string[] presetLabels =
        [
            "Interactive Server, no prerender",
            "SEO balanced prerender",
            "All routes prerendered",
            "Custom prerender strategy"
        ];

        await Assert.That(presetLabels.Count(label => cut.Markup.Contains(label, StringComparison.OrdinalIgnoreCase)))
            .IsEqualTo(4);
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
