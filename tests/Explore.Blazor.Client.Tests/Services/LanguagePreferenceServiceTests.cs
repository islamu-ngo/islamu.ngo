// ABOUTME: Unit tests for browser-side language preference persistence across Blazor render modes.
// ABOUTME: Proves supported cultures use the BFF browser-fetch module and unsupported cultures are rejected locally.

using Explore.Blazor.Client.Models.Responses;
using Explore.Blazor.Client.Services;
using Explore.Blazor.Client.Services.Interop;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Explore.Blazor.Client.Tests.Services;

public sealed class LanguagePreferenceServiceTests
{
    [Test]
    public async Task SetLanguageAsync_SupportedCulture_UsesBrowserFetchModule()
    {
        using var context = new BlazorTestContext();
        context.Services.RemoveAll<ILanguagePreferenceService>();
        context.Services.AddScoped<LanguagePreferenceInterop>();
        context.Services.AddScoped<ILanguagePreferenceService, LanguagePreferenceService>();
        var module = context.JSInterop.SetupModule("/js/bff.js");
        module.Setup<BffMutationResult>("persistLanguagePreference", "ar")
            .SetResult(new BffMutationResult { Ok = true, Status = 200 });
        var service = context.Services.GetRequiredService<ILanguagePreferenceService>();

        var result = await service.SetLanguageAsync("ar");

        await Assert.That(result).IsTrue();
        await Assert.That(context.JSInterop.Invocations.Count(invocation => invocation.Identifier == "import")).IsEqualTo(1);
    }

    [Test]
    public async Task SetLanguageAsync_UnsupportedCulture_DoesNotCallBrowser()
    {
        using var context = new BlazorTestContext();
        context.Services.RemoveAll<ILanguagePreferenceService>();
        context.Services.AddScoped<LanguagePreferenceInterop>();
        context.Services.AddScoped<ILanguagePreferenceService, LanguagePreferenceService>();
        var service = context.Services.GetRequiredService<ILanguagePreferenceService>();

        var result = await service.SetLanguageAsync("zz");

        await Assert.That(result).IsFalse();
        await Assert.That(context.JSInterop.Invocations).IsEmpty();
    }
}
