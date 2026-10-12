// ABOUTME: Browser interop boundary for persisting language preferences through the BFF.
// ABOUTME: Executes browser fetch so response cookies reach the user agent in every Blazor render mode.

using Explore.Blazor.Client.Models.Responses;
using Microsoft.JSInterop;

namespace Explore.Blazor.Client.Services.Interop;

public sealed class LanguagePreferenceInterop(IJSRuntime jsRuntime)
{
    public async Task<BffMutationResult> PersistAsync(
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        await using var module = await jsRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            cancellationToken,
            "/js/bff.js");
        return await module.InvokeAsync<BffMutationResult>(
            "persistLanguagePreference",
            cancellationToken,
            languageCode);
    }
}
