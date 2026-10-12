// ABOUTME: Browser-side wrapper around POST /bff/language with CultureRegistry validation.
// ABOUTME: Uses browser fetch through JS interop so preference cookies reach the user's browser in every render mode.

using Explore.Blazor.Client.Contracts.Services;
using Explore.Blazor.Client.Localization;
using Explore.Blazor.Client.Services.Interop;
using Microsoft.Extensions.Logging;

namespace Explore.Blazor.Client.Services;

/// <summary>
/// Default <see cref="ILanguagePreferenceService"/> implementation.
/// </summary>
public sealed class LanguagePreferenceService : ILanguagePreferenceService
{
    private readonly LanguagePreferenceInterop _browserInterop;
    private readonly ILogger<LanguagePreferenceService> _logger;

    public LanguagePreferenceService(
        LanguagePreferenceInterop browserInterop,
        ILogger<LanguagePreferenceService> logger)
    {
        _browserInterop = browserInterop;
        _logger = logger;
    }

    public async Task<bool> SetLanguageAsync(string languageCode, CancellationToken ct = default)
    {
        if (!CultureRegistry.TryGetEntry(languageCode, out var entry))
        {
            _logger.LogWarning(
                "[LOCALIZATION] Rejected language preference '{Code}'; not in CultureRegistry",
                languageCode);
            return false;
        }

        try
        {
            var response = await _browserInterop.PersistAsync(entry.Code, ct);

            if (!response.Ok)
            {
                _logger.LogWarning(
                    "[LOCALIZATION] /bff/language returned {Status} for code {Code}",
                    response.Status, entry.Code);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LOCALIZATION] Failed to persist language preference for code {Code}", entry.Code);
            return false;
        }
    }
}
