// ABOUTME: Interface for user appearance preferences operations wrapping IIslamuApiClient.
// ABOUTME: Decouples Blazor components from direct IIslamuApiClient injection.

using Explore.Blazor.Client.Clients;

namespace Explore.Blazor.Client.Contracts.Services;

public interface IUserAppearancePreferencesService
{
    Task<ResolvedAppearanceDto> GetCurrentPreferencesAsync(CancellationToken ct = default);
    Task SetActiveProfileAsync(SetActiveProfileRequestDto dto, CancellationToken ct = default);
}
