// ABOUTME: Lookup service for retrieving language collections and individual language records.
// ABOUTME: Keeps Blazor components isolated behind the generated ISLAMU API client contract.

using System.Collections.Generic;
using System.Threading.Tasks;
using Explore.Blazor.Client.Clients;
using Explore.Blazor.Client.Contracts.Services.Lookup;

namespace Explore.Blazor.Client.Services.Lookup;

public class LanguageService : ILanguageService
{
    private readonly IIslamuApiClient _client;

    public LanguageService(IIslamuApiClient client)
    {
        _client = client;
    }

    public async Task<ICollection<LanguageListDto>> GetLanguagesAsync()
    {
        return await _client.GetLanguagesAsync();
    }

    public async Task<LanguageDto> GetLanguageByIdAsync(int id)
    {
        return await _client.GetLanguageByIdAsync(id);
    }
}
